using Database.Models;
using Framework.Emails;
using Framework.Helpers.ExtensionMethods;
using Framework.Implementations;
using Framework.Interfaces;
using Framework.Models;
using Framework.Notifications;
using Framework.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Base;

namespace WebApplication9.Controllers
{
    public class GuidesController : BaseController
    {
        private readonly ICustomClientFunctionsProvider _clientFunctions;

        public GuidesController(IErrorLogger errors,
            IHttpContextAccessor contextAccessor) : base(errors, contextAccessor)
        {
            _clientFunctions = new CustomClientFunctionsProvider(contextAccessor);
        }

        [HttpGet("/uputstva")]
        public async Task<IActionResult> All()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    if (await User.GetRoleAsync() == UserRoles.Therapist) return View("GuidesForTherapists");
                    else return View("GuidesForClients");
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: User.Identity.IsAuthenticated);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/uputstva/lako-zakažite-psihoterapijsku-seansu")]
        public async Task<IActionResult> BookingRoadmap()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    return View();
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: User.Identity.IsAuthenticated);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/uputstva/pristup-google-meetu-putem-računara")]
        public async Task<IActionResult> JoinGoogleMeetByComputer()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    return View();
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: User.Identity.IsAuthenticated);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/uputstva/pristup-google-meetu-putem-telefona")]
        public async Task<IActionResult> JoinGoogleMeetByPhone()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    return View();
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: User.Identity.IsAuthenticated);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }
    }
}
