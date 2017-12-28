using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace SQLData
{
    public interface IRepository<TEntity> where TEntity : class
    {
        TEntity GetById(int id);
        bool Contains(TEntity enity);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> GetAllLocal();
        ObservableCollection<TEntity> GetAllToObservableCollection();
        ObservableCollection<TEntity> GetAllToObservableCollectionWithReletedEntitys(string[] TablesToInclude);
        ObservableCollection<TEntity> RefrashAllToObservableCollection();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        bool RecordExits(Expression<Func<TEntity, bool>> predicate);

        TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate);

        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);

        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
        IEnumerable<object> GetChangedEntries();
        bool CheckIfTheEntityHasChanged();
        
    }
}
