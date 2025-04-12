using System.ComponentModel.DataAnnotations;

namespace Omail.Data.Models
{
    public class EmailMessage
    {
        public int Id { get; set; }
        
        [Required, MaxLength(200)]
        public string Subject { get; set; }
        
        [Required]
        public string Body { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? SentAt { get; set; }
        
        public EmailStatus Status { get; set; } = EmailStatus.Draft;
        
        public bool IsDeleted { get; set; } = false;
        
        public DateTime? DeletedAt { get; set; }
        
        public bool RequiresDepartmentalApproval { get; set; }
        
        // Foreign keys
        public int SenderId { get; set; }
        
        // Navigation properties
        public virtual Employee Sender { get; set; }
        public virtual ICollection<EmailRecipient> Recipients { get; set; } = new List<EmailRecipient>();
        public virtual ICollection<EmailApproval> Approvals { get; set; } = new List<EmailApproval>();
        public virtual ICollection<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
    }
    
    public enum EmailStatus
    {
        Draft,
        PendingApproval,
        Approved,
        Rejected,
        Sent,
        Received
    }
}
