using System;
using Microsoft.Data.Sqlite;

class Program
{
    static void Main()
    {
        string dbPath = @"C:\Users\selko\.vscode\joja\Joja.Api\joja.db";
        string connStr = $"Data Source={dbPath}";

        Console.WriteLine($"=== QUERYING SQLITE DATABASE: {dbPath} ===");

        try
        {
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            Console.WriteLine("Connected successfully to SQLite database!\n");

            // Query Products
            Console.WriteLine("=== PRODUCTS ===");
            using (var cmd = new SqliteCommand("SELECT \"Id\", \"Name\", \"MainImageUrl\", \"VideoUrl\" FROM \"Products\"", conn))
            {
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine($"Product ID: {reader[0]}");
                    Console.WriteLine($"  Name: {reader[1]}");
                    Console.WriteLine($"  MainImageUrl: {reader[2]}");
                    Console.WriteLine($"  VideoUrl: {reader[3]}");
                    Console.WriteLine();
                }
            }

            // Query VideoBanners
            Console.WriteLine("=== VIDEO BANNERS ===");
            using (var cmd = new SqliteCommand("SELECT \"Id\", \"Title\", \"VideoUrl\" FROM \"VideoBanners\"", conn))
            {
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine($"Video Banner ID: {reader[0]}");
                    Console.WriteLine($"  Title: {reader[1]}");
                    Console.WriteLine($"  VideoUrl: {reader[2]}");
                    Console.WriteLine();
                }
            }

            // Query Banners
            Console.WriteLine("=== BANNERS ===");
            using (var cmd = new SqliteCommand("SELECT \"Id\", \"Title\", \"ImageUrl\" FROM \"Banners\"", conn))
            {
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine($"Banner ID: {reader[0]}");
                    Console.WriteLine($"  Title: {reader[1]}");
                    Console.WriteLine($"  ImageUrl: {reader[2]}");
                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error querying SQLite: {ex.Message}");
        }
    }
}
