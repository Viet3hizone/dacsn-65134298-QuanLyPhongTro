using System.Globalization;

namespace QuanLyPhongTro.Models;

public class NhaTro
{
    public int Id { get; set; }
    public string? TenNhaTro { get; set; }
    public string Tinh { get; set; } = "";
    public string Huyen { get; set; } = "";
    public string DiaChiCuThe { get; set; } = "";
    public string? MoTa { get; set; }
    public string Sdt { get; set; } = "";   // SĐT của chủ trọ
}

/// <summary>Một dòng kết quả ở màn hình Tìm phòng trọ.</summary>
public class KetQuaTim
{
    static readonly CultureInfo Vi = new("vi-VN");
    public int NhaTroId { get; set; }
    public string ChuTro { get; set; } = "";
    public string TenNhaTro { get; set; } = "";
    public string DiaChi { get; set; } = "";
    public string Sdt { get; set; } = "";
    public int PhongTrong { get; set; }
    public decimal? GiaMin { get; set; }
    public decimal? GiaMax { get; set; }

    public string GiaText => GiaMin == null ? "-" :
        GiaMin == GiaMax ? GiaMin.Value.ToString("N0", Vi) : $"{GiaMin.Value.ToString("N0", Vi)} - {GiaMax!.Value.ToString("N0", Vi)}";
    public string LienHe => Sdt.Length >= 10 ? $"{Sdt[..4]} xxx {Sdt[^3..]}" : Sdt;
}
