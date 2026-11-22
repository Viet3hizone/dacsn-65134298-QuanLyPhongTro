using QuanLyPhongTro.DAL;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Forms;

/// <summary>Màn hình đầu tiên: Tìm phòng trọ (ai cũng xem được) + nút Đăng nhập.</summary>
public class FrmTimPhong : Form
{
    readonly ComboBox cboTinh = new() { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly ComboBox cboHuyen = new() { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly DataGridView grid = Ui.NewGrid();
    readonly Label lblKq = new() { Dock = DockStyle.Top, Height = 32, Padding = new Padding(10, 8, 0, 0) };
    bool dangNap;

    public FrmTimPhong()
    {
        Text = "Quản Lý Phòng Trọ - Tìm phòng trọ";
        Font = Ui.Base;
        ClientSize = new Size(1100, 620);
        StartPosition = FormStartPosition.CenterScreen;

        var header = new Panel { Dock = DockStyle.Top, Height = 60 };
        header.Controls.Add(new Label { Text = "TÌM PHÒNG TRỌ", Font = Ui.Title, AutoSize = true, Location = new Point(15, 14) });
        var btnLogin = new Button { Text = "Đăng nhập", Size = new Size(120, 36), Top = 12 };
        header.Controls.Add(btnLogin);
        header.Resize += (_, _) => btnLogin.Left = header.ClientSize.Width - btnLogin.Width - 20;
        Shown += (_, _) => btnLogin.Left = header.ClientSize.Width - btnLogin.Width - 20;

        var filter = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10, 8, 0, 0) };
        var btnTim = Ui.Btn("Tìm kiếm", 100);
        var btnXoa = Ui.Btn("Xóa lọc", 90);
        var lblT = new Label { Text = "Tỉnh / Thành phố", AutoSize = true, Margin = new Padding(0, 8, 6, 0) };
        var lblH = new Label { Text = "Quận / Huyện / Thành phố", AutoSize = true, Margin = new Padding(16, 8, 6, 0) };
        btnTim.Margin = new Padding(16, 0, 0, 0);
        filter.Controls.AddRange(new Control[] { lblT, cboTinh, lblH, cboHuyen, btnTim, btnXoa });

        var note = new Label
        {
            Dock = DockStyle.Bottom, Height = 36, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0),
            Text = "Liên hệ trực tiếp chủ trọ để xem phòng và ký hợp đồng."
        };

        Ui.Col(grid, "ChuTro", "Chủ trọ", 16);
        Ui.Col(grid, "TenNhaTro", "Tên nhà trọ", 18);
        Ui.Col(grid, "DiaChi", "Địa chỉ", 18);
        Ui.Col(grid, "PhongTrong", "Phòng trống", 10);
        Ui.Col(grid, "GiaText", "Giá thuê (VNĐ/tháng)", 20);
        Ui.Col(grid, "LienHe", "Liên hệ", 12);
        grid.Columns.Add(new DataGridViewButtonColumn
        {
            HeaderText = "Chi tiết", Text = "Xem chi tiết", UseColumnTextForButtonValue = true, FillWeight = 14
        });

        // Thứ tự add: Fill trước, Bottom, rồi các Top (cái add sau nằm trên cùng)
        Controls.Add(grid); Controls.Add(note); Controls.Add(lblKq); Controls.Add(filter); Controls.Add(header);

        Load += (_, _) => { NapTinh(); TimKiem(); };
        cboTinh.SelectedIndexChanged += (_, _) => { if (!dangNap) NapHuyen(); };
        btnTim.Click += (_, _) => TimKiem();
        btnXoa.Click += (_, _) => { NapTinh(); TimKiem(); };
        btnLogin.Click += (_, _) =>
        {
            using var f = new FrmLogin();
            if (f.ShowDialog(this) == DialogResult.OK) DialogResult = DialogResult.OK;
        };
        grid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == grid.Columns.Count - 1 && grid.Rows[e.RowIndex].DataBoundItem is KetQuaTim k)
            {
                using var f = new FrmChiTietNhaTro(k);
                f.ShowDialog(this);
            }
        };
    }

    void NapTinh()
    {
        try
        {
            dangNap = true;
            cboTinh.Items.Clear(); cboTinh.Items.Add("(Tất cả)");
            cboTinh.Items.AddRange(NhaTroDAL.GetTinh().ToArray());
            cboTinh.SelectedIndex = 0;
            dangNap = false;
            NapHuyen();
        }
        catch (Exception ex) { dangNap = false; Ui.Loi(ex); }
    }

    void NapHuyen()
    {
        try
        {
            cboHuyen.Items.Clear(); cboHuyen.Items.Add("(Tất cả)");
            cboHuyen.Items.AddRange(NhaTroDAL.GetHuyen(cboTinh.SelectedIndex > 0 ? cboTinh.Text : null).ToArray());
            cboHuyen.SelectedIndex = 0;
        }
        catch (Exception ex) { Ui.Loi(ex); }
    }

    void TimKiem()
    {
        try
        {
            string? t = cboTinh.SelectedIndex > 0 ? cboTinh.Text : null;
            string? h = cboHuyen.SelectedIndex > 0 ? cboHuyen.Text : null;
            var ds = NhaTroDAL.TimKiem(t, h);
            grid.DataSource = ds;
            lblKq.Text = $"Tìm thấy {ds.Count} chủ trọ" + (h != null ? $" tại {h}" : t != null ? $" tại {t}" : "");
        }
        catch (Exception ex) { Ui.Loi(ex); }
    }
}
