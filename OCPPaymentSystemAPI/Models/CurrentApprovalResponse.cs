namespace OCPPaymentSystemAPI.Models
{
    public class CurrentApprovalResponse
    {
        public string MemoNo { get; set; } = "";

        public int ApprovalLevel { get; set; }

        public string ApprovalDescription { get; set; } = "";

        public string Status { get; set; } = "";

        public bool IsCompleted { get; set; }
    }
}