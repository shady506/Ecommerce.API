﻿using  Ecommerce.API.Models;
using  Ecommerce.API.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace  Ecommerce.API.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private ApplicationDbContext _context;// = new();
        private DbSet<T> _db;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _db = _context.Set<T>();
        }

        // CRUD
        public async Task CreateAsync(T entity)
        {
            await _db.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _db.Update(entity);
        }

        public void Delete(T entity)
        {
            _db.Remove(entity);
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<T>> GetAsync(Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null, bool tracked = true)
        {
            var entities = _db.AsQueryable();

            if (expression is not null)
            {
                entities = entities.Where(expression);
            }

            if (includes is not null)
            {
                foreach (var item in includes)
                {
                    entities = entities.Include(item);
                }
            }

            if (!tracked)
            {
                entities = entities.AsNoTracking();
            }

            return await entities.ToListAsync();
        }

        public async Task<T?> GetOneAsync(Expression<Func<T, bool>> expression, Expression<Func<T, object>>[]? includes = null, bool tracked = true)
        {
            return (await GetAsync(expression, includes, tracked)).FirstOrDefault();
        }
    }
}
