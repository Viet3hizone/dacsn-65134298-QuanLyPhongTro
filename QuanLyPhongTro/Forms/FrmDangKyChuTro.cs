using QuanLyPhongTro.DAL;
using QuanLyPhongTro.Services;

namespace QuanLyPhongTro.Forms;

public class FrmDangKyChuTro : Form
{
    readonly TextBox txtHoTen = new(), txtSdt = new(), txtTenNha = new(), txtDiaChi = new();
    readonly TextBox txtNganHang = new(), txtSoTK = new(), txtChuTK = new(), txtUser = new();
    readonly TextBox txtPass = new() { UseSystemPasswordChar = true };
    readonly TextBox txtPass2 = new() { UseSystemPasswordChar = true };
    readonly ComboBox cboTinh = new() { DropDownStyle = ComboBoxStyle.DropDown };
    readonly ComboBox cboHuyen = new() { DropDownStyle = ComboBoxStyle.DropDown };

    public FrmDangKyChuTro()
    {
        Text = "Đăng ký tài khoản chủ trọ";
        ClientSize = new Size(780, 430);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        Font = Ui.Base;

        var title = new Label { Text = "ĐĂNG KÝ CHỦ TRỌ", Dock = DockStyle.Top, Height = 56, TextAlign = ContentAlignment.MiddleCenter, Font = Ui.Title };
        var t = Ui.FormTable(6);
        Ui.Field(t, "Họ và tên chủ trọ", txtHoTen, 0, 0); Ui.Field(t, "Số điện thoại", txtSdt, 2, 0);
        Ui.Field(t, "Tên nhà trọ", txtTenNha, 0, 1);       Ui.Field(t, "Tỉnh / Thành phố", cboTinh, 2, 1);
        Ui.Field(t, "Quận / Huyện", cboHuyen, 0, 2);       Ui.Field(t, "Địa chỉ (số nhà, đường)", txtDiaChi, 2, 2);
        Ui.Field(t, "Ngân hàng", txtNganHang, 0, 3);       Ui.Field(t, "Số tài khoản", txtSoTK, 2, 3);
        Ui.Field(t, "Tên chủ tài khoản", txtChuTK, 0, 4);  Ui.Field(t, "Tên đăng nhập", txtUser, 2, 4);
        Ui.Field(t, "Mật khẩu", txtPass, 0, 5);            Ui.Field(t, "Nhập lại mật khẩu", txtPass2, 2, 5);

        var note = new Label
        {
            Dock = DockStyle.Top, Height = 30, Padding = new Padding(12, 6, 0, 0), ForeColor = Color.Gray,
            Text = "Thông tin tỉnh, huyện sẽ dùng để hiển thị nhà trọ trong màn hình tìm phòng. Tài khoản ngân hàng dùng để tạo QR thu tiền."
        };
        var btnDK = new Button { Text = "Đăng ký", Size = new Size(130, 38), Location = new Point(250, 375) };
        var btnQL = new Button { Text = "Quay lại", Size = new Size(130, 38), Location = new Point(395, 375), DialogResult = DialogResult.Cancel };
        btnDK.Click += (_, _) => DangKy();

        Controls.Add(btnDK); Controls.Add(btnQL);
        Controls.Add(note); Controls.Add(t); Controls.Add(title);
        CancelButton = btnQL;

        Load += (_, _) =>
        {
            try
            {
                cboTinh.Items.AddRange(NhaTroDAL.GetTinh().ToArray());
                cboHuyen.Items.AddRange(NhaTroDAL.GetHuyen(null).ToArray());
            }
            catch { /* bỏ qua: vẫn nhập tay được */ }
        };
    }

    void DangKy()
    {
        string ten = txtHoTen.Text.Trim(), sdt = txtSdt.Text.Trim(), nha = txtTenNha.Text.Trim();
        string tinh = cboTinh.Text.Trim(), huyen = cboHuyen.Text.Trim(), dc = txtDiaChi.Text.Trim();
        string user = txtUser.Text.Trim(), pass = txtPass.Text;

        if (ten.Length == 0 || nha.Length == 0 || tinh.Length == 0 || huyen.Length == 0 || dc.Length == 0)
        { Ui.Canh("Vui lòng nhập đủ họ tên, tên nhà trọ, tỉnh, huyện và địa chỉ."); return; }
        if (!Validator.Sdt(sdt)) { Ui.Canh("Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0."); return; }
        if (!Validator.Username(user)) { Ui.Canh("Tên đăng nhập 4-50 ký tự, chỉ gồm chữ, số, dấu _ hoặc dấu chấm."); return; }
        if (pass.Length < 6) { Ui.Canh("Mật khẩu phải có ít nhất 6 ký tự."); return; }
        if (pass != txtPass2.Text) { Ui.Canh("Mật khẩu nhập lại không khớp."); return; }

        try
        {
            string salt = PasswordHasher.NewSalt();
            AuthDAL.DangKyChuTro(ten, sdt, nha, tinh, huyen, dc,
                Nz(txtNganHang.Text), Nz(txtSoTK.Text), Nz(txtChuTK.Text), user, PasswordHasher.Hash(pass, salt), salt);
            Ui.Thong("Đăng ký thành công! Bạn có thể đăng nhập bằng vai trò Chủ trọ.");
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex) { Ui.Loi(ex); }
    }

    static string? Nz(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
