using System.Data;
using QuanLyPhongTro.Models;
using static QuanLyPhongTro.DAL.Db;

namespace QuanLyPhongTro.DAL;

public static class NhaTroDAL
{
    public static List<string> GetTinh() =>
        Query("SELECT DISTINCT Tinh FROM NhaTro ORDER BY Tinh").Rows.Cast<DataRow>().Select(r => (string)r[0]).ToList();

    public static List<string> GetHuyen(string? tinh) =>
        Query("SELECT DISTINCT Huyen FROM NhaTro WHERE (@t IS NULL OR Tinh=@t) ORDER BY Huyen", P("@t", tinh))
            .Rows.Cast<DataRow>().Select(r => (string)r[0]).ToList();

    /// <summary>Màn hình Tìm phòng trọ (công khai).</summary>
    public static List<KetQuaTim> TimKiem(string? tinh, string? huyen)
    {
        var dt = Query(@"SELECT n.Id, c.HoTen AS ChuTro, ISNULL(n.TenNhaTro, N'(chưa đặt tên)') AS TenNhaTro,
                n.DiaChiCuThe, c.SDT,
                SUM(CASE WHEN p.TrangThai = N'Trong' THEN 1 ELSE 0 END) AS PhongTrong,
                MIN(p.GiaThue) AS GiaMin, MAX(p.GiaThue) AS GiaMax
            FROM NhaTro n
            JOIN ChuTro c ON c.Id = n.ChuTroId
            LEFT JOIN Phong p ON p.NhaTroId = n.Id
            WHERE (@t IS NULL OR n.Tinh=@t) AND (@h IS NULL OR n.Huyen=@h)
            GROUP BY n.Id, c.HoTen, n.TenNhaTro, n.DiaChiCuThe, c.SDT
            ORDER BY n.TenNhaTro", P("@t", tinh), P("@h", huyen));
        return dt.Rows.Cast<DataRow>().Select(r => new KetQuaTim
        {
            NhaTroId = (int)r["Id"],
            ChuTro = (string)r["ChuTro"],
            TenNhaTro = (string)r["TenNhaTro"],
            DiaChi = (string)r["DiaChiCuThe"],
            Sdt = (string)r["SDT"],
            PhongTrong = Convert.ToInt32(r["PhongTrong"]),
            GiaMin = r["GiaMin"] as decimal?,
            GiaMax = r["GiaMax"] as decimal?
        }).ToList();
    }

    public static NhaTro? GetCuaChu(int chuTroId)
    {
        var dt = Query(@"SELECT TOP 1 n.*, c.SDT FROM NhaTro n JOIN ChuTro c ON c.Id = n.ChuTroId
                         WHERE n.ChuTroId=@c ORDER BY n.Id", P("@c", chuTroId));
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return new NhaTro
        {
            Id = (int)r["Id"],
            TenNhaTro = r["TenNhaTro"] as string,
            Tinh = (string)r["Tinh"],
            Huyen = (string)r["Huyen"],
            DiaChiCuThe = (string)r["DiaChiCuThe"],
            MoTa = r["MoTa"] as string,
            Sdt = (string)r["SDT"]
        };
    }

    public static void Update(NhaTro n, int chuTroId)
    {
        Execute(@"UPDATE NhaTro SET TenNhaTro=@ten, Tinh=@t, Huyen=@h, DiaChiCuThe=@dc, MoTa=@mt WHERE Id=@id AND ChuTroId=@c",
            P("@id", n.Id), P("@c", chuTroId), P("@ten", n.TenNhaTro), P("@t", n.Tinh), P("@h", n.Huyen),
            P("@dc", n.DiaChiCuThe), P("@mt", n.MoTa));
        Execute("UPDATE ChuTro SET SDT=@s WHERE Id=@c", P("@s", n.Sdt), P("@c", chuTroId));
    }
}
