using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MadaFinishing.Web.Models;

public sealed class StaffUser : IdentityUser
{
    [MaxLength(120)]
    public string DisplayName { get; set; } = "";

    public const string ManagerRole = "Manager";
    public const string CoordinatorRole = "Coordinator";
}
