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
            .Where(c => c.IsDone && c.DoneAt.HasValue && IsExpired(c))
            .ToList();

        var allToReset = new HashSet<Chore>(expired);
        foreach (var chore in expired)
            foreach (var desc in GetAllDescendants(chore))
                if (desc.IsDone)
                    allToReset.Add(desc);

        await Task.WhenAll(allToReset.Select(c => SetDoneState(c, domains, false, false)));
    }

    /// <summary>
    /// "After interval" recurrences are reset 5% earlier than the displayed duration.
    /// </summary>
    private const double AfterIntervalEffectiveFactor = 0.95;

    private static bool IsExpired(Chore c)
    {
        if (!c.IsDone || !c.DoneAt.HasValue) return false;

        if (c.RecurrenceResetTime.HasValue)
        {
            var next = NextOccurrenceAfter(c.DoneAt.Value.ToLocalTime(), c.RecurrenceResetTime.Value, c.RecurrenceDaysOfWeek);
            return next <= DateTime.Now;
        }

        if (c.RecurrenceIntervalDays.HasValue)
            return (DateTime.UtcNow - c.DoneAt.Value).TotalDays >= c.RecurrenceIntervalDays.Value * AfterIntervalEffectiveFactor;

        return false;
    }

    private static DateTime NextOccurrenceAfter(DateTime after, int resetTimeMinutes, int? daysOfWeekMask)
    {
        var candidate = after.Date.AddMinutes(resetTimeMinutes);
        if (candidate <= after)
            candidate = candidate.AddDays(1);

        if (daysOfWeekMask.HasValue)
        {
            for (int i = 0; i < 7; i++)
            {
                if ((daysOfWeekMask.Value & DayOfWeekToBit(candidate.DayOfWeek)) != 0)
                    break;
                candidate = candidate.AddDays(1);
            }
        }

        return candidate;
    }

    private static int DayOfWeekToBit(DayOfWeek dow) => dow switch
    {
        DayOfWeek.Monday => 1,
        DayOfWeek.Tuesday => 2,
        DayOfWeek.Wednesday => 4,
        DayOfWeek.Thursday => 8,
        DayOfWeek.Friday => 16,
        DayOfWeek.Saturday => 32,
        DayOfWeek.Sunday => 64,
        _ => 0
    };

    private static IEnumerable<Chore> GetAllDescendants(Chore chore)
    {
        foreach (var child in chore.Children)
        {
            yield return child;
            foreach (var descendant in GetAllDescendants(child))
                yield return descendant;
        }
    }

    public async Task SetDoneState(
        Chore chore,
        ICollection<Domain> allDomains,
        bool isDone,
        bool changeDomainsScore = true)
    {
        if (changeDomainsScore)
        {
            var root = chore.GetRootOfThis();
            int descCount = root.CountAllDescendants();
            int oldDoneCount = root.CountAllDoneDescendants();
            bool oldRootIsDone = root.IsDone;

            chore.IsDone = isDone;
            await UpdateChoreAsync(chore);

            int newDoneCount = root.CountAllDoneDescendants();
            bool newRootIsDone = root.IsDone;

            var linked = GetLinkedDomains(root, allDomains);
            foreach (var domain in linked)
            {
                int raw = GetRawScore(root, domain);
                domain.DoneScore += Contribution(raw, descCount, newDoneCount, newRootIsDone)
                                  - Contribution(raw, descCount, oldDoneCount, oldRootIsDone);
            }
            await Task.WhenAll(linked.Select(domainRepository.UpdateDomainAsync));
        }
        else
        {
            chore.IsDone = isDone;
            await UpdateChoreAsync(chore);
        }
    }
    public async Task SetParent(Chore chore, Chore parent, ICollection<Domain> allDomains)
    {
        // Undo chore's own domain contribution before it loses its ChoreDomains
        if (chore.ChoreDomains.Count > 0)
            await UndoRootContribution(chore, allDomains);

        foreach (var cd in chore.ChoreDomains.ToList())
            await choreDomainRepository.DeleteChoreDomainAsync(cd.ChoreId, cd.DomainId);
        chore.ChoreDomains.Clear();

        var root = parent.GetRootOfThis();
        int oldDescCount = root.CountAllDescendants();
        int oldDoneCount = root.CountAllDoneDescendants();

        chore.ParentChoreId = parent.Id;
        chore.ParentChore = parent;
        parent.Children.Add(chore);
        await choreRepository.UpdateChoreAsync(chore);

        int newDescCount = root.CountAllDescendants();
        int newDoneCount = root.CountAllDoneDescendants();
        await AdjustRootDomainScores(root, allDomains, oldDescCount, oldDoneCount, newDescCount, newDoneCount);

        if (parent.Duration != null)
        {
            parent.Duration = null;
            await choreRepository.UpdateChoreAsync(parent);
        }
    }

    public async Task RemoveParent(Chore chore, ICollection<Domain> allDomains)
    {
        if (chore.ParentChore == null)
            throw new ArgumentException("chore doesn't have a parent");

        var root = chore.GetRootOfThis();
        int oldDescCount = root.CountAllDescendants();
        int oldDoneCount = root.CountAllDoneDescendants();

        chore.ParentChoreId = null;
        chore.ParentChore.Children.Remove(chore);
        chore.ParentChore = null;
        await choreRepository.UpdateChoreAsync(chore);

        int newDescCount = root.CountAllDescendants();
        int newDoneCount = root.CountAllDoneDescendants();
        await AdjustRootDomainScores(root, allDomains, oldDescCount, oldDoneCount, newDescCount, newDoneCount);
    }

    private async Task AdjustRootDomainScores(
        Chore root, ICollection<Domain> allDomains,
        int oldDescCount, int oldDoneCount,
        int newDescCount, int newDoneCount)
    {
        var linked = GetLinkedDomains(root, allDomains);
        if (linked.Count == 0) return;

        foreach (var domain in linked)
        {
            int raw = GetRawScore(root, domain);
            domain.DoneScore += Contribution(raw, newDescCount, newDoneCount, root.IsDone)
                              - Contribution(raw, oldDescCount, oldDoneCount, root.IsDone);
        }
        await Task.WhenAll(linked.Select(domainRepository.UpdateDomainAsync));
    }

    private async Task UndoRootContribution(Chore root, ICollection<Domain> allDomains)
    {
        var linked = GetLinkedDomains(root, allDomains);
        if (linked.Count == 0) return;

        int descCount = root.CountAllDescendants();
        int doneCount = root.CountAllDoneDescendants();

        foreach (var domain in linked)
        {
            int raw = GetRawScore(root, domain);
            domain.DoneScore -= Contribution(raw, descCount, doneCount, root.IsDone);
        }
        await Task.WhenAll(linked.Select(domainRepository.UpdateDomainAsync));
    }

    private static List<Domain> GetLinkedDomains(Chore root, ICollection<Domain> allDomains) =>
        [.. allDomains.Where(d => root.ChoreDomains.Any(cd => cd.DomainId == d.Id))];

    private static int GetRawScore(Chore root, Domain domain) =>
        root.ChoreDomains.First(cd => cd.DomainId == domain.Id!.Value).LinkIntensity;

    /// <summary>
    /// Score total attribué au domaine pour une racine donnée.
    /// Si la racine n'a pas de descendants, on retourne le score brut si elle est faite.
    /// </summary>
    private static double Contribution(int rawScore, int descCount, int doneCount, bool rootIsDone) =>
        descCount > 0 ? (double)rawScore * doneCount / descCount : (rootIsDone ? rawScore : 0);


    public async Task<Chore> CreateChoreAsync(Guid userId, Guid workspaceId, string name = "") =>
        await choreRepository.CreateChoreAsync(userId, workspaceId, name);
    public async Task<Chore> UpdateChoreAsync(Chore chore) =>
        await choreRepository.UpdateChoreAsync(chore);
    public async Task DeleteChoreAsync(Chore chore)
    {
        chore.ParentChore?.Children.Remove(chore);
        await choreRepository.DeleteChoreAsync(chore.Id);
    }
}
