using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SutureHealth.Patients.Services.AdmitDischargeTransfer
{
    public class Kno2DbRepository<TEntity> : IKno2DbRepository<TEntity> where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;
        private readonly DbContext _context;

        public Kno2DbRepository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<TEntity>();
        }

        public virtual IEnumerable<TEntity> Get() =>
            Get(null);

        public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter) =>
            Get(filter, null);

        public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy) =>
            Get(filter, orderBy, string.Empty);

        public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy, string includeProperties)
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return query.ToArray();
        }

        public virtual TEntity GetByID(object id) =>
            _dbSet.Find(id);

        public virtual void Insert(TEntity entity) =>
            _dbSet.Add(entity);

        public virtual void Delete(object id) =>
            Delete(_dbSet.Find(id));

        public virtual void Delete(TEntity entityToDelete)
        {
            if (_context.Entry(entityToDelete).State == EntityState.Detached)
            {
                _ = _dbSet.Attach(entityToDelete);
            }

            _ = _dbSet.Remove(entityToDelete);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            _context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public IQueryable<TEntity> GetAsQueryable()
        {
            return _dbSet.AsQueryable();
        }
    }
}
