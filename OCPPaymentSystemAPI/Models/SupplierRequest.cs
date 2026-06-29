using System.ComponentModel.DataAnnotations;

namespace OCPPaymentSystemAPI.Models
{
    public class SupplierRequest
    {
        [Required]
        public string MillCode { get; set; } = "";

    }
}
