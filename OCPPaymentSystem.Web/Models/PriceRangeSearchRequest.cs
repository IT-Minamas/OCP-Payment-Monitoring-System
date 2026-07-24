namespace OCPPaymentSystem.Web.Models
{
    public class PriceRangeSearchRequest
    {
        public string? CompanyCode { get; set; }

        public string? MillCode { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }
}