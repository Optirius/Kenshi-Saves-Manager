using System.Linq;
using System.Windows.Forms;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using KenshiSavesManager.Helpers;

namespace KenshiSavesManager
{
    public partial class Form1 : Form
    {
        private Google.Apis.Drive.v3.DriveService? _driveService;
        private Dictionary<string, string> _cloudSaves = new Dictionary<string, string>();
        private SyncData _syncData = new SyncData();
        private UserSyncData _currentUserSyncData = new UserSyncData();
        private string? _loggedInUserEmail;

        private Dictionary<string, (string FileId, DateTime ModifiedTime)> _cloudSavesData = new Dictionary<string, (string, DateTime)>();
        private Dictionary<string, DateTime> _localSavesData = new Dictionary<string, DateTime>();

        public Form1()
        {
            InitializeComponent();
            logoutButton.Enabled = false;
            emailLabel.Text = "Not logged in";
            UpdateActionButtons();
            LoadLocalSaves();
            UpdateAndDisplaySyncStatus();
            // Ensure progress panel is initially hidden
            progressPanel.Visible = false;
        }

        private void SetProgressUI(bool visible, string message = "")
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetProgressUI(visible, message)));
                return;
            }

            progressPanel.Visible = visible;
            statusLabel.Text = message;
            // Disable/enable main form controls based on progress visibility
            foreach (Control control in Controls)
            {
                if (control != progressPanel && control != emailLabel)
                {
                    control.Enabled = !visible;
                }
            }
            emailLabel.BringToFront();
            // Ensure the progress panel itself is always enabled when visible
            progressPanel.Enabled = visible;
        }

        private async void loginButton_Click(object sender, System.EventArgs e)
        {
            SetProgressUI(true, "Logging in...");
            try
            {
                var credential = await GoogleAuthHelper.LoginAsync();
                _driveService = new Google.Apis.Drive.v3.DriveService(new Google.Apis.Services.BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Kenshi Saves Manager",
                });

                var oauthService = new Oauth2Service(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Kenshi Saves Manager",
                });
                var userInfo = await oauthService.Userinfo.Get().ExecuteAsync();
                var userEmail = userInfo.Email;

                if (string.IsNullOrEmpty(userEmail))
                {
                    MessageBox.Show("Could not retrieve user email. Please try again.");
                    return;
                }

                _loggedInUserEmail = userEmail;
                emailLabel.Text = _loggedInUserEmail;
                loginButton.Enabled = false;
                logoutButton.Enabled = true;

                _syncData = SyncDataManager.LoadSyncData();
                #pragma warning disable CS8601 // Possible null reference assignment.
                if (!_syncData.Users.TryGetValue(_loggedInUserEmail, out _currentUserSyncData))
#pragma warning restore CS8601 // Possible null reference assignment.
                {
                    _currentUserSyncData = new UserSyncData();
                    _syncData.Users[_loggedInUserEmail] = _currentUserSyncData;
                }

                LoadLocalSaves();
                await LoadCloudSavesAsync();
                UpdateAndDisplaySyncStatus();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"An error occurred during login: {ex.Message}");
            }
            finally
            {
                SetProgressUI(false);
                UpdateActionButtons();
            }
        }

        private void logoutButton_Click(object sender, System.EventArgs e)
        {
            SetProgressUI(true, "Logging out...");
            try
            {
                GoogleAuthHelper.Logout();
                _driveService = null;
                statusLabel.Text = "You have been logged out.";
                emailLabel.Text = "Not logged in";
                loginButton.Enabled = true;
                logoutButton.Enabled = false;
                cloudSavesListView.Items.Clear();
                _cloudSaves.Clear();
                _loggedInUserEmail = null;
                _currentUserSyncData = new UserSyncData();
                SyncDataManager.SaveSyncData(_syncData);
                UpdateAndDisplaySyncStatus();
            }
            finally
            {
                SetProgressUI(false);
                UpdateActionButtons();
            }
        }

        private void LoadLocalSaves()
        {
            localSavesListView.Items.Clear();
            _localSavesData.Clear();
            try
            {
                var saveFolders = KenshiSaveHelper.ListSaveGames();
                _localSavesData = saveFolders.ToDictionary(f => f.Item1, f => f.Item2);

                if (saveFolders.Count == 0)
                {
                    localSavesListView.Items.Add(new ListViewItem("No Kenshi save folders found."));
                }
                else
                {
                    foreach (var folder in saveFolders)
                    {
                        ListViewItem item = new ListViewItem(folder.Item1);
                        item.SubItems.Add(folder.Item2.ToString("yyyy-MM-dd HH:mm"));
                        item.SubItems.Add(""); // Status will be updated by UpdateAndDisplaySyncStatus
                        localSavesListView.Items.Add(item);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading Kenshi saves: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadCloudSavesAsync()
        {
            cloudSavesListView.Items.Clear();
            _cloudSaves.Clear();
            _cloudSavesData.Clear();
            if (_driveService == null) return;

            SetProgressUI(true, "Loading cloud saves...");
            try
            {
                var cloudSaves = await GoogleDriveHelper.ListSavedGamesAsync(_driveService);
                _cloudSavesData = cloudSaves.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => (kvp.Value.Item1, kvp.Value.Item2.LocalDateTime)
                );
                _cloudSaves = _cloudSavesData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.FileId);

                if (_cloudSavesData.Count == 0)
                {
                    cloudSavesListView.Items.Add(new ListViewItem("No cloud saves found."));
                }
                else
                {
                    foreach (var save in _cloudSavesData)
                    {
                        ListViewItem item = new ListViewItem(save.Key);
                        item.SubItems.Add(save.Value.ModifiedTime.ToString("yyyy-MM-dd HH:mm"));
                        item.SubItems.Add(""); // Status will be updated by UpdateAndDisplaySyncStatus
                        cloudSavesListView.Items.Add(item);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading cloud saves: {ex.Message}");
            }
            finally
            {
                SetProgressUI(false);
                UpdateActionButtons();
            }
        }

        private void UpdateAndDisplaySyncStatus()
        {
            var allSaveNames = _localSavesData.Keys.Union(_cloudSavesData.Keys).ToList();

            foreach (var saveName in allSaveNames)
            {
                bool isLocal = _localSavesData.TryGetValue(saveName, out var localTime);
                bool isCloud = _cloudSavesData.TryGetValue(saveName, out var cloudSave);

                string status = "Not Synced";
                if (isLocal && isCloud && localTime == cloudSave.ModifiedTime)
                {
                    status = "Synced";
                }

                UpdateListViewItemStatus(localSavesListView, saveName, status);
                UpdateListViewItemStatus(cloudSavesListView, saveName, status);
            }
        }

        private void UpdateListViewItemStatus(ListView listView, string saveName, string status)
        {
            foreach (ListViewItem item in listView.Items)
            {
                if (item.Text == saveName)
                {
                    if (item.SubItems.Count > 2)
                    {
                        item.SubItems[2].Text = status;
                    }
                    else
                    {
                        item.SubItems.Add(status);
                    }
                    break;
                }
            }
        }

        private void localSavesListView_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            cloudSavesListView.SelectedItems.Clear();
            UpdateActionButtons();
        }

        private void cloudSavesListView_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            localSavesListView.SelectedItems.Clear();
            UpdateActionButtons();
        }

        private void UpdateActionButtons()
        {
            bool isLoggedIn = _driveService != null;

            ListViewItem? localSelectedItem = localSavesListView.SelectedItems.Count > 0 ? localSavesListView.SelectedItems[0] : null;
            ListViewItem? cloudSelectedItem = cloudSavesListView.SelectedItems.Count > 0 ? cloudSavesListView.SelectedItems[0] : null;

            // Default all to hidden
            uploadButton.Visible = false;
            syncToCloudButton.Visible = false;
            downloadButton.Visible = false;
            syncToLocalButton.Visible = false;
            deleteCloudSaveButton.Visible = false;

            if (isLoggedIn)
            {
                if (localSelectedItem != null)
                {
                    bool existsInCloud = _cloudSaves.ContainsKey(localSelectedItem.Text);
                    if (existsInCloud)
                    {
                        syncToCloudButton.Visible = true;
                    }
                    else
                    {
                        uploadButton.Visible = true;
                    }
                }
                else if (cloudSelectedItem != null)
                {
                    bool existsLocally = localSavesListView.Items.Cast<ListViewItem>().Any(item => item.Text == cloudSelectedItem.Text);
                    deleteCloudSaveButton.Visible = true;
                    if (existsLocally)
                    {
                        syncToLocalButton.Visible = true;
                    }
                    else
                    {
                        downloadButton.Visible = true;
                    }
                }
            }
        }

        private async void uploadButton_Click(object sender, System.EventArgs e)
        {
            if (_driveService == null || localSavesListView.SelectedItems.Count == 0) return;
            string saveName = localSavesListView.SelectedItems[0].Text;
            SetProgressUI(true, $"Uploading '{saveName}'...");
            try
            {
                statusLabel.Text = "Zipping save folder...";
                string zipPath = ZipHelper.ZipSaveFolder(saveName);
                statusLabel.Text = "Uploading to Google Drive...";
                var modifiedTime = await GoogleDriveHelper.UploadSaveAsync(_driveService, zipPath);
                File.Delete(zipPath); // Clean up temp file
                statusLabel.Text = "Upload successful!";

                // Update sync data
                _currentUserSyncData.Saves[saveName] = new SaveInfo { LastModified = modifiedTime.LocalDateTime, LastSynced = modifiedTime.LocalDateTime };
                SyncDataManager.SaveSyncData(_syncData);

                // Update local save time to match cloud save time
                string kenshiSavePath = KenshiSaveHelper.GetKenshiSaveFolderPath();
                string sourceDirectory = Path.Combine(kenshiSavePath, saveName);
                Directory.SetLastWriteTime(sourceDirectory, modifiedTime.LocalDateTime);

                LoadLocalSaves();
                await LoadCloudSavesAsync();
                UpdateAndDisplaySyncStatus();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"An error occurred during upload: {ex.Message}");
            }
            finally
            {
                SetProgressUI(false);
                UpdateActionButtons();
            }
        }

        private async void syncToCloudButton_Click(object sender, System.EventArgs e)
        {
            if (_driveService == null || localSavesListView.SelectedItems.Count == 0) return;
            string saveName = localSavesListView.SelectedItems[0].Text;
            SetProgressUI(true, $"Syncing '{saveName}' to cloud...");
            try
            {
                statusLabel.Text = "Deleting old cloud save...";
                string fileId = _cloudSaves[saveName];
                await GoogleDriveHelper.DeleteSaveAsync(_driveService, fileId);
                statusLabel.Text = "Zipping new save folder...";
                string zipPath = ZipHelper.ZipSaveFolder(saveName);
                statusLabel.Text = "Uploading to Google Drive...";
                var modifiedTime = await GoogleDriveHelper.UploadSaveAsync(_driveService, zipPath);
                File.Delete(zipPath);
                statusLabel.Text = "Sync successful!";

                // Update sync data
                _currentUserSyncData.Saves[saveName] = new SaveInfo { LastModified = modifiedTime.LocalDateTime, LastSynced = modifiedTime.LocalDateTime };
                SyncDataManager.SaveSyncData(_syncData);

                // Update local save time to match cloud save time
                string kenshiSavePath = KenshiSaveHelper.GetKenshiSaveFolderPath();
                string sourceDirectory = Path.Combine(kenshiSavePath, saveName);
                Directory.SetLastWriteTime(sourceDirectory, modifiedTime.LocalDateTime);

                LoadLocalSaves();
                await LoadCloudSavesAsync();
                UpdateAndDisplaySyncStatus();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"An error occurred during sync: {ex.Message}");
            }
            finally
            {
                SetProgressUI(false);
                UpdateActionButtons();
            }
        }

        private async void downloadButton_Click(object sender, System.EventArgs e)
        {
            if (_driveService == null || cloudSavesListView.SelectedItems.Count == 0) return;
            string saveName = cloudSavesListView.SelectedItems[0].Text;
            SetProgressUI(true, $"Downloading '{saveName}'...");
            try
            {
                string fileId = _cloudSaves[saveName];
                string tempZipPath = Path.Combine(Path.GetTempPath(), $"{saveName}.zip");
                statusLabel.Text = "Downloading from Google Drive...";
                var modifiedTime = await GoogleDriveHelper.DownloadSaveAsync(_driveService, fileId, tempZipPath);
                statusLabel.Text = "Extracting save...";
                ZipHelper.UnzipSaveToLocal(tempZipPath, saveName, modifiedTime.LocalDateTime);
                File.Delete(tempZipPath);
                statusLabel.Text = "Download successful!";

                // Update sync data
                _currentUserSyncData.Saves[saveName] = new SaveInfo { LastModified = modifiedTime.LocalDateTime, LastSynced = modifiedTime.LocalDateTime };
                SyncDataManager.SaveSyncData(_syncData);

                LoadLocalSaves();
                await LoadCloudSavesAsync();
                UpdateAndDisplaySyncStatus();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"An error occurred during download: {ex.Message}");
            }
            finally
            {
                SetProgressUI(false);
                UpdateActionButtons();
            }
        }

        private async void syncToLocalButton_Click(object sender, System.EventArgs e)
        {
            if (_driveService == null || cloudSavesListView.SelectedItems.Count == 0) return;
            string saveName = cloudSavesListView.SelectedItems[0].Text;
            SetProgressUI(true, $"Syncing '{saveName}' to local...");
            try
            {
                string fileId = _cloudSaves[saveName];
                string tempZipPath = Path.Combine(Path.GetTempPath(), $"{saveName}.zip");
                statusLabel.Text = "Downloading from Google Drive...";
                var modifiedTime = await GoogleDriveHelper.DownloadSaveAsync(_driveService, fileId, tempZipPath);
                statusLabel.Text = "Extracting and overwriting local save...";
                ZipHelper.UnzipSaveToLocal(tempZipPath, saveName, modifiedTime.LocalDateTime);
                File.Delete(tempZipPath);
                statusLabel.Text = "Sync successful!";

                // Update sync data
                _currentUserSyncData.Saves[saveName] = new SaveInfo { LastModified = modifiedTime.LocalDateTime, LastSynced = modifiedTime.LocalDateTime };
                SyncDataManager.SaveSyncData(_syncData);

                LoadLocalSaves();
                await LoadCloudSavesAsync();
                UpdateAndDisplaySyncStatus();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"An error occurred during sync: {ex.Message}");
            }
            finally
            {
                SetProgressUI(false);
                UpdateActionButtons();
            }
        }

        private async void deleteCloudSaveButton_Click(object sender, System.EventArgs e)
        {
            if (_driveService == null || cloudSavesListView.SelectedItems.Count == 0) return;
            string saveName = cloudSavesListView.SelectedItems[0].Text;
            SetProgressUI(true, $"Deleting '{saveName}' from cloud...");
            try
            {
                string fileId = _cloudSaves[saveName];
                await GoogleDriveHelper.DeleteSaveAsync(_driveService, fileId);
                statusLabel.Text = $"Cloud save '{saveName}' deleted successfully.";

                // Update sync data
                _currentUserSyncData.Saves.Remove(saveName);
                SyncDataManager.SaveSyncData(_syncData);

                LoadLocalSaves();
                await LoadCloudSavesAsync();
                UpdateAndDisplaySyncStatus();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"An error occurred during deletion: {ex.Message}");
            }
            finally
            {
                SetProgressUI(false);
                UpdateActionButtons();
            }
        }
    }
}
