using System;
using System.Collections.Generic;
using System.Text;

namespace DataTransferObjects.PartialViewModels
{
    public class NotFoundPartialViewModel
    {
        public string HolderClasses { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string PrimaryButtonText { get; set; }
        public string PrimaryButtonUrl { get; set; }
        public string PrimaryButtonClasses { get; set; }
        public string SecondaryButtonText { get; set; }
        public string SecondaryButtonUrl { get; set; }
        public string SecondaryButtonClasses { get; set; }
        public bool IncludeSupportInfo { get; set; }

        public bool HasSecondaryButton
        {
            get
            {
                return !string.IsNullOrWhiteSpace(SecondaryButtonText) &&
                    !string.IsNullOrWhiteSpace(SecondaryButtonUrl) &&
                    !string.IsNullOrWhiteSpace(SecondaryButtonClasses);
            }
        }
    }
}