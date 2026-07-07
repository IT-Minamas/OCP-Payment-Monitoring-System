namespace OCPPaymentSystem.Web.Models
{
    public class MemoAttachmentRequest
    {
        public string MemoNo { get; set; } = "";

        public string Type { get; set; } = "";

        public IFormFile File { get; set; }
    }
}
