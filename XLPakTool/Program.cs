using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace XLPakTool
{
    public class TreeDictionary
    {
        public string Path { get; set; }
        public TreeDictionary Parent { get; set; }
        public List<TreeDictionary> Directories { get; set; }
        public List<string> Files { get; set; }

        public TreeDictionary(string path)
        {
            Path = path;
            Directories = new List<TreeDictionary>();
            Files = new List<string>();
        }
    }

    class Program
    {
        public static string GlobalPath = "/master/";
        public static string FsPath;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Start: XLPakTool.exe <path_to_game_pak>");
                return;
            }

            var gamePakPath = args[0];

            Log("Info", "Create file system...");
            if (XLPack.CreateFileSystem())
            {
                Log("Info", "Done");

                Log("Info", "Connect log handler...");
                XLPack.SetFileLogHandler("pack.log", LogHandler);
                Log("Info", "Done");

                MountFileSystem(gamePakPath);
                Thread.Sleep(1000);

                while (true)
                {
                    Console.Write($"~{GlobalPath}$ ");
                    var command = Console.ReadLine();
                    var parse = CommandParser(command);

                    if (parse.Length > 0)
                    {
                        var cmd = parse[0];
                        var cmdArgs = new string[parse.Length - 1];
                        if (cmdArgs.Length > 0)
                            Array.Copy(parse, 1, cmdArgs, 0, cmdArgs.Length);

                        if (cmd == "quit")
                            break;
                        else
                        {
                            switch (cmd)
                            {
                                case "help":
                                    Log("Info", "cd <path> -> move at folders");
                                    Log("Info", "ls <path?> -> get files");
                                    Log("Info", "cp <scr> <dest> -> copy file from src to dest");
                                    Log("Info", "rm <path> -> remove path");
                                    Console.WriteLine("--------------------------------");
                                    Log("Info", "To export file(s)/dir:");
                                    Log("Info", "cp <src> /fs/<dest>");
                                    Log("Info", "To import file(s)/dir:");
                                    Log("Info", "cp /fs/<src> <dest>");
                                    break;
                                case "cd":
                                    if (cmdArgs.Length == 0)
                                        Log("Info", "cd <toDir>");
                                    else
                                    {
                                        var cmdPath = cmdArgs[0];
                                        var prePath = GlobalPath;
                                        GlobalPath = AbsolutePath(cmdPath);
                                        if (!GlobalPath.EndsWith("/") && !GlobalPath.EndsWith("\\"))
                                            GlobalPath += "/";

                                        if (!IsDirectory(GlobalPath))
                                            GlobalPath = prePath;
                                    }
                                    break;
                                case "ls":
                                    var path = GlobalPath;

                                    if (cmdArgs.Length > 0)
                                    {
                                        path = AbsolutePath(cmdArgs[0]);
                                        path += "/";
                                    }

                                    var files = GetFiles(path);
                                    if (files.Count > 0)
                                        foreach (var file in files)
                                        {
                                            if (IsDirectory(file))
                                                Console.BackgroundColor = ConsoleColor.Blue;
                                            Console.WriteLine(file.Replace(path, ""));
                                            Console.ResetColor();
                                        }
                                    else
                                        Console.WriteLine("------ EMPTY ------");
                                    break;
                                case "cp":
                                    if (cmdArgs.Length < 2)
                                        Log("Info", "cp <src> <dest>");
                                    else
                                    {
                                        var src = AbsolutePath(cmdArgs[0]);
                                        var dest = AbsolutePath(cmdArgs[1]);

                                        var exist = false;

                                        if (src.StartsWith("/fs"))
                                        {
                                            var realyPath = src.Replace("/fs", FsPath);
                                            exist = File.Exists(realyPath);
                                            if (!exist)
                                                exist = Directory.Exists(realyPath);
                                        }
                                        else
                                            exist = IsPathExist(src);

                                        Thread.Sleep(1000);

                                        if (dest.StartsWith("/fs"))
                                        {
                                            var realyPath = dest.Replace("/fs", FsPath);
                                            Directory.CreateDirectory(realyPath);
                                        }

                                        if (!exist)
                                            Log("Warn", "Bad source path: {0}", src);
                                        else
                                        {
                                            var result = false;
                                            if (IsDirectory(src))
                                                result = XLPack.CopyDir(src, dest);
                                            else
                                                result = XLPack.Copy(src, dest);

                                            if (result)
                                                Console.WriteLine("Done");
                                            else
                                                Console.WriteLine("Copy failed...");
                                        }
                                    }
                                    break;
                                case "rm":
                                    if (cmdArgs.Length == 0)
                                        Log("Info", "rm <path>");
                                    else
                                    {
                                        path = AbsolutePath(cmdArgs[0]);
                                        if (IsDirectory(path))
                                        { 
                                            if (XLPack.DeleteDir(path))
                                                Console.WriteLine("Done");
                                            else
                                                Console.WriteLine("Remove failed...");
                                        }
                                        else
                                        {
                                            if (XLPack.FDelete(path))
                                                Console.WriteLine("Done");
                                            else
                                                Console.WriteLine("Remove failed...");
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }

                Destroy();
            }
            else
                Log("Error", "Cannot create file system");
        }

        private static void LogHandler(params string[] p)
        {
            foreach (string str in p)
                Console.WriteLine(str);
        }

        private static void MountFileSystem(string path)
        {
            var pack = new FileInfo(path);

            FsPath = pack.DirectoryName;

            Log("Info", "Mount /fs ...");
            XLPack.Mount("/fs", FsPath, true);
            Log("Info", "Done");

            Log("Info", "Mount /master ...");
            XLPack.Mount("/master", path, true);
            Log("Info", "Done");
        }

        public static async Task<TreeDictionary> GetFileSystemStruct(TreeDictionary master)
        {
            var files = await GetFilesAsync(master.Path + "/");
            foreach (var file in files)
            {
                if (await IsDirectoryAsync(file))
                {
                    var folder = new TreeDictionary(file);
                    folder.Parent = master;
                    await GetFileSystemStruct(folder);
                    master.Directories.Add(folder);
                }
                else
                    master.Files.Add(file);
            }
            return master;
        }

        private static Task<List<string>> GetFilesAsync(string path)
        {
            return Task.FromResult(GetFiles(path));
        }

        private static List<string> GetFiles(string path)
        {
            var result = new List<string>();

            var file = path + "*";
            XLPack.afs_finddata fd = new XLPack.afs_finddata();
            int findHandle = XLPack.FindFirst(path, ref fd);
            if (findHandle != -1)
            {
                do
                {
                    string stringAnsi = Marshal.PtrToStringAnsi(XLPack.GetFileName(ref fd));
                    result.Add(path + stringAnsi);
                }
                while (XLPack.FindNext(findHandle, ref fd) != -1);
            }
            XLPack.FindClose(findHandle);
            return result;
        }

        private static bool IsDirectory(string path)
        {
            if (XLPack.IsFileExist(path.ToCharArray()))
                return false;
            XLPack.afs_finddata fd = new XLPack.afs_finddata();
            int first = XLPack.FindFirst(path, ref fd);
            bool flag = first != -1;
            XLPack.FindClose(first);
            return flag;
        }

        private static Task<bool> IsDirectoryAsync(string path)
        {
            return Task.FromResult(IsDirectory(path));
        }

        private static bool IsPathExist(string path)
        {
            if (XLPack.IsFileExist(path.ToCharArray()))
                return true;
            XLPack.afs_finddata fd = new XLPack.afs_finddata();
            int first = XLPack.FindFirst(path, ref fd);
            var exist = first != -1;
            XLPack.FindClose(first);
            return exist;
        }

        private static bool Copy(string from, string to)
        {
            if (IsDirectory(from))
                return XLPack.CopyDir(from, to);
            return XLPack.Copy(from, to);
        }

        private static Task<bool> CopyAsync(string from, string to)
        {
            return Task.FromResult(Copy(from, to));
        }

        private static void Destroy()
        {
            DestroyFileSystem();
            XLPack.DestroyFileLogHandler(IntPtr.Zero);
            XLPack.DestroyFileSystem();
        }

        private static void DestroyFileSystem()
        {
            XLPack.Unmount("/master");
            XLPack.Unmount("/fs");
        }

        public static string[] CommandParser(string str)
        {
            if (str == null || !(str.Length > 0)) return new string[0];
            int idx = str.Trim().IndexOf(" ");
            if (idx == -1) return new string[] { str };
            int count = str.Length;
            ArrayList list = new ArrayList();
            while (count > 0)
            {
                if (str[0] == '"')
                {
                    int temp = str.IndexOf("\"", 1, str.Length - 1);
                    while (str[temp - 1] == '\\')
                    {
                        temp = str.IndexOf("\"", temp + 1, str.Length - temp - 1);
                    }
                    idx = temp + 1;
                }
                if (str[0] == '\'')
                {
                    int temp = str.IndexOf("\'", 1, str.Length - 1);
                    while (str[temp - 1] == '\\')
                    {
                        temp = str.IndexOf("\'", temp + 1, str.Length - temp - 1);
                    }
                    idx = temp + 1;
                }
                string s = str.Substring(0, idx);
                int left = count - idx;
                str = str.Substring(idx, left).Trim();
                list.Add(s.Trim('"'));
                count = str.Length;
                idx = str.IndexOf(" ");
                if (idx == -1)
                {
                    string add = str.Trim('"', ' ');
                    if (add.Length > 0)
                    {
                        list.Add(add);
                    }
                    break;
                }
            }
            return (string[])list.ToArray(typeof(string));
        }

        private static string ExtractFileDirectory(string path)
        {
            if (path == "/")
                return path;

            var index = path.LastIndexOfAny("/".ToCharArray());
            return path.Substring(0, index);
        }

        public static string AbsolutePath(string path)
        {
            if (path.Length == 0)
                return path;
            if (path.StartsWith("/") || path.StartsWith("\\")) return path;
            if (path.Length == 1 && path == ".") return GlobalPath;
            var basePath = ExtractFileDirectory(GlobalPath);
            var relativePath = path;
            while (relativePath.Substring(0, 2) == "..")
            {
                basePath = ExtractFileDirectory(basePath);
                if (relativePath.Length < 3)
                    return "";
                else
                    relativePath = relativePath.Substring(3, relativePath.Length - 3);
                if (relativePath.Length == 0)
                    return basePath;
            }
            return basePath + "/" + relativePath;
        }

        public static void Log(string level, string message, params string[] args)
        {
            Console.WriteLine($"[{level}] {string.Format(message, args)}");
        }
    }
}
