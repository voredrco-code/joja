var dbPath = args.Length > 0 ? args[0] : "Joja.Api/joja.db";

using var conn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}");
conn.Open();

// Ensure the migrations history table exists
using var createCmd = conn.CreateCommand();
createCmd.CommandText = @"
    CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
        ""MigrationId"" TEXT NOT NULL CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY,
        ""ProductVersion"" TEXT NOT NULL
    );";
createCmd.ExecuteNonQuery();

// Check if InitialCreate is already recorded
using var checkCmd = conn.CreateCommand();
checkCmd.CommandText = "SELECT COUNT(*) FROM \"__EFMigrationsHistory\" WHERE MigrationId = '20260221184847_InitialCreate';";
var count = (long)(checkCmd.ExecuteScalar() ?? 0L);

if (count == 0)
{
    using var insertCmd = conn.CreateCommand();
    insertCmd.CommandText = "INSERT INTO \"__EFMigrationsHistory\" (MigrationId, ProductVersion) VALUES ('20260221184847_InitialCreate', '8.0.0');";
    insertCmd.ExecuteNonQuery();
    Console.WriteLine("✓ Inserted InitialCreate into __EFMigrationsHistory");
}
else
{
    Console.WriteLine("✓ InitialCreate already recorded in __EFMigrationsHistory");
}

conn.Close();
Console.WriteLine("Done.");
