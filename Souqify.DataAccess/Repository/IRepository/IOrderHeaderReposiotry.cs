﻿using Souqify.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Souqify.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderReposiotry : IRepository<OrderHeader>
    {
        void update(OrderHeader obj);
    }
}