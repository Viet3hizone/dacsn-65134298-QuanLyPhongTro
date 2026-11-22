using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Forms;

/// <summary>Khung chính: thanh tiêu đề + menu bên trái + vùng nội dung (theo mockup).</summary>
public class FrmMain : Form
{
    public bool DangXuat { get; private set; }
    readonly Panel content = new() { Dock = DockStyle.Fill };
    readonly List<Button> navButtons = new();
    static readonly Color MauThuong = Color.FromArgb(243, 244, 246);
    static readonly Color MauChon = Color.FromArgb(191, 219, 254);

    public FrmMain()
    {
        bool chu = Session.LaChuTro;
        Text = "Quản Lý Phòng Trọ";
        Font = Ui.Base;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1000, 600);

        var header = new Label
        {
            Dock = DockStyle.Top, Height = 52, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(16, 0, 0, 0),
            BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, Font = new Font("Segoe UI", 13, FontStyle.Bold),
            Text = chu ? $"Quản Lý Phòng Trọ - Chủ trọ: {Session.HoTen}" : $"Quản Lý Phòng Trọ - Người thuê: {Session.HoTen}"
        };
        var side = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = MauThuong };

        var items = chu ? MenuChuTro() : MenuKhach();
        // Dock=Top: control add sau nằm trên cùng, nên add ngược để mục đầu ở trên
        for (int i = items.Count - 1; i >= 0; i--)
        {
            var (text, make) = items[i];
            var b = new Button
            {
                Text = "   " + text, Dock = DockStyle.Top, Height = 46, FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft, BackColor = MauThuong
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += (_, _) => Mo(b, make);
            navButtons.Insert(0, b);
            side.Controls.Add(b);
        }
        var btnOut = new Button
        {
            Text = "   Đăng xuất", Dock = DockStyle.Bottom, Height = 46, FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.FromArgb(254, 226, 226)
        };
        btnOut.FlatAppearance.BorderSize = 0;
        btnOut.Click += (_, _) => { if (Ui.Hoi("Đăng xuất khỏi hệ thống?")) { DangXuat = true; Close(); } };
        side.Controls.Add(btnOut);

        Controls.Add(content); Controls.Add(side); Controls.Add(header);
        Load += (_, _) => { if (navButtons.Count > 0) navButtons[0].PerformClick(); };
    }

    static List<(string, Func<Form>)> MenuChuTro() => new()
    {
        ("Thông tin nhà trọ", () => new FrmNhaTro()),
        ("Quản lý phòng", () => new FrmPhong()),
        ("Khách thuê", () => new FrmKhachThue()),
        ("Hợp đồng thuê", () => new FrmTamThoi("Hợp đồng thuê", "Sẽ làm ở Tuần 4 (tiếp theo)")),
        ("Dịch vụ & đơn giá", () => new FrmTamThoi("Dịch vụ & đơn giá", "Sẽ làm ở Tuần 4 (tiếp theo)")),
        ("Chỉ số điện nước", () => new FrmTamThoi("Chỉ số điện nước", "Sẽ làm ở Tuần 5")),
        ("Lập hóa đơn", () => new FrmTamThoi("Lập hóa đơn", "Sẽ làm ở Tuần 5")),
        ("Thu tiền & công nợ", () => new FrmTamThoi("Thu tiền & công nợ", "Sẽ làm ở Tuần 6")),
        ("Thống kê doanh thu", () => new FrmTamThoi("Thống kê doanh thu", "Sẽ làm ở Tuần 7")),
    };

    static List<(string, Func<Form>)> MenuKhach() => new()
    {
        ("Hóa đơn tháng này", () => new FrmTamThoi("Hóa đơn tháng này", "Sẽ làm ở Tuần 5-6")),
        ("Lịch sử thanh toán", () => new FrmTamThoi("Lịch sử thanh toán", "Sẽ làm ở Tuần 6")),
        ("Thông tin cá nhân", () => new FrmThongTinKhach()),
    };

    void Mo(Button b, Func<Form> make)
    {
        foreach (var n in navButtons) n.BackColor = MauThuong;
        b.BackColor = MauChon;
        var cu = content.Controls.OfType<Form>().ToList();
        content.Controls.Clear();
        foreach (var f in cu) f.Dispose();

        var form = make();
        form.TopLevel = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock = DockStyle.Fill;
        content.Controls.Add(form);
        form.Show();
    }
}
