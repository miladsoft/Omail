namespace Omail.Data.Models
{
    public class EmailRecipient
    {
        public int EmailId { get; set; }
        public int RecipientId { get; set; }
        public RecipientType Type { get; set; } = RecipientType.To;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        
        // Navigation properties
        public virtual EmailMessage Email { get; set; }
        public virtual Employee Recipient { get; set; }
    }
    
    public enum RecipientType
    {
        To,
        Cc,
        Bcc
    }
}
