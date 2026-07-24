namespace OCPPaymentSystemAPI.Models
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
        public string AttachmentContentType { get; set; } = "";
        public long AttachmentFileSize { get; set; }

        public string CreatedBy { get; set; } = "";

        public DateTime CreatedOn { get; set; }

        public string CreatedIP { get; set; } = "";

        public string UpdatedBy { get; set; } = "";

        public DateTime? UpdatedOn { get; set; }

        public string UpdatedIP { get; set; } = "";
    }
}