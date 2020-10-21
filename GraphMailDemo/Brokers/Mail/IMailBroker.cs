using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphMailDemo.Brokers.Mail
{
    public interface IMailBroker
    {
        ValueTask SendMail(
            List<string> recipients, 
            List<string> ccRecipients, 
            string subject, 
            string content);
    }
}
