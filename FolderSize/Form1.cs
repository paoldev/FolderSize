using System;
using System.ComponentModel;
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

                    UpdateTree();
                }
                labelNumFolders.Text = progressBar1.Value.ToString() + " / " + progressBar1.Maximum.ToString();
                //label5.Update();
            });

            tokenSource = new CancellationTokenSource();
            m_info = await MyDirInfo.UpdateSubDirectoriesAsync(sStartDirectory, progress, tokenSource.Token);
            await Task.Run(() => { (progress as IProgress<int>).Report(-1); });
            tokenSource = null;

            timerTaskDuration.Stop();
            UpdateElapsedTime();

            taskIsRunningVM.TaskIsRunning = false;
        }

        private void UpdateTree()
        {
            labelTotalSize.Text = "0";

            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();

            if (m_info != null)
            {
                labelTotalSize.Text = m_info.m_totalSize.ToString();// iSize.ToString();

                uint level = 0;
                var rootNode = CreateNewTreeNode(m_info, level);

                treeView1.Nodes.Add(rootNode);

                InsertSubDirs(rootNode, m_info, level + 1);
            }

            treeView1.EndUpdate();
        }

        private static TreeNode CreateNewTreeNode(MyDirInfo info, uint level)
        {
            string link = !string.IsNullOrEmpty(info.m_linkTo) ? $" ({info.m_linkTo})" : (info.m_reparsepoint ? " (<ReparsePoint>)" : string.Empty);
            string exception = info.m_exception ? " (*)" : "";
            string name = (level == 0) ? info.m_name : (info.m_name + link + exception + "       " + GetSizeAsString(info.m_totalSize));

            var newNode = new TreeNode(name);
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

        private static void InsertSubDirs(TreeNode i_node, MyDirInfo i_info, uint i_level)
        {
            if (i_info.m_subDirs != null)
            {
                foreach (MyDirInfo info in i_info.m_subDirs)
                {
                    var treeNode = CreateNewTreeNode(info, i_level);

                    i_node.Nodes.Add(treeNode);

                    InsertSubDirs(treeNode, info, i_level + 1);
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
    }
}
