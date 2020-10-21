using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using GraphMailDemo.Models.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;

namespace GraphMailDemo.Brokers.Mail
{
    public class MailBroker : IMailBroker
    {
        private readonly IConfiguration configuration;
        private readonly IGraphServiceClient graphServiceClient;
        private MailConfiguration mailConfiguration;

        public MailBroker(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.graphServiceClient = BuildGraphServiceClient();
        }

        public async ValueTask SendMail(
            List<string> recipients,
            List<string> ccRecipients,
            string subject,
            string content)
        {
            Message message = BuildMessage(recipients, ccRecipients, subject, content);
            await SendEmailMessageAsync(message);
        }

        private IGraphServiceClient BuildGraphServiceClient()
        {
            this.mailConfiguration =
                this.configuration.Get<LocalConfiguration>()
                    .MailConfiguration;

            IPublicClientApplication publicClientApplication =
                PublicClientApplicationBuilder
                    .Create(this.mailConfiguration.ClientId)
                    .WithTenantId(this.mailConfiguration.TenantId)
                    .Build();

            var authProvider = new UsernamePasswordProvider(
                publicClientApplication,
                this.mailConfiguration.Scopes);

            return new GraphServiceClient(authProvider);
        }

        private static Message BuildMessage(
            List<string> recipients,
            List<string> ccRecipients,
            string subject,
            string content)
        {
            return new Message
            {
                Subject = subject,

                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = content
                },

                ToRecipients = recipients.Select(recipient =>
                {
                    return new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = recipient
                        }
                    };
                }),

                CcRecipients = ccRecipients.Select(ccRecipient =>
                {
                    return new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = ccRecipient
                        }
                    };
                })
            };
        }

        private SecureString GetSecurePasswordString()
        {
            var securePasswordString = new SecureString();

            this.mailConfiguration.Password.ToList()
                .ForEach(character =>
                    securePasswordString.AppendChar(character));

            return securePasswordString;
        }

        private async ValueTask SendEmailMessageAsync(Message message)
        {
            SecureString securePasswordString = GetSecurePasswordString();

            IUserSendMailRequestBuilder emailRequestBuidler =
                this.graphServiceClient.Me.SendMail(message);

            IUserSendMailRequest userSendMailRequest =
                emailRequestBuidler.Request();

            userSendMailRequest.WithUsernamePassword(
                email: this.mailConfiguration.Email,
                password: securePasswordString);

            await userSendMailRequest.PostAsync();
        }
    }
}
