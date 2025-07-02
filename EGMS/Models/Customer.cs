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
        [StringLength(17)] // Changed from 10 to 17 to support 10, 13, or 17 digit NIDs
        public string NID_Number { get; set; }

        public DateTime Created_Date { get; set; }

        public Customer()
        {
            Created_Date = DateTime.UtcNow;
        }
    }
}