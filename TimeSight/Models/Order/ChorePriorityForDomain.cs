using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeSight.Models.Order;

/// <summary>
/// 
/// </summary>
/// <param name="ChoreId"></param>
/// <param name="DomainId"></param>
/// <param name="Priority"></param>
/// <param name="DoneScore">Score ajouté au domaine quand la tâche est effectuée</param>
public record struct ChorePriorityForDomain(
    Guid ChoreId,
    Guid DomainId,
    int Priority,
    int DoneScore)
{
}
