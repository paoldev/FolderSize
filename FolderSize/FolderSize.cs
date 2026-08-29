//
// FolderSize
//
// Copyright (c) 2012-2026 paoldev
//
// Licensed under the MIT license.
// SPDX-License-Identifier: MIT
//

namespace FolderSize
{
    public class MyDirInfo
    {
        public UInt64 DirFileSize = 0;
        public UInt64 SubDirsFileSize = 0;
        public UInt64 TotalFileSize => DirFileSize + SubDirsFileSize;
        public Int64 NumFiles = 0;
        public Int64 NumDirs = 0;
        public string Name = string.Empty;
        public string FullName = string.Empty;
        public string? LinkTarget = null;
        public bool IsDummyFolder = false;
        public bool HasException = false;
        public bool IsReparsePoint = false;

        public List<MyDirInfo> SubDirs = [];

        private static readonly string DummyFilesFolder = "<Files>";

        public struct ProgressValue
        {
            public int NumDirs;
            public int TotalDirs;
            public UInt64 DirsSize;
            public string ProgressInfo;

            public void Reset()
            {
                NumDirs = 0;
                TotalDirs = 1;
                DirsSize = 0;
                ProgressInfo = string.Empty;
            }
        };

        private MyDirInfo() { }

        private static MyDirInfo GetDirectoryInfo(IFileSystemEntry i_dirInfo, IProgress<ProgressValue?> i_progress, ref ProgressValue i_progValue, uint i_level, ref uint o_maxLevel, CancellationToken token)
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

            IFileSystemEntry[] dirs;
            IFileSystemEntry[] files;
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
            foreach (var file in files)
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
                MyDirInfo subInfo = new()
                {
                    Name = DummyFilesFolder,
                    FullName = System.IO.Path.Join(i_dirInfo.FullName, DummyFilesFolder),
                    DirFileSize = newInfo.DirFileSize,
                    SubDirsFileSize = 0,
                    NumFiles = newInfo.NumFiles,
                    NumDirs = 0,
                    IsDummyFolder = true
                };

                newInfo.SubDirs.Add(subInfo);
            }

            foreach (var dir in dirs)
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

        public static Task<(MyDirInfo?, uint)> GetDirectoryInfoAsync(string i_fullname, bool bTryFastOption, IProgress<ProgressValue?> i_progress, CancellationToken token)
        {
            return Task.Run(() =>
            {
                MyDirInfo? info = null;
                uint maxLevel = 1;

                try
                {
                    ProgressValue progValue = new();
                    if (bTryFastOption)
                    {
                        progValue.Reset();
                        progValue.ProgressInfo = $"Pre-scanning {i_fullname}";
                        i_progress.Report(progValue);
                        try
                        {
                            IFileSystemEntry? fileEntry = MinimalNtfsMFTReader.GetFileSystemEntry(i_fullname, i_progress, ref progValue, token);
                            if (fileEntry != null)
                            {
                                // Reset the counters
                                progValue.Reset();
                                progValue.ProgressInfo = $"Scanning {fileEntry.FullName}";
                                i_progress.Report(progValue);
                                info = GetDirectoryInfo(fileEntry, i_progress, ref progValue, i_level: 1, ref maxLevel, token);

                                // Force GC
                                fileEntry = null;
                                GC.Collect();
                            }
                        }
                        catch
                        {
                            info = null;
                        }
                    }
                    if (info == null)
                    {
                        maxLevel = 1;
                        progValue.Reset();
                        progValue.ProgressInfo = $"Scanning {i_fullname}";
                        i_progress.Report(progValue);
                        System.IO.DirectoryInfo dirInfo = new(i_fullname);
                        info = GetDirectoryInfo(new NativeFileSystemEntry(dirInfo), i_progress, ref progValue, i_level: 1, ref maxLevel, token);
                    }
                }
                catch
                {
                }

                return (info, maxLevel);
            });
        }
    };
}
