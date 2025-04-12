using System.ComponentModel.DataAnnotations;

namespace Omail.Data.Models
{
    public class Section
    {
        public int Id { get; set; }
        
        [Required, MaxLength(100)]
        public string Name { get; set; }
        
        // Foreign keys
        public int DepartmentId { get; set; }
        
        // Navigation properties
        public virtual Department Department { get; set; }
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
