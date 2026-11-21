using System.ComponentModel.DataAnnotations;

namespace MadaFinishing.Web.Models;

public enum InquiryStatus
{
    [Display(Name = "جديد")]
    New = 0,

    [Display(Name = "تم التواصل")]
    Contacted = 1,

    [Display(Name = "تم إرسال عرض السعر")]
    QuoteSent = 2,

    [Display(Name = "مغلق")]
    Closed = 3
}
