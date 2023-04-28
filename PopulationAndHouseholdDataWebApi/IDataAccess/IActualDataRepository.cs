﻿using EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDataAccess
{
    public interface IActualDataRepository : IGenericRepository<ActualDataEntity>
    {
        Task<IEnumerable<ActualDataEntity>> LoadByStateAsync();
    }
}
