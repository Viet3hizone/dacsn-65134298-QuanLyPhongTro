using QuanLyPhongTro.Models;
using QuanLyPhongTro.Services;

namespace QuanLyPhongTro.Forms;

public class FrmLogin : Form
{
    readonly TextBox txtUser = new();
    readonly TextBox txtPass = new() { UseSystemPasswordChar = true };
    readonly RadioButton rdoChu = new() { Text = "Chủ trọ", Checked = true, AutoSize = true };
    readonly RadioButton rdoKhach = new() { Text = "Người thuê", AutoSize = true };

    public FrmLogin()
    {
        Text = "Đăng nhập hệ thống - Quản Lý Phòng Trọ";
        ClientSize = new Size(420, 310);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        Font = Ui.Base;

        var title = new Label
        {
            Text = "QUẢN LÝ PHÒNG TRỌ", Dock = DockStyle.Top, Height = 70,
            TextAlign = ContentAlignment.MiddleCenter, Font = Ui.Title
        };

        var t = new TableLayoutPanel { Dock = DockStyle.Top, Height = 135, ColumnCount = 2, RowCount = 3, Padding = new Padding(25, 5, 25, 0) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
        for (int i = 0; i < 3; i++) t.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        txtUser.Dock = DockStyle.Fill; txtPass.Dock = DockStyle.Fill;
        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill };
        flow.Controls.Add(rdoChu); flow.Controls.Add(rdoKhach);
        t.Controls.Add(new Label { Text = "Tên đăng nhập", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        t.Controls.Add(txtUser, 1, 0);
        t.Controls.Add(new Label { Text = "Mật khẩu", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        t.Controls.Add(txtPass, 1, 1);
        t.Controls.Add(new Label { Text = "Vai trò", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        t.Controls.Add(flow, 1, 2);

        var btnLogin = new Button { Text = "Đăng nhập", Size = new Size(130, 38), Location = new Point(70, 242) };
        var btnDangKy = new Button { Text = "Đăng ký", Size = new Size(130, 38), Location = new Point(215, 242) };
        btnLogin.Click += (_, _) => DoLogin();
        btnDangKy.Click += (_, _) =>
        {
            using Form f = rdoChu.Checked ? new FrmDangKyChuTro() : (Form)new FrmDangKyKhach();
            f.ShowDialog(this);
        };

        Controls.Add(btnLogin); Controls.Add(btnDangKy);
        Controls.Add(t); Controls.Add(title);
        AcceptButton = btnLogin;
    }

    void DoLogin()
    {
        string u = txtUser.Text.Trim(), p = txtPass.Text;
        if (u.Length == 0 || p.Length == 0) { Ui.Canh("Vui lòng nhập tên đăng nhập và mật khẩu."); return; }
        try
        {
            string role = rdoChu.Checked ? VaiTro.ChuTro : VaiTro.Khach;
            if (AuthService.DangNhap(u, p, role, out string loi)) DialogResult = DialogResult.OK;
            else { Ui.Canh(loi); txtPass.Clear(); txtPass.Focus(); }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không kết nối được cơ sở dữ liệu.\nKiểm tra chuỗi kết nối trong DAL/Db.cs và đã chạy các file SQL chưa.\n\n" + ex.Message,
                "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
