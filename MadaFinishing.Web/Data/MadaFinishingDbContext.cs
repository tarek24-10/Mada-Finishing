using MadaFinishing.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MadaFinishing.Web.Data;

public sealed class MadaFinishingDbContext(DbContextOptions<MadaFinishingDbContext> options)
    : IdentityDbContext<StaffUser>(options)
{
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();
    public DbSet<InquiryAssignment> InquiryAssignments => Set<InquiryAssignment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var assignment = builder.Entity<InquiryAssignment>();
        assignment.HasOne(entry => entry.Actor).WithMany().OnDelete(DeleteBehavior.NoAction);
        assignment.HasOne(entry => entry.PreviousCoordinator).WithMany().OnDelete(DeleteBehavior.NoAction);
        assignment.HasOne(entry => entry.NewCoordinator).WithMany().OnDelete(DeleteBehavior.NoAction);
    }
}
