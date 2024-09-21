using Microsoft.Research.CommunityTechnologies.Treemap;
using System;
using System.ComponentModel;
using System.IO;
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
        }

        private async void SelectDrive_SelectedIndexChanged(object sender, EventArgs e)
        {
            await UpdateGUIAsync(selectDrive.Text);
        }

        private void LabelTotalSize_Click(object sender, EventArgs e)
        {

        }

        private async Task UpdateGUIAsync(string sStartDirectory)
        {
            //Int64 iSize = DirectoryParser.GetDirectorySize(selectDrive.Text);

            taskIsRunningVM.TaskIsRunning = true;

            progressBar1.Value = 0;

            //int iDirCount = DirectoryParser.CountDirectories(sStartDirectory);
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 1;// iDirCount;
            progressBar1.Step = 1;
            progressBar1.Value = 0;
            labelTotalSize.Text = "0";
            labelTotalSize.Update();
            labelNumFolders.Text = "0";
            labelNumFolders.Update();

            taskStart = DateTime.Now;
            timerTaskDuration.Start();

            uint maxLevel = 0;

            var progress = new Progress<int>(addProgressMax =>
            {
                if (addProgressMax >= 0)
                {
                    progressBar1.Maximum += addProgressMax;
                    progressBar1.PerformStep();
                }
                else
                {
                    progressBar1.Value = progressBar1.Maximum;

                    UpdateTrees(maxLevel);
                }
                labelNumFolders.Text = progressBar1.Value.ToString() + " / " + progressBar1.Maximum.ToString();
                //label5.Update();
            });

            tokenSource = new CancellationTokenSource();
            (m_info, maxLevel) = await MyDirInfo.UpdateSubDirectoriesAsync(sStartDirectory, progress, tokenSource.Token);
            await Task.Run(() => { (progress as IProgress<int>).Report(-1); });
            tokenSource = null;

            timerTaskDuration.Stop();
            UpdateElapsedTime();

            taskIsRunningVM.TaskIsRunning = false;
        }

        private void UpdateTrees(uint i_maxLevel)
        {
            labelTotalSize.Text = "0";

            treeView1.BeginUpdate();
            treemapControl1.BeginUpdate();

            treeView1.Nodes.Clear();
            treemapControl1.Clear();

            if (m_info != null)
            {
                labelTotalSize.Text = m_info.m_totalSize.ToString();// iSize.ToString();

                uint level = 0;
                var rootNode = CreateNewTreeNode(m_info, level);
                var treeMapNode = CreateNewTreeMapNode(m_info, level, i_maxLevel);

                rootNode.Tag = treeMapNode;
                treeMapNode.Tag = rootNode;

                treeView1.Nodes.Add(rootNode);
                treemapControl1.Nodes.Add(treeMapNode);

                InsertSubDirs(rootNode, treeMapNode, m_info, level + 1, i_maxLevel);
            }

            treemapControl1.EndUpdate();
            treeView1.EndUpdate();
        }

        private static TreeNode CreateNewTreeNode(MyDirInfo info, uint level)
        {
            string link = !string.IsNullOrEmpty(info.m_linkTo) ? $" ({info.m_linkTo})" : (info.m_reparsepoint ? " (<ReparsePoint>)" : string.Empty);
            string exception = info.m_exception ? " (*)" : "";
            string name = (level == 0) ? info.m_name : (info.m_name + link + exception + "       " + GetSizeAsString(info.m_totalSize));

            var newNode = new TreeNode(name)
            {
                ToolTipText = GetTooltip(info)
            };
            if (info.m_reparsepoint)
            {
                newNode.ForeColor = System.Drawing.Color.Blue;
            }
            else if (info.m_exception)
            {
                newNode.ForeColor = System.Drawing.Color.Red;
            }
            else if (info.m_dummyfolder)
            {
                newNode.ForeColor = System.Drawing.Color.Green;
            }
            return newNode;
        }

        private Node CreateNewTreeMapNode(MyDirInfo info, uint level, uint maxLevel)
        {
            StringBuilder sbTooltip = new();
            sbTooltip.AppendLine(info.m_dummyfolder ? Path.GetDirectoryName(info.m_fullname) : info.m_fullname);
            sbTooltip.AppendLine($"Num files: {info.m_numFiles}");
            sbTooltip.AppendLine($"Files size: {info.m_size}");
            if (!info.m_dummyfolder)
            {
                sbTooltip.AppendLine($"Num directories: {info.m_numDirs}");
                sbTooltip.AppendLine($"Directories size: {info.m_totalSize - info.m_size}");
            }
            return new Node(info.m_name, info.m_totalSize, LevelToColor(level, maxLevel), null, GetTooltip(info));
        }

        private static string GetTooltip(MyDirInfo info)
        {
            StringBuilder sbTooltip = new();
            sbTooltip.AppendLine(info.m_dummyfolder ? Path.GetDirectoryName(info.m_fullname) : info.m_fullname);
            sbTooltip.AppendLine($"Num files: {info.m_numFiles}");
            sbTooltip.AppendLine($"Files size: {info.m_size}");
            if (!info.m_dummyfolder)
            {
                sbTooltip.AppendLine($"Num directories: {info.m_numDirs}");
                sbTooltip.AppendLine($"Directories size: {info.m_totalSize - info.m_size}");
            }
            return sbTooltip.ToString();
        }

        private void InsertSubDirs(TreeNode i_node, Node i_mapnode, MyDirInfo i_info, uint i_level, uint i_maxLevel)
        {
            if (i_info.m_subDirs != null)
            {
                foreach (MyDirInfo info in i_info.m_subDirs)
                {
                    var treeNode = CreateNewTreeNode(info, i_level);
                    var treeMapNode = CreateNewTreeMapNode(info, i_level, i_maxLevel);

                    treeNode.Tag = treeMapNode;
                    treeMapNode.Tag = treeNode;

                    i_node.Nodes.Add(treeNode);
                    i_mapnode.Nodes.Add(treeMapNode);

                    InsertSubDirs(treeNode, treeMapNode, info, i_level + 1, i_maxLevel);
                }
            }
        }

        private static string GetSizeAsString(long size)
        {
            const long OneMB = 1024 * 1024;
            const long OneKB = 1024;
            if (size >= OneMB)
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

        bool bSyncingTrees = false;

        private void TreemapControl1_SelectedNodeChanged(object sender, EventArgs e)
        {
            if (checkBoxSyncTrees.Checked && !bSyncingTrees)
            {
                bSyncingTrees = true;

                if (treemapControl1.SelectedNode is Node node)
                {
                    treeView1.SelectedNode = node.Tag as TreeNode;
                    treeView1.Invalidate();
                }

                bSyncingTrees = false;
            }
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (checkBoxSyncTrees.Checked && !bSyncingTrees)
            {
                bSyncingTrees = true;

                if (e.Node is TreeNode node)
                {
                    var treeMapNode = node.Tag as Node;
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

                bSyncingTrees = false;
            }
        }

        #endregion
    }
}
