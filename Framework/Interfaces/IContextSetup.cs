using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Interfaces
{
    public interface IContextSetup
    {
        bool AddClientRole();
        bool AddTherapistRole();
        bool AddAdminRole();
        bool AddAdminAccount();
        bool AddContactMethods();
        bool AddSpecialties();
        bool AddPsychotherapyTechniques();
        bool AddClientSupportTicketTopics();
        bool AddTherapistSupportTicketTopics();
        bool PrepareToRun();
    }
}
