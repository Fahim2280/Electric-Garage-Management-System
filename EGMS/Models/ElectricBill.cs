using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EGMS.Models
{
    public class ElectricBill
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [ForeignKey("Customer")]
        public int Customer_ID { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Previous_unit { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Current_Unit { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total_Unit { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Electric_bill { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Previous_duos { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Rent_Bill { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Loan { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total_bill { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Clear_money { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Present_dues { get; set; }

        // Navigation property
        public virtual Customer Customer { get; set; }
    }
}