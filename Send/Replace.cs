using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Send
{
  internal static class Replace
  {
    internal static string ReplaceVariables(this string input, string caller, string target)
      => input?.Replace("${caller}", caller)
        .Replace("${player}", target);
  }
}