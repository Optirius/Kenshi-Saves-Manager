using KenshiSavesManager.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace KenshiSavesManager
{
    public partial class Form1 : Form
    {
        private Google.Apis.Drive.v3.DriveService? _driveService;
        private Dictionary<string, string> _cloudSaves = new Dictionary<string, string>();

        public Form1()
        {
            InitializeComponent();
            logoutButton.Enabled = false;
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
                if (control != progressPanel)
                {
                    control.Enabled = !visible;
                }
            }
            // Ensure the progress panel itself is always enabled when visible
            progressPanel.Enabled = visible;
        }

        private async void loginButton_Click(object sender, System.EventArgs e)
        {
            SetProgressUI(true, "Logging in...");
            try
            {
                _driveService = await GoogleAuthHelper.LoginAsync();
                statusLabel.Text = "Login successful!";
                loginButton.Enabled = false;
                logoutButton.Enabled = true;
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
                loginButton.Enabled = true;
                logoutButton.Enabled = false;
                cloudSavesListView.Items.Clear();
                _cloudSaves.Clear();
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
                if (saveFolders.Length == 0)
                {
                    localSavesListView.Items.Add("No Kenshi save folders found.");
                }
                else
                {
                    foreach (var folder in saveFolders)
                    {
                        localSavesListView.Items.Add(folder);
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
                _cloudSaves = await GoogleDriveHelper.ListSavedGamesAsync(_driveService);
                if (_cloudSaves.Count == 0)
                {
                    cloudSavesListView.Items.Add("No cloud saves found.");
                }
                else
                {
                    foreach (var saveName in _cloudSaves.Keys)
                    {
                        cloudSavesListView.Items.Add(saveName);
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
            uploadButton.Visible = isLoggedIn && localSavesListView.SelectedItems.Count > 0 && !_cloudSaves.ContainsKey(localSavesListView.SelectedItems[0].Text);
            syncToCloudButton.Visible = isLoggedIn && localSavesListView.SelectedItems.Count > 0 && _cloudSaves.ContainsKey(localSavesListView.SelectedItems[0].Text);

            downloadButton.Visible = isLoggedIn && cloudSavesListView.SelectedItems.Count > 0 && !localSavesListView.Items.Cast<ListViewItem>().Any(item => item.Text == cloudSavesListView.SelectedItems[0].Text);
            syncToLocalButton.Visible = isLoggedIn && cloudSavesListView.SelectedItems.Count > 0 && localSavesListView.Items.Cast<ListViewItem>().Any(item => item.Text == cloudSavesListView.SelectedItems[0].Text);
            deleteCloudSaveButton.Visible = isLoggedIn && cloudSavesListView.SelectedItems.Count > 0;
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
                LoadLocalSaves();
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
                LoadLocalSaves();
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
