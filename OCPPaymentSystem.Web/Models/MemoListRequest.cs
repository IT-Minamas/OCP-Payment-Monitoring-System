namespace OCPPaymentSystem.Web.Models
{
    public class MemoListRequest
    {
        public int approvalLevel { get; set; } = -10;
        public List<string> CompanyAccess { get; set; } = new();
        public string millCode { get; set; } = "";
        public string memoNo { get; set; } = "";
        public string supplierCode { get; set; } = "";
        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
    }
}
