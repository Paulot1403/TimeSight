using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;
using TimeSight.Models.Order;

namespace TimeSight.Services;

public class OrderChoresService
{
    public List<Chore> OrderChores(List<Chore> chores, List<Domain> domains)
    {
        Dictionary<Guid, Chore> choresDic = chores.ToDictionary(
            c => (Guid)c.Id,
            c => c);
        Dictionary<Guid, Domain> domainsDic = domains.ToDictionary(
             d => (Guid)d.Id,
             d => d);
        List<ChoreDomain> choreDomains = [.. chores.SelectMany(c => c.ChoreDomains)];

        List<ChorePriorityForDomain> prioritiesForDomains = [ .. choreDomains.Select(cd =>
        {
            Chore chore = choresDic.GetValueOrDefault(cd.ChoreId);
            Domain domain = domainsDic.GetValueOrDefault(cd.DomainId);
            int doneScore = chore.GetScoreForDomain(domain);
            int priorityScore = GetPriorityScore(chore,domain,doneScore);

            return new ChorePriorityForDomain()
            {
                ChoreId = cd.ChoreId,
                DomainId = cd.DomainId,
                Priority = priorityScore,
                DoneScore=doneScore
            };
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

        return [.. sortedChores.Select(s => choresDic.GetValueOrDefault(s))];
    }

    private static int GetPriorityScore(Chore chore, Domain domain, int doneScore)
    {
        return doneScore + (Chore.MAX_DURATION - chore.Duration);
    }

}
