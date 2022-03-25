using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LMSWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSWebApp.Data
{
    public class GenericRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext context;
        private readonly DbSet<TEntity> DbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            this.context = context;
            DbSet = context.Set<TEntity>();
        }

        private static int Skip(BaseSearchModel searchModel)
        {
            var skip = searchModel.Page * searchModel.RowsPerPage - searchModel.RowsPerPage;
            return skip ?? 0;
        }

        private static int Take(BaseSearchModel searchModel, out int skip)
        {
            skip = Skip(searchModel);
            int? take = searchModel.Page * searchModel.RowsPerPage - skip;
            return take ?? 0;
        }

        public virtual DataTableResponse<TEntity> GetPaginationData(BaseSearchModel searchModel, Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, params Expression<Func<TEntity, object>>[] includes)
        {
            try
            {
                IQueryable<TEntity> query = DbSet;

                if (filter != null)
                {
                    query = query.Where(filter);
                }
                if (includes.Any())
                {
                    foreach (var includeProperty in includes)
                    {
                        query = query.Include(includeProperty);
                    }
                }
                var count = query.Count();
                var take = Take(searchModel, out int skip);
                var entities = orderBy(query).Skip(skip).Take(take).ToList();

                return new DataTableResponse<TEntity>
                {
                    data = entities,
                    recordsTotal = count,
                    recordsFiltered = count
                };
            }
            catch (Exception exception)
            {
                return new DataTableResponse<TEntity>();
            }
        }

        public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, params Expression<Func<TEntity, object>>[] includes)
        {
            try
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                IQueryable<TEntity> query = DbSet;

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                foreach (var includeProperty in includes)
                {
                    query = query.Include(includeProperty);
                }
                return orderBy != null ? orderBy(query).ToList() : query.ToList();
            }

            catch (Exception exception)
            {
                return new List<TEntity>();
            }
        }

        public virtual TEntity GetById(object id, params Expression<Func<TEntity, object>>[] includes)
        {
            try
            {
                IQueryable<TEntity> query = DbSet.AsNoTracking();
                foreach (var includeProperty in includes)
                {
                    query = query.Include(includeProperty);
                }
                var entity = query.First();
                return entity;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public virtual TEntity GetByIdAsNoTracking(object id, params Expression<Func<TEntity, object>>[] includes)
        {
            try
            {

                IQueryable<TEntity> query = DbSet.AsNoTracking();
                query = includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty).AsNoTracking());
                var entity = query.First();
                context.Entry(entity).State = EntityState.Detached;
                return entity;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public virtual long? GetMaxId(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, long?>> maxBy)
        {
            try
            {
                IQueryable<TEntity> query = DbSet;
                return query.Where(filter).Max(maxBy);
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public virtual TEntity GetFirstOrDefault(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] includes)
        {
            try
            {
                IQueryable<TEntity> query = DbSet;
                foreach (var includeProperty in includes)
                {
                    query = query.Include(includeProperty);
                }
                return query.FirstOrDefault(filter);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        } 
        public virtual TEntity GetLastOrDefault(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] includes)
        {
            try
            {
                IQueryable<TEntity> query = DbSet;
                foreach (var includeProperty in includes)
                {
                    query = query.Include(includeProperty);
                }
                return query.LastOrDefault(filter);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public virtual TEntity Insert(TEntity entity)
        {
            try
            {
                DbSet.Add(entity);
                return entity;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }


        public virtual void Delete(object id)
        {
            try
            {
                var entityToDelete = DbSet.Find(id);
                if (context.Entry(entityToDelete).State == EntityState.Detached)
                {
                    DbSet.Attach(entityToDelete);
                }
                DbSet.Remove(entityToDelete);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public virtual TEntity Update(TEntity entityToUpdate)
        {
            try
            {
                DbSet.Attach(entityToUpdate);
                context.Entry(entityToUpdate).State = EntityState.Modified;
                return entityToUpdate;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public TEntity GetByIdAsNoTracking(Expression<Func<TEntity, bool>> filter)
        {
            try
            {

                IQueryable<TEntity> query = DbSet.Where(filter).AsNoTracking();
                var entity = query.First();
                context.Entry(entity).State = EntityState.Detached;
                return entity;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public void SaveChanges(bool disposeContext = false)
        {
            context.SaveChanges();
            if (disposeContext)
            {
                context.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
