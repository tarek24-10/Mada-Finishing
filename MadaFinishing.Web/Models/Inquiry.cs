using System.ComponentModel.DataAnnotations;

namespace MadaFinishing.Web.Models;

public sealed class Inquiry
{
    public const int CustomerNameMaxLength = 120;
    public const int PhoneMaxLength = 40;
    public const int ServiceTypeMaxLength = 120;
    public const int MessageMaxLength = 2000;

    private Inquiry()
    {
    }

    public int Id { get; private set; }

    [MaxLength(CustomerNameMaxLength)]
    public string CustomerName { get; private set; } = null!;

    [MaxLength(PhoneMaxLength)]
    public string Phone { get; private set; } = null!;

    [MaxLength(ServiceTypeMaxLength)]
    public string ServiceType { get; private set; } = null!;

    [MaxLength(MessageMaxLength)]
    public string? Message { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    [ConcurrencyCheck]
    public InquiryStatus Status { get; private set; }

    [ConcurrencyCheck]
    public string? CoordinatorId { get; private set; }

    public StaffUser? Coordinator { get; private set; }
    public bool CanBeAssigned => Status != InquiryStatus.Closed;

    public static InquiryStatus? NextStatusAfter(InquiryStatus status) => status switch
    {
        InquiryStatus.New => InquiryStatus.Contacted,
        InquiryStatus.Contacted => InquiryStatus.QuoteSent,
        _ => null
    };

    public static Inquiry Create(
        string customerName, string phone, string serviceType, string? message, DateTimeOffset createdAt)
    {
        return new Inquiry
        {
            CustomerName = customerName.Trim(),
            Phone = phone.Trim(),
            ServiceType = serviceType.Trim(),
            Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim(),
            CreatedAt = createdAt,
            Status = InquiryStatus.New
        };
    }

    public InquiryAssignment? AssignTo(string coordinatorId, string actorId, DateTimeOffset assignedAt)
    {
        if (!CanBeAssigned)
        {
            throw new InvalidOperationException("Closed inquiries cannot be reassigned.");
        }

        if (CoordinatorId == coordinatorId)
        {
            return null;
        }

        var assignment = new InquiryAssignment(this, actorId, CoordinatorId, coordinatorId, assignedAt);
        CoordinatorId = coordinatorId;
        return assignment;
    }

    public (FollowUpResult Result, InquiryFollowUp? Activity) AddNote(
        string actorId, string? note, DateTimeOffset occurredAt)
    {
        if (Status == InquiryStatus.Closed)
        {
            return (FollowUpResult.InquiryClosed, null);
        }

        note = note?.Trim();
        if (string.IsNullOrEmpty(note) || note.Length > InquiryFollowUp.NoteMaxLength)
        {
            return (FollowUpResult.InvalidNote, null);
        }

        return (FollowUpResult.Saved, new InquiryFollowUp(this, actorId, occurredAt, note, null, null));
    }

    public (FollowUpResult Result, InquiryFollowUp? Activity) ChangeStatus(
        string actorId, InquiryStatus? status, string? reason, DateTimeOffset occurredAt)
    {
        if (Status == InquiryStatus.Closed)
        {
            return (FollowUpResult.InquiryClosed, null);
        }

        if (status is null || (status != InquiryStatus.Closed && status != NextStatusAfter(Status)))
        {
            return (FollowUpResult.InvalidStatus, null);
        }

        reason = status == InquiryStatus.Closed ? reason?.Trim() : null;
        if (status == InquiryStatus.Closed &&
            (string.IsNullOrEmpty(reason) || reason.Length > InquiryFollowUp.ClosureReasonMaxLength))
        {
            return (FollowUpResult.InvalidClosureReason, null);
        }

        var activity = new InquiryFollowUp(this, actorId, occurredAt, reason, Status, status);
        Status = status.Value;
        return (FollowUpResult.Saved, activity);
    }
}
