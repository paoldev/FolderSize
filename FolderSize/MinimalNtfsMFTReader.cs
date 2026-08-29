//
// FolderSize
//
// Copyright (c) 2012-2026 paoldev
//
// Licensed under the MIT license.
// SPDX-License-Identifier: MIT
//

using System.Runtime.InteropServices;
using System.Text;
using static FolderSize.MyDirInfo;

namespace FolderSize
{
    // Minimal MFT reader, to get only few file system entries' info.
    // See, among others,
    //  https://learn.microsoft.com/en-us/windows/win32/fileio/master-file-table
    //  https://learn.microsoft.com/en-us/windows/win32/api/winioctl/
    //  https://learn.microsoft.com/en-us/windows/win32/devnotes/attribute-record-header
    //  https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-querydosdevicew
    //  https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-getvolumenameforvolumemountpointw
    //  https://learn.microsoft.com/en-us/windows/win32/api/ioapiset/nf-ioapiset-deviceiocontrol
    public static class MinimalNtfsMFTReader
    {
        // Get the MFT file system entry related to filePath.
        // "Administrator" privileges are required.
        // 'filePath' MUST be a valid rooted path.
        // 'null' may also be returned in case of not-Ntfs volumes.
        public static IFileSystemEntry? GetFileSystemEntry(string fileName, IProgress<ProgressValue?> i_progress, ref ProgressValue i_progValue, CancellationToken token)
        {
            // Only rooted paths are valid
            var volumeRoot = Path.GetPathRoot(fileName);
            if (string.IsNullOrWhiteSpace(volumeRoot))
            {
                return null;
            }

            // Resolve 'subst' or UNC paths
            var volumeRootMapping = ResolveRealPath(volumeRoot);
            var realVolumeRoot = Path.GetPathRoot(volumeRootMapping) ?? string.Empty;

            // Get the volume string for the mapped root
            if (!GetVolumeStringFromPath(realVolumeRoot, out var volumeString))
            {
                return null;
            }

            i_progValue.ProgressInfo = $"Pre-scanning {realVolumeRoot}";
            i_progress.Report(i_progValue);

            // Open the volume
            IntPtr hVol = CreateFile(volumeString, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
                                     IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero);
            if (hVol.ToInt64() == -1)
            {
                return null;
            }

            var mftEnum = new MFT_ENUM_DATA_V0
            {
                StartFileReferenceNumber = 0,
                LowUsn = 0,
                HighUsn = long.MaxValue
            };

            var inBuf = new NTFS_FILE_RECORD_INPUT_BUFFER
            {
                FileReferenceNumber = 0
            };

            byte[] buffer = new byte[256 * 1024];
            byte[] frBuffer = new byte[64 * 1024];
            VolumeInfo volInfo = new();
            bool bDeviceIoControlSucceeded = false;

            // See https://learn.microsoft.com/en-us/windows/win32/api/ioapiset/nf-ioapiset-deviceiocontrol
            while (DeviceIoControl(hVol, FSCTL_ENUM_USN_DATA, ref mftEnum, Marshal.SizeOf(mftEnum),
                                   buffer, buffer.Length, out uint bytesReturned, IntPtr.Zero))
            {
                bDeviceIoControlSucceeded = true;
                int offset = sizeof(long); // Skip USN start
                while (offset < bytesReturned)
                {
                    // See USN_RECORD_V2
                    //  https://learn.microsoft.com/en-us/windows/win32/api/winioctl/ns-winioctl-usn_record_v2
                    uint RecordLength = BitConverter.ToUInt32(buffer, offset);
                    //ushort MajorVersion = BitConverter.ToUInt16(buffer, offset + 4);
                    //ushort MinorVersion = BitConverter.ToUInt16(buffer, offset + 6);
                    ulong FileReferenceNumber = BitConverter.ToUInt64(buffer, offset + 8);
                    ulong ParentFileReferenceNumber = BitConverter.ToUInt64(buffer, offset + 16);
                    uint FileAttributes = BitConverter.ToUInt32(buffer, offset + 52);
                    ushort FileNameLength = BitConverter.ToUInt16(buffer, offset + 56);
                    ushort FileNameOffset = BitConverter.ToUInt16(buffer, offset + 58);
                    ulong FileSize = 0;

                    if ((FileAttributes & (uint)System.IO.FileAttributes.Directory) != 0)
                    {
                        i_progValue.TotalDirs++;
                        i_progValue.NumDirs++;
                        i_progress.Report(i_progValue);

                        if (token.IsCancellationRequested)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        inBuf.FileReferenceNumber = FileReferenceNumber;
                        if (DeviceIoControl(hVol, FSCTL_GET_NTFS_FILE_RECORD, ref inBuf, Marshal.SizeOf(inBuf),
                                            frBuffer, frBuffer.Length, out _, IntPtr.Zero))
                        {
                            FileSize = ParseFileSizeFromRecord(frBuffer.AsSpan(12));
                            i_progValue.DirsSize += FileSize;
                        }
                    }

                    MFTFileSystemEntry entry = new()
                    {
                        FRN = FileReferenceNumber,
                        ParentFRN = ParentFileReferenceNumber,
                        Attributes = (System.IO.FileAttributes)FileAttributes,
                        Name = Encoding.Unicode.GetString(buffer, offset + FileNameOffset, FileNameLength),
                        Length = FileSize
                    };

                    // TODO: manage corrupted MFT (i.e. with duplicated FileReferenceNumber entries)
                    volInfo.entries.Add(FileReferenceNumber, entry);
                    offset += (int)RecordLength;
                }
                mftEnum.StartFileReferenceNumber = BitConverter.ToUInt64(buffer, 0);
            }

            CloseHandle(hVol);

            if (!bDeviceIoControlSucceeded)
            {
                // The first DeviceIoControl call may fail on not-Ntfs volumes, although they could be
                // successfully opened (USB FAT drives, CdRoms, etc.)
                return null;
            }

            // Declare the volume root entry.
            MFTFileSystemEntry Root = new()
            {
                Attributes = System.IO.FileAttributes.Directory,   // This field is only used to test if MFTFileSystemEntry is a directory.
                Name = realVolumeRoot,
                FullName = realVolumeRoot
            };

            // Fix dir hierarchy
            foreach (var kv in volInfo.entries)
            {
                if (kv.Key != kv.Value.ParentFRN)   // Just to avoid invalid hierarchy
                {
                    if (!volInfo.entries.TryGetValue(kv.Value.ParentFRN, out var parentEntry))
                    {
                        parentEntry = Root;
                    }
                    if (kv.Value.IsDir)
                    {
                        parentEntry.SubDirs.Add(kv.Value);
                    }
                    else
                    {
                        parentEntry.Files.Add(kv.Value);
                    }
                }
            }

            // Fix FullName for SubDirs, then for Files.
            FixSubDirFullNames(Root, string.Empty);
            foreach (var kv in volInfo.entries)
            {
                if (!kv.Value.IsDir)
                {
                    if (!volInfo.entries.TryGetValue(kv.Value.ParentFRN, out var parentEntry))
                    {
                        parentEntry = Root;
                    }
                    kv.Value.FullName = Path.Join(parentEntry.FullName, kv.Value.Name);
                }
            }

            // Usually 'volumeRootMapping' is the path root, so this call should never fail.
            if (FindEntry(Root, volumeRootMapping) is MFTFileSystemEntry rootMappingEntry)
            {
                // Fix entries FullName to restore 'subst' drives.
                if (!realVolumeRoot.Equals(volumeRoot, StringComparison.OrdinalIgnoreCase))
                {
                    rootMappingEntry.Name = volumeRoot;
                    //rootMappingEntry.Parent = null; // TODO: here detach the Parent directory in case it will be added to IFileSystemEntry interface.
                    FixSubDirAndFileFullNames(rootMappingEntry, string.Empty);
                }
                return FindEntry(rootMappingEntry, fileName);
            }

            // Entry not found?
            return null;
        }

        private static void FixSubDirFullNames(MFTFileSystemEntry dir, string parent)
        {
            dir.FullName = Path.Join(parent, dir.Name);
            foreach (var subdir in dir.SubDirs)
            {
                FixSubDirFullNames(subdir, dir.FullName);
            }
        }

        private static void FixSubDirAndFileFullNames(MFTFileSystemEntry dir, string parent)
        {
            dir.FullName = Path.Join(parent, dir.Name);
            foreach (var file in dir.Files)
            {
                file.FullName = Path.Join(dir.FullName, file.Name);
            }
            foreach (var subdir in dir.SubDirs)
            {
                FixSubDirAndFileFullNames(subdir, dir.FullName);
            }
        }

        private static MFTFileSystemEntry? FindEntry(MFTFileSystemEntry root, string entryToFind)
        {
            var fullpath = Path.GetFullPath(entryToFind);
            var pathComponents = fullpath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).ToList();
            pathComponents.RemoveAt(0);   //The root should already match with first pathComponent.
            MFTFileSystemEntry? entry = root;
            foreach (var pathComponent in pathComponents)
            {
                entry = entry.SubDirs.FirstOrDefault(d => d.Name.Equals(pathComponent, StringComparison.OrdinalIgnoreCase));
                if (entry == null)
                {
                    // early rejection: dir not found?
                    break;
                }
            }
            return entry;
        }

        private class MFTFileSystemEntry : IFileSystemEntry
        {
            // IFileSystemEntry interface
            public string Name { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public System.IO.FileAttributes Attributes { get; set; }
            public ulong Length { get; set; } = 0;
            public string? LinkTarget => IsReparsePoint ?
                (IsDir ? Directory.ResolveLinkTarget(FullName, false)?.ToString() :
                         System.IO.File.ResolveLinkTarget(FullName, false)?.ToString()) : null;
            public IFileSystemEntry[] GetDirectories() => [.. SubDirs];
            public IFileSystemEntry[] GetFiles() => [.. Files];

            // Internal data
            public ulong FRN = 0;
            public ulong ParentFRN = 0;
            public List<MFTFileSystemEntry> SubDirs = [];
            public List<MFTFileSystemEntry> Files = [];

            public bool IsDir => (Attributes & System.IO.FileAttributes.Directory) != 0;
            private bool IsReparsePoint => (Attributes & System.IO.FileAttributes.ReparsePoint) != 0;
        };

        private class VolumeInfo
        {
            public Dictionary<ulong, MFTFileSystemEntry> entries = [];
        }

        private static ulong ParseFileSizeFromRecord(ReadOnlySpan<byte> record)
        {
            // NTFS File Record Header starts at offset 0
            // Attribute list starts at offset 0x14 (AttrOffset)
            // See https://learn.microsoft.com/en-us/windows/win32/devnotes/attribute-record-header
            int attrOffset = BitConverter.ToUInt16(record[0x14..]);
            while (true)
            {
                int attrType = BitConverter.ToInt32(record[attrOffset..]);
                if (attrType == -1) break; // End marker
                int attrLen = BitConverter.ToInt32(record[(attrOffset + 4)..]);
                byte nonResident = record[attrOffset + 8];
                byte nameLen = record[attrOffset + 9];
                if ((attrType == NTFS_ATTR_TYPE_DATA) && (nameLen == 0))    //unnamed data attribute
                {
                    if (nonResident != 0)
                    {
                        // Non-resident: DataSize at offset 0x30 from attr start
                        // This member is not valid if LowestVcn is nonzero.
                        ulong dataSize = BitConverter.ToUInt64(record[(attrOffset + 0x30)..]);  //?????
                        return dataSize;
                    }
                    else
                    {
                        // Resident: ValueLength at offset 0x10 from attr start
                        uint valueLength = BitConverter.ToUInt32(record[(attrOffset + 0x10)..]);
                        return valueLength;
                    }
                }
                attrOffset += attrLen;
            }
            return 0;
        }

        #region Path FullName management
        private static string DevicePathToDriveLetter(string devicePath)
        {
            string[] drives = System.IO.Directory.GetLogicalDrives();

            foreach (string drive in drives)
            {
                StringBuilder targetPath = new(260);
                string driveLetter = drive.TrimEnd(Path.DirectorySeparatorChar);

                if (QueryDosDevice(driveLetter, targetPath, (uint)targetPath.Capacity) > 0)
                {
                    string mapping = targetPath.ToString();
                    if (devicePath.StartsWith(mapping, StringComparison.OrdinalIgnoreCase))
                    {
                        return string.Concat(driveLetter, devicePath.AsSpan(mapping.Length));
                    }
                }
            }

            return devicePath; // No match found
        }

        private static string ResolveRealPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));

            string fullPath = Path.GetFullPath(path);
            string rootPath = Path.GetPathRoot(fullPath)?.TrimEnd(Path.DirectorySeparatorChar) ?? string.Empty;

            if (string.IsNullOrEmpty(rootPath) || rootPath.Length < 2 || rootPath[1] != Path.VolumeSeparatorChar)
                return fullPath; // Not a valid drive path

            string driveLetter = rootPath[..2]; // e.g., "X:"

            // Check if it's a mapped network drive
            StringBuilder networkPath = new(260);
            uint networkPathLen = (uint)networkPath.Capacity;
            if (WNetGetConnection(driveLetter, networkPath, ref networkPathLen) == 0)
            {
                return string.Concat(networkPath.ToString(), fullPath.AsSpan(2));
            }

            // Check for SUBST or physical mapping
            StringBuilder targetPath = new(260);
            if (QueryDosDevice(driveLetter, targetPath, (uint)targetPath.Capacity) == 0)
            {
                return fullPath; // Could not resolve mapping
            }

            string mapping = targetPath.ToString();

            // Remove \??\ prefix if present
            if (mapping.StartsWith(@"\??\"))
            {
                mapping = mapping[4..];
            }

            // If mapping is another drive letter, resolve recursively
            if (mapping.Length >= 2 && mapping[1] == Path.VolumeSeparatorChar)
            {
                string newPath = string.Concat(mapping, fullPath.AsSpan(2));
                return ResolveRealPath(newPath);
            }

            // If mapping is a device path, convert back to drive letter
            if (mapping.StartsWith(@"\Device\", StringComparison.OrdinalIgnoreCase))
            {
                string converted = DevicePathToDriveLetter(mapping);
                return string.Concat(converted, fullPath.AsSpan(2));
            }

            return string.Concat(mapping, fullPath.AsSpan(2));
        }

        private static bool GetVolumeStringFromPath(string path, out string volumeString)
        {
            volumeString = string.Empty;

            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            // Resolve to absolute path
            var fullPath = Path.GetFullPath(path);

            // Get the path mount point
            var volumePath = new StringBuilder(260);
            if (!GetVolumePathName(fullPath, volumePath, (uint)volumePath.Capacity))
            {
                return false;
            }

            // Drive-letter case
            if (volumePath.Length >= 3 && char.IsLetter(volumePath[0]) && volumePath[1] == Path.VolumeSeparatorChar && volumePath[2] == Path.DirectorySeparatorChar)
            {
                volumeString = $@"\\.\{char.ToUpper(volumePath[0])}{Path.VolumeSeparatorChar}";
                return true;
            }

            // UNC or mounted path case; 'subst' drives should be already resolved by 'ResolveRealPath'
            var deviceName = new StringBuilder(260);
            if (QueryDosDevice(volumePath.ToString().TrimEnd(Path.DirectorySeparatorChar), deviceName, (uint)deviceName.Capacity) > 0)
            {
                if (deviceName.ToString().StartsWith(@"\Device\", StringComparison.OrdinalIgnoreCase))
                {
                    var volumeName = new StringBuilder(260);
                    if (GetVolumeNameForVolumeMountPoint(volumePath.ToString(), volumeName, (uint)volumeName.Capacity))
                    {
                        // Convert \\?\Volume{GUID}\ to \\.\Volume{GUID}
                        string vol = volumeName.ToString();
                        if (vol.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
                        {
                            vol = @"\\.\" + vol[4..];
                            vol = vol.TrimEnd(Path.DirectorySeparatorChar);
                            volumeString = vol;
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        #endregion Path FullName management

        #region Native methods
        const uint FSCTL_ENUM_USN_DATA = 0x000900b3;
        const uint FSCTL_GET_NTFS_FILE_RECORD = 0x00090068;
        const uint GENERIC_READ = 0x80000000;
        const uint FILE_SHARE_READ = 0x00000001;
        const uint FILE_SHARE_WRITE = 0x00000002;
        const uint FILE_SHARE_DELETE = 0x00000004;
        const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        const uint OPEN_EXISTING = 3;
        const int NTFS_ATTR_TYPE_DATA = 0x80;

        [StructLayout(LayoutKind.Sequential)]
        struct MFT_ENUM_DATA_V0
        {
            public ulong StartFileReferenceNumber;
            public long LowUsn;
            public long HighUsn;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct NTFS_FILE_RECORD_INPUT_BUFFER
        {
            public ulong FileReferenceNumber;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool GetVolumePathName(string lpszFileName,
            StringBuilder lpszVolumePathName, uint cchBufferLength);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool GetVolumeNameForVolumeMountPoint(string lpszVolumeMountPoint,
            StringBuilder lpszVolumeName, uint cchBufferLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, ref MFT_ENUM_DATA_V0 lpInBuffer,
            int nInBufferSize, byte[] lpOutBuffer, int nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, ref NTFS_FILE_RECORD_INPUT_BUFFER lpInBuffer,
            int nInBufferSize, byte[] lpOutBuffer, int nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, uint ucchMax);

        [DllImport("mpr.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern uint WNetGetConnection(string lpLocalName, StringBuilder lpRemoteName, ref uint lpnLength);
        #endregion Native methods
    }
}
