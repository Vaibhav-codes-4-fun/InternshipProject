using System.ComponentModel.DataAnnotations;

namespace InternshipProject.Models
{
    public class EmpLoginModel
    {
        [Key]
        public Guid EmpId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Department { get; set; }

        public required string Password { get; set; }
    }
}
