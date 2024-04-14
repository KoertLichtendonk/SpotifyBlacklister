using QuickConfig;
using SpotifyBlacklister.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyBlacklister.Configuration
{
    public class Config : ConfigBase
    {
        public OauthToken Token { get; set; }
        public AppInformation AppInformation { get; set; }
        public EmailServer EmailServer { get; set; }
        public List<BlacklistedArtist> BlacklistedArtists { get; set; }
        public Config()
        {
            this.Token = new OauthToken();
            this.AppInformation = new AppInformation();
            this.BlacklistedArtists = new List<BlacklistedArtist>();
            this.EmailServer = new EmailServer();
        }
    }
}
