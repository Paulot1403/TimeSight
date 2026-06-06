using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeSight.Models.Order;

public record struct ChoreScoreForDomain(Guid ChoreId, Guid DomainId, int Score)
{
}
