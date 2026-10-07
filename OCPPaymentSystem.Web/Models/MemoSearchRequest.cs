public class MemoSearchRequest
{
    public List<string> CompanyAccess { get; set; }
        = new();
    public string CompanyCode { get; set; } = "";
    public string SupplierCode { get; set; } = "";
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public decimal? AmountFrom { get; set; }
    public decimal? AmountTo { get; set; }
    public string Remarks { get; set; } = "";
    public string Approver { get; set; } = "";
}