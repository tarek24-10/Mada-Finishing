using System.ComponentModel.DataAnnotations;

namespace MadaFinishing.Web.Models;

public sealed class InquiryFollowUp
{
    public const int NoteMaxLength = 2000;
    public const int ClosureReasonMaxLength = 500;

    private InquiryFollowUp()
    {
    }

    internal InquiryFollowUp(
        Inquiry inquiry, string actorId, DateTimeOffset occurredAt,
        string? text, InquiryStatus? previousStatus, InquiryStatus? newStatus)
    {
        Inquiry = inquiry;
        ActorId = actorId;
        OccurredAt = occurredAt;
        Text = text;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
    }

    public int Id { get; private set; }
    public int InquiryId { get; private set; }
    public Inquiry Inquiry { get; private set; } = null!;
    public string ActorId { get; private set; } = null!;
    public StaffUser Actor { get; private set; } = null!;
    public DateTimeOffset OccurredAt { get; private set; }
    public InquiryStatus? PreviousStatus { get; private set; }
    public InquiryStatus? NewStatus { get; private set; }

    [MaxLength(NoteMaxLength)]
    public string? Text { get; private set; }
}
