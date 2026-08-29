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
    // Basic IFileSystemEntry interface.
    // It doesn't expose any "Parent" hierarchy property for the purpose of this
    // application, to simplify management in "special" cases (i.e. 'subst' drives).
    public interface IFileSystemEntry
    {
        // File or directory name
        public string Name { get; }

        // File or directory fullname
        public string FullName { get; }

        // File or directory attributes
        public System.IO.FileAttributes Attributes { get; }

        // File size
        public ulong Length { get; }

        // File or directory LinkTarget
        public string? LinkTarget { get; }

        // Get sub-directories (for directories only)
        public IFileSystemEntry[] GetDirectories();

        // Get contained Files (for directories only)
        public IFileSystemEntry[] GetFiles();
    }

    public class NativeFileSystemEntry(System.IO.FileSystemInfo fileSystemInfo) : IFileSystemEntry
    {
        public string Name => _fileSystemInfo.Name;

        public string FullName => _fileSystemInfo.FullName;

        public FileAttributes Attributes => _fileSystemInfo.Attributes;

        public ulong Length => (ulong)(_fileInfo?.Length ?? 0);

        public string? LinkTarget => _fileSystemInfo.LinkTarget;

        //public IFileSystemEntry? Parent =>
        //    _fileInfo?.Directory != null ? new NativeFileSystemEntry(_fileInfo.Directory) :
        //    _directoryInfo?.Parent != null ? new NativeFileSystemEntry(_directoryInfo.Parent) : null;

        public IFileSystemEntry[] GetDirectories() => _directoryInfo?.GetDirectories().Select(d => new NativeFileSystemEntry(d)).ToArray() ?? [];

        public IFileSystemEntry[] GetFiles() => _directoryInfo?.GetFiles().Select(d => new NativeFileSystemEntry(d)).ToArray() ?? [];

        private readonly System.IO.FileSystemInfo _fileSystemInfo = fileSystemInfo;
        private readonly System.IO.DirectoryInfo? _directoryInfo = fileSystemInfo as DirectoryInfo;
        private readonly System.IO.FileInfo? _fileInfo = fileSystemInfo as FileInfo;
    }
}
