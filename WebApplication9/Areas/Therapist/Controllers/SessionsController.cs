using Database.Models;
using Database.RepositoryImplementations;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Areas.Therapist.ViewModels;
using System;
using System.Globalization;
using WebApplication9.Base;
using Framework.Notifications;

namespace WebApplication9.Areas.Therapist.Controllers
{
    [Area(areaName: "Therapist")]
    [Authorize(Roles = "Therapist")]
    public class SessionsController : BaseController
    {
        private readonly IWebHostEnvironment _environment;

        public SessionsController(INotificationRepository notificationRepository,
            IWebHostEnvironment environment,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager) : base(notificationRepository, userManager, signInManager)
        {
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            if (therapistHasCompletedAccountSetup() == 1)
            {
                if (_session.HasToast())
                {
                    ViewBag.toast = _session.GetToast();
                    _session.RemoveToastFromKeys();
                }

                return View();
            }
            else if (therapistHasCompletedAccountSetup() == 0)
            {
                return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
            }
            else
            {
                await _signInManager.SignOutAsync();

                _session.SetToast("Invalid account", "Please contact the Customer Support.", "error");
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetSessions()
        {
            if (therapistHasCompletedAccountSetup() == 1)
            {
                var user = await _userManager.GetUserAsync(User);

                var therapistAccount = _context.Therapists.GetById(user.TherapistAccountId);

                var sessions = _context.Sessions.Find(sess => sess.TherapistId == therapistAccount.Id).ToList();

                if (sessions == null || sessions == default)
                {
                    _session.SetToast("Error", "Please try again. If the error persists, contact the Customer Support.", "error");
                    return Json(new
                    {
                        success = false,
                        location = Url.Action("Index", "Home", new { Area = "" })
                    });
                }

                var listSessionJson = new List<SessionJsonViewModel>();

                foreach (var session in sessions)
                {
                    listSessionJson.Add(new SessionJsonViewModel
                    {
                        SessionId = session.Id,
                        Subject = session.Subject,
                        Description = session.Description,
                        Price = session.Price,
                        Type = session.Type,
                        StartDateTime = session.StartDateTime,
                        EndDateTime = session.EndDateTime,
                        Booked = session.Booked
                    });
                }

                return Json(new
                {
                    success = true,
                    sessions = listSessionJson
                });

            }
            else if (therapistHasCompletedAccountSetup() == 0)
            {
                //return Json(new
                //{
                //    success = false,
                //    location = Url.Action("AccountSetup", "Account", new { Area = "Therapist" })
                //});

                return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
            }
            else
            {
                await _signInManager.SignOutAsync();

                _session.SetToast("Invalid account", "Please contact the Customer Support.", "error");
                //return Json(new
                //{
                //    success = false,
                //    location = Url.Action("Index", "Home", new { Area = "" })
                //});
                return RedirectToAction("Index", "Home", new { Area = "" });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSessionTileClicked(AddSessionViewModel addSessionVm)
        {
            if (therapistHasCompletedAccountSetup() == 1)
            {
                if (ModelState.IsValid)
                {
                    var model = addSessionVm.TileClicked;
                    DateTime sessionDate;

                    if (DateTime.TryParseExact(model.StartEndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out sessionDate))
                    {
                        DateTime sessionStartTime;
                        DateTime sessionEndTime;

                        if (DateTime.TryParseExact(model.StartTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out sessionStartTime) &&
                            DateTime.TryParseExact(model.EndTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out sessionEndTime))
                        {
                            //ovde pitam ako je datum termina, odnosno sessionDate pre sutrasnjeg dana, da ne odobri, jer
                            //najkasnije mogu da se dodaju termini za sutrasnji dan, a ne da se doda termin za isti dan.
                            if (sessionDate.Date >= DateTime.Today.AddDays(1))
                            {
                                if (sessionStartTime.AddHours(1) <= sessionEndTime)
                                {
                                    var user = await _userManager.GetUserAsync(User);

                                    if (!sessionsAreOverlapping(user.TherapistAccountId, sessionDate, sessionStartTime, sessionEndTime))
                                    {
                                        if (model.Price.HasValue)
                                        {
                                            if (model.Type.HasValue && sessionTypeExists(model.Type.Value))
                                            {
                                                TimeSpan startTime;
                                                TimeSpan endTime;

                                                if (TimeSpan.TryParseExact(model.StartTime, @"hh\:mm", CultureInfo.InvariantCulture, out startTime) &&
                                                   TimeSpan.TryParseExact(model.EndTime, @"hh\:mm", CultureInfo.InvariantCulture, out endTime))
                                                {
                                                    Sessions session = new Sessions
                                                    {
                                                        Id = Guid.NewGuid().ToString(),
                                                        TherapistId = user.TherapistAccountId,
                                                        Subject = model.Subject,
                                                        Description = model.Description,
                                                        Price = model.Price.Value + 100,
                                                        Type = model.Type.Value,
                                                        StartDateTime = sessionDate.Add(startTime),
                                                        EndDateTime = sessionDate.Add(endTime),
                                                        Color = null,
                                                        Icon = null,
                                                        Booked = 0
                                                    };

                                                    _context.Sessions.Insert(session);
                                                    await _context.SaveAsync();

                                                    string sessionType = model.Type == 0 ? "Individual session" : "Group session";

                                                    TempData["success"] = true;
                                                    TempData["swalTitle"] = "Session added successfully!";
                                                    TempData["Review"] = "Review the added session";
                                                    TempData["Subject"] = model.Subject;
                                                    TempData["Description"] = model.Description;
                                                    TempData["Price"] = model.Price + " RSD (+ 100 RSD)";
                                                    TempData["Type"] = sessionType;
                                                    TempData["Date"] = sessionDate.ToString("dd/MM/yyyy");
                                                    TempData["StartTime"] = model.StartTime;
                                                    TempData["EndTime"] = model.EndTime;
                                                    TempData["SessionDuration"] = (endTime - startTime).ToString() + " hour(s)";
                                                    TempData["swalSeverity"] = "success";

                                                    return RedirectToAction("All");
                                                }

                                                ViewBag.tileClickFailed = true;
                                                ViewBag.showSwal = true;
                                                ViewBag.swalTitle = "Incorrect session start and/or end time.";
                                                ViewBag.swalBody = "Please check the session start and end time and try again. If the error persists, contact the Customer Support.";
                                                ViewBag.swalSeverity = "warning";

                                                return View("All", addSessionVm);
                                            }

                                            ViewBag.tileClickFailed = true;
                                            ViewBag.showSwal = true;
                                            ViewBag.swalTitle = "Please select an existing session type.";
                                            ViewBag.swalBody = "Selected session type does not exist.";
                                            ViewBag.swalSeverity = "warning";

                                            return View("All", addSessionVm);
                                        }

                                        ViewBag.tileClickFailed = true;
                                        ViewBag.showSwal = true;
                                        ViewBag.swalTitle = "Please enter the correct price.";
                                        ViewBag.swalBody = "Price must be between 500 RSD and 8000 RSD.";
                                        ViewBag.swalSeverity = "warning";

                                        return View("All", addSessionVm);
                                    }

                                    ViewBag.tileClickFailed = true;
                                    ViewBag.showSwal = true;
                                    ViewBag.swalTitle = "Sessions are overlapping!";
                                    ViewBag.swalBody = "You have already added a session which ends after this session has already started. Please change the start time of this session.";
                                    ViewBag.swalSeverity = "warning";

                                    return View("All", addSessionVm);
                                }

                                ViewBag.tileClickFailed = true;
                                ViewBag.showSwal = true;
                                ViewBag.swalTitle = "Attention!";
                                ViewBag.swalBody = "Session's duration must be at least 1 hour.";
                                ViewBag.swalSeverity = "warning";

                                return View("All", addSessionVm);
                            }

                            ViewBag.tileClickFailed = true;
                            ViewBag.showSwal = true;
                            ViewBag.swalTitle = "Attention!";
                            ViewBag.swalBody = "Session's date must be set to a date after today.";
                            ViewBag.swalSeverity = "warning";

                            return View("All", addSessionVm);
                        }

                        ViewBag.tileClickFailed = true;
                        ViewBag.showSwal = true;
                        ViewBag.swalTitle = "Incorrect session start and/or end time.";
                        ViewBag.swalBody = "Please check the session start and end time and try again. If the error persists, contact the Customer Support.";
                        ViewBag.swalSeverity = "warning";

                        return View("All", addSessionVm);
                    }

                    ViewBag.tileClickFailed = true;
                    ViewBag.showSwal = true;
                    ViewBag.swalTitle = "Attention!";
                    ViewBag.swalBody = "Incorrect entered date.";
                    ViewBag.swalSeverity = "warning";

                    return View("All", addSessionVm);
                }

                ViewBag.tileClickFailed = true;
                return View("All", addSessionVm);
            }
            else if (therapistHasCompletedAccountSetup() == 0)
            {
                return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
            }
            else
            {
                await _signInManager.SignOutAsync();

                _session.SetToast("Invalid account", "Please contact the Customer Support.", "error");
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSessionButtonClicked(AddSessionViewModel addSessionVm)
        {
            if (therapistHasCompletedAccountSetup() == 1)
            {
                if (ModelState.IsValid)
                {
                    var model = addSessionVm.ButtonClicked;
                    DateTime sessionDate;

                    if (DateTime.TryParseExact(model.StartEndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out sessionDate))
                    {
                        DateTime sessionStartTime;
                        DateTime sessionEndTime;

                        if (DateTime.TryParseExact(model.StartTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out sessionStartTime) &&
                            DateTime.TryParseExact(model.EndTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out sessionEndTime))
                        {
                            if (sessionDate.Date >= DateTime.Today.AddDays(1))
                            {
                                if (sessionStartTime.AddHours(1) <= sessionEndTime)
                                {
                                    var user = await _userManager.GetUserAsync(User);

                                    if (!sessionsAreOverlapping(user.TherapistAccountId, sessionDate, sessionStartTime, sessionEndTime))
                                    {
                                        if (model.Price.HasValue)
                                        {
                                            if (model.Type.HasValue && sessionTypeExists(model.Type.Value))
                                            {
                                                TimeSpan startTime;
                                                TimeSpan endTime;

                                                if (TimeSpan.TryParseExact(model.StartTime, @"hh\:mm", CultureInfo.InvariantCulture, out startTime) &&
                                                   TimeSpan.TryParseExact(model.EndTime, @"hh\:mm", CultureInfo.InvariantCulture, out endTime))
                                                {
                                                    Sessions session = new Sessions
                                                    {
                                                        Id = Guid.NewGuid().ToString(),
                                                        TherapistId = user.TherapistAccountId,
                                                        Subject = model.Subject,
                                                        Description = model.Description,
                                                        Price = model.Price.Value + 100,
                                                        Type = model.Type.Value,
                                                        StartDateTime = sessionDate.Add(startTime),
                                                        EndDateTime = sessionDate.Add(endTime),
                                                        Color = null,
                                                        Icon = null,
                                                        Booked = 0
                                                    };

                                                    _context.Sessions.Insert(session);
                                                    await _context.SaveAsync();

                                                    string sessionType = model.Type == 0 ? "Individual session" : "Group session";

                                                    TempData["success"] = true;
                                                    TempData["swalTitle"] = "Session added successfully!";
                                                    TempData["Review"] = "Review the added session";
                                                    TempData["Subject"] = model.Subject;
                                                    TempData["Description"] = model.Description;
                                                    TempData["Price"] = model.Price + " RSD (+ 100 RSD)";
                                                    TempData["Type"] = sessionType;
                                                    TempData["Date"] = sessionDate.ToString("dd/MM/yyyy");
                                                    TempData["StartTime"] = model.StartTime;
                                                    TempData["EndTime"] = model.EndTime;
                                                    TempData["SessionDuration"] = (endTime - startTime).ToString() + " hour(s)";
                                                    TempData["swalSeverity"] = "success";

                                                    return RedirectToAction("All");
                                                }

                                                ViewBag.buttonClickFailed = true;
                                                ViewBag.showSwal = true;
                                                ViewBag.swalTitle = "Incorrect session start and/or end time.";
                                                ViewBag.swalBody = "Please check the session start and end time and try again. If the error persists, contact the Customer Support.";
                                                ViewBag.swalSeverity = "warning";

                                                return View("All", addSessionVm);
                                            }

                                            ViewBag.buttonClickFailed = true;
                                            ViewBag.showSwal = true;
                                            ViewBag.swalTitle = "Please select an existing session type.";
                                            ViewBag.swalBody = "Selected session type does not exist.";
                                            ViewBag.swalSeverity = "warning";

                                            return View("All", addSessionVm);
                                        }

                                        ViewBag.buttonClickFailed = true;
                                        ViewBag.showSwal = true;
                                        ViewBag.swalTitle = "Please enter the correct price.";
                                        ViewBag.swalBody = "Price must be between 500 RSD and 8000 RSD.";
                                        ViewBag.swalSeverity = "warning";

                                        return View("All", addSessionVm);
                                    }

                                    ViewBag.buttonClickFailed = true;
                                    ViewBag.showSwal = true;
                                    ViewBag.swalTitle = "Sessions are overlapping!";
                                    ViewBag.swalBody = "You have already added a session which ends after this session has already started. Please change the start time of this session.";
                                    ViewBag.swalSeverity = "warning";

                                    return View("All", addSessionVm);
                                }

                                ViewBag.buttonClickFailed = true;
                                ViewBag.showSwal = true;
                                ViewBag.swalTitle = "Attention!";
                                ViewBag.swalBody = "Session's duration must be at least 1 hour.";
                                ViewBag.swalSeverity = "warning";

                                return View("All", addSessionVm);
                            }

                            ViewBag.buttonClickFailed = true;
                            ViewBag.showSwal = true;
                            ViewBag.swalTitle = "Attention!";
                            ViewBag.swalBody = "Session's date must be set to a date after today.";
                            ViewBag.swalSeverity = "warning";

                            return View("All", addSessionVm);
                        }

                        ViewBag.buttonClickFailed = true;
                        ViewBag.showSwal = true;
                        ViewBag.swalTitle = "Incorrect session start and/or end time.";
                        ViewBag.swalBody = "Please check the session start and end time and try again. If the error persists, contact the Customer Support.";
                        ViewBag.swalSeverity = "warning";

                        return View("All", addSessionVm);
                    }

                    ViewBag.buttonClickFailed = true;
                    ViewBag.showSwal = true;
                    ViewBag.swalTitle = "Attention!";
                    ViewBag.swalBody = "Incorrect entered date.";
                    ViewBag.swalSeverity = "warning";

                    return View("All", addSessionVm);
                }

                ViewBag.buttonClickFailed = true;
                return View("All", addSessionVm);
            }
            else if (therapistHasCompletedAccountSetup() == 0)
            {
                return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
            }
            else
            {
                await _signInManager.SignOutAsync();

                _session.SetToast("Invalid account", "Please contact the Customer Support.", "error");
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSession(string tracker)
        {
            if (therapistHasCompletedAccountSetup() == 1)
            {
                var user = await _userManager.GetUserAsync(User);

                var therapistUser = _context.Therapists.GetById(user.TherapistAccountId);

                var therapistSessionList = _context.Sessions.Find(sess => sess.TherapistId == therapistUser.Id).ToList();

                var hasTheSpecifiedSession = therapistSessionList.SingleOrDefault(sl => sl.Id == tracker);

                if (hasTheSpecifiedSession != null)
                {
                    if (hasTheSpecifiedSession.Booked == 0)
                    {
                        _context.Sessions.Delete(hasTheSpecifiedSession);
                        await _context.SaveAsync();

                        return Json(new
                        {
                            success = true,
                            header = "Session deleted successfully.",
                            body = "",
                            severity = "success"
                        });
                    }

                    _session.SetToast("Error", "Not allowed to delete booked sessions.", "error");
                    return Json(new
                    {
                        success = false,
                        location = Url.Action("All", "Sessions", new { Area = "Therapist" })
                    });
                }
                else
                {
                    _session.SetToast("Selected session not found.", "Please try again or contact the Customer Support.", "error");

                    return Json(new
                    {
                        success = false,
                        location = Url.Action("All", "Sessions", new { Area = "Therapist" })
                    });
                }
            }
            else if (therapistHasCompletedAccountSetup() == 0)
            {
                return Json(new
                {
                    success = false,
                    location = Url.Action("AccountSetup", "Account", new { Area = "Therapist" })
                });
            }
            else
            {
                await _signInManager.SignOutAsync();

                _session.SetToast("Invalid account", "Please contact the Customer Support.", "error");
                return Json(new
                {
                    success = false,
                    location = Url.Action("Index", "Home", new { Area = "" })
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string sessionId)
        {
            return Content("Ovde treba da stavim sessionId.");
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
        private bool sessionTypeExists(int sessionType)
        {
            if (sessionType == 0 || sessionType == 1)
                return true;

            return false;
        }

        [NonAction]
        private bool sessionsAreOverlapping(string therapistId, DateTime sessionDate, DateTime sessionStartTime, DateTime sessionEndTime)
        {
            var overlappingSessions = _context.Sessions.Find(
                                        sess => sess.TherapistId == therapistId &&
                                                (sess.StartDateTime.Date == sessionDate &&
                                                (sess.EndDateTime.TimeOfDay > sessionStartTime.TimeOfDay &&
                                                sess.StartDateTime.TimeOfDay < sessionEndTime.TimeOfDay))).ToList();

            if (overlappingSessions.Count == 0)
                return false;

            return true;
        }
    }
}
