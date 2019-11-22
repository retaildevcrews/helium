using System.Collections.Generic;

namespace Helium
{
    public class Config
    {
        public bool Verbose = false;
        public bool Random = false;
        public int SleepMs = -1;
        public int Threads = -1;
        public int Duration = 0;
        public bool RunLoop = false;
        public bool RunWeb = false;
        public string Host = string.Empty;
        public readonly List<string> FileList = new List<string>();
    }
}
