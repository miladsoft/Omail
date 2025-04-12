using System.ComponentModel.DataAnnotations;

namespace Omail.Data.Models
{
    public class Department
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty; // Setting a default value
        
        public int OrganizationId { get; set; }
        
        // Navigation properties
        public Organization Organization { get; set; }
        public ICollection<Section> Sections { get; set; } = new List<Section>();
    }
}
