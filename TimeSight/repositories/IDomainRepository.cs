using TimeSight.Models;

namespace TimeSight.repositories;

public interface IDomainRepository
{
    Task<ICollection<Domain>> GetDomainsAsync();

    Task<Domain> CreateDomainAsync(Domain domain);

    Task<Domain> GetDomainAsync(Guid domainId);

}
