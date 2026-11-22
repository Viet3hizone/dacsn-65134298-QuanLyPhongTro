using System.Data;
using QuanLyPhongTro.Models;
using static QuanLyPhongTro.DAL.Db;

namespace QuanLyPhongTro.DAL;

public static class KhachDAL
{
    const string SelectSql = @"SELECT k.*, t.TenDangNhap, c.HoTen AS TenChuTro
        FROM Khach k JOIN TaiKhoan t ON t.Id = k.TaiKhoanId
        LEFT JOIN ChuTro c ON c.Id = k.ChuTroId";

    static KhachThue Map(DataRow r) => new()
    {
        Id = (int)r["Id"],
        TaiKhoanId = (int)r["TaiKhoanId"],
        HoTen = (string)r["HoTen"],
        SDT = (string)r["SDT"],
        SoCCCD = (string)r["SoCCCD"],
        NgaySinh = r["NgaySinh"] as DateTime?,
        QueQuan = r["QueQuan"] as string,
        ChuTroId = r["ChuTroId"] as int?,
        MaPhongDangKy = r["MaPhongDangKy"] as string,
        TrangThai = (string)r["TrangThai"],
        TenDangNhap = (string)r["TenDangNhap"],
        TenChuTro = r["TenChuTro"] as string
    };

    public static List<KhachThue> GetAll(int chuTroId, string? keyword)
    {
        keyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
        var dt = Query(SelectSql + @" WHERE k.ChuTroId=@c AND (@kw IS NULL OR k.HoTen LIKE @like OR k.SoCCCD LIKE @like
                OR k.SDT LIKE @like OR k.MaPhongDangKy LIKE @like) ORDER BY k.HoTen",
            P("@c", chuTroId), P("@kw", keyword), P("@like", "%" + keyword + "%"));
        return dt.Rows.Cast<DataRow>().Select(Map).ToList();
    }

    public static KhachThue? GetById(int id)
    {
        var dt = Query(SelectSql + " WHERE k.Id=@id", P("@id", id));
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    /// <summary>Tạo TaiKhoan (Role Khach) + Khach trong một transaction.</summary>
    public static int Insert(KhachThue k, string user, string hash, string salt)
    {
        using var c = GetConnection();
        c.Open();
        using var tx = c.BeginTransaction();
        try
        {
            int tk = Convert.ToInt32(ScalarTx(c, tx,
                "INSERT INTO TaiKhoan(TenDangNhap,MatKhauHash,Salt,Role) VALUES(@u,@h,@s,N'Khach'); SELECT SCOPE_IDENTITY();",
                P("@u", user), P("@h", hash), P("@s", salt)));
            int id = Convert.ToInt32(ScalarTx(c, tx,
                @"INSERT INTO Khach(TaiKhoanId,HoTen,SDT,SoCCCD,NgaySinh,QueQuan,ChuTroId,MaPhongDangKy,TrangThai)
                  VALUES(@tk,@ht,@sdt,@cccd,@ns,@que,@ct,@ma,@tt); SELECT SCOPE_IDENTITY();",
                P("@tk", tk), P("@ht", k.HoTen), P("@sdt", k.SDT), P("@cccd", k.SoCCCD), P("@ns", k.NgaySinh),
                P("@que", k.QueQuan), P("@ct", k.ChuTroId), P("@ma", k.MaPhongDangKy), P("@tt", k.TrangThai)));
            tx.Commit();
            return id;
        }
        catch { tx.Rollback(); throw; }
    }

    public static void Update(KhachThue k, int chuTroId) => Execute(
        @"UPDATE Khach SET HoTen=@ht, SDT=@sdt, SoCCCD=@cccd, NgaySinh=@ns, QueQuan=@que, MaPhongDangKy=@ma
          WHERE Id=@id AND ChuTroId=@c",
        P("@id", k.Id), P("@c", chuTroId), P("@ht", k.HoTen), P("@sdt", k.SDT), P("@cccd", k.SoCCCD),
        P("@ns", k.NgaySinh), P("@que", k.QueQuan), P("@ma", k.MaPhongDangKy));

    public static void Duyet(int id, int chuTroId) => Execute(
        "UPDATE Khach SET TrangThai=N'HoatDong' WHERE Id=@id AND ChuTroId=@c", P("@id", id), P("@c", chuTroId));

    /// <summary>Xóa khách và tài khoản. Lỗi 547 nếu khách còn hợp đồng.</summary>
    public static void Delete(int id, int chuTroId)
    {
        using var c = GetConnection();
        c.Open();
        using var tx = c.BeginTransaction();
        try
        {
            var tk = ScalarTx(c, tx, "SELECT TaiKhoanId FROM Khach WHERE Id=@id AND ChuTroId=@c", P("@id", id), P("@c", chuTroId));
            if (tk == null) { tx.Rollback(); return; }
            ExecTx(c, tx, "UPDATE NguoiO SET KhachId=NULL, TrangThaiLienKet=N'ChuaLienKet' WHERE KhachId=@id", P("@id", id));
            ExecTx(c, tx, "DELETE FROM Khach WHERE Id=@id", P("@id", id));
            ExecTx(c, tx, "DELETE FROM TaiKhoan WHERE Id=@t", P("@t", Convert.ToInt32(tk)));
            tx.Commit();
        }
        catch { tx.Rollback(); throw; }
    }
}
