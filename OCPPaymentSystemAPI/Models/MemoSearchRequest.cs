namespace OCPPaymentSystemAPI.Models
{
    public class MemoSearchRequest
    {
        public int ApprovalLevel { get; set; } = -10;
        public List<string> CompanyAccess { get; set; } = new();
        public string? millCode { get; set; } = "";
        public string MemoNo { get; set; } = "";
        public string SupplierCode { get; set; } = "";
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }



        public string CompanyCode { get; set; } = "";
        public decimal? AmountFrom { get; set; }
        public decimal? AmountTo { get; set; }
        public string Remarks { get; set; } = "";
    }
}