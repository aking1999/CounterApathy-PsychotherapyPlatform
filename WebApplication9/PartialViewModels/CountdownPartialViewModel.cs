namespace WebApplication9.PartialViewModels
{
    public class CountdownPartialViewModel
    {
        //CountdownId is used for JQuery to take ID of element and add a counter to that element.
        public string CountdownId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
