using Database.Models;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Areas.Admin.Helpers;
using WebApplication9.Areas.Admin.ViewModels;
using WebApplication9.Base;
using Framework.Notifications;
using Framework.Emails;
using Framework.Emails.EmailTypes;
using WebApplication9.Interfaces;
using WebApplication9.Implementations;
using Framework.Interfaces;
using Framework.Implementations;
using Framework.Models;
using Microsoft.AspNetCore.Http;
using Framework.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using DataTransferObjects.ViewModels.Admin;
using Framework.Providers;
using System.Transactions;

namespace WebApplication9.Areas.Admin.Controllers
{
    [Area(areaName: "Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : BaseController
    {
        private readonly IFileRepository _files;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ISessionsFunctionsProvider _sessionsFunctions;
        private readonly IWithdrawalsFunctionsProvider _withdrawalsFunctions;
        private readonly ITherapistApplicationsFunctionsProvider _applicationsFunctions;

        public DashboardController(IConfiguration configuration,
            IWebHostEnvironment environment,
            IErrorLogger error,
            IMailService mailService,
            IDateTimeHelper dateHelper,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager) : base(error, mailService, dateHelper, contextAccessor, notificationRepository, userManager, signInManager)
        {
            _configuration = configuration;
            _environment = environment;
            _files = new FileRepository();
            _sessionsFunctions = new SessionsFunctionsProvider();
            _withdrawalsFunctions = new WithdrawalsFunctionsProvider();
            _applicationsFunctions = new TherapistApplicationsFunctionsProvider();
        }

        [HttpGet("/admin/komandna-tabla")]
        public async Task<IActionResult> Dashboard()
        {
            var therapistFee = _configuration.GetSection("TherapistFee").Get<TherapistFee>();

            var bookedSessions = _context.BookedSessions.ReadOnlyGetAll();
            var bookedSessionsPricesSum = bookedSessions.Sum(s => s.Price);

            var ratingIsPending = false;
            if (await _context.PendingRatings.ReadOnlyAnyAsync(r => r.Refused == 0))
                ratingIsPending = true;

            var withdrawalIsPending = false;
            if (await _context.Withdrawals.ReadOnlyAnyAsync(w => w.Status == 0))
                withdrawalIsPending = true;

            var applicationIsPending = false;
            if (await _context.TherapistApplications.ReadOnlyAnyAsync(app => app.Accepted == 0))
                applicationIsPending = true;

            var today = DateTime.UtcNow.Date;
            var thisWeekStart = today.AddDays(-(int)(today.DayOfWeek - DayOfWeek.Monday));
            var thisWeekEnd = thisWeekStart.AddDays(7);
            //var lastWeekStart = thisWeekStart.AddDays(-7);
            //var lastWeekEnd = thisWeekStart.AddSeconds(-1);
            //var thisMonthStart = today.AddDays(1 - today.Day);
            //var thisMonthEnd = thisMonthStart.AddMonths(1).AddSeconds(-1);
            //var lastMonthStart = thisMonthStart.AddMonths(-1);
            //var lastMonthEnd = thisMonthStart.AddSeconds(-1);

            var thisWeekBookedSessions = bookedSessions.Where(s => thisWeekStart < s.BookingDate && s.BookingDate < thisWeekEnd);
            var thisWeekBookedSessionsSum = thisWeekBookedSessions.Sum(s => s.Price);

            var allClients = await _userManager.GetUsersInRoleAsync(UserRoles.Client);
            var allTherapists = _context.Therapists.ReadOnlyGetAll();

            return View(new AdminDashboardViewModel
            {
                TotalEarnings = bookedSessionsPricesSum,
                TotalExpenses = bookedSessionsPricesSum * (1 - (therapistFee.PercentageAmount / 100)),
                TotalProfit = bookedSessionsPricesSum * (therapistFee.PercentageAmount / 100),
                EarningsDuringPeriod = thisWeekBookedSessionsSum,
                ExpensesDuringPeriod = thisWeekBookedSessionsSum * (1 - (therapistFee.PercentageAmount / 100)),
                ProfitDuringPeriod = thisWeekBookedSessionsSum * (therapistFee.PercentageAmount / 100),
                SessionsCount = _context.Sessions.CountEntities(),
                BookedSessionsCount = _context.Sessions.ReadOnlyFind(s => s.Booked == 1).Count(),
                RatingsCount = _context.Ratings.CountEntities(),
                RatingIsPending = ratingIsPending,
                PendingRatingCount = ratingIsPending ? _context.PendingRatings.ReadOnlyFind(r => r.Refused == 0).Count() : 0,
                WithdrawalsCount = _context.Withdrawals.CountEntities(),
                WithdrawalIsPending = withdrawalIsPending,
                PendingWithdrawalCount = withdrawalIsPending ? _context.Withdrawals.ReadOnlyFind(w => w.Status == 0).Count() : 0,
                ApplicationsCount = _context.TherapistApplications.CountEntities(),
                ApplicationIsPending = applicationIsPending,
                PendingApplicationCount = applicationIsPending ? _context.TherapistApplications.ReadOnlyFind(app => app.Accepted == 0).Count() : 0,
                TotalUnusedClientWebCredit = allClients.Any() ? allClients.Sum(user => user.WebCredit.GetValueOrDefault()) : 0,
                ClientsWebCreditLogsCount = _context.WebCreditLogs.CountEntities(),
                TotalUnusedTherapistsEarnings = allTherapists.Any() ? allTherapists.Sum(thr => thr.Earnings) : 0,
                TherapistsEarningsLogsCount = _context.TherapistEarningsLogs.CountEntities()
            });
        }

        [HttpGet("/admin/komandna-tabla/seanse/zakazane-seanse/{filter?}/{predicate?}")]
        public async Task<IActionResult> BookedSessions(string filter = null, string predicate = null)
        {
            try
            {
                var ss = new AdminBookedSessionViewModel();
                //return View(_sessionsFunctions.GetBookedSessions(filter, predicate);
                return View();
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/admin/komandna-tabla/aplikacije/{predicate?}")]
        public async Task<IActionResult> Applications(string predicate)
        {
            try
            {
                ShowToastOnThisPageIfSet();
                return View(_applicationsFunctions.GetApplications(predicate));
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/admin/komandna-tabla/aplikacije/detalji/{userId}")]
        public async Task<IActionResult> ApplicationDetails(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Applications");

                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    _session.SetToast("User not found", null, "info");
                    return RedirectToAction("Applications");
                }

                var app = _context.TherapistApplications.ReadOnlyFind(app => app.UserId == user.Id).SingleOrDefault();

                if (app == default)
                {
                    await HandleErrorAsync($"Application for user with ID '{userId}' not found or the user has multiple applications.");
                    _session.SetToast("User application not found or the user has multiple applications", null, "info");
                    return RedirectToAction("Applications");
                }

                return View(new ApplicationDetailsViewModel
                {
                    UserId = userId,
                    FirstName = app.FirstName,
                    LastName = app.LastName,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = app.PhoneNumber,
                    WebCredit = user.WebCredit != null ? user.WebCredit.ToString() : "0",
                    YearOfBirth = app.YearOfBirth != null ? app.YearOfBirth.ToString() : "0",
                    //AmountDue = userThatApplied.AmountDue != null ? userThatApplied.AmountDue.ToString() : "0",
                    TherapistApplicationId = app.Id,
                    ApplicationDate = app.ApplicationDate,
                    UnderSupervision = app.UnderSupervision.GetValueOrDefault(),
                    Accepted = app.Accepted != null ? app.Accepted.Value : 0,
                    Street = app.Street,
                    City = app.City,
                    Country = app.Country,
                    PostalCode = app.PostalCode,
                    Gender = app.Gender,
                    HouseNumber = app.HouseNumber,
                    University = app.University,
                    PastCompanies = app.PastCompanies,
                    ProfilePhoto = _applicationsFunctions.GetApplicationPhotoPath(userId, app.Id),
                    PsychotherapyTechniques = _applicationsFunctions.GetTherapistApplicationPsychotherapyTechniques(userId),
                    Specialities = _applicationsFunctions.GetTherapistApplicationSpecialties(userId)
                });
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AcceptApplication(string userId)
        {
            try
            {
                if (!(await _applicationsFunctions.AcceptApplicationAsync(userId)))
                {
                    await HandleErrorJsonAsync($"Unable to accept application for user with ID '{userId}'.");
                    return Json(new
                    {
                        success = false,
                        title = "An error occurred"
                    });
                }

                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return Json(new
                    {
                        success = false,
                        title = "User not found",
                        body = ""
                    });

                await _notificationRepository.SendAsync(new Notifications
                {
                    Id = Helper.GenerateNumbersId(),
                    SenderUserId = "System",
                    ReceiverUserId = userId,
                    Title = "Čestitamo! Vaša aplikacija za dobijanje naloga psihoterapeuta je pregledana i prihvaćena. U imejlu koji smo Vam upravo poslali nalazi se uputstvo za podešavanje Vašeg novog terapeutskog naloga.",
                    Body = null,
                    Severity = "success",
                    Read = false,
                    SendingDateTime = DateTime.UtcNow,
                    Icon = "fal fa-user-check",
                    Important = false
                });

                await _mailService.SendTherapistApplicationAcceptedEmailAsync(new TherapistApplicationStatusEmail
                {
                    ToEmail = user.Email,
                    FirstName = user.FirstName
                }, includeTemplateIfExists: true);

                return Json(new
                {
                    success = true,
                    title = "Congratulations!",
                    body = "Application accepted successfully."
                });
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);
                return Json(new
                {
                    success = false,
                    title = "An error occurred"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RejectApplication(string userId)
        {
            try
            {
                if (!(await _applicationsFunctions.RejectApplicationAsync(userId)))
                {
                    await HandleErrorJsonAsync($"Unable to accept application for user with ID {userId}.");
                    return Json(new
                    {
                        success = false,
                        title = "An error occurred"
                    });
                }

                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return Json(new
                    {
                        success = false,
                        title = "User not found",
                        body = ""
                    });

                await _notificationRepository.SendAsync(new Notifications
                {
                    Id = Helper.GenerateNumbersId(),
                    SenderUserId = "System",
                    ReceiverUserId = userId,
                    Title = "Vaša aplikacija za dobijanje naloga psihoterapeuta je pregledana i odbijena.",
                    Body = null,
                    Severity = "danger",
                    Read = false,
                    SendingDateTime = DateTime.UtcNow,
                    Icon = "fal fa-user-times",
                    Important = false
                });

                await _mailService.SendTherapistApplicationRejectedEmailAsync(new TherapistApplicationStatusEmail
                {
                    ToEmail = user.Email,
                    FirstName = user.FirstName
                }, includeTemplateIfExists: true);

                return Json(new
                {
                    success = true,
                    title = "Congratulations!",
                    body = "Application rejected successfully.",
                });
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);
                return Json(new
                {
                    success = false,
                    title = "An error occurred"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PendingRatings(string filter = null)
        {
            try
            {
                ShowToastOnThisPageIfSet();

                var pendingRatings = await GetPendingRatings(filter);

                var list_pendingRatingVm = new List<PendingRatingViewModel>();

                foreach (var pendingRating in pendingRatings)
                {
                    var tempItem = new PendingRatingViewModel();
                    tempItem.Map(pendingRating);

                    var client = await _userManager.FindByIdAsync(pendingRating.ClientId);

                    if (client == null)
                        client = new CustomClient();

                    var therapistUser = await _userManager.FindByTherapistAccountIdAsync(pendingRating.TherapistId);

                    if (therapistUser == null)
                        therapistUser = new CustomClient()
                        {
                            FirstName = "-",
                            Email = "-"
                        };

                    tempItem.TherapistFirstName = therapistUser.FirstName;
                    tempItem.TherapistLastName = therapistUser.LastName;
                    tempItem.TherapistEmail = therapistUser.Email;

                    var sessions = _context.Sessions.Find(s => s.Id == pendingRating.SessionId).ToList();

                    if (sessions.Count > 0)
                    {
                        var session = sessions.First();
                        tempItem.SessionDate = session.StartDateTime.Date.ToString("dd/MM/yyyy");
                        tempItem.SessionStartTime = session.StartDateTime.TimeOfDay.ToString();
                        tempItem.SessionEndTime = session.EndDateTime.TimeOfDay.ToString();
                    }

                    var ratingsOfTherapist = _context.Ratings.Find(r => r.TherapistId == pendingRating.TherapistId).ToList();
                    if (ratingsOfTherapist.Count > 0)
                        tempItem.TherapistAverageStarsRating = ratingsOfTherapist.Average(r => r.Rating);

                    tempItem.ClientEmail = client.Email;

                    list_pendingRatingVm.Add(tempItem);
                }

                return View(list_pendingRatingVm);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken] !!! ovo moram popraviti da radi kad stavim AntiForgetyToken
        public async Task<IActionResult> ApproveRating(string ratingId, string therapistId, string ClientId, string sessionId, string bookedSessionId)
        {
            try
            {
                var pendingRating = _context.PendingRatings.Find(r => r.Id == ratingId &&
                                                                      r.TherapistId == therapistId &&
                                                                      r.ClientId == ClientId &&
                                                                      r.SessionId == sessionId &&
                                                                      r.BookedSessionId == bookedSessionId &&
                                                                      (r.Refused == 0 || r.Refused == null)).SingleOrDefault();

                if (pendingRating == default)
                {
                    _session.SetToast("Rating not found", null, "info");

                    return Json(new
                    {
                        //!!! ovo treba da stoji true jer ne radim nista razlicito u Ajax-u kad je true ili false
                        success = true,
                        redirectUrl = Url.Action("PendingRatings", "Dashboard", new { Area = "Admin" })
                    });
                }

                var rating = new Ratings();
                rating.Map(pendingRating);

                var client = await _userManager.FindByIdAsync(ClientId);

                if (client != null)
                {
                    if (!string.IsNullOrWhiteSpace(client.FirstName))
                        rating.ClientFirstName = client.FirstName[0] + ".";

                    if (!string.IsNullOrWhiteSpace(client.LastName))
                        rating.ClientLastName = client.LastName[0] + ".";
                }

                var adminId = _userManager.GetUserId(User);

                if (string.IsNullOrWhiteSpace(adminId)) throw new GeneralException("User ID is null.", signOutUser: true);

                rating.AdminIdWhoApproved = adminId;
                rating.ApprovalDate = DateTime.UtcNow;

                _context.PendingRatings.Delete(pendingRating);
                _context.Ratings.Insert(rating);

                await _context.SaveAsync();

                _session.SetToast("Rating approved successfully", null, "success");

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("PendingRatings", "Dashboard", new { Area = "Admin" })
                });
            }
            catch (Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken] !!! ovo moram popraviti da radi kad stavim AntiForgetyToken
        public async Task<IActionResult> RefuseRating(string ratingId, string therapistId, string ClientId, string sessionId, string bookedSessionId)
        {
            try
            {
                var pendingRating = _context.PendingRatings.Find(r => r.Id == ratingId &&
                                                          r.TherapistId == therapistId &&
                                                          r.ClientId == ClientId &&
                                                          r.SessionId == sessionId &&
                                                          r.BookedSessionId == bookedSessionId &&
                                                          (r.Refused == 0 || r.Refused == null)).SingleOrDefault();

                if (pendingRating == default)
                {
                    _session.SetToast("Rating not found", null, "info");

                    return Json(new
                    {
                        success = true,
                        redirectUrl = Url.Action("PendingRatings", "Dashboard", new { Area = "Admin" })
                    });
                }

                pendingRating.Refused = 1;
                _context.PendingRatings.Update(pendingRating);
                await _context.SaveAsync();

                _session.SetToast("Rating refused", null, "warning");

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("PendingRatings", "Dashboard", new { Area = "Admin" })
                });
            }
            catch (Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Withdrawals(string filter = null, string predicate = null)
        {
            try
            {
                ShowToastOnThisPageIfSet();

                var WithdrawalVms = new List<WithdrawalViewModel>();

                foreach (var withdrawal in _withdrawalsFunctions.GetWithdrawals(filter, predicate))
                {
                    var withdrawalVm = new WithdrawalViewModel();
                    withdrawalVm.Map(withdrawal);


                    var therapistUser = new CustomClient
                    {
                        FirstName = withdrawal.FirstName,
                        LastName = withdrawal.LastName
                    };

                    if (!string.IsNullOrWhiteSpace(withdrawalVm.TherapistId))
                        therapistUser = await _userManager.FindByTherapistAccountIdAsync(withdrawalVm.TherapistId);

                    if (therapistUser != null)
                    {
                        withdrawalVm.ProfilePhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, therapistUser.Id);
                        withdrawalVm.TherapistAccountFirstName = therapistUser.FirstName;
                        withdrawalVm.TherapistAccountLastName = therapistUser.LastName;
                    }

                    WithdrawalVms.Add(withdrawalVm);
                }

                return View(WithdrawalVms);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptWithdrawal(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return Json(new
                    {
                        success = false,
                        title = "Withdrawal ID is null",
                        body = "",
                        severity = "error"
                    });

                var withdrawal = _context.Withdrawals.GetById(id);

                var therapistUser = await _userManager.FindByTherapistAccountIdAsync(withdrawal.TherapistId);

                if (therapistUser == null)
                    return Json(new
                    {
                        success = false,
                        title = "Therapist account doesn't exist",
                        body = "",
                        severity = "error"
                    });

                if (withdrawal == null)
                    return Json(new
                    {
                        success = false,
                        title = "Withdrawal request not found",
                        body = "",
                        severity = "error"
                    });

                if (withdrawal.Status != 0)
                    return Json(new
                    {
                        success = false,
                        title = "Withdrawal request already accepted",
                        body = "",
                        severity = "error"
                    });

                withdrawal.Status = 1;
                withdrawal.AcceptDateTime = DateTime.UtcNow;

                await _context.SaveAsync();

                await _mailService.SendWithdrawalRequestAcceptedEmailAsync(new WithdrawalRequestAcceptedEmail
                {
                    ToEmail = therapistUser.Email,
                    FirstName = therapistUser.FirstName,
                    Amount = withdrawal.Amount.ToString()
                }, includeTemplateIfExists: true);

                await _notificationRepository.SendAsync(new Notifications
                {
                    Id = Helper.GenerateNumbersId(),
                    SenderUserId = "System",
                    ReceiverUserId = therapistUser.Id,
                    Title = "Vaš zahtev za isplatu RSD " + withdrawal.Amount + " je prihvaćen. Sredstva su putem pošte uplaćena na Vaš račun. " +
                    "Uglavnom traje od 1 do 3 radnih dana da uplata bude proknjižena.",
                    Body = null,
                    Severity = "success",
                    Read = false,
                    SendingDateTime = DateTime.UtcNow,
                    Icon = "fal fa-hand-holding-usd",
                    Important = false
                });

                return Json(new
                {
                    success = true,
                    title = "Congratulations!",
                    body = "Withdrawal request accepted successfully.",
                    severity = "success"
                });
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);
                return Json(new
                {
                    success = false,
                    title = "An error occurred",
                    body = "",
                    severity = "error"
                });
            }
        }

        [HttpGet("/admin/komandna-tabla/dodavanje-veb-kredit")]
        public async Task<IActionResult> AddWebCredit()
        {
            try
            {
                var viewModel = new AdminAddWebCreditPaymentTypesViewModel();

                foreach (var user in (await _userManager.GetUsersInRoleAsync(UserRoles.Client)).Select(s => new { s.Id, s.FirstName, s.LastName, s.Email, s.WebCredit }).ToList())
                {
                    viewModel.Clients.Add(new AdminAddWebCreditViewModel
                    {
                        UserId = user.Id,
                        ProfilePhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, user.Id),
                        FullName = user.FirstName + " " + user.LastName,
                        Email = user.Email,
                        WebCredit = user.WebCredit.HasValue ? user.WebCredit.Value.ToString() : "0"
                    });
                }

                foreach (var payment in new List<PaymentTypes>
                {
                    PaymentTypesProvider.PaymentCards,
                    PaymentTypesProvider.PayPal,
                    PaymentTypesProvider.BankTransfer,
                    PaymentTypesProvider.Posta,
                    PaymentTypesProvider.USDCoin,
                    PaymentTypesProvider.USDTether
                })
                {
                    viewModel.ToChooseFrom_PaymentTypes.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = $"{_files.GetPaymentTypeLogo(_environment, payment.Id)}|{payment.Name}|-",
                        Value = payment.Id
                    });
                };

                return View(viewModel);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/admin/komandna-tabla/dodavanje-veb-kredit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWebCredit([FromForm] AdminAddWebCreditPaymentTypesViewModel addWebCredit)
        {
            try
            {
                var userToAddCreditTo = await _userManager.FindByIdAsync(addWebCredit.UserId);

                if (userToAddCreditTo == null)
                    return Json(new
                    {
                        success = false,
                        title = "Klijent ne postoji",
                        body = "Izaberite drugog klijenta.",
                        icon = "info"
                    });

                if (!(await _userManager.IsInRoleAsync(userToAddCreditTo, UserRoles.Client)))
                    return Json(new
                    {
                        success = false,
                        title = "Izabrani korisnik nije klijent",
                        body = "Veb kredit se može dodavati samo klijentskim nalozima.",
                        icon = "info"
                    });

                if (!ModelState.IsValid)
                    return Json(new
                    {
                        success = false,
                        title = "Popunite sva obavezna polja",
                        body = "",
                        icon = "info"
                    });

                double amountBeforeAdding = userToAddCreditTo.WebCredit.HasValue ? userToAddCreditTo.WebCredit.Value : 0;

                using (var scope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        userToAddCreditTo.WebCredit = amountBeforeAdding + addWebCredit.Amount;
                        var updated = await _userManager.UpdateAsync(userToAddCreditTo);
                        if (!updated.Succeeded)
                        {
                            await HandleErrorJsonAsync(string.Join('|', updated.Errors.Select(e => e.Description)));
                            return Json(new
                            {
                                success = false,
                                title = "Greška",
                                body = "Molimo osvežite stranicu i pokušajte ponovo.",
                                icon = "error"
                            });
                        }

                        var selectedPaymentType = _context.PaymentTypes.GetById(addWebCredit.Chosen_PaymentTypeId);
                        if (selectedPaymentType == null)
                        {
                            await HandleErrorAsync($"Payment type with Id '{addWebCredit.Chosen_PaymentTypeId}' does not exist in the database.");
                            return Json(new
                            {
                                success = false,
                                title = "Izabrani način uplate Veb kredita ne postoji",
                                body = "Izaberite postojeći način uplate.",
                                icon = "error"
                            });
                        }

                        var transactionId = Helper.GenerateNumbersId();

                        _context.Transactions.Insert(new Transactions
                        {
                            Id = transactionId,
                            SenderId = PaymentTypesProvider.CounterApathyPayment.Id,
                            ReceiverId = userToAddCreditTo.Id,
                            DateTime = DateTime.UtcNow,
                            Amount = addWebCredit.Amount,
                            CurrencyCode = "RSD"
                        });

                        _context.WebCreditLogs.Insert(new WebCreditLogs
                        {
                            Id = Helper.GenerateNumbersId(),
                            UserId = userToAddCreditTo.Id,
                            Email = userToAddCreditTo.Email,
                            Amount = addWebCredit.Amount,
                            CurrentAmount = userToAddCreditTo.WebCredit.Value,
                            PaymentTypeId = selectedPaymentType.Id,
                            PaymentTypeName = selectedPaymentType.Name,
                            ExecutionDateTime = DateTime.UtcNow,
                            TransactionId = transactionId
                        });

                        await _context.SaveAsync();

                        scope.Complete();
                    }
                    catch(Exception)
                    {
                        scope.Dispose();
                        throw;
                    }
                }

                if (addWebCredit.Chosen_PaymentTypeId == PaymentTypesProvider.PayPal.Id)
                {
                    await _notificationRepository.SendAsync(new Notifications
                    {
                        Id = Helper.GenerateNumbersId(),
                        SenderUserId = SystemInformation.Name,
                        ReceiverUserId = userToAddCreditTo.Id,
                        Title = $"Na Vaš nalog je dodat Veb kredit u iznosu od RSD {addWebCredit.Amount}.",
                        Body = null,
                        Severity = "primary",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "fab fa-paypal",
                        Important = true
                    });
                }
                else
                {
                    await _notificationRepository.SendAsync(new Notifications
                    {
                        Id = Helper.GenerateNumbersId(),
                        SenderUserId = SystemInformation.Name,
                        ReceiverUserId = userToAddCreditTo.Id,
                        Title = $"Na Vaš nalog je dodat Veb kredit u iznosu od RSD {addWebCredit.Amount}.",
                        Body = null,
                        Severity = "success",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "fal fa-usd-circle",
                        Important = true
                    });
                }

                await _mailService.SendWebCreditAddedEmailAsync(new WebCreditAddedEmail
                {
                    ToEmail = userToAddCreditTo.Email,
                    FirstName = userToAddCreditTo.FirstName,
                    Amount = addWebCredit.Amount.ToString(),
                    CurrentAmount = userToAddCreditTo.WebCredit.Value.ToString()
                }, includeTemplateIfExists: true);

                return Json(new
                {
                    success = true,
                    amountBeforeAdding = amountBeforeAdding,
                    amountAdded = addWebCredit.Amount,
                    totalAmount = userToAddCreditTo.WebCredit.Value,
                    currencyCode = "RSD"
                });
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);

                return Json(new
                {
                    success = false,
                    title = "Greška",
                    body = "Molimo osvežite stranicu i pokušajte ponovo.",
                    icon = "error"
                });
            }
        }

        [HttpGet("/admin/komandna-tabla/transakcije-klijenata/{filter?}/{predicate?}")]
        public async Task<IActionResult> WebCreditLogs(string filter = null, string predicate = null)
        {
            var webCreditLogVms = new List<AdminWebCreditLogViewModel>();

            foreach (var log in (from log in _context.WebCreditLogs.ReadOnlyGetAll()
                                 join paymentType in _context.PaymentTypes.ReadOnlyGetAll()
                                 on log.PaymentTypeId equals paymentType.Id
                                 orderby log.ExecutionDateTime descending
                                 select new
                                 {
                                     log.Id,
                                     log.UserId,
                                     paymentType.Logo,
                                     paymentType.Name,
                                     log.Amount,
                                     log.CurrentAmount,
                                     log.ExecutionDateTime,
                                     log.TransactionId
                                 }).ToList())
            {
                var user = await _userManager.FindByIdAsync(log.UserId) ?? new CustomClient { FirstName = "Anonymous" };

                var transaction = new Transactions();
                var webCreditLogVm = new AdminWebCreditLogViewModel();

                if (!string.IsNullOrWhiteSpace(log.TransactionId))
                    transaction = _context.Transactions.GetById(log.TransactionId);
                else webCreditLogVm.TransactionIsMissing = true;

                if (!webCreditLogVm.TransactionIsMissing)
                {
                    if (transaction.SenderId == PaymentTypesProvider.CounterApathyPayment.Id)
                    {
                        webCreditLogVm.SenderIsUser = false;
                        webCreditLogVm.SenderPhoto = _files.GetPaymentTypeLogo(_environment, transaction.SenderId);
                        webCreditLogVm.ReceiverPhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, transaction.ReceiverId);
                    }
                    else
                    {
                        webCreditLogVm.SenderIsUser = true;
                        webCreditLogVm.SenderPhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, transaction.SenderId);
                        webCreditLogVm.ReceiverPhoto = _files.GetPaymentTypeLogo(_environment, transaction.ReceiverId);
                    }

                    if (transaction.ReceiverId == PaymentTypesProvider.CounterApathyPayment.Id)
                    {
                        webCreditLogVm.ReceiverIsUser = false;
                    }
                    else
                    {
                        webCreditLogVm.ReceiverIsUser = true;
                    }

                    webCreditLogVm.UserFullName = user.FirstName + " " + user.LastName;
                    webCreditLogVm.UserEmail = user.Email;
                    webCreditLogVm.UserId = log.UserId;
                }

                webCreditLogVm.WebCreditLogId = log.Id;
                webCreditLogVm.PaymentTypeLogo = log.Logo;
                webCreditLogVm.Amount = log.Amount;
                webCreditLogVm.CurrentAmount = log.CurrentAmount.ToString();
                webCreditLogVm.CurrencyCode = transaction.CurrencyCode;
                webCreditLogVm.ExecutionDateTime = _dateHelper.ConvertDateTimeFromUtcToLocalString(log.ExecutionDateTime.GetValueOrDefault());

                webCreditLogVms.Add(webCreditLogVm);
            }

            return View(webCreditLogVms);
        }

        [NonAction]
        private async Task<List<PendingRatings>> GetPendingRatings(string filter = null)
        {
            try
            {
                var pendingRatings = _context.PendingRatings.Find(r => r.Refused == 0 || r.Refused == null).ToList();

                if (!string.IsNullOrEmpty(filter) && filter.ToLower() == "show all")
                    pendingRatings = _context.PendingRatings.GetAll().ToList();

                if (!string.IsNullOrEmpty(filter) && filter.ToLower() == "refused")
                    pendingRatings = _context.PendingRatings.Find(r => r.Refused == 1).ToList();

                return pendingRatings;
            }
            catch (Exception e)
            {
                await HandleErrorAsync(e);
                return new List<PendingRatings>();
            }
        }
    }
}
