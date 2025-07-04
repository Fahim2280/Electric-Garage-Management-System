namespace EGMS.DTOs
{
    public class ElectricBillPreviewDTO
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal PreviousMeterReading { get; set; }
        public decimal CurrentMeterReading { get; set; }
        public decimal ConsumedUnits { get; set; }
        public decimal ElectricBill { get; set; }
        public decimal RentBill { get; set; }
        public decimal PreviousDues { get; set; }
        public decimal TotalBill { get; set; }
    }
}
