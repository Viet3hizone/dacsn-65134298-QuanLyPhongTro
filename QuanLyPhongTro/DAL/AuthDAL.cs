using Microsoft.Data.SqlClient;
using QuanLyPhongTro.Models;
using static QuanLyPhongTro.DAL.Db;

namespace QuanLyPhongTro.DAL;

public static class AuthDAL
{
    public static TaiKhoan? GetTaiKhoan(string user)
    {
        var dt = Query("SELECT * FROM TaiKhoan WHERE TenDangNhap=@u", P("@u", user));
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return new TaiKhoan
        {
            Id = (int)r["Id"],
            TenDangNhap = (string)r["TenDangNhap"],
            MatKhauHash = (string)r["MatKhauHash"],
            Salt = (string)r["Salt"],
            Role = (string)r["Role"]
        };
    }

    public static (int Id, string HoTen, int NhaTroId)? LayChuTro(int taiKhoanId)
    {
        var dt = Query(@"SELECT c.Id, c.HoTen,
                ISNULL((SELECT TOP 1 n.Id FROM NhaTro n WHERE n.ChuTroId = c.Id ORDER BY n.Id), 0) AS NhaTroId
            FROM ChuTro c WHERE c.TaiKhoanId=@t", P("@t", taiKhoanId));
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return ((int)r["Id"], (string)r["HoTen"], (int)r["NhaTroId"]);
    }

    public static (int Id, string HoTen, string TrangThai)? LayKhach(int taiKhoanId)
    {
        var dt = Query("SELECT Id, HoTen, TrangThai FROM Khach WHERE TaiKhoanId=@t", P("@t", taiKhoanId));
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return ((int)r["Id"], (string)r["HoTen"], (string)r["TrangThai"]);
    }

    /// <summary>Tạo TaiKhoan + ChuTro + NhaTro trong một transaction.</summary>
    public static void DangKyChuTro(string hoTen, string sdt, string tenNhaTro, string tinh, string huyen, string diaChi,
        string? nganHang, string? soTK, string? chuTK, string user, string hash, string salt)
    {
        using var c = GetConnection();
        c.Open();
        using var tx = c.BeginTransaction();
        try
        {
            int tk = Convert.ToInt32(ScalarTx(c, tx,
                "INSERT INTO TaiKhoan(TenDangNhap,MatKhauHash,Salt,Role) VALUES(@u,@h,@s,N'ChuTro'); SELECT SCOPE_IDENTITY();",
                P("@u", user), P("@h", hash), P("@s", salt)));

            int ct = Convert.ToInt32(ScalarTx(c, tx,
                @"INSERT INTO ChuTro(TaiKhoanId,HoTen,SDT,MaChuTro,TenNganHang,SoTaiKhoan,TenChuTaiKhoan)
                  VALUES(@tk,@ht,@sdt,LEFT(CONVERT(VARCHAR(36),NEWID()),10),@nh,@stk,@ctk); SELECT SCOPE_IDENTITY();",
                P("@tk", tk), P("@ht", hoTen), P("@sdt", sdt), P("@nh", nganHang), P("@stk", soTK), P("@ctk", chuTK)));

            ExecTx(c, tx, "UPDATE ChuTro SET MaChuTro = 'CT' + RIGHT('0000' + CAST(Id AS VARCHAR(10)), 4) WHERE Id=@id", P("@id", ct));

            ExecTx(c, tx, @"INSERT INTO NhaTro(ChuTroId,TenNhaTro,Tinh,Huyen,DiaChiCuThe) VALUES(@ct,@ten,@t,@h,@dc)",
                P("@ct", ct), P("@ten", tenNhaTro), P("@t", tinh), P("@h", huyen), P("@dc", diaChi));
            tx.Commit();
        }
        catch { tx.Rollback(); throw; }
    }

    /// <summary>Người thuê tự đăng ký bằng mã phòng. Trả về thông báo lỗi, hoặc null nếu thành công (chờ chủ trọ xác nhận).</summary>
    public static string? DangKyKhach(KhachThue k, string maPhongNhap, string user, string hash, string salt)
    {
        // Cho phép nhập "P101" hoặc "CT0001-P101" (khi trùng mã phòng giữa nhiều chủ trọ)
        string? mct = null;
        string ma = maPhongNhap.Trim();
        int i = ma.IndexOf('-');
        if (i > 0) { mct = ma[..i].Trim().ToUpper(); ma = ma[(i + 1)..].Trim(); }

        var dt = Query(@"SELECT p.ChuTroId, p.MaPhong FROM Phong p JOIN ChuTro c ON c.Id = p.ChuTroId
                         WHERE p.MaPhong=@ma AND (@mct IS NULL OR c.MaChuTro=@mct)",
            P("@ma", ma), P("@mct", mct));
        if (dt.Rows.Count == 0) return "Không tìm thấy mã phòng này. Hãy kiểm tra lại với chủ trọ.";
        if (dt.Rows.Count > 1) return "Mã phòng này trùng ở nhiều chủ trọ. Hãy nhập kèm mã chủ trọ, ví dụ CT0001-P101.";

        k.ChuTroId = (int)dt.Rows[0]["ChuTroId"];
        k.MaPhongDangKy = (string)dt.Rows[0]["MaPhong"];
        k.TrangThai = "ChoDuyet";
        KhachDAL.Insert(k, user, hash, salt);
        return null;
    }
}
