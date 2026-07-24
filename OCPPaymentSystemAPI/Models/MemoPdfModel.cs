namespace OCPPaymentSystemAPI.Models
{
    public class MemoPdfModel
    {
        public string MemoNo { get; set; } = "";
        public DateTime MemoDate { get; set; }

        public string CompanyName { get; set; } = "";
        public string SupplierCode { get; set; } = "";
        public string SupplierName { get; set; } = "";

        public string InvoiceNo { get; set; } = "";
        public decimal Amount { get; set; }

        public string BankName { get; set; } = "";
        public string AccountNo { get; set; } = "";
        public string AccountName { get; set; } = "";

        public string Remarks { get; set; } = "";

        public string CreatedBy { get; set; } = "";
        public DateTime CreatedOn { get; set; }

        public string ApprovedBy { get; set; } = "";
        public DateTime? ApprovedOn { get; set; }
    }
}