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

            // Check if the backup path exists and load .zip files
            if (Directory.Exists(backupPath))
            {
                // Get all .zip files in the backup path
                string[] zipFiles = Directory.GetFiles(backupPath, "*.zip");

                // Add each file name to the ListBox with date and time prefix
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
                        backupsListBox.Items.Add($"{formattedDate} {fileName}");
                    }
                    else
                    {
                        // If parsing fails, just add the original filename
                        backupsListBox.Items.Add(fileName);
                    }
                }
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
            // Ensure a backup is selected
            if (backupsListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a backup to roll back to.", "No Backup Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ensure an option is selected
            if (optionsListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select an option for rollback (Everything, Cell, Player, World).", "No Option Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the selected backup item from the ListBox
            string selectedItem = backupsListBox.SelectedItem.ToString();

            // Extract the actual file name by removing the date prefix
            int startOfFileName = selectedItem.IndexOf("]") + 2; // Skip past the closing bracket and space
            string actualFileName = selectedItem.Substring(startOfFileName);

            // Construct the full path of the backup file
            string backupFilePath = Path.Combine(Properties.Settings.Default.BackupPath, actualFileName);

            // Determine the target subfolder for selective extraction
            string selectedOption = optionsListBox.SelectedItem.ToString();
            string targetSubfolder = selectedOption switch
            {
                "Everything" => "",        // Extract everything
                "Cell" => "data/cell",     // Extract only the 'data/cell' folder
                "Player" => "data/player", // Extract only the 'data/player' folder
                "World" => "data/world",   // Extract only the 'data/world' folder
                _ => throw new InvalidOperationException("Invalid option selected.")
            };

            try
            {
                // Pause the backup timer in Main
                if (Owner is Main mainForm)
                {
                    mainForm.Invoke((MethodInvoker)(() =>
                    {
                        mainForm.StopBackupTimer();
                        mainForm.LogMessage("Performing rollback...");
                    }));
                }

                // Clear the target directory if "Everything" is selected
                string sourcePath = Properties.Settings.Default.SourcePath;
                if (selectedOption == "Everything")
                {
                    DirectoryInfo di = new DirectoryInfo(sourcePath);
                    foreach (FileInfo file in di.GetFiles()) file.Delete();
                    foreach (DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);
                }

                // Extract the selected folders from the backup archive
                ExtractSelectedFolders(backupFilePath, sourcePath, targetSubfolder);

                // Log rollback completion in Main
                if (Owner is Main mainFormWithLogging)
                {
                    mainFormWithLogging.Invoke((MethodInvoker)(() =>
                    {
                        mainFormWithLogging.LogMessage($"Rollback completed using backup: {actualFileName} (Range: {selectedOption})");
                    }));
                }
            }
            catch (Exception ex)
            {
                // Log any errors in Main
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
                // Restart the backup timer in Main and log it
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

        private void ExtractSelectedFolders(string zipFilePath, string destinationPath, string targetSubfolder)
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
    }
}
