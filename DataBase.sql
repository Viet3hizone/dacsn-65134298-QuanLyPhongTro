-- =============================================
-- CSDL: Quản lý phòng trọ (SQL Server 2022 / SSMS)
-- Nếu đã chạy bản cũ: DROP DATABASE QuanLyPhongTro; rồi chạy lại file này
-- =============================================
IF DB_ID(N'QuanLyPhongTro') IS NULL CREATE DATABASE QuanLyPhongTro;
GO
USE QuanLyPhongTro;
GO

-- 1. Tài khoản & vai trò
CREATE TABLE TaiKhoan (
    Id           INT IDENTITY PRIMARY KEY,
    TenDangNhap  VARCHAR(50)  NOT NULL UNIQUE,
    MatKhauHash  VARCHAR(256) NOT NULL,          -- băm SHA256 + salt
    Salt         VARCHAR(64)  NOT NULL,
    Role         NVARCHAR(10) NOT NULL CHECK (Role IN (N'ChuTro', N'Khach')),
    NgayTao      DATETIME     NOT NULL DEFAULT GETDATE()
);

CREATE TABLE ChuTro (
    Id             INT IDENTITY PRIMARY KEY,
    TaiKhoanId     INT NOT NULL UNIQUE REFERENCES TaiKhoan(Id),
    HoTen          NVARCHAR(100) NOT NULL,
    SDT            VARCHAR(15)   NOT NULL,
    MaChuTro       VARCHAR(10)   NOT NULL UNIQUE,   -- ví dụ CT0001
    AnhQR          VARCHAR(260)  NULL,              -- ảnh QR nhận tiền
    TenNganHang    NVARCHAR(50)  NULL,
    SoTaiKhoan     VARCHAR(30)   NULL,
    TenChuTaiKhoan NVARCHAR(100) NULL
);

CREATE TABLE Khach (
    Id           INT IDENTITY PRIMARY KEY,
    TaiKhoanId   INT NOT NULL UNIQUE REFERENCES TaiKhoan(Id),
    HoTen        NVARCHAR(100) NOT NULL,
    SDT          VARCHAR(15)   NOT NULL,
    TinhMuonO    NVARCHAR(50)  NULL,
    HuyenMuonO   NVARCHAR(50)  NULL,
    SoCCCD       VARCHAR(12)   NOT NULL UNIQUE,
    AnhCCCDTruoc VARCHAR(260)  NULL,                -- đường dẫn tương đối
    AnhCCCDSau   VARCHAR(260)  NULL
);

-- 2. Nhà trọ & phòng
CREATE TABLE NhaTro (
    Id          INT IDENTITY PRIMARY KEY,
    ChuTroId    INT NOT NULL REFERENCES ChuTro(Id),
    TenNhaTro   NVARCHAR(100) NULL,
    Tinh        NVARCHAR(50)  NOT NULL,
    Huyen       NVARCHAR(50)  NOT NULL,             -- huyện / thành phố
    DiaChiCuThe NVARCHAR(200) NOT NULL,
    MoTa        NVARCHAR(500) NULL
);
CREATE INDEX IX_NhaTro_KhuVuc ON NhaTro(Tinh, Huyen);   -- tìm trọ theo khu vực

CREATE TABLE AnhNhaTro (
    Id          INT IDENTITY PRIMARY KEY,
    NhaTroId    INT NOT NULL REFERENCES NhaTro(Id) ON DELETE CASCADE,
    DuongDanAnh VARCHAR(260) NOT NULL
);

CREATE TABLE Phong (
    Id        INT IDENTITY PRIMARY KEY,
    NhaTroId  INT NOT NULL REFERENCES NhaTro(Id),
    ChuTroId  INT NOT NULL REFERENCES ChuTro(Id),   -- để MaPhong duy nhất trong phạm vi 1 chủ
    MaPhong   VARCHAR(10) NOT NULL,                 -- ví dụ P101
    ViTri     NVARCHAR(50) NULL,                    -- tầng / dãy
    DienTich  DECIMAL(6,2) NULL,
    GiaThue   DECIMAL(12,0) NOT NULL CHECK (GiaThue >= 0),
    TrangThai NVARCHAR(10) NOT NULL DEFAULT N'Trong'
              CHECK (TrangThai IN (N'Trong', N'DangThue', N'BaoTri')),
    MoTa      NVARCHAR(500) NULL,
    CONSTRAINT UQ_Phong_Chu_Ma UNIQUE (ChuTroId, MaPhong)
);

CREATE TABLE AnhPhong (
    Id          INT IDENTITY PRIMARY KEY,
    PhongId     INT NOT NULL REFERENCES Phong(Id) ON DELETE CASCADE,
    DuongDanAnh VARCHAR(260) NOT NULL
);

-- 3. Hợp đồng, người ở, xe
CREATE TABLE HopDong (
    Id           INT IDENTITY PRIMARY KEY,
    PhongId      INT NOT NULL REFERENCES Phong(Id),
    KhachId      INT NOT NULL REFERENCES Khach(Id),
    SoThang      INT NOT NULL CHECK (SoThang >= 6),    -- 6, 12, 18...
    NgayBatDau   DATE NULL,
    NgayKetThuc  DATE NULL,
    TienCoc      DECIMAL(12,0) NOT NULL DEFAULT 0,
    SoNguoi      INT NOT NULL CHECK (SoNguoi >= 1),
    ChiSoDienDau INT NULL CHECK (ChiSoDienDau >= 0),
    ChiSoNuocDau INT NULL CHECK (ChiSoNuocDau >= 0),
    TrangThai    NVARCHAR(10) NOT NULL DEFAULT N'ChoDuyet'
                 CHECK (TrangThai IN (N'ChoDuyet', N'HieuLuc', N'TuChoi', N'KetThuc')),
    NgayTao      DATETIME NOT NULL DEFAULT GETDATE()
);
-- Mỗi phòng chỉ có 1 hợp đồng hiệu lực
CREATE UNIQUE INDEX UX_HopDong_PhongHieuLuc
    ON HopDong(PhongId) WHERE TrangThai = N'HieuLuc';

CREATE TABLE NguoiO (
    Id               INT IDENTITY PRIMARY KEY,
    HopDongId        INT NOT NULL REFERENCES HopDong(Id) ON DELETE CASCADE,
    HoTen            NVARCHAR(100) NOT NULL,
    NgaySinh         DATE NULL,
    SoCCCD           VARCHAR(12) NOT NULL,
    SDT              VARCHAR(15) NULL,
    AnhCCCDTruoc     VARCHAR(260) NULL,
    AnhCCCDSau       VARCHAR(260) NULL,
    KhachId          INT NULL REFERENCES Khach(Id),     -- tài khoản đã liên kết (nếu có)
    TrangThaiLienKet NVARCHAR(12) NOT NULL DEFAULT N'ChuaLienKet'
                     CHECK (TrangThaiLienKet IN (N'ChuaLienKet', N'ChoXacNhan', N'DaLienKet')),
    CONSTRAINT UQ_NguoiO_HopDong_CCCD UNIQUE (HopDongId, SoCCCD)
);
CREATE INDEX IX_NguoiO_CCCD ON NguoiO(SoCCCD);          -- tra cứu khi khách đăng ký bằng CCCD

CREATE TABLE PhuongTien (
    Id        INT IDENTITY PRIMARY KEY,
    NguoiOId  INT NOT NULL REFERENCES NguoiO(Id) ON DELETE CASCADE,
    BienSoXe  VARCHAR(15) NOT NULL,
    LoaiXe    NVARCHAR(30) NULL
);

-- 4. Dịch vụ, điện nước, hóa đơn, thanh toán
-- Mỗi dòng là một khoản thu. Điện chia thành nhiều dòng, mỗi dòng là một bậc.
CREATE TABLE DichVu (
    Id           INT IDENTITY PRIMARY KEY,
    ChuTroId     INT NOT NULL REFERENCES ChuTro(Id),
    TenDichVu    NVARCHAR(50) NOT NULL,                 -- Điện bậc 1..5, Nước, Rác, Internet, Giữ xe
    LoaiDichVu   NVARCHAR(10) NOT NULL
                 CHECK (LoaiDichVu IN (N'Dien', N'Nuoc', N'Khac')),
    DonGia       DECIMAL(12,0) NOT NULL CHECK (DonGia >= 0),   -- đồng / đơn vị
    DonVi        NVARCHAR(20) NULL,                     -- kWh, m3, tháng, xe
    TuMuc        INT NULL,                              -- Điện: kWh bắt đầu của bậc (bậc 2 = 101...)
    DenMuc       INT NULL,                              -- Điện: kWh cuối của bậc (NULL = không giới hạn)
                                                        -- Nước: định mức m3 / người / tháng
    PhanTramPhat DECIMAL(5,2) NOT NULL DEFAULT 0
                 CHECK (PhanTramPhat >= 0),             -- Nước: % phụ thu khi dùng vượt định mức
    CONSTRAINT UQ_DichVu_Ten UNIQUE (ChuTroId, TenDichVu),
    CONSTRAINT CK_DichVu_Muc CHECK (TuMuc IS NULL OR DenMuc IS NULL OR DenMuc >= TuMuc)
);

CREATE TABLE ChiSoDienNuoc (
    Id       INT IDENTITY PRIMARY KEY,
    PhongId  INT NOT NULL REFERENCES Phong(Id),
    Thang    INT NOT NULL CHECK (Thang BETWEEN 1 AND 12),
    Nam      INT NOT NULL,
    DienCu   INT NOT NULL CHECK (DienCu >= 0),
    DienMoi  INT NOT NULL,
    NuocCu   INT NOT NULL CHECK (NuocCu >= 0),
    NuocMoi  INT NOT NULL,
    CONSTRAINT CK_Dien CHECK (DienMoi >= DienCu),
    CONSTRAINT CK_Nuoc CHECK (NuocMoi >= NuocCu),
    CONSTRAINT UQ_ChiSo_Phong_Thang UNIQUE (PhongId, Nam, Thang)
);

CREATE TABLE HoaDon (
    Id             INT IDENTITY PRIMARY KEY,
    HopDongId      INT NOT NULL REFERENCES HopDong(Id),
    ChiSoId        INT NOT NULL REFERENCES ChiSoDienNuoc(Id),
    Thang          INT NOT NULL,
    Nam            INT NOT NULL,
    TienPhong      DECIMAL(12,0) NOT NULL,
    TienDien       DECIMAL(12,0) NOT NULL,
    TienNuoc       DECIMAL(12,0) NOT NULL,
    TienDichVuKhac DECIMAL(12,0) NOT NULL DEFAULT 0,
    TongTien       DECIMAL(12,0) NOT NULL,
    HanDong        DATE NOT NULL,
    TrangThai      NVARCHAR(12) NOT NULL DEFAULT N'ChuaDong'
                   CHECK (TrangThai IN (N'ChuaDong', N'DongMotPhan', N'DaDong')),
    CONSTRAINT UQ_HoaDon_HD_Thang UNIQUE (HopDongId, Nam, Thang)
);

CREATE TABLE ThanhToan (
    Id        INT IDENTITY PRIMARY KEY,
    HoaDonId  INT NOT NULL REFERENCES HoaDon(Id),
    SoTien    DECIMAL(12,0) NOT NULL CHECK (SoTien > 0),
    HinhThuc  NVARCHAR(20) NOT NULL CHECK (HinhThuc IN (N'TienMat', N'ChuyenKhoan')),
    NgayDong  DATETIME NOT NULL DEFAULT GETDATE(),
    GhiChu    NVARCHAR(200) NULL
);
GO