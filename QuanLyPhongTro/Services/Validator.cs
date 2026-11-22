using System.Text.RegularExpressions;

namespace QuanLyPhongTro.Services;

public static class Validator
{
    public static bool Sdt(string s) => Regex.IsMatch(s, @"^0\d{9}$");
    public static bool Cccd(string s) => Regex.IsMatch(s, @"^\d{12}$");
    public static bool Username(string s) => Regex.IsMatch(s, @"^[A-Za-z0-9_.]{4,50}$");
    public static bool MaPhong(string s) => Regex.IsMatch(s, @"^[A-Za-z0-9]{1,10}$");
}
