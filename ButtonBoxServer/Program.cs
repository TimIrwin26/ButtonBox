using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ButtonBoxServer
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var host = CreateHostBuilder().Build();
            Task.Run(() => host.RunAsync());
            ApplicationConfiguration.Initialize();
            Application.Run(new ButtonBoxServerApplicationContext());
            host.Dispose();
        }

        private static IHostBuilder CreateHostBuilder()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Add JSON configuration file
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;
                    // Load configuration section into ButtonBoxConfig
                    services.Configure<ButtonBoxConfig>(configuration.GetSection("ButtonBox"));
                    services.AddHostedService<ButtonBoxListener>();

                    var serilogLogger = new LoggerConfiguration()
                            .ReadFrom.Configuration(configuration)                           
#if DEBUG
                            .WriteTo.Debug()
#endif
                            .CreateLogger();
                    services.AddLogging(x =>
                    {
                        x.SetMinimumLevel(LogLevel.Information);
                        x.AddSerilog(logger: serilogLogger, dispose: true);
                    });
                });            

            return builder;
        }
    }
}