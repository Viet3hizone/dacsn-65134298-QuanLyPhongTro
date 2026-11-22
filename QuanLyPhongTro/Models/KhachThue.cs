namespace QuanLyPhongTro.Models;

public class KhachThue
{
    public int Id { get; set; }
    public int TaiKhoanId { get; set; }
    public string HoTen { get; set; } = "";
    public string SDT { get; set; } = "";
    public string SoCCCD { get; set; } = "";
    public DateTime? NgaySinh { get; set; }
    public string? QueQuan { get; set; }
    public int? ChuTroId { get; set; }
    public string? MaPhongDangKy { get; set; }
    public string TrangThai { get; set; } = "HoatDong";   // ChoDuyet | HoatDong
    public string TenDangNhap { get; set; } = "";
    public string? TenChuTro { get; set; }

    public string TrangThaiText => TrangThai == "ChoDuyet" ? "Chờ xác nhận" : "Đang hoạt động";
}
