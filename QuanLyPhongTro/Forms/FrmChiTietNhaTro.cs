using QuanLyPhongTro.DAL;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Forms;

public class FrmChiTietNhaTro : Form
{
    public FrmChiTietNhaTro(KetQuaTim k)
    {
        Text = k.TenNhaTro;
        ClientSize = new Size(820, 440);
        StartPosition = FormStartPosition.CenterParent;
        Font = Ui.Base;

        var info = new Label
        {
            Dock = DockStyle.Top, Height = 80, Padding = new Padding(12, 10, 0, 0),
            Text = $"Nhà trọ: {k.TenNhaTro}\nChủ trọ: {k.ChuTro}    |    Liên hệ: {k.Sdt}\nĐịa chỉ: {k.DiaChi}"
        };
        var grid = Ui.NewGrid();
        Ui.Col(grid, "MaPhong", "Mã phòng", 12);
        Ui.Col(grid, "TenPhong", "Tên phòng", 20);
        Ui.Col(grid, "DienTich", "Diện tích (m²)", 14, "0.##");
        Ui.Col(grid, "GiaThue", "Giá thuê (đ)", 18, "N0");
        Ui.Col(grid, "TrangThaiText", "Trạng thái", 14);
        Ui.Col(grid, "TienNghi", "Tiện nghi", 30);
        Controls.Add(grid); Controls.Add(info);
        Load += (_, _) =>
        {
            try { grid.DataSource = PhongDAL.GetByNhaTro(k.NhaTroId); } catch (Exception ex) { Ui.Loi(ex); }
        };
    }
}
