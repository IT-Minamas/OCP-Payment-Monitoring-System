namespace OCPPaymentSystemAPI.Models
{
    public class MemoAttachmentDownload
    {
        public string FileName { get; set; } = "";

        public string ContentType { get; set; } = "";

        public byte[] FileData { get; set; } = [];
    }
}