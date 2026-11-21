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
        client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        await using var scope = factory.Services.CreateAsyncScope();
        await ManagerSetup.RunAsync(scope.ServiceProvider, scope.ServiceProvider.GetRequiredService<IConfiguration>());
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

        using var signIn = await SignInAsync(ManagerEmail);
        Assert.AreEqual(HttpStatusCode.Redirect, signIn.StatusCode);
        using var listAfterSignIn = await client.GetAsync(signIn.Headers.Location!);
        Assert.AreEqual(HttpStatusCode.OK, listAfterSignIn.StatusCode);

        var token = ReadToken(await client.GetStringAsync("/Inquiries/Create"));
        using var invalid = await PostAsync("/Inquiries/Create",
            ("CustomerName", customer), ("ServiceType", "دهانات"), ("__RequestVerificationToken", token));
        Assert.AreEqual(HttpStatusCode.OK, invalid.StatusCode);
        StringAssert.Contains(WebUtility.HtmlDecode(await invalid.Content.ReadAsStringAsync()), "أدخل رقم الهاتف.");
        Assert.IsFalse(await database.Inquiries.AnyAsync(inquiry => inquiry.CustomerName == customer));

        var beforeCreation = DateTimeOffset.UtcNow;
        using var created = await PostAsync("/Inquiries/Create", ("CustomerName", customer), ("Phone", Phone),
            ("ServiceType", "دهانات"), ("Message", "طلب من البريد الإلكتروني"), ("__RequestVerificationToken", token));
        Assert.AreEqual(HttpStatusCode.Redirect, created.StatusCode);

        var phoneResults = WebUtility.HtmlDecode(await client.GetStringAsync("/Inquiries?search=12345678"));
        StringAssert.Contains(phoneResults, customer);
        StringAssert.Contains(phoneResults, earlierCustomer);
        Assert.IsFalse(phoneResults.Contains(otherCustomer, StringComparison.Ordinal));
        Assert.IsTrue(phoneResults.IndexOf(customer, StringComparison.Ordinal) <
                      phoneResults.IndexOf(earlierCustomer, StringComparison.Ordinal));
        StringAssert.Contains(phoneResults, "<td>جديد</td>");
        StringAssert.Contains(phoneResults, "<td>غير مسند</td>");
        StringAssert.Contains(phoneResults, "lang=\"ar\" dir=\"rtl\"");

        var nameSearch = $"/Inquiries?search={Uri.EscapeDataString("سارة")}";
        var newResults = WebUtility.HtmlDecode(await client.GetStringAsync($"{nameSearch}&status=0"));
        StringAssert.Contains(newResults, customer);
        Assert.IsFalse(newResults.Contains(earlierCustomer, StringComparison.Ordinal));
        var closedResults = WebUtility.HtmlDecode(await client.GetStringAsync($"{nameSearch}&status=3"));
        Assert.IsFalse(closedResults.Contains(customer, StringComparison.Ordinal));

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

        using var signIn = await SignInAsync(OtherStaffEmail);
        Assert.AreEqual(HttpStatusCode.Redirect, signIn.StatusCode);
        using var list = await client.GetAsync("/Inquiries");
        Assert.AreEqual(HttpStatusCode.Redirect, list.StatusCode);
        StringAssert.Contains(list.Headers.Location!.ToString(), "/Account/AccessDenied");
        using var post = await PostAsync("/Inquiries/Create", ("CustomerName", "محاولة"));
        Assert.AreEqual(HttpStatusCode.Redirect, post.StatusCode);
        StringAssert.Contains(post.Headers.Location!.ToString(), "/Account/AccessDenied");
        using var denied = await client.GetAsync(post.Headers.Location);
        Assert.AreEqual(HttpStatusCode.Forbidden, denied.StatusCode);
    }

    private async Task<HttpResponseMessage> SignInAsync(string email)
    {
        var token = ReadToken(await client.GetStringAsync("/Account/SignIn"));
        return await PostAsync("/Account/SignIn", ("Email", email), ("Password", TestPassword),
            ("__RequestVerificationToken", token));
    }

    private async Task<HttpResponseMessage> PostAsync(string path, params (string Name, string Value)[] fields)
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
