using BackupTool.Postgres.Settings;
using System.Diagnostics;

namespace BackupTool.Postgres;


internal class Backup(PostgresSettings postgresBackupSettings)
{
    private readonly string _host = postgresBackupSettings.Host;
    private readonly string _username = postgresBackupSettings.Username;
    private readonly int _port = postgresBackupSettings.Port;
    private readonly string _password = postgresBackupSettings.Password;
    private readonly string _backupFolder = postgresBackupSettings.BackupFolder;

    public static void LimparBackupsAntigos(string backupFolder)
    {
        IEnumerable<FileInfo> files = Enumerable.Empty<FileInfo>();

        try
        {
            files = new DirectoryInfo(backupFolder)
                .GetFiles("server_*.sql")
                .OrderByDescending(f => f.CreationTime)
                .Skip(2);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro obter arquivos de backup antigos. {ex}.");
        }
        
        foreach (FileInfo file in files)
        {
            try
            {
                file.Delete();
                Console.WriteLine($"Backup antigos limpos com sucesso. {file.FullName}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao limpar backup antigos. {file.FullName}: {ex}.");
            }
        }
    }

    public async Task<string> FazerBackupLocalAsync()
    {      
        string fileName = $"server_{DateTime.Now:yyyyMMdd_HHmmss}.sql";

        string backupPath = Path.Combine(_backupFolder, fileName);
        Environment.SetEnvironmentVariable("PGPASSWORD", _password);

        Directory.CreateDirectory(_backupFolder);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "pg_dumpall",
                Arguments = $"--host {_host} --username {_username} --port {_port} --file \"{backupPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        string result = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"Erro ao fazer backup: {error}.");
            return string.Empty;
        }

        Console.WriteLine("Backup gerado com sucesso: " + backupPath);
        return _backupFolder;
    }
}
