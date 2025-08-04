using Google.Apis.Drive.v3;
using Google.Apis.Upload;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KenshiSavesManager.Helpers
{
    public static class GoogleDriveHelper
    {
        // Using the dedicated, hidden app data folder is the most secure approach.
        private const string AppDataFolder = "appDataFolder";

        // Lists the save game files from the app data folder.
        public static async Task<Dictionary<string, Tuple<string, DateTimeOffset>>> ListSavedGamesAsync(DriveService service)
        {
            var listRequest = service.Files.List();
            listRequest.Spaces = AppDataFolder;
            listRequest.Fields = "files(id, name, modifiedTime)";
            var files = await listRequest.ExecuteAsync();
            return files.Files.ToDictionary(f => Path.GetFileNameWithoutExtension(f.Name), f => Tuple.Create(f.Id, f.ModifiedTimeDateTimeOffset ?? DateTimeOffset.MinValue));
        }

        // Uploads a save file to the app data folder.
        public static async Task UploadSaveAsync(DriveService service, string zipFilePath)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(zipFilePath),
                // We specify the appDataFolder as the parent.
                Parents = new List<string> { AppDataFolder }
            };

            using (var stream = new FileStream(zipFilePath, FileMode.Open))
            {
                var request = service.Files.Create(fileMetadata, stream, "application/zip");
                request.Fields = "id";
                await request.UploadAsync();
            }
        }

        // Downloads a save file from the app data folder.
        public static async Task DownloadSaveAsync(DriveService service, string fileId, string destinationPath)
        {
            var request = service.Files.Get(fileId);
            using (var memoryStream = new MemoryStream())
            {
                await request.DownloadAsync(memoryStream);
                using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                {
                    memoryStream.WriteTo(fileStream);
                }
            }
        }

        // Deletes a save file from the app data folder.
        public static async Task DeleteSaveAsync(DriveService service, string fileId)
        {
            await service.Files.Delete(fileId).ExecuteAsync();
        }
    }
}