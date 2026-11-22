using Microsoft.Data.SqlClient;

namespace QuanLyPhongTro.Forms;

/// <summary>Các hàm dựng giao diện dùng chung.</summary>
public static class Ui
{
    public static readonly Font Base = new("Segoe UI", 10);
    public static readonly Font Title = new("Segoe UI", 16, FontStyle.Bold);

    public static DataGridView NewGrid() => new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
        AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        RowHeadersVisible = false, BackgroundColor = SystemColors.Window
    };

    public static void Col(DataGridView g, string prop, string header, float weight, string? fmt = null)
    {
        var c = new DataGridViewTextBoxColumn { DataPropertyName = prop, HeaderText = header, FillWeight = weight };
        if (fmt != null) c.DefaultCellStyle.Format = fmt;
        g.Columns.Add(c);
    }

    /// <summary>Bảng 4 cột: nhãn | ô nhập | nhãn | ô nhập.</summary>
    public static TableLayoutPanel FormTable(int rows, int rowHeight = 40)
    {
        var t = new TableLayoutPanel
        {
            Dock = DockStyle.Top, ColumnCount = 4, RowCount = rows,
            Height = rows * rowHeight + 16, Padding = new Padding(10, 8, 10, 8)
        };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < rows; i++) t.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));
        return t;
    }

    public static void Field(TableLayoutPanel t, string label, Control c, int col, int row)
    {
        t.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, col, row);
        c.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        t.Controls.Add(c, col + 1, row);
    }

    public static Button Btn(string text, int width = 90) => new() { Text = text, Width = width, Height = 34 };

    public static void Canh(string m) => MessageBox.Show(m, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public static void Thong(string m) => MessageBox.Show(m, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public static bool Hoi(string m) =>
        MessageBox.Show(m, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    /// <summary>Báo lỗi thân thiện cho lỗi CSDL thường gặp.</summary>
    public static void Loi(Exception ex)
    {
        if (ex is SqlException s)
        {
            if (s.Number is 2627 or 2601) { Canh(LoiTrung(s)); return; }
            if (s.Number == 547) { Canh("Dữ liệu này đang được dùng ở nơi khác (ví dụ hợp đồng) nên không thể thực hiện."); return; }
        }
        MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    static string LoiTrung(SqlException s)
    {
        string m = s.Message;
        if (m.Contains("dbo.TaiKhoan")) return "Tên đăng nhập đã tồn tại.";
        if (m.Contains("dbo.Khach")) return "CCCD này đã được đăng ký.";
        if (m.Contains("dbo.Phong")) return "Mã phòng đã tồn tại.";
        if (m.Contains("dbo.ChuTro")) return "Thông tin chủ trọ bị trùng.";
        return "Dữ liệu bị trùng.";
    }
}
