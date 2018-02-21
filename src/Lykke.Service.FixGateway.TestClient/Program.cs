using System;
using Common.Log;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.TestClient;
using Microsoft.Extensions.Configuration;

namespace TestClient
{
    internal class Program
    {
        private ILog _log;
        private readonly IConfigurationRoot _configuration;
        private FixClient _quoteSessionClient;
        private FixClient _tradeSessionClient;
        private static void Main(string[] args)
        {
            var p = new Program();
            p.InputLoop();

        }

        private Program()
        {
            _log = new LogToConsole();
            var builder = new ConfigurationBuilder()
                .AddJsonFile(@"appsettings.Test.json", false);
            _configuration = builder.Build();
            StartClient();
        }

        private void StartClient()
        {
            var settings = new AppSettings();
            _configuration.Bind(settings);
            _quoteSessionClient = new FixClient(settings.TestClient.ServiceUrl, settings.TestClient.Credentials.Password, settings.TestClient.Sessions.QuoteSession);
            _quoteSessionClient.Start();
            _log.WriteWarning("Quote session connected", "", "");

            _tradeSessionClient = new FixClient(settings.TestClient.ServiceUrl, settings.TestClient.Credentials.Password, settings.TestClient.Sessions.TradeSession);
            _tradeSessionClient.Start();
            _log.WriteWarning("Quote session connected", "", "");

        }

        private void InputLoop()
        {
            while (true)
            {
                Console.WriteLine(@"Type al to request assets list. Type o to create a new market order. Type q to exit");
                var command = Console.ReadLine();
                if (string.Equals("Q", command, StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }
            }
        }
    }
}
