using System.Collections.Generic;

namespace Helium
{
    public class Config
    {
        public string Host { get; set; } = string.Empty;
        public bool RunLoop { get; set; } = false;
        public bool RunWeb { get; set; } = false;
        public int Threads { get; set; } = -1;
        public int SleepMs { get; set; } = -1;
        public int Duration { get; set; } = 0;
        public bool Random { get; set; } = false;
        public bool Verbose { get; set; } = false;
        public List<string> FileList { get; } = new List<string>();
    }
}
