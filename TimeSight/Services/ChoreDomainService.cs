using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;
using TimeSight.repositories;

namespace TimeSight.Services;

public class ChoreDomainService(IChoreDomainRepository choreDomainRepository)
{
    public async Task RemoveLink(Chore chore, Domain domain)
    {
        chore.ChoreDomains.RemoveAll(cd => cd.IsMadeOf(chore, domain));
        await choreDomainRepository.DeleteChoreDomainAsync(chore.Id!.Value, domain.Id!.Value);
    }

    public async Task AddLink(Chore chore, Domain domain)
    {
        ChoreDomain choreDomain = new ChoreDomain(chore, domain);
        chore.ChoreDomains.Add(choreDomain);
        await choreDomainRepository.CreateChoreDomainAsync(choreDomain);
    }
}
