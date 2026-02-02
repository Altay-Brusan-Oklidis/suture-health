using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SutureHealth.Patients.Services.AdmitDischargeTransfer
{
    public class Kno2DbUnitOfWork: IDisposable
    {
        private readonly ConcurrentDictionary<Type, object> _repositories = new();
        private readonly Kno2DbContext _context;
        private bool _disposed = false;

        public Kno2DbUnitOfWork(Kno2DbContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IKno2DbRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);

            if (!_repositories.TryGetValue(type, out var repository))
            {
                repository = Activator.CreateInstance(typeof(Kno2DbRepository<TEntity>), _context);
                _repositories[type] = repository;
            }

            return (IKno2DbRepository<TEntity>)repository;
        }

        public int Save() => _context.SaveChanges();

        public Task<int> SaveAsync() => _context.SaveChangesAsync();

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }

            _disposed = true;
        }
    }
}
