using System.ComponentModel.DataAnnotations;

namespace MadaFinishing.Web.Models;

public sealed class SignInInput
{
    [Display(Name = "البريد الإلكتروني")]
    [Required(ErrorMessage = "أدخل البريد الإلكتروني.")]
    [EmailAddress(ErrorMessage = "أدخل بريدًا إلكترونيًا صالحًا.")]
    [StringLength(256, ErrorMessage = "يجب ألا يتجاوز البريد الإلكتروني {1} حرفًا.")]
    public string Email { get; set; } = "";

    [Display(Name = "كلمة المرور")]
    [Required(ErrorMessage = "أدخل كلمة المرور.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}
