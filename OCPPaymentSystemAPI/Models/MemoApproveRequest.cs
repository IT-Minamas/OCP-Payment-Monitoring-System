using System.ComponentModel.DataAnnotations;

namespace OCPPaymentSystemAPI.Models
{
    public class MemoApproveRequest
    {
        [Required]
        public string MemoNo { get; set; } = "";

        [Required]
        public string UserName { get; set; } = "";

        public string Remarks { get; set; } = "";

        [Required]
        public string UserIP { get; set; } = "";
    }
}