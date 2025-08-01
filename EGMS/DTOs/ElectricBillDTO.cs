﻿using System.ComponentModel.DataAnnotations;

namespace EGMS.DTOs
{
    public class ElectricBillDTO
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "Customer")]
        public int Customer_ID { get; set; }

        [Required]
        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Display(Name = "Previous Unit")]
        public decimal Previous_unit { get; set; }

        [Required]
        [Display(Name = "Current Unit")]
        [Range(0, double.MaxValue, ErrorMessage = "Current unit must be greater than or equal to 0")]
        public decimal Current_Unit { get; set; } // User input

        [Display(Name = "Total Unit")]
        public decimal Total_Unit { get; set; } // Calculated

        [Display(Name = "Electric Bill")]
        public decimal Electric_bill { get; set; } // Calculated

        [Display(Name = "Previous Dues")]
        public decimal Previous_duos { get; set; } // From previous bill or customer advance

        [Required]
        [Display(Name = "Rent Bill")]
        [Range(0, double.MaxValue, ErrorMessage = "Rent bill must be greater than or equal to 0")]
        public decimal Rent_Bill { get; set; } // User input

        [Display(Name = "Total Bill")]
        public decimal Total_bill { get; set; } // Calculated

        [Required]
        [Display(Name = "Clear Money")]
        [Range(0, double.MaxValue, ErrorMessage = "Clear money must be greater than or equal to 0")]
        public decimal Clear_money { get; set; } // User input

        [Display(Name = "Present Dues")]
        public decimal Present_dues { get; set; } // Calculated

        // Additional properties for display
        public string CustomerName { get; set; }

       
    }

}
