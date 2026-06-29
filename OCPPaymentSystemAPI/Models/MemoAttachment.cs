namespace OCPPaymentSystemAPI.Models
{
    public class MemoAttachmentModel
    {
        public string DocumentType { get; set; } = "";

        public string FileName { get; set; } = "";

        public long FileSize { get; set; }

        public DateTime? UploadOn { get; set; }

        public string UploadedBy { get; set; } = "";
    }
}
