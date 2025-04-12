using System.ComponentModel.DataAnnotations;

namespace Omail.Data.Models
{
    public class Section
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty; // Setting a default value
        
        public int DepartmentId { get; set; }
        
        // Navigation properties
        public Department Department { get; set; }
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
