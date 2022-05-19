using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.PartialViewModels
{
    public class SessionStatusPartial
    {
        //SessionStatusId Koristi se za JQuery da uzima ID elemenata i dodeljuje im status Pending|Proggress|Complete na osnovu vremena.
        public string SessionStatusId { get; set; }
        public string SessionDate { get; set; }
        public string SessionStartTime { get; set; }
        public string SessionEndTime { get; set; }
    }
}
