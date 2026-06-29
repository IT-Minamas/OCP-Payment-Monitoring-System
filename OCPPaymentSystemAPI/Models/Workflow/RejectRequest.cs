using System.ComponentModel.DataAnnotations;

namespace OCPPaymentSystemAPI.Models.Workflow
{
    public class RejectRequest
    {
        [Required]
        public string MemoNo { get; set; } = "";

        [Required]
        public int ApprovalLevel { get; set; }

        public string Remarks { get; set; } = "";

        [Required]
        public string UserName { get; set; } = "";

        [Required]
        public string UserIP { get; set; } = "";
    }
}