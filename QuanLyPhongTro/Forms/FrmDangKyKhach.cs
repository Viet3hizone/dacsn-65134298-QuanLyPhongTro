using QuanLyPhongTro.DAL;
using QuanLyPhongTro.Models;
using QuanLyPhongTro.Services;

namespace QuanLyPhongTro.Forms;

public class FrmDangKyKhach : Form
{
    readonly TextBox txtHoTen = new(), txtSdt = new(), txtCccd = new(), txtQue = new(), txtMaPhong = new(), txtUser = new();
    readonly TextBox txtPass = new() { UseSystemPasswordChar = true };
    readonly TextBox txtPass2 = new() { UseSystemPasswordChar = true };
    readonly DateTimePicker dtpNgaySinh = new() { Format = DateTimePickerFormat.Short, MaxDate = DateTime.Today };

    public FrmDangKyKhach()
    {
        Text = "Đăng ký tài khoản người thuê";
        ClientSize = new Size(780, 370);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        Font = Ui.Base;
        dtpNgaySinh.Value = DateTime.Today.AddYears(-20);

        var title = new Label { Text = "ĐĂNG KÝ NGƯỜI THUÊ", Dock = DockStyle.Top, Height = 56, TextAlign = ContentAlignment.MiddleCenter, Font = Ui.Title };
        var t = Ui.FormTable(5);
        Ui.Field(t, "Họ và tên", txtHoTen, 0, 0);       Ui.Field(t, "Ngày sinh", dtpNgaySinh, 2, 0);
        Ui.Field(t, "Số điện thoại", txtSdt, 0, 1);     Ui.Field(t, "CCCD", txtCccd, 2, 1);
        Ui.Field(t, "Quê quán", txtQue, 0, 2);          Ui.Field(t, "Mã phòng (chủ trọ cấp)", txtMaPhong, 2, 2);
        Ui.Field(t, "Tên đăng nhập", txtUser, 0, 3);
        Ui.Field(t, "Mật khẩu", txtPass, 0, 4);         Ui.Field(t, "Nhập lại mật khẩu", txtPass2, 2, 4);

        var note = new Label
        {
            Dock = DockStyle.Top, Height = 30, Padding = new Padding(12, 6, 0, 0), ForeColor = Color.Gray,
            Text = "Tài khoản được kích hoạt sau khi chủ trọ xác nhận. Nếu mã phòng trùng nhiều chủ trọ, nhập dạng CT0001-P101."
        };
        var btnDK = new Button { Text = "Đăng ký", Size = new Size(130, 38), Location = new Point(250, 315) };
        var btnQL = new Button { Text = "Quay lại", Size = new Size(130, 38), Location = new Point(395, 315), DialogResult = DialogResult.Cancel };
        btnDK.Click += (_, _) => DangKy();

        Controls.Add(btnDK); Controls.Add(btnQL);
        Controls.Add(note); Controls.Add(t); Controls.Add(title);
        CancelButton = btnQL;
    }

    void DangKy()
    {
        string ten = txtHoTen.Text.Trim(), sdt = txtSdt.Text.Trim(), cccd = txtCccd.Text.Trim();
        string ma = txtMaPhong.Text.Trim(), user = txtUser.Text.Trim(), pass = txtPass.Text;

        if (ten.Length == 0) { Ui.Canh("Vui lòng nhập họ tên."); return; }
        if (!Validator.Sdt(sdt)) { Ui.Canh("Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0."); return; }
        if (!Validator.Cccd(cccd)) { Ui.Canh("CCCD phải gồm đúng 12 chữ số."); return; }
        if (ma.Length == 0) { Ui.Canh("Vui lòng nhập mã phòng do chủ trọ cung cấp."); return; }
        if (!Validator.Username(user)) { Ui.Canh("Tên đăng nhập 4-50 ký tự, chỉ gồm chữ, số, dấu _ hoặc dấu chấm."); return; }
        if (pass.Length < 6) { Ui.Canh("Mật khẩu phải có ít nhất 6 ký tự."); return; }
        if (pass != txtPass2.Text) { Ui.Canh("Mật khẩu nhập lại không khớp."); return; }

        try
        {
            var k = new KhachThue
            {
                HoTen = ten, SDT = sdt, SoCCCD = cccd, NgaySinh = dtpNgaySinh.Value.Date,
                QueQuan = string.IsNullOrWhiteSpace(txtQue.Text) ? null : txtQue.Text.Trim()
            };
            string salt = PasswordHasher.NewSalt();
            string? loi = AuthDAL.DangKyKhach(k, ma, user, PasswordHasher.Hash(pass, salt), salt);
            if (loi != null) { Ui.Canh(loi); return; }
            Ui.Thong("Đăng ký thành công! Tài khoản sẽ được kích hoạt sau khi chủ trọ xác nhận.");
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex) { Ui.Loi(ex); }
    }
}
