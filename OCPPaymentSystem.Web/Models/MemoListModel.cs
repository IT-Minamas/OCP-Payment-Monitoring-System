namespace OCPPaymentSystem.Web.Models
{
    public class MemoListModel
    {
        public int No { get; set; }
        public string memoNo { get; set; } = "";
        public string companyCode { get; set; } = "";
        public string supplierCode { get; set; } = "";
        public string supplierName { get; set; } = "";
        public DateTime memoDate { get; set; }
        public decimal amount { get; set; }
        public string remarks { get; set; } = "";
        public string createdBy { get; set; } = "";
        public DateTime createdOn { get; set; }
        public int approvalLevel { get; set; } = -10;
        public string approvalStatus { get; set; } = "";
        public string approvedBy { get; set; } = "";
        public DateTime? approvedOn { get; set; }
        public DateTime? approvalCreatedOn { get; set; }
        public string MillCode { get; set; } = "";
        public string? invoice { get; set; } = "";
        public string? bap { get; set; } = "";
        public string? fakturPajak { get; set; } = "";
        public string? memo { get; set; } = "";
    }
}