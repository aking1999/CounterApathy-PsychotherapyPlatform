using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Notifications
{
    public class NotificationsViewModel
    {
        private IDateTimeHelper _dateHelper { get; set; }

        public string Id { get; set; }
        public string SenderUserId { get; set; }
        public string ReceiverUserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Severity { get; set; }
        public bool? Read { get; set; }
        public string SendingDateTime { get; set; }
        public string Icon { get; set; }
        public bool? Important { get; set; }

        public NotificationsViewModel(IDateTimeHelper dateHelper)
        {
            _dateHelper = dateHelper;
        }

        public void Map(Database.Models.Notifications notification)
        {
            Id = notification.Id;
            SenderUserId = notification.SenderUserId;
            ReceiverUserId = notification.ReceiverUserId;
            Title = notification.Title;
            Body = notification.Body;
            Severity = notification.Severity;
            Read = notification.Read;
            SendingDateTime = _dateHelper.ConvertDateTimeFromUtcToLocalString(notification.SendingDateTime.GetValueOrDefault());
            Important = notification.Important;
            Icon = notification.Icon;
        }
    }
}
