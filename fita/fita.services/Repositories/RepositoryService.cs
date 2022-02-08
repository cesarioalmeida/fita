using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LiteDB;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class RepositoryService<T> : IRepositoryService<T> where T : class
    {
        private readonly List<Expression<Func<T, object>>> _expressionsToInclude = GetExpressionsToInclude();

        protected RepositoryService(IDBHelperService dbHelperService, ILoggingService loggingService)
        {
            DBHelperService = dbHelperService;
            LoggingService = loggingService;
        }
        
        public IDBHelperService DBHelperService { get; }

        public ILoggingService LoggingService { get; }

        public ILiteCollection<T> Collection => DBHelperService.DB.GetCollection<T>();

        public virtual void IndexData()
        {
        }

        public virtual Task<Result> SaveAsync(T model)
        {
            return Task.Run(
                () =>
                {
                    if (model == null)
                    {
                        return Result.Fail;
                    }

                    try
                    {
                        Collection.Upsert(model);
                        return Result.Success;
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(SaveAsync)}: {ex}");
                        return Result.Fail;
                    }
                });
        }

        public virtual Task<Result> DeleteAsync(ObjectId id)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection.Delete(id) ? Result.Success : Result.Fail;
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(DeleteAsync)}: {ex}");
                        return Result.Fail;
                    }
                });
        }

        public virtual Task<T> DetailsAsync(ObjectId id)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection.FindById(id);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(DetailsAsync)}: {ex}");
                        return null;
                    }
                });
        }

        public virtual Task<T> DetailsEnrichedAsync(ObjectId id)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        var collection = _expressionsToInclude
                            .Aggregate(Collection, (current, expression) => current.Include(expression));
                        return collection.FindById(id);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(DetailsEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
        }

        public virtual Task<IEnumerable<T>> AllAsync()
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection.FindAll();
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(AllAsync)}: {ex}");
                        return null;
                    }
                });
        }

        public virtual Task<IEnumerable<T>> AllEnrichedAsync()
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        var collection = _expressionsToInclude
                            .Aggregate(Collection, (current, expression) => current.Include(expression));
                        return collection.FindAll();
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(AllEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
        }

        private static List<Expression<Func<T, object>>> GetExpressionsToInclude()
        {
            var result = new List<Expression<Func<T, object>>>();
            
            var propertiesRequireLoading = typeof(T).GetProperties().Where(x => Attribute.IsDefined(x, typeof(BsonRefAttribute)));
            
            foreach (var propertyInfo in propertiesRequireLoading)
            {
                var entityType = propertyInfo.DeclaringType;
                var parameter = Expression.Parameter(entityType!, "x");
                var property = Expression.Property(parameter, propertyInfo);
                var conversion = Expression.Convert(property, typeof(object));
                result.Add(Expression.Lambda<Func<T, object>>(conversion, parameter));
            }

            return result;
        }
    }
}