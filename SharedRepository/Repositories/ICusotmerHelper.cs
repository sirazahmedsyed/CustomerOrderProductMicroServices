using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedRepository.Repositories
{
    public interface ICusotmerHelper
    {
        Task<bool> CustomerExistsAsync(Guid customerId);
    }
}
