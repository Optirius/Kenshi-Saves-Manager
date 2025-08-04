using System;
using System.IO;
using System.Linq;

namespace KenshiSavesManager.Helpers
{
    public static class KenshiSaveHelper
    {
        public static string GetKenshiSaveFolderPath()
        {
            // This is the most reliable way to get the "AppData\Local" folder path.
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "kenshi", "save");
        }

        public static string[] ListSaveGames()
        {
            string savePath = GetKenshiSaveFolderPath();

            if (Directory.Exists(savePath))
            {
                // Return the names of the directories inside the 'save' folder.
                return Directory.GetDirectories(savePath)
                                .Select(Path.GetFileName)
                                .Where(name => name != null && !name.EndsWith("_bk"))
                                .ToArray()!;
            }

            // Return an empty array if the directory doesn't exist.
            return Array.Empty<string>();
        }
    }
}
