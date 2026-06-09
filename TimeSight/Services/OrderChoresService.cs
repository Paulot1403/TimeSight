using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;
using TimeSight.Models.Order;

namespace TimeSight.Services;

public class OrderChoresService
{
    public List<Guid> OrderChores(IDictionary<Guid, Chore> choresDic, List<Domain> domains)
    {
        Dictionary<Guid, Domain> domainsDic = domains.ToDictionary(
             d => (Guid)d.Id,
             d => d);

        List<ChorePriorityForDomain> prioritiesForDomains = [ .. choresDic.Values.SelectMany(originalChore =>
        {
            var choreDomainsAssociated = originalChore.GetRootOfThis().ChoreDomains;

            return choreDomainsAssociated.Select(cd=>{
                Domain domain = domainsDic.GetValueOrDefault(cd.DomainId);
                int doneScore = originalChore.GetScoreForDomain(domain);
                int priorityScore = GetPriorityScore(originalChore,domain,doneScore);

                return new ChorePriorityForDomain()
                {
                    ChoreId = originalChore.Id,
                    DomainId = cd.DomainId,
                    Priority = priorityScore,
                    DoneScore = doneScore
                };
            });

        })];

        List<DomainScoreComputation> domainsScore = [.. domains.Select(d =>
        {
            return new DomainScoreComputation(){
                DomainId=(Guid)d.Id,
                CurrentScore= d.CurrentDoneScore
            };
        })];

        List<Guid> sortedChores = [];


        while (prioritiesForDomains.Count != 0 && domainsScore.Count != 0)
        {
            DomainScoreComputation leastDoneDomain = domainsScore.MinBy(d => d.CurrentScore)!;
            // TODO  : manage case when no chores is done with this domain and add one to say that this domain needs tasks

            ICollection<ChorePriorityForDomain> prioritiesForThisDomain = [.. prioritiesForDomains
                .Where(s => s.DomainId == leastDoneDomain.DomainId)];
            if (prioritiesForThisDomain.Count == 0)
            {
                domainsScore.Remove(leastDoneDomain);
                continue;
            }

            ChorePriorityForDomain maximumPriorityForThisDomain = prioritiesForThisDomain.MaxBy(s => s.Priority);
            Guid choreIdToDo = maximumPriorityForThisDomain.ChoreId;
            sortedChores.Add(choreIdToDo);
            ICollection<ChorePriorityForDomain> prioritiesForThisChore = [.. prioritiesForDomains.Where(s => s.ChoreId == choreIdToDo)];
            foreach (var priority in prioritiesForThisChore)
            {
                //update score of domain
                domainsScore.First(d => d.DomainId == priority.DomainId).CurrentScore += priority.DoneScore;
                prioritiesForDomains.Remove(priority);
            }
        }

        List<Guid> missingChoresId = [.. choresDic.Keys.Where(id => !sortedChores.Contains(id))];
        sortedChores.AddRange(missingChoresId);

        return sortedChores;
    }

    private static int GetPriorityScore(Chore chore, Domain domain, int doneScore)
    {
        return doneScore + (Chore.MAX_DURATION - chore.Duration);
    }

}
