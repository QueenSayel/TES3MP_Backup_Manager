using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Timers;
using System.Diagnostics;

namespace TES3MP_Manager
{

    public partial class Main : Form
    {
        private System.Timers.Timer backupTimer;
        private DateTime nextBackupTime;
        private string sourcePath;
        private string backupPath;
        private string exePath;
        private int backupInterval;
        private CompressionLevel compressionLevel;
        private Rollback rollbackForm;
        private FileSystemWatcher commandWatcher;
        private System.Timers.Timer statusCheckTimer;
        private NotifyIcon trayIcon;
        public Main()
        {
            InitializeComponent();
            InitializeBackup();
            InitializeCommandWatcher();
            InitializeTrayIcon();
            this.FormClosing += Main_FormClosing;
        }
        private void InitializeTrayIcon()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Information,
                Visible = false,
                BalloonTipTitle = "TES3MP Manager",
                BalloonTipText = "TES3MP minimised.",
                Text = "TES3MP Manager",
            };
            trayIcon.DoubleClick += TrayIcon_DoubleClick;
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void minimiseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (minimiseCheckBox.Checked)
            {

                this.Resize += Main_ResizeToTray;
            }
            else
            {

                this.Resize -= Main_ResizeToTray;
            }
        }

        private void Main_ResizeToTray(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized && minimiseCheckBox.Checked)
            {

                this.Hide();
                trayIcon.Visible = true;
            }
            else if (this.WindowState == FormWindowState.Normal)
            {
                trayIcon.Visible = false;
            }
        }

        private void InitializeStatusMonitor()
        {
            statusCheckTimer = new System.Timers.Timer(1000);
            statusCheckTimer.Elapsed += (sender, e) => UpdateServerStatus();
            statusCheckTimer.Start();
        }

        private void InitializeCommandWatcher()
        {
            string sourcePath = Properties.Settings.Default.SourcePath;
            if (!string.IsNullOrEmpty(sourcePath) && Directory.Exists(sourcePath))
            {
                string commandFilePath = Path.Combine(sourcePath, "scripts", "custom", "backup_manager", "command.json");
                string commandDirectory = Path.GetDirectoryName(commandFilePath);

                if (Directory.Exists(commandDirectory))
                {
                    commandWatcher = new FileSystemWatcher
                    {
                        Path = commandDirectory,
                        Filter = "command.json",
                        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                        EnableRaisingEvents = true
                    };

                    commandWatcher.Changed += CommandFile_Changed;
                }
                else
                {
                    LogMessage("Command directory does not exist. Command watcher not initialized.");
                }
            }
            else
            {
                LogMessage("Source path not set. Command watcher not initialized.");
            }
        }

        private DateTime lastCommandProcessedTime = DateTime.MinValue;
        private readonly TimeSpan debounceTime = TimeSpan.FromSeconds(1);

        private void CommandFile_Changed(object sender, FileSystemEventArgs e)
        {
            System.Threading.Thread.Sleep(100);

            if (DateTime.Now - lastCommandProcessedTime < debounceTime)
            {
                return;
            }

            lastCommandProcessedTime = DateTime.Now;

            try
            {
                string commandFilePath = e.FullPath;
                if (File.Exists(commandFilePath))
                {
                    string jsonContent = File.ReadAllText(commandFilePath);

                    // Parse the JSON file
                    var command = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

                    if (command != null && command.ContainsKey("backup") && command.ContainsKey("option"))
                    {
                        string backupDate = command["backup"];
                        string option = command["option"];

                        Invoke((MethodInvoker)(() => PerformRollbackFromCommand(backupDate, option)));
                    }
                    else
                    {
                        Invoke((MethodInvoker)(() => LogMessage("Invalid or incomplete command.json format.")));
                    }
                }
            }
            catch (Exception ex)
            {
                Invoke((MethodInvoker)(() => LogMessage($"Error reading or processing command.json: {ex.Message}")));
            }
        }


        private void PerformRollbackFromCommand(string backupDate, string option)
        {
            bool rollbackFormCreated = false;

            try
            {
                string backupFileName = $"tes3mp_{backupDate.Replace("-", "").Replace(":", "").Replace(" ", "_")}.zip";
                string backupFilePath = Path.Combine(backupPath, backupFileName);

                if (!File.Exists(backupFilePath))
                {
                    Invoke((MethodInvoker)(() =>
                        MessageBox.Show($"Backup file not found: {backupFileName}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                    return;
                }

                if (rollbackForm == null || rollbackForm.IsDisposed)
                {
                    rollbackForm = new Rollback(this);
                    rollbackFormCreated = true;
                }

                rollbackForm.KillTes3mpServer();
                StopBackupTimer();
                string sourcePath = Properties.Settings.Default.SourcePath;

                if (option == "Everything")
                {
                    rollbackForm.ExtractSelectedFolders(backupFilePath, sourcePath, "data/cell");
                    rollbackForm.ExtractSelectedFolders(backupFilePath, sourcePath, "data/player");
                    rollbackForm.ExtractSelectedFolders(backupFilePath, sourcePath, "data/world");
                }
                else
                {
                    string targetSubfolder = $"data/{option.ToLower()}";
                    rollbackForm.ExtractSelectedFolders(backupFilePath, sourcePath, targetSubfolder);
                }

                LogMessage($"Rollback completed using backup: {backupFileName}, range: {option}");
            }
            catch (Exception ex)
            {
                LogMessage($"Error during rollback triggered by console: {ex.Message}");
            }
            finally
            {
                rollbackForm.RestartTes3mpServer();
                StartBackupTimer();
                LogMessage("Backup process resumed.");

                if (rollbackFormCreated && rollbackForm != null)
                {
                    rollbackForm.Dispose();
                    rollbackForm = null;
                }
            }
        }


        private void InitializeBackup()
        {
            sourcePath = Properties.Settings.Default.SourcePath;
            backupPath = Properties.Settings.Default.BackupPath;
            backupInterval = Properties.Settings.Default.BackupInterval;
            exePath = Properties.Settings.Default.ExePath;

            pathTextBox.Text = sourcePath;
            pathBackupTextBox.Text = backupPath;
            intervalNumeric.Value = backupInterval;
            exePathTextBox.Text = exePath;

            string savedCompression = Properties.Settings.Default.CompressionLevel;
            int compressionIndex = compressionComboBox.Items.IndexOf(savedCompression);
            compressionComboBox.SelectedIndex = compressionIndex >= 0 ? compressionIndex : 0;
            int maxBackups = Properties.Settings.Default.MaxBackups;
            stopBtn.Visible = false;
            timerLabel.Visible = false;
            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(backupPath))
            {
                logTextBox.Text = "Please set source and backup folders." + Environment.NewLine;
            }
            else
            {
                logTextBox.Clear();
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
            Properties.Settings.Default.Minimise = minimiseCheckBox.Checked;
            Properties.Settings.Default.Save();
        }
        private void pathBtn_Click(object sender, EventArgs e)
        {
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

                    if (rollbackForm != null && !rollbackForm.IsDisposed)
                    {
                        rollbackForm.RefreshBackupList(backupPath);
                    }
                }
            }
        }

        private void setExeBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "TES3MP Server Executable|tes3mp-server.exe";
                dialog.Title = "Select tes3mp-server.exe";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = dialog.FileName;
                    exePathTextBox.Text = selectedPath;

                    Properties.Settings.Default.ExePath = selectedPath;
                    Properties.Settings.Default.Save();

                    LogMessage($"Server executable path set to: {selectedPath}");
                }
            }
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(backupPath))
            {
                MessageBox.Show("Please set both source and backup paths.");
                return;
            }

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
            setExeBtn.Enabled = false;
            startBtn.Enabled = false;
            stopBtn.Visible = true;

            if (backupTimer == null)
            {
                backupTimer = new System.Timers.Timer(1000);
                backupTimer.Elapsed += BackupTimer_Tick;
            }

            nextBackupTime = DateTime.Now.AddMinutes(backupInterval);
            backupTimer.Start();
            LogMessage($"Backup process started. Next backup in {backupInterval} minutes.");
        }
        private void stopBtn_Click(object sender, EventArgs e)
        {

            if (backupTimer != null && backupTimer.Enabled)
            {
                backupTimer.Stop();
            }

            intervalNumeric.Enabled = true;
            compressionComboBox.Enabled = true;
            pathBtn.Enabled = true;
            setPathBackupBtn.Enabled = true;
            setExeBtn.Enabled = true;
            startBtn.Enabled = true;
            stopBtn.Visible = false;
            timerLabel.Visible = false;
            LogMessage("Backup process stopped by user.");
        }

        private void BackupTimer_Tick(object sender, ElapsedEventArgs e)
        {
            var timeRemaining = nextBackupTime - DateTime.Now;

            if (timeRemaining.TotalSeconds > 0)
            {
                Invoke((MethodInvoker)(() =>
                {
                    timerLabel.Text = $"Next backup in: {timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";
                }));
            }
            else
            {
                backupTimer.Stop();
                PerformBackup();

                nextBackupTime = DateTime.Now.AddMinutes(backupInterval);
                backupTimer.Start();
            }
        }

        private void PerformBackup()
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string zipFileName = $"tes3mp_{timestamp}.zip";
                string zipFilePath = Path.Combine(backupPath, zipFileName);

                Invoke((MethodInvoker)(() =>
                {
                    timerLabel.Text = "Compressing files...";
                    LogMessage($"Starting backup...");
                }));

                ZipFile.CreateFromDirectory(sourcePath, zipFilePath, compressionLevel, false);

                EnforceBackupLimit();

                Invoke((MethodInvoker)(() =>
                {
                    LogMessage($"Backup completed: {zipFileName}");

                    if (rollbackForm != null && !rollbackForm.IsDisposed)
                    {
                        rollbackForm.RefreshBackupList(backupPath);
                    }
                }));
            }
            catch (Exception ex)
            {
                Invoke((MethodInvoker)(() =>
                {
                    LogMessage($"Error during backup: {ex.Message}");
                }));
            }
        }

        private void EnforceBackupLimit()
        {
            int maxBackups = Properties.Settings.Default.MaxBackups;
            string[] zipFiles = Directory.GetFiles(backupPath, "*.zip");

            if (zipFiles.Length > maxBackups)
            {
                var filesToDelete = zipFiles
                    .OrderBy(File.GetCreationTime)
                    .Take(zipFiles.Length - maxBackups);

                foreach (var file in filesToDelete)
                {
                    File.Delete(file);

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
                    nextBackupTime = DateTime.Now.AddMinutes(backupInterval);
                    backupTimer.Start();
                    timerLabel.Visible = true;
                }));
            }
        }

        private bool IsServerRunning()
        {
            var processes = System.Diagnostics.Process.GetProcessesByName("tes3mp-server");
            return processes.Length > 0;
        }

        private void UpdateServerStatus()
        {
            if (serverStatusLabel.InvokeRequired)
            {
                serverStatusLabel.Invoke(new Action(UpdateServerStatus));
                return;
            }

            serverStatusLabel.Text = "Server Status: ";

            if (IsServerRunning())
            {
                statusLabel.Text = "Online";
                statusLabel.ForeColor = Color.Green;
                launchServerBtn.Enabled = false;

                // Show and enable the new buttons when the server is running
                shutdownBtn.Visible = true;
                restartBtn.Visible = true;
                shutdownBtn.Enabled = true;
                restartBtn.Enabled = true;
            }
            else
            {
                statusLabel.Text = "Offline";
                statusLabel.ForeColor = Color.Red;
                launchServerBtn.Enabled = true;

                // Hide the new buttons when the server is offline
                shutdownBtn.Visible = false;
                restartBtn.Visible = false;
                shutdownBtn.Enabled = false;
                restartBtn.Enabled = false;
            }
        }

        private void ShutdownBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var processes = Process.GetProcessesByName("tes3mp-server");
                foreach (var process in processes)
                {
                    process.Kill();
                    process.WaitForExit(); // Ensure the process is fully terminated
                }

                LogMessage("TES3MP server has been shut down.");
                UpdateServerStatus(); // Update UI to reflect the new server state
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error shutting down the server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RestartBtn_Click(object sender, EventArgs e)
        {
            try
            {
                // Shutdown the server
                var processes = Process.GetProcessesByName("tes3mp-server");
                foreach (var process in processes)
                {
                    process.Kill();
                    process.WaitForExit(); // Ensure the process is fully terminated
                }

                // Start the server
                if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                {
                    Process.Start(exePath);
                    LogMessage("TES3MP server has been restarted.");
                }
                else
                {
                    MessageBox.Show("TES3MP server executable path is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                UpdateServerStatus(); // Update UI to reflect the new server state
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restarting the server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void launchServerBtn_Click(object sender, EventArgs e)
        {
            if (!IsServerRunning())
            {
                try
                {
                    Process.Start(exePath);
                    UpdateServerStatus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error launching the server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            InitializeStatusMonitor();
            UpdateServerStatus();
            minimiseCheckBox.Checked = Properties.Settings.Default.Minimise;

            if (minimiseCheckBox.Checked)
            {
                this.Hide();
                trayIcon.Visible = true;
            }
        }
    }
}