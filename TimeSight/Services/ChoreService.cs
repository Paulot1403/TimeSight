using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;
using TimeSight.repositories;

namespace TimeSight.Services;

public class ChoreService(
    IChoreRepository choreRepository,
    IChoreDomainRepository choreDomainRepository)
{
    public async Task<IDictionary<Guid, Chore>> GetChoresAsync()
    {
        List<Chore> chores = await choreRepository.GetChoresAsync();

        Dictionary<Guid, Chore> choresDic = chores.ToDictionary(
            c => c.Id,
            c => c);

        chores.ForEach(c =>
        {
            if (c.ParentChoreId != null)
            {
                if (!choresDic.TryGetValue(c.ParentChoreId.Value, out var parentChore))
                {
                    throw new Exception("If chore as parentChoreId, we should be able to find its parent chore");
                }
                c.ParentChore = parentChore;
                parentChore.Children.Add(c);
            }
        });
        return choresDic;
    }
    public async Task SetParent(Chore chore, Chore parent)
    {
        foreach (var cd in chore.ChoreDomains.ToList())
            await choreDomainRepository.DeleteChoreDomainAsync(cd.ChoreId, cd.DomainId);
        chore.ChoreDomains.Clear();
        chore.ParentChoreId = parent?.Id;
        chore.ParentChore = parent;
        parent?.Children.Add(chore);
        await choreRepository.UpdateChoreAsync(chore);
    }

    public async Task RemoveParent(Chore chore)
    {
        if (chore.ParentChore == null)
        {
            throw new ArgumentException("chore doesn't have a parent");
        }
        chore.ParentChoreId = null;
        chore.ParentChore.Children.Remove(chore);
        chore.ParentChore = null;
        await choreRepository.UpdateChoreAsync(chore);
    }

    public async Task<Chore> CreateChoreAsync(Guid userId) =>
        await choreRepository.CreateChoreAsync(userId);
    public async Task<Chore> UpdateChoreAsync(Chore chore) =>
        await choreRepository.UpdateChoreAsync(chore);
    public async Task DeleteChoreAsync(Chore chore)
    {
        chore.ParentChore?.Children.Remove(chore);
        await choreRepository.DeleteChoreAsync(chore.Id);
    }
}
