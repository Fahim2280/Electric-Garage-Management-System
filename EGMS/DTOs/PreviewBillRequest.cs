namespace EGMS.DTOs
{
    public class PreviewBillRequest
    {
        public int CustomerId { get; set; }
        public decimal CurrentMeterReading { get; set; }
        public decimal RentBill { get; set; }
    }
}
