using System.ComponentModel.DataAnnotations;

namespace Omail.Data.Models
{
    public class Organization
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty; // Setting a default value
        
        // Navigation properties
        public ICollection<Department> Departments { get; set; } = new List<Department>();
    }
}
