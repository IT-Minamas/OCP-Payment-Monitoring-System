namespace OCPPaymentSystemAPI.Models
{
    public class PriceRangeCreateRequest
    {
        public string MillCode { get; set; } = "";

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public decimal PriceFrom { get; set; }

        public decimal PriceTo { get; set; }

        public string UserName { get; set; } = "";

        public string UserIP { get; set; } = "";
    }
}