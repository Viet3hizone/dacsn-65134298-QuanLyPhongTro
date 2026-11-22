namespace QuanLyPhongTro.Models;

public static class VaiTro
{
    public const string ChuTro = "ChuTro";
    public const string Khach = "Khach";
}

public class TaiKhoan
{
    public int Id { get; set; }
    public string TenDangNhap { get; set; } = "";
    public string MatKhauHash { get; set; } = "";
    public string Salt { get; set; } = "";
    public string Role { get; set; } = "";
}
