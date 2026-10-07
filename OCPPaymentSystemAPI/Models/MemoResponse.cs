namespace OCPPaymentSystemAPI.Models
{
    public class MemoResponse
    {
        public string MemoNo { get; set; } = "";
        public string CompanyCode { get; set; } = "";
        public string SupplierCode { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public DateTime MemoDate { get; set; }
        public string perihal { get; set; } = "";
        public string invoiceNo { get; set; } = "";
        public string bankCode { get; set; } = "";
        public decimal Amount { get; set; }
        public string Remarks { get; set; } = "";
        public string CreatedBy { get; set; } = "";
        public DateTime CreatedOn { get; set; }
        public int ApprovalLevel { get; set; } = -10;
        public string ApprovalStatus { get; set; } = "";
        public string ApprovalRemarks { get; set; } = "";
        public string ApprovedBy { get; set; } = "";
        public DateTime? ApprovedOn { get; set; }
        public DateTime? ApprovalCreatedOn { get; set; }
        public string? MillCode { get; set; } = "";
        public string? MillAbbv { get; set; } = "";
        public string? Invoice { get; set; } = "";
        public string? BAP { get; set; } = "";
        public string? FakturPajak { get; set; } = "";
        public string? Memo { get; set; } = "";
        public decimal? NettWeight { get; set; }
        public decimal? Deduction { get; set; }
        public decimal? PricePerKg { get; set; }
        public decimal? PPN { get; set; }
        public decimal? PPH { get; set; }
    }
}