using QuanLyPhongTro.DAL;
using QuanLyPhongTro.Models;
using QuanLyPhongTro.Services;

namespace QuanLyPhongTro.Forms;

/// <summary>Thông tin nhà trọ (hiển thị ở màn hình Tìm phòng trọ).</summary>
public class FrmNhaTro : Form
{
    readonly TextBox txtTen = new(), txtDiaChi = new(), txtSdt = new();
    readonly TextBox txtMoTa = new() { Multiline = true, Height = 70 };
    readonly ComboBox cboTinh = new() { DropDownStyle = ComboBoxStyle.DropDown };
    readonly ComboBox cboHuyen = new() { DropDownStyle = ComboBoxStyle.DropDown };
    NhaTro? nhaTro;

    public FrmNhaTro()
    {
        Text = "Thông tin nhà trọ";
        Font = Ui.Base;

        var title = new Label { Text = "Thông tin nhà trọ", Dock = DockStyle.Top, Height = 48, Font = new Font("Segoe UI", 13, FontStyle.Bold), Padding = new Padding(12, 12, 0, 0) };
        var t = Ui.FormTable(4);
        Ui.Field(t, "Tên nhà trọ", txtTen, 0, 0);          Ui.Field(t, "Số điện thoại liên hệ", txtSdt, 2, 0);
        Ui.Field(t, "Tỉnh / Thành phố", cboTinh, 0, 1);    Ui.Field(t, "Quận / Huyện", cboHuyen, 2, 1);
        Ui.Field(t, "Địa chỉ (số nhà, đường)", txtDiaChi, 0, 2);
        Ui.Field(t, "Mô tả ngắn", txtMoTa, 0, 3);

        var note = new Label
        {
            Dock = DockStyle.Top, Height = 34, Padding = new Padding(12, 8, 0, 0), ForeColor = Color.Gray,
            Text = "Ảnh nhà trọ (tối đa 5 ảnh, mỗi ảnh dưới 2 MB): chức năng tải ảnh sẽ bổ sung sau."
        };
        var btns = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10, 6, 0, 0) };
        var btnLuu = Ui.Btn("Lưu"); var btnHuy = Ui.Btn("Hủy");
        btns.Controls.AddRange(new Control[] { btnLuu, btnHuy });

        Controls.Add(btns); Controls.Add(note); Controls.Add(t); Controls.Add(title);
        Load += (_, _) => Nap();
        btnHuy.Click += (_, _) => Nap();
        btnLuu.Click += (_, _) => Luu();
    }

    void Nap()
    {
        try
        {
            cboTinh.Items.Clear(); cboHuyen.Items.Clear();
            cboTinh.Items.AddRange(NhaTroDAL.GetTinh().ToArray());
            cboHuyen.Items.AddRange(NhaTroDAL.GetHuyen(null).ToArray());
            nhaTro = NhaTroDAL.GetCuaChu(Session.ChuTroId);
            if (nhaTro == null) { Ui.Canh("Chưa có thông tin nhà trọ."); return; }
            txtTen.Text = nhaTro.TenNhaTro ?? ""; txtSdt.Text = nhaTro.Sdt;
            cboTinh.Text = nhaTro.Tinh; cboHuyen.Text = nhaTro.Huyen;
            txtDiaChi.Text = nhaTro.DiaChiCuThe; txtMoTa.Text = nhaTro.MoTa ?? "";
        }
        catch (Exception ex) { Ui.Loi(ex); }
    }

    void Luu()
    {
        if (nhaTro == null) { Ui.Canh("Chưa có thông tin nhà trọ để cập nhật."); return; }
        if (txtTen.Text.Trim().Length == 0 || cboTinh.Text.Trim().Length == 0 ||
            cboHuyen.Text.Trim().Length == 0 || txtDiaChi.Text.Trim().Length == 0)
        { Ui.Canh("Vui lòng nhập đủ tên nhà trọ, tỉnh, huyện và địa chỉ."); return; }
        if (!Validator.Sdt(txtSdt.Text.Trim())) { Ui.Canh("Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0."); return; }

        nhaTro.TenNhaTro = txtTen.Text.Trim(); nhaTro.Sdt = txtSdt.Text.Trim();
        nhaTro.Tinh = cboTinh.Text.Trim(); nhaTro.Huyen = cboHuyen.Text.Trim();
        nhaTro.DiaChiCuThe = txtDiaChi.Text.Trim();
        nhaTro.MoTa = string.IsNullOrWhiteSpace(txtMoTa.Text) ? null : txtMoTa.Text.Trim();
        try { NhaTroDAL.Update(nhaTro, Session.ChuTroId); Ui.Thong("Đã lưu thông tin nhà trọ."); }
        catch (Exception ex) { Ui.Loi(ex); }
    }
}
