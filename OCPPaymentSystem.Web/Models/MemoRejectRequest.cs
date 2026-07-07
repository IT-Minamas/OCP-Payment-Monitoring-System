namespace OCPPaymentSystem.Web.Models
{
    public class MemoRejectRequest
    {
        public string MemoNo { get; set; } = "";
        public string Remarks { get; set; } = "";
        public string UserName { get; set; } = "";
        public string UserIP { get; set; } = "";
    }
}