using Database.Models;
using Database.RepositoryImplementations;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using Framework.Notifications;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication9.Base
{
    public class BaseController : Controller
    {
        protected ISession _session => HttpContext.Session;
        protected INotificationRepository _notificationRepository;
        protected readonly UnitOfWork _context;
        protected readonly UserManager<CustomClient> _userManager;
        protected readonly SignInManager<CustomClient> _signInManager;

        protected BaseController(INotificationRepository notificationRepository)
        {
            _context = new UnitOfWork(new LajsnaProbaContext());
            _notificationRepository = notificationRepository;
            //_notificationRepository.Send(new Notifications
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    SenderUserId = "1",
            //    ReceiverUserId = "4088a8d0-baca-4018-b78c-7705ae7f27d4",
            //    Title = "TITLE_1",
            //    Body = "BODY_1",
            //    Severity = "primary",
            //    SendingDateTime = DateTime.Now,
            //    Icon = "far fa-money-bill",
            //    Read = false
            //});
        }

        //protected BaseController(UserManager<CustomClient> userManager)
        //{
        //    _context = new UnitOfWork(new LajsnaProbaContext());
        //    _userManager = userManager;
        //}

        protected BaseController(INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager)
        {
            _context = new UnitOfWork(new LajsnaProbaContext());
            _userManager = userManager;
            _notificationRepository = notificationRepository;
        }

        protected BaseController(INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager)
        {
            _context = new UnitOfWork(new LajsnaProbaContext());
            _userManager = userManager;
            _signInManager = signInManager;
            _notificationRepository = notificationRepository;
        }

        protected void ShowToastOnThisPageIfSet()
        {
            if (_session.HasToast())
            {
                ViewBag.toast = _session.GetToast();
                _session.RemoveToastFromKeys();
            }
        }
    }
}
