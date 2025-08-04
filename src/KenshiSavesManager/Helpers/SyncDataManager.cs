using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System;

namespace KenshiSavesManager.Helpers
{
    public class SyncDataManager
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
        public Dictionary<string, LocalSaveInfo> LocalSaves { get; set; } = new Dictionary<string, LocalSaveInfo>();
        public Dictionary<string, CloudSaveInfo> CloudSaves { get; set; } = new Dictionary<string, CloudSaveInfo>();
    }

    public class LocalSaveInfo
    {
        public DateTime LastModified { get; set; }
        public DateTime? LastSyncedToCloud { get; set; }
    }

    public class CloudSaveInfo
    {
        public DateTime LastModified { get; set; }
        public DateTime? LastSyncedToLocal { get; set; }
    }
}