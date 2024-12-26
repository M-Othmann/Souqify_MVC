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
    public class OrderHeaderReposiotry : Repository<OrderHeader>, IOrderHeaderReposiotry
    {
        private readonly AppDbContext _db;

        public OrderHeaderReposiotry(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public void update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }
    }
}
