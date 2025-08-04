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

        public Form1()
        {
            InitializeComponent();
            logoutButton.Enabled = false;
            emailLabel.Text = "Not logged in";
            UpdateActionButtons();
            LoadLocalSaves();
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

                statusLabel.Text = "Login successful!";
                _loggedInUserEmail = userInfo.Email;
                emailLabel.Text = _loggedInUserEmail;
                loginButton.Enabled = false;
                logoutButton.Enabled = true;

                _syncData = SyncDataManager.LoadSyncData();
                if (!_syncData.Users.TryGetValue(_loggedInUserEmail, out _currentUserSyncData))
                {
                    _currentUserSyncData = new UserSyncData();
                    _syncData.Users[_loggedInUserEmail] = _currentUserSyncData;
                }

                await LoadCloudSavesAsync();
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
            try
            {
                var saveFolders = KenshiSaveHelper.ListSaveGames();
                MessageBox.Show($"Found {saveFolders.Count} local save folders.");
                if (saveFolders.Count == 0)
                {
                    localSavesListView.Items.Add(new ListViewItem("No Kenshi save folders found."));
                }
                else
                {
                    foreach (var folder in saveFolders)
                    {
                        string saveName = folder.Item1;
                        DateTime lastModified = folder.Item2;
                        string syncStatus = "";

                        _currentUserSyncData.LocalSaves.TryGetValue(saveName, out var localSaveInfo);
                        _currentUserSyncData.CloudSaves.TryGetValue(saveName, out var cloudSaveInfo);

                        if (localSaveInfo == null)
                        {
                            localSaveInfo = new LocalSaveInfo { LastModified = lastModified };
                            _currentUserSyncData.LocalSaves[saveName] = localSaveInfo;
                        }
                        else
                        {
                            localSaveInfo.LastModified = lastModified;
                        }

                        if (cloudSaveInfo != null)
                        {
                            if (localSaveInfo.LastSyncedToCloud.HasValue && localSaveInfo.LastSyncedToCloud.Value == lastModified)
                            {
                                syncStatus = "Synced";
                            }
                            else if (cloudSaveInfo.LastModified > lastModified)
                            {
                                syncStatus = "Cloud Newer";
                            }
                            else if (cloudSaveInfo.LastModified < lastModified)
                            {
                                syncStatus = "Local Newer";
                            }
                            else
                            {
                                syncStatus = "Synced"; // Dates match, consider it synced
                            }
                        }
                        else
                        {
                            syncStatus = "Not Synced";
                        }

                        ListViewItem item = new ListViewItem(saveName);
                        item.SubItems.Add(lastModified.ToString("yyyy-MM-dd HH:mm"));
                        item.SubItems.Add(syncStatus);
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
            if (_driveService == null) return;

            SetProgressUI(true, "Loading cloud saves...");
            try
            {
                var cloudSavesData = await GoogleDriveHelper.ListSavedGamesAsync(_driveService);
                _cloudSaves = cloudSavesData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Item1);

                MessageBox.Show($"Found {cloudSavesData.Count} cloud saves.");

                if (cloudSavesData.Count == 0)
                {
                    cloudSavesListView.Items.Add(new ListViewItem("No cloud saves found."));
                }
                else
                {
                    foreach (var save in cloudSavesData)
                    {
                        string saveName = save.Key;
                        DateTimeOffset lastModified = save.Value.Item2;
                        string syncStatus = "";

                        _currentUserSyncData.CloudSaves.TryGetValue(saveName, out var cloudSaveInfo);
                        _currentUserSyncData.LocalSaves.TryGetValue(saveName, out var localSaveInfo);

                        if (cloudSaveInfo == null)
                        {
                            cloudSaveInfo = new CloudSaveInfo { LastModified = lastModified.LocalDateTime };
                            _currentUserSyncData.CloudSaves[saveName] = cloudSaveInfo;
                        }
                        else
                        {
                            cloudSaveInfo.LastModified = lastModified.LocalDateTime;
                        }

                        if (localSaveInfo != null)
                        {
                            if (cloudSaveInfo.LastSyncedToLocal.HasValue && cloudSaveInfo.LastSyncedToLocal.Value == lastModified.LocalDateTime)
                            {
                                syncStatus = "Synced";
                            }
                            else if (localSaveInfo.LastModified > lastModified.LocalDateTime)
                            {
                                syncStatus = "Local Newer";
                            }
                            else if (localSaveInfo.LastModified < lastModified.LocalDateTime)
                            {
                                syncStatus = "Cloud Newer";
                            }
                            else
                            {
                                syncStatus = "Synced"; // Dates match, consider it synced
                            }
                        }
                        else
                        {
                            syncStatus = "Not Synced";
                        }

                        ListViewItem item = new ListViewItem(saveName);
                        item.SubItems.Add(lastModified.LocalDateTime.ToString("yyyy-MM-dd HH:mm"));
                        item.SubItems.Add(syncStatus);
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

            uploadButton.Visible = isLoggedIn && localSelectedItem != null && !_cloudSaves.ContainsKey(localSelectedItem.Text);
            syncToCloudButton.Visible = isLoggedIn && localSelectedItem != null && _cloudSaves.ContainsKey(localSelectedItem.Text);

            downloadButton.Visible = isLoggedIn && cloudSelectedItem != null && !localSavesListView.Items.Cast<ListViewItem>().Any(item => item.Text == cloudSelectedItem.Text);
            syncToLocalButton.Visible = isLoggedIn && cloudSelectedItem != null && localSavesListView.Items.Cast<ListViewItem>().Any(item => item.Text == cloudSelectedItem.Text);
            deleteCloudSaveButton.Visible = isLoggedIn && cloudSelectedItem != null;
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
                await GoogleDriveHelper.UploadSaveAsync(_driveService, zipPath);
                File.Delete(zipPath); // Clean up temp file
                statusLabel.Text = "Upload successful!";

                // Update sync data
                _currentUserSyncData.LocalSaves[saveName].LastSyncedToCloud = DateTime.Now;
                _currentUserSyncData.CloudSaves[saveName] = new CloudSaveInfo { LastModified = DateTime.Now };
                SyncDataManager.SaveSyncData(_syncData);

                LoadLocalSaves();
                await LoadCloudSavesAsync();
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
                await GoogleDriveHelper.UploadSaveAsync(_driveService, zipPath);
                File.Delete(zipPath);
                statusLabel.Text = "Sync successful!";

                // Update sync data
                _currentUserSyncData.LocalSaves[saveName].LastSyncedToCloud = DateTime.Now;
                _currentUserSyncData.CloudSaves[saveName] = new CloudSaveInfo { LastModified = DateTime.Now };
                SyncDataManager.SaveSyncData(_syncData);

                LoadLocalSaves();
                await LoadCloudSavesAsync();
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
                await GoogleDriveHelper.DownloadSaveAsync(_driveService, fileId, tempZipPath);
                statusLabel.Text = "Extracting save...";
                ZipHelper.UnzipSaveToLocal(tempZipPath, saveName);
                File.Delete(tempZipPath);
                statusLabel.Text = "Download successful!";

                // Update sync data
                _currentUserSyncData.CloudSaves[saveName].LastSyncedToLocal = DateTime.Now;
                _currentUserSyncData.LocalSaves[saveName] = new LocalSaveInfo { LastModified = DateTime.Now };
                SyncDataManager.SaveSyncData(_syncData);

                LoadLocalSaves();
                await LoadCloudSavesAsync();
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
                await GoogleDriveHelper.DownloadSaveAsync(_driveService, fileId, tempZipPath);
                statusLabel.Text = "Extracting and overwriting local save...";
                ZipHelper.UnzipSaveToLocal(tempZipPath, saveName);
                File.Delete(tempZipPath);
                statusLabel.Text = "Sync successful!";

                // Update sync data
                _currentUserSyncData.CloudSaves[saveName].LastSyncedToLocal = DateTime.Now;
                _currentUserSyncData.LocalSaves[saveName] = new LocalSaveInfo { LastModified = DateTime.Now };
                SyncDataManager.SaveSyncData(_syncData);

                LoadLocalSaves();
                await LoadCloudSavesAsync();
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
                _currentUserSyncData.CloudSaves.Remove(saveName);
                if (_currentUserSyncData.LocalSaves.ContainsKey(saveName))
                {
                    _currentUserSyncData.LocalSaves[saveName].LastSyncedToCloud = null;
                }
                SyncDataManager.SaveSyncData(_syncData);

                LoadLocalSaves();
                await LoadCloudSavesAsync();
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
