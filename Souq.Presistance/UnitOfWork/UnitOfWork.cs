using Core.BaseRepository;
using Core.Entities;
using Core.UOW;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Souq.Presistance.Entities;
using Souq.Presistance.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Souq.Presistance.UnitOfWork;

public class UnitOfWork<T>(DbContext dbContext, 
    RoleRepository roleRepository) 
    : IUnitOfWork<T> where T : class, IBaseEntity
{
    public DbContext DbContext => dbContext;

    public IBaseRepository<T> Repository => new BaseRepository<T>(DbContext);

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    private readonly Dictionary<Type, object> repos = new()
    {
        { typeof(Role), roleRepository }
    };

    public TRepo Get<TRepo>() => (TRepo)repos[typeof(TRepo)];

    public async Task<int> SaveChangesAsync()
    {
        return await dbContext.SaveChangesAsync();
    }

    public Task UpdateValue(Expression<Func<T, bool>> exp, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls)
    {
        throw new NotImplementedException();
    }
}
