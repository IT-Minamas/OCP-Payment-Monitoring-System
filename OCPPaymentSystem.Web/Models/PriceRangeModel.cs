namespace OCPPaymentSystem.Web.Models
{
    public class PriceRangeModel
    {
        public int ID { get; set; }

        public string MillCode { get; set; } = "";

        public string MillName { get; set; } = "";

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public decimal PriceFrom { get; set; }

        public decimal PriceTo { get; set; }

        public string AttachmentFileName { get; set; } = "";
    }
}