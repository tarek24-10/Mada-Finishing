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
}
