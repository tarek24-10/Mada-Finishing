namespace MadaFinishing.Web.Models;

public sealed record InquiryListViewModel(
    string? Search, InquiryStatus? Status, IReadOnlyList<InquiryListRow> Inquiries);

public sealed record InquiryListRow(
    int Id, string CustomerName, string Phone, string ServiceType, DateTimeOffset CreatedAt,
    InquiryStatus Status, string? CoordinatorName);
