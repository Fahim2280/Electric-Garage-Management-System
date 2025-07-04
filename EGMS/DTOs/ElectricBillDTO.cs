using System.ComponentModel.DataAnnotations;

namespace EGMS.DTOs
{
    public class ElectricBillDTO
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        public int Customer_ID { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Previous unit is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Previous unit must be a positive number")]
        public decimal Previous_unit { get; set; }

        [Required(ErrorMessage = "Total unit is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Total unit must be a positive number")]
        public decimal Total_Unit { get; set; }

        [Required(ErrorMessage = "Electric bill is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Electric bill must be a positive number")]
        public decimal Electric_bill { get; set; }

        [Required(ErrorMessage = "Rent bill is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Rent bill must be a positive number")]
        public decimal Rent_Bill { get; set; }

        [Required(ErrorMessage = "Clear money is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Clear money must be a positive number")]
        public decimal Clear_money { get; set; }

        // Read-only properties for display
        public string? CustomerName { get; set; }
        public decimal? Previous_duos { get; set; }
        public decimal? Total_bill { get; set; }
        public decimal? Present_dues { get; set; }
    }
}
