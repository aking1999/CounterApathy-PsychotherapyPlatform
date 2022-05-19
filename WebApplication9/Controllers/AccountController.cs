using Database.Models;
using Database.RepositoryImplementations;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication9.Base;
using WebApplication9.Helpers;
using Framework.Notifications;
using WebApplication9.Models;
using WebApplication9.ViewModels;

namespace WebApplication9.Controllers
{
    [Authorize(Roles = "Client")]
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

            var viewModel = new CustomClientViewModel();
            viewModel.Map(user, _environment);
            viewModel.HasAppliedForTherapistAccount = _userManager.HasAppliedForTherapistAccount(user, _context);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomClientViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var tempUser = await _userManager.GetUserAsync(User);

                viewModel.MapProfilePhoto(tempUser, _environment);
                return View("Profile", viewModel);
            }

            //var customClient = await _userManager.FindByIdAsync(viewModel.Id);
            var customClient = await _userManager.GetUserAsync(User);

            //var user = await _userManager.FindByIdAsync(viewModel.Id);
            var user = await _userManager.GetUserAsync(User);

            if (viewModel.ProfilePhoto == null && user.ProfilePhoto == null)
                customClient.ProfilePhoto = null;

            else if (viewModel.ProfilePhoto == null && user.ProfilePhoto != null)
                customClient.ProfilePhoto = user.ProfilePhoto;


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

        // !!! Ovu metodu testirati kasnije kad dodam funkcionalnost za upload slike
        // jer mislim da ako umesto u if stavim !result.Succeeded da nece raditi jer ajax-u vraca
        //celu stranicu umesto Json-a !!!
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
        public async Task<IActionResult> Apply()
        {
            var customClient = await _userManager.GetUserAsync(User);

            var hasAlreadyApplied = _userManager.HasAppliedForTherapistAccount(customClient, _context);

            if (hasAlreadyApplied)
            {
                _session.SetToast("You have already applied once.", null, "error");
                return RedirectToAction("Profile");
            }

            var applicationVm = new TherapistApplicationsViewModel();

            applicationVm.ToChooseFrom_Specialities = getSpecialtiesForDropdown();

            applicationVm.Map(customClient, _environment);
            //applicationVm.ApplicationDate = DateTime.Now.ToString();

            return View(applicationVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(TherapistApplicationsViewModel applicationVm)
        {
            var customClient = await _userManager.GetUserAsync(User);

            var hasAlreadyApplied = _userManager.HasAppliedForTherapistAccount(customClient, _context);

            if (hasAlreadyApplied)
            {
                _session.SetToast("You have already applied once.", "Each account can apply only once.", "error");
                return RedirectToAction("Profile");
            }

            if (ModelState.IsValid)
            {
                var therapistApplicationId = Guid.NewGuid().ToString();

                string fileName = null;
                string fileExtension = string.Empty;
                string uniqueFileName = string.Empty;
                string uniquieFileNameWithExtension = null;
                string saveToPath = string.Empty;

                fileName = applicationVm.ProfilePhoto.FileName;
                fileExtension = Path.GetExtension(fileName);
                uniqueFileName = Guid.NewGuid().ToString();
                uniquieFileNameWithExtension = uniqueFileName + fileExtension;

                saveToPath = Path.Combine(_environment.WebRootPath, @"\images\user-images") + $@"\{uniquieFileNameWithExtension}";

                using (FileStream stream = System.IO.File.Create(_environment.ContentRootPath + @"\wwwroot" + saveToPath))
                {
                    await applicationVm.ProfilePhoto.CopyToAsync(stream);
                    await stream.FlushAsync();
                }

                TherapistApplications application = new TherapistApplications
                {
                    Id = therapistApplicationId,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    ProfilePhoto = uniquieFileNameWithExtension,
                    ApplicationDate = DateTime.Now.ToString(),
                    Street = !string.IsNullOrEmpty(applicationVm.Street) ? applicationVm.Street.Trim() : null,
                    HouseNumber = !string.IsNullOrEmpty(applicationVm.HouseNumber) ? applicationVm.HouseNumber.Trim() : null,
                    City = !string.IsNullOrEmpty(applicationVm.City) ? applicationVm.City.Trim() : null,
                    Country = !string.IsNullOrEmpty(applicationVm.Country) ? applicationVm.Country.Trim() : null,
                    PostalCode = !string.IsNullOrEmpty(applicationVm.PostalCode) ? applicationVm.PostalCode.Trim() : null,
                    Gender = !string.IsNullOrEmpty(applicationVm.Gender) ? applicationVm.Gender.Trim() : null,
                    University = !string.IsNullOrEmpty(applicationVm.University) ? applicationVm.University.Trim() : null,
                    PastCompanies = !string.IsNullOrEmpty(applicationVm.PastCompanies) ? applicationVm.PastCompanies.Trim() : null,
                    Accepted = 0,
                };

                bool hasOneGender = false;

                if (genderExists(application.Gender))
                {
                    hasOneGender = true;
                }

                _context.TherapistApplications.Insert(application);
                //await _context.SaveAsync();

                bool hasAtLeastOneSpeciality = false;

                foreach (var specialityId in applicationVm.Chosen_SpecialitiesIds)
                {
                    if (specialityExistsInDatabase(specialityId))
                    {
                        hasAtLeastOneSpeciality = true;
                        _context.TherapistApplicationsSpecialities.Insert(new TherapistApplicationsSpecialities
                        {
                            TherapistApplicationId = therapistApplicationId,
                            SpecialityId = specialityId
                        });
                    }
                }

                if (!hasAtLeastOneSpeciality)
                {
                    _session.SetToast("Selected specialities are not valid", null, "error");
                    return RedirectToAction("Profile", "Account");
                }

                if (!hasOneGender)
                {
                    _session.SetToast("Selected gender is not valid", null, "error");
                    return RedirectToAction("Profile", "Account");
                }

                ViewBag.showSwal = true;

                await _context.SaveAsync();

                applicationVm.ToChooseFrom_Specialities = getSpecialtiesForDropdown();

                //applicationVm.MapProfilePhoto(customClient, _environment);

                return View(applicationVm);
            }

            applicationVm.ToChooseFrom_Specialities = getSpecialtiesForDropdown();

            return View(applicationVm);
        }

        [HttpPost]
        public JsonResult DeleteAccount(string password)
        {
            var awaitableUser = _userManager.GetUserAsync(User);
            awaitableUser.Wait();

            var customClient = awaitableUser.Result;

            if (customClient == null)
            {
                return Json(new
                {
                    success = false,
                    errorMessage = "User not found."
                });
            }

            var correctPasswordAwaitable = _userManager.CheckPasswordAsync(customClient, password);
            correctPasswordAwaitable.Wait();

            var correctPassword = correctPasswordAwaitable.Result;

            if (!correctPassword)
            {
                return Json(new
                {
                    success = false,
                    errorMessage = "Incorrect password."
                });
            }

            //Obrisi sliku, ukoliko je korisnik ima
            if (!string.IsNullOrEmpty(customClient.ProfilePhoto))
            {
                string path = Path.Combine(_environment.WebRootPath, @"images\user-images\" + customClient.ProfilePhoto);

                FileInfo file = new FileInfo(path);

                if (file.Exists)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception)
                    {
                        return Json(new
                        {
                            success = false,
                            errorMessage = "Unexpected error occurred while deleting your account. Try again."
                        });
                    }
                }
            }

            var therapistApplicationArray = _context.TherapistApplications.Find(app => app.UserId == customClient.Id).ToList();

            if (therapistApplicationArray.Count > 0)
            {
                var therapistApplication = therapistApplicationArray.ElementAtOrDefault(0);

                if (!string.IsNullOrEmpty(therapistApplication.ProfilePhoto))
                {
                    string path = Path.Combine(_environment.WebRootPath, @"images\user-images\" + therapistApplication.ProfilePhoto);

                    FileInfo file = new FileInfo(path);

                    if (file.Exists)
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception)
                        {
                            return Json(new
                            {
                                success = false,
                                errorMessage = "Unexpected error occurred while deleting your account. Try again."
                            });
                        }
                    }
                }
            }

            _signInManager.SignOutAsync();
            var deleteCustomClientAwaitable = _userManager.DeleteAsync(customClient);
            deleteCustomClientAwaitable.Wait();

            var deleteCustomClient = deleteCustomClientAwaitable.Result;

            if (!deleteCustomClient.Succeeded)
            {
                return Json(new
                {
                    success = false,
                    errorMessage = "Unexpected error occurred while deleting your account. Try again."
                });
            }

            return Json(new
            {
                success = true,
                errorMessage = "Your account has been deleted successfully."
            });
        }

        //[HttpPost]
        //public async Task<IActionResult> PayDue()
        //{
        //    var user = await _userManager.GetUserAsync(User);

        //    user.WebCredit = user.WebCredit != null ? user.WebCredit : 0;
        //    user.AmountDue = user.AmountDue != null ? user.AmountDue : 0;

        //    if (user.AmountDue == 0)
        //    {
        //        return Json(new
        //        {
        //            success = true,
        //            icon = "info",
        //            title = "Congratulations!",
        //            body = "You currently do not have any dues to pay.",
        //            location = Url.Action("Profile", "Account")
        //        });
        //    }

        //    if (user.WebCredit >= user.AmountDue)
        //    {
        //        user.WebCredit -= user.AmountDue;
        //        user.AmountDue = null;

        //        if (user.WebCredit == 0)
        //            user.WebCredit = null;

        //        var result = await _userManager.UpdateAsync(user);

        //        if (result.Succeeded)
        //        {
        //            return Json(new
        //            {
        //                success = true,
        //                icon = "success",
        //                title = "Congratulations!",
        //                body = "You have successfully paid your dues.",
        //                location = Url.Action("Profile", "Account")
        //            });
        //        }
        //        else
        //        {
        //            return Json(new
        //            {
        //                success = false,
        //                icon = "error",
        //                title = "An error occured!",
        //                body = "Please try again. If the error persists, contact the customer support.",
        //                location = Url.Action("Profile", "Action")
        //            });
        //        }
        //    }
        //    else
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            icon = "error",
        //            title = "Oh no!",
        //            body = "You do not have enough web credit to pay your dues. Please add more web credit then try again.",
        //            location = Url.Action("Profile", "Account")
        //        });
        //    }
        //}

        [HttpGet]
        public IActionResult AddWebCredit()
        {
            return View();
        }

        [NonAction]
        private bool specialityExistsInDatabase(string specialityId)
        {
            if (_context.Specialities.GetById(specialityId) != null)
                return true;

            return false;
        }

        [NonAction]
        private bool genderExists(string gender)
        {
            if (gender.ToLower() == "male" || gender.ToLower() == "female")
                return true;

            return false;
        }

        [NonAction]
        private List<SelectListItem> getSpecialtiesForDropdown()
        {
            var dropdown = new List<SelectListItem>();

            foreach (var speciality in _context.Specialities.GetAll().ToList())
            {
                dropdown.Add(new SelectListItem { Text = speciality.Name, Value = speciality.Id });
            }

            return dropdown.OrderBy(x => x.Text).ToList();
        }
    }
}
