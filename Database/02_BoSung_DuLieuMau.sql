-- Chạy SAU 01_DataBase.sql. Bổ sung các cột mà mockup cần + dữ liệu mẫu.
USE QuanLyPhongTro;
GO
-- Khach: ngày sinh, quê quán, thuộc chủ trọ nào, mã phòng đăng ký, trạng thái kích hoạt
IF COL_LENGTH('Khach','NgaySinh') IS NULL ALTER TABLE Khach ADD NgaySinh DATE NULL;
IF COL_LENGTH('Khach','QueQuan') IS NULL ALTER TABLE Khach ADD QueQuan NVARCHAR(100) NULL;
IF COL_LENGTH('Khach','ChuTroId') IS NULL ALTER TABLE Khach ADD ChuTroId INT NULL CONSTRAINT FK_Khach_ChuTro REFERENCES ChuTro(Id);
IF COL_LENGTH('Khach','MaPhongDangKy') IS NULL ALTER TABLE Khach ADD MaPhongDangKy VARCHAR(10) NULL;
IF COL_LENGTH('Khach','TrangThai') IS NULL
    ALTER TABLE Khach ADD TrangThai NVARCHAR(12) NOT NULL CONSTRAINT DF_Khach_TT DEFAULT N'HoatDong'
        CONSTRAINT CK_Khach_TT CHECK (TrangThai IN (N'ChoDuyet', N'HoatDong'));
-- Phong: tên phòng, số người tối đa, tiện nghi
IF COL_LENGTH('Phong','TenPhong') IS NULL ALTER TABLE Phong ADD TenPhong NVARCHAR(50) NULL;
IF COL_LENGTH('Phong','SoNguoiToiDa') IS NULL
    ALTER TABLE Phong ADD SoNguoiToiDa INT NOT NULL CONSTRAINT DF_Phong_SNTD DEFAULT 2 CONSTRAINT CK_Phong_SNTD CHECK (SoNguoiToiDa >= 1);
IF COL_LENGTH('Phong','TienNghi') IS NULL ALTER TABLE Phong ADD TienNghi NVARCHAR(300) NULL;
GO
-- Dữ liệu mẫu: chủ trọ admin / admin123 (băm SHA-256 + salt, giống code C#)
IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = 'admin')
BEGIN
    DECLARE @salt VARCHAR(64) = '4F8A2C91D07B3E65A1C94D2B7E08F3A6';
    INSERT INTO TaiKhoan(TenDangNhap, MatKhauHash, Salt, Role)
    VALUES ('admin', CONVERT(VARCHAR(256), HASHBYTES('SHA2_256', @salt + 'admin123'), 2), @salt, N'ChuTro');
    DECLARE @tk INT = SCOPE_IDENTITY();

    INSERT INTO ChuTro(TaiKhoanId, HoTen, SDT, MaChuTro, TenNganHang, SoTaiKhoan, TenChuTaiKhoan)
    VALUES (@tk, N'Lê Hoàng Việt', '0901234567', 'CT0001', N'Vietcombank', '0123456789', N'LE HOANG VIET');
    DECLARE @ct INT = SCOPE_IDENTITY();

    INSERT INTO NhaTro(ChuTroId, TenNhaTro, Tinh, Huyen, DiaChiCuThe)
    VALUES (@ct, N'Nhà trọ Hùng Phát', N'Khánh Hòa', N'TP. Nha Trang', N'12 đường A');
    DECLARE @nt INT = SCOPE_IDENTITY();

    INSERT INTO Phong(NhaTroId, ChuTroId, MaPhong, TenPhong, DienTich, GiaThue, TrangThai, SoNguoiToiDa) VALUES
     (@nt, @ct, 'P101', N'Phòng 101', 20, 2500000, N'DangThue', 3),
     (@nt, @ct, 'P102', N'Phòng 102', 20, 2500000, N'Trong',    3),
     (@nt, @ct, 'P103', N'Phòng 103', 24, 2800000, N'Trong',    3),
     (@nt, @ct, 'P201', N'Phòng 201', 25, 3000000, N'Trong',    4),
     (@nt, @ct, 'P202', N'Phòng 202', 25, 3000000, N'BaoTri',   4);
END
GO
