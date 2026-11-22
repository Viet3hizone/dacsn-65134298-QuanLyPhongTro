using System.Data;
using QuanLyPhongTro.Models;
using static QuanLyPhongTro.DAL.Db;

namespace QuanLyPhongTro.DAL;

public static class PhongDAL
{
    static Phong Map(DataRow r) => new()
    {
        Id = (int)r["Id"],
        NhaTroId = (int)r["NhaTroId"],
        ChuTroId = (int)r["ChuTroId"],
        MaPhong = (string)r["MaPhong"],
        TenPhong = r["TenPhong"] as string ?? "",
        DienTich = r["DienTich"] as decimal?,
        GiaThue = (decimal)r["GiaThue"],
        TrangThai = (string)r["TrangThai"],
        SoNguoiToiDa = (int)r["SoNguoiToiDa"],
        TienNghi = r["TienNghi"] as string,
        NguoiThue = r.Table.Columns.Contains("NguoiThue") ? r["NguoiThue"] as string : null
    };

    /// <summary>Danh mục phòng của chủ trọ, kèm tên người thuê (từ hợp đồng đang hiệu lực).</summary>
    public static List<Phong> GetAll(int chuTroId, string? keyword, string? trangThai)
    {
        keyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
        var dt = Query(@"SELECT p.*, k.HoTen AS NguoiThue FROM Phong p
            LEFT JOIN HopDong h ON h.PhongId = p.Id AND h.TrangThai = N'HieuLuc'
            LEFT JOIN Khach k ON k.Id = h.KhachId
            WHERE p.ChuTroId=@c
              AND (@kw IS NULL OR p.MaPhong LIKE @like OR p.TenPhong LIKE @like)
              AND (@tt IS NULL OR p.TrangThai=@tt)
            ORDER BY p.MaPhong",
            P("@c", chuTroId), P("@kw", keyword), P("@like", "%" + keyword + "%"), P("@tt", trangThai));
        return dt.Rows.Cast<DataRow>().Select(Map).ToList();
    }

    public static List<Phong> GetByNhaTro(int nhaTroId) =>
        Query("SELECT * FROM Phong WHERE NhaTroId=@n ORDER BY MaPhong", P("@n", nhaTroId))
            .Rows.Cast<DataRow>().Select(Map).ToList();

    /// <summary>Dùng cho ComboBox chọn phòng; mục đầu (Id = 0) là "chưa xếp phòng".</summary>
    public static List<Phong> GetForChon(int chuTroId)
    {
        var list = new List<Phong> { new() { Id = 0, MaPhong = "(Chưa xếp phòng)" } };
        list.AddRange(GetAll(chuTroId, null, null));
        return list;
    }

    public static void Insert(Phong p) => Execute(
        @"INSERT INTO Phong(NhaTroId,ChuTroId,MaPhong,TenPhong,DienTich,GiaThue,TrangThai,SoNguoiToiDa,TienNghi)
          VALUES(@nt,@ct,@ma,@ten,@dt,@gia,@tt,@sn,@tn)",
        P("@nt", p.NhaTroId), P("@ct", p.ChuTroId), P("@ma", p.MaPhong), P("@ten", p.TenPhong), P("@dt", p.DienTich),
        P("@gia", p.GiaThue), P("@tt", p.TrangThai), P("@sn", p.SoNguoiToiDa), P("@tn", p.TienNghi));

    public static void Update(Phong p) => Execute(
        @"UPDATE Phong SET MaPhong=@ma, TenPhong=@ten, DienTich=@dt, GiaThue=@gia, TrangThai=@tt,
                SoNguoiToiDa=@sn, TienNghi=@tn WHERE Id=@id AND ChuTroId=@ct",
        P("@id", p.Id), P("@ct", p.ChuTroId), P("@ma", p.MaPhong), P("@ten", p.TenPhong), P("@dt", p.DienTich),
        P("@gia", p.GiaThue), P("@tt", p.TrangThai), P("@sn", p.SoNguoiToiDa), P("@tn", p.TienNghi));

    public static void UpdateTrangThai(int id, int chuTroId, string trangThai) => Execute(
        "UPDATE Phong SET TrangThai=@tt WHERE Id=@id AND ChuTroId=@c", P("@tt", trangThai), P("@id", id), P("@c", chuTroId));

    public static void Delete(int id, int chuTroId) =>
        Execute("DELETE FROM Phong WHERE Id=@id AND ChuTroId=@c", P("@id", id), P("@c", chuTroId));

    public static int CountHopDongHieuLuc(int phongId) =>
        Convert.ToInt32(Scalar("SELECT COUNT(*) FROM HopDong WHERE PhongId=@id AND TrangThai=N'HieuLuc'", P("@id", phongId)));
}
