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
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private AppDbContext _db;

        public ApplicationUserRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
