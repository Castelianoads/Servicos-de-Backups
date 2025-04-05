using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace BackupTool.Postgres;

internal static class GoogleDrive
{
    public static async Task EnviarParaGoogleDriveAsync(string backupFolder)
    {
        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = "SEU_CLIENT_ID",
                ClientSecret = "SEU_CLIENT_SECRET"
            },
            new[] { DriveService.Scope.Drive },
            "user",
            CancellationToken.None,
            new FileDataStore("DriveBackupAuth")
        );

        var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "PostgreSQL Backup App"
        });

        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = Path.GetFileName(backupFolder),
            Parents = new[] { "PASTA_ID_DO_GOOGLE_DRIVE" }
        };

        using var stream = new FileStream(backupFolder, FileMode.Open);
        var request = service.Files.Create(fileMetadata, stream, "application/sql");
        request.Fields = "id";
        await request.UploadAsync();

        Console.WriteLine("Backup enviado ao Google Drive.");
    }
}
