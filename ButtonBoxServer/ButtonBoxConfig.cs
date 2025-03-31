using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ButtonBoxServer
{
    public class ButtonBoxConfig
    {
        public required string PortName { get; set; }
        public int BaudRate { get; set; } = 115200;
        public int Retries { get; set; } = 3;
        public int RetryWaitSeconds { get; set; } = 3;
        public Dictionary<int, string> Actions { get; set; } = [];
    }
}
