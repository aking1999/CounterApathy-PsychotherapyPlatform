using Framework.Helpers.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTransferObjects.ViewModels.Shared
{
    public class NewsletterSubscriptionViewModel
    {
        private string email;

        [Display(Name = "Imejl", Prompt = "example@gmail.com")]
        [Required(ErrorMessage = "Polje je obavezno.")]
        [EmailAddress(ErrorMessage = "Unesite validan imejl.")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Unesite validan imejl.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Unesite od 5 do 100 karaktera.")]
        public string Email
        {
            get
            {
                return email;
            }
            set
            {
                email = value.RemoveSpecialCharacters(AllowedSpecialCharacters.ForEmail);
            }
        }
    }
}
