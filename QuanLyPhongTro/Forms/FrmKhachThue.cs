using QuanLyPhongTro.DAL;
using QuanLyPhongTro.Models;
using QuanLyPhongTro.Services;

namespace QuanLyPhongTro.Forms;

/// <summary>Chủ trọ quản lý khách thuê: thêm / sửa / xóa / tìm kiếm / duyệt tài khoản người thuê tự đăng ký.</summary>
public class FrmKhachThue : Form
{
    const string MatKhauMacDinh = "123456";

    readonly DataGridView grid = Ui.NewGrid();
    readonly TextBox txtTim = new() { Width = 260 };
    readonly TextBox txtHoTen = new(), txtSdt = new(), txtCccd = new(), txtQue = new(), txtUser = new(), txtPass = new();
    readonly DateTimePicker dtpNgay = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, MaxDate = DateTime.Today };
    readonly ComboBox cboPhong = new() { DropDownStyle = ComboBoxStyle.DropDownList };

    public FrmKhachThue()
    {
        Text = "Khách thuê";
        Font = Ui.Base;

        Ui.Col(grid, "HoTen", "Họ tên", 20);
        Ui.Col(grid, "SoCCCD", "CCCD", 14);
        Ui.Col(grid, "SDT", "Số điện thoại", 12);
        Ui.Col(grid, "NgaySinh", "Ngày sinh", 10, "dd/MM/yyyy");
        Ui.Col(grid, "QueQuan", "Quê quán", 14);
        Ui.Col(grid, "MaPhongDangKy", "Phòng", 8);
        Ui.Col(grid, "TenDangNhap", "Tài khoản", 12);
        Ui.Col(grid, "TrangThaiText", "Trạng thái", 12);

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(8, 8, 0, 0) };
        var btnTim = Ui.Btn("Tìm", 70);
        top.Controls.AddRange(new Control[] {
            new Label { Text = "Tìm (tên / CCCD / SĐT / phòng):", AutoSize = true, Margin = new Padding(0, 6, 4, 0) }, txtTim, btnTim });

        var form = Ui.FormTable(4, 38);
        Ui.Field(form, "Họ và tên", txtHoTen, 0, 0);      Ui.Field(form, "Ngày sinh", dtpNgay, 2, 0);
        Ui.Field(form, "Số điện thoại", txtSdt, 0, 1);    Ui.Field(form, "CCCD", txtCccd, 2, 1);
        Ui.Field(form, "Quê quán", txtQue, 0, 2);         Ui.Field(form, "Phòng", cboPhong, 2, 2);
        Ui.Field(form, "Tên đăng nhập", txtUser, 0, 3);   Ui.Field(form, "Mật khẩu", txtPass, 2, 3);

        var hint = new Label
        {
            Dock = DockStyle.Top, Height = 44, ForeColor = Color.Gray, Padding = new Padding(10, 2, 0, 0),
            Text = "Tên đăng nhập / mật khẩu chỉ dùng khi bấm Thêm (để trống = SĐT / 123456). Bấm Làm mới để nhập khách mới.\n" +
                   "Khách tự đăng ký sẽ ở trạng thái \"Chờ xác nhận\" - chọn dòng đó và bấm Duyệt để kích hoạt."
        };

        var btns = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(8, 4, 0, 0) };
        var btnThem = Ui.Btn("Thêm", 80); var btnSua = Ui.Btn("Sửa", 80); var btnXoa = Ui.Btn("Xóa", 80);
        var btnDuyet = Ui.Btn("Duyệt", 90); var btnMoi = Ui.Btn("Làm mới", 90);
        btns.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnDuyet, btnMoi });

        Controls.Add(grid); Controls.Add(hint); Controls.Add(btns); Controls.Add(form); Controls.Add(top);

        Load += (_, _) => { LoadPhong(); LoadData(); };
        grid.SelectionChanged += (_, _) => ShowSelected();
        btnTim.Click += (_, _) => LoadData();
        txtTim.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { LoadData(); e.SuppressKeyPress = true; } };
        btnMoi.Click += (_, _) => { txtTim.Clear(); LoadPhong(); LoadData(); ClearInputs(); };
        btnThem.Click += (_, _) => Them();
        btnSua.Click += (_, _) => Sua();
        btnXoa.Click += (_, _) => Xoa();
        btnDuyet.Click += (_, _) => Duyet();
    }

    KhachThue? Current() => grid.CurrentRow?.DataBoundItem as KhachThue;

    void LoadPhong()
    {
        try { cboPhong.DataSource = PhongDAL.GetForChon(Session.ChuTroId); }
        catch (Exception ex) { Ui.Loi(ex); }
    }

    void SelectPhong(string? ma)
    {
        foreach (Phong p in cboPhong.Items)
            if (p.Id != 0 && p.MaPhong == ma) { cboPhong.SelectedItem = p; return; }
        if (cboPhong.Items.Count > 0) cboPhong.SelectedIndex = 0;
    }

    void LoadData()
    {
        try { grid.DataSource = KhachDAL.GetAll(Session.ChuTroId, txtTim.Text); grid.CurrentCell = null; }
        catch (Exception ex) { Ui.Loi(ex); }
    }

    void ShowSelected()
    {
        if (Current() is not KhachThue k) { ClearInputs(); return; }
        txtHoTen.Text = k.HoTen; txtCccd.Text = k.SoCCCD; txtSdt.Text = k.SDT; txtQue.Text = k.QueQuan ?? "";
        if (k.NgaySinh.HasValue) { dtpNgay.Value = k.NgaySinh.Value; dtpNgay.Checked = true; } else dtpNgay.Checked = false;
        SelectPhong(k.MaPhongDangKy);
    }

    void ClearInputs()
    {
        txtHoTen.Clear(); txtCccd.Clear(); txtSdt.Clear(); txtQue.Clear(); txtUser.Clear(); txtPass.Clear();
        dtpNgay.Checked = false;
        if (cboPhong.Items.Count > 0) cboPhong.SelectedIndex = 0;
    }

    bool TryRead(out KhachThue k, int id = 0)
    {
        k = new KhachThue { Id = id };
        string ten = txtHoTen.Text.Trim(), cccd = txtCccd.Text.Trim(), sdt = txtSdt.Text.Trim();
        if (ten.Length == 0) { Ui.Canh("Họ tên không được để trống."); return false; }
        if (!Validator.Cccd(cccd)) { Ui.Canh("CCCD phải gồm đúng 12 chữ số."); return false; }
        if (!Validator.Sdt(sdt)) { Ui.Canh("Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0."); return false; }
        k.HoTen = ten; k.SoCCCD = cccd; k.SDT = sdt;
        k.QueQuan = string.IsNullOrWhiteSpace(txtQue.Text) ? null : txtQue.Text.Trim();
        k.NgaySinh = dtpNgay.Checked ? dtpNgay.Value.Date : null;
        k.MaPhongDangKy = cboPhong.SelectedItem is Phong p && p.Id != 0 ? p.MaPhong : null;
        return true;
    }

    void Them()
    {
        if (!TryRead(out var k)) return;
        string user = txtUser.Text.Trim(); if (user.Length == 0) user = k.SDT;
        string pass = txtPass.Text; if (pass.Length == 0) pass = MatKhauMacDinh;
        if (!Validator.Username(user)) { Ui.Canh("Tên đăng nhập 4-50 ký tự, chỉ gồm chữ, số, dấu _ hoặc dấu chấm."); return; }
        if (pass.Length < 6) { Ui.Canh("Mật khẩu phải có ít nhất 6 ký tự."); return; }
        k.ChuTroId = Session.ChuTroId; k.TrangThai = "HoatDong";
        try
        {
            string salt = PasswordHasher.NewSalt();
            KhachDAL.Insert(k, user, PasswordHasher.Hash(pass, salt), salt);
            Ui.Thong($"Đã thêm khách thuê.\nTài khoản: {user}\nMật khẩu: {pass}");
            LoadData(); ClearInputs();
        }
        catch (Exception ex) { Ui.Loi(ex); }
    }

    void Sua()
    {
        if (Current() is not KhachThue cur) { Ui.Canh("Hãy chọn một khách thuê trong danh sách."); return; }
        if (!TryRead(out var k, cur.Id)) return;
        try { KhachDAL.Update(k, Session.ChuTroId); LoadData(); }
        catch (Exception ex) { Ui.Loi(ex); }
    }

    void Xoa()
    {
        if (Current() is not KhachThue cur) { Ui.Canh("Hãy chọn một khách thuê trong danh sách."); return; }
        if (!Ui.Hoi($"Xóa khách thuê {cur.HoTen}? Tài khoản đăng nhập của khách cũng bị xóa.")) return;
        try { KhachDAL.Delete(cur.Id, Session.ChuTroId); LoadData(); ClearInputs(); }
        catch (Exception ex) { Ui.Loi(ex); }
    }

    void Duyet()
    {
        if (Current() is not KhachThue cur) { Ui.Canh("Hãy chọn một khách thuê trong danh sách."); return; }
        if (cur.TrangThai != "ChoDuyet") { Ui.Thong("Tài khoản này đã được kích hoạt."); return; }
        try { KhachDAL.Duyet(cur.Id, Session.ChuTroId); LoadData(); Ui.Thong($"Đã kích hoạt tài khoản của {cur.HoTen}."); }
        catch (Exception ex) { Ui.Loi(ex); }
    }
}
