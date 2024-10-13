using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class TherapistApplicationsPsychotherapyTechniques
    {
        public string TherapistApplicationId { get; set; }
        public string PsychotherapyTechniqueId { get; set; }

        public virtual PsychotherapyTechniques PsychotherapyTechnique { get; set; }
        public virtual TherapistApplications TherapistApplication { get; set; }
    }
}
