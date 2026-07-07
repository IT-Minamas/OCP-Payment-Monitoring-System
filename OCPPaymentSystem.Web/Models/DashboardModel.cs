namespace OCPPaymentSystem.Web.Models
{
    public class DashboardModel
    {
        public int Draft { get; set; }

        public List<ApprovalSummary> Approval { get; set; }
            = new();
    }


    public class ApprovalSummary
    {
        public int ApprovalLevel { get; set; }

        public string ApprovalName { get; set; } = "";

        public int Total { get; set; }
    }
}