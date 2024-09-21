using System;
using System.Collections.Generic;

namespace FolderSize
{
    public class MyDirInfo
    {
        public Int64 m_size;
        public Int64 m_totalSize;
        public string m_name;
        public string m_fullname;

        public List<MyDirInfo> m_subDirs;

        public void UpdateTotalSize()
        {
            m_totalSize = m_size;

            if (m_subDirs != null)
            {
                foreach (MyDirInfo info in m_subDirs)
                {
                    info.UpdateTotalSize();
                    m_totalSize += info.m_totalSize;
                }
            }
        }

        void UpdateProgressText(System.Windows.Forms.ProgressBar i_progress, System.Windows.Forms.Label i_directories)
        {
            i_directories.Text = i_progress.Value.ToString() + " / " + i_progress.Maximum.ToString();
            i_directories.Update();
        }

        void UpdateSubDirectories(System.IO.DirectoryInfo i_pDirInfo, System.Windows.Forms.ProgressBar i_progress, System.Windows.Forms.Label i_directories)
        {
            m_size = 0;

            System.IO.DirectoryInfo[] dirs = null;
            try
            {
                dirs = i_pDirInfo.GetDirectories();
            }
            catch
            {
                return;
            }

            i_progress.Maximum = i_progress.Maximum + dirs.Length;
            i_progress.PerformStep();
            UpdateProgressText(i_progress, i_directories);

            System.IO.FileInfo[] files = i_pDirInfo.GetFiles();

            foreach (System.IO.FileInfo file in files)
            {
                m_size += file.Length;
            }

            if (dirs.Length > 0)
            {
                m_subDirs = new List<MyDirInfo>();
                foreach (System.IO.DirectoryInfo dir in dirs)
                {
                    MyDirInfo info = new MyDirInfo();
                    info.m_fullname = dir.FullName;
                    info.m_name = dir.Name;
                    info.m_size = 0;
                    info.m_totalSize = 0;

                    info.UpdateSubDirectories(dir, i_progress, i_directories);

                    m_subDirs.Add(info);
                }
            }
        }

        public void UpdateSubDirectories(string i_fullname, System.Windows.Forms.ProgressBar i_progress, System.Windows.Forms.Label i_directories)
        {
            m_name = i_fullname;
            m_fullname = i_fullname;
            m_size = 0;
            m_totalSize = 0;

            try
            {
                System.IO.DirectoryInfo pDir = new System.IO.DirectoryInfo(i_fullname);
                UpdateSubDirectories(pDir, i_progress, i_directories);
                UpdateTotalSize();
                SortByTotalSize();

                i_progress.Value = i_progress.Maximum;
                UpdateProgressText(i_progress, i_directories);
            }
            catch
            {
            }
        }

        public void SortByTotalSize()
        {
            if (m_subDirs != null)
            {
                MyComparer comp = new MyComparer();
                m_subDirs.Sort(comp);

                foreach (MyDirInfo dir in m_subDirs)
                {
                    dir.SortByTotalSize();
                }
            }
        }
    };

    public class MyComparer : IComparer<MyDirInfo>
    {
        public int Compare(MyDirInfo x, MyDirInfo y)
        {
            if (x.m_totalSize != y.m_totalSize)
            {
                return (x.m_totalSize > y.m_totalSize) ? -1 : 1;
            }

            return x.m_fullname.CompareTo(y.m_fullname);
        }
    }

    class DirectoryParser
    {
        public static int CountDirectories(string i_sDirectory)
        {
            int iNumDirs = 0;

            try
            {
                System.IO.DirectoryInfo pDir = new System.IO.DirectoryInfo(i_sDirectory);
                iNumDirs = 1;

                System.IO.DirectoryInfo[] dirs = pDir.GetDirectories();
                foreach (System.IO.DirectoryInfo dir in dirs)
                {
                    iNumDirs += CountDirectories(dir.FullName);
                }
            }
            catch
            {
            }

            return iNumDirs;
        }

        public static Int64 GetDirectorySize(string i_sDirectory)
        {
            Int64 iFileSize = 0;

            try
            {
                System.IO.DirectoryInfo pDir = new System.IO.DirectoryInfo(i_sDirectory);
                System.IO.FileInfo[] files = pDir.GetFiles();

                foreach (System.IO.FileInfo file in files)
                {
                    iFileSize += file.Length;
                }

                System.IO.DirectoryInfo[] dirs = pDir.GetDirectories();
                foreach (System.IO.DirectoryInfo dir in dirs)
                {
                    iFileSize += GetDirectorySize(dir.FullName);
                }
            }
            catch
            {
            }

            return iFileSize;
        }
    }
}
