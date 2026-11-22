using System.Security.Cryptography;
using System.Text;

namespace QuanLyPhongTro.Services;

/// <summary>SHA-256 + salt (đúng thiết kế bảng TaiKhoan: MatKhauHash, Salt). Hash = SHA256(salt + matKhau) dạng hex.</summary>
public static class PasswordHasher
{
    public static string NewSalt() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

    public static string Hash(string matKhau, string salt) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(salt + matKhau)));

    public static bool Verify(string matKhau, string salt, string hash) =>
        string.Equals(Hash(matKhau, salt), hash.Trim(), StringComparison.OrdinalIgnoreCase);
}
