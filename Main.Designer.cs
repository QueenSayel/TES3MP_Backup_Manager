namespace TES3MP_Manager
{
    partial class Main
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
            logTextBox = new TextBox();
            pathBtn = new Button();
            pathTextBox = new TextBox();
            intervalNumeric = new NumericUpDown();
            startBtn = new Button();
            backupsBtn = new Button();
            intervalLabel = new Label();
            setPathBackupBtn = new Button();
            pathBackupTextBox = new TextBox();
            compressionComboBox = new ComboBox();
            timerLabel = new Label();
            stopBtn = new Button();
            compressionLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)intervalNumeric).BeginInit();
            SuspendLayout();
            // 
            // logTextBox
            // 
            logTextBox.BackColor = SystemColors.Window;
            logTextBox.Location = new Point(28, 312);
            logTextBox.Multiline = true;
            logTextBox.Name = "logTextBox";
            logTextBox.ReadOnly = true;
            logTextBox.Size = new Size(471, 255);
            logTextBox.TabIndex = 0;
            // 
            // pathBtn
            // 
            pathBtn.Location = new Point(28, 63);
            pathBtn.Name = "pathBtn";
            pathBtn.Size = new Size(75, 23);
            pathBtn.TabIndex = 1;
            pathBtn.Text = "Set";
            pathBtn.UseVisualStyleBackColor = true;
            pathBtn.Click += pathBtn_Click;
            // 
            // pathTextBox
            // 
            pathTextBox.BackColor = SystemColors.Window;
            pathTextBox.Location = new Point(113, 63);
            pathTextBox.Name = "pathTextBox";
            pathTextBox.PlaceholderText = "Select /server/ folder...";
            pathTextBox.ReadOnly = true;
            pathTextBox.Size = new Size(249, 23);
            pathTextBox.TabIndex = 2;
            // 
            // intervalNumeric
            // 
            intervalNumeric.Location = new Point(252, 186);
            intervalNumeric.Name = "intervalNumeric";
            intervalNumeric.Size = new Size(109, 23);
            intervalNumeric.TabIndex = 3;
            intervalNumeric.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // startBtn
            // 
            startBtn.Location = new Point(29, 184);
            startBtn.Name = "startBtn";
            startBtn.Size = new Size(75, 23);
            startBtn.TabIndex = 4;
            startBtn.Text = "Start";
            startBtn.UseVisualStyleBackColor = true;
            startBtn.Click += startBtn_Click;
            // 
            // backupsBtn
            // 
            backupsBtn.Location = new Point(28, 578);
            backupsBtn.Name = "backupsBtn";
            backupsBtn.Size = new Size(75, 23);
            backupsBtn.TabIndex = 5;
            backupsBtn.Text = "Backups";
            backupsBtn.UseVisualStyleBackColor = true;
            backupsBtn.Click += backupsBtn_Click;
            // 
            // intervalLabel
            // 
            intervalLabel.AutoSize = true;
            intervalLabel.Location = new Point(141, 190);
            intervalLabel.Name = "intervalLabel";
            intervalLabel.Size = new Size(105, 15);
            intervalLabel.TabIndex = 6;
            intervalLabel.Text = "Interval in minutes";
            // 
            // setPathBackupBtn
            // 
            setPathBackupBtn.Location = new Point(29, 101);
            setPathBackupBtn.Name = "setPathBackupBtn";
            setPathBackupBtn.Size = new Size(75, 23);
            setPathBackupBtn.TabIndex = 7;
            setPathBackupBtn.Text = "Set";
            setPathBackupBtn.UseVisualStyleBackColor = true;
            setPathBackupBtn.Click += setPathBackupBtn_Click;
            // 
            // pathBackupTextBox
            // 
            pathBackupTextBox.BackColor = SystemColors.Window;
            pathBackupTextBox.Location = new Point(113, 101);
            pathBackupTextBox.Name = "pathBackupTextBox";
            pathBackupTextBox.PlaceholderText = "Select backup folder...";
            pathBackupTextBox.ReadOnly = true;
            pathBackupTextBox.Size = new Size(249, 23);
            pathBackupTextBox.TabIndex = 8;
            // 
            // compressionComboBox
            // 
            compressionComboBox.FormattingEnabled = true;
            compressionComboBox.Items.AddRange(new object[] { "Best", "Fastest", "None" });
            compressionComboBox.Location = new Point(251, 223);
            compressionComboBox.Name = "compressionComboBox";
            compressionComboBox.Size = new Size(110, 23);
            compressionComboBox.TabIndex = 9;
            // 
            // timerLabel
            // 
            timerLabel.AutoSize = true;
            timerLabel.Location = new Point(28, 285);
            timerLabel.Name = "timerLabel";
            timerLabel.Size = new Size(0, 15);
            timerLabel.TabIndex = 10;
            // 
            // stopBtn
            // 
            stopBtn.Location = new Point(29, 223);
            stopBtn.Name = "stopBtn";
            stopBtn.Size = new Size(75, 23);
            stopBtn.TabIndex = 11;
            stopBtn.Text = "Stop";
            stopBtn.UseVisualStyleBackColor = true;
            stopBtn.Click += stopBtn_Click;
            // 
            // compressionLabel
            // 
            compressionLabel.AutoSize = true;
            compressionLabel.Location = new Point(141, 226);
            compressionLabel.Name = "compressionLabel";
            compressionLabel.Size = new Size(77, 15);
            compressionLabel.TabIndex = 12;
            compressionLabel.Text = "Compression";
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(529, 635);
            Controls.Add(compressionLabel);
            Controls.Add(stopBtn);
            Controls.Add(timerLabel);
            Controls.Add(compressionComboBox);
            Controls.Add(pathBackupTextBox);
            Controls.Add(setPathBackupBtn);
            Controls.Add(intervalLabel);
            Controls.Add(backupsBtn);
            Controls.Add(startBtn);
            Controls.Add(intervalNumeric);
            Controls.Add(pathTextBox);
            Controls.Add(pathBtn);
            Controls.Add(logTextBox);
            MaximizeBox = false;
            Name = "Main";
            Text = "TES3MP Backup Manager";
            Load += Main_Load;
            ((System.ComponentModel.ISupportInitialize)intervalNumeric).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox logTextBox;
        private Button pathBtn;
        private TextBox pathTextBox;
        private NumericUpDown intervalNumeric;
        private Button startBtn;
        private Button backupsBtn;
        private Label intervalLabel;
        private Button setPathBackupBtn;
        private TextBox pathBackupTextBox;
        private ComboBox compressionComboBox;
        private Label timerLabel;
        private Button stopBtn;
        private Label compressionLabel;
    }
}
