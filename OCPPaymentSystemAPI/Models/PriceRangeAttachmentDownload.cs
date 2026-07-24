namespace OCPPaymentSystemAPI.Models
{
    public class PriceRangeAttachmentDownload
    {
        public string FileName { get; set; } = "";

        public string ContentType { get; set; } = "";

        public byte[] FileData { get; set; } = Array.Empty<byte>();
    }
}