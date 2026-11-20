using MadaFinishing.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace MadaFinishing.Tests.Unit;

[TestClass]
public sealed class HomeControllerTests
{
    [TestMethod]
    public void Index_ReturnsView()
    {
        var controller = new HomeController();

        var result = controller.Index();

        Assert.IsInstanceOfType<ViewResult>(result);
    }
}
