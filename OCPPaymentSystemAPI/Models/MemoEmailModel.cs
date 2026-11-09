namespace OCPPaymentSystemAPI.Models
{
    public class MemoEmailModel
    {
        public string fldSupplierName { get; set; } = "";
        public DateTime? fldDate { get; set; }
        public decimal fldAmount { get; set; }
        public string MillAbbv { get; set; } = "";
        public string fldInvoice { get; set; } = "";
    }
}
