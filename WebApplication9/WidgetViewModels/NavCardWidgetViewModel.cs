using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.WidgetViewModels
{
    public class NavCardWidgetViewModel
    {
        public string FontAwesomeIcon { get; }
        public string IconBootstrapColorClass { get; }
        public string PrimaryHeader { get; }
        public string SecondaryHeader { get; }
        public string BootstrapColsClass { get; }
        public bool HasAdditionalInfo { get { return !string.IsNullOrWhiteSpace(AdditionalInfo); } }
        public string AdditionalInfo { get; set; }

        public NavCardWidgetViewModel(string fontAwesomeIcon, string iconBootstrapColorClass, string primaryHeader, string secondaryHeader)
        {
            FontAwesomeIcon = fontAwesomeIcon;
            IconBootstrapColorClass = iconBootstrapColorClass;
            PrimaryHeader = primaryHeader;
            SecondaryHeader = secondaryHeader;
            BootstrapColsClass = "col-lg-8";
        }

        //public NavCardWidgetViewModel(string fontAwesomeIcon,
        //    string iconBootstrapColorClass,
        //    string primaryHeader,
        //    string secondaryHeader,
        //    string additionalInfo)
        //{
        //    FontAwesomeIcon = fontAwesomeIcon;
        //    IconBootstrapColorClass = iconBootstrapColorClass;
        //    PrimaryHeader = primaryHeader;
        //    SecondaryHeader = secondaryHeader;
        //    BootstrapColsClass = "col-lg-8"; ;
        //    AdditionalInfo = additionalInfo;
        //}

        public NavCardWidgetViewModel(string fontAwesomeIcon, string iconBootstrapColorClass, string primaryHeader, string secondaryHeader, string bootstrapColsClass)
        {
            FontAwesomeIcon = fontAwesomeIcon;
            IconBootstrapColorClass = iconBootstrapColorClass;
            PrimaryHeader = primaryHeader;
            SecondaryHeader = secondaryHeader;
            BootstrapColsClass = bootstrapColsClass;
        }

        public NavCardWidgetViewModel(string fontAwesomeIcon,
            string iconBootstrapColorClass,
            string primaryHeader,
            string secondaryHeader,
            string bootstrapColsClass,
            string additionalInfo)
        {
            FontAwesomeIcon = fontAwesomeIcon;
            IconBootstrapColorClass = iconBootstrapColorClass;
            PrimaryHeader = primaryHeader;
            SecondaryHeader = secondaryHeader;
            BootstrapColsClass = bootstrapColsClass;
            AdditionalInfo = additionalInfo;
        }
    }
}
