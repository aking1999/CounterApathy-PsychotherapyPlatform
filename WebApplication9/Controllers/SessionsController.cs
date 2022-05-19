using Database.Models;
using Database.RepositoryImplementations;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Areas.Therapist.ViewModels;
using WebApplication9.Helpers;
using WebApplication9.ViewModels;
using WebApplication9.PartialViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication9.Base;
using Framework.Notifications;

namespace WebApplication9.Controllers
{
    [Authorize(Roles = "Client")]
    public class SessionsController : BaseController
    {
        private readonly IWebHostEnvironment _environment;

        public SessionsController(INotificationRepository notificationRepository,
            IWebHostEnvironment environment,
            UserManager<CustomClient> userManager) : base(notificationRepository, userManager)
        {
            _environment = environment;
        }

        [HttpGet]
        public IActionResult GetSessions()
        {
            //if (therapist == null)
            //    return RedirectToAction("All", "Therapists", new { Area = "" });

            //var sessions = _context.Sessions.Find(sess => sess.TherapistId == therapist).ToList();
            var sessions = _context.Sessions.GetAll();
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
                sessions = listSessionJson
            });
        }

        [HttpGet]
        public async Task<IActionResult> BookedSessions(string filter = null, string predicate = null)
        {
            ShowToastOnThisPageIfSet();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _session.SetToast("An error occured", "Please try again or contact the Customer Support.", "error");

                return RedirectToAction("Index", "Home");
            }

            var bookedSessions = getBookedSessions(user.Id, filter, predicate);

            var list_bookedSessionVm = await getApprovedRatingsOrPendingRatingsForAsync(bookedSessions);

            return View(list_bookedSessionVm);
        }

        [HttpGet]
        public async Task<IActionResult> SessionDetails(string bookingId)
        {
            if (string.IsNullOrEmpty(bookingId) || string.IsNullOrWhiteSpace(bookingId))
            {
                _session.SetToast("An error occured", null, "error");
                return RedirectToAction("BookedSessions");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _session.SetToast("An error occured", null, "error");
                return RedirectToAction("Index", "Home");
            }

            var bookedSessionList = _context.BookedSessions.Find(s => s.ClientId == user.Id && s.Id == bookingId).ToList();

            var bookedSessionVm = new BookedSessionViewModel();

            if (bookedSessionList != null)
            {
                if (bookedSessionList.Count > 0)
                {
                    bookedSessionVm.Map(bookedSessionList.ElementAt(0));

                    var therapist = _context.Therapists.GetById(bookedSessionVm.TherapistId);
                    var therapistUserList = _context.AspNetUsers.Find(usr => usr.TherapistAccountId == therapist.Id).ToList();
                    AspNetUsers therapistUser;

                    if (therapistUserList != null)
                    {
                        therapistUser = therapistUserList.ElementAt(0);

                        var therapistCustomClient = await _userManager.FindByIdAsync(therapistUser.Id);

                        if (therapistCustomClient != null)
                        {
                            bookedSessionVm.TherapistProfilePhoto = therapistCustomClient.ProfilePhoto;
                        }

                        var ratingList = _context.Ratings.Find(r => r.BookedSessionId == bookedSessionVm.BookingId).ToList();

                        if (ratingList.Count > 0)
                        {
                            var rating = ratingList.ElementAt(0);
                            bookedSessionVm.Rating.StarsRating = rating.Rating;
                        }
                        else
                        {
                            var pendingRatingList = _context.PendingRatings.Find(pr => pr.BookedSessionId == bookedSessionVm.BookingId).ToList();

                            if (pendingRatingList.Count > 0)
                            {
                                var pendingRating = pendingRatingList.ElementAt(0);
                                bookedSessionVm.Rating.StarsRating = pendingRating.Rating;
                            }
                        }
                    }

                    setContactMethodColorAndIcon(bookedSessionVm);
                }
                else
                {
                    _session.SetToast("An error occured", null, "error");
                    return RedirectToAction("BookedSessions");
                }
            }

            return View(bookedSessionVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateSession(BookedSessionViewModel bookedSessionVm)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return Json(new
                    {
                        success = false,
                        toastHeader = "An error occured",
                        toastBody = "",
                        toastSeverity = "error"
                    });

                if (sessionAlreadyRated(bookedSessionVm))
                {
                    return Json(new
                    {
                        success = false,
                        toastHeader = "This session is already rated",
                        toastBody = "",
                        toastSeverity = "warning"
                    });
                }

                //if the session is already RATED
                if (sessionRatingPending(bookedSessionVm))
                {
                    return Json(new
                    {
                        success = false,
                        toastHeader = "This session is already rated",
                        toastBody = "",
                        toastSeverity = "warning"
                    });
                }

                _context.PendingRatings.Insert(new PendingRatings
                {
                    Id = Guid.NewGuid().ToString(),
                    BookedSessionId = bookedSessionVm.BookingId,
                    SessionId = bookedSessionVm.SessionId,
                    TherapistId = bookedSessionVm.TherapistId,
                    ClientId = user.Id,
                    Rating = bookedSessionVm.Rating.StarsRating,
                    Comment = bookedSessionVm.Rating.Comment,
                    RatingDate = DateTime.Now
                });

                await _context.SaveAsync();

                return Json(new
                {
                    success = true
                });
            }
            else
            {
                var modelStateError = new List<ModelStateErrorPartialViewModel>();

                foreach (var modelStateKey in ViewData.ModelState.Keys)
                {
                    var value = ViewData.ModelState[modelStateKey];
                    foreach (var error in value.Errors)
                    {
                        modelStateError.Add(new ModelStateErrorPartialViewModel
                        {
                            Key = modelStateKey,
                            Value = error.ErrorMessage
                        });
                    }
                }

                return Json(new
                {
                    success = false,
                    errors = modelStateError
                });
            }
        }

        [NonAction]
        private List<BookedSessions> getBookedSessions(string userId, string filter, string predicate)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(userId))
                return new List<BookedSessions>();

            var listOfBookedSessions = new List<BookedSessions>();

            listOfBookedSessions = _context.BookedSessions.Find(s => s.ClientId == userId)
                                                    .OrderByDescending(s => s.SessionDate.Date + s.StartTime.TimeOfDay)
                                                    .ToList();

            if (string.IsNullOrEmpty(filter) || string.IsNullOrWhiteSpace(filter) ||
                string.IsNullOrEmpty(predicate) || string.IsNullOrWhiteSpace(predicate))
                return listOfBookedSessions;

            else
            {
                if (filter.ToLower() == "date")
                {
                    if (predicate.ToLower() == "today")
                    {
                        listOfBookedSessions = _context.BookedSessions.Find(s => s.SessionDate.Date == DateTime.Today)
                                                                .OrderByDescending(s => s.SessionDate.Date + s.StartTime.TimeOfDay)
                                                                .ToList();
                    }
                    //else if (predicate.ToLower() == "this week")
                    //{
                    //    listOfBookedSessions = _context.BookedSessions.Find(s => s.ClientId == userId && s.SessionDate.Date >= DateTime.AddDays(1 - (int)DateTime.Today.DayOfWeek)
                    //                                            .OrderByDescending(s => s.SessionDate.Date + s.StartTime.TimeOfDay)
                    //                                            .ToList();
                    //}
                    //else if (predicate.ToLower() == "this month")
                    //{

                    //}
                    //else if (predicate.ToLower() == "this year")
                    //{

                    //}
                }
                //else if (filter.ToLower() == "priority")
                //{
                //    if (predicate.ToLower() == "highest")
                //    {
                //        listOfBookedSessions = _context.BookedSessions.Find(s => ((s.SessionDate.Date + s.StartTime.TimeOfDay) - DateTime.Now).TotalHours < 3)
                //                                                .OrderByDescending(s => s.SessionDate.Date + s.StartTime.TimeOfDay)
                //                                                .ToList();
                //    }
                //    else if (predicate.ToLower() == "high")
                //    {

                //    }
                //    else if (predicate.ToLower() == "normal")
                //    {

                //    }
                //    else if(predicate.ToLower() == "low")
                //    {

                //    }
                //}
                //else return listOfBookedSessions;
            }

            return listOfBookedSessions;
        }

        [NonAction]
        private async Task<List<BookedSessionViewModel>> getApprovedRatingsOrPendingRatingsForAsync(List<BookedSessions> bookedSessions)
        {
            var list_bookedSessionVm = new List<BookedSessionViewModel>();

            foreach (var bookedSession in bookedSessions)
            {
                var tempItem = new BookedSessionViewModel();
                tempItem.Map(bookedSession);

                var ratingList = _context.Ratings.Find(r => r.BookedSessionId == tempItem.BookingId).ToList();

                if(ratingList.Count > 0)
                {
                    var rating = ratingList.ElementAt(0);
                    tempItem.Rating.StarsRating = rating.Rating;
                }
                else
                {
                    var pendingRatingList = _context.PendingRatings.Find(pr => pr.BookedSessionId == tempItem.BookingId).ToList();

                    if (pendingRatingList.Count > 0)
                    {
                        var pendingRating = pendingRatingList.ElementAt(0);
                        tempItem.Rating.StarsRating = pendingRating.Rating;
                    }
                }
                await mapTherapistProfilePhotoForAsync(tempItem);
                setContactMethodColorAndIcon(tempItem);
                list_bookedSessionVm.Add(tempItem);
            }

            return list_bookedSessionVm;
        }

        //[NonAction]
        //private async Task<List<Pending>>

        [NonAction]
        private async Task mapTherapistProfilePhotoForAsync(BookedSessionViewModel bookedSessionVm)
        {
            var therapistAspUserList = _context.AspNetUsers.Find(u => u.TherapistAccountId == bookedSessionVm.TherapistId).ToList();

            if (therapistAspUserList.Count > 0)
            {
                var therapistAspUser = therapistAspUserList.ElementAt(0);
                bookedSessionVm.TherapistProfilePhoto = await HtmlHelperExtensionMethods.RenderUserProfilePhotoOrDefaultPhoto(null, therapistAspUser.Id, _userManager);
            }
        }

        [NonAction]
        private void setContactMethodColorAndIcon(BookedSessionViewModel bookedSessionVm)
        {
            if (bookedSessionVm != null)
            {
                if (!string.IsNullOrEmpty(bookedSessionVm.ContactMethodName))
                {
                    var contactMethodList = _context.ContactMethods.Find(cm => cm.Name.ToLower() == bookedSessionVm.ContactMethodName.ToLower()).ToList();

                    if (contactMethodList != null)
                    {
                        if (contactMethodList.Count > 0)
                        {
                            var contactMethod = contactMethodList.ElementAt(0);

                            bookedSessionVm.ContactMethodColor = contactMethod.Color;
                            bookedSessionVm.ContactMethodIcon = contactMethod.Icon;
                        }
                    }
                }
            }
        }

        [NonAction]
        private bool sessionRatingPending(BookedSessionViewModel bookedSessionVm)
        {
            //var bookingIdUnique = _context.PendingRatings.Find(pnd => pnd.BookedSessionId == bookedSessionVm.BookingId).ToList();
            //var sessionIdUnique = _context.PendingRatings.Find(pnd => pnd.SessionId == bookedSessionVm.SessionId).ToList();

            //if (bookingIdUnique.Count == 0 && sessionIdUnique.Count == 0)
            //    return true;

            //return false;

            var pendingRating = _context.PendingRatings.Find(pr => pr.BookedSessionId == bookedSessionVm.BookingId &&
                                                                   pr.SessionId == bookedSessionVm.SessionId).ToList();

            if (pendingRating.Count > 0)
                return true;

            return false;
        }

        [NonAction]
        private bool sessionAlreadyRated(BookedSessionViewModel bookedSessionVm)
        {
            var bookingIdUnique = _context.Ratings.Find(r => r.BookedSessionId == bookedSessionVm.BookingId).ToList();
            var sessionIdUnique = _context.Ratings.Find(r => r.SessionId == bookedSessionVm.SessionId).ToList();

            if (bookingIdUnique.Count != 0 && sessionIdUnique.Count != 0)
                return true;

            return false;
        }
    }
}
