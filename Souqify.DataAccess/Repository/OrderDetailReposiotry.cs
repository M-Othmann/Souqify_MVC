using Souqify.Data;
using Souqify.DataAccess.Repository.IRepository;
using Souqify.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Souqify.DataAccess.Repository
{
    public class OrderDetailReposiotry : Repository<OrderDetail>, IOrderDetailReposiotry
    {
        private readonly AppDbContext _db;

        public OrderDetailReposiotry(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public void update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
        }
    }
}
