using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using LTHDT2.Models;
using LTHDT2.DataAccess.Repositories;
using AppModel = LTHDT2.Models.Application;

namespace LTHDT2.Services
{
    /// <summary>
    /// Email Service - Implement IEmailService
    /// </summary>
    public class EmailService : BaseService, IEmailService
    {
        private readonly EmailLogRepository _emailLogRepository;
        private readonly SmtpClient _smtpClient;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService()
        {
            _emailLogRepository = new EmailLogRepository();
            
            // Load SMTP config từ App.config
            var smtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
            var smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"] ?? "";
            var smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"] ?? "";
            var smtpEnableSSL = bool.Parse(ConfigurationManager.AppSettings["SmtpEnableSSL"] ?? "true");
            
            _fromEmail = ConfigurationManager.AppSettings["SmtpFromEmail"] ?? "noreply@company.com";
            _fromName = ConfigurationManager.AppSettings["SmtpFromName"] ?? "HR Management System";

            _smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = smtpEnableSSL,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };
        }

        /// <summary>
        /// Method Overloading
        /// </summary>
        public bool SendEmail(string to, string subject, string body)
        {
            return SendEmail(to, subject, body, false);
        }

        /// <summary>
        /// Gửi email với định dạng HTML
        /// </summary>
        public bool SendEmail(string to, string subject, string body, bool isHtml)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                message.To.Add(to);

                _smtpClient.Send(message);
                
                LogAction("Email Sent", $"To: {to}, Subject: {subject}");
                return true;
            }
            catch (Exception ex)
            {
                LogError(nameof(SendEmail), ex);
                return false;
            }
        }

        /// <summary>
        /// Gửi email xác nhận nhận đơn ứng tuyển
        /// </summary>
        public bool SendApplicationConfirmation(AppModel application)
        {
            try
            {
                if (application.Candidate == null || application.JobPosting == null)
                {
                    application = new ApplicationRepository().GetById(application.Id);
                    if (application == null) return false;
                }

                var subject = "Xác nhận nhận hồ sơ ứng tuyển";
                var body = $@"
                    <html>
                    <body>
                        <h2>Xin chào {application.Candidate?.FullName},</h2>
                        <p>Chúng tôi đã nhận được hồ sơ ứng tuyển của bạn cho vị trí <strong>{application.JobPosting?.JobTitle}</strong>.</p>
                        <p>Mã đơn ứng tuyển của bạn: <strong>#{application.Id}</strong></p>
                        <p>Chúng tôi sẽ xem xét hồ sơ và liên hệ lại với bạn trong thời gian sớm nhất.</p>
                        <br/>
                        <p>Trân trọng,<br/>Phòng Nhân sự</p>
                    </body>
                    </html>
                ";

                var result = SendEmail(application.Candidate?.Email ?? "", subject, body, true);
                LogEmailToDatabase(application.Id, "Confirmation", application.Candidate?.Email ?? "", subject, body, result);
                
                return result;
            }
            catch (Exception ex)
            {
                LogError(nameof(SendApplicationConfirmation), ex);
                return false;
            }
        }

        /// <summary>
        /// Gửi email mời phỏng vấn
        /// </summary>
        public bool SendInterviewInvitation(Interview interview)
        {
            try
            {
                if (interview.Application?.Candidate == null || interview.Application?.JobPosting == null)
                {
                    // Load full info
                    var appRepo = new ApplicationRepository();
                    interview.Application = appRepo.GetById(interview.ApplicationId);
                    if (interview.Application == null) return false;
                }

                var subject = "Thư mời phỏng vấn";
                var body = $@"
                    <html>
                    <body>
                        <h2>Xin chào {interview.Application.Candidate?.FullName},</h2>
                        <p>Chúc mừng! Hồ sơ của bạn đã được chọn để tham gia phỏng vấn cho vị trí <strong>{interview.Application.JobPosting?.JobTitle}</strong>.</p>
                        <h3>Thông tin phỏng vấn:</h3>
                        <ul>
                            <li><strong>Thời gian:</strong> {interview.InterviewDate:dd/MM/yyyy HH:mm}</li>
                            <li><strong>Hình thức:</strong> {interview.InterviewType}</li>
                            <li><strong>Vòng:</strong> {interview.InterviewRound}</li>
                        </ul>
                        <p>Vui lòng xác nhận tham gia và chuẩn bị đầy đủ.</p>
                        <br/>
                        <p>Trân trọng,<br/>Phòng Nhân sự</p>
                    </body>
                    </html>
                ";

                var result = SendEmail(interview.Application.Candidate?.Email ?? "", subject, body, true);
                LogEmailToDatabase(interview.ApplicationId, "InterviewInvitation", interview.Application.Candidate?.Email ?? "", subject, body, result);
                
                return result;
            }
            catch (Exception ex)
            {
                LogError(nameof(SendInterviewInvitation), ex);
                return false;
            }
        }

        /// <summary>
        /// Gửi email thông báo kết quả
        /// </summary>
        public bool SendResultNotification(AppModel application, string result)
        {
            try
            {
                if (application.Candidate == null || application.JobPosting == null)
                {
                    application = new ApplicationRepository().GetById(application.Id);
                    if (application == null) return false;
                }

                var isPassed = result.Equals("Pass", StringComparison.OrdinalIgnoreCase);
                var subject = isPassed ? "Chúc mừng! Bạn đã trúng tuyển" : "Thông báo kết quả ứng tuyển";
                
                var body = isPassed
                    ? $@"
                        <html>
                        <body>
                            <h2>Xin chào {application.Candidate?.FullName},</h2>
                            <p>Chúc mừng! Bạn đã <strong style='color: green;'>TRÚNG TUYỂN</strong> vị trí <strong>{application.JobPosting?.JobTitle}</strong>.</p>
                            <p>Chúng tôi sẽ liên hệ với bạn trong thời gian sớm nhất để hoàn tất các thủ tục nhận việc.</p>
                            <br/>
                            <p>Trân trọng,<br/>Phòng Nhân sự</p>
                        </body>
                        </html>
                    "
                    : $@"
                        <html>
                        <body>
                            <h2>Xin chào {application.Candidate?.FullName},</h2>
                            <p>Cảm ơn bạn đã quan tâm và ứng tuyển vị trí <strong>{application.JobPosting?.JobTitle}</strong> tại công ty chúng tôi.</p>
                            <p>Rất tiếc, sau quá trình xem xét, chúng tôi nhận thấy hồ sơ của bạn chưa phù hợp với yêu cầu công việc lần này.</p>
                            <p>Chúng tôi rất mong được hợp tác với bạn trong tương lai.</p>
                            <br/>
                            <p>Trân trọng,<br/>Phòng Nhân sự</p>
                        </body>
                        </html>
                    ";

                var sendResult = SendEmail(application.Candidate?.Email ?? "", subject, body, true);
                var emailType = isPassed ? "PassNotification" : "FailNotification";
                LogEmailToDatabase(application.Id, emailType, application.Candidate?.Email ?? "", subject, body, sendResult);
                
                return sendResult;
            }
            catch (Exception ex)
            {
                LogError(nameof(SendResultNotification), ex);
                return false;
            }
        }

        /// <summary>
        /// Method test đơn giản để xác nhận gửi email hoạt động
        /// </summary>
        /// <param name="toEmail">Email người nhận</param>
        /// <returns>True nếu gửi thành công, False nếu có lỗi</returns>
        public bool TestSendEmail(string toEmail)
        {
            try
            {
                var subject = "[TEST] Kiểm tra gửi email - HR Management System";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2 style='color: #2c3e50;'>✅ Email Test Thành Công!</h2>
                        <p>Xin chào,</p>
                        <p>Đây là email test từ hệ thống <strong>HR Management System</strong>.</p>
                        <p>Nếu bạn nhận được email này, có nghĩa là cấu hình email đã hoạt động đúng.</p>
                        <hr style='border: 1px solid #ecf0f1; margin: 20px 0;'/>
                        <p style='color: #7f8c8d; font-size: 12px;'>
                            <strong>Thông tin cấu hình:</strong><br/>
                            SMTP Host: {_smtpClient.Host}<br/>
                            SMTP Port: {_smtpClient.Port}<br/>
                            From Email: {_fromEmail}<br/>
                            From Name: {_fromName}<br/>
                            Thời gian gửi: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
                        </p>
                        <p style='color: #95a5a6; font-size: 11px; margin-top: 20px;'>
                            Đây là email tự động, vui lòng không trả lời.
                        </p>
                    </body>
                    </html>";

                var result = SendEmail(toEmail, subject, body, true);
                
                if (result)
                {
                    LogAction("Email Test", $"Test email sent successfully to: {toEmail}");
                }
                else
                {
                    LogError("Email Test", new Exception("Failed to send test email"));
                }
                
                return result;
            }
            catch (Exception ex)
            {
                LogError("TestSendEmail", ex);
                return false;
            }
        }

        /// <summary>
        /// Gửi email với template
        /// </summary>
        public bool SendEmailWithTemplate(string to, string templateName, Dictionary<string, string> placeholders)
        {
            try
            {
                var template = "Template content here...";
                
                foreach (var placeholder in placeholders)
                {
                    template = template.Replace($"{{{placeholder.Key}}}", placeholder.Value);
                }

                return SendEmail(to, "Subject from template", template, true);
            }
            catch (Exception ex)
            {
                LogError(nameof(SendEmailWithTemplate), ex);
                return false;
            }
        }

        /// <summary>
        /// Log email vào database
        /// </summary>
        private void LogEmailToDatabase(int applicationId, string emailType, string recipientEmail, string subject, string body, bool isSent)
        {
            try
            {
                var log = new EmailLog
                {
                    ApplicationId = applicationId,
                    EmailType = emailType,
                    RecipientEmail = recipientEmail,
                    Subject = subject,
                    Body = body,
                    SentDate = DateTime.Now,
                    IsSent = isSent,
                    ErrorMessage = isSent ? null : "Failed to send"
                };

                _emailLogRepository.Add(log);
            }
            catch (Exception ex)
            {
                LogError("LogEmailToDatabase", ex);
            }
        }
    }

    /// <summary>
    /// EmailLog Repository
    /// </summary>
    public class EmailLogRepository : DataAccess.Repositories.BaseRepository<EmailLog>
    {
        protected override string TableName => "EmailLog";

        public override EmailLog? GetById(int id)
        {
            return null;
        }

        public override IEnumerable<EmailLog> GetAll()
        {
            return new List<EmailLog>();
        }

        public override int Add(EmailLog entity)
        {
            try
            {
                var sql = @"INSERT INTO EmailLog (ApplicationID, EmailType, RecipientEmail, Subject, Body, SentDate, IsSent, ErrorMessage)
                           VALUES (@ApplicationID, @EmailType, @RecipientEmail, @Subject, @Body, @SentDate, @IsSent, @ErrorMessage);
                           SELECT LAST_INSERT_ID();";
                
                var result = ExecuteScalar(sql,
                    new MySql.Data.MySqlClient.MySqlParameter("@ApplicationID", entity.ApplicationId),
                    new MySql.Data.MySqlClient.MySqlParameter("@EmailType", entity.EmailType ?? (object)DBNull.Value),
                    new MySql.Data.MySqlClient.MySqlParameter("@RecipientEmail", entity.RecipientEmail),
                    new MySql.Data.MySqlClient.MySqlParameter("@Subject", entity.Subject ?? (object)DBNull.Value),
                    new MySql.Data.MySqlClient.MySqlParameter("@Body", entity.Body ?? (object)DBNull.Value),
                    new MySql.Data.MySqlClient.MySqlParameter("@SentDate", entity.SentDate),
                    new MySql.Data.MySqlClient.MySqlParameter("@IsSent", entity.IsSent),
                    new MySql.Data.MySqlClient.MySqlParameter("@ErrorMessage", entity.ErrorMessage ?? (object)DBNull.Value)
                );

                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                LogError(nameof(Add), ex);
                return 0;
            }
        }

        public override bool Update(EmailLog entity) => false;
    }
}

