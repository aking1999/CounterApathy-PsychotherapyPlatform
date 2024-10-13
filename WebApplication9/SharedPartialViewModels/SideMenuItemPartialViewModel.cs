using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.SharedPartialViewModels
{
    public class SideMenuItemPartialViewModel
    {
        public string Id { get; private set; }
        public string Active { get; private set; }
        public string Area { get; private set; }
        public string Controller { get; private set; }
        public string Action { get; private set; }
        public string FontAwesomeIcon { get; private set; }
        public string Text { get; private set; }
        public bool ShowCount { get; private set; }

        public SideMenuItemPartialViewModel(string active, string area, string controller, string action, string fontAwesomeIcon, string text)
        {
            Active = active;
            Area = area;
            Controller = controller;
            Action = action;
            FontAwesomeIcon = fontAwesomeIcon;
            Text = text;
        }

        public SideMenuItemPartialViewModel(string id, string active, string area, string controller, string action, string fontAwesomeIcon, string text, bool showCount)
        {
            CheckIdAndShowCount(id, showCount);

            Id = id;
            Active = active;
            Area = area;
            Controller = controller;
            Action = action;
            FontAwesomeIcon = fontAwesomeIcon;
            Text = text;
            ShowCount = showCount;
        }

        private void CheckIdAndShowCount(string id, bool showCount)
        {
            if(string.IsNullOrWhiteSpace(id) && showCount)
                throw new Exception("If ShowCount is set to True, Id cannot be null (must be set to Menu Link Name, example: unread-notifications).");
        }
    }
}
