using System.Collections.Generic;

namespace WebApplication9.WidgetViewModels
{
    public class NavFilterDropdownWidgetViewModel
    {
        public string Id { get; }
        public bool AriaExpanded { get; }
        public string FontAwesomeIcon { get; }
        public string IconBootstrapColorClass { get; }
        public string MainFilterText { get; }
        public string DropdownId { get; }
        public string AriaLabelledby { get; }

        public List<NavFilterDropdownLinkWidgetViewModel> Links { get; }

        public NavFilterDropdownWidgetViewModel(string id, bool ariaExpanded, string fontAwesomeIcon, string iconBootstrapColorClass, string mainFilterText, string dropdownId, string ariaLabelledby, List<NavFilterDropdownLinkWidgetViewModel> links)
        {
            Links = links ?? new List<NavFilterDropdownLinkWidgetViewModel>();

            Id = id;
            AriaExpanded = ariaExpanded;
            FontAwesomeIcon = fontAwesomeIcon;
            IconBootstrapColorClass = iconBootstrapColorClass;
            MainFilterText = mainFilterText;
            DropdownId = dropdownId;
            AriaLabelledby = ariaLabelledby;
        }

    }
}
