using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace Send
{
  [ApiVersion(2, 1)]
  public class Send : TerrariaPlugin
  {
    private static Regex ColorRegex = new Regex(@"(\d{1,3}), ?(\d{1,3}), ?(\d{1,3})", RegexOptions.Compiled);

    public Send(Main game) : base(game)
    {
    }

    public override void Initialize()
    {
      Commands.ChatCommands.Add(new Command("send", SendCommand, "send"));

      Commands.ChatCommands.Add(new Command("send.broadcast", BroadcastCommand, "sendbc"));
      Commands.ChatCommands.Add(new Command("send.broadcast.impersonate", ImpersonateCommand, "sendas"));
    }

    private static IEnumerable<TSPlayer> GetTargetsByName(string name)
      => TShock.Players.Where(p => p?.Name.IndexOf(name, StringComparison.InvariantCultureIgnoreCase) >= 0);

/*
    private static string GetImpersonatedString(User user, string text)
    {
      var group = TShock.Groups.GetGroupByName(user.Group);
    
      return string.Format(TShock.Config.ChatFormat, group.Name, group.Prefix, user.Name, group.Suffix, text);
    }
*/

/*
    private static IEnumerable<TSPlayer> GetTargetsByRegion(Region region)
      => TShock.Players.Where(p => p?.CurrentRegion == region);
*/

/*
    private static TSPlayer GetTSPlayerFromUser(User user) =>
      TShock.Players.FirstOrDefault(p => p?.Name.Equals(user.Name, StringComparison.InvariantCultureIgnoreCase) ?? false);
*/

    private static string GetImpersonatedString(TSPlayer p, string text)
      => string.Format(TShock.Config.ChatFormat, p.Group.Name, p.Group.Prefix, p.Name, p.Group.Suffix, text);

    private static bool InvariantContains(string input, string substr)
    {
      if (input == null || substr == null) return false;

      return input.IndexOf(substr, StringComparison.InvariantCultureIgnoreCase) >= 0;
    }

    private static bool TryParseColor(string input, out Color color)
    {
      var matches = ColorRegex.Matches(input);

      if (matches.Count == 0)
      {
        var c = TShock.Config.BroadcastRGB;
        color = new Color(c[0], c[1], c[2]);
        return false;
      }

      color = new Color(int.Parse(matches[0].Groups[1].Value),
        int.Parse(matches[0].Groups[2].Value),
        int.Parse(matches[0].Groups[3].Value));
      return true;
    }

    private static void SendMessage(CommandArgs args, bool broadcast = false, bool impersonate = false)
    {
      if (args.Parameters.Count < 1 || string.IsNullOrWhiteSpace(args.Parameters[0]))
      {
        args.Player.SendErrorMessage($"Invalid usage! Usage: {Commands.Specifier}send (color) <player> <text>");
        return;
      }

      Color color;
      var playerparam = TryParseColor(args.Parameters[0], out color) ? 1 : 0;

      if (args.Parameters.Count <= playerparam)
      {
        args.Player.SendErrorMessage($"Invalid usage! Usage: {Commands.Specifier}send (color) <player> <text>");
        return;
      }

      if (broadcast)
      {
        args.Parameters.RemoveRange(0, playerparam);

        var bcmessage = string.Join(" ", args.Parameters);

        TShock.Players.Where(p => p != null && p.Active && p.RealPlayer)
          .ForEach(p => p.SendMessage(bcmessage.ReplaceVariables(args.Player.Name, p.Name), color));

        TSPlayer.Server.SendMessage(bcmessage.ReplaceVariables(args.Player.Name, "Server"), color);

        //TShock.Utils.Broadcast(bcmessage, color);
        return;
      }

      var target =
        TShock.Players.Where(p => p != null && InvariantContains(p.Name, args.Parameters[playerparam])).ToList();

      if (target.Count != 1)
      {
        if (target.Count < 1)
        {
          args.Player.SendErrorMessage("Player \"{0}\" not found!{1}", args.Parameters[playerparam],
            args.Parameters[playerparam].StartsWith("\"") && args.Parameters[playerparam].EndsWith("\"")
              ? ""
              : " Try surrounding the name in quotes.");

          return;
        }

        if (target.Count > 1)
        {
          TShock.Utils.SendMultipleMatchError(args.Player, target.Select(p => p.Name));
          return;
        }
      }

      var tsplayer = target.FirstOrDefault();

      if (tsplayer == null || !tsplayer.RealPlayer)
      {
        args.Player.SendErrorMessage("Player \"{0}\" not found!{1}", args.Parameters[playerparam],
          args.Parameters[playerparam].StartsWith("\"") && args.Parameters[playerparam].EndsWith("\"")
            ? ""
            : " Try surrounding the name in quotes.");

        return;
      }

      args.Parameters.RemoveRange(0, playerparam + 1);

      var message = string.Join(" ", args.Parameters).ReplaceVariables(args.Player.Name, tsplayer.Name);

      if (impersonate)
      {
        Color usercolor;
        Color broadcastcolor;

        TryParseColor(string.Join(",", TShock.Config.BroadcastRGB), out broadcastcolor);
        TryParseColor(tsplayer.Group.ChatColor, out usercolor);

        if (color != broadcastcolor)
          usercolor = color;

        if (usercolor == default(Color))
          usercolor = broadcastcolor;

        TShock.Utils.Broadcast(GetImpersonatedString(tsplayer, message), usercolor);
      }
      else
      {
        tsplayer.SendMessage(message, color);
      }
    }

    private static void SendCommand(CommandArgs args)
    {
      SendMessage(args);
    }

    private static void BroadcastCommand(CommandArgs args)
    {
      SendMessage(args, true);
    }

    private static void ImpersonateCommand(CommandArgs args)
    {
      SendMessage(args, false, true);
    }

    #region Meta

    public override string Name => "Send";
    public override string Author => "Newy";
    public override string Description => "Adds a barebones /send command.";
    public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

    #endregion
  }
}