namespace QuanLyPhongTro.Models;

/// <summary>Giá trị lưu trong CSDL là không dấu (Trong/DangThue/BaoTri); hiển thị có dấu.</summary>
public static class TrangThaiPhong
{
    public const string Trong = "Trong";
    public const string DangThue = "DangThue";
    public const string BaoTri = "BaoTri";
    public static readonly string[] Texts = { "Trống", "Đang thuê", "Bảo trì" };

    public static string Text(string v) => v switch { DangThue => "Đang thuê", BaoTri => "Bảo trì", _ => "Trống" };
    public static string FromText(string t) => t switch { "Đang thuê" => DangThue, "Bảo trì" => BaoTri, _ => Trong };
}

public class Phong
{
    public int Id { get; set; }
    public int NhaTroId { get; set; }
    public int ChuTroId { get; set; }
    public string MaPhong { get; set; } = "";
    public string TenPhong { get; set; } = "";
    public decimal? DienTich { get; set; }
    public decimal GiaThue { get; set; }
    public string TrangThai { get; set; } = TrangThaiPhong.Trong;
    public int SoNguoiToiDa { get; set; } = 2;
    public string? TienNghi { get; set; }
    public string? NguoiThue { get; set; }

    public string TrangThaiText => TrangThaiPhong.Text(TrangThai);
    public override string ToString() => Id == 0 ? MaPhong : $"{MaPhong} - {TenPhong}";
}
