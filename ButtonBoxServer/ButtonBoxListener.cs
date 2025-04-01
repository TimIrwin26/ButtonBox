using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Ports;
using System.Management.Automation;

namespace ButtonBoxServer
{
    public class ButtonBoxListener(IOptions<ButtonBoxConfig> configuration, ILogger<ButtonBoxListener> logger) : BackgroundService
    {
        private readonly ButtonBoxConfig _config = configuration.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using SerialPort serialPort = new SerialPort(_config.PortName, _config.BaudRate);
            var retries = await OpenPort(serialPort, _config.Retries, stoppingToken);
            if (retries == 0) return;

            while (!stoppingToken.IsCancellationRequested)
            {
                var data = serialPort.ReadLine();
                try
                {
                    int button = Convert.ToInt32(data);

                    if (_config.Actions.TryGetValue(button, out var script))
                    {
                        Console.WriteLine($"Button {button} pressed, executing script: {script}");
                        await Task.Run(() => ExecuteScript(script), stoppingToken);
                    }
                    else
                    {
                        Console.WriteLine($"Button {button} pressed, no action configured");
                    }
                }
                catch (FormatException) { } //ignore non-numeric data
            }

            serialPort.Close();
        }

        private async Task<int> OpenPort(SerialPort serialPort, int retryCount, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested && !serialPort.IsOpen && retryCount > 0)
            {
                try
                {
                    serialPort.Open();
                    logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    logger.LogInformation("Listening for data from box on {port}", _config.PortName);
                }
                catch (UnauthorizedAccessException e)
                {
                    logger.LogError(e, "Failed to open port {port}", _config.PortName);
                    retryCount--;
                    if (retryCount == 0) throw;

                    logger.LogInformation("Retrying {retry} in {wait} seconds", retryCount, _config.RetryWaitSeconds);
                    await Task.Delay(_config.RetryWaitSeconds * 1000, stoppingToken);
                }
            }

            return retryCount;
        }

        private async Task ExecuteScript(string script)
        {
            using PowerShell powerShell = PowerShell.Create();

            // Set the execution policy to RemoteSigned
            powerShell.AddScript("Set-ExecutionPolicy RemoteSigned -Scope Process -Force");
            await powerShell.InvokeAsync();

            // Clear the previous commands
            powerShell.Commands.Clear();

            powerShell.AddScript(script);
            var result = await powerShell.InvokeAsync();

            foreach (var item in result)
            {
                logger.LogInformation("Script return {item}", item);
            }
            if (powerShell.HadErrors)
            {
                foreach (var error in powerShell.Streams.Error)
                {
                    logger.LogError("Script error {error}", error);
                }
            }
        }
    }
}
