namespace OCPPaymentSystemAPI.Models
{
    public class PriceRangeAttachmentRequest
    {
        public int ID { get; set; }

        public IFormFile file { get; set; } = null!;
    }
}