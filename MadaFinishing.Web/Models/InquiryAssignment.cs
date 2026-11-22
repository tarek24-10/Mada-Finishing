namespace MadaFinishing.Web.Models;

public sealed class InquiryAssignment
{
    private InquiryAssignment()
    {
    }

    internal InquiryAssignment(
        Inquiry inquiry, string actorId, string? previousCoordinatorId, string newCoordinatorId, DateTimeOffset assignedAt)
    {
        Inquiry = inquiry;
        ActorId = actorId;
        PreviousCoordinatorId = previousCoordinatorId;
        NewCoordinatorId = newCoordinatorId;
        AssignedAt = assignedAt;
    }

    public int Id { get; private set; }
    public int InquiryId { get; private set; }
    public Inquiry Inquiry { get; private set; } = null!;
    public string ActorId { get; private set; } = null!;
    public StaffUser Actor { get; private set; } = null!;
    public string? PreviousCoordinatorId { get; private set; }
    public StaffUser? PreviousCoordinator { get; private set; }
    public string NewCoordinatorId { get; private set; } = null!;
    public StaffUser NewCoordinator { get; private set; } = null!;
    public DateTimeOffset AssignedAt { get; private set; }
}
