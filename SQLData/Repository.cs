using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace SQLData
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private  static DbContext _context;
        private readonly DbSet<TEntity> _dbSet;
        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }
        #region PublicMethods
        public TEntity GetById(int id)
        {
            return _dbSet.Find(id);
        }
        public bool Contains(TEntity entity)
        {
            return _dbSet.Local.Contains(entity);
        }
        public IEnumerable<TEntity> GetAll()
        {
            return _dbSet.ToList();
        }
        public IEnumerable<TEntity> GetAllLocal()
        {
            return _dbSet.Local.ToList();
        }
        public ObservableCollection<TEntity> GetAllToObservableCollection()
        {
            _dbSet.Load();
            ObservableCollection<TEntity> observableCollection = _dbSet.Local;
            return observableCollection;
        }
        public ObservableCollection<TEntity> GetAllToObservableCollectionWithReletedEntitys(string[] entitysToInclude)
        {
            Get<TEntity>(entitysToInclude);
            ObservableCollection<TEntity> observableCollection = _dbSet.Local;
            return observableCollection;
        }
        public ObservableCollection<TEntity> RefrashAllToObservableCollection()
        {
            var refreshableObjects = (from entry in ((IObjectContextAdapter)_context).ObjectContext.ObjectStateManager.GetObjectStateEntries(
                                                            EntityState.Deleted
                                                          | EntityState.Modified
                                                          | EntityState.Unchanged)
                                      where entry.EntityKey != null
                                      select entry.Entity);

            ((IObjectContextAdapter)_context).ObjectContext.Refresh(RefreshMode.StoreWins, refreshableObjects);
            ObservableCollection<TEntity> observableCollection = _dbSet.Local;
            return observableCollection;

        }
        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }
        public bool RecordExits(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.Any(predicate);
        }
        public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.SingleOrDefault(predicate);
        }
        public void Add(TEntity entity)
        {
            if (entity != null)
                _dbSet.Add(entity);
        }
        public void AddRange(IEnumerable<TEntity> entities)
        {
            if (entities != null)
                _dbSet.AddRange(entities);
        }
        public void Remove(object id)
        {
            TEntity entityToDelete = _dbSet.Find(id);
            if (entityToDelete != null)
                _dbSet.Remove(entityToDelete);
        }
        public void Remove(TEntity entityToDelete)
        {
            if (entityToDelete != null)
            {
                if (_context.Entry(entityToDelete).State == EntityState.Detached)
                {
                    _dbSet.Attach(entityToDelete);
                }
                _dbSet.Remove(entityToDelete);
            }
        }
        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            if (entities != null)
                foreach (var entity in entities)
                {
                    Remove(entity);
                }
        }
        public IEnumerable<object> GetChangedEntries()
        {
            return _context.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged).Select(e => e.Entity);
        }
        public bool CheckIfTheEntityHasChanged()
        {

            var allReletedChangedList = GetChangedEntries();
            return allReletedChangedList.Count() > 0;
        }
        public IList<TEntity> Get<TParamater>(string[] includeProperties)
        {
            var query = _dbSet.AsQueryable();
            foreach (var include in includeProperties)
            {
                query = query.Include(include.ToString());
            }
            return query.ToList();
        }
       
        #endregion


    }
}
