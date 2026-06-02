using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;

namespace TimeSight.repositories;

public interface IChoreRepository
{
    Task<ICollection<Chore>> GetChores(Guid userId);
}
