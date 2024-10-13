using DataTransferObjects.ViewModels.Shared;
using System.Collections.Generic;

namespace WebApplication9.PartialViewModels
{
    public class ContactMethodsModalPartialViewModel
    {
        public string TherapistId { get; set; }
        public List<ContactMethodViewModel> ContactMethods { get; set; }

        public ContactMethodsModalPartialViewModel()
        {
            ContactMethods = new List<ContactMethodViewModel>();
        }
    }
}
