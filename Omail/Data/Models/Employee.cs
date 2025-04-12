using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Omail.Data.Models
{
    public class Employee
    {
        public int Id { get; set; }
        
        [Required, MaxLength(50)]
        public string FirstName { get; set; }
        
        [Required, MaxLength(50)]
        public string LastName { get; set; }
        
        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        
        public string ProfilePicture { get; set; }
        
        public string Position { get; set; }
        
        public bool IsManager { get; set; }
        
        // Foreign keys
        public int SectionId { get; set; }
        
        // Navigation properties
        public virtual Section Section { get; set; }
        public virtual ICollection<EmailRecipient> ReceivedEmails { get; set; } = new List<EmailRecipient>();
        public virtual ICollection<EmailMessage> SentEmails { get; set; } = new List<EmailMessage>();
        public virtual ICollection<EmailApproval> PendingApprovals { get; set; } = new List<EmailApproval>();
        
        // Helper property to get full name
        public string FullName => $"{FirstName} {LastName}";
        
        // Helper property to get initials (for avatar)
        public string Initials => $"{FirstName[0]}{LastName[0]}".ToUpper();
    }
}
