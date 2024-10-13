using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class PsychotherapyTechniques
    {
        public PsychotherapyTechniques()
        {
            TherapistApplicationsPsychotherapyTechniques = new HashSet<TherapistApplicationsPsychotherapyTechniques>();
            TherapistPsychotherapyTechniques = new HashSet<TherapistPsychotherapyTechniques>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }

        public virtual ICollection<TherapistApplicationsPsychotherapyTechniques> TherapistApplicationsPsychotherapyTechniques { get; set; }
        public virtual ICollection<TherapistPsychotherapyTechniques> TherapistPsychotherapyTechniques { get; set; }
    }
}
