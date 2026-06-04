using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;

namespace TimeSight.Services;

public class OrderChoresService
{
    public List<Chore> OrderChores(List<Chore> chores, List<Domain> domains)
    {

        foreach (var domain in domains)
        {
            AddChoreIfMissingForDomain(chores, domain);
        }

        Chore firstChore = null;
        Dictionary<Guid, List<Chore>> orderedChores = domains.ToDictionary(
            d => d.Id.Value,
            d => OrderChoresInDomain(chores, d));
        List<Domain> orderedDomains = domains;

        List<Chore> sortedChores = new();
        do
        {
            orderedDomains = orderedDomains.OrderBy(d => d.CurrentDoneScore)
                .ToList();

            firstChore = GetFirstChoreAndRemove(orderedChores, orderedDomains);
            if (firstChore == null)
            {
                break;
            }
            foreach (var domain in domains)
            {

                domain.CurrentDoneScore += GetScoreThatGivesChoreForDomain(firstChore, domain);
            }
            sortedChores.Add(firstChore);
        } while (firstChore != null);




        return sortedChores;
    }

    private static Chore? GetFirstChoreAndRemove(
        Dictionary<Guid, List<Chore>> orderedChores,
         List<Domain> orderedDomains)
    {
        if (orderedDomains.Count == 0)
        {
            return null;
        }

        if (orderedChores.TryGetValue(orderedDomains.First().Id.Value, out List<Chore> choresOfFirstDomain))
        {
            if (choresOfFirstDomain.Count == 0)
            {
                orderedDomains.RemoveAt(0);
                return GetFirstChoreAndRemove(orderedChores, orderedDomains);
            }

            var firstChoreToDo = choresOfFirstDomain.FirstOrDefault();
            foreach (var choresList in orderedChores.Values)
                choresList.RemoveAll(c => c.Id == firstChoreToDo.Id);
            return firstChoreToDo;
        }
        else
        {
            orderedDomains.RemoveAt(0);
            return GetFirstChoreAndRemove(orderedChores, orderedDomains);
        }
    }

    private void AddChoreIfMissingForDomain(List<Chore> chores, Domain domain)
    {
        if (chores.Any(c => c.ChoreDomains.Any(cd => cd.IsMadeOf(c, domain))))
        {
            //TODO : add link
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="domain"></param>
    /// <returns> Most important chores first of domain</returns>
    private List<Chore> OrderChoresInDomain(List<Chore> chores, Domain domain)
    {
        return chores
            .Where(c => c.ChoreDomains.Any(cd => cd.IsMadeOf(c, domain)))
            .OrderByDescending(c => GetScoreThatGivesChoreForDomain(c, domain))
            .ToList();
    }

    private int GetScoreThatGivesChoreForDomain(Chore chore, Domain domain)
    {
        ChoreDomain? cd = chore.ChoreDomains.FirstOrDefault(c => c.IsMadeOf(chore, domain));
        if (cd == null)
            return 0;

        return cd.LinkIntensity * chore.Duration * chore.Significance;
    }
}
