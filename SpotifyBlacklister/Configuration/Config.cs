using SpotifyBlacklister.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyBlacklister.Configuration
{
    public class Config
    {
        public OauthToken Token { get; set; }
        public AppInformation AppInformation { get; set; }
        public EmailServer EmailServer { get; set; }
        public List<string> Artists { get; set; }
        public Config()
        {
            this.Token = new OauthToken();
            this.AppInformation = new AppInformation();
            this.Artists = new List<string>();
            this.EmailServer = new EmailServer();
        }
    }
}
