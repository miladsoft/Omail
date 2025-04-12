using System.ComponentModel.DataAnnotations;

namespace Omail.Data.Models
{
    public class Department
    {
        public int Id { get; set; }
        
        [Required, MaxLength(100)]
        public string Name { get; set; }
        
        // Foreign keys
        public int OrganizationId { get; set; }
        
        // Navigation properties
        public virtual Organization Organization { get; set; }
        public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
    }
}
