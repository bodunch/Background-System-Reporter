using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundApp.Services
{
    public interface INetworkMonitor
    {
        void SaveReport(List<string> lines);
    }
}
