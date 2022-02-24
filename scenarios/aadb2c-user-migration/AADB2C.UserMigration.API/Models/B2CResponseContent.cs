using System.Net;
using System.Reflection;

namespace AADB2C.UserMigration.API.Models
{
    public class B2CResponseContent
    {
        public string Version { get; set; }
        public int Status { get; set; }
        public string UserMessage { get; set; }

        public B2CResponseContent(string message, HttpStatusCode status)
        {
            UserMessage = message;
            Status = (int)status;
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }

}