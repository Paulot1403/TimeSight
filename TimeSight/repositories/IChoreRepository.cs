using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;

namespace TimeSight.repositories;

public interface IChoreRepository
{
    Task<List<Chore>> GetChoresAsync(Guid workspaceId);

    Task<Chore> CreateChoreAsync(Guid userId, Guid workspaceId, string name = "");

    Task<Chore> UpdateChoreAsync(Chore chore);


    Task DeleteChoreAsync(Guid choreId);
}
