using Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Database.RepositoryInterfaces
{
    public interface IRepository<DatabaseEntity> where DatabaseEntity : class
    {
        DatabaseEntity GetById(string id);
        IEnumerable<DatabaseEntity> GetAll();
        bool Any(Expression<Func<DatabaseEntity, bool>> expression);
        Task<bool> AnyAsync(Expression<Func<DatabaseEntity, bool>> expression);
        IEnumerable<DatabaseEntity> Find(Expression<Func<DatabaseEntity, bool>> expression);
        void Insert(DatabaseEntity entity);
        void Update(DatabaseEntity entity);
        void Delete(DatabaseEntity entity);
        int CountEntities();

        IQueryable<DatabaseEntity> NoTracking();

        IEnumerable<DatabaseEntity> ReadOnlyGetAll();
        bool ReadOnlyAny(Expression<Func<DatabaseEntity, bool>> expression);
        Task<bool> ReadOnlyAnyAsync(Expression<Func<DatabaseEntity, bool>> expression);
        IEnumerable<DatabaseEntity> ReadOnlyFind(Expression<Func<DatabaseEntity, bool>> expression);
    }
}
