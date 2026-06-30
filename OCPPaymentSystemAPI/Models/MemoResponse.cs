namespace OCPPaymentSystemAPI.Models
{
    public class MemoResponse
    {
        public string MemoNo { get; set; } = "";
        public string CompanyCode { get; set; } = "";
        public string SupplierCode { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public DateTime MemoDate { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; } = "";
        public string CreatedBy { get; set; } = "";
        public DateTime CreatedOn { get; set; }
        public int ApprovalLevel { get; set; } = -10;
        public string ApprovalStatus { get; set; } = "";
        public string ApprovedBy { get; set; } = "";
        public DateTime? ApprovedOn { get; set; }
        public DateTime? ApprovalCreatedOn { get; set; }
        public string? MillCode { get; set; } = "";
    }
}