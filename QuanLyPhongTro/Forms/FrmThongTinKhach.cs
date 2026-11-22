using QuanLyPhongTro.DAL;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Forms;

/// <summary>Người thuê xem thông tin cá nhân của mình.</summary>
public class FrmThongTinKhach : Form
{
    public FrmThongTinKhach()
    {
        Text = "Thông tin cá nhân";
        Font = Ui.Base;
        var lbl = new Label { Dock = DockStyle.Fill, Padding = new Padding(30), Font = new Font("Segoe UI", 12) };
        try
        {
            var k = KhachDAL.GetById(Session.KhachId);
            lbl.Text = k == null ? "Không tìm thấy thông tin." :
                $"Họ tên: {k.HoTen}\n\nNgày sinh: {(k.NgaySinh.HasValue ? k.NgaySinh.Value.ToString("dd/MM/yyyy") : "-")}\n\n" +
                $"Số điện thoại: {k.SDT}\n\nCCCD: {k.SoCCCD}\n\nQuê quán: {k.QueQuan ?? "-"}\n\n" +
                $"Phòng: {k.MaPhongDangKy ?? "(chưa xếp phòng)"}\n\nChủ trọ: {k.TenChuTro ?? "-"}\n\nTên đăng nhập: {k.TenDangNhap}";
        }
        catch (Exception ex) { lbl.Text = ex.Message; }
        Controls.Add(lbl);
    }
}
