using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;

namespace SQLData
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MedicalContext _context = new MedicalContext();
        public void Dispose()
        {
            _context.Dispose();
        }

        public Repository<T> GetRepository<T>() where T : class
        {
            return new Repository<T>(_context);
        }

        public int SaveChanges()
        {
            TrySave:
            try
            {
                return _context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {

                foreach (var error in ex.EntityValidationErrors)
                {
                    error.Entry.State = EntityState.Detached;
                    goto TrySave;
                }
            }
            return _context.SaveChanges();
        }

        public List<T> SqlStoredProcedure<T>(string sql,  params object[] parameters)
        {
            return _context.Database.SqlQuery<T>(sql, parameters).ToList<T>();
        }
    }
}
