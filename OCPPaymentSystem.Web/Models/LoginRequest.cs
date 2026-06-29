namespace OCPPaymentSystem.Web.Models
{
    public class LoginRequest
    {
        public string fldUserId { get; set; } = "";

        public string fldPassword { get; set; } = "";

        public string fldName { get; set; } = "";

        public string fldApiKey { get; set; } = "";
    }
}