using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Serilog;
using BackgroundApp.Services;

namespace BackgroundApp
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _systemLogger;
        private readonly IProcessReporter _processLogger;
        private readonly INetworkMonitor _networkMonitor;

        private bool _started = true;

        public Worker(ILogger<Worker> logger, IProcessReporter processLogger, INetworkMonitor networkMonitor)
        {
            _systemLogger = logger;
            _processLogger = processLogger;
            _networkMonitor = networkMonitor;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        [SupportedOSPlatform("windows")]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var searcherSystem = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                var searcherComputer = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                var searcherCPU = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                var searcherRAM = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");

                var reportProcessData = new List<string>();
                var reportNetworkData = new List<string>();

                if (_started)
                {
                    foreach (ManagementObject obj in searcherSystem.Get())
                    {
                        _systemLogger.LogInformation(" === About System: === ");

                        _systemLogger.LogInformation("OS:              " + Convert.ToString(obj["Caption"]));
                        _systemLogger.LogInformation("Win version:     " + Convert.ToString(obj["Version"]));
                        _systemLogger.LogInformation("PS name:         " + Convert.ToString(obj["CSName"]));
                        _systemLogger.LogInformation("User name:       " + Convert.ToString(obj["RegisteredUser"]));

                        string value = obj["LastBootUpTime"].ToString()!;
                        DateTime bootTime = ManagementDateTimeConverter.ToDateTime(value);
                        _systemLogger.LogInformation("Last login time: " + Convert.ToString(bootTime));

                        _systemLogger.LogInformation(" ");
                    }

                    foreach (ManagementObject obj in searcherComputer.Get())
                    {
                        _systemLogger.LogInformation(" === About Computer: ===");

                        _systemLogger.LogInformation("Manufacturer:    " + Convert.ToString(obj["Manufacturer"]));
                        _systemLogger.LogInformation("PC Model:        " + Convert.ToString(obj["Model"]));
                        _systemLogger.LogInformation("System type:     " + Convert.ToString(obj["SystemType"]));
                        _systemLogger.LogInformation("Count of CPU:    " + Convert.ToString(obj["NumberOfProcessors"]));
                        _systemLogger.LogInformation("System start:    " + Convert.ToString(obj["BootupState"]));
                        _systemLogger.LogInformation("Status of start: " + Convert.ToString(obj["Status"]));

                        _systemLogger.LogInformation(" ");
                    }

                    foreach (ManagementObject obj in searcherCPU.Get())
                    {
                        _systemLogger.LogInformation(" === About CPU: ===");

                        _systemLogger.LogInformation("CPU Name:        " + Convert.ToString(obj["Name"]));
                        _systemLogger.LogInformation("Manufacturer:    " + Convert.ToString(obj["Manufacturer"]));
                        _systemLogger.LogInformation("Num of cores:    " + Convert.ToString(obj["NumberOfCores"]));
                        _systemLogger.LogInformation("Num of streams:  " + Convert.ToString(obj["NumberOfLogicalProcessors"]));

                        _systemLogger.LogInformation(" ");
                    }

                    foreach (ManagementObject obj in searcherRAM.Get())
                    {
                        _systemLogger.LogInformation(" === About RAM: ===");

                        _systemLogger.LogInformation("Type:            " + Convert.ToString(obj["Caption"]));
                        _systemLogger.LogInformation("Part Number:     " + Convert.ToString(obj["PartNumber"]));
                        _systemLogger.LogInformation("Frequency:       " + Convert.ToString(obj["ConfiguredClockSpeed"]) + "MGc");
                        var memCount = (Convert.ToInt64(obj["Capacity"])) / 1073741824.0;
                        _systemLogger.LogInformation("Memory count:    " + Convert.ToString(memCount) + "GB");

                        _systemLogger.LogInformation(" ");
                    }
                    _started = false;
                }

                _systemLogger.LogInformation(" === Current info about CPU / RAM: === ");
                _systemLogger.LogInformation(" ");
                foreach (ManagementObject obj in searcherCPU.Get())
                {
                    _systemLogger.LogInformation("About CPU:");

                    _systemLogger.LogInformation("Load CPU:    " + Convert.ToString(obj["LoadPercentage"]) + "%");
                    string errorcode = Convert.ToString(obj["LastErrorCode"])!;
                    if (errorcode == "" || errorcode == null)
                        errorcode = "No error";
                    _systemLogger.LogInformation("Error:       " + errorcode);
                    _systemLogger.LogInformation("Status:      " + Convert.ToString(obj["Status"]));

                    _systemLogger.LogInformation(" ");
                }

                foreach (ManagementObject obj in searcherSystem.Get())
                {
                    _systemLogger.LogInformation("About RAM:");

                    var totalmem = Math.Round((Convert.ToDouble(obj["TotalVisibleMemorySize"])) / 1048576.0 , 2);
                    var freemem = Math.Round((Convert.ToDouble(obj["FreePhysicalMemory"])) / 1048576.0 , 2);
                    var usemem = Math.Round(totalmem - freemem, 2);
                    _systemLogger.LogInformation("Total memory:    " + Convert.ToString(totalmem) + " GB");
                    _systemLogger.LogInformation("Free memory:     " + Convert.ToString(freemem) + " GB");
                    _systemLogger.LogInformation("Used memroy:     " + Convert.ToString(usemem) + " GB");

                    _systemLogger.LogInformation(" ");
                }

                foreach (var proc in Process.GetProcesses())
                {
                    try
                    {
                        string name = proc.ProcessName;
                        int id = proc.Id;
                        double ram = Math.Round(proc.WorkingSet64 / 1048576.0, 2);
                        string path = proc.MainModule?.FileName ?? "Unknown Path";
                        DateTime time = proc.StartTime;

                        reportProcessData.Add($"Process name: {name, -40} | ID: {id, -6} | RAM: {ram + " MB", -10} | Start time: {time, -25} | Path: {path}");
                    }
                    catch 
                    {
                        continue;
                    } 
                }
                _processLogger.SaveReport(reportProcessData);

                reportNetworkData.Add("");
                reportNetworkData.Add(" === Adapters: ===");
                foreach (var net in NetworkInterface.GetAllNetworkInterfaces())
                {
                    try 
                    {
                        string name = net.Name;
                        string ?stat;
                        if (net.OperationalStatus == OperationalStatus.Up)
                            stat = Convert.ToString(net.OperationalStatus);
                        else stat = "No connection";
                        long speed = net.Speed / 1_000_000;
                        double receivedMB = Math.Round(net.GetIPStatistics().BytesReceived / 1024.0 / 1024.0, 2);
                        double sentMB = Math.Round(net.GetIPStatistics().BytesSent / 1024.0 / 1024.0, 2);

                        reportNetworkData.Add($"Network Name: {name, -87} | Status: {stat, -15} | Speed: {speed + "MB/s", -10} | Received: {receivedMB + "MB", -10} | Sent: {sentMB + "MB", -10}");
                    }
                    catch
                    {
                        continue;
                    }
                }
                _networkMonitor.SaveReport(reportNetworkData);

                reportNetworkData.Add("");
                reportNetworkData.Add(" === Active connections: ===");
                foreach (var net in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections())
                {
                    if(net.State == TcpState.Established)
                    {
                        reportNetworkData.Add($"Remote: {net.RemoteEndPoint, -20} | Local: {net.LocalEndPoint}");
                    }
                }
                _networkMonitor.SaveReport(reportNetworkData);

                reportNetworkData.Add("");
                reportNetworkData.Add(" === Open ports: ===");
                foreach (var net in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners())
                {
                    reportNetworkData.Add($"Listening on: {net.Address, -18} : {net.Port}");
                }
                _networkMonitor.SaveReport(reportNetworkData);

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
