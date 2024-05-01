using Microsoft.EntityFrameworkCore;

namespace InternshipProject.Models
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<ApprovalRequestViewModel> Requests { get; set; }
        public DbSet<ResponseViewModel> Responses { get; set; }
        public DbSet<EmpLoginModel> UserDetails { get; set; }
    }
}
