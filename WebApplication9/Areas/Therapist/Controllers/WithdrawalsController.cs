using Database.Models;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Areas.Therapist.ViewModels;
using WebApplication9.Base;
using WebApplication9.Helpers;
using Framework.Helpers;
using Framework.Notifications;

namespace WebApplication9.Areas.Therapist.Controllers
{
    [Area(areaName: "Therapist")]
    [Authorize(Roles = "Therapist")]
    public class WithdrawalsController : BaseController
    {
        public WithdrawalsController(INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager) : base(notificationRepository, userManager)
        {

        }

        [HttpGet]
        public async Task<IActionResult> Withdraw()
        {
            if (therapistHasCompletedAccountSetup() == 1)
            {
                var user = await _userManager.GetUserAsync(User);

                var therapist = _context.Therapists.GetById(user.TherapistAccountId);

                if(therapist.Earnings <= 0)
                {
                    _session.SetToast("You do not have earnings to withdraw", null, "info");
                    return RedirectToAction("Profile", "Account", new { Area = "Therapist" });
                }

                var withdrawVm = new WithdrawViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };

                withdrawVm.Map(therapist);

                return View(withdrawVm);
            }
            else if (therapistHasCompletedAccountSetup() == 0)
            {
                return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
            }
            else
            {
                _session.SetToast("An error occured", null, "error");
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(WithdrawViewModel withdrawVm)
        {
            if (therapistHasCompletedAccountSetup() == 1)
            {
                var user = await _userManager.GetUserAsync(User);

                var therapist = _context.Therapists.GetById(user.TherapistAccountId);

                if (therapist.Earnings <= 0)
                {
                    _session.SetToast("You do not have earnings to withdraw", null, "info");
                    return RedirectToAction("Profile", "Account", new { Area = "Therapist" });
                }

                if (ModelState.IsValid)
                {
                    var withdraw = new Withdrawals
                    {
                        Id = Guid.NewGuid().ToString(),
                        TherapistId = therapist.Id,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Status = 0,
                        Amount = therapist.Earnings,
                        RequestDateTime = DateTime.Now
                    };

                    withdraw.Map(withdrawVm);

                    therapist.Earnings = therapist.Earnings - withdraw.Amount;

                    _context.Therapists.Update(therapist);
                    _context.Withdrawals.Insert(withdraw);

                    await _context.SaveAsync();

                    ViewBag.success = true;
                    ViewBag.swalTitle = "Successfully requested a withdrawal of your earnings.";
                    ViewBag.swalBody = "Your earnings are safe. It usually takes 2-3 business days to complete your request.";
                    ViewBag.swalSeverity = "success";
                    ViewBag.redirectUrl = Url.Action("Details", "Withdrawals", new { Area = "Therapist" });
                }

                return View(withdrawVm);
            }
            else if (therapistHasCompletedAccountSetup() == 0)
            {
                return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
            }
            else
            {
                _session.SetToast("An error occured", null, "error");
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> All(string filter = null, string predicate = null)
        {
            if(therapistHasCompletedAccountSetup() == 1)
            {
                var user = await _userManager.GetUserAsync(User);

                //var therapistWithdrawals = _context.Withdrawals.Find(w => w.TherapistId == user.TherapistAccountId).ToList();

                var therapistWithdrawals = getWithdrawals(user.TherapistAccountId, filter, predicate);

                return View(therapistWithdrawals);
            }
            else if(therapistHasCompletedAccountSetup() == 0)
            {
                return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
            }
            else
            {
                _session.SetToast("An error occured", null, "error");
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
        }

        [NonAction]
        private int therapistHasCompletedAccountSetup()
        {
            var userAwaitable = _userManager.GetUserAsync(User);
            userAwaitable.Wait();
            var user = userAwaitable.Result;

            if (user == null)
                return -1;

            if (user.TherapistAccountId == null)
                return -1;

            var therapistUser = _context.Therapists.GetById(user.TherapistAccountId);

            if (therapistUser == null)
                return -1;

            var therapistContactMethods = _context.TherapistsContactMethods.Find(cm => cm.TherapistId == therapistUser.Id);

            if (therapistContactMethods == null || therapistContactMethods == default)
                therapistContactMethods = new List<TherapistsContactMethods>();


            if (therapistContactMethods.Count() > 0 &&
                !string.IsNullOrEmpty(therapistUser.About))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        [NonAction]
        private List<Withdrawals> getWithdrawals(string therapistId, string filter, string predicate)
        {
            if (string.IsNullOrEmpty(therapistId) || string.IsNullOrWhiteSpace(therapistId))
                return new List<Withdrawals>();

            var listOfWithdrawals = new List<Withdrawals>();

            listOfWithdrawals = _context.Withdrawals.Find(w => w.TherapistId == therapistId)
                                                    .OrderByDescending(w => w.RequestDateTime)
                                                    .ToList();

            if (string.IsNullOrEmpty(filter) || string.IsNullOrWhiteSpace(filter) ||
                string.IsNullOrEmpty(predicate) || string.IsNullOrWhiteSpace(predicate))
                return listOfWithdrawals;
            else
            {
                if(filter.ToLower() == "date")
                {
                    if(predicate.ToLower() == "today")
                    {
                        return listOfWithdrawals.Where(w => w.RequestDateTime == DateTime.Today).ToList();
                    }
                    else if(predicate.ToLower() == "this week")
                    {
                        var startOfWeek = new DateTime().StartOfWeek(DayOfWeek.Monday);
                        var nextMonday = startOfWeek.AddDays(7);

                        return listOfWithdrawals.Where(w => startOfWeek <= w.RequestDateTime && w.RequestDateTime <= nextMonday).ToList();
                    }
                    else if (predicate.ToLower() == "this month")
                    {
                        var date = DateTime.Today;
                        var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                        return listOfWithdrawals.Where(w => firstDayOfMonth <= w.RequestDateTime && w.RequestDateTime <= lastDayOfMonth).ToList();
                    }
                    else if (predicate.ToLower() == "this year")
                    {
                        var startOfYear = new DateTime(DateTime.Today.Year, 1, 1);
                        var endOfYear = new DateTime(DateTime.Today.Year, 12, 31);

                        return listOfWithdrawals.Where(w => startOfYear <= w.RequestDateTime && w.RequestDateTime <= endOfYear).ToList();
                    }
                }
                else if(filter.ToLower() == "status")
                {
                    if(predicate.ToLower() == "requested")
                    {
                        return listOfWithdrawals.Where(w => w.Status == 0).ToList();
                    }
                    else if(predicate.ToLower() == "completed")
                    {
                        return listOfWithdrawals.Where(w => w.Status == 1).ToList();
                    }
                }
                return listOfWithdrawals;
            }
        }
    }
}
