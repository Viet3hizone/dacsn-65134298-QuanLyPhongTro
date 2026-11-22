namespace QuanLyPhongTro.Models;

/// <summary>Thông tin người đang đăng nhập.</summary>
public static class Session
{
    public static TaiKhoan? TaiKhoan;
    public static int ChuTroId;
    public static int KhachId;
    public static int NhaTroId;
    public static string HoTen = "";

    public static bool LaChuTro => TaiKhoan?.Role == VaiTro.ChuTro;

    public static void Clear()
    {
        TaiKhoan = null; ChuTroId = 0; KhachId = 0; NhaTroId = 0; HoTen = "";
    }
}
