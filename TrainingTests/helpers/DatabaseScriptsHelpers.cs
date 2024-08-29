using Microsoft.Data.Sqlite;

namespace TrainingTests.helpers;

public static class DatabaseScriptsHelpers
{
    public static void InitializeDatabaseFromScript(SqliteConnection connection, string scriptFilePath = null)
    {
        scriptFilePath ??= Path.Combine(AppContext.BaseDirectory, "../../../../database_script.sql");
        ExecuteScript(connection, scriptFilePath);
    }

    public static void SeedExercisesMusclesEquipmentAndRelations(SqliteConnection connection, string scriptFilePath = null)
    {
        scriptFilePath ??= Path.Combine(AppContext.BaseDirectory, "../../../../insertion_script.sql");
        ExecuteScript(connection, scriptFilePath, useTransaction: true);
    }

    private static void ExecuteScript(SqliteConnection connection, string scriptFilePath, bool useTransaction = false)
    {
        var script = File.ReadAllText(scriptFilePath);

        using var command = connection.CreateCommand();
        SqliteTransaction transaction = null;

        if (useTransaction)
        {
            transaction = connection.BeginTransaction();
            command.Transaction = transaction;
        }

        var commands = script.Split(new[] { ";\n", ";\r\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var cmdText in commands)
        {
            var trimmedCmd = cmdText.Trim();

            // Skip empty commands
            if (string.IsNullOrEmpty(trimmedCmd)) continue;

            try
            {
                command.CommandText = trimmedCmd;
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command: {trimmedCmd}");
                Console.WriteLine($"Exception: {ex.Message}");
                throw; // Re-throw the exception to ensure the test fails if there's a real problem
            }
        }

        transaction?.Commit();
    }
}
