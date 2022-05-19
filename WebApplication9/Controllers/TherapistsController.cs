using Database.Models;
using Database.RepositoryImplementations;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Areas.Therapist.ViewModels;
using WebApplication9.ViewModels;
using WebApplication9.PartialViewModels;
using WebApplication9.Helpers;
using WebApplication9.Base;
using Microsoft.AspNetCore.Authorization;
using Framework.Notifications;

namespace WebApplication9.Controllers
{
    public class TherapistsController : BaseController
    {
        public TherapistsController(INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager) : base(notificationRepository, userManager)
        {

        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            ShowToastOnThisPageIfSet();

            var therapists = _context.Therapists.GetAll().ToList();

            var listOfAllTherapists = new List<TherapistRatingsSessionsViewModel>();

            foreach (var therapist in therapists)
            {
                var therapistUserList = _context.AspNetUsers.Find(user => user.TherapistAccountId == therapist.Id).ToList();

                if (therapistUserList == null || therapistUserList.Count == 0)
                    continue;

                var therapistUser = therapistUserList.ElementAt(0);

                var therapistCustomClient = await _userManager.FindByIdAsync(therapistUser.Id);

                if (!await _userManager.IsInRoleAsync(therapistCustomClient, "Therapist"))
                    continue;

                var therapistRatingsSessionsVm = new TherapistRatingsSessionsViewModel()
                {
                    Id = therapist.Id,
                    ProfilePhoto = !string.IsNullOrEmpty(therapistUser.ProfilePhoto) ? @"/images/user-images/" + therapistUser.ProfilePhoto : @"/images/content-images/default-user-image.png",
                    FullName = therapistUser.FirstName + " " + therapistUser.LastName,
                    About = therapist.About,
                    Rating = getTherapistAverageRating(therapist.Id)
                };

                var therapistSpecialties = _context.TherapistsSpecialities.Find(spec => spec.TherapistId == therapistUser.TherapistAccountId).ToList();

                if (therapistSpecialties == null || therapistSpecialties.Count == 0)
                    therapistRatingsSessionsVm.Specialties.Add("-");
                else
                {
                    foreach (var specialties in therapistSpecialties)
                    {
                        var speciality = _context.Specialities.GetById(specialties.SpecialityId);

                        therapistRatingsSessionsVm.Specialties.Add(speciality.Name);
                    }
                }

                listOfAllTherapists.Add(therapistRatingsSessionsVm);
            }

            return View(listOfAllTherapists);
        }

        [HttpGet]
        public async Task<IActionResult> Profile(string therapist)
        {
            if (therapist == null)
                return RedirectToAction("All");

            var therapistUser = _context.Therapists.GetById(therapist);

            if (therapistUser == null)
            {
                _session.SetToast("Selected therapist does not exist", null, "error");
                return RedirectToAction("All");
            }

            var userAccountList = _context.AspNetUsers.Find(user => user.TherapistAccountId == therapistUser.Id).ToList();

            if (userAccountList == null || userAccountList.Count == 0)
            {
                _session.SetToast("Selected therapist does not exist", null, "error");
                return RedirectToAction("All");
            }

            var userAccount = userAccountList.ElementAt(0);

            var user = await _userManager.FindByIdAsync(userAccount.Id);

            if (user == null)
            {
                _session.SetToast("Selected therapist does not exist", null, "error");
                return RedirectToAction("All");
            }

            var therapistSpecialities = _context.TherapistsSpecialities.Find(spec => spec.TherapistId == therapistUser.Id).ToList();

            //!!! ovde sam stao, treba da dodam da ako nije korisnik logovan, da ne prikazuje podatke terapeuta poput adrese broja tel itd.
            var youMustBeLoggedIn = "You must be signed in.";

            var publicProfileVm = new TherapistPublicProfileViewModel()
            {
                Id = user.Id,
                TherapistAccountId = user.TherapistAccountId,
                Firstname = user.FirstName,
                Lastname = user.LastName,
                YearOfBirth = user.YearOfBirth.ToString(),
                Gender = therapistUser.Gender,
                Street = User.Identity.IsAuthenticated ? therapistUser.Street : youMustBeLoggedIn,
                HouseNumber = User.Identity.IsAuthenticated ? therapistUser.HouseNumber : youMustBeLoggedIn,
                City = therapistUser.City,
                Country = therapistUser.Country,
                PostalCode = User.Identity.IsAuthenticated ? therapistUser.PostalCode : youMustBeLoggedIn,
                About = therapistUser.About,
                ClientReviews = getClientReviewsForTherapist(therapistUser.Id)
            };



            if (!User.Identity.IsAuthenticated)
                publicProfileVm.Address = "You must be signed in to view therapist's address information.";
            else publicProfileVm.Address = publicProfileVm.Firstname + " " + publicProfileVm.Lastname + "\n" +
                                           "Street: " + publicProfileVm.Street + "\n" +
                                           "House number: " + publicProfileVm.HouseNumber + "\n" +
                                           "City: " + publicProfileVm.City + " " + publicProfileVm.PostalCode + "\n" +
                                           "Country: " + publicProfileVm.Country;

            var therapistContactMethods = _context.TherapistsContactMethods.Find(cm => cm.TherapistId == therapistUser.Id).ToList();

            if (therapistContactMethods != null)
            {
                foreach (var therapistContactMethod in therapistContactMethods)
                {
                    var contactMethod = _context.ContactMethods.GetById(therapistContactMethod.ContactMethodId);

                    if (contactMethod != null)
                    {
                        publicProfileVm.ToChooseFrom_ContactMethods.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Text = contactMethod.Icon + "|" + contactMethod.Name + "|" + contactMethod.Color,
                            Value = contactMethod.Id
                        });
                    }
                }
            }

            if (therapistSpecialities != null)
            {
                foreach (var therapistSpecialty in therapistSpecialities)
                {
                    var specialty = _context.Specialities.GetById(therapistSpecialty.SpecialityId);

                    publicProfileVm.SpecialitiesTherapistDescription.Add(new SpecialitiesTherapistDescription
                    {
                        Name = specialty.Name,
                        Color = specialty.Color,
                        Icon = specialty.Icon
                    });
                }
            }

            var sessions = _context.Sessions.Find(sess => sess.TherapistId == therapistUser.Id).ToList();

            if (sessions != null)
            {
                foreach (var session in sessions)
                {
                    publicProfileVm.Sessions.Add(new SessionJsonViewModel
                    {
                        SessionId = session.Id,
                        Subject = session.Subject,
                        Description = session.Description,
                        StartDateTime = session.StartDateTime,
                        EndDateTime = session.EndDateTime,
                        Type = session.Type,
                        Price = session.Price,
                        Booked = session.Booked
                    });
                }
            }

            publicProfileVm.SerializeSessions();

            return View(publicProfileVm);
        }

        [HttpPost]
        //[Authorize(Roles = "Client")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookSession(TherapistPublicProfileViewModel publicProfileVm, string sessionId)
        {
            var loggedUser = await _userManager.GetUserAsync(User);

            if (loggedUser == null)
            {
                _session.SetToast("An error occured", "Please contact the Customer Support.", "error");
                return RedirectToAction("Index", "Home");
            }

            var therapistUser = _context.AspNetUsers.GetById(publicProfileVm.Id);

            if (therapistUser == null)
            {
                _session.SetToast("Selected therapist does not exist", null, "error");
                return RedirectToAction("All");
            }

            var user = await _userManager.FindByIdAsync(therapistUser.Id);

            if (user == null)
            {
                _session.SetToast("Selected therapist does not exist", null, "error");
                return RedirectToAction("All");
            }

            var therapist = _context.Therapists.GetById(therapistUser.TherapistAccountId);

            if (therapist == null)
            {
                _session.SetToast("Selected therapist does not exist", null, "error");
                return RedirectToAction("All");
            }

            var publicProfileVm_2 = new TherapistPublicProfileViewModel()
            {
                Id = publicProfileVm.Id,
                TherapistAccountId = user.TherapistAccountId,
                PhoneNumber = therapistUser.PhoneNumber,
                About = therapist.About,
                Chosen_ContactMethodId = publicProfileVm.Chosen_ContactMethodId,
                City = therapist.City,
                ContactInfo = publicProfileVm.ContactInfo,
                Country = therapist.Country,
                Firstname = user.FirstName,
                Lastname = user.LastName,
                Gender = therapist.Gender,
                HouseNumber = therapist.HouseNumber,
                PostalCode = therapist.PostalCode,
                //Rating = publicProfileVm.Rating, !!! ovde da se vratim
                Sessions = new List<SessionJsonViewModel>(),
                SpecialitiesTherapistDescription = new List<SpecialitiesTherapistDescription>(),
                Street = therapist.Street,
                ToChooseFrom_ContactMethods = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>(),
                YearOfBirth = user.YearOfBirth.Value.ToString()
            };

            if (!User.Identity.IsAuthenticated)
                publicProfileVm_2.Address = "You must be signed in to view therapist's address information.";
            else publicProfileVm_2.Address = publicProfileVm_2.Firstname + " " + publicProfileVm_2.Lastname + "\n" +
                                           "Street: " + publicProfileVm_2.Street + "\n" +
                                           "House number: " + publicProfileVm_2.HouseNumber + "\n" +
                                           "City: " + publicProfileVm_2.City + " " + publicProfileVm_2.PostalCode + "\n" +
                                           "Country: " + publicProfileVm_2.Country;

            var therapistContactMethods = _context.TherapistsContactMethods.Find(cm => cm.TherapistId == therapist.Id).ToList();

            if (therapistContactMethods != null)
            {
                foreach (var therapistContactMethod in therapistContactMethods)
                {
                    var contactMethod = _context.ContactMethods.GetById(therapistContactMethod.ContactMethodId);

                    if (contactMethod != null)
                    {
                        publicProfileVm_2.ToChooseFrom_ContactMethods.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Text = contactMethod.Icon + "|" + contactMethod.Name + "|" + contactMethod.Color,
                            Value = contactMethod.Id
                        });
                    }
                }
            }

            var therapistSpecialities = _context.TherapistsSpecialities.Find(spec => spec.TherapistId == therapist.Id).ToList();

            if (therapistSpecialities != null)
            {
                foreach (var therapistSpecialty in therapistSpecialities)
                {
                    var specialty = _context.Specialities.GetById(therapistSpecialty.SpecialityId);

                    publicProfileVm_2.SpecialitiesTherapistDescription.Add(new SpecialitiesTherapistDescription
                    {
                        Name = specialty.Name,
                        Color = specialty.Color,
                        Icon = specialty.Icon
                    });
                }
            }

            var sessions = _context.Sessions.Find(sess => sess.TherapistId == therapist.Id).ToList();

            if (sessions != null)
            {
                foreach (var session in sessions)
                {
                    publicProfileVm_2.Sessions.Add(new SessionJsonViewModel
                    {
                        SessionId = session.Id,
                        Subject = session.Subject,
                        Description = session.Description,
                        StartDateTime = session.StartDateTime,
                        EndDateTime = session.EndDateTime,
                        Type = session.Type,
                        Price = session.Price,
                        Booked = session.Booked
                    });
                }
            }

            publicProfileVm_2.SerializeSessions();

            ////////////////////////////

            string errorKeys = string.Empty;

            foreach (var modelStateKey in ViewData.ModelState.Keys)
            {
                var value = ViewData.ModelState[modelStateKey];
                foreach (var error in value.Errors)
                {
                    errorKeys += modelStateKey + " / ";
                }
            }

            if (!errorKeys.Contains("Chosen_ContactMethodId") && publicProfileVm_2.Chosen_ContactMethodId == "7")
            {
                ModelState.Remove("ContactInfo");

                ViewBag.inPersonChosen = true;
            }

            if (!errorKeys.Contains("Chosen_ContactMethodId") && publicProfileVm_2.Chosen_ContactMethodId != "7")
            {
                ViewBag.otherContactMethodChosen = true;
            }

            if (ModelState.IsValid)
            {
                var isTherapist = await _userManager.IsInRoleAsync(loggedUser, "Therapist");

                if (isTherapist)
                {
                    ViewBag.validationFailed = true;
                    ViewBag.sessionId = sessionId;
                    ViewBag.showSwal = true;
                    ViewBag.swalTitle = "Therapists are currently not allowed to book sessions.";
                    ViewBag.swalBody = "Please sign in with a client account in order to book a session.";
                    ViewBag.swalSeverity = "warning";

                    return View("Profile", publicProfileVm_2);
                }

                // !!! 1. proveravam da li korisnik ne zakazuje slucajno sam sa sobom !!!
                if (loggedUser.Id == publicProfileVm.Id)
                {
                    ViewBag.validationFailed = true;
                    ViewBag.sessionId = sessionId;
                    ViewBag.showSwal = true;
                    ViewBag.swalTitle = "You cannot book a session with yourself!";
                    ViewBag.swalBody = "Please choose another therapist to book a session with.";
                    ViewBag.swalSeverity = "warning";

                    return View("Profile", publicProfileVm_2);
                }

                // !!! 2. proverim da li sesija postoji !!!
                // 3. proverim da li sesija pripada tom terapeutu kod koga se zakazuje.
                var sessionToBook = _context.Sessions.Find(s => s.Id == sessionId && s.TherapistId == therapist.Id).ToList();
                if (sessionToBook == null || sessionToBook.Count == 0)
                {
                    ViewBag.validationFailed = true;
                    ViewBag.sessionId = sessionId;
                    ViewBag.showSwal = true;
                    ViewBag.swalTitle = "Selected session does not exist.";
                    ViewBag.swalBody = "Please choose another session to book. If the error persists, contact the Customer Support.";
                    ViewBag.swalSeverity = "warning";

                    return View("Profile", publicProfileVm_2);
                }

                //proveravam da li je booked ili ne
                var sessionThatIsNotBooked = sessionToBook.Find(s => s.Booked == 0);
                if (sessionThatIsNotBooked == default || sessionThatIsNotBooked == null)
                {
                    ViewBag.validationFailed = true;
                    ViewBag.sessionId = sessionId;
                    ViewBag.showSwal = true;
                    ViewBag.swalTitle = "Selected session is booked already.";
                    ViewBag.swalBody = "Please choose another session to book.";
                    ViewBag.swalSeverity = "warning";

                    return View("Profile", publicProfileVm_2);
                }

                // 4. proverim da li korisnik koji zakazuje ima dovoljno kredita
                loggedUser.WebCredit = loggedUser.WebCredit != null ? loggedUser.WebCredit : 0;
                if (loggedUser.WebCredit < sessionThatIsNotBooked.Price)
                {
                    ViewBag.validationFailed = true;
                    ViewBag.sessionId = sessionId;
                    ViewBag.showSwalNotEnoughWebCredit = true;
                    ViewBag.swalTitle = "You do not have enough web credit.";
                    ViewBag.swalBody = "Please add web credit to your account and try again.";
                    ViewBag.redirectUrl = User.IsInRole("Therapist") ? Url.Action("AddWebCredit", "Account", new { Area = "Therapist" }) : @Url.Action("AddWebCredit", "Account", new { Area = "" });
                    ViewBag.swalSeverity = "warning";

                    return View("Profile", publicProfileVm_2);
                }

                // 5. proverim da li korisnik koji zakazuje nema neku sesiju koja se zavrsava posle nego sto pocinje sesija koju trenutno zakazuje.
                var sessionsToAttend = _context.BookedSessions.Find(s => s.ClientId == loggedUser.Id &&
                                                                         s.SessionDate.Date == sessionThatIsNotBooked.StartDateTime.Date &&
                                                                         (s.EndTime.TimeOfDay > sessionThatIsNotBooked.StartDateTime.TimeOfDay &&
                                                                         s.StartTime.TimeOfDay < sessionThatIsNotBooked.EndDateTime.TimeOfDay) &&
                                                                         s.Status == 0).ToList();

                if (sessionsToAttend != null)
                {
                    if (sessionsToAttend.Count > 0)
                    {
                        ViewBag.validationFailed = true;
                        ViewBag.sessionId = sessionId;
                        ViewBag.showSwal = true;
                        ViewBag.swalTitle = "Sessions are overlapping.";
                        ViewBag.swalBody = "You already have a session that ends after this session starts. You can not attend two sessions at the same time.";
                        ViewBag.swalSeverity = "warning";

                        return View("Profile", publicProfileVm_2);
                    }
                }

                // 7. proverim da li je sesija za bar jedan dan unapred.
                // sessionDate.Date >= DateTime.Today.AddDays(1)
                if (sessionThatIsNotBooked.StartDateTime.Date < DateTime.Today.AddDays(1))
                {
                    ViewBag.validationFailed = true;
                    ViewBag.sessionId = sessionId;
                    ViewBag.showSwal = true;
                    ViewBag.swalTitle = "Cannot book a session that starts today.";
                    ViewBag.swalBody = "Please seleact a session that starts at least one day in advance.";
                    ViewBag.swalSeverity = "warning";

                    return View("Profile", publicProfileVm_2);
                }

                if (getContactMethodName(publicProfileVm.Chosen_ContactMethodId) == "not found")
                {
                    ViewBag.validationFailed = true;
                    ViewBag.sessionId = sessionId;
                    ViewBag.showSwal = true;
                    ViewBag.swalTitle = "Please select a contact method.";
                    ViewBag.swalBody = "By selecting a contact method, you choose where you would like your session to be held.";
                    ViewBag.swalSeverity = "warning";

                    return View("Profile", publicProfileVm_2);
                }

                var bookedSession = new BookedSessions
                {
                    Id = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    TherapistId = therapist.Id,
                    TherapistFirstName = user.FirstName,
                    TherapistLastName = user.LastName,
                    TherapistEmail = user.Email,
                    TherapistPhoneNumber = user.PhoneNumber,
                    TherapistStreet = therapist.Street,
                    TherapistHouseNumber = therapist.HouseNumber,
                    TherapistCity = therapist.City,
                    TherapistCountry = therapist.Country,
                    TherapistPostalCode = therapist.PostalCode,
                    ClientId = loggedUser.Id,
                    ClientFirstName = loggedUser.FirstName,
                    ClientLastName = loggedUser.LastName,
                    ClientEmail = loggedUser.Email,
                    ClientPhoneNumber = loggedUser.PhoneNumber,
                    Subject = sessionThatIsNotBooked.Subject,
                    Description = sessionThatIsNotBooked.Description,
                    Price = sessionThatIsNotBooked.Price,
                    Type = sessionThatIsNotBooked.Type,
                    SessionDate = sessionThatIsNotBooked.StartDateTime.Date,
                    StartTime = new DateTime() + sessionThatIsNotBooked.StartDateTime.TimeOfDay,
                    EndTime = new DateTime() + sessionThatIsNotBooked.EndDateTime.TimeOfDay,
                    BookingDate = DateTime.Now,
                    ContactMethodName = getContactMethodName(publicProfileVm.Chosen_ContactMethodId),
                    ContactInfo = publicProfileVm.ContactInfo,
                    //status 0 = pending
                    Status = 0
                };

                var aspNetUser = _context.AspNetUsers.GetById(loggedUser.Id);
                aspNetUser.WebCredit = aspNetUser.WebCredit - sessionThatIsNotBooked.Price;

                _context.AspNetUsers.Update(aspNetUser);
                _context.BookedSessions.Insert(bookedSession);

                sessionThatIsNotBooked.Booked = 1;
                _context.Sessions.Update(sessionThatIsNotBooked);

                await _context.SaveAsync();

                ViewBag.success = true;
                ViewBag.swalTitle = "Congratulations!";
                ViewBag.swalBody = "You have successfully booked the session.";
                ViewBag.swalSeverity = "success";
                ViewBag.redirectUrl = Url.Action("SessionDetails", "Sessions", new { Area = "", bookingId = bookedSession.Id });

                return View("Profile", publicProfileVm_2);
            }
            else
            {
                ViewBag.validationFailed = true;
                ViewBag.sessionId = sessionId;

                return View("Profile", publicProfileVm_2);
            }
        }

        [NonAction]
        private string getContactMethodName(string contactMethodId)
        {
            var contactMethod = _context.ContactMethods.GetById(contactMethodId);

            if (contactMethod != null || contactMethod != default)
                return contactMethod.Name;

            return "not found";
        }

        [NonAction]
        private List<ClientReviewPartialViewModel> getClientReviewsForTherapist(string therapistId)
        {
            if (string.IsNullOrEmpty(therapistId))
                return new List<ClientReviewPartialViewModel>();

            var ratings = _context.Ratings.Find(r => r.TherapistId == therapistId).ToList();

            var clientReviews = new List<ClientReviewPartialViewModel>();

            foreach(var rating in ratings)
            {
                var tempItem = new ClientReviewPartialViewModel();
                tempItem.Map(rating);
                clientReviews.Add(tempItem);
            }

            return clientReviews;
        }

        [NonAction]
        private double getTherapistAverageRating(string therapistId)
        {
            if (string.IsNullOrEmpty(therapistId))
                return 0;

            var ratings = _context.Ratings.Find(r => r.TherapistId == therapistId).ToList();

            if (ratings.Count > 0)
            {
                return ratings.Average(r => r.Rating);
            }

            return 0;
        }
    }
}
