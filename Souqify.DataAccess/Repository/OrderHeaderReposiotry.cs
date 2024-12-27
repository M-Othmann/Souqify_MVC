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

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(o => o.Id == id);

            if (orderFromDb is not null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                    orderFromDb.PaymentStatus = paymentStatus;
            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(o => o.Id == id);

            if (!string.IsNullOrEmpty(sessionId))
                orderFromDb.SessionId = sessionId;

            if (!string.IsNullOrEmpty(paymentIntentId))
            {

                orderFromDb.PaymentIntentId = paymentIntentId;
                orderFromDb.PaymentDate = DateTime.Now;

            }
        }
    }
}
