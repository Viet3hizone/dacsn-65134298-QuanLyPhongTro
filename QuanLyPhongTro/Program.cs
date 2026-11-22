using QuanLyPhongTro.Forms;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        while (true)
        {
            Session.Clear();
            using (var tim = new FrmTimPhong())
            {
                // Đóng cửa sổ Tìm phòng trọ = thoát; đăng nhập thành công = vào khung chính
                if (tim.ShowDialog() != DialogResult.OK || Session.TaiKhoan == null) break;
            }
            using var main = new FrmMain();
            main.ShowDialog();
            if (!main.DangXuat) break;   // Đăng xuất -> quay lại màn hình Tìm phòng trọ
        }
    }
}
