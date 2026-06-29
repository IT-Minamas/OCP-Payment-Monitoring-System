using System.ComponentModel.DataAnnotations;

namespace OCPPaymentSystemAPI.Models
{
    public class OCPSupplierRequest
    {
        [Required]
        public string MillCode { get; set; } = "";

    }
}
