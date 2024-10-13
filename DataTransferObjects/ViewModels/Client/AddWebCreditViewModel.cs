using System;
using System.Collections.Generic;
using System.Text;

namespace DataTransferObjects.ViewModels.Client
{
    public class AddWebCreditViewModel
    {
        public CardsAddWebCreditViewModel Cards { get; set; }
        public PayPalAddWebCreditViewModel PayPal { get; set; }
        public PostOfSerbiaAddWebCreditViewModel PostOfSerbia { get; set; }


        public AddWebCreditViewModel()
        {
            PayPal = new PayPalAddWebCreditViewModel();
            PostOfSerbia = new PostOfSerbiaAddWebCreditViewModel();
            Cards = new CardsAddWebCreditViewModel();
        }
    }
}
