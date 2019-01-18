using System;
using System.Runtime.InteropServices;

namespace XLPakTool
{
    public class XLPack
    {
        [DllImport("xlpack.dll", EntryPoint = "?ApplyPatchPak@@YA_NPBD0@Z", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ApplyPatchPak([MarshalAs(UnmanagedType.LPStr)] string s1, [MarshalAs(UnmanagedType.LPStr)] string s2);

        [DllImport("xlpack.dll", EntryPoint = "?Copy@@YA_NPBD0@Z", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Copy([MarshalAs(UnmanagedType.LPStr)] string from, [MarshalAs(UnmanagedType.LPStr)] string to);

        [DllImport("xlpack.dll", EntryPoint = "?CopyDir@@YA_NPBD0@Z", CharSet = CharSet.Ansi)]
        public static extern bool CopyDir([MarshalAs(UnmanagedType.LPStr)] string from, [MarshalAs(UnmanagedType.LPStr)] string to);

        [DllImport("xlpack.dll", EntryPoint = "?CreateFileSystem@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CreateFileSystem();

        [DllImport("xlpack.dll", EntryPoint = "?DestroyFileLogHandler@@YAXPAX@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyFileLogHandler(IntPtr lp1);

        [DllImport("xlpack.dll", EntryPoint = "?DestroyFileSystem@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyFileSystem();

        [DllImport("xlpack.dll", EntryPoint = "?FDelete@@YA_NPBD@Z", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool FDelete([MarshalAs(UnmanagedType.LPStr)] string where);

        [DllImport("xlpack.dll", EntryPoint = "?FindClose@@YAHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern int FindClose(int i);

        [DllImport("xlpack.dll", EntryPoint = "?FindFirst@@YAHPBDPAUafs_finddata@@@Z", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int FindFirst([MarshalAs(UnmanagedType.LPStr)] string file, ref afs_finddata fd);

        [DllImport("xlpack.dll", EntryPoint = "?FindNext@@YAHHPAUafs_finddata@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern int FindNext(int i, ref afs_finddata fd);

        [DllImport("xlpack.dll", EntryPoint = "?GetFileName@@YAPBDPBUafs_finddata@@@Z", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetFileName(ref afs_finddata fd);

        [DllImport("xlpack.dll", EntryPoint = "?IsDirectory@@YA_NPBUafs_finddata@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsDirectory(ref afs_finddata fd);

        [DllImport("xlpack.dll", EntryPoint = "?IsFileExist@@YA_NPBD@Z", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsFileExist([MarshalAs(UnmanagedType.LPStr)] string file);

        [DllImport("xlpack.dll", EntryPoint = "?Mount@@YAPAXPBD0_N@Z", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Mount([MarshalAs(UnmanagedType.LPStr)] string where, [MarshalAs(UnmanagedType.LPStr)] string which,
            [MarshalAs(UnmanagedType.Bool)] bool editable);
        
        [DllImport("xlpack.dll", EntryPoint = "?Unmount@@YA_NPBD@Z", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Unmount([MarshalAs(UnmanagedType.LPStr)] string where);
        
        [DllImport("xlpack.dll", EntryPoint = "?Unmount@@YA_NPAX@Z", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Unmount(IntPtr handler);

        [DllImport("xlpack.dll", EntryPoint = "?SetFileLogHandler@@YAPAXPBDP6AX0ZZ@Z", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SetFileLogHandler([MarshalAs(UnmanagedType.LPStr)] string s, Func f);

        [DllImport("xlpack.dll", EntryPoint = "?DeleteDir@@YA_NPBD@Z", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DeleteDir([MarshalAs(UnmanagedType.LPStr)] string path);
        
        [DllImport("xlpack.dll", EntryPoint = "?FOpen@@YAPAUFile@@PBD0@Z", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr FOpen([MarshalAs(UnmanagedType.LPStr)] string path, [MarshalAs(UnmanagedType.LPStr)] string mode);
        
        [DllImport("xlpack.dll", EntryPoint = "?FClose@@YAXAAPAUFile@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FClose(ref IntPtr filePosition);
        
        [DllImport("xlpack.dll", EntryPoint = "?FGetStat@@YA_NPAUFile@@PAUpack_stat_t@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool FGetStat(IntPtr filePosition, ref pack_stat_t stat);
        
        [DllImport("xlpack.dll", EntryPoint = "?FGetStat@@YA_NPAUFile@@PAUpack_stat2@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool FGetStat(IntPtr filePosition, ref pack_stat2 stat);
        
        [DllImport("xlpack.dll", EntryPoint = "?FSize@@YA_JPAUFile@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern long FSize(IntPtr filePosition);
        
        [StructLayout(LayoutKind.Explicit)]
        public struct afs_finddata
        {
            [FieldOffset(0)] public long Offset;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct File
        {
            [FieldOffset(0)] public uint pntr;
            [FieldOffset(4)] public uint cnt;
            [FieldOffset(8)] public uint based; // base
            [FieldOffset(12)] public uint flag;
            [FieldOffset(16)] public uint file;
            [FieldOffset(20)] public uint charbuf;
            [FieldOffset(24)] public uint bufsize;
            [FieldOffset(28)] public uint tmpfname;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct XlFileInfo
        {
            [FieldOffset(0)] public uint dwFileAttributes;
            [FieldOffset(4)] public ulong ftCreationTime;
            [FieldOffset(12)] public ulong ftLastAccessTime;
            [FieldOffset(20)] public ulong ftLastWriteTime;
            [FieldOffset(28)] public uint dwVolumeSerialNumber;
            [FieldOffset(32)] public uint nFileSizeHigh;
            [FieldOffset(36)] public uint nFileSizeLow;
            [FieldOffset(40)] public uint nNumberOfLinks;
            [FieldOffset(44)] public uint nFileIndexHigh;
            [FieldOffset(48)] public uint nFileIndexLow;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct afs_md5_ctx
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] [FieldOffset(0)]
            public byte[] md5;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct pack_stat_t
        {
            [FieldOffset(0)] public long creationTime;
            [FieldOffset(8)] public long modifiedTime;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct pack_stat2
        {
            [FieldOffset(0)] public pack_stat_t stat;
            [FieldOffset(16)] public long length;
            [FieldOffset(24)] public afs_md5_ctx digest;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void Func(params string[] values);
    }
}
