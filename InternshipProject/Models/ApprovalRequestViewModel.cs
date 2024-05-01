using System.ComponentModel.DataAnnotations;

namespace InternshipProject.Models
{
        public class ApprovalRequestViewModel
        {
            [Key]
            public Guid Id { get; set; }
            [Required]
            public string? Department { get; set; }
            [Required]
            public string? Email { get; set; }
            [Required]
            public string? Topic { get; set; }
            public DateTime Created { get; set; } = DateTime.Now;
            public List<ResponseViewModel> ApproverEmails { get; set; } = new List<ResponseViewModel>();

            public bool? Status { get; set; }
    }
}



