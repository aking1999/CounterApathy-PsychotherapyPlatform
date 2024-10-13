using Database.Models;
using Database.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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

        public bool Any(Expression<Func<DatabaseEntity, bool>> expression)
        {
            return _context.Set<DatabaseEntity>().Any(expression);
        }

        public async Task<bool> AnyAsync(Expression<Func<DatabaseEntity, bool>> expression)
        {
            return await _context.Set<DatabaseEntity>().AnyAsync(expression);
        }

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
            return _context.Set<DatabaseEntity>().AsNoTracking().Count();
        }

        public IQueryable<DatabaseEntity> NoTracking()
        {
            return _context.Set<DatabaseEntity>().AsNoTracking();
        }

        public IEnumerable<DatabaseEntity> ReadOnlyGetAll()
        {
            return _context.Set<DatabaseEntity>().AsNoTracking().ToList();
        }

        public bool ReadOnlyAny(Expression<Func<DatabaseEntity, bool>> expression)
        {
            return _context.Set<DatabaseEntity>().AsNoTracking().Any(expression);
        }

        public async Task<bool> ReadOnlyAnyAsync(Expression<Func<DatabaseEntity, bool>> expression)
        {
            return await _context.Set<DatabaseEntity>().AsNoTracking().AnyAsync(expression);
        }

        public IEnumerable<DatabaseEntity> ReadOnlyFind(Expression<Func<DatabaseEntity, bool>> expression)
        {
            return _context.Set<DatabaseEntity>().AsNoTracking().Where(expression);
        }
    }
}
