using MadaFinishing.Web.Models;

namespace MadaFinishing.Tests.Unit;

[TestClass]
public sealed class InquiryTests
{
    [TestMethod]
    public void AssignTo_RecordsPreviousAndNewCoordinatorWithoutChangingStatus()
    {
        var assignedAt = new DateTimeOffset(2026, 11, 21, 9, 0, 0, TimeSpan.Zero);
        var inquiry = Inquiry.Create("سارة", "01012345678", "دهانات", null, assignedAt);
        var first = inquiry.AssignTo("mariam", "manager", assignedAt);

        var reassignment = inquiry.AssignTo("omar", "manager", assignedAt.AddMinutes(1));
        var repeated = inquiry.AssignTo("omar", "manager", assignedAt.AddMinutes(2));

        Assert.IsNotNull(first);
        Assert.IsNull(first.PreviousCoordinatorId);
        Assert.IsNotNull(reassignment);
        Assert.AreEqual("mariam", reassignment.PreviousCoordinatorId);
        Assert.AreEqual("omar", reassignment.NewCoordinatorId);
        Assert.AreEqual("manager", reassignment.ActorId);
        Assert.AreEqual(assignedAt.AddMinutes(1), reassignment.AssignedAt);
        Assert.AreEqual("omar", inquiry.CoordinatorId);
        Assert.AreEqual(InquiryStatus.New, inquiry.Status);
        Assert.IsNull(repeated);
    }

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
