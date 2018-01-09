using System.Collections.Generic;
using System.IO;

namespace Lykke.Service.FixGateway.Core.Settings.ServiceSettings
{
    public sealed class SessionSetting
    {
        public string SenderCompID { get; set; }
        public string TargetCompID { get; set; }
        public string[] FixConfiguration { get; set; }

        public TextReader GetFixConfigAsReader()
        {
            var config = new List<string>(FixConfiguration)
            {
                $"SenderCompID={SenderCompID}",
                $"TargetCompID={TargetCompID}"
            };
            return new StringReader(string.Join("\n", config));
        }
    }
}
