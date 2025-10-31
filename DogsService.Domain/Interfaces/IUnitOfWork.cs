using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogsService.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IDogRepository Dogs { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    }
}
