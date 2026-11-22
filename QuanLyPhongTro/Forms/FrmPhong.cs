using System.Globalization;
using QuanLyPhongTro.DAL;
using QuanLyPhongTro.Models;
using QuanLyPhongTro.Services;

namespace QuanLyPhongTro.Forms;

/// <summary>Danh mục phòng + thêm / sửa / xóa / đổi trạng thái (theo mockup).</summary>
public class FrmPhong : Form
{
    readonly DataGridView grid = Ui.NewGrid();
    readonly TextBox txtTim = new() { Width = 200 };
    readonly ComboBox cboLoc = new() { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly TextBox txtMa = new(), txtTen = new(), txtGia = new(), txtDienTich = new(), txtTienNghi = new();
    readonly ComboBox cboTrangThai = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    readonly NumericUpDown nudNguoi = new() { Minimum = 1, Maximum = 20, Value = 2 };
    readonly Label lblTong = new() { Dock = DockStyle.Bottom, Height = 32, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0) };

    public FrmPhong()
    {
        Text = "Danh mục phòng";
        Font = Ui.Base;

        Ui.Col(grid, "MaPhong", "Mã phòng", 12);
        Ui.Col(grid, "TenPhong", "Tên phòng", 18);
        Ui.Col(grid, "GiaThue", "Giá thuê (VNĐ)", 16, "N0");
        Ui.Col(grid, "DienTich", "Diện tích (m²)", 12, "0.##");
        Ui.Col(grid, "SoNguoiToiDa", "Số người tối đa", 12);
        Ui.Col(grid, "NguoiThue", "Người thuê", 18);
        Ui.Col(grid, "TrangThaiText", "Trạng thái", 12);

        cboLoc.Items.Add("Tất cả trạng thái"); cboLoc.Items.AddRange(TrangThaiPhong.Texts); cboLoc.SelectedIndex = 0;
        cboTrangThai.Items.AddRange(TrangThaiPhong.Texts); cboTrangThai.SelectedIndex = 0;

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(8, 8, 0, 0) };
        var btnTim = Ui.Btn("Tìm", 70);
        top.Controls.AddRange(new Control[] { new Label { Text = "Tìm phòng:", AutoSize = true, Margin = new Padding(0, 6, 4, 0) }, txtTim, cboLoc, btnTim });

        var form = Ui.FormTable(4, 38);
        Ui.Field(form, "Mã phòng", txtMa, 0, 0);          Ui.Field(form, "Tên phòng", txtTen, 2, 0);
        Ui.Field(form, "Giá thuê (VNĐ)", txtGia, 0, 1);   Ui.Field(form, "Diện tích (m²)", txtDienTich, 2, 1);
        Ui.Field(form, "Trạng thái", cboTrangThai, 0, 2); Ui.Field(form, "Số người tối đa", nudNguoi, 2, 2);
        Ui.Field(form, "Tiện nghi", txtTienNghi, 0, 3);

        var btns = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(8, 4, 0, 0) };
        var btnThem = Ui.Btn("Thêm", 80); var btnSua = Ui.Btn("Sửa", 80); var btnXoa = Ui.Btn("Xóa", 80);
        var btnDoi = Ui.Btn("Đổi trạng thái", 130); var btnMoi = Ui.Btn("Làm mới", 90);
        btns.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnDoi, btnMoi });

        Controls.Add(grid); Controls.Add(lblTong); Controls.Add(btns); Controls.Add(form); Controls.Add(top);

        Load += (_, _) => LoadData();
        grid.SelectionChanged += (_, _) => ShowSelected();
        btnTim.Click += (_, _) => LoadData();
        txtTim.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { LoadData(); e.SuppressKeyPress = true; } };
        cboLoc.SelectedIndexChanged += (_, _) => LoadData();
        btnMoi.Click += (_, _) => { txtTim.Clear(); cboLoc.SelectedIndex = 0; LoadData(); ClearInputs(); };
        btnThem.Click += (_, _) => Them();
        btnSua.Click += (_, _) => Sua();
        btnXoa.Click += (_, _) => Xoa();
        btnDoi.Click += (_, _) => DoiTrangThai();
    }

    Phong? Current() => grid.CurrentRow?.DataBoundItem as Phong;

    void LoadData()
    {
        try
        {
            string? tt = cboLoc.SelectedIndex > 0 ? TrangThaiPhong.FromText(cboLoc.Text) : null;
            var ds = PhongDAL.GetAll(Session.ChuTroId, txtTim.Text, tt);
            grid.DataSource = ds;
            grid.CurrentCell = null;
            lblTong.Text = $"Tổng: {ds.Count} phòng     Đang thuê: {ds.Count(p => p.TrangThai == TrangThaiPhong.DangThue)}     " +
                           $"Trống: {ds.Count(p => p.TrangThai == TrangThaiPhong.Trong)}     Bảo trì: {ds.Count(p => p.TrangThai == TrangThaiPhong.BaoTri)}";
        }
        catch (Exception ex) { Ui.Loi(ex); }
    }

    void ShowSelected()
    {
        if (Current() is not Phong p) { ClearInputs(); return; }
        txtMa.Text = p.MaPhong; txtTen.Text = p.TenPhong;
        txtGia.Text = p.GiaThue.ToString("N0");
        txtDienTich.Text = p.DienTich?.ToString("0.##", CultureInfo.InvariantCulture) ?? "";
        cboTrangThai.SelectedItem = p.TrangThaiText;
        nudNguoi.Value = Math.Clamp(p.SoNguoiToiDa, 1, 20);
        txtTienNghi.Text = p.TienNghi ?? "";
    }

    void ClearInputs()
    {
        txtMa.Clear(); txtTen.Clear(); txtGia.Clear(); txtDienTich.Clear(); txtTienNghi.Clear();
        cboTrangThai.SelectedIndex = 0; nudNguoi.Value = 2;
    }

    bool TryRead(out Phong p, int id = 0)
    {
        p = new Phong { Id = id, NhaTroId = Session.NhaTroId, ChuTroId = Session.ChuTroId };
        string ma = txtMa.Text.Trim(), ten = txtTen.Text.Trim();
        if (Session.NhaTroId == 0) { Ui.Canh("Hãy nhập thông tin nhà trọ trước khi thêm phòng."); return false; }
        if (!Validator.MaPhong(ma)) { Ui.Canh("Mã phòng chỉ gồm chữ không dấu và số, tối đa 10 ký tự (ví dụ P101)."); return false; }
        if (ten.Length == 0 || ten.Length > 50) { Ui.Canh("Tên phòng không được để trống (tối đa 50 ký tự)."); return false; }

        string digits = new string(txtGia.Text.Where(char.IsDigit).ToArray());
        if (digits.Length == 0 || digits.Length > 12) { Ui.Canh("Giá thuê phải là số hợp lệ (không âm)."); return false; }

        decimal? dt = null;
        if (txtDienTich.Text.Trim().Length > 0)
        {
            if (!decimal.TryParse(txtDienTich.Text.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var d)
                || d < 0 || d > 9999)
            { Ui.Canh("Diện tích phải là số không âm."); return false; }
            dt = d;
        }
        p.MaPhong = ma; p.TenPhong = ten; p.DienTich = dt;
        p.GiaThue = decimal.Parse(digits, CultureInfo.InvariantCulture);
        p.TrangThai = TrangThaiPhong.FromText(cboTrangThai.Text);
        p.SoNguoiToiDa = (int)nudNguoi.Value;
        p.TienNghi = string.IsNullOrWhiteSpace(txtTienNghi.Text) ? null : txtTienNghi.Text.Trim();
        return true;
    }

    bool CheckTrangThai(int id, string tt)
    {
        if (tt != TrangThaiPhong.DangThue && PhongDAL.CountHopDongHieuLuc(id) > 0)
        {
            Ui.Canh("Phòng đang có hợp đồng hiệu lực, không thể chuyển sang Trống/Bảo trì. Hãy thanh lý hợp đồng trước.");
            return false;
        }
        return true;
    }

    void Them()
    {
        if (!TryRead(out var p)) return;
        try { PhongDAL.Insert(p); LoadData(); ClearInputs(); }
        catch (Exception ex) { Ui.Loi(ex); }
    }

    void Sua()
    {
        if (Current() is not Phong cur) { Ui.Canh("Hãy chọn một phòng trong danh sách."); return; }
        if (!TryRead(out var p, cur.Id) || !CheckTrangThai(cur.Id, p.TrangThai)) return;
        try { PhongDAL.Update(p); LoadData(); }
        catch (Exception ex) { Ui.Loi(ex); }
    }

    void Xoa()
    {
        if (Current() is not Phong cur) { Ui.Canh("Hãy chọn một phòng trong danh sách."); return; }
        if (!Ui.Hoi($"Xóa phòng {cur.MaPhong}?")) return;
        try { PhongDAL.Delete(cur.Id, Session.ChuTroId); LoadData(); ClearInputs(); }
        catch (Exception ex) { Ui.Loi(ex); }
    }

    void DoiTrangThai()
    {
        if (Current() is not Phong cur) { Ui.Canh("Hãy chọn một phòng trong danh sách."); return; }
        string tt = TrangThaiPhong.FromText(cboTrangThai.Text);
        if (!CheckTrangThai(cur.Id, tt)) return;
        try { PhongDAL.UpdateTrangThai(cur.Id, Session.ChuTroId, tt); LoadData(); }
        catch (Exception ex) { Ui.Loi(ex); }
    }
}
