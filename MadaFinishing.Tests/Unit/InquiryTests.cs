using MadaFinishing.Web.Models;

namespace MadaFinishing.Tests.Unit;

[TestClass]
public sealed class InquiryTests
{
    [TestMethod]
    [DataRow(InquiryStatus.New)]
    [DataRow(InquiryStatus.Contacted)]
    [DataRow(InquiryStatus.QuoteSent)]
    public void ChangeStatus_RequiresReasonToCloseFromAnyOpenStatus(InquiryStatus initialStatus)
    {
        var occurredAt = new DateTimeOffset(2026, 11, 21, 9, 0, 0, TimeSpan.Zero);
        var inquiry = Inquiry.Create("سارة", "01012345678", "دهانات", null, occurredAt);
        while (inquiry.Status < initialStatus && Inquiry.NextStatusAfter(inquiry.Status) is { } next)
        {
            inquiry.ChangeStatus("omar", next, null, occurredAt);
        }

        var rejected = inquiry.ChangeStatus("omar", InquiryStatus.Closed, "  ", occurredAt);
        Assert.AreEqual(FollowUpResult.InvalidClosureReason, rejected.Result);
        Assert.AreEqual(initialStatus, inquiry.Status);
        Assert.IsNull(rejected.Activity);

        var closed = inquiry.ChangeStatus("omar", InquiryStatus.Closed, "  العميل اعتذر  ", occurredAt);
        Assert.AreEqual(FollowUpResult.Saved, closed.Result);
        Assert.AreEqual(InquiryStatus.Closed, inquiry.Status);
        Assert.IsNotNull(closed.Activity);
        Assert.AreEqual(initialStatus, closed.Activity.PreviousStatus);
        Assert.AreEqual("العميل اعتذر", closed.Activity.Text);
        Assert.AreEqual(FollowUpResult.InquiryClosed, inquiry.AddNote("omar", "ملاحظة", occurredAt).Result);
        Assert.AreEqual(FollowUpResult.InquiryClosed,
            inquiry.ChangeStatus("omar", InquiryStatus.Contacted, null, occurredAt).Result);
    }

    [TestMethod]
    public void AddNote_RejectsBlankAndOverlongTextAndKeepsStatus()
    {
        var occurredAt = DateTimeOffset.UnixEpoch;
        var inquiry = Inquiry.Create("سارة", "01012345678", "دهانات", null, occurredAt);

        var blank = inquiry.AddNote("omar", "   ", occurredAt);
        var overlong = inquiry.AddNote("omar", new string('a', InquiryFollowUp.NoteMaxLength + 1), occurredAt);
        var recorded = inquiry.AddNote("omar", "  تم الاتصال  ", occurredAt);

        Assert.AreEqual(FollowUpResult.InvalidNote, blank.Result);
        Assert.AreEqual(FollowUpResult.InvalidNote, overlong.Result);
        Assert.AreEqual(FollowUpResult.Saved, recorded.Result);
        Assert.IsNotNull(recorded.Activity);
        Assert.AreEqual("تم الاتصال", recorded.Activity.Text);
        Assert.AreEqual("omar", recorded.Activity.ActorId);
        Assert.AreEqual(occurredAt, recorded.Activity.OccurredAt);
        Assert.IsNull(recorded.Activity.NewStatus);
        Assert.AreEqual(InquiryStatus.New, inquiry.Status);
    }

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
