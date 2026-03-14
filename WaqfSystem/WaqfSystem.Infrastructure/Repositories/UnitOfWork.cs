using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Interfaces;
using WaqfSystem.Infrastructure.Data;

namespace WaqfSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly WaqfDbContext _context;
        private IDbContextTransaction? _transaction;
        private IPropertyRepository? _properties;

        public UnitOfWork(WaqfDbContext context)
        {
            _context = context;
        }

        public IPropertyRepository Properties =>
            _properties ??= new PropertyRepository(_context);

        public async Task<T> AddAsync<T>(T entity) where T : class
        {
            var entry = await _context.Set<T>().AddAsync(entity);
            return entry.Entity;
        }

        public Task UpdateAsync<T>(T entity) where T : class
        {
            _context.Set<T>().Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync<T>(T entity) where T : class
        {
            _context.Set<T>().Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<T?> GetByIdAsync<T>(int id) where T : class
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public IQueryable<T> GetQueryable<T>() where T : class
        {
            return _context.Set<T>().AsQueryable();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
