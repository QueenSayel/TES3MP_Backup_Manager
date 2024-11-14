using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Timers;

namespace TES3MP_Manager
{
    public partial class Main : Form
    {
        private System.Timers.Timer backupTimer;
        private DateTime nextBackupTime;
        private string sourcePath;
        private string backupPath;
        private int backupInterval;
        private CompressionLevel compressionLevel;
        private Rollback rollbackForm;

        public Main()
        {
            InitializeComponent();
            InitializeBackup();
            this.FormClosing += Main_FormClosing;
        }

        private void InitializeBackup()
        {
            // Load saved settings
            sourcePath = Properties.Settings.Default.SourcePath;
            backupPath = Properties.Settings.Default.BackupPath;
            backupInterval = Properties.Settings.Default.BackupInterval;

            // Update controls with loaded settings
            pathTextBox.Text = sourcePath;
            pathBackupTextBox.Text = backupPath;
            intervalNumeric.Value = backupInterval;

            // Set compression level based on saved setting
            string savedCompression = Properties.Settings.Default.CompressionLevel;
            int compressionIndex = compressionComboBox.Items.IndexOf(savedCompression);
            compressionComboBox.SelectedIndex = compressionIndex >= 0 ? compressionIndex : 0;
            int maxBackups = Properties.Settings.Default.MaxBackups;
            // Set other defaults
            stopBtn.Visible = false;
            timerLabel.Visible = false;
            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(backupPath))
            {
                logTextBox.Text = "Please set source and backup folders." + Environment.NewLine;
            }
            else
            {
                logTextBox.Clear(); // Clear any residual text if paths are valid
                LogMessage("Settings loaded successfully.");
            }
        }
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save settings
            Properties.Settings.Default.SourcePath = sourcePath;
            Properties.Settings.Default.BackupPath = backupPath;
            Properties.Settings.Default.BackupInterval = (int)intervalNumeric.Value;
            Properties.Settings.Default.CompressionLevel = compressionComboBox.SelectedItem.ToString();
            Properties.Settings.Default.Save();
        }
        private void pathBtn_Click(object sender, EventArgs e)
        {
            // Show folder dialog to select source folder
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    sourcePath = dialog.SelectedPath;
                    pathTextBox.Text = sourcePath;
                    LogMessage($"Source path set to: {sourcePath}");
                }
            }
        }

        private void setPathBackupBtn_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    backupPath = dialog.SelectedPath;
                    pathBackupTextBox.Text = backupPath;
                    LogMessage($"Backup path set to: {backupPath}");

                    // Refresh the rollback form if it is open
                    if (rollbackForm != null && !rollbackForm.IsDisposed)
                    {
                        rollbackForm.RefreshBackupList(backupPath); // Pass the updated path directly
                    }
                }
            }
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            // Check if paths are set
            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(backupPath))
            {
                MessageBox.Show("Please set both source and backup paths.");
                return;
            }

            // Set interval and compression level
            backupInterval = (int)intervalNumeric.Value;
            switch (compressionComboBox.SelectedItem.ToString())
            {
                case "Best":
                    compressionLevel = CompressionLevel.Optimal;
                    break;
                case "Fastest":
                    compressionLevel = CompressionLevel.Fastest;
                    break;
                case "None":
                    compressionLevel = CompressionLevel.NoCompression;
                    break;
            }
            timerLabel.Visible = true;
            intervalNumeric.Enabled = false;
            compressionComboBox.Enabled = false;
            pathBtn.Enabled = false;
            setPathBackupBtn.Enabled = false;
            startBtn.Enabled = false;
            stopBtn.Visible = true;

            // Initialize and start the timer
            if (backupTimer == null)
            {
                backupTimer = new System.Timers.Timer(1000); // Tick every second
                backupTimer.Elapsed += BackupTimer_Tick;
            }

            nextBackupTime = DateTime.Now.AddMinutes(backupInterval);
            backupTimer.Start();
            LogMessage($"Backup process started. Next backup in {backupInterval} minutes.");
        }
        private void stopBtn_Click(object sender, EventArgs e)
        {
            // Stop the timer if it's running
            if (backupTimer != null && backupTimer.Enabled)
            {
                backupTimer.Stop();
            }

            // Reset the controls to their initial state
            intervalNumeric.Enabled = true;
            compressionComboBox.Enabled = true;
            pathBtn.Enabled = true;
            setPathBackupBtn.Enabled = true;
            startBtn.Enabled = true;
            stopBtn.Visible = false;
            timerLabel.Visible = false;
            LogMessage("Backup process stopped by user.");
        }

        private void BackupTimer_Tick(object sender, ElapsedEventArgs e)
        {
            // Calculate remaining time
            var timeRemaining = nextBackupTime - DateTime.Now;

            if (timeRemaining.TotalSeconds > 0)
            {
                // Update timer label with time remaining
                Invoke((MethodInvoker)(() =>
                {
                    timerLabel.Text = $"Next backup in: {timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";
                }));
            }
            else
            {
                // Stop the timer while backup is in progress
                backupTimer.Stop();
                PerformBackup();

                // Set the next backup time and restart timer
                nextBackupTime = DateTime.Now.AddMinutes(backupInterval);
                backupTimer.Start();
            }
        }

        private void PerformBackup()
        {
            try
            {
                // Prepare the backup file path
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string zipFileName = $"tes3mp_{timestamp}.zip";
                string zipFilePath = Path.Combine(backupPath, zipFileName);

                // Log start of backup
                Invoke((MethodInvoker)(() =>
                {
                    timerLabel.Text = "Compressing files...";
                    LogMessage($"Starting backup...");
                }));

                // Perform zipping
                ZipFile.CreateFromDirectory(sourcePath, zipFilePath, compressionLevel, false);

                // After creating the new backup, enforce the backup limit
                EnforceBackupLimit();

                // Log completion of backup
                Invoke((MethodInvoker)(() =>
                {
                    LogMessage($"Backup completed: {zipFileName}");

                    // Refresh the Rollback form if it is open
                    if (rollbackForm != null && !rollbackForm.IsDisposed)
                    {
                        rollbackForm.RefreshBackupList(backupPath); // Update ListBox with the new file
                    }
                }));
            }
            catch (Exception ex)
            {
                // Log any errors during backup
                Invoke((MethodInvoker)(() =>
                {
                    LogMessage($"Error during backup: {ex.Message}");
                }));
            }
        }

        private void EnforceBackupLimit()
        {
            // Retrieve max backups setting directly from saved settings
            int maxBackups = Properties.Settings.Default.MaxBackups;
            string[] zipFiles = Directory.GetFiles(backupPath, "*.zip");

            if (zipFiles.Length > maxBackups)
            {
                // Order files by creation date to identify the oldest ones
                var filesToDelete = zipFiles
                    .OrderBy(File.GetCreationTime)
                    .Take(zipFiles.Length - maxBackups);

                foreach (var file in filesToDelete)
                {
                    File.Delete(file);

                    // Ensure LogMessage is called on the main thread
                    Invoke((MethodInvoker)(() =>
                    {
                        LogMessage($"Deleted old backup: {Path.GetFileName(file)}");
                    }));
                }
            }
        }

        public void LogMessage(string message)
        {
            string timestamp = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ";
            logTextBox.AppendText(timestamp + message + Environment.NewLine);
        }

        private void backupsBtn_Click(object sender, EventArgs e)
        {
            if (rollbackForm == null || rollbackForm.IsDisposed)
            {
                rollbackForm = new Rollback(this);
                rollbackForm.Owner = this;
                rollbackForm.LoadBackupFiles(backupPath);
                rollbackForm.Show();
            }
            else
            {
                rollbackForm.Close();
                rollbackForm = null;
            }
        }

        public void StopBackupTimer()
        {
            if (backupTimer != null && backupTimer.Enabled)
            {
                Invoke((MethodInvoker)(() =>
                {
                    backupTimer.Stop();
                    timerLabel.Text = "Performing rollback...";
                }));
            }
        }

        public void StartBackupTimer()
        {
            if (backupTimer != null)
            {
                Invoke((MethodInvoker)(() =>
                {
                    nextBackupTime = DateTime.Now.AddMinutes(backupInterval); // Reset the next backup time
                    backupTimer.Start();
                    timerLabel.Visible = true; // Make sure timer label is visible again
                }));
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }
    }
}