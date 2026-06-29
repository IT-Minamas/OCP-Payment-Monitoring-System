namespace OCPPaymentSystemAPI.Models
{
    public class SDGWeighResponse
    {
        public bool Selected { get; set; } = false;

        public string MillCode { get; set; } = "";

        public string SupplierCode { get; set; } = "";

        public DateTime Date { get; set; }

        public string TicketNo { get; set; } = "";

        public string PoliceNo { get; set; } = "";

        public string Driver { get; set; } = "";

        public int BunchCount { get; set; }

        public decimal Bruto { get; set; }

        public decimal Tarra { get; set; }

        public decimal Netto { get; set; }
    }
}