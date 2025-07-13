using System.ComponentModel.DataAnnotations;

namespace EGMS.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
