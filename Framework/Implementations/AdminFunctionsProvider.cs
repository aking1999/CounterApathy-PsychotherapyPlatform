using Database.Models;
using Framework.Emails;
using Framework.Interfaces;
using Framework.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Framework.Implementations
{
    public class AdminFunctionsProvider : IAdminFunctionsProvider
    {
        private readonly IMailService _mailService;
        private readonly INotificationRepository _notificationRepository;
        private UserManager<CustomClient> _userManager;

        public AdminFunctionsProvider(IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            _mailService = scope.ServiceProvider.GetRequiredService<IMailService>();
            _notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            _userManager = scope.ServiceProvider.GetRequiredService<UserManager<CustomClient>>();

        }
    }
}
