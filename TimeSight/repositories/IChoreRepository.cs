using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;

namespace TimeSight.repositories;

public interface IChoreRepository
{
    Task<List<Chore>> GetChoresAsync();

    Task<Chore> CreateChoreAsync(Guid userId, String name = "");

    Task<Chore> UpdateChoreAsync(Chore chore);


    Task DeleteChoreAsync(Guid choreId);
}
