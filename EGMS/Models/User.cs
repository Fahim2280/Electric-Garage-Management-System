using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EGMS.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } // "SuperAdmin" or "Admin"

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(15)]
        public string Number { get; set; } // Phone number

        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; }

        // Navigation property for customers
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}