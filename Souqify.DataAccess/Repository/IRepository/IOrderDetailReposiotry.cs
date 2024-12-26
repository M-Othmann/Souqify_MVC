using Souqify.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Souqify.DataAccess.Repository.IRepository
{
    public interface IOrderDetailReposiotry : IRepository<OrderDetail>
    {
        void update(OrderDetail obj);
    }
}
