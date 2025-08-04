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

        public static List<Tuple<string, DateTime>> ListSaveGames()
        {
            string savePath = GetKenshiSaveFolderPath();
            var saveGames = new List<Tuple<string, DateTime>>();

            if (Directory.Exists(savePath))
            {
                foreach (var directory in Directory.GetDirectories(savePath))
                {
                    string folderName = Path.GetFileName(directory);
                    if (!folderName.EndsWith("_bk"))
                    {
                        DateTime lastModified = Directory.GetLastWriteTime(directory);
                        saveGames.Add(Tuple.Create(folderName, lastModified));
                    }
                }
            }
            return saveGames;
        }
    }
}
