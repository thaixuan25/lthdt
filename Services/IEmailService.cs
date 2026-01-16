using LTHDT2.Models;
using AppModel = LTHDT2.Models.Application;

namespace LTHDT2.Services
{
    /// <summary>
    /// Interface Email Service
    /// Áp dụng Abstraction - Định nghĩa contract cho email
    /// Thể hiện Polymorphism qua method overloading
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Gửi email đơn giản
        /// </summary>
        bool SendEmail(string to, string subject, string body);

        /// <summary>
        /// Gửi email với HTML
        /// Method Overloading - Polymorphism
        /// </summary>
        bool SendEmail(string to, string subject, string body, bool isHtml);

        /// <summary>
        /// Gửi email xác nhận nhận đơn ứng tuyển
        /// </summary>
        bool SendApplicationConfirmation(AppModel application);

        /// <summary>
        /// Gửi email mời phỏng vấn
        /// </summary>
        bool SendInterviewInvitation(Interview interview);

        /// <summary>
        /// Gửi email thông báo kết quả (đạt/không đạt)
        /// </summary>
        bool SendResultNotification(AppModel application, string result);

        /// <summary>
        /// Gửi email với template
        /// </summary>
        bool SendEmailWithTemplate(string to, string templateName, Dictionary<string, string> placeholders);
    }
}

