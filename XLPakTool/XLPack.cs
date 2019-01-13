using System;
using System.Runtime.InteropServices;

namespace XLPakTool
{
    internal class XLPack
    {
        [DllImport("xlpack.dll", EntryPoint = "?ApplyPatchPak@@YA_NPBD0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ApplyPatchPak([MarshalAs(UnmanagedType.LPStr)] string s1, [MarshalAs(UnmanagedType.LPStr)] string s2);

        [DllImport("xlpack.dll", EntryPoint = "?Copy@@YA_NPBD0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Copy([MarshalAs(UnmanagedType.LPStr)] string from, [MarshalAs(UnmanagedType.LPStr)] string to);

        [DllImport("xlpack.dll", EntryPoint = "?CopyDir@@YA_NPBD0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CopyDir([MarshalAs(UnmanagedType.LPStr)] string from, [MarshalAs(UnmanagedType.LPStr)] string to);

        [DllImport("xlpack.dll", EntryPoint = "?CreateFileSystem@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CreateFileSystem();

        [DllImport("xlpack.dll", EntryPoint = "?DestroyFileLogHandler@@YAXPAX@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyFileLogHandler(IntPtr lp1);

        [DllImport("xlpack.dll", EntryPoint = "?DestroyFileSystem@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyFileSystem();

        [DllImport("xlpack.dll", EntryPoint = "?FDelete@@YA_NPBD@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool FDelete([MarshalAs(UnmanagedType.LPStr)] string where);

        [DllImport("xlpack.dll", EntryPoint = "?FindClose@@YAHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern int FindClose(int i);

        [DllImport("xlpack.dll", EntryPoint = "?FindFirst@@YAHPBDPAUafs_finddata@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern int FindFirst([MarshalAs(UnmanagedType.LPStr)] string file, ref afs_finddata fd);

        [DllImport("xlpack.dll", EntryPoint = "?FindNext@@YAHHPAUafs_finddata@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern int FindNext(int i, ref afs_finddata fd);

        [DllImport("xlpack.dll", EntryPoint = "?GetFileName@@YAPBDPBUafs_finddata@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetFileName(ref afs_finddata fd);

        [DllImport("xlpack.dll", EntryPoint = "?IsDirectory@@YA_NPBUafs_finddata@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern char IsDirectory(ref afs_finddata fd);

        [DllImport("xlpack.dll", EntryPoint = "?IsFileExist@@YA_NPBD@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsFileExist(char[] file);

        [DllImport("xlpack.dll", EntryPoint = "?Mount@@YAPAXPBD0_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Mount([MarshalAs(UnmanagedType.LPStr)] string where, [MarshalAs(UnmanagedType.LPStr)] string which, [MarshalAs(UnmanagedType.Bool)] bool editable);

        [DllImport("xlpack.dll", EntryPoint = "?SetFileLogHandler@@YAPAXPBDP6AX0ZZ@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SetFileLogHandler([MarshalAs(UnmanagedType.LPStr)] string s, XlFunc f);

        [DllImport("xlpack.dll", EntryPoint = "?Unmount@@YA_NPBD@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Unmount([MarshalAs(UnmanagedType.LPStr)] string where);

        [DllImport("xlpack.dll", EntryPoint = "?DeleteDir@@YA_NPBD@Z", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DeleteDir([MarshalAs(UnmanagedType.LPStr)] string path);

        [StructLayout(LayoutKind.Explicit)]
        public struct afs_finddata
        {
            [FieldOffset(0)] public long data0;
            [FieldOffset(8)] public long data1;
            [FieldOffset(16)] public long data2;
            [FieldOffset(24)] public long data3;
            [FieldOffset(32)] public long data4;
            [FieldOffset(40)] public long data5;
            [FieldOffset(48)] public long data6;
            [FieldOffset(56)] public long data7;
            [FieldOffset(64)] public long data8;
            [FieldOffset(72)] public long data9;
            [FieldOffset(80)] public long data10;
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

        public delegate void XlFunc(params string[] p);
    }
}
