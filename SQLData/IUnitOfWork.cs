using System;
using System.Collections.Generic;

namespace SQLData
{
    public interface IUnitOfWork : IDisposable
    {
        Repository<T> GetRepository<T>() where T : class;
        int SaveChanges();
        List<T> SqlStoredProcedure<T>(string sql, params object[] parameters);
    }
}
