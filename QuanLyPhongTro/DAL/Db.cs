using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyPhongTro.DAL;

/// <summary>Lớp kết nối SQL Server dùng chung (ADO.NET).</summary>
public static class Db
{
    // Server=MSI là tên máy của em (xem trong SSMS). Đổi nếu chạy ở máy khác.
    public static string ConnectionString =
        @"Server=MSI;Database=QuanLyPhongTro;Trusted_Connection=True;TrustServerCertificate=True;";

    public static SqlConnection GetConnection() => new(ConnectionString);

    public static SqlParameter P(string name, object? value) => new(name, value ?? DBNull.Value);

    public static DataTable Query(string sql, params SqlParameter[] ps)
    {
        using var conn = GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(ps);
        var dt = new DataTable();
        new SqlDataAdapter(cmd).Fill(dt);
        return dt;
    }

    public static int Execute(string sql, params SqlParameter[] ps)
    {
        using var conn = GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(ps);
        conn.Open();
        return cmd.ExecuteNonQuery();
    }

    public static object? Scalar(string sql, params SqlParameter[] ps)
    {
        using var conn = GetConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(ps);
        conn.Open();
        return cmd.ExecuteScalar();
    }

    // ---- Dùng trong transaction ----
    public static object? ScalarTx(SqlConnection c, SqlTransaction t, string sql, params SqlParameter[] ps)
    {
        using var cmd = new SqlCommand(sql, c, t);
        cmd.Parameters.AddRange(ps);
        return cmd.ExecuteScalar();
    }

    public static int ExecTx(SqlConnection c, SqlTransaction t, string sql, params SqlParameter[] ps)
    {
        using var cmd = new SqlCommand(sql, c, t);
        cmd.Parameters.AddRange(ps);
        return cmd.ExecuteNonQuery();
    }
}
