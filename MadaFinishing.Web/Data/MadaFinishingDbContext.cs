using MadaFinishing.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MadaFinishing.Web.Data;

public sealed class MadaFinishingDbContext(DbContextOptions<MadaFinishingDbContext> options)
    : IdentityDbContext<StaffUser>(options)
{
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();
}
