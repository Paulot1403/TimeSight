using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;
using TimeSight.repositories;

namespace TimeSight.Services;

public class ChoreService(
    IChoreRepository choreRepository,
    IChoreDomainRepository choreDomainRepository,
    IDomainRepository domainRepository)
{
    public async Task<IDictionary<Guid, Chore>> GetChoresAsync(Guid workspaceId)
    {
        List<Chore> chores = await choreRepository.GetChoresAsync(workspaceId);

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

    public async Task ResetExpiredRecurringChoresAsync(ICollection<Chore> chores, ICollection<Domain> domains)
    {
        var expired = chores
            .Where(c => c.RecurrenceIntervalDays.HasValue
                        && c.IsDone
                        && c.DoneAt.HasValue
                        && (DateTime.UtcNow - c.DoneAt.Value).TotalDays >= c.RecurrenceIntervalDays.Value)
            .ToList();

        foreach (var chore in expired)
            await SetDoneState(chore, domains, false, false);
    }

    public async Task SetDoneState(
        Chore chore,
        ICollection<Domain> allDomains,
        bool isDone,
        bool changeDomainsScore = true)
    {
        chore.IsDone = isDone;
        await UpdateChoreAsync(chore);

        if (changeDomainsScore)
        {
            var associatedDomains = allDomains.Where(d => chore.ChoreDomains.Any(cd => cd.DomainId == d.Id));

            foreach (var domain in associatedDomains)
            {
                int score = chore.GetScoreForDomain(domain);
                if (isDone)
                {
                    domain.DoneScore += score;
                }
                else
                {
                    domain.DoneScore -= score;
                }
                await domainRepository.UpdateDomainAsync(domain);
            }
        }

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

    public async Task<Chore> CreateChoreAsync(Guid userId, Guid workspaceId) =>
        await choreRepository.CreateChoreAsync(userId, workspaceId);
    public async Task<Chore> UpdateChoreAsync(Chore chore) =>
        await choreRepository.UpdateChoreAsync(chore);
    public async Task DeleteChoreAsync(Chore chore)
    {
        chore.ParentChore?.Children.Remove(chore);
        await choreRepository.DeleteChoreAsync(chore.Id);
    }
}
