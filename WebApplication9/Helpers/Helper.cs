using Database.Models;
using Database.RepositoryImplementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Helpers
{
    public static class Helper
    {
        public static AspNetUsers GetTherapistAspNetUser(string therapistId, UnitOfWork context)
        {
            AspNetUsers thUser = null;

            var therapistAspNetUser = context.AspNetUsers.Find(u => !string.IsNullOrEmpty(u.TherapistAccountId) && 
                                                                u.TherapistAccountId == therapistId).ToList();
            
            if(therapistAspNetUser.Count > 0)
            {
                thUser = therapistAspNetUser.First();
            }

            return thUser;
        }
    }
}
