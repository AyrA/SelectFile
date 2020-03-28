using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectFile
{
    public static class Tools
    {
        public static void WriteError(string Line)
        {
            Console.Error.WriteLine(Line);
#if DEBUG
            System.Diagnostics.Debug.Print(Line);
#endif
        }
    }
}
