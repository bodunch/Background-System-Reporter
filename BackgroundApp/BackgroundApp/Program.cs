using BackgroundApp;
using BackgroundApp.Services;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace BackgroundApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var systemLogPath = @"D:\C#\dotnet\LogsForBackgroundApp\SystemLogFile.txt";
            var processLogPath = @"D:\C#\dotnet\LogsForBackgroundApp\ProcessLogFile.txt";
            var netwokLogPath = @"D:\C#\dotnet\LogsForBackgroundApp\NetworkLogFile.txt";

            if (File.Exists(systemLogPath))
            {
                File.Delete(systemLogPath);
            }
            if(File.Exists(processLogPath))
            {
                File.Delete(processLogPath);
            }
            if (File.Exists(netwokLogPath))
            {
                File.Delete(netwokLogPath);
            }

            try
            {
                var builder = Host.CreateApplicationBuilder(args);

                Log.Information("Starting up the service");

                Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(systemLogPath)
                .CreateLogger();

                builder.Logging.ClearProviders();
                builder.Logging.AddSerilog(Log.Logger);

                builder.Services.AddSingleton<IProcessReporter>(new ProcessReporter(processLogPath));
                builder.Services.AddSingleton<INetworkMonitor>(new NetworkMonitor(netwokLogPath));

                builder.Services.AddWindowsService();

                builder.Services.AddHostedService<Worker>();

                var host = builder.Build();
                host.Run();

                return;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Here is a problem with a service");
                return;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}