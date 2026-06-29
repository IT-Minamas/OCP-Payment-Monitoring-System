namespace OCPPaymentSystem.Web.Models
{
    public class LoginUser
    {
        public string fldUserId { get; set; } = "";
        public string fldName { get; set; } = "";
        public string fldApiKey { get; set; } = "";
        public string UnitCode { get; set; } = "";
        public string UnitName { get; set; } = "";
        public string CompanyCode { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string Role { get; set; } = "";
        public int ApprovalLevel { get; set; }
        public string ApprovalLevelName { get; set; }
        public List<string> CompanyAccess { get; set; } = new();
        public string BusinessTitle { get; set; }
        public string AreaName { get; set; } = "";
        public string RegionName { get; set; } = "";
    }
}