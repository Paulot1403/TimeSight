using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeSight.SupabaseClient.Models;

/// <summary>
/// 
/// </summary>
/// <param name="SignInUri"> Uri à appeler pour obtenir le authorization code</param>
/// <param name="PKCEVerifier"></param>
public record SignInInfo(Uri SignInUri, string PKCEVerifier)
{
}
