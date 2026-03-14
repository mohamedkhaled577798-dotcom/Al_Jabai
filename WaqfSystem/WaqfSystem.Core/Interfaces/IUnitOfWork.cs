using System;
using System.Threading.Tasks;
using WaqfSystem.Core.Entities;

namespace WaqfSystem.Core.Interfaces
{
    /// <summary>
    /// Unit of Work interface for transaction management.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IPropertyRepository Properties { get; }
        
        // Generic repository access for child entities
        Task<T> AddAsync<T>(T entity) where T : class;
        Task UpdateAsync<T>(T entity) where T : class;
        Task DeleteAsync<T>(T entity) where T : class;
        Task<T?> GetByIdAsync<T>(int id) where T : class;
        IQueryable<T> GetQueryable<T>() where T : class;

        // Transaction
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
