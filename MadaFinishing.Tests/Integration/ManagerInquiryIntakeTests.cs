using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using MadaFinishing.Web.Data;
using MadaFinishing.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MadaFinishing.Tests.Integration;

[TestClass]
public sealed class ManagerInquiryIntakeTests
{
    private const string ManagerEmail = "manager@example.test";
    private const string OtherStaffEmail = "staff@example.test";
    private const string TestPassword = "LocalDemo123!";
    private const string Phone = "01012345678";
    private WebApplicationFactory<Program>? factory;
    private HttpClient client = null!;

    [TestInitialize]
    public async Task SetUpAsync()
    {
        var server = Environment.GetEnvironmentVariable("MADA_TEST_SQL_SERVER");
        if (string.IsNullOrWhiteSpace(server))
        {
            Assert.Inconclusive("Set MADA_TEST_SQL_SERVER to a SQL Server connection string for the isolated integration tests.");
        }

        var connection = new SqlConnectionStringBuilder(server)
        {
            InitialCatalog = $"MadaFinishingTest_{Guid.NewGuid():N}",
            AttachDBFilename = ""
        };
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(host => host
            .ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:MadaFinishing"] = connection.ConnectionString,
                    ["InitialManager:Email"] = ManagerEmail,
                    ["InitialManager:Password"] = TestPassword,
                    ["InitialManager:DisplayName"] = "نور"
                })));
        client = CreateStaffClient();
        await using var scope = factory.Services.CreateAsyncScope();
        await StaffSetup.RunAsync(scope.ServiceProvider, scope.ServiceProvider.GetRequiredService<IConfiguration>(),
            StaffUser.ManagerRole);
    }

    [TestCleanup]
    public async Task CleanUpAsync()
    {
        if (factory is null)
        {
            return;
        }

        try
        {
            await using var scope = factory.Services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<MadaFinishingDbContext>().Database.EnsureDeletedAsync();
        }
        finally
        {
            client?.Dispose();
            await factory.DisposeAsync();
        }
    }

    [TestMethod]
    public async Task ManagerCanEnterAndFindNewUnassignedInquiry()
    {
        const string customer = "سارة إبراهيم";
        const string earlierCustomer = "عميل سابق";
        const string otherCustomer = "عميل آخر";
        await using var scope = factory!.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<MadaFinishingDbContext>();
        database.Inquiries.AddRange(
            Inquiry.Create(earlierCustomer, Phone, "تشطيب", null, DateTimeOffset.UtcNow.AddDays(-1)),
            Inquiry.Create(otherCustomer, "01134567890", "تشطيب", null, DateTimeOffset.UtcNow.AddDays(-2)));
        await database.SaveChangesAsync();

        using var signIn = await SignInAsync(client, ManagerEmail);
        Assert.AreEqual(HttpStatusCode.Redirect, signIn.StatusCode);
        using var listAfterSignIn = await client.GetAsync(signIn.Headers.Location!);
        Assert.AreEqual(HttpStatusCode.OK, listAfterSignIn.StatusCode);

        var token = ReadToken(await client.GetStringAsync("/Inquiries/Create"));
        using var invalid = await PostAsync(client, "/Inquiries/Create",
            ("CustomerName", customer), ("ServiceType", "دهانات"), ("__RequestVerificationToken", token));
        Assert.AreEqual(HttpStatusCode.OK, invalid.StatusCode);
        StringAssert.Contains(WebUtility.HtmlDecode(await invalid.Content.ReadAsStringAsync()), "أدخل رقم الهاتف.");
        Assert.IsFalse(await database.Inquiries.AnyAsync(inquiry => inquiry.CustomerName == customer));

        var beforeCreation = DateTimeOffset.UtcNow;
        using var created = await PostAsync(client, "/Inquiries/Create", ("CustomerName", customer), ("Phone", Phone),
            ("ServiceType", "دهانات"), ("Message", "طلب من البريد الإلكتروني"), ("__RequestVerificationToken", token));
        Assert.AreEqual(HttpStatusCode.Redirect, created.StatusCode);

        var phoneResults = WebUtility.HtmlDecode(await client.GetStringAsync("/Inquiries?search=12345678"));
        StringAssert.Contains(phoneResults, customer);
        StringAssert.Contains(phoneResults, earlierCustomer);
        Assert.IsFalse(phoneResults.Contains(otherCustomer, StringComparison.Ordinal));
        Assert.IsTrue(phoneResults.IndexOf(customer, StringComparison.Ordinal) <
                      phoneResults.IndexOf(earlierCustomer, StringComparison.Ordinal));
        StringAssert.Matches(phoneResults, new Regex(@"<td>\s*<span[^>]*>جديد</span>\s*</td>"));
        StringAssert.Contains(phoneResults, "<td>غير مسند</td>");
        StringAssert.Contains(phoneResults, "lang=\"ar\" dir=\"rtl\"");

        var nameSearch = $"/Inquiries?search={Uri.EscapeDataString("سارة")}";
        var newResults = WebUtility.HtmlDecode(await client.GetStringAsync($"{nameSearch}&status=0"));
        StringAssert.Contains(newResults, customer);
        Assert.IsFalse(newResults.Contains(earlierCustomer, StringComparison.Ordinal));
        var closedResults = WebUtility.HtmlDecode(await client.GetStringAsync($"{nameSearch}&status=3"));
        Assert.IsFalse(closedResults.Contains(customer, StringComparison.Ordinal));

        using var ajaxSearch = new HttpRequestMessage(HttpMethod.Get, $"{nameSearch}&status=0");
        ajaxSearch.Headers.Add("X-Requested-With", "XMLHttpRequest");
        using var ajaxResponse = await client.SendAsync(ajaxSearch);
        Assert.AreEqual(HttpStatusCode.OK, ajaxResponse.StatusCode);
        var ajaxResults = WebUtility.HtmlDecode(await ajaxResponse.Content.ReadAsStringAsync());
        StringAssert.Contains(ajaxResults, "id=\"inquiry-results\"");
        StringAssert.Contains(ajaxResults, customer);
        Assert.IsFalse(ajaxResults.Contains(earlierCustomer, StringComparison.Ordinal));
        Assert.IsFalse(ajaxResults.Contains("<html", StringComparison.OrdinalIgnoreCase));

        var saved = await database.Inquiries.SingleAsync(inquiry => inquiry.CustomerName == customer);
        Assert.AreEqual(Phone, saved.Phone);
        Assert.AreEqual("دهانات", saved.ServiceType);
        Assert.AreEqual("طلب من البريد الإلكتروني", saved.Message);
        Assert.IsTrue(saved.CreatedAt >= beforeCreation && saved.CreatedAt <= DateTimeOffset.UtcNow);
        Assert.AreEqual(InquiryStatus.New, saved.Status);
        Assert.IsNull(saved.CoordinatorId);
    }

    [TestMethod]
    public async Task IntakeRequiresManagerRole()
    {
        await using var scope = factory!.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<StaffUser>>();
        var newStaff = await users.CreateAsync(
            new StaffUser { UserName = OtherStaffEmail, Email = OtherStaffEmail, DisplayName = "موظف" }, TestPassword);
        Assert.IsTrue(newStaff.Succeeded);

        using var anonymous = await client.GetAsync("/Inquiries/Create");
        Assert.AreEqual(HttpStatusCode.Redirect, anonymous.StatusCode);
        StringAssert.Contains(anonymous.Headers.Location!.ToString(), "/Account/SignIn");

        using var signIn = await SignInAsync(client, OtherStaffEmail);
        Assert.AreEqual(HttpStatusCode.Redirect, signIn.StatusCode);
        using var list = await client.GetAsync("/Inquiries");
        Assert.AreEqual(HttpStatusCode.Redirect, list.StatusCode);
        StringAssert.Contains(list.Headers.Location!.ToString(), "/Account/AccessDenied");
        using var post = await PostAsync(client, "/Inquiries/Create", ("CustomerName", "محاولة"));
        Assert.AreEqual(HttpStatusCode.Redirect, post.StatusCode);
        StringAssert.Contains(post.Headers.Location!.ToString(), "/Account/AccessDenied");
        using var denied = await client.GetAsync(post.Headers.Location);
        Assert.AreEqual(HttpStatusCode.Forbidden, denied.StatusCode);
    }

    [TestMethod]
    public async Task ManagerCanReassignInquiryAndFormerCoordinatorLosesAccess()
    {
        const string mariamEmail = "mariam@example.test";
        const string omarEmail = "omar@example.test";
        const string customer = "عميل يحتاج إلى إسناد";
        await using var scope = factory!.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<MadaFinishingDbContext>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<StaffUser>>();
        Assert.IsTrue((await roles.CreateAsync(new IdentityRole(StaffUser.CoordinatorRole))).Succeeded);
        var mariam = await CreateCoordinatorAsync(users, mariamEmail, "مريم");
        var omar = await CreateCoordinatorAsync(users, omarEmail, "عمر");
        var manager = (await users.FindByEmailAsync(ManagerEmail))!;
        var inquiry = Inquiry.Create(customer, Phone, "تشطيب", "رسالة العميل", DateTimeOffset.UtcNow);
        database.Inquiries.Add(inquiry);
        await database.SaveChangesAsync();

        using var mariamClient = CreateStaffClient();
        using var omarClient = CreateStaffClient();
        using var managerSignIn = await SignInAsync(client, ManagerEmail);
        using var mariamSignIn = await SignInAsync(mariamClient, mariamEmail);
        using var omarSignIn = await SignInAsync(omarClient, omarEmail);
        Assert.AreEqual(HttpStatusCode.Redirect, managerSignIn.StatusCode);
        Assert.AreEqual(HttpStatusCode.Redirect, mariamSignIn.StatusCode);
        Assert.AreEqual(HttpStatusCode.Redirect, omarSignIn.StatusCode);

        var detailsPath = $"/Inquiries/Details/{inquiry.Id}";
        var assignmentPath = $"/Inquiries/Assign/{inquiry.Id}";
        using var unassignedDetails = await mariamClient.GetAsync(detailsPath);
        Assert.AreEqual(HttpStatusCode.NotFound, unassignedDetails.StatusCode);
        var managerForm = await client.GetStringAsync(detailsPath);
        var options = WebUtility.HtmlDecode(managerForm);
        StringAssert.Contains(options, $">{mariam.DisplayName}</option>");
        StringAssert.Contains(options, $">{omar.DisplayName}</option>");
        Assert.IsFalse(options.Contains($"value=\"{manager.Id}\"", StringComparison.Ordinal));
        var token = ReadToken(managerForm);

        using var invalid = await PostAsync(client, assignmentPath, ("coordinatorId", manager.Id),
            ("__RequestVerificationToken", token));
        Assert.AreEqual(HttpStatusCode.OK, invalid.StatusCode);
        StringAssert.Contains(WebUtility.HtmlDecode(await invalid.Content.ReadAsStringAsync()), "اختر منسق مبيعات صالحًا.");
        Assert.IsFalse(await database.InquiryAssignments.AnyAsync());

        using var first = await PostAsync(client, assignmentPath, ("coordinatorId", mariam.Id),
            ("__RequestVerificationToken", token));
        Assert.AreEqual(HttpStatusCode.Redirect, first.StatusCode);
        var mariamDetailsPage = await mariamClient.GetStringAsync(detailsPath);
        StringAssert.Contains(WebUtility.HtmlDecode(mariamDetailsPage), customer);
        Assert.IsFalse(mariamDetailsPage.Contains("name=\"coordinatorId\"", StringComparison.Ordinal));
        StringAssert.Contains(WebUtility.HtmlDecode(await mariamClient.GetStringAsync("/Inquiries?search=12345678")), customer);
        using var omarBeforeReassignment = await omarClient.GetAsync(detailsPath);
        Assert.AreEqual(HttpStatusCode.NotFound, omarBeforeReassignment.StatusCode);

        using var reassigned = await PostAsync(client, assignmentPath, ("coordinatorId", omar.Id),
            ("__RequestVerificationToken", token));
        Assert.AreEqual(HttpStatusCode.Redirect, reassigned.StatusCode);

        using var savedLink = await mariamClient.GetAsync(detailsPath);
        Assert.AreEqual(HttpStatusCode.NotFound, savedLink.StatusCode);
        var formerCoordinatorList = WebUtility.HtmlDecode(await mariamClient.GetStringAsync("/Inquiries?search=12345678"));
        Assert.IsFalse(formerCoordinatorList.Contains(customer, StringComparison.Ordinal));
        using var ajaxSearch = new HttpRequestMessage(HttpMethod.Get, "/Inquiries?search=12345678");
        ajaxSearch.Headers.Add("X-Requested-With", "XMLHttpRequest");
        using var formerCoordinatorResults = await mariamClient.SendAsync(ajaxSearch);
        Assert.AreEqual(HttpStatusCode.OK, formerCoordinatorResults.StatusCode);
        var formerCoordinatorFragment = WebUtility.HtmlDecode(await formerCoordinatorResults.Content.ReadAsStringAsync());
        StringAssert.Contains(formerCoordinatorFragment, "لا توجد استفسارات مطابقة.");
        Assert.IsFalse(formerCoordinatorFragment.Contains(customer, StringComparison.Ordinal));
        var currentCoordinatorList = WebUtility.HtmlDecode(await omarClient.GetStringAsync("/Inquiries?search=12345678"));
        StringAssert.Contains(currentCoordinatorList, customer);
        StringAssert.Contains(WebUtility.HtmlDecode(await omarClient.GetStringAsync(detailsPath)), customer);
        using var formerCoordinatorPost = await PostAsync(mariamClient, assignmentPath, ("coordinatorId", mariam.Id),
            ("__RequestVerificationToken", ReadToken(mariamDetailsPage)));
        Assert.AreEqual(HttpStatusCode.Redirect, formerCoordinatorPost.StatusCode);
        StringAssert.Contains(formerCoordinatorPost.Headers.Location!.ToString(), "/Account/AccessDenied");

        using var unchanged = await PostAsync(client, assignmentPath, ("coordinatorId", omar.Id),
            ("__RequestVerificationToken", token));
        Assert.AreEqual(HttpStatusCode.Redirect, unchanged.StatusCode);
        var managerDetails = WebUtility.HtmlDecode(await client.GetStringAsync(detailsPath));
        StringAssert.Contains(managerDetails, "نور — من غير مسند إلى مريم");
        StringAssert.Contains(managerDetails, "نور — من مريم إلى عمر");

        var saved = await database.Inquiries.AsNoTracking().SingleAsync(item => item.Id == inquiry.Id);
        Assert.AreEqual(omar.Id, saved.CoordinatorId);
        Assert.AreEqual(InquiryStatus.New, saved.Status);
        var history = await database.InquiryAssignments.AsNoTracking()
            .Where(entry => entry.InquiryId == inquiry.Id).OrderBy(entry => entry.Id).ToListAsync();
        Assert.HasCount(2, history);
        Assert.AreEqual(manager.Id, history[0].ActorId);
        Assert.IsNull(history[0].PreviousCoordinatorId);
        Assert.AreEqual(mariam.Id, history[0].NewCoordinatorId);
        Assert.AreEqual(manager.Id, history[1].ActorId);
        Assert.AreEqual(mariam.Id, history[1].PreviousCoordinatorId);
        Assert.AreEqual(omar.Id, history[1].NewCoordinatorId);
        Assert.IsTrue(history[1].AssignedAt >= history[0].AssignedAt);

        await database.Inquiries.Where(item => item.Id == inquiry.Id)
            .ExecuteUpdateAsync(update => update.SetProperty(item => item.Status, InquiryStatus.Closed));
        using var closed = await PostAsync(client, assignmentPath, ("coordinatorId", mariam.Id),
            ("__RequestVerificationToken", token));
        Assert.AreEqual(HttpStatusCode.OK, closed.StatusCode);
        StringAssert.Contains(WebUtility.HtmlDecode(await closed.Content.ReadAsStringAsync()), "لا يمكن تغيير مسؤول استفسار مغلق.");
        Assert.AreEqual(2, await database.InquiryAssignments.CountAsync());
        Assert.AreEqual(omar.Id, await database.Inquiries.Where(item => item.Id == inquiry.Id)
            .Select(item => item.CoordinatorId).SingleAsync());
    }

    [TestMethod]
    public async Task AssignedCoordinatorRecordsFollowUpAndClosedInquiryStaysReadOnly()
    {
        const string mariamEmail = "mariam@example.test";
        const string omarEmail = "omar@example.test";
        const string note = "تم الاتصال وشرح خيارات التشطيب";
        const string reason = "اختار العميل تأجيل العمل";
        await using var scope = factory!.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<MadaFinishingDbContext>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<StaffUser>>();
        Assert.IsTrue((await roles.CreateAsync(new IdentityRole(StaffUser.CoordinatorRole))).Succeeded);
        var mariam = await CreateCoordinatorAsync(users, mariamEmail, "مريم");
        var omar = await CreateCoordinatorAsync(users, omarEmail, "عمر");
        var manager = (await users.FindByEmailAsync(ManagerEmail))!;
        var createdAt = DateTimeOffset.UtcNow;
        var inquiry = Inquiry.Create("عميل المتابعة", Phone, "تشطيب", null, createdAt);
        database.Inquiries.Add(inquiry);
        database.InquiryAssignments.Add(inquiry.AssignTo(mariam.Id, manager.Id, createdAt)!);
        await database.SaveChangesAsync();

        using var mariamClient = CreateStaffClient();
        using var omarClient = CreateStaffClient();
        using var managerSignIn = await SignInAsync(client, ManagerEmail);
        using var mariamSignIn = await SignInAsync(mariamClient, mariamEmail);
        using var omarSignIn = await SignInAsync(omarClient, omarEmail);
        var detailsPath = $"/Inquiries/Details/{inquiry.Id}";
        var notePath = $"/Inquiries/AddNote/{inquiry.Id}";
        var managerPage = await client.GetStringAsync(detailsPath);
        var managerToken = ReadToken(managerPage);
        var formerOwnerToken = ReadToken(await mariamClient.GetStringAsync(detailsPath));

        using var reassigned = await PostAsync(client, $"/Inquiries/Assign/{inquiry.Id}",
            ("coordinatorId", omar.Id), ("__RequestVerificationToken", managerToken));
        Assert.AreEqual(HttpStatusCode.Redirect, reassigned.StatusCode);
        var coordinatorPage = await omarClient.GetStringAsync(detailsPath);
        var token = ReadToken(coordinatorPage);

        using var managerWrite = await PostAsync(client, notePath, ("note", note),
            ("__RequestVerificationToken", managerToken));
        Assert.AreEqual(HttpStatusCode.Redirect, managerWrite.StatusCode);
        StringAssert.Contains(managerWrite.Headers.Location!.ToString(), "/Account/AccessDenied");
        using var formerOwnerWrite = await PostAsync(mariamClient, notePath, ("note", note),
            ("__RequestVerificationToken", formerOwnerToken));
        Assert.AreEqual(HttpStatusCode.NotFound, formerOwnerWrite.StatusCode);
        using var formerOwnerStatus = await PostStatusAsync(mariamClient, inquiry.Id, InquiryStatus.Contacted,
            formerOwnerToken);
        Assert.AreEqual(HttpStatusCode.NotFound, formerOwnerStatus.StatusCode);

        using var skipped = await PostStatusAsync(omarClient, inquiry.Id, InquiryStatus.QuoteSent, token);
        Assert.AreEqual(HttpStatusCode.OK, skipped.StatusCode);
        StringAssert.Contains(WebUtility.HtmlDecode(await skipped.Content.ReadAsStringAsync()), "اختر الحالة التالية المتاحة.");
        using var blankNote = await PostAsync(omarClient, notePath, ("note", ""),
            ("__RequestVerificationToken", token));
        Assert.AreEqual(HttpStatusCode.OK, blankNote.StatusCode);
        Assert.IsFalse(await database.InquiryFollowUps.AnyAsync());

        using var contacted = await PostStatusAsync(omarClient, inquiry.Id, InquiryStatus.Contacted, token);
        Assert.AreEqual(HttpStatusCode.Redirect, contacted.StatusCode);
        using var recorded = await PostAsync(omarClient, notePath, ("note", $"  {note}  "),
            ("__RequestVerificationToken", token));
        Assert.AreEqual(HttpStatusCode.Redirect, recorded.StatusCode);
        using var quoted = await PostStatusAsync(omarClient, inquiry.Id, InquiryStatus.QuoteSent, token);
        Assert.AreEqual(HttpStatusCode.Redirect, quoted.StatusCode);

        using var noReason = await PostStatusAsync(omarClient, inquiry.Id, InquiryStatus.Closed, token);
        Assert.AreEqual(HttpStatusCode.OK, noReason.StatusCode);
        StringAssert.Contains(WebUtility.HtmlDecode(await noReason.Content.ReadAsStringAsync()), "أدخل سبب إغلاق");
        Assert.AreEqual(InquiryStatus.QuoteSent, await database.Inquiries.Where(item => item.Id == inquiry.Id)
            .Select(item => item.Status).SingleAsync());
        Assert.AreEqual(3, await database.InquiryFollowUps.CountAsync());

        using var closed = await PostStatusAsync(omarClient, inquiry.Id, InquiryStatus.Closed, token, reason);
        Assert.AreEqual(HttpStatusCode.Redirect, closed.StatusCode);
        using var closedNote = await PostAsync(omarClient, notePath, ("note", note),
            ("__RequestVerificationToken", token));
        Assert.AreEqual(HttpStatusCode.OK, closedNote.StatusCode);
        StringAssert.Contains(WebUtility.HtmlDecode(await closedNote.Content.ReadAsStringAsync()), "الاستفسار مغلق ولا يمكن تعديله.");
        using var reopened = await PostStatusAsync(omarClient, inquiry.Id, InquiryStatus.Contacted, token);
        Assert.AreEqual(HttpStatusCode.OK, reopened.StatusCode);

        var managerDetails = WebUtility.HtmlDecode(await client.GetStringAsync(detailsPath));
        var coordinatorDetails = WebUtility.HtmlDecode(await omarClient.GetStringAsync(detailsPath));
        StringAssert.Contains(managerDetails, "نور — من مريم إلى عمر");
        StringAssert.Contains(managerDetails, "عمر — من جديد إلى تم التواصل");
        StringAssert.Contains(managerDetails, "عمر — من تم التواصل إلى تم إرسال عرض السعر");
        StringAssert.Contains(managerDetails, note);
        StringAssert.Contains(managerDetails, $"سبب الإغلاق: {reason}");
        StringAssert.Contains(coordinatorDetails, reason);
        Assert.IsFalse(coordinatorDetails.Contains("name=\"note\"", StringComparison.Ordinal));
        Assert.IsFalse(managerDetails.Contains("name=\"coordinatorId\"", StringComparison.Ordinal));

        var saved = await database.Inquiries.AsNoTracking().SingleAsync(item => item.Id == inquiry.Id);
        Assert.AreEqual(InquiryStatus.Closed, saved.Status);
        Assert.AreEqual(omar.Id, saved.CoordinatorId);
        var history = await database.InquiryFollowUps.AsNoTracking().OrderBy(entry => entry.Id).ToListAsync();
        Assert.HasCount(4, history);
        Assert.IsTrue(history.All(entry => entry.ActorId == omar.Id && entry.OccurredAt >= createdAt));
        Assert.AreEqual(InquiryStatus.Contacted, history[0].NewStatus);
        Assert.AreEqual(note, history[1].Text);
        Assert.IsNull(history[1].NewStatus);
        Assert.AreEqual(InquiryStatus.QuoteSent, history[2].NewStatus);
        Assert.AreEqual(InquiryStatus.QuoteSent, history[3].PreviousStatus);
        Assert.AreEqual(InquiryStatus.Closed, history[3].NewStatus);
        Assert.AreEqual(reason, history[3].Text);
    }

    [TestMethod]
    public async Task ConcurrentAssignmentDoesNotWriteStaleHistory()
    {
        await using var scope = factory!.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<MadaFinishingDbContext>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<StaffUser>>();
        Assert.IsTrue((await roles.CreateAsync(new IdentityRole(StaffUser.CoordinatorRole))).Succeeded);
        var mariam = await CreateCoordinatorAsync(users, "mariam@example.test", "مريم");
        var omar = await CreateCoordinatorAsync(users, "omar@example.test", "عمر");
        var manager = (await users.FindByEmailAsync(ManagerEmail))!;
        var inquiry = Inquiry.Create("عميل", Phone, "تشطيب", null, DateTimeOffset.UtcNow);
        database.Inquiries.Add(inquiry);
        await database.SaveChangesAsync();

        await using var firstScope = factory.Services.CreateAsyncScope();
        await using var secondScope = factory.Services.CreateAsyncScope();
        var firstDatabase = firstScope.ServiceProvider.GetRequiredService<MadaFinishingDbContext>();
        var secondDatabase = secondScope.ServiceProvider.GetRequiredService<MadaFinishingDbContext>();
        var firstCopy = await firstDatabase.Inquiries.SingleAsync(item => item.Id == inquiry.Id);
        var staleCopy = await secondDatabase.Inquiries.SingleAsync(item => item.Id == inquiry.Id);

        firstDatabase.InquiryAssignments.Add(firstCopy.AssignTo(mariam.Id, manager.Id, DateTimeOffset.UtcNow)!);
        secondDatabase.InquiryAssignments.Add(staleCopy.AssignTo(omar.Id, manager.Id, DateTimeOffset.UtcNow)!);
        await firstDatabase.SaveChangesAsync();

        await Assert.ThrowsExactlyAsync<DbUpdateConcurrencyException>(() => secondDatabase.SaveChangesAsync());
        var savedCoordinator = await database.Inquiries.Where(item => item.Id == inquiry.Id)
            .Select(item => item.CoordinatorId).SingleAsync();
        Assert.AreEqual(mariam.Id, savedCoordinator);
        var savedAssignment = await database.InquiryAssignments.SingleAsync(entry => entry.InquiryId == inquiry.Id);
        Assert.AreEqual(mariam.Id, savedAssignment.NewCoordinatorId);
    }

    private HttpClient CreateStaffClient() => factory!.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false
    });

    private static async Task<StaffUser> CreateCoordinatorAsync(UserManager<StaffUser> users, string email, string name)
    {
        var user = new StaffUser { UserName = email, Email = email, DisplayName = name };
        Assert.IsTrue((await users.CreateAsync(user, TestPassword)).Succeeded);
        Assert.IsTrue((await users.AddToRoleAsync(user, StaffUser.CoordinatorRole)).Succeeded);
        return user;
    }

    private static async Task<HttpResponseMessage> SignInAsync(HttpClient client, string email)
    {
        var token = ReadToken(await client.GetStringAsync("/Account/SignIn"));
        return await PostAsync(client, "/Account/SignIn", ("Email", email), ("Password", TestPassword),
            ("__RequestVerificationToken", token));
    }

    private static Task<HttpResponseMessage> PostStatusAsync(
        HttpClient client, int id, InquiryStatus status, string token, string? reason = null) =>
        PostAsync(client, $"/Inquiries/ChangeStatus/{id}", ("status", ((int)status).ToString(CultureInfo.InvariantCulture)),
            ("reason", reason ?? ""), ("__RequestVerificationToken", token));

    private static async Task<HttpResponseMessage> PostAsync(
        HttpClient client, string path, params (string Name, string Value)[] fields)
    {
        using var content = new FormUrlEncodedContent(fields.Select(field =>
            new KeyValuePair<string, string>(field.Name, field.Value)));
        return await client.PostAsync(path, content);
    }

    private static string ReadToken(string html)
    {
        // The authenticated layout also contains a sign-out form. Antiforgery tokens are valid
        // for either form because they belong to the same user and request cookie.
        var input = Regex.Match(html, """<input\b[^>]*\bname="__RequestVerificationToken"[^>]*>""");
        var value = Regex.Match(input.Value, """\bvalue="([^"]+)""");
        Assert.IsTrue(value.Success, "Expected an antiforgery token in the form.");
        return WebUtility.HtmlDecode(value.Groups[1].Value);
    }
}
