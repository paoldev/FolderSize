//
// FolderSize
//
// Copyright (c) 2012-2026 paoldev
//
// Licensed under the MIT license.
// SPDX-License-Identifier: MIT
//

using Microsoft.Research.CommunityTechnologies.Treemap;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace FolderSize
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            string[] asDrives = System.IO.Directory.GetLogicalDrives();
            foreach (string sDrive in asDrives)
            {
                selectDrive.Items.Add(sDrive);
            }

            buttonCancel.DataBindings.Add("Visible", taskIsRunningVM, "TaskIsRunning");
            selectDrive.DataBindings.Add("Enabled", taskIsRunningVM, "TaskIsNotRunning");
            buttonBack.DataBindings.Add("Enabled", taskIsRunningVM, "TaskIsNotRunning");
            buttonForward.DataBindings.Add("Enabled", taskIsRunningVM, "TaskIsNotRunning");
            buttonZoomIn.DataBindings.Add("Enabled", taskIsRunningVM, "TaskIsNotRunning");
            buttonZoomOut.DataBindings.Add("Enabled", taskIsRunningVM, "TaskIsNotRunning");
            splitContainer1.DataBindings.Add("Enabled", taskIsRunningVM, "TaskIsNotRunning");
            checkBoxSyncViews.DataBindings.Add("Enabled", taskIsRunningVM, "TaskIsNotRunning");
            buttonRestartAdmin.DataBindings.Add("Enabled", taskIsRunningVM, "TaskIsNotRunning");

            bool bIsAdministrator = ApplicationHelpers.IsAdministrator();
            labelAdmin.Visible = bIsAdministrator;
            buttonRestartAdmin.Visible = !bIsAdministrator;
            if (bIsAdministrator)
            {
                int right = checkBoxFastMode.Right;
                checkBoxFastMode.Text = "Try Fast Mode";
                checkBoxFastMode.Left = right - checkBoxFastMode.Width;
                checkBoxFastMode.Checked = true;
                checkBoxFastMode.DataBindings.Add("Enabled", taskIsRunningVM, "TaskIsNotRunning");
            }
            else
            {
                checkBoxFastMode.Checked = false;
                checkBoxFastMode.Enabled = false;
            }
        }

        private async void SelectDrive_SelectedIndexChanged(object sender, EventArgs e)
        {
            await UpdateGUIAsync(selectDrive.Text);
        }

        private async Task UpdateGUIAsync(string sStartDirectory)
        {
            taskIsRunningVM.TaskIsRunning = true;

            progressBar1.Value = 0;

            progressBar1.Minimum = 0;
            progressBar1.Maximum = 1;
            progressBar1.Step = 1;
            progressBar1.Value = 0;
            labelTotalSize.Text = "0";
            labelTotalSize.Update();
            labelNumFolders.Text = "0";
            labelNumFolders.Update();

            taskStart = DateTime.Now;
            timerTaskDuration.Start();

            uint numLevels = 0;
            MyDirInfo? dirInfo = null;

            IProgress<MyDirInfo.ProgressValue?> progress = new Progress<MyDirInfo.ProgressValue?>(value =>
            {
                if (value.HasValue)
                {
                    var v = value.Value;
                    //Don't stress UI thread too much.
                    if ((v.NumDirs % 128) == 0)
                    {
                        progressBar1.Maximum = v.TotalDirs;
                        progressBar1.Value = v.NumDirs;
                        labelTotalSize.Text = $"{GetNumStringWithSep(v.DirsSize)} bytes ({GetSizeAsShortString(v.DirsSize)})";
                        labelNumFolders.Text = $"{GetNumStringWithSep(v.NumDirs)} / {GetNumStringWithSep(v.TotalDirs)}";
                        labelProgressInfo.Text = v.ProgressInfo;
                    }
                }
                else
                {
                    progressBar1.Value = progressBar1.Maximum;
                    labelTotalSize.Text = (dirInfo == null) ? "0" : $"{GetNumStringWithSep(dirInfo.TotalFileSize)} bytes ({GetSizeAsShortString(dirInfo.TotalFileSize)})";
                    labelNumFolders.Text = $"{GetNumStringWithSep(progressBar1.Value)} / {GetNumStringWithSep(progressBar1.Maximum)}";
                    labelProgressInfo.Text = string.Empty;

                    UpdateTrees(dirInfo, numLevels);
                }
            });

            tokenSource = new CancellationTokenSource();
            (dirInfo, numLevels) = await MyDirInfo.GetDirectoryInfoAsync(sStartDirectory, checkBoxFastMode.Checked, progress, tokenSource.Token);
            await Task.Run(() => { progress.Report(null); });
            tokenSource = null;

            timerTaskDuration.Stop();
            UpdateElapsedTime();

            taskIsRunningVM.TaskIsRunning = false;
        }

        private readonly struct NodeInfo(MyDirInfo i_dirInfo, TreeNode i_treeNode, Node i_treeMapNode)
        {
            public readonly TreeNode TreeNode = i_treeNode;
            public readonly Node TreeMapNode = i_treeMapNode;
            public readonly string? DirFullName = i_dirInfo.IsDummyFolder ? Path.GetDirectoryName(i_dirInfo.FullName) : i_dirInfo.FullName;
        };

        private void UpdateTrees(MyDirInfo? i_dirInfo, uint i_numLevels)
        {
            treeView1.BeginUpdate();
            treemapControl1.BeginUpdate();

            treeView1.Nodes.Clear();
            treemapControl1.Clear();

            treeView1.Tag = null;
            treemapControl1.Tag = null;

            if (i_dirInfo != null)
            {
                uint level = 0;
                var rootNode = CreateNewTreeNode(i_dirInfo, level);
                var treeMapNode = CreateNewTreeMapNode(i_dirInfo, level, i_numLevels);

                NodeInfo nodeInfo = new(i_dirInfo, rootNode, treeMapNode);
                rootNode.Tag = nodeInfo;
                treeMapNode.Tag = nodeInfo;

                InsertSubDirs(rootNode, treeMapNode, i_dirInfo, level + 1, i_numLevels);

                treeView1.Nodes.Add(rootNode);
                treemapControl1.Nodes.Add(treeMapNode);

                rootNode.Expand();
            }

            treemapControl1.EndUpdate();
            treeView1.EndUpdate();
        }

        private TreeNode CreateNewTreeNode(MyDirInfo i_dirInfo, uint i_level)
        {
            string link = !string.IsNullOrEmpty(i_dirInfo.LinkTarget) ? $" ({i_dirInfo.LinkTarget})" : (i_dirInfo.IsReparsePoint ? " (<ReparsePoint>)" : string.Empty);
            string exception = i_dirInfo.HasException ? " (*)" : "";
            string name = (i_level == 0) ? i_dirInfo.FullName : (i_dirInfo.Name + link + exception + "       " + GetSizeAsShortString(i_dirInfo.TotalFileSize));

            var newNode = new TreeNode(name)
            {
                ToolTipText = GetTooltip(i_dirInfo),
                ContextMenuStrip = contextMenuStrip1
            };
            if (i_dirInfo.IsReparsePoint)
            {
                newNode.ForeColor = System.Drawing.Color.Blue;
            }
            else if (i_dirInfo.HasException)
            {
                newNode.ForeColor = System.Drawing.Color.Red;
            }
            else if (i_dirInfo.IsDummyFolder)
            {
                newNode.ForeColor = System.Drawing.Color.Green;
            }
            return newNode;
        }

        private Node CreateNewTreeMapNode(MyDirInfo i_dirInfo, uint i_level, uint i_numLevels)
        {
            return new Node((i_level == 0) ? i_dirInfo.FullName : i_dirInfo.Name, i_dirInfo.TotalFileSize, LevelToColor(i_level, i_numLevels), null, GetTooltip(i_dirInfo));
        }

        private static string GetTooltip(MyDirInfo i_dirInfo)
        {
            StringBuilder sbTooltip = new();
            sbTooltip.AppendLine(i_dirInfo.IsDummyFolder ? Path.GetDirectoryName(i_dirInfo.FullName) : i_dirInfo.FullName);
            sbTooltip.AppendLine($"Num files: {GetNumStringWithSep(i_dirInfo.NumFiles)}");
            sbTooltip.AppendLine($"Files size: {GetNumStringWithSep(i_dirInfo.DirFileSize)} bytes");
            if (!i_dirInfo.IsDummyFolder)
            {
                sbTooltip.AppendLine($"Num directories: {GetNumStringWithSep(i_dirInfo.NumDirs)}");
                sbTooltip.AppendLine($"Directories size: {GetNumStringWithSep(i_dirInfo.SubDirsFileSize)} bytes");
            }
            return sbTooltip.ToString();
        }

        private void InsertSubDirs(TreeNode i_node, Node i_mapnode, MyDirInfo i_dirInfo, uint i_level, uint i_numLevels)
        {
            if (i_dirInfo.SubDirs != null)
            {
                foreach (MyDirInfo dirInfo in i_dirInfo.SubDirs)
                {
                    var treeNode = CreateNewTreeNode(dirInfo, i_level);
                    var treeMapNode = CreateNewTreeMapNode(dirInfo, i_level, i_numLevels);

                    NodeInfo nodeInfo = new(dirInfo, treeNode, treeMapNode);
                    treeNode.Tag = nodeInfo;
                    treeMapNode.Tag = nodeInfo;

                    i_node.Nodes.Add(treeNode);
                    i_mapnode.Nodes.Add(treeMapNode);

                    InsertSubDirs(treeNode, treeMapNode, dirInfo, i_level + 1, i_numLevels);
                }
            }
        }

        // Although only English UI is available, use CurrentCulture to get thousands' separator.
        private static string GetNumStringWithSep<T>(T v) => string.Format(CultureInfo.CurrentCulture, "{0:##,0}", v);

        private static string GetSizeAsShortString(ulong size)
        {
            const ulong OneGB = 1024 * 1024 * 1024;
            const ulong OneMB = 1024 * 1024;
            const ulong OneKB = 1024;
            if (size >= OneGB)
            {
                return (size / 1024.0 / 1024.0 / 1024.0).ToString("F1") + " GB";
            }
            else if (size >= OneMB)
            {
                return (size / 1024.0 / 1024.0).ToString("F1") + " MB";
            }
            else if (size >= OneKB)
            {
                return (size / 1024.0).ToString("F1") + " KB";
            }
            else
            {
                return size.ToString() + " B";
            }
        }

        CancellationTokenSource? tokenSource = null;
        DateTime taskStart = DateTime.MinValue;

        public class TaskIsRunningVM : INotifyPropertyChanged
        {
            private bool _taskIsRunning = false;
            public bool TaskIsRunning
            {
                get
                {
                    return _taskIsRunning;
                }
                set
                {
                    if (_taskIsRunning == value) return;
                    _taskIsRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TaskIsRunning)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TaskIsNotRunning)));
                }
            }

            public bool TaskIsNotRunning
            {
                get
                {
                    return !TaskIsRunning;
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }
        private readonly TaskIsRunningVM taskIsRunningVM = new();

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            tokenSource?.Cancel();
        }

        private void TimerTaskDuration_Tick(object sender, EventArgs e)
        {
            UpdateElapsedTime();
        }

        private void UpdateElapsedTime()
        {
            labelElapsedTime.Text = (DateTime.Now - taskStart).ToString(@"hh\:mm\:ss\.fff");
        }

        #region Treemap control

        private float LevelToColor(uint level, uint numLevels)
        {
            float min = 0.0f;// treemapControl1.MinColorMetric;
            float max = treemapControl1.MaxColorMetric;

            if ((level <= 0) || (numLevels <= 1))
            {
                return min;
            }
            else if (level >= numLevels - 1)
            {
                return max;
            }
            else
            {
                return min + level * (max - min) / (numLevels - 1);
            }
        }

        private void ButtonZoomIn_Click(object sender, EventArgs e)
        {
            if (treemapControl1.SelectedNode is Node node)
            {
                if (treemapControl1.CanZoomIn(node))
                {
                    treemapControl1.ZoomIn(node);
                }
            }
        }

        private void ButtonZoomOut_Click(object sender, EventArgs e)
        {
            if (treemapControl1.CanZoomOut())
            {
                treemapControl1.ZoomOut();
            }
        }

        private void ButtonBack_Click(object sender, EventArgs e)
        {
            if (treemapControl1.CanMoveBack())
            {
                treemapControl1.MoveBack();
            }
        }

        private void ButtonForward_Click(object sender, EventArgs e)
        {
            if (treemapControl1.CanMoveForward())
            {
                treemapControl1.MoveForward();
            }
        }

        private void TreemapControl1_NodeDoubleClick(object sender, NodeEventArgs nodeEventArgs)
        {
            if (nodeEventArgs?.Node is Node node)
            {
                if (treemapControl1.CanZoomIn(node))
                {
                    treemapControl1.ZoomIn(node);
                }
            }
        }

        bool bSyncingViews = false;

        private void TreemapControl1_SelectedNodeChanged(object sender, EventArgs e)
        {
            if (checkBoxSyncViews.Checked && !bSyncingViews)
            {
                bSyncingViews = true;

                if (treemapControl1.SelectedNode is Node node)
                {
                    treeView1.SelectedNode = (node.Tag as NodeInfo?)?.TreeNode!;
                    treeView1.Invalidate();
                }

                bSyncingViews = false;
            }
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (checkBoxSyncViews.Checked && !bSyncingViews)
            {
                bSyncingViews = true;

                if (e.Node is TreeNode node)
                {
                    var treeMapNode = (node.Tag as NodeInfo?)?.TreeMapNode!;
                    if (treemapControl1.Nodes.Count == 1)
                    {
                        var treeMapCurrentRoot = treemapControl1.Nodes[0];

                        bool bParentFound = false;
                        var parent = treeMapNode.Parent;
                        while (parent != null)
                        {
                            if (parent == treeMapCurrentRoot)
                            {
                                bParentFound = true;
                                break;
                            }
                            parent = parent.Parent;
                        }

                        //Reset zoom
                        if (!bParentFound)
                        {
                            while (treemapControl1.CanZoomOut())
                            {
                                treemapControl1.ZoomOut();
                            }
                        }
                    }

                    treemapControl1.SelectNode(treeMapNode);
                }

                bSyncingViews = false;
            }
        }

        #endregion

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var aboutBox = new AboutBox1
            {
                StartPosition = FormStartPosition.CenterParent
            };
            aboutBox.ShowDialog(this);
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ButtonRestartAdmin_Click(object sender, EventArgs e)
        {
            ApplicationHelpers.RestartAsAdministrator();
        }

        private static NodeInfo? GetNodeInfoFromToolStripMenu(object sender)
        {
            NodeInfo? node = null;
            if (sender is ToolStripMenuItem toolStripMenuItem)
            {
                if (toolStripMenuItem.Owner is ContextMenuStrip menuStrip)
                {
                    if (menuStrip.SourceControl is TreeView treeView)
                    {
                        var pt = treeView.PointToClient(menuStrip.Location);
                        var item = treeView.HitTest(pt);
                        node = item?.Node?.Tag as NodeInfo?;
                    }
                    else if (menuStrip.SourceControl is TreemapControl treemapControl)
                    {
                        var item = treemapControl.Tag as Node;
                        node = item?.Tag as NodeInfo?;
                        treemapControl.Tag = null;
                    }
                }
            }
            return node;
        }

        private static void StartProcess(string processFullPath, string[] arguments, string? workingDirectory)
        {
            try
            {
                bool bIsAdministrator = ApplicationHelpers.IsAdministrator();
                if (Directory.Exists(workingDirectory) && File.Exists(processFullPath))
                {
                    var psi = new ProcessStartInfo(processFullPath, arguments)
                    {
                        WorkingDirectory = workingDirectory,
                        Verb = bIsAdministrator ? "runas" : string.Empty
                    };
                    Process.Start(psi);
                }
            }
            catch
            { }
        }

        private void OpenInWindowsExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetNodeInfoFromToolStripMenu(sender) is NodeInfo nodeInfo)
            {
                var processPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
                StartProcess(processPath, [nodeInfo.DirFullName ?? string.Empty], nodeInfo.DirFullName);
            }
        }

        private void OpenInCommandPromptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetNodeInfoFromToolStripMenu(sender) is NodeInfo nodeInfo)
            {
                var processPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");
                StartProcess(processPath, [], nodeInfo.DirFullName);
            }
        }

        private void OpenInPowershellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetNodeInfoFromToolStripMenu(sender) is NodeInfo nodeInfo)
            {
                var processPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe");
                StartProcess(processPath, [], nodeInfo.DirFullName);
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        private static extern int SHObjectProperties(nint hwnd, uint shopObjectType, string? pszObjectName, string? pszPropertyPage);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        private void PropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetNodeInfoFromToolStripMenu(sender) is NodeInfo nodeInfo)
            {
                string? DirName = nodeInfo.DirFullName;
                if (Directory.Exists(DirName))
                {
                    _ = SHObjectProperties(nint.Zero, 0x2/*SHOP_FILEPATH*/, DirName, null);
                }
            }
        }

        private void TreemapControl1_NodeMouseUp(object sender, NodeMouseEventArgs nodeMouseEventArgs)
        {
            if (nodeMouseEventArgs.Button == MouseButtons.Right)
            {
                if (nodeMouseEventArgs.Node != null)
                {
                    //This Tag is reset in GetNodeInfoFromToolStripMenu.
                    treemapControl1.Tag = nodeMouseEventArgs.Node;
                    contextMenuStrip1.Show(treemapControl1, nodeMouseEventArgs.Location);
                }
            }
        }
    }
}
