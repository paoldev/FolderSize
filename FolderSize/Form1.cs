using Microsoft.Research.CommunityTechnologies.Treemap;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FolderSize
{
    public partial class Form1 : Form
    {
        MyDirInfo m_info;

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

            bool bIsAdministator = IsAdministrator();
            labelAdmin.Visible = bIsAdministator;
            buttonRestartAdmin.Visible = !bIsAdministator;
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

            uint maxLevel = 0;

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
                    }
                }
                else
                {
                    progressBar1.Value = progressBar1.Maximum;
                    labelTotalSize.Text = (m_info == null) ? "0" : $"{GetNumStringWithSep(m_info.TotalFileSize)} bytes ({GetSizeAsShortString(m_info.TotalFileSize)})";
                    labelNumFolders.Text = $"{GetNumStringWithSep(progressBar1.Value)} / {GetNumStringWithSep(progressBar1.Maximum)}";

                    UpdateTrees(maxLevel);
                }
            });

            tokenSource = new CancellationTokenSource();
            (m_info, maxLevel) = await MyDirInfo.GetDirectoryInfoAsync(sStartDirectory, progress, tokenSource.Token);
            await Task.Run(() => { progress.Report(null); });
            tokenSource = null;

            timerTaskDuration.Stop();
            UpdateElapsedTime();

            taskIsRunningVM.TaskIsRunning = false;
        }

        private readonly struct NodeInfo(MyDirInfo dirInfo, TreeNode treeNode, Node treeMapNode)
        {
            public readonly MyDirInfo DirInfo = dirInfo;
            public readonly TreeNode TreeNode = treeNode;
            public readonly Node TreeMapNode = treeMapNode;
        };

        private void UpdateTrees(uint i_maxLevel)
        {
            treeView1.BeginUpdate();
            treemapControl1.BeginUpdate();

            treeView1.Nodes.Clear();
            treemapControl1.Clear();

            treeView1.Tag = null;
            treemapControl1.Tag = null;

            if (m_info != null)
            {
                uint level = 0;
                var rootNode = CreateNewTreeNode(m_info, level);
                var treeMapNode = CreateNewTreeMapNode(m_info, level, i_maxLevel);

                NodeInfo nodeInfo = new(m_info, rootNode, treeMapNode);
                rootNode.Tag = nodeInfo;
                treeMapNode.Tag = nodeInfo;

                InsertSubDirs(rootNode, treeMapNode, m_info, level + 1, i_maxLevel);

                treeView1.Nodes.Add(rootNode);
                treemapControl1.Nodes.Add(treeMapNode);

                rootNode.Expand();
            }

            treemapControl1.EndUpdate();
            treeView1.EndUpdate();
        }

        private TreeNode CreateNewTreeNode(MyDirInfo info, uint level)
        {
            string link = !string.IsNullOrEmpty(info.LinkTarget) ? $" ({info.LinkTarget})" : (info.IsReparsePoint ? " (<ReparsePoint>)" : string.Empty);
            string exception = info.HasException ? " (*)" : "";
            string name = (level == 0) ? info.Name : (info.Name + link + exception + "       " + GetSizeAsShortString(info.TotalFileSize));

            var newNode = new TreeNode(name)
            {
                ToolTipText = GetTooltip(info),
                ContextMenuStrip = contextMenuStrip1
            };
            if (info.IsReparsePoint)
            {
                newNode.ForeColor = System.Drawing.Color.Blue;
            }
            else if (info.HasException)
            {
                newNode.ForeColor = System.Drawing.Color.Red;
            }
            else if (info.IsDummyFolder)
            {
                newNode.ForeColor = System.Drawing.Color.Green;
            }
            return newNode;
        }

        private Node CreateNewTreeMapNode(MyDirInfo info, uint level, uint maxLevel)
        {
            return new Node(info.Name, info.TotalFileSize, LevelToColor(level, maxLevel), null, GetTooltip(info));
        }

        private static string GetTooltip(MyDirInfo info)
        {
            StringBuilder sbTooltip = new();
            sbTooltip.AppendLine(info.IsDummyFolder ? Path.GetDirectoryName(info.FullName) : info.FullName);
            sbTooltip.AppendLine($"Num files: {GetNumStringWithSep(info.NumFiles)}");
            sbTooltip.AppendLine($"Files size: {GetNumStringWithSep(info.DirFileSize)} bytes");
            if (!info.IsDummyFolder)
            {
                sbTooltip.AppendLine($"Num directories: {GetNumStringWithSep(info.NumDirs)}");
                sbTooltip.AppendLine($"Directories size: {GetNumStringWithSep(info.SubDirsFileSize)} bytes");
            }
            return sbTooltip.ToString();
        }

        private void InsertSubDirs(TreeNode i_node, Node i_mapnode, MyDirInfo i_info, uint i_level, uint i_maxLevel)
        {
            if (i_info.SubDirs != null)
            {
                foreach (MyDirInfo info in i_info.SubDirs)
                {
                    var treeNode = CreateNewTreeNode(info, i_level);
                    var treeMapNode = CreateNewTreeMapNode(info, i_level, i_maxLevel);

                    NodeInfo nodeInfo = new(info, treeNode, treeMapNode);
                    treeNode.Tag = nodeInfo;
                    treeMapNode.Tag = nodeInfo;

                    i_node.Nodes.Add(treeNode);
                    i_mapnode.Nodes.Add(treeMapNode);

                    InsertSubDirs(treeNode, treeMapNode, info, i_level + 1, i_maxLevel);
                }
            }
        }

        // Although only English UI is available, use CurrentCulture to get thousands' separator.
        private static string GetNumStringWithSep<T>(T v) => string.Format(CultureInfo.CurrentCulture, "{0:##,0}", v);

        private static string GetSizeAsShortString(long size)
        {
            const long OneGB = 1024 * 1024 * 1024;
            const long OneMB = 1024 * 1024;
            const long OneKB = 1024;
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

        CancellationTokenSource tokenSource = null;
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

            public event PropertyChangedEventHandler PropertyChanged;
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
            labelElapsedTime.Text = (DateTime.Now - taskStart).ToString();
        }

        #region Treemap control

        private float LevelToColor(uint level, uint max_level)
        {
            float min = 0.0f;// treemapControl1.MinColorMetric;
            float max = treemapControl1.MaxColorMetric;

            if ((level <= 0) || (max_level <= 1))
            {
                return min;
            }
            else if (level >= max_level)
            {
                return max;
            }
            else
            {
                return min + level * (max - min) / max_level;
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

        #region Administrator

        private void ButtonRestartAdmin_Click(object sender, EventArgs e)
        {
            ProcessStartInfo currentStartInfo = new(Application.ExecutablePath, Environment.GetCommandLineArgs()[1..])
            {
                Verb = "runas",
                UseShellExecute = true
            };

            Application.Exit();
            try
            {
                // Operation may be cancelled by the user, in which case a Win32Exception is triggered.
                Process.Start(currentStartInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static bool IsAdministrator()
        {
            using var winIdentity = WindowsIdentity.GetCurrent();
            var winPrincipal = new WindowsPrincipal(winIdentity);
            return winPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        #endregion

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

        private void OpenInWindowsExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetNodeInfoFromToolStripMenu(sender) is NodeInfo nodeInfo)
            {
                string DirName = nodeInfo.DirInfo.IsDummyFolder ?
                    Path.GetDirectoryName(nodeInfo.DirInfo.FullName) : nodeInfo.DirInfo.FullName;
                if (Directory.Exists(DirName))
                {
                    var psi = new ProcessStartInfo("explorer.exe", DirName)
                    {
                        WorkingDirectory = DirName
                    };
                    Process.Start(psi);
                }
            }
        }

        private void OpenInCommandPromptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetNodeInfoFromToolStripMenu(sender) is NodeInfo nodeInfo)
            {
                string DirName = nodeInfo.DirInfo.IsDummyFolder ?
                    Path.GetDirectoryName(nodeInfo.DirInfo.FullName) : nodeInfo.DirInfo.FullName;
                if (Directory.Exists(DirName))
                {
                    var psi = new ProcessStartInfo("cmd.exe")
                    {
                        WorkingDirectory = DirName
                    };
                    Process.Start(psi);
                }
            }
        }

        private void OpenInPowershellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetNodeInfoFromToolStripMenu(sender) is NodeInfo nodeInfo)
            {
                string DirName = nodeInfo.DirInfo.IsDummyFolder ?
                    Path.GetDirectoryName(nodeInfo.DirInfo.FullName) : nodeInfo.DirInfo.FullName;
                if (Directory.Exists(DirName))
                {
                    var psi = new ProcessStartInfo("powershell.exe")
                    {
                        WorkingDirectory = DirName
                    };
                    Process.Start(psi);
                }
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        private static extern int SHObjectProperties(nint hwnd, uint shopObjectType, string pszObjectName, string pszPropertyPage);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        private void PropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GetNodeInfoFromToolStripMenu(sender) is NodeInfo nodeInfo)
            {
                string DirName = nodeInfo.DirInfo.IsDummyFolder ?
                    Path.GetDirectoryName(nodeInfo.DirInfo.FullName) : nodeInfo.DirInfo.FullName;
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
