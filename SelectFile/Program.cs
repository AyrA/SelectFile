using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SelectFile
{
    class Program
    {
        public struct RET
        {
            public const int SUCCESS = 0;
            public const int CANCEL = 1;
            public const int PARAMS = 2;
            public const int ERR = 255;
        }

        [STAThread]
        static int Main(string[] args)
        {
#if DEBUG
            //args = "/title|Test title|/multi|/default|test.exe|/start|C:\\Temp|/mask|exe|/mask|*".Split('|');
#endif
            ParsedArgs PA;
            try
            {
                PA = new ParsedArgs(args);
            }
            catch (Exception ex)
            {
                E(ex);
                return RET.PARAMS;
            }
            if (PA.Help)
            {
                Help();
                return RET.SUCCESS;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (PA.IsDir)
            {
                return ProcessDirectory(PA);
            }
            else
            {
                return ProcessFile(PA);
            }
        }

        private static int ProcessFile(ParsedArgs PA)
        {
            OpenFileDialog OFD;
            SaveFileDialog SFD;
            FileDialog FD;
            if (PA.FileSave)
            {
                OFD = null;
                FD = SFD = new SaveFileDialog();
            }
            else
            {
                SFD = null;
                FD = OFD = new OpenFileDialog();
                OFD.Multiselect = PA.MultiSelect;
            }
            if (!string.IsNullOrEmpty(PA.Start))
            {
                FD.InitialDirectory = PA.Start;
            }
            if (!string.IsNullOrEmpty(PA.DefaultName))
            {
                FD.FileName = PA.DefaultName;
            }

            FD.Filter = string.Join("|", PA.Masks.Select(m => m.Replace('=', '|')));
            FD.Title = string.IsNullOrEmpty(PA.Title) ? Console.Title : PA.Title;

            using (FD)
            {
                if (FD.ShowDialog() == DialogResult.OK)
                {
                    if (!PA.FileSave && OFD.Multiselect)
                    {
                        foreach (var F in FD.FileNames)
                        {
                            Console.WriteLine(F);
                        }
                    }
                    else
                    {
                        Console.WriteLine(FD.FileName);
                    }
                    return RET.SUCCESS;
                }
            }
            return RET.CANCEL;
        }

        private static int ProcessDirectory(ParsedArgs PA)
        {
            using (FolderBrowserDialog FBD = new FolderBrowserDialog())
            {
                FBD.ShowNewFolderButton = PA.NewDir;
                FBD.Description = string.IsNullOrEmpty(PA.Title) ? Console.Title : PA.Title;
                if (!string.IsNullOrEmpty(PA.Start))
                {
                    if (IsFile(PA.Start))
                    {
                        FBD.SelectedPath = System.IO.Path.GetDirectoryName(PA.Start);
                    }
                    else if (IsDir(PA.Start))
                    {
                        FBD.SelectedPath = PA.Start;
                    }
                    else
                    {
                        return E(new Exception("Neither directory nor file: " + PA.Start));
                    }
                }
                if (FBD.ShowDialog() == DialogResult.OK)
                {
                    Console.WriteLine(FBD.SelectedPath);
                    return RET.SUCCESS;
                }
            }
            return RET.CANCEL;
        }

        private static int E(Exception e)
        {
            Tools.WriteError("Error processing your arguments. Use /? for help.");
            Tools.WriteError(e.Message);
            return RET.ERR;
        }

        private static bool IsDir(string start)
        {
            try
            {
                return System.IO.Directory.Exists(start);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsFile(string start)
        {
            try
            {
                return System.IO.File.Exists(start);
            }
            catch
            {
                return false;
            }
        }

        static void Help()
        {
            Console.WriteLine(@"
SelectFile.exe [/save | /multi] [/title <text>] [/default <name>] [/start <dir>]
    [/mask name=value[;value] [/mask ...]] [/mask ext [/mask ...]]
SelectFile.exe /dir [/new] [/title <text>] [/start <dir>]

Selects a file /first variant) or directory (second variant)

File selection
--------------
/title    - Title of the directory or file window
/start    - Directory to start browsing in
/save     - Show the 'save file' dialog instead of the open dialog
/multi    - Allow multiple file selections. Mutually exclusive with /save
/default  - Default (proposed) file name (path information will be stripped).
/mask     - File selection mask. This argument is repeatable.
            If not specified at all, defaults to 'All Files (*.*)=*.*'
            See below for more details

Directory selection
-------------------
/dir      - Select a directory instead of a file
/new      - Show the 'New Directory' button
/title    - Title of the directory window
/start    - Directory to initially select

File selection masks
--------------------
The first specified mask will be selected. Multiple values can be specified
to group multiple types. It is highly recommended to wrap the name=value
part into quotes. Do not use vertical bars (|) at any place. Windows uses
these to split the entries.
A mask for a single extension can be specified as /mask ext
In that case we try to find the name from the registry. If no name can be
found in the registry, it will be substituted using a generic display string:
/mask {0}*.ext files=*.ext{0}

Note: The 'All files' type is not present by default if you specify
      your own masks. You can add it again by specifying it.
Note: Users can always override your selection by typing *.* as name.
      This is a feature of Windows and not this application.
Note: It's common but not required that you show the file masks in the name.
Note: You can use masks to limit users to a single file name

Example: /mask {0}Text files=*.txt;*.ini;*.log{0}
         /mask {0}All files (*.*)=*.*{0}
         /mask doc
         /mask {0}MyApp configuration=custom_name.config{0}
", '"');
        }
    }
}
