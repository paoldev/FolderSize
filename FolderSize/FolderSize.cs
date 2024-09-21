using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FolderSize
{
    public class MyDirInfo
    {
        public Int64 m_size = 0;
        public Int64 m_totalSize = 0;
        public Int64 m_numFiles = 0;
        public Int64 m_numDirs = 0;
        public string m_name;
        public string m_fullname;
        public string m_linkTo = null;
        public bool m_dummyfolder = false;
        public bool m_exception = false;
        public bool m_reparsepoint = false;

        public List<MyDirInfo> m_subDirs;

        private MyDirInfo() { }

        private void UpdateTotalSize()
        {
            m_totalSize = m_size;

            if (m_subDirs != null)
            {
                foreach (MyDirInfo info in m_subDirs)
                {
                    info.UpdateTotalSize();
                    if (!info.m_dummyfolder)
                    {
                    	m_totalSize += info.m_totalSize;
                	}
                }
            }
        }


        private void UpdateSubDirectories(System.IO.DirectoryInfo i_pDirInfo, IProgress<int> i_progress, CancellationToken token)
        {
			m_size = 0;
            m_numFiles = 0;
            m_numDirs = 0;
            m_dummyfolder = false;
            m_exception = false;
            m_linkTo = i_pDirInfo.LinkTarget;
            m_reparsepoint = (i_pDirInfo.Attributes & System.IO.FileAttributes.ReparsePoint) != 0;
            if (m_reparsepoint)
            {
                return;
            }

            System.IO.DirectoryInfo[] dirs = null;
            try
            {
                dirs = i_pDirInfo.GetDirectories();
            }
            catch
            {
                m_exception = true;
                return;
            }

            m_numDirs = dirs.Length;

            i_progress.Report(dirs.Length);

            token.ThrowIfCancellationRequested();

            System.IO.FileInfo[] files = i_pDirInfo.GetFiles();

			m_numFiles = files.Length;
            foreach (System.IO.FileInfo file in files)
            {
                m_size += file.Length;
            }

            m_subDirs = new List<MyDirInfo>();
            if (m_size > 0)
            {
                //Dummy directory containing total file size.
                MyDirInfo info = new MyDirInfo();
                info.m_name = "<files>";
                info.m_fullname = System.IO.Path.Combine(i_pDirInfo.FullName, info.m_name);
                info.m_size = m_size;
                info.m_totalSize = m_size;
                info.m_numFiles = m_numFiles;
                info.m_dummyfolder = true;

                m_subDirs.Add(info);
            }

            if (dirs.Length > 0)
            {
                foreach (System.IO.DirectoryInfo dir in dirs)
                {
                    MyDirInfo info = new MyDirInfo();
                    info.m_fullname = dir.FullName;
                    info.m_name = dir.Name;
                    info.m_size = 0;
                    info.m_totalSize = 0;

                    info.UpdateSubDirectories(dir, i_progress, token);

                    m_subDirs.Add(info);
                }
            }
        }

        public static Task<MyDirInfo> UpdateSubDirectoriesAsync(string i_fullname, IProgress<int> i_progress, CancellationToken token)
        {
            return Task.Run(() =>
            {
                var info = new MyDirInfo()
                {
                    m_name = i_fullname,
                    m_fullname = i_fullname,
                    m_size = 0,
                    m_totalSize = 0
                };

                try
                {
                    System.IO.DirectoryInfo pDir = new System.IO.DirectoryInfo(i_fullname);
                    info.UpdateSubDirectories(pDir, i_progress, token);
                    info.UpdateTotalSize();
                    info.SortByTotalSize();
                }
                catch (OperationCanceledException)
                {
                    //Update some infos
                    info.UpdateTotalSize();
                    info.SortByTotalSize();
                }
                catch
                {
                }

                return info;
            });
        }

        private void SortByTotalSize()
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
