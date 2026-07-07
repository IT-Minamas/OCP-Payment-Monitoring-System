using System.ComponentModel.DataAnnotations;

namespace OCPPaymentSystemAPI.Models
{
    public class SupplierRequest
    {
        public string MillCode { get; set; } = "";

        public string CompanyCode { get; set; } = "";
    }
}
