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
        string? search, InquiryStatus? status, CancellationToken cancellationToken)
    {
        var inquiries = database.Inquiries.AsNoTracking();
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
                inquiry.CustomerName, inquiry.Phone, inquiry.ServiceType, inquiry.CreatedAt, inquiry.Status,
                inquiry.Coordinator == null ? null : inquiry.Coordinator.DisplayName))
            .ToListAsync(cancellationToken);
    }
}
