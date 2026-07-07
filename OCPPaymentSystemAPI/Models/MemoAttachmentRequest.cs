using Microsoft.AspNetCore.Http;

namespace OCPPaymentSystemAPI.Models
{
    public class MemoAttachmentRequest
    {
        public string? memoNo { get; set; }

        public string? type { get; set; }

        public IFormFile? file { get; set; }
    }
}