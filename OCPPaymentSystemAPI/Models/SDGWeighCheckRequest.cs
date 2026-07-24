namespace OCPPaymentSystemAPI.Models
{
    public class SDGWeighCheckRequest
    {
        public string MemoNo { get; set; } = "";

        public string MillCode { get; set; } = "";

        public List<string> Tickets { get; set; } = new();
    }
}