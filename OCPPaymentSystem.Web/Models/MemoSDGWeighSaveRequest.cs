namespace OCPPaymentSystem.Web.Models
{
    public class MemoSDGWeighSaveRequest
    {
        public string MemoNo { get; set; } = "";

        public string MillCode { get; set; } = "";

        public List<string> Tickets { get; set; } = new();
    }
}