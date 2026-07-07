namespace OCPPaymentSystemAPI.Models
{
    public class DashboardRequest
    {
        public List<string> CompanyAccess { get; set; }
            = new();
    }


    public class DashboardResponse
    {
        public int Draft { get; set; }


        public List<DashboardApproval> Approval { get; set; }
            = new();
    }


    public class DashboardApproval
    {
        public int ApprovalLevel { get; set; }

        public string ApprovalName { get; set; } = "";

        public int Total { get; set; }
    }
}