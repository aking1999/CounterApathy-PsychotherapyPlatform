using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using System.IO;
using System.Threading.Tasks;
using Framework.Emails.EmailTypes;
using System;
using Framework.Interfaces;
using Framework.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Database.Models;
using System.Linq;
using MailKit;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Framework.Providers;
using Framework.Helpers.ExtensionMethods;

namespace Framework.Emails
{
    public class MailService : IMailService
    {
        private const string PROJECT = "Framework";
        private const string CLASS = "MailService";

        private readonly IDateTimeHelper _dateHelper;
        private readonly IConfiguration _configuration;
        private readonly ISystemErrorLogger _systemError;
        private readonly IWebHostEnvironment _environment;
        private readonly MailSettings _mailSettings;
        private readonly UserManager<CustomClient> _userManager;
        private readonly LinkGenerator _linkGenerator;

        public MailService(IOptions<MailSettings> mailSettings,
            IServiceScopeFactory serviceScopeFactory,
            IDateTimeHelper dateHelper,
            IWebHostEnvironment environment)
        {
            _systemError = new SystemErrorLogger();
            _mailSettings = mailSettings.Value;
            _environment = environment;
            _dateHelper = dateHelper;

            var scope = serviceScopeFactory.CreateScope();
            _linkGenerator = scope.ServiceProvider.GetRequiredService<LinkGenerator>();
            _configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            _userManager = scope.ServiceProvider.GetRequiredService<UserManager<CustomClient>>();
        }

        public async Task SendEmailAsync(Email email)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Sender = MailboxAddress.Parse(_mailSettings.Mail),
                    Subject = email.Subject
                };

                mime.To.Add(MailboxAddress.Parse(email.ToEmail));

                var builder = new BodyBuilder();
                if (email.Attachments != null)
                {
                    byte[] fileBytes;
                    foreach (var file in email.Attachments)
                    {
                        if (file.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                fileBytes = ms.ToArray();
                            }
                            builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                        }
                    }
                }

                builder.HtmlBody = email.Body;
                mime.Body = builder.ToMessageBody();
                using var smtp = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendEmailAsync.Email.log"));
                smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp.SendAsync(mime);
                smtp.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendEmailAsync");
                return;
            }
        }

        public async Task SendEmailAsync(EmailForRole email, string userRole)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Sender = MailboxAddress.Parse(_mailSettings.Mail),
                    Subject = email.Subject
                };

                var builder = new BodyBuilder();
                if (email.Attachments != null)
                {
                    byte[] fileBytes;
                    foreach (var file in email.Attachments)
                    {
                        if (file.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                fileBytes = ms.ToArray();
                            }
                            builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                        }
                    }
                }

                builder.HtmlBody = email.Body;
                mime.Body = builder.ToMessageBody();
                using var smtp = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendEmailAsync.EmailForRole.log"));
                smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);

                var userEmails = (await _userManager.GetUsersInRoleAsync(userRole)).Select(usr => usr.Email);

                if (!userEmails.Any())
                    throw new Exception($"No users in role '{userRole}' to send email to.");

                foreach (var receriverUserEmail in userEmails)
                {
                    mime.To.Add(MailboxAddress.Parse(receriverUserEmail));
                    await smtp.SendAsync(mime);
                }

                smtp.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendEmailAsync");
                return;
            }
        }

        public async Task SendEmailConfirmationEmailAsync(ConfirmationEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = "Potvrdite Vaš imejl",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.ToEmail));

                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za EmailConfirmation, trba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {
                    var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, @"..\Framework\Emails\EmailTemplates\EmailConfirmationTemplate\email-confirmation.html"));

                    if (new FileInfo(filePath).Exists)
                    {
                        var mailText = await new StreamReader(filePath).ReadToEndAsync();
                        mailText = mailText.Replace("{{FirstName}}", email.FirstName);
                        mailText = mailText.Replace("{{Uid}}", email.UserId);
                        mailText = mailText.Replace("{{Token}}", email.Token);

                        builder.HtmlBody = mailText;
                        mime.Body = builder.ToMessageBody();
                        using var smtp1 = new SmtpClient();
                        smtp1.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                        smtp1.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                        await smtp1.SendAsync(mime);
                        smtp1.Disconnect(true);

                        return;
                    }
                }

                builder.HtmlBody = $"Zdravo {email.FirstName}," +
                                   $"<br />" +
                                   $"<br />" +
                                   $"<a href='{_linkGenerator.GetPathByAction("EmailConfirmation", "Authorization", new { uid = email.UserId, token = email.Token }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>Potvrdite Vaš imejl klikom ovde</a>." +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Ukoliko Vam je potrebna pomoć ili imate pitanja, <a href='{_linkGenerator.GetPathByAction("CustomerSupport", "Home", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>kontaktirajte korisničku podršku</a>." +
                                   "<br />" +
                                   "<br />" +
                                   $"Sve najbolje," +
                                   $"<br />" +
                                   $"{_configuration.GetSection("Application:AppName")?.Value} tim";

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient();
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendEmailConfirmationEmailAsync");
                return;
            }
        }

        public async Task SendPasswordResetEmailAsync(PasswordResetEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = "Obnovite lozinku naloga",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };
                
                mime.To.Add(MailboxAddress.Parse(email.ToEmail));

                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za PasswordReset, trba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {
                    var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, @"..\Framework\Emails\EmailTemplates\PasswordResetTemplate\password-reset.html"));

                    if (new FileInfo(filePath).Exists)
                    {
                        var mailText = await new StreamReader(filePath).ReadToEndAsync();
                        mailText = mailText.Replace("{{FirstName}}", email.FirstName);
                        mailText = mailText.Replace("{{Uid}}", email.UserId);
                        mailText = mailText.Replace("{{Token}}", email.Token);

                        builder.HtmlBody = mailText;
                        mime.Body = builder.ToMessageBody();
                        using var smtp1 = new SmtpClient();
                        smtp1.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                        smtp1.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                        await smtp1.SendAsync(mime);
                        smtp1.Disconnect(true);

                        return;
                    }
                }

                builder.HtmlBody = $"Zdravo {email.FirstName}," +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Primili smo Vaš zahtev za obnovu lozinke naloga." +
                                   $"<br />" +
                                   $"Obnovite lozinku klikom <a href='{_linkGenerator.GetPathByAction("PasswordReset", "Authorization", new { uid = email.UserId, token = email.Token }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>ovde</a>." +
                                   $"<br />" +
                                   $"Ako niste Vi poslali ovaj zahtev za obnovu lozinke, ignorišite ovaj imejl." +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Ukoliko Vam je potrebna pomoć ili imate pitanja, <a href='{_linkGenerator.GetPathByAction("CustomerSupport", "Home", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>kontaktirajte korisničku podršku</a>." +
                                   "<br />" +
                                   "<br />" +
                                   $"Sve najbolje," +
                                   $"<br />" +
                                   $"{_configuration.GetSection("Application:AppName")?.Value} tim";

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient();
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendPasswordResetEmailAsync");
                return;
            }
        }

        public async Task SendWelcomeEmailAsync(WelcomeEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = $"Dobro došli {email.FirstName}",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.ToEmail));

                var builder = new BodyBuilder();

                if (includeTemplateIfExists)
                {
                    string filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, @"..\Framework\Emails\EmailTemplates\WelcomeTemplate\welcome.html"));

                    if (new FileInfo(filePath).Exists)
                    {
                        var mailText = await new StreamReader(filePath).ReadToEndAsync();
                        mailText = mailText.Replace("{{FirstName}}", email.FirstName);

                        builder.HtmlBody = mailText;
                        mime.Body = builder.ToMessageBody();
                        using var smtp1 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendWelcomeEmailAsync.log"));
                        smtp1.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                        smtp1.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                        await smtp1.SendAsync(mime);
                        smtp1.Disconnect(true);

                        return;
                    }
                }

                builder.HtmlBody = $"Zdravo {email.FirstName}," +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Hvala Vam što ste izabrali {_configuration.GetSection("Application:AppName")?.Value}." +
                                   $"<br />" +
                                   $"Pogledajte naše terapeute klikom <a href='{_linkGenerator.GetPathByAction("All", "Therapists", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>ovde</a>." +
                                   $"<br />" +
                                   $"Saznajte kako da <a href='{_linkGenerator.GetPathByAction("BookingRoadmap", "Guides", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>zakažete seansu u 4 jednostava koraka</a>." +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Ukoliko Vam je potrebna pomoć ili imate pitanja, <a href='{_linkGenerator.GetPathByAction("CustomerSupport", "Home", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>kontaktirajte korisničku podršku</a>." +
                                   "<br />" +
                                   "<br />" +
                                   $"Sve najbolje," +
                                   $"<br />" +
                                   $"{_configuration.GetSection("Application:AppName")?.Value} tim";

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendWelcomeEmailAsync.log"));
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendWelcomeEmailAsync");
                return;
            }
        }

        public async Task SendSignInSuccessfulEmailAsync(SignInSuccessfulEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = "Logovanje uspešno",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.ToEmail));

                var localizedDateTime = _dateHelper.ConvertDateTimeFromUtcToLocalString(DateTime.UtcNow);

                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za SignInSuccessful, trba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {
                    var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, @"..\Framework\Emails\EmailTemplates\SignInSuccessfulTemplate\sign-in-successful.html"));

                    if (new FileInfo(filePath).Exists)
                    {
                        var mailText = await new StreamReader(filePath).ReadToEndAsync();
                        mailText = mailText.Replace("{{LocalizedDateTime}}", localizedDateTime);

                        builder.HtmlBody = mailText;
                        mime.Body = builder.ToMessageBody();
                        using var smtp1 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendSignInSuccessfulEmailAsync.log"));
                        smtp1.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                        smtp1.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                        await smtp1.SendAsync(mime);
                        smtp1.Disconnect(true);

                        return;
                    }
                }

                builder.HtmlBody = $"Zdravo," +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Uspešno ste se ulogovali na svoj nalog datuma {localizedDateTime}h." +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Sve najbolje," +
                                   $"<br />" +
                                   $"{_configuration.GetSection("Application:AppName")?.Value} tim";

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendSignInSuccessfulEmailAsync.log"));
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendSignInSuccessfulEmailAsync");
                return;
            }
        }

        public async Task SendPasswordChangedEmailAsync(PasswordChangedEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = "Lozinka uspešno promenjena",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.ToEmail));

                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za PasswordChanged, treba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {
                    var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, @"..\Framework\Emails\EmailTemplates\PasswordChangedTemplate\password-changed.html"));

                    if (new FileInfo(filePath).Exists)
                    {
                        var mailText = await new StreamReader(filePath).ReadToEndAsync();

                        //ovde treba da stoje razlicite {{}} stvari, ali trenutno nemam template pa nije ni bitno
                        //mailText = mailText.Replace("{{FirstName}}", email.FirstName);
                        //mailText = Replace() nesto drugo itd...

                        builder.HtmlBody = mailText;
                        mime.Body = builder.ToMessageBody();
                        using var smtp1 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendPasswordChangedEmailAsync.log"));
                        smtp1.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                        smtp1.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                        await smtp1.SendAsync(mime);
                        smtp1.Disconnect(true);

                        return;
                    }
                }

                var changedDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(DateTime.UtcNow);

                builder.HtmlBody = $"Zdravo {email.FirstName}," +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Uspešno ste promenili lozinku datuma {_dateHelper.DateStringFromDateTime(changedDateTime)}, u {_dateHelper.TimeStringFromDateTime(changedDateTime)}h." +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Sve najbolje," +
                                   $"<br />" +
                                   $"{_configuration.GetSection("Application:AppName")?.Value} tim";

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendPasswordChangedEmailAsync.log"));
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendPasswordChangedEmailAsync");
                return;
            }
        }

        public async Task SendWebCreditAddedEmailAsync(WebCreditAddedEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = $"Iznos Veb kredita na Vašem nalogu je uvećan za RSD {email.Amount}.",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.ToEmail));

                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za WebCreditAdded, treba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {
                    var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, @"..\Framework\Emails\EmailTemplates\PasswordChangedTemplate\password-changed.html"));

                    if (new FileInfo(filePath).Exists)
                    {
                        var mailText = await new StreamReader(filePath).ReadToEndAsync();

                        //ovde treba da stoje razlicite {{}} stvari, ali trenutno nemam template pa nije ni bitno
                        //mailText = mailText.Replace("{{FirstName}}", email.FirstName);
                        //mailText = Replace() nesto drugo itd...

                        builder.HtmlBody = mailText;
                        mime.Body = builder.ToMessageBody();
                        using var smtp1 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendWebCreditAddedEmailAsync.log"));
                        smtp1.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                        smtp1.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                        await smtp1.SendAsync(mime);
                        smtp1.Disconnect(true);

                        return;
                    }
                }

                var changedDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(DateTime.UtcNow);

                builder.HtmlBody = $"Zdravo {email.FirstName}," +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Iznos Veb kredita na Vašen nalogu je uvećan za RSD {email.Amount} datuma {_dateHelper.DateStringFromDateTime(changedDateTime)}, u {_dateHelper.TimeStringFromDateTime(changedDateTime)}h." +
                                   $"<br />" +
                                   $"Ukupan Veb kredit na Vašem nalogu trenutno iznosi RSD {email.CurrentAmount}." +
                                   $"<br />" +
                                   $"Stanje Veb kredita na nalogu možete pratiti odlaskom na svoj <a href='{_linkGenerator.GetPathByAction("Profile", "Account", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>profil</a>." +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Pogledajte naše terapeute klikom <a href='{_linkGenerator.GetPathByAction("All", "Therapists", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>ovde</a>." +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Sve najbolje," +
                                   $"<br />" +
                                   $"{_configuration.GetSection("Application:AppName")?.Value} tim";

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendWebCreditAddedEmailAsync.log"));
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendWebCreditAddedEmailAsync");
                return;
            }
        }

        public async Task SendWithdrawalRequestAcceptedEmailAsync(WithdrawalRequestAcceptedEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = $"Sredstva u iznosu od RSD {email.Amount} su uspešno poslata na Vaš Stripe nalog",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.ToEmail));

                var localizedDateTime = _dateHelper.ConvertDateTimeFromUtcToLocalString(DateTime.UtcNow);

                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za WithdrawalRequestAccepted, trba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {
                    string filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, @"..\Framework\Emails\EmailTemplates\WithdrawalRequestAcceptedTemplate\withdrawal-request-accepted.html"));

                    if (new FileInfo(filePath).Exists)
                    {
                        var mailText = await new StreamReader(filePath).ReadToEndAsync();
                        mailText = mailText.Replace("{{FirstName}}", email.FirstName);
                        mailText = mailText.Replace("{{Amount}}", email.Amount);
                        mailText = mailText.Replace("{{LocalizedDateTime}}", localizedDateTime);

                        builder.HtmlBody = mailText;
                        mime.Body = builder.ToMessageBody();
                        using var smtp1 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendWithdrawalRequestAcceptedEmailAsync.log"));
                        smtp1.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                        smtp1.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                        await smtp1.SendAsync(mime);
                        smtp1.Disconnect(true);

                        return;
                    }
                }

                builder.HtmlBody = $"Zdravo {email.FirstName}," +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Sredstva u iznosu od RSD {email.Amount} su uspešno poslata na Vaš <a href='{_linkGenerator.GetPathByAction("StripeAccount", "Withdrawals", new { Area = "Therapist" }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>Stripe nalog</a>, datuma {localizedDateTime}h, " +
                                   $"odakle se automatski prosleđuju na Vaš devizni bankovni račun i ležu u roku od 1-14 radnih dana." +
                                   $"<br />" +
                                   $"Pogledajte Vaš stripe nalog klikom <a href='{_linkGenerator.GetPathByAction("StripeAccount", "Withdrawals", new { Area = "Therapist" }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>ovde</a>." +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Ukoliko Vam je potrebna pomoć ili imate pitanja, <a href='{_linkGenerator.GetPathByAction("Support", "Account", new { Area = "Therapist" }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>kontaktirajte podršku za terapeute</a>." +
                                   "<br />" +
                                   "<br />" +
                                   $"Sve najbolje," +
                                   $"<br />" +
                                   $"{_configuration.GetSection("Application:AppName")?.Value} tim";

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendWithdrawalRequestAcceptedEmailAsync.log"));
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendWithdrawalRequestAcceptedEmailAsync");
                return;
            }
        }

        public async Task SendTherapistApplicationReceivedEmailAsync(TherapistApplicationStatusEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = "Uspešno ste aplicirali za dobijanje naloga psihoterapeuta",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.ToEmail));


                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za ApplicationReceived, treba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {
                    var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, @"..\Framework\Emails\EmailTemplates\TherapistApplicationStatusTemplate\therapist-application-received.html"));

                    if (new FileInfo(filePath).Exists)
                    {
                        var mailText = await new StreamReader(filePath).ReadToEndAsync();
                        mailText = mailText.Replace("{{FirstName}}", email.FirstName);

                        builder.HtmlBody = mailText;
                        mime.Body = builder.ToMessageBody();
                        using var smtp1 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendTherapistApplicationReceivedEmailAsync.log"));
                        smtp1.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                        smtp1.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                        await smtp1.SendAsync(mime);
                        smtp1.Disconnect(true);

                        return;
                    }
                }

                builder.HtmlBody = $"Zdravo {email.FirstName}," +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Hvala Vam što ste zainteresovani za rad na {_configuration.GetSection("Application:AppName")?.Value} platformi." +
                                   $"<br />" +
                                   $"Bićete uskoro kontaktirani putem imejla u vezi nastavka proseca apliciranja." +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Ukoliko Vam je potrebna pomoć ili imate pitanja, <a href='{_linkGenerator.GetPathByAction("CustomerSupport", "Home", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>kontaktirajte korisničku podršku</a>." +
                                   "<br />" +
                                   "<br />" +
                                   $"Sve najbolje," +
                                   $"<br />" +
                                   $"{_configuration.GetSection("Application:AppName")?.Value} tim";

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendTherapistApplicationReceivedEmailAsync.log"));
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendTherapistApplicationReceivedEmailAsync");
                return;
            }
        }

        public async Task SendTherapistApplicationAcceptedEmailAsync(TherapistApplicationStatusEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = "Vaša aplikacija za nalog psihoterapeuta je prihvaćena",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.ToEmail));

                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za ApplicationAccepted, trba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {
                    var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, @"..\Framework\Emails\EmailTemplates\TherapistApplicationStatusTemplate\therapist-application-accepted.html"));

                    if (new FileInfo(filePath).Exists)
                    {
                        var mailText = await new StreamReader(filePath).ReadToEndAsync();
                        mailText = mailText.Replace("{{FirstName}}", email.FirstName);

                        builder.HtmlBody = mailText;
                        mime.Body = builder.ToMessageBody();
                        using var smtp1 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendTherapistApplicationAcceptedEmailAsync.log"));
                        smtp1.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                        smtp1.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                        await smtp1.SendAsync(mime);
                        smtp1.Disconnect(true);

                        return;
                    }
                }

                builder.HtmlBody = $"Zdravo {email.FirstName}," +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Dobro došli u naš tim stručnjaka." +
                                   $"<br />" +
                                   $"Vaša aplikacija je pregledana i prihvaćena." +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Ovo je detaljan redosled stavki koje treba da ispunite kako biste uspešno podesili Vaš nov terapeutski nalog:" +
                                   $"<ol>" +
                                   $"<li>Ukoliko ste ulogovani, izlogujte se sa svog naloga na platformi, zatim se ponovo ulogujte starim imejlom i lozinkom. Ovim će doći do ažuriranja naloga.</li>" +
                                   $"<li>Nakon što ste se ponovo ulogovali, izvršite podešavanje naloga klikom <a href='{_linkGenerator.GetPathByAction("AccountSetup", "Account", new { Area = "Therapist" }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>ovde</a> " +
                                   $"i popunjavanjem formulara.</li>" +
                                   $"<li>Dodajte psihoterapijske seanse klikom <a href='{_linkGenerator.GetPathByAction("All", "Sessions", new { Area = "Therapist" }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>ovde</a>.</li>" +
                                   $"<li>(opciono) Dodajte uže informacije o Vašem iskustvu sa psihoterapijskim tehnikama klikom <a href='{_linkGenerator.GetPathByAction("PsychotherapyTechniquesExperiences", "Account", new { Area = "Therapist" }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>ovde</a>.</li>" +
                                   $"<li>(opciono) Dodajte uže informacije o Vašem iskustvu sa specijalnostima klikom <a href='{_linkGenerator.GetPathByAction("SpecialtiesExperiences", "Account", new { Area = "Therapist" }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>ovde</a>.</li>" +
                                   $"</ol>" +
                                   $"<br />" +
                                   $"Ukoliko Vam je potrebna pomoć ili imate pitanja, <a href='{_linkGenerator.GetPathByAction("Support", "Account", new { Area = "Therapist" }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>kontaktirajte podršku za terapeute</a>." +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Sve najbolje," +
                                   $"<br />" +
                                   $"{_configuration.GetSection("Application:AppName")?.Value} tim";

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendTherapistApplicationAcceptedEmailAsync.log"));
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendTherapistApplicationAcceptedEmailAsync");
                return;
            }
        }

        public async Task SendTherapistApplicationRejectedEmailAsync(TherapistApplicationStatusEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = "Vaša aplikacija za nalog psihoterapeuta je odbijena",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.ToEmail));

                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za ApplicationRejected, treba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {
                    var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, @"..\Framework\Emails\EmailTemplates\TherapistApplicationStatusTemplate\therapist-application-rejected.html"));

                    if (new FileInfo(filePath).Exists)
                    {
                        var mailText = await new StreamReader(filePath).ReadToEndAsync();
                        mailText = mailText.Replace("{{FirstName}}", email.FirstName);

                        builder.HtmlBody = mailText;
                        mime.Body = builder.ToMessageBody();
                        using var smtp1 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendTherapistApplicationRejectedEmailAsync.log"));
                        smtp1.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                        smtp1.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                        await smtp1.SendAsync(mime);
                        smtp1.Disconnect(true);

                        return;
                    }
                }

                builder.HtmlBody = $"Zdravo {email.FirstName}," +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Vaša aplikacija za nalog psihoterapeuta je pregledana i odbijena." +
                                   $"<br />" +
                                   $"<br />" +
                                   $"Sve najbolje," +
                                   $"<br />" +
                                   $"{_configuration.GetSection("Application:AppName")?.Value} tim";

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendTherapistApplicationRejectedEmailAsync.log"));
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendTherapistApplicationRejectedEmailAsync");
                return;
            }
        }

        public async Task SendClientBookedConsultationEmailAsync(BookedConsultationEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = $"Detalji zakazanih konsultacija sa psihoterapeutom {email.BookedConsultation.TherapistFirstName} {email.BookedConsultation.TherapistLastName}",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.BookedConsultation.ClientEmail));

                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za BookedConsultation, treba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {

                }

                builder.HtmlBody = $"Zdravo {email.BookedConsultation.ClientFirstName}," +
                                       "<br />" +
                                       "<br />" +
                                       $"Uspešno ste zakazali besplatne konsultacije sa psihoterapeutom {email.BookedConsultation.TherapistFirstName} {email.BookedConsultation.TherapistLastName}." +
                                       $"<br />" +
                                       $"<br />" +
                                       "Tip: Individualne konsultacije" +
                                       "<br />" +
                                       $"Datum početka: {_dateHelper.ConvertDateTimeFromUtcToLocalString(email.BookedConsultation.StartDateTime)}h" +
                                       "<br />" +
                                       $"Datum završetka: {_dateHelper.ConvertDateTimeFromUtcToLocalString(email.BookedConsultation.EndDateTime)}h" +
                                       "<br />" +
                                       $"Kontakt metoda: {email.BookedConsultation.ContactMethodName}" +
                                       $"<br />" +
                                       $"Imejl terapeuta: {email.BookedConsultation.TherapistEmail}" +
                                       "<br />" +
                                       $"Broj telefona terapeuta: {email.BookedConsultation.TherapistPhoneNumber}" +
                                       "<br />" +
                                       "<br />" +
                                       $"Najkasnije 4 sata pre početka konsultacija, {email.BookedConsultation.ContactMethodName} link za pristup konsultacijama biće poslat na ovaj imejl. Takodje, jedna kopija linka će biti dostupna i na " +
                                       $"<a href='{_linkGenerator.GetPathByAction("ConsultationDetails", "Consultations", new { bookingId = email.BookedConsultation.Id }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>stranici sa detaljima o zakazanim konsultacijama</a>." +
                                       "<br />" +
                                       "<br />" +
                                       $"Za pristup konsultacijama putem računara, pročitajte <a href='{_linkGenerator.GetPathByAction("JoinGoogleMeetByComputer", "Guides", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>uputstvo za pristup putem računara</a>." +
                                       "<br />" +
                                       $"Za pristup konsultacijama putem mobilnog telefona, pročitajte <a href='{_linkGenerator.GetPathByAction("JoinGoogleMeetByPhone", "Guides", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>uputstvo za pristup putem mobilnog telefona</a>." +
                                       "<br />" +
                                       "<br />" +
                                       "Sve najbolje," +
                                       "<br />" +
                                       $"{_configuration.GetSection("Application:AppName")?.Value} tim";

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendClientBookedConsultationEmailAsync.log"));
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendClientBookedConsultationEmailAsync");
                return;
            }
        }

        public async Task SendClientBookedSessionEmailAsync(BookedSessionEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = $"Detalji zakazane seanse sa psihoterapeutom {email.BookedSession.TherapistFirstName} {email.BookedSession.TherapistLastName}",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.BookedSession.ClientEmail));

                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za BookedSession, treba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {
                    var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, @"..\Framework\Emails\EmailTemplates\BookedSessionTemplate\booked-session.html"));

                    if (new FileInfo(filePath).Exists)
                    {
                        var mailText = await new StreamReader(filePath).ReadToEndAsync();
                        mailText = mailText.Replace("{{ClientFullName}}", email.BookedSession.ClientFirstName + " " + email.BookedSession.ClientLastName);
                        mailText = mailText.Replace("{{ClientEmail}}", email.BookedSession.ClientEmail);
                        mailText = mailText.Replace("{{ClientPhoneNumber}}", email.BookedSession.ClientPhoneNumber);

                        mailText = mailText.Replace("{{TherapistFullName}}", email.BookedSession.TherapistFirstName + " " + email.BookedSession.TherapistLastName);
                        mailText = mailText.Replace("{{TherapistEmail}}", email.BookedSession.TherapistEmail);
                        mailText = mailText.Replace("{{TherapistPhoneNumber}}", email.BookedSession.TherapistPhoneNumber);

                        mailText = mailText.Replace("{{Type}}", email.BookedSession.Type == 0 ? "Individual session" : "Group session");
                        mailText = mailText.Replace("{{Price}}", email.BookedSession.Price.ToString());
                        mailText = mailText.Replace("{{StartDate}}", _dateHelper.ConvertDateTimeFromUtcToLocalString(email.BookedSession.StartTime));
                        mailText = mailText.Replace("{{EndDate}}", _dateHelper.ConvertDateTimeFromUtcToLocalString(email.BookedSession.EndTime));
                        mailText = mailText.Replace("{{ContactMethodName}}", email.BookedSession.ContactMethodName);
                        //mailText = mailText.Replace("{{ContactInfo}}", email.BookedSession.ContactInfo);

                        builder.HtmlBody = mailText;
                        mime.Body = builder.ToMessageBody();
                        using var smtp1 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendClientBookedSessionEmailAsync.log"));
                        smtp1.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                        smtp1.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                        await smtp1.SendAsync(mime);
                        smtp1.Disconnect(true);

                        return;
                    }
                }

                var sessionDetailsUrl = string.Empty;
                var sessionRatingUrl = string.Empty; 

                if (email.BookedSession.ClientId.IsAnonymousOrUnauthorized())
                {
                    sessionDetailsUrl = _linkGenerator.GetPathByAction("SessionDetailsUnauthorized", "Sessions", new { email = email.BookedSession.ClientEmail, bookingId = email.BookedSession.Id }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..];
                    sessionRatingUrl = _linkGenerator.GetPathByAction("UnratedSessionsUnauthorized", "Sessions", new { email = email.BookedSession.ClientEmail, bookingId = email.BookedSession.Id }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..];
                }
                else
                {
                    sessionDetailsUrl = _linkGenerator.GetPathByAction("SessionDetails", "Sessions", new { bookingId = email.BookedSession.Id }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..];
                    sessionRatingUrl = _linkGenerator.GetPathByAction("UnratedSessions", "Sessions", new { bookingId = email.BookedSession.Id }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..];
                }   

                if (email.BookedSession.ContactMethodId.ToLower() == ContactMethodsProvider.InPerson.Id)
                {
                    builder.HtmlBody = $"Zdravo {email.BookedSession.ClientFirstName}," +
                                       "<br />" +
                                       "<br />" +
                                       $"Uspešno ste zakazali {(email.BookedSession.Type == 0 ? "individualnu" : "grupnu")} ličnu seansu sa psihoterapeutom {email.BookedSession.TherapistFirstName} {email.BookedSession.TherapistLastName}." +
                                       $"<br />" +
                                       $"<br />" +
                                       "Tip: " + (email.BookedSession.Type == 0 ? "Individualna seansa" : "Grupna seansa") +
                                       "<br />" +
                                       $"Datum početka: {_dateHelper.ConvertDateTimeFromUtcToLocalString(email.BookedSession.StartTime)}h" +
                                       "<br />" +
                                       $"Datum završetka: {_dateHelper.ConvertDateTimeFromUtcToLocalString(email.BookedSession.EndTime)}h" +
                                       "<br />" +
                                       $"Kontakt metoda: {email.BookedSession.ContactMethodName}" +
                                       "<br />" +
                                       $"Lokacija održavanja: {email.BookedSession.TherapistStreet} {email.BookedSession.TherapistHouseNumber}, {email.BookedSession.TherapistCity} {email.BookedSession.TherapistPostalCode}, {email.BookedSession.TherapistCountry}." +
                                       "<br />" +
                                       $"Molimo {(email.BookedSession.Type == 1 ? "svi grupno" : "")} budite na lokaciji održavanja 5 minuta pre početka seanse." +
                                       "<br />" +
                                       "<br />" +
                                       $"Imejl terapeuta: {email.BookedSession.TherapistEmail}" +
                                       "<br />" +
                                       $"Broj telefona terapeuta: {email.BookedSession.TherapistPhoneNumber}" +
                                       "<br />" +
                                       "<br />" +
                                       $"Za dodatne informacije o zakazanoj seansi, posetite <a target='_blank' href='{sessionDetailsUrl}'>stranicu sa detaljima o seansi</a>." +
                                       "<br />" +
                                       "<br />" +
                                       $"Nakon završetka seanse, molimo da ocenite seansu klikom <a target='_blank' href='{sessionRatingUrl}'>ovde</a>." +
                                       "<br />" +
                                       "<br />" +
                                       "<b><i>BITNO: Kada ocenite seansu, platforma garantuje da ni u kom trenutku neće Vaše lične podatke prikazati javno. Biće prikazani samo ocena, komentar i datum ocenjivanja.</i></b>" +
                                       "<br />" +
                                       "<br />" +
                                       "Sve najbolje," +
                                       "<br />" +
                                       $"{_configuration.GetSection("Application:AppName")?.Value} tim";
                }
                else
                {
                    builder.HtmlBody = $"Zdravo {email.BookedSession.ClientFirstName}," +
                                       "<br />" +
                                       "<br />" +
                                       $"Uspešno ste zakazali {(email.BookedSession.Type == 0 ? "individualnu" : "grupnu")} {email.BookedSession.ContactMethodName} seansu sa psihoterapeutom {email.BookedSession.TherapistFirstName} {email.BookedSession.TherapistLastName}." +
                                       $"<br />" +
                                       $"<br />" +
                                       "Tip: " + (email.BookedSession.Type == 0 ? "Individualna seansa" : "Grupna seansa") +
                                       "<br />" +
                                       $"Datum početka: {_dateHelper.ConvertDateTimeFromUtcToLocalString(email.BookedSession.StartTime)}h" +
                                       "<br />" +
                                       $"Datum završetka: {_dateHelper.ConvertDateTimeFromUtcToLocalString(email.BookedSession.EndTime)}h" +
                                       "<br />" +
                                       $"Kontakt metoda: {email.BookedSession.ContactMethodName}" +
                                       $"<br />" +
                                       $"Imejl terapeuta: {email.BookedSession.TherapistEmail}" +
                                       "<br />" +
                                       $"Broj telefona terapeuta: {email.BookedSession.TherapistPhoneNumber}" +
                                       "<br />" +
                                       "<br />" +
                                       $"Najkasnije 4 sata pre početka seanse, {email.BookedSession.ContactMethodName} link za pristup seansi biće poslat na ovaj imejl. Takodje, jedna kopija linka će biti dostupna i na " +
                                       $"<a href='{sessionDetailsUrl}'>stranici sa detaljima o zakazanoj seansi</a>." +
                                       "<br />" +
                                       "<br />" +
                                       $"Za pristup seansi putem računara, pročitajte <a href='{_linkGenerator.GetPathByAction("JoinGoogleMeetByComputer", "Guides", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>uputstvo za pristup putem računara</a>." +
                                       "<br />" +
                                       $"Za pristup seansi putem mobilnog telefona, pročitajte <a href='{_linkGenerator.GetPathByAction("JoinGoogleMeetByPhone", "Guides", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>uputstvo za pristup putem mobilnog telefona</a>." +
                                       "<br />" +
                                       "<br />" +
                                       $"{(email.BookedSession.Type == 1 ? "<br />Pošto je seansa grupna, neophodno je da link za pristup seansi koji budete primili, prosledite svim članovima koji pohadjaju seansu." : "")}" +
                                       "<br />" +
                                       "<br />" +
                                       $"Nakon završetka seanse, molimo da ocenite seansu klikom <a target='_blank' href='{sessionRatingUrl}'>ovde</a>." +
                                       "<br />" +
                                       "<br />" +
                                       "<b><i>BITNO: Kada ocenite seansu, platforma garantuje da ni u kom trenutku neće Vaše lične podatke prikazati javno. Biće prikazani samo ocena, komentar i datum ocenjivanja.</i></b>" +
                                       "<br />" +
                                       "<br />" +
                                       "Sve najbolje," +
                                       "<br />" +
                                       $"{_configuration.GetSection("Application:AppName")?.Value} tim";
                }

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendClientBookedSessionEmailAsync.log"));
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendClientBookedSessionEmailAsync");
                return;
            }
        }

        public async Task SendTherapistConsultationBookedEmailAsync(BookedConsultationEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = "Klijent je zakazao besplatne konsultacije sa Vama",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.BookedConsultation.TherapistEmail));

                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za BookedConsultation, treba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {

                }

                var contactMethodId = email.BookedConsultation.ContactMethodId.ToLower();

                builder.HtmlBody = $"Zdravo {email.BookedConsultation.TherapistFirstName}," +
                                       "<br />" +
                                       "<br />" +
                                       $"Klijent {email.BookedConsultation.ClientFirstName} {email.BookedConsultation.ClientLastName} je zakazao {email.BookedConsultation.ContactMethodName} besplatne konsultacije sa Vama." +
                                       $"<br />" +
                                       "<br />" +
                                       $"Molimo da {email.BookedConsultation.ContactMethodName} link za pristup konsultacijama dodate najkasnije 4 sata pre početka. " +
                                       $"Ovo možete uraditi odlaskom na <a href='{_linkGenerator.GetPathByAction("Details", "Consultations", new { Area = "Therapist", bookingId = email.BookedConsultation.Id }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>stranicu sa detaljima o konsultacijama</a>." +
                                       "<br />" +
                                       "<br />" +
                                       $"Datum početka: {_dateHelper.ConvertDateTimeFromUtcToLocalString(email.BookedConsultation.StartDateTime)}h" +
                                       "<br />" +
                                       $"Datum završetka: {_dateHelper.ConvertDateTimeFromUtcToLocalString(email.BookedConsultation.EndDateTime)}h" +
                                       "<br />" +
                                       $"Kontakt metoda: {email.BookedConsultation.ContactMethodName}" +
                                       "<br />" +
                                       $"Imejl klijenta: {email.BookedConsultation.ClientEmail}" +
                                       "<br />" +
                                       $"Telefon klijenta: {(!string.IsNullOrWhiteSpace(email.BookedConsultation.ClientPhoneNumber) ? email.BookedConsultation.ClientPhoneNumber : "Broj telefona nije dodat")}" +
                                       "<br />" +
                                       "<br />" +
                                       $"Za dodatne informacije o zakazanim konsultacijama, posetite <a href='{_linkGenerator.GetPathByAction("Details", "Consultations", new { Area = "Therapist", bookingId = email.BookedConsultation.Id }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>stranicu sa detaljima o konsultacijama</a>." +
                                       $"<br />" +
                                       $"<br />" +
                                       $"Ukoliko Vam je potrebna pomoć ili imate pitanja, <a href='{_linkGenerator.GetPathByAction("Support", "Account", new { Area = "Therapist" }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>kontaktirajte podršku za terapeute</a>." +
                                       "<br />" +
                                       "<br />" +
                                       "Sve najbolje," +
                                       "<br />" +
                                       $"{_configuration.GetSection("Application:AppName")?.Value} tim";

                mime.Body = builder.ToMessageBody();
                using var smtp = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendTherapistConsultationBookedEmailAsync.log"));
                smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp.SendAsync(mime);
                smtp.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendTherapistConsultationBookedEmailAsync");
                return;
            }
        }

        public async Task SendTherapistSessionBookedEmailAsync(BookedSessionEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = "Klijent je zakazao seansu sa Vama",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.BookedSession.TherapistEmail));

                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za BookedSession, treba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {
                    var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, @"..\Framework\Emails\EmailTemplates\BookedSessionTemplate\booked-session.html"));

                    if (new FileInfo(filePath).Exists)
                    {
                        var mailText = await new StreamReader(filePath).ReadToEndAsync();
                        mailText = mailText.Replace("{{ClientFullName}}", email.BookedSession.ClientFirstName + " " + email.BookedSession.ClientLastName);
                        mailText = mailText.Replace("{{ClientEmail}}", email.BookedSession.ClientEmail);
                        mailText = mailText.Replace("{{ClientPhoneNumber}}", email.BookedSession.ClientPhoneNumber);

                        mailText = mailText.Replace("{{TherapistFullName}}", email.BookedSession.TherapistFirstName + " " + email.BookedSession.TherapistLastName);
                        mailText = mailText.Replace("{{TherapistEmail}}", email.BookedSession.TherapistEmail);
                        mailText = mailText.Replace("{{TherapistPhoneNumber}}", email.BookedSession.TherapistPhoneNumber);

                        mailText = mailText.Replace("{{Type}}", email.BookedSession.Type == 0 ? "Individual session" : "Group session");
                        mailText = mailText.Replace("{{Price}}", email.BookedSession.Price.ToString());
                        mailText = mailText.Replace("{{StartDate}}", _dateHelper.ConvertDateTimeFromUtcToLocalString(email.BookedSession.StartTime));
                        mailText = mailText.Replace("{{EndDate}}", _dateHelper.ConvertDateTimeFromUtcToLocalString(email.BookedSession.EndTime));
                        mailText = mailText.Replace("{{ContactMethodName}}", email.BookedSession.ContactMethodName);
                        //mailText = mailText.Replace("{{ContactInfo}}", email.BookedSession.ContactInfo);

                        builder.HtmlBody = mailText;
                        mime.Body = builder.ToMessageBody();
                        using var smtp1 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendTherapistSessionBookedEmailAsync.log"));
                        smtp1.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                        smtp1.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                        await smtp1.SendAsync(mime);
                        smtp1.Disconnect(true);

                        return;
                    }
                }

                var contactMethodId = email.BookedSession.ContactMethodId.ToLower();

                if (contactMethodId == ContactMethodsProvider.InPerson.Id)
                {
                    builder.HtmlBody = $"Zdravo {email.BookedSession.TherapistFirstName}," +
                                       "<br />" +
                                       "<br />" +
                                       $"Klijent {email.BookedSession.ClientFirstName} {email.BookedSession.ClientLastName} je zakazao ličnu seansu sa Vama." +
                                       $"<br />" +
                                       "Tip: " + (email.BookedSession.Type == 0 ? "Individualna seansa" : "Grupna seansa") +
                                       "<br />" +
                                       $"Datum početka: {_dateHelper.ConvertDateTimeFromUtcToLocalString(email.BookedSession.StartTime)}h" +
                                       "<br />" +
                                       $"Datum završetka: {_dateHelper.ConvertDateTimeFromUtcToLocalString(email.BookedSession.EndTime)}h" +
                                       "<br />" +
                                       $"Kontakt metoda: {email.BookedSession.ContactMethodName}" +
                                       "<br />" +
                                       $"Lokacija održavanja: {email.BookedSession.TherapistStreet} {email.BookedSession.TherapistHouseNumber}, {email.BookedSession.TherapistCity} {email.BookedSession.TherapistPostalCode}, {email.BookedSession.TherapistCountry}" +
                                       "<br />" +
                                       $"{(email.BookedSession.Type == 0 ? "Klijent" : "Grupa klijenata")} će biti na Vašoj adresi 5 minuta pre početka seanse." +
                                       "<br />" +
                                       $"Imejl klijenta: {email.BookedSession.ClientEmail}" +
                                       "<br />" +
                                       $"Telefon klijenta: {(!string.IsNullOrWhiteSpace(email.BookedSession.ClientPhoneNumber) ? email.BookedSession.ClientPhoneNumber : "Broj telefona nije dodat")}" +
                                       "<br />" +
                                       "<br />" +
                                       $"Za dodatne informacije o zakazanoj seansi, posetite <a href='{_linkGenerator.GetPathByAction("Details", "Sessions", new { Area = "Therapist", bookingId = email.BookedSession.Id }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>stranicu sa detaljima o seansi</a>." +
                                       "<br />" +
                                       "<br />" +
                                      $"Ukoliko Vam je potrebna pomoć ili imate pitanja, <a href='{_linkGenerator.GetPathByAction("Support", "Account", new { Area = "Therapist" }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>kontaktirajte podršku za terapeute</a>." +
                                       "<br />" +
                                       "<br />" +
                                       "Sve najbolje," +
                                       "<br />" +
                                       $"{_configuration.GetSection("Application:AppName")?.Value} tim";
                }
                else
                {
                    builder.HtmlBody = $"Zdravo {email.BookedSession.TherapistFirstName}," +
                                       "<br />" +
                                       "<br />" +
                                       $"Klijent {email.BookedSession.ClientFirstName} {email.BookedSession.ClientLastName} je zakazao {email.BookedSession.ContactMethodName} seansu sa Vama." +
                                       $"<br />" +
                                       "<br />" +
                                       $"Molimo da {email.BookedSession.ContactMethodName} link za pristup seansi dodate najkasnije 4 sata pre početka. " +
                                       $"Ovo možete uraditi odlaskom na <a href='{_linkGenerator.GetPathByAction("Details", "Sessions", new { Area = "Therapist", bookingId = email.BookedSession.Id }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>stranicu sa detaljima o seansi</a>." +
                                       "<br />" +
                                       "<br />" +
                                       "Tip: " + (email.BookedSession.Type == 0 ? "Individualna seansa" : "Grupna seansa") +
                                       "<br />" +
                                       $"Datum početka: {_dateHelper.ConvertDateTimeFromUtcToLocalString(email.BookedSession.StartTime)}h" +
                                       "<br />" +
                                       $"Datum završetka: {_dateHelper.ConvertDateTimeFromUtcToLocalString(email.BookedSession.EndTime)}h" +
                                       "<br />" +
                                       $"Kontakt metoda: {email.BookedSession.ContactMethodName}" +
                                       "<br />" +
                                       $"Imejl klijenta: {email.BookedSession.ClientEmail}" +
                                       "<br />" +
                                       $"Telefon klijenta: {(!string.IsNullOrWhiteSpace(email.BookedSession.ClientPhoneNumber) ? email.BookedSession.ClientPhoneNumber : "Broj telefona nije dodat")}" +
                                       "<br />" +
                                       "<br />" +
                                       $"Za dodatne informacije o zakazanoj seansi, posetite <a href='{_linkGenerator.GetPathByAction("Details", "Sessions", new { Area = "Therapist", bookingId = email.BookedSession.Id }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>stranicu sa detaljima o seansi</a>." +
                                       $"<br />" +
                                       $"<br />" +
                                       $"Ukoliko Vam je potrebna pomoć ili imate pitanja, <a href='{_linkGenerator.GetPathByAction("Support", "Account", new { Area = "Therapist" }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>kontaktirajte podršku za terapeute</a>." +
                                       "<br />" +
                                       "<br />" +
                                       "Sve najbolje," +
                                       "<br />" +
                                       $"{_configuration.GetSection("Application:AppName")?.Value} tim";
                }

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendTherapistSessionBookedEmailAsync.log"));
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendTherapistSessionBookedEmailAsync");
                return;
            }
        }

        public async Task SendConsultationInviteLinkEmailAsync(ConsultationInviteLinkEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = $"Link za pristup {email.ContactMethodName} konsultacijama",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.ClientEmail));

                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za ConsultationInviteLink, treba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {

                }

                var localizedDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(email.StartDateTime);

                builder.HtmlBody = $"Zdravo {email.ClientFirstName}," +
                                   "<br />" +
                                   "<br />" +
                                   $"Vaše besplatne {email.ContactMethodName} konsultacije sa psihoterapeutom {email.TherapistFirstName} {email.TherapistLastName} počinju datuma {_dateHelper.DateStringFromDateTime(localizedDateTime)}, u {_dateHelper.TimeStringFromDateTime(localizedDateTime)}h." +
                                   "<br />" +
                                   "<br />" +
                                   $"Na vreme početka, kliknite na ovaj linka za pristup {email.ContactMethodName} konsultacijama: " +
                                   "<br />" +
                                   $"{email.InviteLink}" +
                                   "<br />" +
                                   "<br />" +
                                   $"Za pristup konsultacijama putem računara, pročitajte <a href='{_linkGenerator.GetPathByAction("JoinGoogleMeetByComputer", "Guides", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>uputstvo za pristup putem računara</a>." +
                                   "<br />" +
                                   $"Za pristup konsultacijama putem mobilnog telefona, pročitajte <a href='{_linkGenerator.GetPathByAction("JoinGoogleMeetByPhone", "Guides", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>uputstvo za pristup putem mobilnog telefona</a>." +
                                   "<br />" +
                                   "<br />" +
                                   $"Imejl terapeuta: {email.TherapistEmail}" +
                                   "<br />" +
                                   $"Telefon terapeuta: {email.TherapistPhoneNumber}" +
                                   "<br />" +
                                   $"Ukoliko imate bilo kakav problem prilikom pristupa konsultacijama, kontaktirajte psihoterapeuta {email.TherapistFirstName} putem imejla ili telefona." +
                                   "<br />" +
                                   "<br />" +
                                   $"Kopiju linka za pristup, kao i ostale detalje o konsultacijama možete pročitati odlaskom na <a href='{_linkGenerator.GetPathByAction("ConsultationDetails", "Consultations", new { bookingId = email.BookingId }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>stranicu sa detaljima o konsultacijama</a>." +
                                   "<br />" +
                                   "<br />" +
                                   $"Ukoliko Vam je potrebna pomoć ili imate pitanja, <a href='{_linkGenerator.GetPathByAction("CustomerSupport", "Home", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>kontaktirajte korisničku podršku</a>." +
                                   "<br />" +
                                   "<br />" +
                                   "Sve najbolje," +
                                   "<br />" +
                                   $"{_configuration.GetSection("Application:AppName")?.Value} tim";

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendConsultationInviteLinkEmailAsync.log"));
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendConsultationInviteLinkEmailAsync");
                return;
            }
        }

        public async Task SendSessionInviteLinkEmailAsync(SessionInviteLinkEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = $"Link za pristup {email.ContactMethodName} seansi",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.ClientEmail));

                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za SessionInviteLink, treba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {
                    var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, @"..\Framework\Emails\EmailTemplates\SessionInviteLinkTemplate\session-invite-link.html"));

                    if (new FileInfo(filePath).Exists)
                    {
                        var mailText = await new StreamReader(filePath).ReadToEndAsync();
                        mailText = mailText.Replace("{{ClientFirstName}}", email.ClientFirstName);

                        mailText = mailText.Replace("{{TherapistFullName}}", email.TherapistFirstName + " " + email.TherapistLastName);
                        mailText = mailText.Replace("{{TherapistEmail}}", email.TherapistEmail);
                        mailText = mailText.Replace("{{TherapistPhoneNumber}}", email.TherapistPhoneNumber);

                        var earlier = _dateHelper.ConvertDateTimeFromUtcToLocal(email.StartTime.Subtract(new TimeSpan(4, 0, 0)));

                        mailText = mailText.Replace("{{earlier}}", _dateHelper.ConvertDateTimeFromUtcToLocalString(earlier));
                        mailText = mailText.Replace("{{StartDate}}", _dateHelper.ConvertDateTimeFromUtcToLocalString(email.StartTime));
                        mailText = mailText.Replace("{{ContactMethodName}}", email.ContactMethodName);
                        mailText = mailText.Replace("{{InviteLink}}", email.InviteLink);

                        builder.HtmlBody = mailText;
                        mime.Body = builder.ToMessageBody();
                        using var smtp1 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendSessionInviteLinkEmailAsync.log"));
                        smtp1.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                        smtp1.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                        await smtp1.SendAsync(mime);
                        smtp1.Disconnect(true);

                        return;
                    }
                }

                var localizedDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(email.StartTime);

                var sessionDetailsUrl = email.ClientId.IsAnonymousOrUnauthorized() ?
                       _linkGenerator.GetPathByAction("SessionDetailsUnauthorized", "Sessions", new { bookingId = email.BookingId }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..] :
                       _linkGenerator.GetPathByAction("SessionDetails", "Sessions", new { bookingId = email.BookingId }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..];


                builder.HtmlBody = $"Zdravo {email.ClientFirstName}," +
                                   "<br />" +
                                   "<br />" +
                                   $"Vaša {email.ContactMethodName} seansa sa psihoterapeutom {email.TherapistFirstName} {email.TherapistLastName} počinje datuma {_dateHelper.DateStringFromDateTime(localizedDateTime)}, u {_dateHelper.TimeStringFromDateTime(localizedDateTime)}h." +
                                   "<br />" +
                                   "<br />" +
                                   $"Na vreme početka, kliknite na ovaj linka za pristup {email.ContactMethodName} seansi: " +
                                   "<br />" +
                                   $"{email.InviteLink}" +
                                   $"{(email.Type == 1 ? "<br /><br />Pošto je seansa grupna, neophodno je da link za pristup seansi prosledite svim članovima koji pohađaju seansu." : "")}" +
                                   "<br />" +
                                   "<br />" +
                                   $"Za pristup seansi putem računara, pročitajte <a href='{_linkGenerator.GetPathByAction("JoinGoogleMeetByComputer", "Guides", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>uputstvo za pristup seansi putem računara</a>." +
                                   "<br />" +
                                   $"Za pristup seansi putem mobilnog telefona, pročitajte <a href='{_linkGenerator.GetPathByAction("JoinGoogleMeetByPhone", "Guides", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>uputstvo za pristup seansi putem mobilnog telefona</a>." +
                                   "<br />" +
                                   "<br />" +
                                   $"Imejl terapeuta: {email.TherapistEmail}" +
                                   "<br />" +
                                   $"Telefon terapeuta: {email.TherapistPhoneNumber}" +
                                   "<br />" +
                                   $"Ukoliko imate bilo kakav problem prilikom pristupa seansi, kontaktirajte psihoterapeuta {email.TherapistFirstName} putem imejla ili telefona." +
                                   "<br />" +
                                   "<br />" +
                                   $"Kopiju linka za pristup, kao i ostale detalje o seansi možete pročitati odlaskom na <a href='{sessionDetailsUrl}'>stranicu sa detaljima o seansi</a>." +
                                   "<br />" +
                                   "<br />" +
                                   $"Ukoliko Vam je potrebna pomoć ili imate pitanja, <a href='{_linkGenerator.GetPathByAction("CustomerSupport", "Home", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>kontaktirajte korisničku podršku</a>." +
                                   "<br />" +
                                   "<br />" +
                                   "Sve najbolje," +
                                   "<br />" +
                                   $"{_configuration.GetSection("Application:AppName")?.Value} tim";

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendSessionInviteLinkEmailAsync.log"));
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendSessionInviteLinkEmailAsync");
                return;
            }
        }

        public async Task SendPayPalPaymentSuccessfulEmailAsync(PayPalPaymentSuccessfulEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = $"You received {email.SecondaryCurrencyCode} {email.SecondaryCurrencyAmount} Web Credit to your account.",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.ToEmail));

                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za PayPalPaymentSuccessful, treba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {
                    var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, @"..\Framework\Emails\EmailTemplates\PayPalPaymentTemplates\paypal-payment-successful.html"));

                    if (new FileInfo(filePath).Exists)
                    {

                    }
                }

                builder.HtmlBody = $"Zdravo," +
                                   "<br />" +
                                   "<br />" +
                                   "OVDE IDE PORUKA" +
                                   "<br />" +
                                   $"Ukoliko Vam je potrebna pomoć ili imate pitanja, <a href='{_linkGenerator.GetPathByAction("CustomerSupport", "Home", null, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..]}'>kontaktirajte korisničku podršku</a>." +
                                   "<br />" +
                                   "<br />" +
                                   "Sve najbolje," +
                                   "<br />" +
                                   $"{_configuration.GetSection("Application:AppName")?.Value} tim";

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendPayPalPaymentSuccessfulEmailAsync.log"));
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);

                return;
            }
            catch (Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendPayPalPaymentSuccessfulEmailAsync");
                return;
            }
        }

        public async Task SendUnratedSessionsReminderEmailAsync(UnratedSessionReminderEmail email, bool includeTemplateIfExists)
        {
            try
            {
                var mime = new MimeMessage
                {
                    Subject = $"Molimo ocenite pohađane seanse",
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };

                mime.To.Add(MailboxAddress.Parse(email.ToEmail));

                var builder = new BodyBuilder();

                // !!! jos uvek ne postoji template za SessionInviteLink, treba tek da napravim u buduce.
                if (includeTemplateIfExists)
                {
                    var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, @"..\Framework\Emails\EmailTemplates\UnratedSessionReminderTemplate\unrated-session-reminder.html"));

                    if (new FileInfo(filePath).Exists)
                    {
                        var mailText = await new StreamReader(filePath).ReadToEndAsync();

                        builder.HtmlBody = mailText;
                        mime.Body = builder.ToMessageBody();
                        using var smtp1 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendUnratedSessionsReminderEmailAsync.log"));
                        smtp1.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                        smtp1.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                        await smtp1.SendAsync(mime);
                        smtp1.Disconnect(true);

                        return;
                    }
                }

                var allBookedSessions = string.Empty;

                foreach(var bookedSession in email.BookedSessionsToRemindAbout)
                {
                    var sessionRatingUrl = bookedSession.ClientId.IsAnonymousOrUnauthorized() ?
                       _linkGenerator.GetPathByAction("UnratedSessionsUnauthorized", "Sessions", new { email = bookedSession.ClientEmail, bookingId = bookedSession.Id }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..] :
                       _linkGenerator.GetPathByAction("UnratedSessions", "Sessions", new { bookingId = bookedSession.Id }, new PathString($"/{_configuration.GetSection("Application:AppDomain")?.Value}"))[1..];

                    allBookedSessions += $"<li style='margin-bottom:1rem !important;'><a target='_blank' href='{sessionRatingUrl}'>{bookedSession.TherapistFirstName} {bookedSession.TherapistLastName}, {(bookedSession.Type == 0 ? "Individualna seansa" : "Grupna seansa")}, {_dateHelper.ConvertDateTimeFromUtcToLocalString(bookedSession.StartTime)}h</a></li>";
                }

                builder.HtmlBody = "Zdravo," +
                                   "<br />" +
                                   "<br />" +
                                   "Kako bi se pomoglo narednim klijentima da donesu informisanu odluku prilikom odabira terapeuta, zamolili bismo Vas da ocenite pohađane seanse:" +
                                   "<ol style='margin-bottom:0 !important;'>" +
                                   allBookedSessions +
                                   "</ol>" +
                                   "<b><i>BITNO: Kada ocenite seansu, platforma garantuje da ni u kom trenutku neće Vaše lične podatke prikazati javno. Biće prikazani samo ocena, komentar i datum ocenjivanja.</i></b>" +
                                   "<br />" +
                                   "<br />" +
                                   "Sve najbolje," +
                                   "<br />" +
                                   $"{_configuration.GetSection("Application:AppName")?.Value} tim";

                mime.Body = builder.ToMessageBody();
                using var smtp2 = new SmtpClient(new ProtocolLogger("Logs/EmailLogs/SendUnratedSessionsReminderEmailAsync.log"));
                smtp2.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp2.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp2.SendAsync(mime);
                smtp2.Disconnect(true);

                return;
            }
            catch(Exception e)
            {
                await _systemError.SaveErrorAsync(e, PROJECT, CLASS, "SendUnratedSessionsReminderEmailAsync");
                return;
            }
        }
    }
}