using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Areas.Therapist.ViewModels
{
    public class GenderViewModel
    {
        private const string SOURCE_MALE = "male";
        private const string SOURCE_FEMALE = "female";
        private const string TARGET_MALE = "Muško";
        private const string TARGET_FEMALE = "Žensko";
        private string name;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && value.ToLower() == SOURCE_FEMALE.ToLower()) name = TARGET_FEMALE;
                else name = TARGET_MALE;
            }
        }

        public string ColorClass
        {
            get
            {
                return (!string.IsNullOrWhiteSpace(Name) && Name.ToLower() == TARGET_FEMALE.ToLower()) ? "text-pink" : "text-primary";
            }
        }

        public string Icon
        {
            get
            {
                return (!string.IsNullOrWhiteSpace(Name) && Name.ToLower() == TARGET_FEMALE.ToLower()) ? "fal fa-female" : "fal fa-male";
            }
        }

        public GenderViewModel()
        {
            Name = TARGET_MALE;
        }

        public GenderViewModel(string name)
        {
            Name = name;
        }
    }
}
