using System.ComponentModel.DataAnnotations;

namespace Omail.Data.Models
{
    public class Organization
    {
        public int Id { get; set; }
        
        [Required, MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(500)]
        public string Description { get; set; }
        
        // Navigation properties
        public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    }
}
