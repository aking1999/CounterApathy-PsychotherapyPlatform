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
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication9.Areas.Therapist.ViewModels;
using WebApplication9.Helpers;
using Framework.Helpers;
using System.IO;
using WebApplication9.ViewModels;
using WebApplication9.Models;
using System.Diagnostics;
using WebApplication9.Base;
using Framework.Notifications;

namespace WebApplication9.Areas.Therapist.Controllers
{
    [Area(areaName: "Therapist")]
    [Authorize(Roles = "Therapist")]
    public class AccountController : BaseController
    {
        private readonly IWebHostEnvironment _environment;

        public AccountController(INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager,
            IWebHostEnvironment environment) : base(notificationRepository, userManager, signInManager)
        {
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (therapistHasCompletedAccountSetup() == 1)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user.TherapistAccountId == null || user.TherapistAccountId == default)
                {
                    _session.SetToast("Invalid account.", "Please contact the Customer Support for help.", "Error");
                    return RedirectToAction("Index", "Home", new { Area = "" });
                }

                var therapistAccount = _context.Therapists.GetById(user.TherapistAccountId);

                if (therapistAccount == null || therapistAccount == default)
                {
                    _session.SetToast("Invalid account.", "Please contact the Customer Support for help.", "Error");
                    return RedirectToAction("Index", "Home", new { Area = "" });
                }

                var therapistSpecialities = _context.TherapistsSpecialities.Find(thrSpec => thrSpec.TherapistId == therapistAccount.Id).ToList();

                var viewModel = new TherapistProfileViewModel();

                foreach (var therapistSpeciality in therapistSpecialities)
                {
                    var speciality = _context.Specialities.GetById(therapistSpeciality.SpecialityId);

                    viewModel.Specialities += speciality.Name + "\n";
                    viewModel.NumberOfSpecialities++;
                }

                var contactMethods = _context.ContactMethods.GetAll().ToList();

                if (contactMethods == null)
                {
                    viewModel.ToChooseFrom_ContactMethods = new List<SelectListItem>();
                }
                else if (contactMethods.Count() == 0)
                {
                    viewModel.ToChooseFrom_ContactMethods = new List<SelectListItem>();
                }
                else
                {
                    viewModel.ToChooseFrom_ContactMethods = new List<SelectListItem>();
                    foreach (var contactMethod in contactMethods)
                    {
                        viewModel.ToChooseFrom_ContactMethods.Add(new SelectListItem
                        {
                            Text = contactMethod.Icon + "|" + contactMethod.Name + "|" + contactMethod.Color,
                            Value = contactMethod.Id,
                            Selected = checkIfContactMethodIsSelectedByTherapist(therapistAccount.Id, contactMethod.Id)
                        });
                    }
                }

                viewModel.Map(user, _environment);
                viewModel.Map(therapistAccount);

                ShowToastOnThisPageIfSet();

                return View(viewModel);
            }
            else if (therapistHasCompletedAccountSetup() == 0)
            {
                ShowToastOnThisPageIfSet();

                return RedirectToAction("AccountSetup");
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
        public async Task<IActionResult> Edit(TherapistProfileViewModel viewModel)
        {
            if (therapistHasCompletedAccountSetup() == 1)
            {
                if (!ModelState.IsValid)
                {
                    var tempTherapistUser = await _userManager.GetUserAsync(User);
                    var tempTherapistAccount = _context.Therapists.GetById(tempTherapistUser.TherapistAccountId);

                    viewModel.MapProfilePhoto(tempTherapistUser, _environment);

                    viewModel.ToChooseFrom_ContactMethods = new List<SelectListItem>();
                    foreach (var contactMethod in _context.ContactMethods.GetAll().ToList())
                    {
                        viewModel.ToChooseFrom_ContactMethods.Add(new SelectListItem
                        {
                            Text = contactMethod.Icon + "|" + contactMethod.Name + "|" + contactMethod.Color,
                            Value = contactMethod.Id,
                            Selected = checkIfContactMethodIsSelectedByTherapist(tempTherapistAccount.Id, contactMethod.Id)
                        });
                    }

                    _session.SetToast("Profile not edited", "Check your entire profile page and try again.", "error");

                    // !!! ovde mozda bude trebalo da se stavi if(_session.HasToast())
                    ViewBag.toast = _session.GetToast();
                    _session.RemoveToastFromKeys();

                    return View("Profile", viewModel);
                }

                var therapistCustomClientAccount = await _userManager.GetUserAsync(User);

                if (therapistCustomClientAccount.TherapistAccountId == null)
                {
                    _session.SetToast("Invalid account", "Please contact the Customer Support for help.", "error");
                    return RedirectToAction("Index", "Home", new { Area = "" });
                }

                var therapistAccount = _context.Therapists.GetById(therapistCustomClientAccount.TherapistAccountId);

                if (therapistAccount == null || therapistAccount == default)
                {
                    _session.SetToast("Invalid account", "Please contact the Customer Support for help.", "error");
                    return RedirectToAction("Index", "Home", new { Area = "" });
                }

                var user = await _userManager.GetUserAsync(User);

                if (viewModel.ProfilePhoto == null && user.ProfilePhoto == null)
                    therapistCustomClientAccount.ProfilePhoto = null;

                else if (viewModel.ProfilePhoto == null && user.ProfilePhoto != null)
                    therapistCustomClientAccount.ProfilePhoto = user.ProfilePhoto;

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

                    therapistCustomClientAccount.ProfilePhoto = uniquieFileNameWithExtension;
                }

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

                    therapistCustomClientAccount.ProfilePhoto = uniquieFileNameWithExtension;
                }

                therapistCustomClientAccount.Map(viewModel);

                var result = await _userManager.UpdateAsync(therapistCustomClientAccount);

                if (result.Succeeded)
                {
                    therapistAccount.Map(viewModel);

                    if (alreadyHasContactMethods(therapistAccount.Id))
                        deleteTherapistContactMethods(therapistAccount.Id);

                    bool hasAtleastOneContactMethod = false;

                    foreach (var contactMethodId in viewModel.Chosen_ContactMethodsIds)
                    {
                        if (contactMethodExistsInDatabase(contactMethodId))
                        {
                            hasAtleastOneContactMethod = true;
                            _context.TherapistsContactMethods.Insert(new TherapistsContactMethods
                            {
                                TherapistId = therapistAccount.Id,
                                ContactMethodId = contactMethodId
                            });
                        }
                    }

                    if (!hasAtleastOneContactMethod)
                    {
                        _session.SetToast("An Error occured", null, "error");
                        return View("Profile", viewModel);
                    }

                    _context.Therapists.Update(therapistAccount);
                    var numberOfEditedEntities = await _context.SaveAsync();

                    if (numberOfEditedEntities < 1)
                    {
                        _session.SetToast("An Error occured", null, "error");
                        return View("Profile", viewModel);
                    }

                    _session.SetToast("Profile edited successfully", null, "success");
                    return RedirectToAction("Profile", "Account", new { Area = "Therapist" });
                }

                _session.SetToast("An error occured", null, "error");

                return View("Profile", viewModel);
            }
            else if (therapistHasCompletedAccountSetup() == 0)
            {
                return RedirectToAction("AccountSetup");
            }
            else
            {
                await _signInManager.SignOutAsync();

                _session.SetToast("Invalid account", "Please contact the Customer Support.", "error");
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            if (therapistHasCompletedAccountSetup() == 1)
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
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel viewModel)
        {
            if (therapistHasCompletedAccountSetup() == 1)
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
            else if (therapistHasCompletedAccountSetup() == 0)
            {
                return RedirectToAction("AccountSetup");
            }
            else
            {
                await _signInManager.SignOutAsync();

                _session.SetToast("Invalid account", "Please contact the Customer Support.", "error");
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProfilePhoto()
        {
            if (therapistHasCompletedAccountSetup() == 1)
            {
                var result = await _userManager.DeleteProfilePhotoIfExists(User, _environment);

                if (result.Succeeded)
                {
                    _session.SetToast("You have deleted your profile photo.", null, "success");

                    return Json(new
                    {
                        success = true,
                        location = Url.Action("Profile", "Account", new { Area = "Therapist" })
                    });
                }
                else
                {
                    var errorMessage = result.Errors.ElementAt(0).Description;
                    _session.SetToast(errorMessage, null, "error");
                    return RedirectToAction("Profile", "Account", new { Area = "Therapist" });
                }
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

        [HttpGet]
        public IActionResult AccountSetup()
        {
            if (therapistHasCompletedAccountSetup() == 1)
            {
                _session.SetToast("You have already set up your therapist account", "To edit your account, go to the Profile page.", "info");

                return RedirectToAction("Profile");
            }

            var accountSetupVm = new TherapistAccountSetupViewModel();

            foreach (var contactMethod in _context.ContactMethods.GetAll())
            {
                accountSetupVm.ToChooseFrom_ContactMethods.Add(new SelectListItem
                {
                    Text = contactMethod.Icon + "|" + contactMethod.Name + "|" + contactMethod.Color,
                    Value = contactMethod.Id
                });
            }

            return View(accountSetupVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountSetup(TherapistAccountSetupViewModel accountSetupVm)
        {
            if (therapistHasCompletedAccountSetup() == 1)
            {
                _session.SetToast("You have already set up your therapist account", "To edit your account, go to the Profile page.", "info");
                return RedirectToAction("Profile");
            }

            string errorKeys = string.Empty;

            foreach (var modelStateKey in ViewData.ModelState.Keys)
            {
                var value = ViewData.ModelState[modelStateKey];
                foreach (var error in value.Errors)
                {
                    errorKeys += modelStateKey + " / ";
                }
            }

            if (!errorKeys.Contains("Chosen_ContactMethodsIds") && !accountSetupVm.Chosen_ContactMethodsIds.Contains("7"))
            {
                ViewBag.otherContactMethodChosen = true;

                ModelState.Remove("Street");
                ModelState.Remove("HouseNumber");
                ModelState.Remove("City");
                ModelState.Remove("PostalCode");
                ModelState.Remove("Country");
            }
            
            if(!errorKeys.Contains("Chosen_ContactMethodsIds") && accountSetupVm.Chosen_ContactMethodsIds.Contains("7"))
            {
                ViewBag.inPersonChosen = true;
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var therapistUser = _context.Therapists.GetById(user.TherapistAccountId);

                if (alreadyHasContactMethods(user.TherapistAccountId))
                    deleteTherapistContactMethods(user.TherapistAccountId);

                therapistUser.About = accountSetupVm.About;
                therapistUser.Street = accountSetupVm.Street;
                therapistUser.HouseNumber = accountSetupVm.HouseNumber;
                therapistUser.City = accountSetupVm.City;
                therapistUser.PostalCode = accountSetupVm.PostalCode;
                therapistUser.Country = accountSetupVm.Country;

                bool hasAtleastOneContactMethod = false;

                foreach (var contactMethodId in accountSetupVm.Chosen_ContactMethodsIds)
                {
                    if (contactMethodExistsInDatabase(contactMethodId))
                    {
                        hasAtleastOneContactMethod = true;
                        _context.TherapistsContactMethods.Insert(new TherapistsContactMethods
                        {
                            TherapistId = user.TherapistAccountId,
                            ContactMethodId = contactMethodId
                        });
                    }
                }

                if (!hasAtleastOneContactMethod)
                {
                    _session.SetToast("Selected contact methods are not valid.", "Please contact the Customer Support if the error persists.", "error");
                    return RedirectToAction("AccountSetup", "Account");
                }

                await _context.SaveAsync();

                _session.SetToast("You have set up your therapist account successfully.", null, "success");
                return RedirectToAction("Profile", "Account");
            }

            foreach (var contactMethod in _context.ContactMethods.GetAll())
            {
                accountSetupVm.ToChooseFrom_ContactMethods.Add(new SelectListItem
                {
                    Text = contactMethod.Icon + "|" + contactMethod.Name + "|" + contactMethod.Color,
                    Value = contactMethod.Id
                });
            }

            return View(accountSetupVm);
        }

        //[HttpPost]
        //public async Task<IActionResult> PayDue()
        //{
        //    if (therapistHasCompletedAccountSetup() == 1)
        //    {
        //        var user = await _userManager.GetUserAsync(User);

        //        user.WebCredit = user.WebCredit != null ? user.WebCredit : 0;
        //        user.AmountDue = user.AmountDue != null ? user.AmountDue : 0;

        //        if (user.AmountDue == 0)
        //        {
        //            return Json(new
        //            {
        //                success = true,
        //                icon = "info",
        //                title = "Thank you anyway!",
        //                body = "You currently do not have any dues to pay.",
        //                location = Url.Action("Profile", "Account")
        //            });
        //        }

        //        if (user.WebCredit >= user.AmountDue)
        //        {
        //            user.WebCredit -= user.AmountDue;
        //            user.AmountDue = null;

        //            if (user.WebCredit == 0)
        //                user.WebCredit = null;

        //            var result = await _userManager.UpdateAsync(user);

        //            if (result.Succeeded)
        //            {
        //                return Json(new
        //                {
        //                    success = true,
        //                    icon = "success",
        //                    title = "Congratulations!",
        //                    body = "You have successfully paid your dues.",
        //                    location = Url.Action("Profile", "Account")
        //                });
        //            }
        //            else
        //            {
        //                return Json(new
        //                {
        //                    success = false,
        //                    icon = "error",
        //                    title = "An error occured!",
        //                    body = "Please try again. If the error persists, contact the Customer Support.",
        //                    location = Url.Action("Profile", "Action")
        //                });
        //            }
        //        }
        //        else
        //        {
        //            return Json(new
        //            {
        //                success = false,
        //                icon = "error",
        //                title = "Oh no!",
        //                body = "You do not have enough web credit to pay your dues. Please add more web credit then try again.",
        //                location = Url.Action("Profile", "Account")
        //            });
        //        }
        //    }
        //    else if (therapistHasCompletedAccountSetup() == 0)
        //    {
        //        return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
        //    }
        //    else
        //    {
        //        await _signInManager.SignOutAsync();

        //        _session.SetToast("Invalid account", "Please contact the Customer Support.", "error");
        //        return RedirectToAction("Index", "Home", new { Area = "" });
        //    }
        //}

        [HttpGet]
        public IActionResult AddWebCredit()
        {
            if (therapistHasCompletedAccountSetup() == 1)
            {
                return View();
            }
            else if (therapistHasCompletedAccountSetup() == 0)
            {
                return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
            }
            else
            {
                _signInManager.SignOutAsync();

                _session.SetToast("Invalid account", "Please contact the Customer Support.", "error");
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
        }

        //[HttpPost]
        //public IActionResult DeleteAccount(string password)
        //{
        //    if(therapistHasCompletedAccountSetup() == 1)
        //    {
        //        return Json("");
        //    }
        //    else if(therapistHasCompletedAccountSetup() == 0)
        //    {
        //        return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
        //    }
        //    else
        //    {
        //        _signInManager.SignOutAsync();

        //        _session.SetToast("Invalid account", "Please contact the Customer Support.", "error");
        //        return RedirectToAction("Index", "Home", new { Area = "" });
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> PublicProfile()
        {
            if (therapistHasCompletedAccountSetup() == 1)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user.TherapistAccountId == null || user.TherapistAccountId == default)
                {
                    _session.SetToast("Invalid account.", "Please contact the Customer Support for help.", "Error");
                    return RedirectToAction("Index", "Home", new { Area = "" });
                }

                var therapistAccount = _context.Therapists.GetById(user.TherapistAccountId);

                if (therapistAccount == null || therapistAccount == default)
                {
                    _session.SetToast("Invalid account.", "Please contact the Customer Support for help.", "Error");
                    return RedirectToAction("Index", "Home", new { Area = "" });
                }

                var therapistSpecialities = _context.TherapistsSpecialities.Find(thrSpec => thrSpec.TherapistId == therapistAccount.Id).ToList();

                var publicProfileVm = new TherapistPublicProfileViewModel()
                {
                    Firstname = user.FirstName,
                    Lastname = user.LastName,
                    YearOfBirth = user.YearOfBirth.ToString(),
                    Gender = therapistAccount.Gender,
                    City = therapistAccount.City,
                    Country = therapistAccount.Country,
                    About = therapistAccount.About,
                    SpecialitiesTherapistDescription = new List<SpecialitiesTherapistDescription>()
                };

                //publicProfileVm.SpecialitiesTherapistDescription = new List<SpecialitiesTherapistDescription>();

                foreach (var therapistSpeciality in therapistSpecialities)
                {
                    var specialty = _context.Specialities.GetById(therapistSpeciality.SpecialityId);

                    publicProfileVm.SpecialitiesTherapistDescription.Add(new SpecialitiesTherapistDescription
                    {
                        Name = specialty.Name,
                        Color = specialty.Color,
                        Icon = specialty.Icon
                        // !!! ovde dodati jos i da iz baze izvlaci deskripciju od terapeuta za svaki speciality ali ta funkcionalnost jos nije dodata !!!
                    });
                }

                return View(publicProfileVm);
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
        private bool contactMethodExistsInDatabase(string contactMethodId)
        {
            if (_context.ContactMethods.GetById(contactMethodId) != null)
                return true;

            return false;
        }

        [NonAction]
        private bool alreadyHasContactMethods(string therapistId)
        {
            var contactMethods = _context.TherapistsContactMethods.Find(contact => contact.TherapistId == therapistId);
            if (contactMethods != null && contactMethods != default)
            {
                if (contactMethods.Any())
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        [NonAction]
        private void deleteTherapistContactMethods(string therapistId)
        {
            var contactMethods = _context.TherapistsContactMethods.Find(contact => contact.TherapistId == therapistId);

            foreach (var contactMethod in contactMethods)
                _context.TherapistsContactMethods.Delete(contactMethod);
        }

        //Ovu metodu koristim za stikliranje izabranih kontakt metoda u multi-selectu.
        [NonAction]
        private bool checkIfContactMethodIsSelectedByTherapist(string therapistId, string contactMethodId)
        {
            var therapistContactMethod = _context.TherapistsContactMethods.Find(contact => (contact.TherapistId == therapistId) && (contact.ContactMethodId == contactMethodId)).ToList();

            if (therapistContactMethod == null)
                return false;

            if (therapistContactMethod == default)
                return false;

            if (therapistContactMethod.Count == 0)
                return false;

            return true;
        }
    }
}
