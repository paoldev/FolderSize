using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FolderSize
{
    public class MyDirInfo
    {
        public Int64 DirFileSize = 0;
        public Int64 SubDirsFileSize = 0;
        public Int64 TotalFileSize => DirFileSize + SubDirsFileSize;
        public Int64 NumFiles = 0;
        public Int64 NumDirs = 0;
        public string Name;
        public string FullName;
        public string LinkTarget = null;
        public bool IsDummyFolder = false;
        public bool HasException = false;
        public bool IsReparsePoint = false;

        public List<MyDirInfo> SubDirs = [];

        public struct ProgressValue
        {
            public int NumDirs;
            public int TotalDirs;
            public Int64 DirsSize;
        };

        private MyDirInfo() { }

        private static MyDirInfo GetDirectoryInfo(System.IO.DirectoryInfo i_dirInfo, IProgress<ProgressValue?> i_progress, ref ProgressValue i_progValue, uint i_level, ref uint o_maxLevel, CancellationToken token)
        {
            o_maxLevel = Math.Max(i_level, o_maxLevel);

            var newInfo = new MyDirInfo()
            {
                FullName = i_dirInfo.FullName,
                Name = i_dirInfo.Name,
                DirFileSize = 0,
                SubDirsFileSize = 0,
                NumFiles = 0,
                NumDirs = 0,
                SubDirs = [],
                IsDummyFolder = false,
                HasException = false,
                LinkTarget = i_dirInfo.LinkTarget,
                IsReparsePoint = (i_dirInfo.Attributes & System.IO.FileAttributes.ReparsePoint) != 0
            };

            if (newInfo.IsReparsePoint)
            {
                return newInfo;
            }

            System.IO.DirectoryInfo[] dirs;
            System.IO.FileInfo[] files;
            try
            {
                dirs = i_dirInfo.GetDirectories();
                files = i_dirInfo.GetFiles();
            }
            catch
            {
                newInfo.HasException = true;
                return newInfo;
            }

            newInfo.NumDirs = dirs.Length;
            newInfo.NumFiles = files.Length;
            foreach (System.IO.FileInfo file in files)
            {
                newInfo.DirFileSize += file.Length;
            }

            //TotalDirs is also counting subfolders which are reparse points or that may be unaccessible,
            //but this value is just a guess for the total number of enumerated directories.
            i_progValue.TotalDirs += dirs.Length;
            i_progValue.NumDirs++;
            i_progValue.DirsSize += newInfo.DirFileSize;
            i_progress.Report(i_progValue);

            //token.ThrowIfCancellationRequested();
            if (token.IsCancellationRequested)
            {
                return newInfo;
            }

            if (newInfo.DirFileSize > 0)
            {
                //Dummy directory containing total file size.
                const string dummyFolderName = "<files>";
                MyDirInfo subInfo = new()
                {
                    Name = dummyFolderName,
                    FullName = System.IO.Path.Combine(i_dirInfo.FullName, dummyFolderName),
                    DirFileSize = newInfo.DirFileSize,
                    SubDirsFileSize = 0,
                    NumFiles = newInfo.NumFiles,
                    NumDirs = 0,
                    IsDummyFolder = true
                };

                newInfo.SubDirs.Add(subInfo);
            }

            foreach (System.IO.DirectoryInfo dir in dirs)
            {
                MyDirInfo subInfo = GetDirectoryInfo(dir, i_progress, ref i_progValue, i_level + 1, ref o_maxLevel, token);

                newInfo.SubDirsFileSize += subInfo.TotalFileSize;

                newInfo.SubDirs.Add(subInfo);
            }

            // Sort SubDirs by TotalFileSize, then by FullName.
            newInfo.SubDirs.Sort((x, y) =>
            {
                if (x.TotalFileSize != y.TotalFileSize)
                {
                    return (x.TotalFileSize > y.TotalFileSize) ? -1 : 1;
                }

                return x.FullName.CompareTo(y.FullName);
            });

            return newInfo;
        }

        public static Task<(MyDirInfo, uint)> GetDirectoryInfoAsync(string i_fullname, IProgress<ProgressValue?> i_progress, CancellationToken token)
        {
            return Task.Run(() =>
            {
                MyDirInfo info = null;
                uint maxLevel = 1;

                try
                {
                    uint level = 1;

                    ProgressValue progValue = new()
                    {
                        NumDirs = 0,
                        TotalDirs = 1,
                        DirsSize = 0
                    };
                    System.IO.DirectoryInfo dirInfo = new(i_fullname);
                    info = GetDirectoryInfo(dirInfo, i_progress, ref progValue, level, ref maxLevel, token);
                }
                catch
                {
                }

                return (info, maxLevel);
            });
        }
    };
}
