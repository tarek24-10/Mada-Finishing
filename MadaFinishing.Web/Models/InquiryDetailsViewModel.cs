namespace MadaFinishing.Web.Models;

public sealed record InquiryDetailsViewModel(
    int Id, string CustomerName, string Phone, string ServiceType, string? Message, DateTimeOffset CreatedAt,
    InquiryStatus Status, string? CoordinatorId, string? CoordinatorName)
{
    public IReadOnlyList<InquiryAssignmentRow> History { get; init; } = [];
    public IReadOnlyList<CoordinatorOption> Coordinators { get; init; } = [];
}

public sealed record InquiryAssignmentRow(
    string ActorName, string? PreviousCoordinatorName, string NewCoordinatorName, DateTimeOffset AssignedAt);

public sealed record CoordinatorOption(string Id, string DisplayName);
