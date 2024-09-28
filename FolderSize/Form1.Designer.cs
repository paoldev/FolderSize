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
            System.Windows.Forms.ToolStripMenuItem legendaToolStripMenuItem;
            System.Windows.Forms.ToolStripMenuItem normalDirectoriesToolStripMenuItem;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            System.Windows.Forms.ToolStripMenuItem unaccessibleDirectoriesToolStripMenuItem;
            System.Windows.Forms.ToolStripMenuItem reparsePointsToolStripMenuItem;
            System.Windows.Forms.ToolStripMenuItem filesSetsToolStripMenuItem;
            selectDrive = new System.Windows.Forms.ComboBox();
            treeView1 = new System.Windows.Forms.TreeView();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            openInWindowsExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openInCommandPromptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openInPowershellToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            labelTotalSize = new System.Windows.Forms.Label();
            progressBar1 = new System.Windows.Forms.ProgressBar();
            labelNumFolders = new System.Windows.Forms.Label();
            buttonCancel = new System.Windows.Forms.Button();
            timerTaskDuration = new System.Windows.Forms.Timer(components);
            labelElapsedTime = new System.Windows.Forms.Label();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            treemapControl1 = new Microsoft.Research.CommunityTechnologies.Treemap.TreemapControl();
            buttonZoomIn = new System.Windows.Forms.Button();
            buttonZoomOut = new System.Windows.Forms.Button();
            buttonBack = new System.Windows.Forms.Button();
            buttonForward = new System.Windows.Forms.Button();
            checkBoxSyncViews = new System.Windows.Forms.CheckBox();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            buttonRestartAdmin = new System.Windows.Forms.Button();
            labelAdmin = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            legendaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            normalDirectoriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            unaccessibleDirectoriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            reparsePointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            filesSetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 37);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(67, 15);
            label2.TabIndex = 4;
            label2.Text = "Select drive";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(206, 37);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(55, 15);
            label3.TabIndex = 5;
            label3.Text = "Total Size";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(206, 68);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(63, 15);
            label4.TabIndex = 6;
            label4.Text = "Directories";
            // 
            // legendaToolStripMenuItem
            // 
            legendaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { normalDirectoriesToolStripMenuItem, unaccessibleDirectoriesToolStripMenuItem, reparsePointsToolStripMenuItem, filesSetsToolStripMenuItem });
            legendaToolStripMenuItem.Name = "legendaToolStripMenuItem";
            legendaToolStripMenuItem.Size = new System.Drawing.Size(87, 20);
            legendaToolStripMenuItem.Text = "Color legend";
            // 
            // normalDirectoriesToolStripMenuItem
            // 
            normalDirectoriesToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("normalDirectoriesToolStripMenuItem.Image");
            normalDirectoriesToolStripMenuItem.Name = "normalDirectoriesToolStripMenuItem";
            normalDirectoriesToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            normalDirectoriesToolStripMenuItem.Text = "Normal directories";
            // 
            // unaccessibleDirectoriesToolStripMenuItem
            // 
            unaccessibleDirectoriesToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("unaccessibleDirectoriesToolStripMenuItem.Image");
            unaccessibleDirectoriesToolStripMenuItem.Name = "unaccessibleDirectoriesToolStripMenuItem";
            unaccessibleDirectoriesToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            unaccessibleDirectoriesToolStripMenuItem.Text = "Inaccessible directories";
            // 
            // reparsePointsToolStripMenuItem
            // 
            reparsePointsToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("reparsePointsToolStripMenuItem.Image");
            reparsePointsToolStripMenuItem.Name = "reparsePointsToolStripMenuItem";
            reparsePointsToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            reparsePointsToolStripMenuItem.Text = "Reparse points";
            // 
            // filesSetsToolStripMenuItem
            // 
            filesSetsToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("filesSetsToolStripMenuItem.Image");
            filesSetsToolStripMenuItem.Name = "filesSetsToolStripMenuItem";
            filesSetsToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            filesSetsToolStripMenuItem.Text = "Files sets";
            // 
            // selectDrive
            // 
            selectDrive.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            selectDrive.FormattingEnabled = true;
            selectDrive.Location = new System.Drawing.Point(85, 34);
            selectDrive.Name = "selectDrive";
            selectDrive.Size = new System.Drawing.Size(106, 23);
            selectDrive.TabIndex = 0;
            selectDrive.SelectedIndexChanged += SelectDrive_SelectedIndexChanged;
            // 
            // treeView1
            // 
            treeView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            treeView1.HideSelection = false;
            treeView1.Location = new System.Drawing.Point(0, 0);
            treeView1.Name = "treeView1";
            treeView1.ShowNodeToolTips = true;
            treeView1.Size = new System.Drawing.Size(395, 383);
            treeView1.TabIndex = 1;
            treeView1.AfterSelect += TreeView1_AfterSelect;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { openInWindowsExplorerToolStripMenuItem, openInCommandPromptToolStripMenuItem, openInPowershellToolStripMenuItem, toolStripSeparator2, propertiesToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(230, 98);
            // 
            // openInWindowsExplorerToolStripMenuItem
            // 
            openInWindowsExplorerToolStripMenuItem.Name = "openInWindowsExplorerToolStripMenuItem";
            openInWindowsExplorerToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            openInWindowsExplorerToolStripMenuItem.Text = "Open in Windows Explorer";
            openInWindowsExplorerToolStripMenuItem.Click += OpenInWindowsExplorerToolStripMenuItem_Click;
            // 
            // openInCommandPromptToolStripMenuItem
            // 
            openInCommandPromptToolStripMenuItem.Name = "openInCommandPromptToolStripMenuItem";
            openInCommandPromptToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            openInCommandPromptToolStripMenuItem.Text = "Open in Command Prompt";
            openInCommandPromptToolStripMenuItem.Click += OpenInCommandPromptToolStripMenuItem_Click;
            // 
            // openInPowershellToolStripMenuItem
            // 
            openInPowershellToolStripMenuItem.Name = "openInPowershellToolStripMenuItem";
            openInPowershellToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            openInPowershellToolStripMenuItem.Text = "Open in Windows PowerShell";
            openInPowershellToolStripMenuItem.Click += OpenInPowershellToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(226, 6);
            // 
            // propertiesToolStripMenuItem
            // 
            propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
            propertiesToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            propertiesToolStripMenuItem.Text = "Properties";
            propertiesToolStripMenuItem.Click += PropertiesToolStripMenuItem_Click;
            // 
            // labelTotalSize
            // 
            labelTotalSize.AutoSize = true;
            labelTotalSize.Location = new System.Drawing.Point(287, 37);
            labelTotalSize.Name = "labelTotalSize";
            labelTotalSize.Size = new System.Drawing.Size(13, 15);
            labelTotalSize.TabIndex = 2;
            labelTotalSize.Text = "0";
            // 
            // progressBar1
            // 
            progressBar1.Location = new System.Drawing.Point(12, 100);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new System.Drawing.Size(393, 30);
            progressBar1.TabIndex = 3;
            // 
            // labelNumFolders
            // 
            labelNumFolders.AutoSize = true;
            labelNumFolders.Location = new System.Drawing.Point(287, 68);
            labelNumFolders.Name = "labelNumFolders";
            labelNumFolders.Size = new System.Drawing.Size(13, 15);
            labelNumFolders.TabIndex = 7;
            labelNumFolders.Text = "0";
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new System.Drawing.Point(410, 100);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(75, 30);
            buttonCancel.TabIndex = 8;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += ButtonCancel_Click;
            // 
            // timerTaskDuration
            // 
            timerTaskDuration.Interval = 1000;
            timerTaskDuration.Tick += TimerTaskDuration_Tick;
            // 
            // labelElapsedTime
            // 
            labelElapsedTime.AutoSize = true;
            labelElapsedTime.Location = new System.Drawing.Point(12, 68);
            labelElapsedTime.Name = "labelElapsedTime";
            labelElapsedTime.Size = new System.Drawing.Size(0, 15);
            labelElapsedTime.TabIndex = 9;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            splitContainer1.Location = new System.Drawing.Point(12, 136);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(treeView1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(treemapControl1);
            splitContainer1.Size = new System.Drawing.Size(999, 383);
            splitContainer1.SplitterDistance = 395;
            splitContainer1.TabIndex = 10;
            // 
            // treemapControl1
            // 
            treemapControl1.AllowDrag = false;
            treemapControl1.BorderColor = System.Drawing.SystemColors.WindowFrame;
            treemapControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            treemapControl1.DiscreteNegativeColors = 20;
            treemapControl1.DiscretePositiveColors = 20;
            treemapControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            treemapControl1.EmptySpaceLocation = Microsoft.Research.CommunityTechnologies.Treemap.EmptySpaceLocation.DeterminedByLayoutAlgorithm;
            treemapControl1.FontFamily = "Arial";
            treemapControl1.FontSolidColor = System.Drawing.SystemColors.WindowText;
            treemapControl1.IsZoomable = true;
            treemapControl1.LayoutAlgorithm = Microsoft.Research.CommunityTechnologies.Treemap.LayoutAlgorithm.TopWeightedSquarified;
            treemapControl1.Location = new System.Drawing.Point(0, 0);
            treemapControl1.MaxColor = System.Drawing.Color.Green;
            treemapControl1.MaxColorMetric = 100F;
            treemapControl1.MinColor = System.Drawing.Color.Red;
            treemapControl1.MinColorMetric = -100F;
            treemapControl1.Name = "treemapControl1";
            treemapControl1.NodeColorAlgorithm = Microsoft.Research.CommunityTechnologies.Treemap.NodeColorAlgorithm.UseColorMetric;
            treemapControl1.NodeLevelsWithText = Microsoft.Research.CommunityTechnologies.Treemap.NodeLevelsWithText.All;
            treemapControl1.PaddingDecrementPerLevelPx = 1;
            treemapControl1.PaddingPx = 5;
            treemapControl1.PenWidthDecrementPerLevelPx = 1;
            treemapControl1.PenWidthPx = 3;
            treemapControl1.SelectedBackColor = System.Drawing.SystemColors.Highlight;
            treemapControl1.SelectedFontColor = System.Drawing.SystemColors.HighlightText;
            treemapControl1.ShowToolTips = true;
            treemapControl1.Size = new System.Drawing.Size(600, 383);
            treemapControl1.TabIndex = 0;
            treemapControl1.TextLocation = Microsoft.Research.CommunityTechnologies.Treemap.TextLocation.Top;
            treemapControl1.NodeMouseUp += TreemapControl1_NodeMouseUp;
            treemapControl1.NodeDoubleClick += TreemapControl1_NodeDoubleClick;
            treemapControl1.SelectedNodeChanged += TreemapControl1_SelectedNodeChanged;
            // 
            // buttonZoomIn
            // 
            buttonZoomIn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonZoomIn.Location = new System.Drawing.Point(829, 100);
            buttonZoomIn.Name = "buttonZoomIn";
            buttonZoomIn.Size = new System.Drawing.Size(41, 30);
            buttonZoomIn.TabIndex = 11;
            buttonZoomIn.Text = "+";
            toolTip1.SetToolTip(buttonZoomIn, "Zoom In");
            buttonZoomIn.UseVisualStyleBackColor = true;
            buttonZoomIn.Click += ButtonZoomIn_Click;
            // 
            // buttonZoomOut
            // 
            buttonZoomOut.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonZoomOut.Location = new System.Drawing.Point(876, 100);
            buttonZoomOut.Name = "buttonZoomOut";
            buttonZoomOut.Size = new System.Drawing.Size(41, 30);
            buttonZoomOut.TabIndex = 12;
            buttonZoomOut.Text = "-";
            toolTip1.SetToolTip(buttonZoomOut, "Zoom Out");
            buttonZoomOut.UseVisualStyleBackColor = true;
            buttonZoomOut.Click += ButtonZoomOut_Click;
            // 
            // buttonBack
            // 
            buttonBack.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonBack.Location = new System.Drawing.Point(923, 100);
            buttonBack.Name = "buttonBack";
            buttonBack.Size = new System.Drawing.Size(41, 30);
            buttonBack.TabIndex = 13;
            buttonBack.Text = "<";
            toolTip1.SetToolTip(buttonBack, "Back");
            buttonBack.UseVisualStyleBackColor = true;
            buttonBack.Click += ButtonBack_Click;
            // 
            // buttonForward
            // 
            buttonForward.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonForward.Location = new System.Drawing.Point(970, 100);
            buttonForward.Name = "buttonForward";
            buttonForward.Size = new System.Drawing.Size(41, 30);
            buttonForward.TabIndex = 14;
            buttonForward.Text = ">";
            toolTip1.SetToolTip(buttonForward, "Forward");
            buttonForward.UseVisualStyleBackColor = true;
            buttonForward.Click += ButtonForward_Click;
            // 
            // checkBoxSyncViews
            // 
            checkBoxSyncViews.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            checkBoxSyncViews.AutoSize = true;
            checkBoxSyncViews.Checked = true;
            checkBoxSyncViews.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxSyncViews.Location = new System.Drawing.Point(725, 107);
            checkBoxSyncViews.Name = "checkBoxSyncViews";
            checkBoxSyncViews.Size = new System.Drawing.Size(83, 19);
            checkBoxSyncViews.TabIndex = 15;
            checkBoxSyncViews.Text = "Sync views";
            checkBoxSyncViews.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, legendaToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(1023, 24);
            menuStrip1.TabIndex = 16;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { aboutToolStripMenuItem, toolStripSeparator1, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += AboutToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(104, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            // 
            // buttonRestartAdmin
            // 
            buttonRestartAdmin.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonRestartAdmin.Location = new System.Drawing.Point(782, 34);
            buttonRestartAdmin.Name = "buttonRestartAdmin";
            buttonRestartAdmin.Size = new System.Drawing.Size(229, 23);
            buttonRestartAdmin.TabIndex = 18;
            buttonRestartAdmin.Text = "To scan more folders, restart as Admin";
            buttonRestartAdmin.UseVisualStyleBackColor = true;
            buttonRestartAdmin.Click += ButtonRestartAdmin_Click;
            // 
            // labelAdmin
            // 
            labelAdmin.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            labelAdmin.AutoSize = true;
            labelAdmin.BackColor = System.Drawing.Color.Teal;
            labelAdmin.ForeColor = System.Drawing.Color.White;
            labelAdmin.Location = new System.Drawing.Point(959, 36);
            labelAdmin.Name = "labelAdmin";
            labelAdmin.Padding = new System.Windows.Forms.Padding(3);
            labelAdmin.Size = new System.Drawing.Size(52, 21);
            labelAdmin.TabIndex = 19;
            labelAdmin.Text = "ADMIN";
            labelAdmin.Visible = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1023, 531);
            Controls.Add(labelAdmin);
            Controls.Add(buttonRestartAdmin);
            Controls.Add(checkBoxSyncViews);
            Controls.Add(buttonForward);
            Controls.Add(buttonBack);
            Controls.Add(buttonZoomOut);
            Controls.Add(buttonZoomIn);
            Controls.Add(splitContainer1);
            Controls.Add(labelElapsedTime);
            Controls.Add(buttonCancel);
            Controls.Add(labelNumFolders);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(progressBar1);
            Controls.Add(labelTotalSize);
            Controls.Add(selectDrive);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            MinimumSize = new System.Drawing.Size(800, 250);
            Name = "Form1";
            Text = "FolderSize";
            contextMenuStrip1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
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
        private System.Windows.Forms.SplitContainer splitContainer1;
        private Microsoft.Research.CommunityTechnologies.Treemap.TreemapControl treemapControl1;
        private System.Windows.Forms.Button buttonZoomIn;
        private System.Windows.Forms.Button buttonZoomOut;
        private System.Windows.Forms.Button buttonBack;
        private System.Windows.Forms.Button buttonForward;
        private System.Windows.Forms.CheckBox checkBoxSyncViews;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Button buttonRestartAdmin;
        private System.Windows.Forms.Label labelAdmin;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem openInWindowsExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openInCommandPromptToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openInPowershellToolStripMenuItem;
    }
}

