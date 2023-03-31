using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyBlacklister.Entities
{
    public class EmailServer
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string Password { get; set; }

        public EmailServer()
        {
            this.FromAddress = String.Empty;
            this.ToAddress = String.Empty;
            this.SmtpServer = String.Empty;
            this.SmtpPort = 587;
            this.Password = String.Empty;
        }
    }
}
