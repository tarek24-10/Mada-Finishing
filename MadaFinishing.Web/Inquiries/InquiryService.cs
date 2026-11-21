using MadaFinishing.Web.Data;
using MadaFinishing.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace MadaFinishing.Web.Inquiries;

public sealed class InquiryService(MadaFinishingDbContext database)
{
    public async Task CreateAsync(InquiryInput input, CancellationToken cancellationToken)
    {
        var inquiry = Inquiry.Create(
            input.CustomerName, input.Phone, input.ServiceType, input.Message, DateTimeOffset.UtcNow);
        database.Inquiries.Add(inquiry);
        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InquiryListRow>> ListAsync(
        string staffId, bool isManager, string? search, InquiryStatus? status, CancellationToken cancellationToken)
    {
        var inquiries = VisibleInquiries(staffId, isManager);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            inquiries = inquiries.Where(inquiry => inquiry.CustomerName.Contains(term) || inquiry.Phone.Contains(term));
        }

        if (status is not null)
        {
            inquiries = inquiries.Where(inquiry => inquiry.Status == status);
        }

        return await inquiries
            .OrderByDescending(inquiry => inquiry.CreatedAt)
            .ThenByDescending(inquiry => inquiry.Id)
            .Select(inquiry => new InquiryListRow(
                inquiry.Id, inquiry.CustomerName, inquiry.Phone, inquiry.ServiceType, inquiry.CreatedAt, inquiry.Status,
                inquiry.Coordinator == null ? null : inquiry.Coordinator.DisplayName))
            .ToListAsync(cancellationToken);
    }

    public async Task<InquiryDetailsViewModel?> DetailsAsync(
        int id, string staffId, bool isManager, CancellationToken cancellationToken)
    {
        var details = await VisibleInquiries(staffId, isManager)
            .Where(inquiry => inquiry.Id == id)
            .Select(inquiry => new InquiryDetailsViewModel(
                inquiry.Id, inquiry.CustomerName, inquiry.Phone, inquiry.ServiceType, inquiry.Message,
                inquiry.CreatedAt, inquiry.Status, inquiry.CoordinatorId,
                inquiry.Coordinator == null ? null : inquiry.Coordinator.DisplayName))
            .SingleOrDefaultAsync(cancellationToken);
        if (details is null)
        {
            return null;
        }

        var history = await database.InquiryAssignments.AsNoTracking()
            .Where(entry => entry.InquiryId == id && (isManager || entry.Inquiry.CoordinatorId == staffId))
            .OrderByDescending(entry => entry.AssignedAt)
            .ThenByDescending(entry => entry.Id)
            .Select(entry => new InquiryAssignmentRow(
                entry.Actor.DisplayName,
                entry.PreviousCoordinator == null ? null : entry.PreviousCoordinator.DisplayName,
                entry.NewCoordinator.DisplayName, entry.AssignedAt))
            .ToListAsync(cancellationToken);

        var coordinators = isManager && details.Status != InquiryStatus.Closed
            ? await CoordinatorAccounts()
                .OrderBy(user => user.DisplayName)
                .ThenBy(user => user.Id)
                .Select(user => new CoordinatorOption(user.Id, user.DisplayName))
                .ToListAsync(cancellationToken)
            : [];

        return details with { History = history, Coordinators = coordinators };
    }

    public async Task<AssignmentResult> AssignAsync(
        int id, string? coordinatorId, string managerId, CancellationToken cancellationToken)
    {
        var inquiry = await database.Inquiries.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (inquiry is null)
        {
            return AssignmentResult.InquiryNotFound;
        }

        if (!inquiry.CanBeAssigned)
        {
            return AssignmentResult.InquiryClosed;
        }

        if (string.IsNullOrEmpty(coordinatorId) ||
            !await CoordinatorAccounts().AnyAsync(user => user.Id == coordinatorId, cancellationToken))
        {
            return AssignmentResult.InvalidCoordinator;
        }

        var assignment = inquiry.AssignTo(coordinatorId, managerId, DateTimeOffset.UtcNow);
        if (assignment is null)
        {
            return AssignmentResult.Unchanged;
        }

        database.InquiryAssignments.Add(assignment);
        try
        {
            await database.SaveChangesAsync(cancellationToken);
            return AssignmentResult.Assigned;
        }
        catch (DbUpdateConcurrencyException)
        {
            return AssignmentResult.Conflict;
        }
    }

    private IQueryable<Inquiry> VisibleInquiries(string staffId, bool isManager) =>
        database.Inquiries.AsNoTracking().Where(inquiry => isManager || inquiry.CoordinatorId == staffId);

    private IQueryable<StaffUser> CoordinatorAccounts()
    {
        var coordinatorRole = StaffUser.CoordinatorRole.ToUpperInvariant();
        return from user in database.Users
               join membership in database.UserRoles on user.Id equals membership.UserId
               join role in database.Roles on membership.RoleId equals role.Id
               where role.NormalizedName == coordinatorRole
               select user;
    }
}
