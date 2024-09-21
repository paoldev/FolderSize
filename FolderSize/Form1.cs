using System;
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
        }

        private void selectDrive_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGUI(selectDrive.Text);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void UpdateGUI(string sStartDirectory)
        {
            //Int64 iSize = DirectoryParser.GetDirectorySize(selectDrive.Text);

            progressBar1.Value = 0;

            //int iDirCount = DirectoryParser.CountDirectories(sStartDirectory);
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 1;// iDirCount;
            progressBar1.Step = 1;
            progressBar1.Value = 0;
            label1.Text = "0";
            label1.Update();
            label5.Text = "0";
            label5.Update();

            m_info = new MyDirInfo();
            m_info.UpdateSubDirectories(sStartDirectory, progressBar1, label5);

            label1.Text = m_info.m_totalSize.ToString();// iSize.ToString();

            treeView1.Nodes.Clear();

            TreeNode rootNode = treeView1.Nodes.Add(m_info.m_name);

            InsertSubDirs(rootNode, m_info);
        }

        void InsertSubDirs(TreeNode i_node, MyDirInfo i_info)
        {
            if (i_info.m_subDirs != null)
            {
                foreach (MyDirInfo info in i_info.m_subDirs)
                {
                    string name = info.m_name + "       " + (info.m_totalSize / 1024.0 / 1024.0).ToString("F1") + " MB";

                    TreeNode newNode = i_node.Nodes.Add(name);

                    InsertSubDirs(newNode, info);
                }
            }
        }
    }


}
