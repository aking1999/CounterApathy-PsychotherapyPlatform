using Database.Models;
using Database.RepositoryImplementations;
using Framework.Helpers;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
//using WebApplication9.Helpers;

namespace WebApplication9.Areas.Admin.Controllers
{
    [Area(areaName: "Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : BaseController
    {
        public DashboardController(INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager) : base(notificationRepository, userManager)
        {
        }

        [HttpGet]
        public async Task<IActionResult> PendingRatings(string filter = null)
        {
            ShowToastOnThisPageIfSet();

            var pendingRatings = getPendingRatings(filter);

            var list_pendingRatingVm = new List<PendingRatingViewModel>();

            foreach (var pendingRating in pendingRatings)
            {
                var tempItem = new PendingRatingViewModel();
                tempItem.Map(pendingRating);

                var client = await _userManager.FindByIdAsync(pendingRating.ClientId);

                if (client == null)
                    client = new CustomClient();

                var therapistAsp = WebApplication9.Helpers.Helper.GetTherapistAspNetUser(pendingRating.TherapistId, _context);

                if (therapistAsp == null)
                    therapistAsp = new AspNetUsers()
                    {
                        FirstName = "-",
                        Email = "-"
                    };

                tempItem.TherapistFirstName = therapistAsp.FirstName;
                tempItem.TherapistLastName = therapistAsp.LastName;
                tempItem.TherapistEmail = therapistAsp.Email;

                var sessions = _context.Sessions.Find(s => s.Id == pendingRating.SessionId).ToList();

                if(sessions.Count > 0)
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

        [HttpPost]
        //[ValidateAntiForgeryToken] !!! ovo moram popraviti da radi kad stavim AntiForgetyToken
        public async Task<IActionResult> ApproveRating(string ratingId, string therapistId, string ClientId, string sessionId, string bookedSessionId)
        {
            var pendingRating = _context.PendingRatings.Find(r => r.Id == ratingId &&
                                                                  r.TherapistId == therapistId &&
                                                                  r.ClientId == ClientId &&
                                                                  r.SessionId == sessionId &&
                                                                  r.BookedSessionId == bookedSessionId &&
                                                                  (r.Refused == 0 || r.Refused == null)).ToList().FirstOrDefault();

            if(pendingRating == default)
            {
                _session.SetToast("Rating not found", null, "error");

                return Json(new
                {
                    //!!! ovo treba da stoji true jer ne radim nista razlicito u Ajax-u kad je true ili false
                    success = true
                });
            }

            var rating = new Ratings();
            rating.Map(pendingRating);

            var client = await _userManager.FindByIdAsync(ClientId);
            
            if(client != null)
            {
                if (!string.IsNullOrEmpty(client.FirstName))
                    rating.ClientFirstName = client.FirstName[0] + ".";
                
                if (!string.IsNullOrEmpty(client.LastName))
                    rating.ClientLastName = client.LastName[0] + ".";
            }

            var admin = await _userManager.GetUserAsync(User);
            rating.AdminIdWhoApproved = admin.Id;

            rating.ApprovalDate = DateTime.Now;

            _context.PendingRatings.Delete(pendingRating);
            _context.Ratings.Insert(rating);

            await _context.SaveAsync();

            _session.SetToast("Rating approved", null, "success");

            return Json(new
            {
                success = true
            });
        }

        [HttpPost]
        //[ValidateAntiForgeryToken] !!! ovo moram popraviti da radi kad stavim AntiForgetyToken
        public async Task<IActionResult> RefuseRating(string ratingId, string therapistId, string ClientId, string sessionId, string bookedSessionId)
        {
            var pendingRating = _context.PendingRatings.Find(r => r.Id == ratingId &&
                                                      r.TherapistId == therapistId &&
                                                      r.ClientId == ClientId &&
                                                      r.SessionId == sessionId &&
                                                      r.BookedSessionId == bookedSessionId &&
                                                      (r.Refused == 0 || r.Refused == null)).ToList().FirstOrDefault();

            if (pendingRating == default)
            {
                _session.SetToast("Rating not found", null, "error");

                return Json(new
                {
                    //!!! ovo treba da stoji true jer ne radim nista razlicito u Ajax-u kad je true ili false
                    success = true
                });
            }

            pendingRating.Refused = 1;
            _context.PendingRatings.Update(pendingRating);
            await _context.SaveAsync();

            _session.SetToast("Rating refused", null, "warning");

            return Json(new
            {
                success = true
            });
        }

        [HttpGet]
        public IActionResult Withdrawals(string filter = null, string predicate = null)
        {
            ShowToastOnThisPageIfSet();

            var listOfWithdrawalVm = new List<WithdrawalViewModel>();

            foreach(var withdrawal in getWithdrawals(filter, predicate))
            {
                var withdrawalVm = new WithdrawalViewModel();
                withdrawalVm.Map(withdrawal);

                var therapistUserList = _context.AspNetUsers.Find(u => u.TherapistAccountId == withdrawalVm.TherapistId).ToList();

                var therapistUser = new AspNetUsers();

                if (therapistUserList.Count > 0)
                    therapistUser = therapistUserList[0];

                if(therapistUser != null)
                {
                    withdrawalVm.ProfilePhoto = therapistUser.ProfilePhoto;
                    withdrawalVm.TherapistAccountFirstName = therapistUser.FirstName;
                    withdrawalVm.TherapistAccountLastName = therapistUser.LastName;
                }

                listOfWithdrawalVm.Add(withdrawalVm);
            }

            return View(listOfWithdrawalVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptWithdrawal(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new
                {
                    success = false,
                    title = "An error occured!",
                    body = "Withdrawal ID is null.",
                    severity = "error"
                });

            var withdrawal = _context.Withdrawals.GetById(id);

            if(withdrawal == null)
                return Json(new
                {
                    success = false,
                    title = "An error occured!",
                    body = "Withdrawal request not found.",
                    severity = "error"
                });

            if(withdrawal.Status != 0)
                return Json(new
                {
                    success = false,
                    title = "An error occured!",
                    body = "Withdrawal request already accepted.",
                    severity = "error"
                });

            withdrawal.Status = 1;
            withdrawal.AcceptDateTime = DateTime.Now;

            _context.Withdrawals.Update(withdrawal);
            await _context.SaveAsync();

            return Json(new
            {
                success = true,
                title = "Congratulations!",
                body = "Withdrawal request accepted successfully.",
                severity = "success"
            });
        }

        [NonAction]
        private List<PendingRatings> getPendingRatings(string filter = null)
        {
            var pendingRatings = _context.PendingRatings.Find(r => r.Refused == 0 || r.Refused == null).ToList();

            if (!string.IsNullOrEmpty(filter) && filter.ToLower() == "show all")
                pendingRatings = _context.PendingRatings.GetAll().ToList();

            if (!string.IsNullOrEmpty(filter) && filter.ToLower() == "refused")
                pendingRatings = _context.PendingRatings.Find(r => r.Refused == 1).ToList();

            return pendingRatings;
        }

        [NonAction]
        private List<Withdrawals> getWithdrawals(string filter, string predicate)
        {
            var listOfWithdrawals = new List<Withdrawals>();

            listOfWithdrawals = _context.Withdrawals.GetAll()
                                                    .OrderByDescending(w => w.RequestDateTime)
                                                    .ToList();

            if (string.IsNullOrEmpty(filter) || string.IsNullOrWhiteSpace(filter) ||
                string.IsNullOrEmpty(predicate) || string.IsNullOrWhiteSpace(predicate))
                return listOfWithdrawals;
            else
            {
                if (filter.ToLower() == "date")
                {
                    if (predicate.ToLower() == "today")
                    {
                        return listOfWithdrawals.Where(w => w.RequestDateTime == DateTime.Today).ToList();
                    }
                    else if (predicate.ToLower() == "this week")
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
                else if (filter.ToLower() == "status")
                {
                    if (predicate.ToLower() == "requested")
                    {
                        return listOfWithdrawals.Where(w => w.Status == 0).ToList();
                    }
                    else if (predicate.ToLower() == "completed")
                    {
                        return listOfWithdrawals.Where(w => w.Status == 1).ToList();
                    }
                }
                return listOfWithdrawals;
            }
        }
    }
}
