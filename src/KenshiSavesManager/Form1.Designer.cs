namespace KenshiSavesManager;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
        loginButton = new Button();
        logoutButton = new Button();
        localSavesListView = new ListView();
        saveNameColumnLocal = new ColumnHeader();
        modifiedDateColumnLocal = new ColumnHeader();
        syncStatusColumnLocal = new ColumnHeader();
        cloudSavesListView = new ListView();
        saveNameColumnCloud = new ColumnHeader();
        modifiedDateColumnCloud = new ColumnHeader();
        syncStatusColumnCloud = new ColumnHeader();
        localSavesLabel = new Label();
        cloudSavesLabel = new Label();
        uploadButton = new Button();
        syncToCloudButton = new Button();
        downloadButton = new Button();
        syncToLocalButton = new Button();
        deleteCloudSaveButton = new Button();
        progressBar = new ProgressBar();
        statusLabel = new Label();
        progressPanel = new Panel();
        emailLabel = new Label();
        progressPanel.SuspendLayout();
        SuspendLayout();
        // 
        // loginButton
        // 
        loginButton.Location = new Point(12, 12);
        loginButton.Name = "loginButton";
        loginButton.Size = new Size(150, 23);
        loginButton.TabIndex = 0;
        loginButton.Text = "Login with Google";
        loginButton.UseVisualStyleBackColor = true;
        loginButton.Click += loginButton_Click;
        // 
        // logoutButton
        // 
        logoutButton.Location = new Point(168, 12);
        logoutButton.Name = "logoutButton";
        logoutButton.Size = new Size(150, 23);
        logoutButton.TabIndex = 1;
        logoutButton.Text = "Logout";
        logoutButton.UseVisualStyleBackColor = true;
        logoutButton.Click += logoutButton_Click;
        // 
        // localSavesListView
        // 
        localSavesListView.Columns.AddRange(new ColumnHeader[] { saveNameColumnLocal, modifiedDateColumnLocal, syncStatusColumnLocal });
        localSavesListView.FullRowSelect = true;
        localSavesListView.Location = new Point(12, 65);
        localSavesListView.Name = "localSavesListView";
        localSavesListView.Size = new Size(356, 373);
        localSavesListView.TabIndex = 2;
        localSavesListView.UseCompatibleStateImageBehavior = false;
        localSavesListView.View = View.Details;
        localSavesListView.SelectedIndexChanged += localSavesListView_SelectedIndexChanged;
        // 
        // saveNameColumnLocal
        // 
        saveNameColumnLocal.Text = "Save Name";
        saveNameColumnLocal.Width = 120;
        // 
        // modifiedDateColumnLocal
        // 
        modifiedDateColumnLocal.Text = "Modified Date";
        modifiedDateColumnLocal.Width = 130;
        // 
        // syncStatusColumnLocal
        // 
        syncStatusColumnLocal.Text = "Sync Status";
        syncStatusColumnLocal.Width = 100;
        // 
        // cloudSavesListView
        // 
        cloudSavesListView.Columns.AddRange(new ColumnHeader[] { saveNameColumnCloud, modifiedDateColumnCloud, syncStatusColumnCloud });
        cloudSavesListView.FullRowSelect = true;
        cloudSavesListView.Location = new Point(530, 65);
        cloudSavesListView.Name = "cloudSavesListView";
        cloudSavesListView.Size = new Size(354, 373);
        cloudSavesListView.TabIndex = 3;
        cloudSavesListView.UseCompatibleStateImageBehavior = false;
        cloudSavesListView.View = View.Details;
        cloudSavesListView.SelectedIndexChanged += cloudSavesListView_SelectedIndexChanged;
        // 
        // saveNameColumnCloud
        // 
        saveNameColumnCloud.Text = "Save Name";
        saveNameColumnCloud.Width = 120;
        // 
        // modifiedDateColumnCloud
        // 
        modifiedDateColumnCloud.Text = "Modified Date";
        modifiedDateColumnCloud.Width = 130;
        // 
        // syncStatusColumnCloud
        // 
        syncStatusColumnCloud.Text = "Sync Status";
        syncStatusColumnCloud.Width = 100;
        // 
        // localSavesLabel
        // 
        localSavesLabel.AutoSize = true;
        localSavesLabel.Location = new Point(12, 47);
        localSavesLabel.Name = "localSavesLabel";
        localSavesLabel.Size = new Size(67, 15);
        localSavesLabel.TabIndex = 4;
        localSavesLabel.Text = "Local Saves";
        // 
        // cloudSavesLabel
        // 
        cloudSavesLabel.AutoSize = true;
        cloudSavesLabel.Location = new Point(530, 47);
        cloudSavesLabel.Name = "cloudSavesLabel";
        cloudSavesLabel.Size = new Size(71, 15);
        cloudSavesLabel.TabIndex = 5;
        cloudSavesLabel.Text = "Cloud Saves";
        // 
        // uploadButton
        // 
        uploadButton.Location = new Point(374, 150);
        uploadButton.Name = "uploadButton";
        uploadButton.Size = new Size(150, 23);
        uploadButton.TabIndex = 6;
        uploadButton.Text = "Upload to Cloud";
        uploadButton.UseVisualStyleBackColor = true;
        uploadButton.Visible = false;
        uploadButton.Click += uploadButton_Click;
        // 
        // syncToCloudButton
        // 
        syncToCloudButton.Location = new Point(374, 179);
        syncToCloudButton.Name = "syncToCloudButton";
        syncToCloudButton.Size = new Size(150, 23);
        syncToCloudButton.TabIndex = 7;
        syncToCloudButton.Text = "Replace Cloud File";
        syncToCloudButton.UseVisualStyleBackColor = true;
        syncToCloudButton.Visible = false;
        syncToCloudButton.Click += syncToCloudButton_Click;
        // 
        // downloadButton
        // 
        downloadButton.Location = new Point(374, 208);
        downloadButton.Name = "downloadButton";
        downloadButton.Size = new Size(150, 23);
        downloadButton.TabIndex = 8;
        downloadButton.Text = "Download to Local";
        downloadButton.UseVisualStyleBackColor = true;
        downloadButton.Visible = false;
        downloadButton.Click += downloadButton_Click;
        // 
        // syncToLocalButton
        // 
        syncToLocalButton.Location = new Point(374, 237);
        syncToLocalButton.Name = "syncToLocalButton";
        syncToLocalButton.Size = new Size(150, 23);
        syncToLocalButton.TabIndex = 9;
        syncToLocalButton.Text = "Replace Local File";
        syncToLocalButton.UseVisualStyleBackColor = true;
        syncToLocalButton.Visible = false;
        syncToLocalButton.Click += syncToLocalButton_Click;
        // 
        // deleteCloudSaveButton
        // 
        deleteCloudSaveButton.Location = new Point(374, 266);
        deleteCloudSaveButton.Name = "deleteCloudSaveButton";
        deleteCloudSaveButton.Size = new Size(150, 23);
        deleteCloudSaveButton.TabIndex = 10;
        deleteCloudSaveButton.Text = "Delete from Cloud";
        deleteCloudSaveButton.UseVisualStyleBackColor = true;
        deleteCloudSaveButton.Visible = false;
        deleteCloudSaveButton.Click += deleteCloudSaveButton_Click;
        // 
        // progressBar
        // 
        progressBar.Location = new Point(240, 196);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(400, 23);
        progressBar.Style = ProgressBarStyle.Marquee;
        progressBar.TabIndex = 0;
        // 
        // statusLabel
        // 
        statusLabel.Location = new Point(240, 173);
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(400, 20);
        statusLabel.TabIndex = 1;
        statusLabel.Text = "Please wait...";
        statusLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // progressPanel
        // 
        progressPanel.Controls.Add(statusLabel);
        progressPanel.Controls.Add(progressBar);
        progressPanel.Location = new Point(1, 0);
        progressPanel.Name = "progressPanel";
        progressPanel.Size = new Size(895, 450);
        progressPanel.TabIndex = 11;
        progressPanel.Visible = false;
        // 
        // emailLabel
        // 
        emailLabel.AutoSize = true;
        emailLabel.Location = new Point(680, 16);
        emailLabel.Name = "emailLabel";
        emailLabel.Size = new Size(67, 15);
        emailLabel.TabIndex = 13;
        emailLabel.Text = "Email Label";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(895, 450);
        Controls.Add(progressPanel);
        Controls.Add(emailLabel);
        Controls.Add(deleteCloudSaveButton);
        Controls.Add(syncToLocalButton);
        Controls.Add(downloadButton);
        Controls.Add(syncToCloudButton);
        Controls.Add(uploadButton);
        Controls.Add(cloudSavesLabel);
        Controls.Add(localSavesLabel);
        Controls.Add(cloudSavesListView);
        Controls.Add(localSavesListView);
        Controls.Add(logoutButton);
        Controls.Add(loginButton);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        Icon = (Icon)resources.GetObject("$this.Icon");
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "Form1";
        Text = "Kenshi Saves Manager";
        progressPanel.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button loginButton;
    private System.Windows.Forms.Button logoutButton;
    private System.Windows.Forms.ListView localSavesListView;
    private System.Windows.Forms.ListView cloudSavesListView;
    private System.Windows.Forms.Label localSavesLabel;
    private System.Windows.Forms.Label cloudSavesLabel;
    private System.Windows.Forms.Button uploadButton;
    private System.Windows.Forms.Button syncToCloudButton;
    private System.Windows.Forms.Button downloadButton;
    private System.Windows.Forms.Button syncToLocalButton;
    private System.Windows.Forms.Button deleteCloudSaveButton;
    private ProgressBar progressBar;
    private System.Windows.Forms.Label statusLabel;
    private System.Windows.Forms.Panel progressPanel;
    private System.Windows.Forms.Label emailLabel;
    private ColumnHeader saveNameColumnLocal;
    private ColumnHeader modifiedDateColumnLocal;
    private ColumnHeader syncStatusColumnLocal;
    private ColumnHeader saveNameColumnCloud;
    private ColumnHeader modifiedDateColumnCloud;
    private ColumnHeader syncStatusColumnCloud;
}