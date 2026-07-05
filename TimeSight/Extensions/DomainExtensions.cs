using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;
using TimeSight.SupabaseClient.Models;

namespace TimeSight.Extensions;

public static class DomainExtensions
{
    public static SupabaseDomain ToSupabaseDomain(this Domain domain)
    {
        return new()
        {
            Id = domain.Id,
            UserId = domain.UserId,
            WorkspaceId = domain.WorkspaceId,
            Name = domain.Name,
            Color = domain.Color,
            Description = domain.Description,
            DoneScore = domain.DoneScore.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
    }
}
