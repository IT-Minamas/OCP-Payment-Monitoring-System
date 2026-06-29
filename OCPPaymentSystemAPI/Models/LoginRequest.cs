namespace OCPPaymentSystemAPI.Models
{
    public class LoginRequest
    {
        public string fldUserId { get; set; } = "";

        public string fldPassword { get; set; } = "";

        public int fldApplicationID { get; set; } = 0;
    }
}