using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace XLPakTool
{
    public class TreeDictionary
    {
        public class XlFile
        {
            public string Path { get; set; }
            public long Size { get; set; }
            public string Hash { get; set; } // TODO md5
            public DateTime CreateTime { get; set; }
            public DateTime ModifyTime { get; set; }

            public XlFile(string path, XLPack.pack_stat2 stat)
            {
                Path = path;
                CreateTime = DateTime.FromFileTime(stat.stat.creationTime);
                ModifyTime = DateTime.FromFileTime(stat.stat.modifiedTime);
                Hash = BitConverter.ToString(stat.digest.md5).Replace("-", "").ToLower();
                Size = stat.length;
            }
        }

        public string Path { get; set; }
        public TreeDictionary Parent { get; set; }
        public List<TreeDictionary> Directories { get; set; }
        public List<XlFile> Files { get; set; }

        public TreeDictionary(string path)
        {
            Path = path;
            Directories = new List<TreeDictionary>();
            Files = new List<XlFile>();
        }
    }

    class Program
    {
        private static string _globalPath = "/master/";
        private static string _fsPath;
        private static IntPtr _fsHandler;
        private static IntPtr _masterHandler;
        private static IntPtr _logHandler;

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
                _logHandler = XLPack.SetFileLogHandler("pack.log", LogHandler);
                Log("Info", "Done");

                MountFileSystem(gamePakPath);
                Thread.Sleep(1000);

                while (true)
                {
                    Console.Write($"~{_globalPath}$ ");
                    var command = Console.ReadLine();
                    var parse = CommandParser(command);

                    if (parse.Length <= 0)
                        continue;

                    var cmd = parse[0];
                    var cmdArgs = new string[parse.Length - 1];
                    if (cmdArgs.Length > 0)
                        Array.Copy(parse, 1, cmdArgs, 0, cmdArgs.Length);

                    if (cmd == "quit")
                        break;

                    switch (cmd)
                    {
                        case "help":
                            Log("Help", "cd <path> -> move at folders");
                            Log("Help", "ls <path?> -> get files");
                            Log("Help", "cp <scr> <dest> -> copy file from src to dest");
                            Log("Help", "rm <path> -> remove path");
                            Log("Help", "fstat <file path> -> Get file stat");
                            Console.WriteLine("--------------------------------");
                            Log("Help", "To export file(s)/dir:");
                            Log("Help", "cp <src> /fs/<dest>");
                            Log("Help", "To import file(s)/dir:");
                            Log("Help", "cp /fs/<src> <dest>");
                            break;
                        case "cd":
                            if (cmdArgs.Length == 0)
                                Log("Info", "cd <toDir>");
                            else
                            {
                                var cmdPath = cmdArgs[0];
                                var prePath = _globalPath;
                                _globalPath = AbsolutePath(cmdPath);
                                if (!_globalPath.EndsWith("/") && !_globalPath.EndsWith("\\"))
                                    _globalPath += "/";

                                if (!IsDirectory(_globalPath))
                                    _globalPath = prePath;
                            }

                            break;
                        case "ls":
                            var path = _globalPath;

                            if (cmdArgs.Length > 0)
                            {
                                path = AbsolutePath(cmdArgs[0]);
                                path += "/";
                            }

                            var files = GetFiles(path);
                            if (files.Count > 0)
                                foreach (var (file, isDirectory) in files)
                                {
                                    if (isDirectory)
                                        Console.BackgroundColor = ConsoleColor.DarkBlue;
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

                                bool exist;

                                if (src.StartsWith("/fs"))
                                {
                                    var realyPath = src.Replace("/fs", _fsPath);
                                    exist = File.Exists(realyPath);
                                    if (!exist)
                                        exist = Directory.Exists(realyPath);
                                }
                                else
                                    exist = IsPathExist(src);

                                if (!exist)
                                    Log("Warn", "Bad source path: {0}", src);
                                else
                                {
                                    var dir = IsDirectory(src);
                                    var result = dir ? XLPack.CopyDir(src, dest) : XLPack.Copy(src, dest);

                                    Console.WriteLine(result ? "Done" : "Copy failed...");
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
                                    Console.WriteLine(XLPack.DeleteDir(path) ? "Done" : "Remove failed...");
                                else
                                    Console.WriteLine(XLPack.FDelete(path) ? "Done" : "Remove failed...");
                            }

                            break;
                        case "fstat":
                            if (cmdArgs.Length == 0)
                                Log("Info", "fstat <file path>");
                            else
                            {
                                path = cmdArgs[0];
                                var temp = GetFileState(path);
                                if (temp == null)
                                    Log("Warn", "[File] Doesn't exist or get stat...");
                                else
                                {
                                    Log("File", path);
                                    Log("File", $"Size: {temp.Size}");
                                    Log("File", $"CreationTime: {temp.CreateTime}");
                                    Log("File", $"ModifiedTime: {temp.ModifyTime}");
                                    Log("File", $"MD5: {temp.Hash}");
                                }
                            }

                            break;
                        case "struct":
                            var tree = new TreeDictionary("/master");
                            GetFileSystemStruct(tree);
                            // TODO ... save to file...
                            break;
                    }
                }

                Destroy();
            }
            else
                Log("Error", "Cannot create file system");
        }

        private static void LogHandler(params string[] p)
        {
            foreach (var str in p)
                Console.WriteLine(str);
        }

        private static void MountFileSystem(string path)
        {
            var pack = new FileInfo(path);

            _fsPath = pack.DirectoryName;

            Log("Info", "Mount /fs ...");
            _fsHandler = XLPack.Mount("/fs", _fsPath, true);
            Log("Info", "Done");

            Log("Info", "Mount /master ...");
            _masterHandler = XLPack.Mount("/master", path, true);
            Log("Info", "Done");
        }

        private static List<(string, bool)> GetFiles(string path)
        {
            var result = new List<(string, bool)>();

            var file = path + "*";
            var fd = new XLPack.afs_finddata();
            var findHandle = XLPack.FindFirst(file, ref fd);
            if (findHandle != -1)
            {
                do
                {
                    var fileName = Marshal.PtrToStringAnsi(XLPack.GetFileName(ref fd));
                    var tempFile = path + fileName;
                    var isDirectory = !XLPack.IsFileExist(tempFile);
                    result.Add((tempFile, isDirectory));
                } while (XLPack.FindNext(findHandle, ref fd) != -1);
            }

            XLPack.FindClose(findHandle);
            return result;
        }

        private static bool IsDirectory(string path)
        {
            if (XLPack.IsFileExist(path))
                return false;
            var fd = new XLPack.afs_finddata();
            var first = XLPack.FindFirst(path, ref fd);
            var flag = first != -1;
            XLPack.FindClose(first);
            return flag;
        }

        private static bool IsPathExist(string path)
        {
            if (XLPack.IsFileExist(path))
                return true;
            var fd = new XLPack.afs_finddata();
            var first = XLPack.FindFirst(path, ref fd);
            var exist = first != -1;
            XLPack.FindClose(first);
            return exist;
        }

        private static void Destroy()
        {
            DestroyFileSystem();
            XLPack.DestroyFileLogHandler(_logHandler);
            XLPack.DestroyFileSystem();
        }

        private static void DestroyFileSystem()
        {
            XLPack.Unmount(_masterHandler);
            XLPack.Unmount(_fsHandler);
        }

        private static TreeDictionary.XlFile GetFileState(string path)
        {
            if (!XLPack.IsFileExist(path))
                return null;
            var position = XLPack.FOpen(path, "r");
            var stat = new XLPack.pack_stat2();
            var res = XLPack.FGetStat(position, ref stat);
            XLPack.FClose(ref position);
            return res ? new TreeDictionary.XlFile(path, stat) : null;
        }

        private static TreeDictionary GetFileSystemStruct(TreeDictionary master)
        {
            var files = GetFiles(master.Path + "/");
            foreach (var (file, isDirectory) in files)
            {
                if (isDirectory)
                {
                    var folder = new TreeDictionary(file) {Parent = master};
                    GetFileSystemStruct(folder);
                    master.Directories.Add(folder);
                }
                else
                {
                    var temp = GetFileState(file);
                    if (temp != null)
                        master.Files.Add(temp);
                }
            }

            return master;
        }

        private static string[] CommandParser(string str)
        {
            if (str == null || !(str.Length > 0))
                return new string[0];
            var idx = str.Trim().IndexOf(" ", StringComparison.Ordinal);
            if (idx == -1)
                return new[] {str};
            var count = str.Length;
            var list = new ArrayList();
            while (count > 0)
            {
                if (str[0] == '"')
                {
                    var temp = str.IndexOf("\"", 1, str.Length - 1, StringComparison.Ordinal);
                    while (str[temp - 1] == '\\')
                        temp = str.IndexOf("\"", temp + 1, str.Length - temp - 1, StringComparison.Ordinal);

                    idx = temp + 1;
                }

                if (str[0] == '\'')
                {
                    var temp = str.IndexOf("\'", 1, str.Length - 1, StringComparison.Ordinal);
                    while (str[temp - 1] == '\\')
                        temp = str.IndexOf("\'", temp + 1, str.Length - temp - 1, StringComparison.Ordinal);

                    idx = temp + 1;
                }

                var s = str.Substring(0, idx);
                var left = count - idx;
                str = str.Substring(idx, left).Trim();
                list.Add(s.Trim('"'));
                count = str.Length;
                idx = str.IndexOf(" ", StringComparison.Ordinal);
                if (idx != -1)
                    continue;
                var add = str.Trim('"', ' ');
                if (add.Length > 0)
                    list.Add(add);

                break;
            }

            return (string[]) list.ToArray(typeof(string));
        }

        private static string ExtractFileDirectory(string path)
        {
            if (path == "/")
                return path;

            var index = path.LastIndexOfAny("/".ToCharArray());
            return path.Substring(0, index);
        }

        private static string AbsolutePath(string path)
        {
            if (path.Length == 0)
                return path;
            if (path.StartsWith("/") || path.StartsWith("\\")) return path;
            if (path.Length == 1 && path == ".") return _globalPath;
            var basePath = ExtractFileDirectory(_globalPath);
            var relativePath = path;
            if (relativePath.Length <= 1)
                return basePath + "/" + relativePath;

            while (relativePath.Substring(0, 2) == "..")
            {
                basePath = ExtractFileDirectory(basePath);
                if (relativePath.Length < 3)
                    return "";
                relativePath = relativePath.Substring(3, relativePath.Length - 3);
                if (relativePath.Length == 0)
                    return basePath;
            }

            return basePath + "/" + relativePath;
        }

        private static void Log(string level, string message, params object[] args)
        {
            Console.WriteLine($"[{level}] {string.Format(message, args)}");
        }
    }
}