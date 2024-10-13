using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.SharedPartialViewModels
{
    public class SideMenuPartialViewModel
    {
        public string TitleIcon { get; }
        public string Title { get; }
        public string  TitleIconClasses { get; }
        public List<SideMenuItemPartialViewModel> MenuLinks { get; }

        public SideMenuPartialViewModel(string titleIcon, string title, string titleIconClasses, List<SideMenuItemPartialViewModel> menuLinks)
        {
            if (menuLinks == null)
                MenuLinks = new List<SideMenuItemPartialViewModel>();
            else
            {
                MenuLinks = menuLinks;

                var allUnique = MenuLinks
                                .Where(m => m.ShowCount)
                                .GroupBy(m => m.Id)
                                .All(g => g.Count() == 1);

                if(!allUnique)
                    throw new Exception("Duplicate LinkIds found among menu links.");
            }

            TitleIcon = titleIcon;
            Title = title;
            TitleIconClasses = titleIconClasses;
        }
    }
}
