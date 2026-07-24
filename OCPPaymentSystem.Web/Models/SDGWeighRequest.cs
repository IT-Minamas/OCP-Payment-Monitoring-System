namespace OCPPaymentSystem.Web.Models
{
    public class SDGWeighRequest
    {
        public string SupplierCode { get; set; } = "";

        public string MillCode { get; set; } = "";

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }
    }
}