using System.ComponentModel.DataAnnotations;

namespace EGMS.DTOs
{
    public class CustomerCreateDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Father's name is required")]
        [StringLength(100, ErrorMessage = "Father's name cannot exceed 100 characters")]
        public string F_name { get; set; }

        [Required(ErrorMessage = "Mother's name is required")]
        [StringLength(100, ErrorMessage = "Mother's name cannot exceed 100 characters")]
        public string M_name { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^(\+880|880|0)1[3-9]\d{8}$", ErrorMessage = "Please enter a valid Bangladeshi mobile number")]
        public string Mobile_number { get; set; }

        [Required(ErrorMessage = "NID number is required")]
        [RegularExpression(@"^(?:\d{10}|\d{13}|\d{17})$", ErrorMessage = "NID must be 10, 13, or 17 digits")]
        public string NID_Number { get; set; }

        [Required(ErrorMessage = "Pervious Unit is required")]
        public string Previous_Unit { get; set; }

        [Required(ErrorMessage = "Advance money is required")]
        public string Advance_money { get; set; }
    }
}
