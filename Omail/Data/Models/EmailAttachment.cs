using System.ComponentModel.DataAnnotations;

namespace Omail.Data.Models
{
    public class EmailAttachment
    {
        public int Id { get; set; }
        
        [Required, MaxLength(255)]
        public string FileName { get; set; }
        
        [Required]
        public string FilePath { get; set; }
        
        public long FileSize { get; set; }
        
        [MaxLength(100)]
        public string ContentType { get; set; }
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign keys
        public int EmailId { get; set; }
        
        // Navigation properties
        public virtual EmailMessage Email { get; set; }
    }
}
