namespace OCPPaymentSystem.Web.Models
{
    public class LoginResponse
    {
        public string fldType { get; set; } = "";

        public string fldMessage { get; set; } = "";

        public string errorDebugMessageForDeveloper { get; set; } = "";

        public LoginUser user { get; set; } = new();
    }
}