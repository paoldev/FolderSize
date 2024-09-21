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

        private void UpdateSubDirectories(System.IO.DirectoryInfo i_pDirInfo, IProgress<int> i_progress, uint i_level, ref uint o_maxLevel, CancellationToken token)
        {
            o_maxLevel = Math.Max(i_level, o_maxLevel);
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

            System.IO.DirectoryInfo[] dirs;
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

            m_subDirs = [];
            if (m_size > 0)
            {
                //Dummy directory containing total file size.
                const string dummyFolderName = "<files>";
                MyDirInfo info = new()
                {
                    m_name = dummyFolderName,
                    m_fullname = System.IO.Path.Combine(i_pDirInfo.FullName, dummyFolderName),
                    m_size = m_size,
                    m_totalSize = m_size,
                    m_numFiles = m_numFiles,
                    m_dummyfolder = true
                };

                m_subDirs.Add(info);
            }

            if (dirs.Length > 0)
            {
                foreach (System.IO.DirectoryInfo dir in dirs)
                {
                    MyDirInfo info = new()
                    {
                        m_fullname = dir.FullName,
                        m_name = dir.Name,
                        m_size = 0,
                        m_totalSize = 0
                    };

                    info.UpdateSubDirectories(dir, i_progress, i_level + 1, ref o_maxLevel, token);

                    m_subDirs.Add(info);
                }
            }
        }

        public static Task<(MyDirInfo, uint)> UpdateSubDirectoriesAsync(string i_fullname, IProgress<int> i_progress, CancellationToken token)
        {
            return Task.Run(() =>
            {
                uint maxLevel = 1;

                var info = new MyDirInfo()
                {
                    m_name = i_fullname,
                    m_fullname = i_fullname,
                    m_size = 0,
                    m_totalSize = 0
                };

                try
                {
                    uint level = 1;

                    System.IO.DirectoryInfo pDir = new(i_fullname);
                    info.UpdateSubDirectories(pDir, i_progress, level, ref maxLevel, token);
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

                return (info, maxLevel);
            });
        }

        private void SortByTotalSize()
        {
            if (m_subDirs != null)
            {
                MyComparer comp = new();
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
}
