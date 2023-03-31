using SpotifyBlacklister.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyBlacklister.Helpers
{
    public class EmailHelper
    {
        public static void SendEmailWithDeletedSongs(Dictionary<string, string> deletedSongs)
        {
            var fromAddress = new MailAddress(ConfigManager.Instance.Data.EmailServer.FromAddress, Assembly.GetCallingAssembly().GetName().Name.Replace(" ", ""));
            var toAddress = new MailAddress(ConfigManager.Instance.Data.EmailServer.ToAddress);
            const string subject = "Deleted Songs";
            string body = GenerateEmailBody(deletedSongs);

            var smtp = new SmtpClient
            {
                Host = ConfigManager.Instance.Data.EmailServer.SmtpServer, 
                Port = ConfigManager.Instance.Data.EmailServer.SmtpPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, ConfigManager.Instance.Data.EmailServer.Password)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }

        private static string GenerateEmailBody(Dictionary<string, string> deletedSongs)
        {
            var sb = new StringBuilder();
            sb.AppendLine("The following songs have been deleted:");
            sb.AppendLine();

            foreach (KeyValuePair<string, string> song in deletedSongs)
            {
                sb.AppendLine($"- {song.Key} by {song.Value}");
            }

            return sb.ToString();
        }
    }
}
