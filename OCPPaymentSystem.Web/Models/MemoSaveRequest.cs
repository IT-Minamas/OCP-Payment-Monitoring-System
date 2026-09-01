namespace OCPPaymentSystem.Web.Models
{
    public class MemoSaveRequest
    {
        public string memoNo { get; set; } = "";
        public string companyCode { get; set; } = "";
        public string supplierCode { get; set; } = "";
        public DateTime memoDate { get; set; }

        public string perihal { get; set; } = "";
        public string invoiceNo { get; set; } = "";
        public string bankCode { get; set; } = "";
        
        public decimal amount { get; set; }
        public string remarks { get; set; } = "";
        public string userName { get; set; } = "";
        public string userIP { get; set; } = "";
        public string millCode { get; set; } = "";

        public decimal? NettWeight { get; set; }
        public decimal? Deduction { get; set; }
        public decimal? PricePerKg { get; set; }
        public decimal? PPN { get; set; }
        public decimal? PPH { get; set; }

    }
}