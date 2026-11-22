using QuanLyPhongTro.DAL;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Services;

public static class AuthService
{
    /// <summary>Đăng nhập theo vai trò đã chọn. Thành công thì nạp Session.</summary>
    public static bool DangNhap(string user, string matKhau, string role, out string loi)
    {
        loi = "";
        var tk = AuthDAL.GetTaiKhoan(user);
        if (tk == null || !PasswordHasher.Verify(matKhau, tk.Salt, tk.MatKhauHash))
        { loi = "Sai tên đăng nhập hoặc mật khẩu."; return false; }

        if (tk.Role != role)
        {
            loi = role == VaiTro.ChuTro ? "Tài khoản này không phải tài khoản Chủ trọ. Hãy chọn vai trò \"Người thuê\"."
                                         : "Tài khoản này không phải tài khoản Người thuê. Hãy chọn vai trò \"Chủ trọ\".";
            return false;
        }

        Session.Clear();
        if (role == VaiTro.ChuTro)
        {
            var c = AuthDAL.LayChuTro(tk.Id);
            if (c == null) { loi = "Không tìm thấy hồ sơ chủ trọ của tài khoản này."; return false; }
            Session.ChuTroId = c.Value.Id; Session.HoTen = c.Value.HoTen; Session.NhaTroId = c.Value.NhaTroId;
        }
        else
        {
            var k = AuthDAL.LayKhach(tk.Id);
            if (k == null) { loi = "Không tìm thấy hồ sơ người thuê của tài khoản này."; return false; }
            if (k.Value.TrangThai == "ChoDuyet")
            { loi = "Tài khoản đang chờ chủ trọ xác nhận. Vui lòng liên hệ chủ trọ."; return false; }
            Session.KhachId = k.Value.Id; Session.HoTen = k.Value.HoTen;
        }
        Session.TaiKhoan = tk;
        return true;
    }
}
