namespace MadaFinishing.Web.Models;

public sealed record InquiryDetailsViewModel(
    int Id, string CustomerName, string Phone, string ServiceType, string? Message, DateTimeOffset CreatedAt,
    InquiryStatus Status, string? CoordinatorId, string? CoordinatorName)
{
    public IReadOnlyList<InquiryHistoryRow> History { get; init; } = [];
    public IReadOnlyList<CoordinatorOption> Coordinators { get; init; } = [];
    public string? Note { get; init; }
    public string? ClosureReason { get; init; }
}

public enum InquiryHistoryKind
{
    Assignment,
    Note,
    StatusChange
}

public sealed record InquiryHistoryRow(
    int Id, InquiryHistoryKind Kind, string ActorName, DateTimeOffset OccurredAt,
    string? PreviousCoordinatorName, string? NewCoordinatorName,
    InquiryStatus? PreviousStatus, InquiryStatus? NewStatus, string? Text);

public sealed record CoordinatorOption(string Id, string DisplayName);
