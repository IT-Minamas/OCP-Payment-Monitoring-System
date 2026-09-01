using System.ComponentModel.DataAnnotations;

namespace OCPPaymentSystemAPI.Models
{
    public class MemoUpdateRequest
    {
        [Required]
        public string MemoNo { get; set; } = "";
        [Required]
        public string CompanyCode { get; set; } = "";
        [Required]
        public string SupplierCode { get; set; } = "";
        [Required]
        public DateTime MemoDate { get; set; }
        public string perihal { get; set; } = "";
        public string invoiceNo { get; set; } = "";
        public string bankCode { get; set; } = "";
        public decimal Amount { get; set; }
        public string Remarks { get; set; } = "";
        public string UserName { get; set; } = "";
        public string UserIP { get; set; } = "";
        public decimal? NettWeight { get; set; }
        public decimal? Deduction { get; set; }
        public decimal? PricePerKg { get; set; }
        public decimal? PPN { get; set; }
        public decimal? PPH { get; set; }
    }
}