using Database.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Database.RepositoryInterfaces
{
    public interface IRepository<DatabaseEntity> where DatabaseEntity : class
    {
        DatabaseEntity GetById(string id);
        IEnumerable<DatabaseEntity> GetAll();
       // IEnumerable<TherapistApplications> GetTherapistApplicationsWithAspNetUsers();
        IEnumerable<DatabaseEntity> Find(Expression<Func<DatabaseEntity, bool>> expression);
        //IEnumerable<TherapistApplications> FindTherapistApplicationsWithAspNetUsers(Expression<Func<TherapistApplications, bool>> expression);

        void Insert(DatabaseEntity entity);
        void Update(DatabaseEntity entity);
        void Delete(DatabaseEntity entity);
        int CountEntities();
    }
}
