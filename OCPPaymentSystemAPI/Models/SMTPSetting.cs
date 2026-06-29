namespace OCPPaymentSystemAPI.Models
{
    public class SMTPSetting
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public bool EnableSSL { get; set; }
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string DisplayName { get; set; } = "";
    }
}