using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundApp.Services
{
    public class NetworkMonitor : INetworkMonitor
    {
        private readonly string _filePath;

        public NetworkMonitor(string filePath)
        {
            _filePath = filePath;
        }

        public void SaveReport(List<string> lines)
        {
            File.WriteAllLines(_filePath, lines);
        }
    }
}
