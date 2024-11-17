using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;

namespace TES3MP_Manager
{
    public partial class Rollback : Form
    {
        private const int Gap = 10;
        public Rollback(Form mainForm)
        {
            InitializeComponent();
            PositionToLeftOfMainForm(mainForm);
            backupsNumeric.Value = Properties.Settings.Default.MaxBackups;
            backupsNumeric.ValueChanged += BackupsNumeric_ValueChanged;
        }
        private void BackupsNumeric_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MaxBackups = (int)backupsNumeric.Value;
            Properties.Settings.Default.Save();
        }
        private void PositionToLeftOfMainForm(Form mainForm)
        {
            // Position the Rollback form to the left of the Main form, with a small gap
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(mainForm.Location.X - this.Width - Gap, mainForm.Location.Y);
        }
        public void LoadBackupFiles(string backupPath)
        {
            // Clear the ListBox items to prevent duplicates if reloaded
            backupsListBox.Items.Clear();
            List<string> prefixes = new List<string>(); // Store prefixes for the JSON file

            // Check if the backup path exists and load .zip files
            if (Directory.Exists(backupPath))
            {
                string[] zipFiles = Directory.GetFiles(backupPath, "*.zip");

                foreach (string zipFile in zipFiles)
                {
                    string fileName = Path.GetFileName(zipFile);

                    // Extract the timestamp from the filename and parse it as DateTime
                    string timestampString = fileName.Substring(7, 15); // Extract "20241112_233517"
                    if (DateTime.TryParseExact(timestampString, "yyyyMMdd_HHmmss",
                                               System.Globalization.CultureInfo.InvariantCulture,
                                               System.Globalization.DateTimeStyles.None,
                                               out DateTime dateTime))
                    {
                        // Format the date and time and add it as a prefix
                        string formattedDate = $"[{dateTime:yyyy-MM-dd HH:mm:ss}]";
                        prefixes.Add(formattedDate); // Add prefix to the list
                        backupsListBox.Items.Add($"{formattedDate} {fileName}");
                    }
                    else
                    {
                        // If parsing fails, just add the original filename
                        backupsListBox.Items.Add(fileName);
                    }
                }

                // Update the JSON file with the collected prefixes
                UpdateBackupJson(prefixes);
            }
            else
            {
                MessageBox.Show("Backup path not found. Please set a valid backup path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RefreshBackupList(string backupPath)
        {
            LoadBackupFiles(backupPath);
        }

        private void backupsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check if an item is selected
            if (backupsListBox.SelectedItem != null)
            {
                // Get the selected backup file name
                string selectedBackup = backupsListBox.SelectedItem.ToString();

                // This is where restoring functionality would go in the future
            }
        }

        private void rollbackBtn_Click(object sender, EventArgs e)
        {
            if (backupsListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a backup to roll back to.", "No Backup Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (optionsListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select an option for rollback (Everything, Cell, Player, World).", "No Option Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedItem = backupsListBox.SelectedItem.ToString();
            int startOfFileName = selectedItem.IndexOf("]") + 2; // Skip past the closing bracket and space
            string actualFileName = selectedItem.Substring(startOfFileName);
            string backupFilePath = Path.Combine(Properties.Settings.Default.BackupPath, actualFileName);
            string selectedOption = optionsListBox.SelectedItem.ToString();

            try
            {
                KillTes3mpServer();

                if (Owner is Main mainForm)
                {
                    mainForm.Invoke((MethodInvoker)(() =>
                    {
                        mainForm.StopBackupTimer();
                        mainForm.LogMessage("Performing rollback...");
                    }));
                }

                string sourcePath = Properties.Settings.Default.SourcePath;

                if (selectedOption == "Everything")
                {
                    // Perform selective rollback for Cell, Player, and World
                    ExtractSelectedFolders(backupFilePath, sourcePath, "data/cell");
                    ExtractSelectedFolders(backupFilePath, sourcePath, "data/player");
                    ExtractSelectedFolders(backupFilePath, sourcePath, "data/world");
                }
                else
                {
                    string targetSubfolder = $"data/{selectedOption.ToLower()}";
                    ExtractSelectedFolders(backupFilePath, sourcePath, targetSubfolder);
                }

                if (Owner is Main mainFormWithLogging)
                {
                    mainFormWithLogging.Invoke((MethodInvoker)(() =>
                    {
                        mainFormWithLogging.LogMessage($"Rollback completed using backup: {actualFileName}, range: {selectedOption}");
                    }));
                }
            }
            catch (Exception ex)
            {
                if (Owner is Main mainFormWithErrorLogging)
                {
                    mainFormWithErrorLogging.Invoke((MethodInvoker)(() =>
                    {
                        mainFormWithErrorLogging.LogMessage($"Error during rollback: {ex.Message}");
                    }));
                }
                MessageBox.Show($"Error during rollback: {ex.Message}", "Rollback Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                RestartTes3mpServer();

                if (Owner is Main mainFormToRestart)
                {
                    mainFormToRestart.Invoke((MethodInvoker)(() =>
                    {
                        mainFormToRestart.StartBackupTimer();
                        mainFormToRestart.LogMessage("Backup process resumed.");
                    }));
                }
            }
        }


        public void ExtractSelectedFolders(string zipFilePath, string destinationPath, string targetSubfolder)
        {
            string targetPath = Path.Combine(destinationPath, targetSubfolder);

            // Delete contents of the target subfolder if it exists
            if (Directory.Exists(targetPath))
            {
                DirectoryInfo di = new DirectoryInfo(targetPath);
                foreach (FileInfo file in di.GetFiles()) file.Delete();
                foreach (DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);
            }

            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // Check if entry is in the target folder or if everything should be extracted
                    if (string.IsNullOrEmpty(targetSubfolder) || entry.FullName.StartsWith(targetSubfolder + "/"))
                    {
                        string destinationFileName = Path.Combine(destinationPath, entry.FullName);

                        // Ensure directory exists for the entry
                        string directoryPath = Path.GetDirectoryName(destinationFileName);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        // Only extract files (ignore directories)
                        if (!string.IsNullOrEmpty(entry.Name))
                        {
                            entry.ExtractToFile(destinationFileName, overwrite: true);
                        }
                    }
                }
            }
        }

        public void KillTes3mpServer()
        {
            try
            {
                var processes = System.Diagnostics.Process.GetProcessesByName("tes3mp-server");
                foreach (var process in processes)
                {
                    process.Kill();
                    process.WaitForExit(); // Ensure the process has terminated
                }
                // Log the termination of the process
                if (Owner is Main mainForm)
                {
                    mainForm.Invoke((MethodInvoker)(() =>
                    {
                        mainForm.LogMessage("TES3MP server process terminated before rollback.");
                    }));
                }
            }
            catch (Exception ex)
            {
                // Log any errors during process termination
                if (Owner is Main mainForm)
                {
                    mainForm.Invoke((MethodInvoker)(() =>
                    {
                        mainForm.LogMessage($"Error terminating TES3MP server process: {ex.Message}");
                    }));
                }
            }
        }

        public void RestartTes3mpServer()
        {
            try
            {
                string exePath = Properties.Settings.Default.ExePath;
                if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
                {
                    throw new FileNotFoundException("TES3MP server executable path is invalid or missing.");
                }

                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    }
                };
                process.Start();

                // Log the restart of the process
                if (Owner is Main mainForm)
                {
                    mainForm.Invoke((MethodInvoker)(() =>
                    {
                        mainForm.LogMessage("TES3MP server process restarted after rollback.");
                    }));
                }
            }
            catch (Exception ex)
            {
                // Log any errors during process restart
                if (Owner is Main mainForm)
                {
                    mainForm.Invoke((MethodInvoker)(() =>
                    {
                        mainForm.LogMessage($"Error restarting TES3MP server process: {ex.Message}");
                    }));
                }
            }
        }

        private void UpdateBackupJson(List<string> prefixes)
        {
            try
            {
                // Construct the path for the .json file
                string sourcePath = Properties.Settings.Default.SourcePath;
                if (string.IsNullOrEmpty(sourcePath) || !Directory.Exists(sourcePath))
                {
                    throw new DirectoryNotFoundException("Source path is invalid or missing.");
                }

                string jsonFilePath = Path.Combine(sourcePath, "scripts", "custom", "backup_manager", "backups.json");

                // Ensure the directory exists
                string jsonDirectory = Path.GetDirectoryName(jsonFilePath);
                if (!Directory.Exists(jsonDirectory))
                {
                    Directory.CreateDirectory(jsonDirectory);
                }

                // Convert the list of prefixes to JSON format
                string jsonContent = System.Text.Json.JsonSerializer.Serialize(prefixes, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true // Pretty-print the JSON
                });

                // Write the JSON content to the file
                File.WriteAllText(jsonFilePath, jsonContent);
                /*
                // Log the update
                if (Owner is Main mainForm)
                {
                    mainForm.Invoke((MethodInvoker)(() =>
                    {
                        mainForm.LogMessage($"Backup prefixes updated in JSON file at: {jsonFilePath}");
                    }));
                }
                */
            }
            catch (Exception ex)
            {
                // Log any errors
                if (Owner is Main mainForm)
                {
                    mainForm.Invoke((MethodInvoker)(() =>
                    {
                        mainForm.LogMessage($"Error updating backup JSON file: {ex.Message}");
                    }));
                }
            }
        }


    }
}
