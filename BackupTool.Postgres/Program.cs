using BackupTool.Postgres;
using BackupTool.Postgres.Settings;
using Microsoft.Extensions.Configuration;

var environment = "Production";
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

var config = builder.Build();

var postgresSettings = config.GetSection("PostgresSettings").Get<PostgresSettings>();
if(postgresSettings == null)
{
    Console.WriteLine("Erro ao obter dados do postgres do arquivo de configuração. ");
    return;
}

var backup = new Backup(postgresSettings);
string backupFolder = await backup.FazerBackupLocalAsync();

//await GoogleDrive.EnviarParaGoogleDriveAsync(postgresSettings);

Backup.LimparBackupsAntigos(backupFolder);



