namespace WebApplication9.WidgetViewModels
{
    public class NavFilterDropdownLinkWidgetViewModel
    {
        public string BootstrapTextColor { get; }
        public string Area { get; }
        public string Controller { get; }
        public string Action { get; }
        public string Filter { get; }
        public string Predicate { get; }
        public string Text { get; }

        public NavFilterDropdownLinkWidgetViewModel(string area, string controller, string action, string filter, string predicate, string text)
        {
            Area = area;
            Controller = controller;
            Action = action;
            Filter = filter;
            Predicate = predicate;
            Text = text;
        }

        public NavFilterDropdownLinkWidgetViewModel(string bootstrapTextColor, string area, string controller, string action, string filter, string predicate, string text)
        {
            BootstrapTextColor = bootstrapTextColor;
            Area = area;
            Controller = controller;
            Action = action;
            Filter = filter;
            Predicate = predicate;
            Text = text;
        }
    }
}
