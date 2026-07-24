namespace OCPPaymentSystemAPI.Models
{
    public class MemoDetailModel
    {
        public string MemoNo { get; set; } = "";
        public string MillCode { get; set; } = "";
        public string TicketNo { get; set; } = "";

        public DateTime PostDate { get; set; }
        public string LorryNo { get; set; } = "";
        public string DriverCode { get; set; } = "";
        public string SerialNo { get; set; } = "";
        public decimal BunchWeight { get; set; }
        public decimal WeightIn { get; set; }
        public decimal WeightOut { get; set; }
        public decimal NettWeight { get; set; }
        public string SupplierCode { get; set; } = "";
        public string DELIVERY_ORDER_NO { get; set; } = "";
        public decimal DED_WT { get; set; }
    }
}