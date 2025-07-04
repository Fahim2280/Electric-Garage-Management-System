using System.ComponentModel.DataAnnotations;

namespace EGMS.Models
{
    public class Customer
    {
        [Key]
        public int C_ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string F_name { get; set; }

        [Required]
        [StringLength(100)]
        public string M_name { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [StringLength(15)]
        public string Mobile_number { get; set; }

        [Required]
        [StringLength(17)]
        public string NID_Number { get; set; }

        public DateTime Created_Date { get; set; }

        [Required]
        public string Previous_Unit { get; set; }

        [Required]
        public string Advance_money { get; set; }

        // Navigation property for electric bills
        public virtual ICollection<ElectricBill> ElectricBills { get; set; } = new List<ElectricBill>();
    }
}