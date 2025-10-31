using DogsService.Domain.Interfaces;
using DogsService.Infrastructure.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogsService.Infrastructure.UnitOfWork
{
    public class UnitOfWork(
        AppDbContext context,
        IDogRepository dogRepository
        ) : IUnitOfWork
    {
        public IDogRepository Dogs => dogRepository;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            await context.SaveChangesAsync(cancellationToken);
    }
}
