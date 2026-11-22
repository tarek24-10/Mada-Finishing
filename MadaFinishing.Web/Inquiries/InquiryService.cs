using MadaFinishing.Web.Data;
using MadaFinishing.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace MadaFinishing.Web.Inquiries;

public sealed class InquiryService(MadaFinishingDbContext database)
{
    private const int InquiryPageSize = 5;

    public async Task CreateAsync(InquiryInput input, CancellationToken cancellationToken)
    {
        var inquiry = Inquiry.Create(
            input.CustomerName, input.Phone, input.ServiceType, input.Message, DateTimeOffset.UtcNow);
        database.Inquiries.Add(inquiry);
        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task<InquiryListViewModel> ListAsync(
        string staffId, bool isManager, string? search, InquiryStatus? status, int pageNumber,
        CancellationToken cancellationToken)
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

        var totalCount = await inquiries.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return new InquiryListViewModel(search, status, [], 1, 1, 0);
        }

        var totalPages = (totalCount - 1) / InquiryPageSize + 1;
        var currentPage = Math.Clamp(pageNumber, 1, totalPages);
        var rows = await inquiries
            .OrderByDescending(inquiry => inquiry.CreatedAt)
            .ThenByDescending(inquiry => inquiry.Id)
            .Skip((currentPage - 1) * InquiryPageSize)
            .Take(InquiryPageSize)
            .Select(inquiry => new InquiryListRow(
                inquiry.Id, inquiry.CustomerName, inquiry.Phone, inquiry.ServiceType, inquiry.CreatedAt, inquiry.Status,
                inquiry.Coordinator == null ? null : inquiry.Coordinator.DisplayName))
            .ToListAsync(cancellationToken);

        return new InquiryListViewModel(search, status, rows, currentPage, totalPages, totalCount);
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

        var assignments = await database.InquiryAssignments.AsNoTracking()
            .Where(entry => entry.InquiryId == id && (isManager || entry.Inquiry.CoordinatorId == staffId))
            .Select(entry => new InquiryHistoryRow(
                entry.Id, InquiryHistoryKind.Assignment, entry.Actor.DisplayName, entry.AssignedAt,
                entry.PreviousCoordinator == null ? null : entry.PreviousCoordinator.DisplayName,
                entry.NewCoordinator.DisplayName, null, null, null))
            .ToListAsync(cancellationToken);

        var followUps = await database.InquiryFollowUps.AsNoTracking()
            .Where(entry => entry.InquiryId == id && (isManager || entry.Inquiry.CoordinatorId == staffId))
            .Select(entry => new InquiryHistoryRow(
                entry.Id, entry.NewStatus == null ? InquiryHistoryKind.Note : InquiryHistoryKind.StatusChange,
                entry.Actor.DisplayName, entry.OccurredAt, null, null,
                entry.PreviousStatus, entry.NewStatus, entry.Text))
            .ToListAsync(cancellationToken);

        var history = assignments.Concat(followUps)
            .OrderByDescending(entry => entry.OccurredAt)
            .ThenByDescending(entry => entry.Kind)
            .ThenByDescending(entry => entry.Id)
            .ToArray();

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

    public Task<FollowUpResult> AddNoteAsync(int id, string coordinatorId, string? note, CancellationToken cancellationToken) =>
        SaveFollowUpAsync(id, coordinatorId, inquiry => inquiry.AddNote(coordinatorId, note, DateTimeOffset.UtcNow),
            cancellationToken);

    public Task<FollowUpResult> ChangeStatusAsync(
        int id, string coordinatorId, InquiryStatus? status, string? reason, CancellationToken cancellationToken) =>
        SaveFollowUpAsync(id, coordinatorId,
            inquiry => inquiry.ChangeStatus(coordinatorId, status, reason, DateTimeOffset.UtcNow), cancellationToken);

    private async Task<FollowUpResult> SaveFollowUpAsync(
        int id, string coordinatorId, Func<Inquiry, (FollowUpResult Result, InquiryFollowUp? Activity)> apply,
        CancellationToken cancellationToken)
    {
        var inquiry = await database.Inquiries.SingleOrDefaultAsync(
            item => item.Id == id && item.CoordinatorId == coordinatorId, cancellationToken);
        if (inquiry is null)
        {
            return FollowUpResult.InquiryNotFound;
        }

        var (result, activity) = apply(inquiry);
        if (activity is null)
        {
            return result;
        }

        if (activity.NewStatus is null)
        {
            // A note must still check the inquiry's original owner and status in the same save.
            database.Entry(inquiry).Property(item => item.Status).IsModified = true;
        }

        database.InquiryFollowUps.Add(activity);
        try
        {
            await database.SaveChangesAsync(cancellationToken);
            return FollowUpResult.Saved;
        }
        catch (DbUpdateConcurrencyException)
        {
            return FollowUpResult.Conflict;
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
