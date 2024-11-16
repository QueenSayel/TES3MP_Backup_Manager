namespace TES3MP_Manager
{
    partial class Rollback
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            rollbackBtn = new Button();
            backupsNumeric = new NumericUpDown();
            backupsListBox = new ListBox();
            optionsListBox = new ListBox();
            backupsLabel = new Label();
            backupsListLabel = new Label();
            rangeLabel = new Label();
            restartCheckBox = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)backupsNumeric).BeginInit();
            SuspendLayout();
            // 
            // rollbackBtn
            // 
            rollbackBtn.Location = new Point(353, 230);
            rollbackBtn.Name = "rollbackBtn";
            rollbackBtn.Size = new Size(75, 23);
            rollbackBtn.TabIndex = 0;
            rollbackBtn.Text = "Rollback";
            rollbackBtn.UseVisualStyleBackColor = true;
            rollbackBtn.Click += rollbackBtn_Click;
            // 
            // backupsNumeric
            // 
            backupsNumeric.Location = new Point(143, 46);
            backupsNumeric.Name = "backupsNumeric";
            backupsNumeric.Size = new Size(75, 23);
            backupsNumeric.TabIndex = 1;
            // 
            // backupsListBox
            // 
            backupsListBox.FormattingEnabled = true;
            backupsListBox.ItemHeight = 15;
            backupsListBox.Location = new Point(25, 125);
            backupsListBox.Name = "backupsListBox";
            backupsListBox.Size = new Size(298, 424);
            backupsListBox.TabIndex = 2;
            // 
            // optionsListBox
            // 
            optionsListBox.FormattingEnabled = true;
            optionsListBox.ItemHeight = 15;
            optionsListBox.Items.AddRange(new object[] { "Everything", "Cell", "Player", "World" });
            optionsListBox.Location = new Point(353, 125);
            optionsListBox.Name = "optionsListBox";
            optionsListBox.Size = new Size(142, 94);
            optionsListBox.TabIndex = 3;
            // 
            // backupsLabel
            // 
            backupsLabel.AutoSize = true;
            backupsLabel.Location = new Point(25, 48);
            backupsLabel.Name = "backupsLabel";
            backupsLabel.Size = new Size(112, 15);
            backupsLabel.TabIndex = 4;
            backupsLabel.Text = "Number of backups";
            // 
            // backupsListLabel
            // 
            backupsListLabel.AutoSize = true;
            backupsListLabel.Location = new Point(25, 102);
            backupsListLabel.Name = "backupsListLabel";
            backupsListLabel.Size = new Size(86, 15);
            backupsListLabel.TabIndex = 5;
            backupsListLabel.Text = "List of backups";
            // 
            // rangeLabel
            // 
            rangeLabel.AutoSize = true;
            rangeLabel.Location = new Point(353, 102);
            rangeLabel.Name = "rangeLabel";
            rangeLabel.Size = new Size(40, 15);
            rangeLabel.TabIndex = 6;
            rangeLabel.Text = "Range";
            // 
            // restartCheckBox
            // 
            restartCheckBox.AutoSize = true;
            restartCheckBox.Location = new Point(25, 564);
            restartCheckBox.Name = "restartCheckBox";
            restartCheckBox.RightToLeft = RightToLeft.Yes;
            restartCheckBox.Size = new Size(96, 19);
            restartCheckBox.TabIndex = 7;
            restartCheckBox.Text = "Restart server";
            restartCheckBox.UseVisualStyleBackColor = true;
            // 
            // Rollback
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(529, 635);
            Controls.Add(restartCheckBox);
            Controls.Add(rangeLabel);
            Controls.Add(backupsListLabel);
            Controls.Add(backupsLabel);
            Controls.Add(optionsListBox);
            Controls.Add(backupsListBox);
            Controls.Add(backupsNumeric);
            Controls.Add(rollbackBtn);
            Name = "Rollback";
            Text = "Backups";
            ((System.ComponentModel.ISupportInitialize)backupsNumeric).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button rollbackBtn;
        private NumericUpDown backupsNumeric;
        private ListBox backupsListBox;
        private ListBox optionsListBox;
        private Label backupsLabel;
        private Label backupsListLabel;
        private Label rangeLabel;
        private CheckBox restartCheckBox;
    }
}