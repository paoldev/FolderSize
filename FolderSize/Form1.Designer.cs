namespace FolderSize
{
    partial class Form1
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
            components = new System.ComponentModel.Container();
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            selectDrive = new System.Windows.Forms.ComboBox();
            treeView1 = new System.Windows.Forms.TreeView();
            labelTotalSize = new System.Windows.Forms.Label();
            progressBar1 = new System.Windows.Forms.ProgressBar();
            labelNumFolders = new System.Windows.Forms.Label();
            buttonCancel = new System.Windows.Forms.Button();
            timerTaskDuration = new System.Windows.Forms.Timer(components);
            labelElapsedTime = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(2, 14);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(67, 15);
            label2.TabIndex = 4;
            label2.Text = "Select drive";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(206, 14);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(55, 15);
            label3.TabIndex = 5;
            label3.Text = "Total Size";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(206, 45);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(63, 15);
            label4.TabIndex = 6;
            label4.Text = "Directories";
            // 
            // selectDrive
            // 
            selectDrive.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            selectDrive.FormattingEnabled = true;
            selectDrive.Location = new System.Drawing.Point(75, 11);
            selectDrive.Name = "selectDrive";
            selectDrive.Size = new System.Drawing.Size(106, 23);
            selectDrive.TabIndex = 0;
            selectDrive.SelectedIndexChanged += selectDrive_SelectedIndexChanged;
            // 
            // treeView1
            // 
            treeView1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            treeView1.Location = new System.Drawing.Point(1, 112);
            treeView1.Name = "treeView1";
            treeView1.Size = new System.Drawing.Size(523, 359);
            treeView1.TabIndex = 1;
            // 
            // labelTotalSize
            // 
            labelTotalSize.AutoSize = true;
            labelTotalSize.Location = new System.Drawing.Point(287, 14);
            labelTotalSize.Name = "labelTotalSize";
            labelTotalSize.Size = new System.Drawing.Size(13, 15);
            labelTotalSize.TabIndex = 2;
            labelTotalSize.Text = "0";
            labelTotalSize.Click += label1_Click;
            // 
            // progressBar1
            // 
            progressBar1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            progressBar1.Location = new System.Drawing.Point(1, 77);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new System.Drawing.Size(522, 30);
            progressBar1.TabIndex = 3;
            // 
            // labelNumFolders
            // 
            labelNumFolders.AutoSize = true;
            labelNumFolders.Location = new System.Drawing.Point(287, 45);
            labelNumFolders.Name = "labelNumFolders";
            labelNumFolders.Size = new System.Drawing.Size(13, 15);
            labelNumFolders.TabIndex = 7;
            labelNumFolders.Text = "0";
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonCancel.Location = new System.Drawing.Point(448, 10);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(75, 23);
            buttonCancel.TabIndex = 8;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // timerTaskDuration
            // 
            timerTaskDuration.Interval = 1000;
            timerTaskDuration.Tick += timerTaskDuration_Tick;
            // 
            // labelElapsedTime
            // 
            labelElapsedTime.AutoSize = true;
            labelElapsedTime.Location = new System.Drawing.Point(1, 45);
            labelElapsedTime.Name = "labelElapsedTime";
            labelElapsedTime.Size = new System.Drawing.Size(0, 15);
            labelElapsedTime.TabIndex = 9;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(524, 472);
            Controls.Add(labelElapsedTime);
            Controls.Add(buttonCancel);
            Controls.Add(labelNumFolders);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(progressBar1);
            Controls.Add(labelTotalSize);
            Controls.Add(treeView1);
            Controls.Add(selectDrive);
            Name = "Form1";
            Text = "FolderSize";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ComboBox selectDrive;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Label labelTotalSize;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelNumFolders;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Timer timerTaskDuration;
        private System.Windows.Forms.Label labelElapsedTime;
    }
}

