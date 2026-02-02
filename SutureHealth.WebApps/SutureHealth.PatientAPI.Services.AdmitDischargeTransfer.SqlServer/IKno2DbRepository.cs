using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SutureHealth.Patients.Services.AdmitDischargeTransfer
{
    public interface IKno2DbRepository<TEntity> where TEntity : class
    {
        void Delete(TEntity entityToDelete);
        IQueryable<TEntity> GetAsQueryable();
        IEnumerable<TEntity> Get();
        IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter);
        IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy);
        IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy, string includeProperties);
        TEntity GetByID(object id);
        void Insert(TEntity entity);
        void Update(TEntity entityToUpdate);
        void Delete(object id);
    }
}
