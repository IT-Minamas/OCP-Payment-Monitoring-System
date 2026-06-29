using System.ComponentModel.DataAnnotations;

namespace OCPPaymentSystemAPI.Models.Workflow
{
    public class MemoSubmitRequest
    {
        [Required]
        public string MemoNo { get; set; } = "";

        [Required]
        public string UserName { get; set; } = "";

        [Required]
        public string UserIP { get; set; } = "";
    }
}