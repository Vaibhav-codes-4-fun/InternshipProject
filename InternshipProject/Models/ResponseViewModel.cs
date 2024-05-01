using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InternshipProject.Models
{
    public class ResponseViewModel
    {
            [Key]
            [Required]
            public int ResponseId { get; set; }
            public string ApproverEmail { get; set; }
            public bool? Response { get; set; }
            public Guid ApprovalRequestViewModelId { get; set; }

            [ForeignKey("ApprovalRequestViewModelId")]
            public ApprovalRequestViewModel Request { get; set; }

    }
}

