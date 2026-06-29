using System.ComponentModel.DataAnnotations;

namespace OCPPaymentSystemAPI.Models
{
    public class SDGWeighRequest
    {
        [Required]
        public string SupplierCode { get; set; } = "";

        [Required]
        public DateTime DateFrom { get; set; }

        [Required]
        public DateTime DateTo { get; set; }
    }
}
