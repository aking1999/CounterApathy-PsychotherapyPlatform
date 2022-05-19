using Database.Models;
using Database.RepositoryImplementations;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WebApplication9.Areas.Admin.ViewModels;
using WebApplication9.Base;
using WebApplication9.Helpers;
using Framework.Notifications;
using WebApplication9.Models;
using WebApplication9.ViewModels;

namespace WebApplication9.Areas.Admin.Controllers
{
    [Area(areaName: "Admin")]
    [Authorize(Roles = "Admin")]
    public class AccountController : BaseController
    {
        private readonly IWebHostEnvironment _environment;

        public AccountController(
            IWebHostEnvironment environment,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager) : base(notificationRepository, userManager, signInManager)
        {
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            ShowToastOnThisPageIfSet();

            var user = await _userManager.GetUserAsync(User);

            var viewModel = new AdminProfileViewModel();
            viewModel.Map(user, _environment);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminProfileViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var tempUser = await _userManager.GetUserAsync(User);

                viewModel.MapProfilePhoto(tempUser, _environment);
                return View("Profile", viewModel);
            }

            var customClient = await _userManager.FindByIdAsync(viewModel.Id);

            var user = await _userManager.FindByIdAsync(viewModel.Id);

            if (viewModel.ProfilePhoto == null && user.ProfilePhoto == null)
                customClient.ProfilePhoto = null;

            else if (viewModel.ProfilePhoto == null && user.ProfilePhoto != null)
            {
                customClient.ProfilePhoto = user.ProfilePhoto;
            }

            //Ako se nova slika uploaduje, a stara slika ne postoji. Tj prvi put se uploaduje slika
            else if (viewModel.ProfilePhoto != null && user.ProfilePhoto == null)
            {
                string fileName = viewModel.ProfilePhoto.FileName;
                string fileExtension = Path.GetExtension(fileName);
                string uniqueFileName = Guid.NewGuid().ToString();
                string uniquieFileNameWithExtension = uniqueFileName + fileExtension;
                string saveToPath = Path.Combine(_environment.WebRootPath, @"\images\user-images") + $@"\{uniquieFileNameWithExtension}";

                using (FileStream stream = System.IO.File.Create(_environment.ContentRootPath + @"\wwwroot" + saveToPath))
                {
                    await viewModel.ProfilePhoto.CopyToAsync(stream);
                    await stream.FlushAsync();
                }

                customClient.ProfilePhoto = uniquieFileNameWithExtension;
            }

            //Ako se nova slika uploaduje, ali postoji i stara slika, tj user edituje sliku
            //Prilikom edita, ako ima i fajla u bazi, i uploadovan je novi fajl, treba izbrisati stari iz foldera sa slikama
            else if (viewModel.ProfilePhoto != null && user.ProfilePhoto != null)
            {
                string path = Path.Combine(_environment.WebRootPath, @"images\user-images\" + user.ProfilePhoto);

                FileInfo file = new FileInfo(path);

                if (file.Exists)
                    file.Delete();

                string fileName = viewModel.ProfilePhoto.FileName;
                string fileExtension = Path.GetExtension(fileName);
                string uniqueFileName = Guid.NewGuid().ToString();
                string uniquieFileNameWithExtension = uniqueFileName + fileExtension;
                string saveToPath = Path.Combine(_environment.WebRootPath, @"\images\user-images") + $@"\{uniquieFileNameWithExtension}";

                using (FileStream stream = System.IO.File.Create(_environment.ContentRootPath + @"\wwwroot" + saveToPath))
                {
                    await viewModel.ProfilePhoto.CopyToAsync(stream);
                    await stream.FlushAsync();
                }

                customClient.ProfilePhoto = uniquieFileNameWithExtension;
            }

            customClient.Map(viewModel);

            var result = await _userManager.UpdateAsync(customClient);

            if (result.Succeeded)
            {
                _session.SetToast("You have edited your profile.", null, "success");

                return RedirectToAction("Profile");
            }

            _session.SetToast("Error while updating profile. Please try again.", null, "error");

            return View("Profile", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);

            if (!hasPassword)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, viewModel.OldPassword, viewModel.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(viewModel);
            }

            await _signInManager.RefreshSignInAsync(user);

            _session.SetToast("You have changed your password.", null, "success");

            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProfilePhoto()
        {
            var result = await _userManager.DeleteProfilePhotoIfExists(User, _environment);

            if (result.Succeeded)
            {
                _session.SetToast("You have deleted your profile photo.", null, "success");

                return Json(new
                {
                    success = true,
                    location = Url.Action("Profile", "Account")
                });
            }
            else
            {
                var errorMessage = result.Errors.ElementAt(0).Description;
                _session.SetToast(errorMessage, null, "error");
                return RedirectToAction("Profile", "Account");
            }
        }

        [HttpGet]
        public IActionResult Dashboard(string applications = "all")
        {
            ShowToastOnThisPageIfSet();

            var dashboard = new AdminDashboardViewModel();

            if (applications.ToLower() == "accepted")
                dashboard.AggregatedInformationUnparsed = getAcceptedApplications();

            else if (applications.ToLower() == "rejected")
                dashboard.AggregatedInformationUnparsed = getRejectedApplications();

            else if (applications.ToLower() == "pending")
                dashboard.AggregatedInformationUnparsed = getPendingApplications();

            else dashboard.AggregatedInformationUnparsed = getAllApplications();

            dashboard.ParseAggregatedInformation();

            return View(dashboard);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplicationDetails(string userId)
        {
            var userThatApplied = await _userManager.FindByIdAsync(userId);
            var userApplicationArray = _context.TherapistApplications.Find(app => app.UserId == userThatApplied.Id);

            if (userThatApplied != null && userApplicationArray != null)
            {
                var userApplication = userApplicationArray.ElementAtOrDefault(0);

                if (userApplication != default)
                {
                    var detailsVm = new ApplicationDetailsViewModel
                    {
                        UserId = userThatApplied.Id,
                        FirstName = userThatApplied.FirstName,
                        LastName = userThatApplied.LastName,
                        UserName = userThatApplied.UserName,
                        Email = userThatApplied.Email,
                        EmailConfirmed = userThatApplied.EmailConfirmed.ToString(),
                        PhoneNumber = userThatApplied.PhoneNumber,
                        WebCredit = userThatApplied.WebCredit != null ? userThatApplied.WebCredit.ToString() : "0",
                        YearOfBirth = userThatApplied.YearOfBirth != null ? userThatApplied.YearOfBirth.ToString() : "0",
                        AmountDue = userThatApplied.AmountDue != null ? userThatApplied.AmountDue.ToString() : "0",
                        TherapistApplicationId = userApplication.Id,
                        ApplicationDate = userApplication.ApplicationDate,
                        Accepted = userApplication.Accepted != null ? userApplication.Accepted.ToString() : "error",
                        Street = userApplication.Street,
                        City = userApplication.City,
                        Country = userApplication.Country,
                        PostalCode = userApplication.PostalCode,
                        Gender = userApplication.Gender,
                        HouseNumber = userApplication.HouseNumber,
                        University = userApplication.University,
                        PastCompanies = userApplication.PastCompanies,
                        ProfilePhoto = userApplication.ProfilePhoto,
                    };

                    var applicationSpecialitiesOfApplication = _context.TherapistApplicationsSpecialities.Find(appSpec => appSpec.TherapistApplicationId == userApplication.Id);

                    if (applicationSpecialitiesOfApplication == null)
                    {
                        _session.SetToast("User did not choose any specialities.", null, "error");

                        return RedirectToAction("Dashboard");
                    }

                    if (applicationSpecialitiesOfApplication == default || applicationSpecialitiesOfApplication.Count() == 0)
                    {
                        _session.SetToast("User did not choose any specialities.", null, "error");

                        return RedirectToAction("Dashboard");
                    }

                    foreach (var appSpec in applicationSpecialitiesOfApplication.ToList())
                    {
                        var speciality = _context.Specialities.GetById(appSpec.SpecialityId);
                        detailsVm.Specialities.Add(speciality);
                    }

                    if (detailsVm.Specialities == default || detailsVm.Specialities.Count() == 0)
                    {
                        _session.SetToast("User did not choose any valid specialities.", null, "error");

                        return RedirectToAction("Dashboard");
                    }

                    detailsVm.Specialities = detailsVm.Specialities.OrderBy(spec => spec.Name).ToList();

                    return View(detailsVm);
                }

                _session.SetToast("User or user's application does not exist.", null, "error");

                return RedirectToAction("Dashboard");
            }

            _session.SetToast("User or user's application does not exist.", null, "error");

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public IActionResult AcceptApplication(string userId)
        {
            try
            {
                var userThatAppliedAwaitable = _userManager.FindByIdAsync(userId);
                userThatAppliedAwaitable.Wait();
                var userThatApplied = userThatAppliedAwaitable.Result;

                var userApplicationArray = _context.TherapistApplications.Find(app => app.UserId == userThatApplied.Id);

                if (userThatApplied != null && userApplicationArray != null)
                {
                    var userApplication = userApplicationArray.ElementAtOrDefault(0);

                    if (userApplication != default)
                    {
                        if (userApplication.Accepted == 0)
                        {
                            var therapistAccountId = Guid.NewGuid().ToString();

                            var therapist = new Therapists()
                            {
                                Id = therapistAccountId,
                                OnVacation = 0,
                                Gender = userApplication.Gender,
                                Street = userApplication.Street,
                                HouseNumber = userApplication.HouseNumber,
                                City = userApplication.City,
                                Country = userApplication.Country,
                                PostalCode = userApplication.PostalCode,
                                University = userApplication.University,
                                PastCompanies = userApplication.PastCompanies
                            };

                            _context.Therapists.Insert(therapist);

                            foreach (var therapistsSpeciality in _context.TherapistApplicationsSpecialities.Find(appSpec => appSpec.TherapistApplicationId == userApplication.Id))
                            {
                                _context.TherapistsSpecialities.Insert(new TherapistsSpecialities()
                                {
                                    TherapistId = therapistAccountId,
                                    SpecialityId = therapistsSpeciality.SpecialityId
                                });
                            }

                            _context.SaveAsync().Wait();

                            userThatApplied.ProfilePhoto = userApplication.ProfilePhoto;
                            userThatApplied.TherapistAccountId = therapistAccountId;

                            var userUpdatedAwaitable = _userManager.UpdateAsync(userThatApplied);
                            userUpdatedAwaitable.Wait();

                            var userUpdated = userUpdatedAwaitable.Result;

                            userApplication.Accepted = 1;

                            _context.TherapistApplications.Update(userApplication);

                            _context.SaveAsync().Wait();

                            _userManager.RemoveFromRoleAsync(userThatApplied, "Client").Wait();
                            _userManager.AddToRoleAsync(userThatApplied, "Therapist").Wait();

                            return Json(new
                            {
                                success = true,
                                title = "Congratulations!",
                                body = "You have successfully <span class='text-success'>accepted</span> therapist's application.",
                                location = Url.Action("Dashboard", "Account", new { Area = "Admin" })
                            });


                            //_session.SetToast("Error while accepting therapist's application.", null, "error");

                            //return RedirectToAction("Dashboard", "Account", new { Area = "Admin" });
                        }

                        _session.SetToast("Therapist's application already accepted/rejected", null, "warning");

                        return RedirectToAction("Dashboard", "Account", new { Area = "Admin" });
                    }

                    _session.SetToast("Therapist's application does not exist", null, "error");

                    return RedirectToAction("Dashboard", "Account", new { Area = "Admin" });
                }

                _session.SetToast("User or their application does not exist", null, "error");

                return RedirectToAction("Dashboard", "Account", new { Area = "Admin" });
            }
            catch (Exception e)
            {
                _session.SetToast("Error while accepting therapist's application", null, "error");

                return RedirectToAction("Dashboard", "Account", new { Area = "Admin" });
            }
        }

        [HttpPost]
        public IActionResult RejectApplication(string userId)
        {
            try
            {
                var userThatAppliedAwaitable = _userManager.FindByIdAsync(userId);
                userThatAppliedAwaitable.Wait();
                var userThatApplied = userThatAppliedAwaitable.Result;

                var userApplicationArray = _context.TherapistApplications.Find(app => app.UserId == userThatApplied.Id);

                if (userThatApplied != null && userApplicationArray != null)
                {
                    var userApplication = userApplicationArray.ElementAtOrDefault(0);

                    if (userApplication != default)
                    {
                        if (userApplication.Accepted == 0)
                        {
                            userApplication.Accepted = -1;

                            _context.TherapistApplications.Update(userApplication);

                            _context.SaveAsync().Wait();

                            return Json(new
                            {
                                success = true,
                                title = "Congratulations!",
                                body = "You have successfully <span class='text-danger'>rejected</span> therapist's application.",
                                location = Url.Action("Dashboard", "Account", new { Area = "Admin" })
                            });
                        }

                        _session.SetToast("Therapist's application already accepted/rejected", null, "warning");

                        return RedirectToAction("Dashboard", "Account", new { Area = "Admin" });
                    }

                    _session.SetToast("Therapist's application does not exist", "", "error");

                    return RedirectToAction("Dashboard", "Account", new { Area = "Admin" });
                }

                _session.SetToast("User or their application does not exist", "", "error");

                return RedirectToAction("Dashboard", "Account", new { Area = "Admin" });
            }
            catch (Exception)
            {
                _session.SetToast("Error while rejecting therapist's application.", null, "error");

                return RedirectToAction("Dashboard", "Account", new { Area = "Admin" });
            }
        }

        [HttpGet]
        public IActionResult KeepPendingApplication()
        {
            //return Content("cao");
            _session.SetToast("Therapist's application kept pending", null, "warning");

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> AddWebCredit(string role = "all")
        {
            var addWebCreditUsersList = new List<AdminAddWebCreditUserViewModel>();

            IList<CustomClient> usersWithSpecifiedRole = new List<CustomClient>();

            if (role.ToLower() == "admin")
                usersWithSpecifiedRole = await _userManager.GetUsersInRoleAsync("Admin");

            else if (role.ToLower() == "therapist")
                usersWithSpecifiedRole = await _userManager.GetUsersInRoleAsync("Therapist");

            else if (role.ToLower() == "client")
                usersWithSpecifiedRole = await _userManager.GetUsersInRoleAsync("Client");

            else
            {
                var listOfAdmins = await _userManager.GetUsersInRoleAsync("Admin");
                var listOfTherapists = await _userManager.GetUsersInRoleAsync("Therapist");
                var listOfClients = await _userManager.GetUsersInRoleAsync("Client");

                foreach (var adminUser in listOfAdmins)
                    usersWithSpecifiedRole.Add(adminUser);

                foreach (var therapistUser in listOfTherapists)
                    usersWithSpecifiedRole.Add(therapistUser);

                foreach (var clientUser in listOfClients)
                    usersWithSpecifiedRole.Add(clientUser);
            }

            foreach (var user in usersWithSpecifiedRole)
            {
                addWebCreditUsersList.Add(new AdminAddWebCreditUserViewModel
                {
                    UserId = user.Id,
                    ProfilePhoto = user.ProfilePhoto,
                    FullName = user.FirstName + " " + user.LastName,
                    Username = user.UserName,
                    Email = user.Email,
                    Role = await _userManager.GetUserRoleAsync(user.Id),
                    WebCredit = user.WebCredit?.ToString()
                });
            }

            return View(addWebCreditUsersList);
        }

        [HttpPost]
        public async Task<IActionResult> AddWebCredit(string tracker, string webCreditAmount)
        {
            //tracker je userId, ali nisam nazvao userId jer bih onda i u ajax requestu morao userId da nazovem,
            //a nije dobro na front-endu da se zove userId jer ce se onda znati da je userId.

            var userToAddCreditTo = await _userManager.FindByIdAsync(tracker);

            if (userToAddCreditTo == null || userToAddCreditTo == default)
                return Json(new
                {
                    success = false,
                    title = "Oh no!",
                    body = "User does not exist.",
                    icon = "error"
                });

            double webCreditAmountParsed;

            if (double.TryParse(webCreditAmount, out webCreditAmountParsed))
            {
                if(webCreditAmountParsed > 0)
                {
                    if(webCreditAmountParsed <= 200000)
                    {
                        double? userWebCreditInDatabase = userToAddCreditTo.WebCredit.HasValue ? userToAddCreditTo.WebCredit : 0;

                        userToAddCreditTo.WebCredit = userWebCreditInDatabase + webCreditAmountParsed;

                        var result = await _userManager.UpdateAsync(userToAddCreditTo);

                        if (result.Succeeded)
                            return Json(new
                            {
                                success = true,
                                title = "Congratulations!",
                                body = "You have successfully added <span class='text-success'>" + webCreditAmountParsed + "</span> Web Credit to the user.",
                                icon = "success",
                                newAmount = userToAddCreditTo.WebCredit
                            });

                        return Json(new
                        {
                            success = false,
                            title = "Oh no!",
                            body = "An error occured while trying to add Web Credit to the user.",
                            icon = "error"
                        });
                    }

                    return Json(new
                    {
                        success = false,
                        title = "Limit surpassed!",
                        body = "Please enter an amount between <span class='text-primary'>1 and 200000</span>.",
                        icon = "warning"
                    });
                }

                return Json(new
                {
                    success = false,
                    title = "Oh no!",
                    body = "Entered number must be higher than 0.",
                    icon = "error"
                });
            }

            return Json(new
            {
                success = false,
                title = "Oh no!",
                body = "Positive number is required.",
                icon = "error"
            });
        }

        [NonAction]
        private IEnumerable<object> getAcceptedApplications()
        {
            return from application in _context.TherapistApplications.Find(app => app.Accepted == 1)
                   join user in _context.AspNetUsers.GetAll()
                   on application.UserId equals user.Id
                   orderby application.ApplicationDate
                   select new
                   {
                       user.Id,
                       application.ProfilePhoto,
                       user.FirstName,
                       user.LastName,
                       user.Email,
                       application.ApplicationDate,
                       application.Accepted
                   };

            //return _context.TherapistApplications.FindTherapistApplicationsWithAspNetUsers(app => app.Accepted == 1).OrderBy(app => app.ApplicationDate);
        }

        [NonAction]
        private IEnumerable<object> getRejectedApplications()
        {
            return from application in _context.TherapistApplications.Find(app => app.Accepted == -1)
                   join user in _context.AspNetUsers.GetAll()
                   on application.UserId equals user.Id
                   orderby application.ApplicationDate
                   select new
                   {
                       user.Id,
                       application.ProfilePhoto,
                       user.FirstName,
                       user.LastName,
                       user.Email,
                       application.ApplicationDate,
                       application.Accepted
                   };
        }

        [NonAction]
        private IEnumerable<object> getPendingApplications()
        {
            return from application in _context.TherapistApplications.Find(app => app.Accepted == 0)
                   join user in _context.AspNetUsers.GetAll()
                   on application.UserId equals user.Id
                   orderby application.ApplicationDate
                   select new
                   {
                       user.Id,
                       application.ProfilePhoto,
                       user.FirstName,
                       user.LastName,
                       user.Email,
                       application.ApplicationDate,
                       application.Accepted
                   };
        }

        [NonAction]
        private IEnumerable<object> getAllApplications()
        {
            return from application in _context.TherapistApplications.GetAll()
                   join user in _context.AspNetUsers.GetAll()
                   on application.UserId equals user.Id
                   orderby application.ApplicationDate
                   select new
                   {
                       user.Id,
                       application.ProfilePhoto,
                       user.FirstName,
                       user.LastName,
                       user.Email,
                       application.ApplicationDate,
                       application.Accepted
                   };
        }
    }
}
