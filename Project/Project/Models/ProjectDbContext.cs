using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Project.Models
{
   
   

    public class ProjectDbContext : IdentityDbContext<ApplicationUser>
    {
        public ProjectDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public virtual IDbSet<Article> Articles { get; set; }

        

        public static ProjectDbContext Create()
        {
            return new ProjectDbContext();
        }
    }
}