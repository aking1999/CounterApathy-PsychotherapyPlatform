using Database.Models;
using Database.RepositoryImplementations;
using Framework.Providers;
using Framework.Helpers;
using Framework.Helpers.ExtensionMethods;
using Framework.Interfaces;
using Framework.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UAParser;

namespace Framework.Implementations
{
    public class CustomClientFunctionsProvider : ICustomClientFunctionsProvider
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IErrorLogger _errors;
        private readonly ClaimsPrincipal User;
        private readonly UnitOfWork _context;
        private readonly UserManager<CustomClient> _userManager;

        public CustomClientFunctionsProvider(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
            _errors = contextAccessor.HttpContext.RequestServices.GetRequiredService<IErrorLogger>();
            User = contextAccessor.HttpContext.User;
            _userManager = contextAccessor.HttpContext.RequestServices.GetRequiredService<UserManager<CustomClient>>();
            _context = new UnitOfWork(new LajsnaProbaContext());
        }

        public void UpdateUserAccountInformation(string userId, bool isSigningIn)
        {
            try
            {
                if (!User.Identity.IsAuthenticated) throw new GeneralException("User is not signed in.", signOutUser: true);

                var userIdInDb = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdInDb != userId) throw new GeneralException($"Currently logged user with Id '{userIdInDb}' does not match with passed Id '{userId}'.", signOutUser: true);

                var area = !string.IsNullOrWhiteSpace(_contextAccessor.HttpContext.GetRouteData()?.Values["area"]?.ToString()) ?
                        _contextAccessor.HttpContext.GetRouteData()?.Values["area"]?.ToString() + "/" : null;

                var controller = _contextAccessor.HttpContext.GetRouteValue("controller")?.ToString();
                var action = _contextAccessor.HttpContext.GetRouteValue("action")?.ToString();

                if (!IgnoreRoutes.Contains($"{controller}/{action}"))
                {
                    var accInfo = _context.UserAccountInformation.Find(i => i.UserId == userId).SingleOrDefault();

                    if (accInfo != default)
                    {
                        var userAgentParser = Parser.GetDefault().Parse(_contextAccessor.HttpContext.Request.Headers["User-Agent"]);
                        accInfo.LastActivity = !string.IsNullOrWhiteSpace(_contextAccessor.HttpContext.Request?.QueryString.Value) ?
                                $"{area}{controller}/{action}{_contextAccessor.HttpContext.Request?.QueryString.Value}".TakeMax(256) :
                                $"{area}{controller}/{action}".TakeMax(256);
                        accInfo.LastActivityDateTime = DateTime.UtcNow;
                        accInfo.UserAgent = userAgentParser.ToString().TakeMax(1024);
                        accInfo.IsCrawler = userAgentParser.Device.IsSpider;
                        accInfo.CurrentIpAddress = _contextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString().TakeMax(256);

                        if (isSigningIn) 
                            accInfo.SignInDateTime = DateTime.UtcNow;

                        //Parser.GetDefault().ParseOS(_contextAccessor.HttpContext.Request.Headers["User-Agent"]).Family; -> Windows
                        //Parser.GetDefault().ParseOS(_contextAccessor.HttpContext.Request.Headers["User-Agent"]).Major; -> 10

                        //Parser.GetDefault().ParseUserAgent(_contextAccessor.HttpContext.Request.Headers["User-Agent"]).Family; -> Chrome
                        //Parser.GetDefault().ParseUserAgent(_contextAccessor.HttpContext.Request.Headers["User-Agent"]).Major; -> 111

                        //Parser.GetDefault().ParseDevice(_contextAccessor.HttpContext.Request.Headers["User-Agent"]).Brand; -> "" prazan string / Apple
                        //Parser.GetDefault().ParseDevice(_contextAccessor.HttpContext.Request.Headers["User-Agent"]).Family; -> Other
                        //Parser.GetDefault().ParseDevice(_contextAccessor.HttpContext.Request.Headers["User-Agent"]).Model; -> "" prazan string / SM-G955U (za samsung)
                        //Parser.GetDefault().ParseDevice(_contextAccessor.HttpContext.Request.Headers["User-Agent"]).IsSpider.ToString(); -> False
                    }
                    else
                    {
                        _context.UserAccountInformation.Insert(new UserAccountInformation
                        {
                            Id = Helper.GenerateNumbersId(),
                            UserId = userId,
                            LastActivity = !string.IsNullOrWhiteSpace(_contextAccessor.HttpContext.Request?.QueryString.Value) ?
                                $"{area}{controller}/{action}{_contextAccessor.HttpContext.Request?.QueryString.Value}".TakeMax(256) :
                                $"{area}{controller}/{action}".TakeMax(256),
                            LastActivityDateTime = DateTime.UtcNow,
                            UserAgent = Parser.GetDefault().Parse(_contextAccessor.HttpContext.Request.Headers["User-Agent"]).ToString().TakeMax(1024),
                            CurrentIpAddress = _contextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString().TakeMax(256),
                            SignInDateTime = DateTime.UtcNow,
                            RegistrationDateTime = DateTime.UtcNow
                        });
                    }

                    _context.Save();
                }
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "Framework", "CustomClientFunctionsProvider", "UpdateUserAccountInformation");
            }
        }

        public List<SelectListItem> Get_ToChooseFrom_Topics()
        {
            var items = new List<SelectListItem>();

            foreach (var topic in _context.ClientSupportTicketTopics.GetAll().ToList())
            {
                items.Add(new SelectListItem
                {
                    Value = topic.Id,
                    Text = topic.Icon + "|" + topic.Name + "|" + topic.Color
                });
            }

            return items;
        }

        public bool HasUpcomingConsultation(string userId)
        {
            var now = DateTime.UtcNow;
            return _context.BookedConsultations.ReadOnlyAny(s => s.ClientId == userId && (s.EndDateTime >= now));
        }

        public bool HasUpcomingSession(string userId)
        {
            var now = DateTime.UtcNow;
            return _context.BookedSessions.ReadOnlyAny(s => s.ClientId == userId && (s.EndTime >= now));
        }

        public bool HasUnreviewedBookedSession(string userId)
        {
            var now = DateTime.UtcNow;

            foreach(var bookedSessionId in _context.BookedSessions.ReadOnlyFind(s => s.ClientId == userId && (s.EndTime <= now)).Select(s => s.Id).ToList())
            {
                if (!_context.PendingRatings.ReadOnlyAny(r => r.BookedSessionId == bookedSessionId) &&
                    !_context.Ratings.ReadOnlyAny(r => r.BookedSessionId == bookedSessionId))
                    return true;
            }

            return false;
        }

        public List<string> GetUnreviewedBookedSessionsIds(string userId)
        {
            var now = DateTime.UtcNow;
            var unreviewedSessionIds = new List<string>();

            foreach (var bookedSessionId in _context.BookedSessions.ReadOnlyFind(s => s.ClientId == userId && (s.EndTime <= now)).Select(s => s.Id).ToList())
            {
                if (!_context.PendingRatings.ReadOnlyAny(r => r.BookedSessionId == bookedSessionId) &&
                    !_context.Ratings.ReadOnlyAny(r => r.BookedSessionId == bookedSessionId))
                    unreviewedSessionIds.Add(bookedSessionId);
            }

            return unreviewedSessionIds;
        }

        public async Task<EmailConfirmationStatus> HasConfirmedEmailAsync()
        {
            if (!User.Identity.IsAuthenticated)
                return EmailConfirmationStatus.Confirmed;

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return EmailConfirmationStatus.Error;

            if (await User.GetRoleAsync() != UserRoles.Client)
                return EmailConfirmationStatus.Confirmed;

            if (string.IsNullOrWhiteSpace(user.Email))
                return EmailConfirmationStatus.Error;

            if (await _userManager.IsEmailConfirmedAsync(user))
                return EmailConfirmationStatus.Confirmed;

            return EmailConfirmationStatus.NotConfirmed;
        }
    }
}
