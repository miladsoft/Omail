namespace Omail.Data.Models
{
    public class EmailApproval
    {
        public int Id { get; set; }
        public int EmailId { get; set; }
        public int ApproverId { get; set; }
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
        public string Comments { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        
        // Navigation properties
        public virtual EmailMessage Email { get; set; }
        public virtual Employee Approver { get; set; }
    }
    
    public enum ApprovalStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
