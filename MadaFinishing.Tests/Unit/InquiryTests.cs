using MadaFinishing.Web.Models;

namespace MadaFinishing.Tests.Unit;

[TestClass]
public sealed class InquiryTests
{
    [TestMethod]
    public void Create_SetsNewUnassignedAndRecordsCreationTime()
    {
        var createdAt = new DateTimeOffset(2026, 11, 20, 9, 0, 0, TimeSpan.Zero);

        var inquiry = Inquiry.Create(" سارة ", " 01012345678 ", " دهانات ", " ", createdAt);

        Assert.AreEqual("سارة", inquiry.CustomerName);
        Assert.AreEqual("01012345678", inquiry.Phone);
        Assert.AreEqual("دهانات", inquiry.ServiceType);
        Assert.IsNull(inquiry.Message);
        Assert.AreEqual(createdAt, inquiry.CreatedAt);
        Assert.AreEqual(InquiryStatus.New, inquiry.Status);
        Assert.IsNull(inquiry.CoordinatorId);
    }
}
