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

        public string Dibuat { get; set; } = "";
        public string JabatanDibuat { get; set; } = "";
        public string Disetujui { get; set; } = "";
        public string JabatanDisetujui { get; set; } = "";
        public string Approved { get; set; } = "";
        public string JabatanApproved { get; set; } = "";

        public string Region { get; set; } = "";
        public string Remarks { get; set; } = "";
        public string Perihal { get; set; } = "";

        public string CreatedBy { get; set; } = "";
        public DateTime CreatedOn { get; set; }

        public string ApprovedBy { get; set; } = "";
        public DateTime? ApprovedOn { get; set; }

        // =========================================================
        // APPROVAL LIST
        // =========================================================

        public List<MemoApprover> Approvers { get; set; } = new();
    }

    public class MemoApprover
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Designation { get; set; } = "";
        public DateTime? Date { get; set; }
    }
}