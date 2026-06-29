using System.ComponentModel.DataAnnotations;

namespace OCPPaymentSystemAPI.Models
{
    public class MemoCreateRequest
    {
        [Required]
        public string CompanyCode { get; set; } = "";

        [Required]
        public string SupplierCode { get; set; } = "";

        [Required]
        public DateTime MemoDate { get; set; }

        public decimal Amount { get; set; }

        public string Remarks { get; set; } = "";

        public string UserName { get; set; } = "";

        public string UserIP { get; set; } = "";
        public string MillCode { get; set; } = "";
    }
}