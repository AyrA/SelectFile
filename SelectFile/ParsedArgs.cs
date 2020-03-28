using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectFile
{
    public class ParsedArgs
    {
        public const string DEFAULT_MASK = "All files (*.*)=*.*";
        public bool Empty { get; private set; }
        public bool Help { get; private set; }
        public bool IsDir { get; private set; }
        public bool IsFile { get { return !IsDir; } private set { IsDir = !value; } }
        public bool NewDir { get; private set; }
        public bool FileSave { get; private set; }
        public bool MultiSelect { get; private set; }
        public string Title { get; private set; }
        public string Start { get; private set; }
        public string RootDir { get; private set; }
        public string DefaultName { get; private set; }
        public string[] Masks { get; private set; }

        public ParsedArgs(IEnumerable<string> Args)
        {
            var MaskBuffer = new List<string>();

            //Arguments as list
            var A = Args
                .Where(m => m != null)
                .ToList();
            //Lowercase arguments
            var Lower = A
                .Select(m => m.ToLower())
                .ToList();
            //Stop if empty
            Empty = A.Count == 0;
            if (Empty)
            {
                Masks = new string[] { DEFAULT_MASK };
                return;
            }
            //Stop if help request
            Help = A.Contains("/?") || A.Contains("-?") || Lower.Contains("--help");
            if (Help)
            {
                return;
            }

            //Arguments valid for both cases
            for (int i = 0; i < A.Count; i++)
            {
                if (Lower[i] == "/title")
                {
                    if (Title != null)
                    {
                        throw new ArgumentException("Multiple instances of /title");
                    }
                    if (i < A.Count - 1)
                    {
                        Title = A[++i];
                    }
                    else
                    {
                        throw new ArgumentException("/title specified without a title");
                    }
                }
                else if (Lower[i] == "/start")
                {
                    if (Start != null)
                    {
                        throw new ArgumentException("Multiple instances of /start");
                    }
                    if (i < A.Count - 1)
                    {
                        Start = A[++i];
                    }
                    else
                    {
                        throw new ArgumentException("/start specified without a value");
                    }
                }
            }

            if (Lower.Count(m => m == "/dir") > 1)
            {
                throw new ArgumentException("Multiple instances of /dir");
            }
            IsDir = Lower.Contains("/dir");

            if (IsDir)
            {
                for (int i = 0; i < A.Count; i++)
                {
                    if (NewDir)
                    {
                        throw new ArgumentException("Multiple instances of /new");
                    }
                    if (Lower[i] == "/new")
                    {
                        NewDir = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < A.Count; i++)
                {
                    if (Lower[i] == "/save")
                    {
                        if (FileSave)
                        {
                            throw new ArgumentException("Multiple instances of /save");
                        }
                        if (MultiSelect)
                        {
                            throw new ArgumentException("Can't use /multi and /save at the same time");
                        }
                        FileSave = true;
                    }
                    else if (Lower[i] == "/multi")
                    {
                        if (MultiSelect)
                        {
                            throw new ArgumentException("Multiple instances of /multi");
                        }
                        if (FileSave)
                        {
                            throw new ArgumentException("Can't use /multi and /save at the same time");
                        }
                        MultiSelect = true;
                    }
                    else if (Lower[i] == "/default")
                    {
                        if (DefaultName != null)
                        {
                            throw new ArgumentException("Multiple instances of /default");
                        }
                        if (i < A.Count - 1)
                        {
                            DefaultName = A[++i];
                        }
                        else
                        {
                            throw new ArgumentException("/start specified without a value");
                        }
                    }
                    else if (Lower[i] == "/mask")
                    {
                        if (i < A.Count - 1)
                        {
                            MaskBuffer.Add(A[++i]);
                            if (MaskBuffer.Last().Contains('|'))
                            {
                                throw new ArgumentException("File name mask contains a vertical bar (|) which is not allowed.");
                            }
                            if (!MaskBuffer.Last().Contains('='))
                            {
                                MaskBuffer[MaskBuffer.Count - 1] = BuildFilter(MaskBuffer.Last());
                            }
                        }
                        else
                        {
                            throw new ArgumentException("/mask specified without a value");
                        }
                    }
                }
                if (MaskBuffer.Count == 0)
                {
                    MaskBuffer.Add(DEFAULT_MASK) ;
                }
                Masks = MaskBuffer.ToArray();
            }
        }

        private static string BuildFilter(string ext)
        {
            if (!ValidName(ext))
            {
                throw new ArgumentException($"Invalid mask extension: {ext}, masks usually only contain alphanumeric characters");
            }
            try
            {
                var RealName = Registry.GetValue($"HKEY_CLASSES_ROOT\\.{ext}", "", null).ToString();
                if(!ValidName(RealName))
                {
                    throw new Exception("Invalid file type name");
                }
                var RealType = Registry.GetValue($"HKEY_CLASSES_ROOT\\{RealName}", "", null).ToString();
                return $"{RealType} (*.{ext})=*.{ext}";
            }
            catch
            {
                return $"*.{ext} files=*.{ext}";
            }
        }

        private static bool ValidName(string Name)
        {
            if(string.IsNullOrEmpty(Name))
            {
                return false;
            }
            var Invalids = System.IO.Path.GetInvalidFileNameChars()
                .Concat(System.IO.Path.GetInvalidPathChars())
                .Distinct()
                .ToArray();
            if (Name.Any(m => Invalids.Contains(m)))
            {
                return false;
            }
            return true;
        }
    }
}
