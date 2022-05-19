using Database.Models;
using Database.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Database.RepositoryImplementations
{
    public class Repository<DatabaseEntity> : IRepository<DatabaseEntity>
        where DatabaseEntity : class
    {
        protected readonly LajsnaProbaContext _context;

        public Repository(LajsnaProbaContext context)
        {
            _context = context;
        }

        public DatabaseEntity GetById(string id)
        {
            return _context.Set<DatabaseEntity>().Find(id);
        }

        public IEnumerable<DatabaseEntity> GetAll()
        {
            return _context.Set<DatabaseEntity>().ToList();
        }

        //ovo da obrisem posle mozda jer ga ne koristim
        /*public IEnumerable<DatabaseEntity> GetAll(List<string> includes)
        {
            var query = _context.Set<DatabaseEntity>().AsQueryable();

            foreach (string include in includes)
            {
                query = query.Include(include);
            }

            return query.ToList();
        }*/

        public IEnumerable<DatabaseEntity> Find(Expression<Func<DatabaseEntity, bool>> expression)
        {
            return _context.Set<DatabaseEntity>().Where(expression);
        }

        public void Insert(DatabaseEntity entity)
        {
            _context.Set<DatabaseEntity>().Add(entity);
        }

        public void Update(DatabaseEntity entity)
        {
            _context.Set<DatabaseEntity>().Update(entity);
        }

        public void Delete(DatabaseEntity entity)
        {
            _context.Set<DatabaseEntity>().Remove(entity);
        }

        public int CountEntities()
        {
            return _context.Set<DatabaseEntity>().Count();
        }

        /*public IEnumerable<TherapistApplications> GetTherapistApplicationsWithAspNetUsers()
        {
            return _context.TherapistApplications.Include(inc => inc.User).ToList();
        }*/

        /*public IEnumerable<TherapistApplications> FindTherapistApplicationsWithAspNetUsers(Expression<Func<TherapistApplications, bool>> expression)
        {
            return _context.TherapistApplications.Where(expression).Include(app => app.User);
        }*/
    }
}
