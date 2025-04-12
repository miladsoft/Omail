using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Omail.Data.Models
{
    public class Employee
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        
        // Change to computed property, not stored in database
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
        
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        [MaxLength(15)]
        public string PhoneNumber { get; set; } = string.Empty; // Default value to avoid null
        
        [MaxLength(100)]
        public string Position { get; set; } = string.Empty; // Default value
        
        public string ProfilePicture { get; set; } = string.Empty; // Default value
        
        public bool IsManager { get; set; }
        
        public bool IsActive { get; set; }
        
        [Required]
        public int SectionId { get; set; }
        
        public DateTime HireDate { get; set; }
        
        // Navigation property
        public Section Section { get; set; }
        
        public ICollection<EmailMessage> SentEmails { get; set; } = new List<EmailMessage>();
        public ICollection<EmailRecipient> ReceivedEmails { get; set; } = new List<EmailRecipient>();
        public ICollection<EmailApproval> EmailApprovals { get; set; } = new List<EmailApproval>();
        
        [NotMapped]
        public ICollection<EmailApproval> PendingApprovals { get; set; } = new List<EmailApproval>();
        
        // Helper property for avatar display
        public string Initials => $"{FirstName?.FirstOrDefault() ?? '?'}{LastName?.FirstOrDefault() ?? '?'}";
    }
}
