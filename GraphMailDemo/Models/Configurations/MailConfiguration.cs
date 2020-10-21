namespace GraphMailDemo.Models.Configurations
{
    public class MailConfiguration
    {
        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string[] Scopes { get; set; }
    }
}
