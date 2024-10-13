using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.WidgetViewModels
{
    public class NavFilterWithClearWidgetViewModel
    {
        public string Classes { get; set; }
        public string ClearFilterId { get; }
        public string ClearFilterArea { get; }
        public string ClearFilterController { get; }
        public string ClearFilterAction { get; }

        public List<NavFilterDropdownWidgetViewModel> Filters { get; }

        public NavFilterWithClearWidgetViewModel(string classes, string clearFilterArea, string clearFilterController, string clearFilterAction, List<NavFilterDropdownWidgetViewModel> filters)
        {
            Filters = filters ?? new List<NavFilterDropdownWidgetViewModel>();

            Classes = classes;
            ClearFilterId = "clear-filter";
            ClearFilterArea = clearFilterArea;
            ClearFilterController = clearFilterController;
            ClearFilterAction = clearFilterAction;
        }

        public NavFilterWithClearWidgetViewModel(string clearFilterArea, string clearFilterController, string clearFilterAction, List<NavFilterDropdownWidgetViewModel> filters)
        {
            Filters = filters ?? new List<NavFilterDropdownWidgetViewModel>();

            Classes = "mb-3";
            ClearFilterId = "clear-filter";
            ClearFilterArea = clearFilterArea;
            ClearFilterController = clearFilterController;
            ClearFilterAction = clearFilterAction;
        }
    }
}
