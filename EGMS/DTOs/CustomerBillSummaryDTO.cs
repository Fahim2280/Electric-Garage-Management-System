namespace EGMS.DTOs
{
    public class CustomerBillSummaryDTO
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal LastMeterReading { get; set; }
        public decimal PreviousDues { get; set; }
        public DateTime? LastBillDate { get; set; }
    }
}
 