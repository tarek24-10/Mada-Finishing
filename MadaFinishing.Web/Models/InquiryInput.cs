using System.ComponentModel.DataAnnotations;

namespace MadaFinishing.Web.Models;

public sealed class InquiryInput
{
    [Display(Name = "اسم العميل")]
    [Required(ErrorMessage = "أدخل اسم العميل.")]
    [StringLength(Inquiry.CustomerNameMaxLength, ErrorMessage = "يجب ألا يتجاوز الاسم {1} حرفًا.")]
    public string CustomerName { get; set; } = "";

    [Display(Name = "رقم الهاتف")]
    [Required(ErrorMessage = "أدخل رقم الهاتف.")]
    [StringLength(Inquiry.PhoneMaxLength, ErrorMessage = "يجب ألا يتجاوز رقم الهاتف {1} حرفًا.")]
    public string Phone { get; set; } = "";

    [Display(Name = "نوع الخدمة")]
    [Required(ErrorMessage = "أدخل نوع الخدمة.")]
    [StringLength(Inquiry.ServiceTypeMaxLength, ErrorMessage = "يجب ألا يتجاوز نوع الخدمة {1} حرفًا.")]
    public string ServiceType { get; set; } = "";

    [Display(Name = "رسالة العميل (اختياري)")]
    [StringLength(Inquiry.MessageMaxLength, ErrorMessage = "يجب ألا تتجاوز الرسالة {1} حرفًا.")]
    public string? Message { get; set; }
}
