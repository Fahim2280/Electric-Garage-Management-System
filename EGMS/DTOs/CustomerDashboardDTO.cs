namespace EGMS.DTOs
{
    public class CustomerDashboardDTO
    {
        public int C_ID { get; set; }
        public string Name { get; set; }
        public string Mobile_number { get; set; }
        public decimal Present_dues { get; set; }
        public DateTime LastBillDate { get; set; }
        public bool HasBills { get; set; }

        // Helper property for display
        public string DuesStatus => Present_dues > 0 ? "Due" : Present_dues < 0 ? "Advance" : "Clear";
        public string DuesAmount => Math.Abs(Present_dues).ToString("F2");
    }
}
