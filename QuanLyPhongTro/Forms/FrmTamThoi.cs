namespace QuanLyPhongTro.Forms;

/// <summary>Trang giữ chỗ cho chức năng sẽ làm ở các tuần sau.</summary>
public class FrmTamThoi : Form
{
    public FrmTamThoi(string ten, string ghiChu)
    {
        Text = ten;
        Controls.Add(new Label
        {
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 14),
            ForeColor = Color.Gray, Text = $"{ten}\n\n{ghiChu}"
        });
    }
}
