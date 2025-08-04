using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace KenshiSavesManager.Helpers
{
    public static class SyncDataManager
    {
        private static readonly string SyncDataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sync_data.json");

        public static SyncData LoadSyncData()
        {
            if (File.Exists(SyncDataFilePath))
            {
                string json = File.ReadAllText(SyncDataFilePath);
                return JsonConvert.DeserializeObject<SyncData>(json) ?? new SyncData();
            }
            return new SyncData();
        }

        public static void SaveSyncData(SyncData data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(SyncDataFilePath, json);
        }
    }

    public class SyncData
    {
        public Dictionary<string, UserSyncData> Users { get; set; } = new Dictionary<string, UserSyncData>();
    }

    public class UserSyncData
    {
        public Dictionary<string, SaveInfo> Saves { get; set; } = new Dictionary<string, SaveInfo>();
    }

    public class SaveInfo
    {
        public DateTime LastModified { get; set; }
        public DateTime? LastSynced { get; set; }
    }
}