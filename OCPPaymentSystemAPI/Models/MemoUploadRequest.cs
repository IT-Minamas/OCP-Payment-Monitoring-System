using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace OCPPaymentSystemAPI.Models
{
    public class MemoUploadRequest
    {
        [Required]
        public string MemoNo { get; set; } = "";

        [Required]
        public string DocumentType { get; set; } = "";

        [Required]
        public IFormFile File { get; set; } = null!;

        public string UserName { get; set; } = "";

        public string UserIP { get; set; } = "";
    }
}