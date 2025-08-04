using System.IO;
using System.IO.Compression;

namespace KenshiSavesManager.Helpers
{
    public static class ZipHelper
    {
        public static string ZipSaveFolder(string saveFolderName)
        {
            string kenshiSavePath = KenshiSaveHelper.GetKenshiSaveFolderPath();
            string sourceDirectory = Path.Combine(kenshiSavePath, saveFolderName);
            string tempZipPath = Path.Combine(Path.GetTempPath(), $"{saveFolderName}.zip");

            // Ensure the temp file doesn't already exist.
            if (File.Exists(tempZipPath))
            {
                File.Delete(tempZipPath);
            }

            ZipFile.CreateFromDirectory(sourceDirectory, tempZipPath);
            return tempZipPath;
        }

        public static void UnzipSaveToLocal(string zipFilePath, string saveFolderName, DateTime modifiedTime)
        {
            string kenshiSavePath = KenshiSaveHelper.GetKenshiSaveFolderPath();
            string destinationDirectory = Path.Combine(kenshiSavePath, saveFolderName);
            string backupDirectory = $"{destinationDirectory}_bk";

            // If the destination directory exists, back it up.
            if (Directory.Exists(destinationDirectory))
            {
                // If an old backup exists, delete it first.
                if (Directory.Exists(backupDirectory))
                {
                    Directory.Delete(backupDirectory, true);
                }
                // Rename the current directory to the backup directory.
                Directory.Move(destinationDirectory, backupDirectory);
            }

            // Extract the new save from the zip file.
            ZipFile.ExtractToDirectory(zipFilePath, destinationDirectory);

            // Set the modification date of the new folder to match the cloud save's date
            Directory.SetLastWriteTime(destinationDirectory, modifiedTime);
        }
    }
}
