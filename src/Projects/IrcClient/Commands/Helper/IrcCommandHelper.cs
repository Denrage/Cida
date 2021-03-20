using System.Collections.Generic;
using System.Linq;
using IrcClient.Commands;

namespace IrcClient.Commands.Helper
{
    public static class IrcCommandHelper
    {
        private static readonly Dictionary<IrcCommand, string> ircCommands = new Dictionary<IrcCommand, string>()
        {
            { IrcCommand.Admin, "ADMIN" },
            { IrcCommand.Away, "AWAY" },
            { IrcCommand.Cap, "CAP" },
            { IrcCommand.CNotice, "CNOTICE" },
            { IrcCommand.CPrivMsg, "CPRIVMSG" },
            { IrcCommand.Connect, "CONNECT" },
            { IrcCommand.Ctcp, "CTCP" },
            { IrcCommand.Dcc, "DCC" },
            { IrcCommand.Die, "DIE" },
            { IrcCommand.Encap, "ENCAP" },
            { IrcCommand.Error, "ERROR" },
            { IrcCommand.Help, "HELP" },
            { IrcCommand.Info, "INFO" },
            { IrcCommand.Invite, "INVITE" },
            { IrcCommand.IsOn, "ISON" },
            { IrcCommand.Join, "JOIN" },
            { IrcCommand.Kick, "KICK" },
            { IrcCommand.Kill, "KILL" },
            { IrcCommand.Knock, "KNOCK" },
            { IrcCommand.Links, "LINKS" },
            { IrcCommand.List, "LIST" },
            { IrcCommand.LUsers, "LUSERS" },
            { IrcCommand.Mode, "MODE" },
            { IrcCommand.Motd, "MOTD" },
            { IrcCommand.Names, "NAMES" },
            { IrcCommand.NamesX, "NAMESX" },
            { IrcCommand.Nick, "NICK" },
            { IrcCommand.Notice, "NOTICE" },
            { IrcCommand.Oper, "OPER" },
            { IrcCommand.Part, "PART" },
            { IrcCommand.Pass, "PASS" },
            { IrcCommand.Ping, "PING" },
            { IrcCommand.Pong, "PONG" },
            { IrcCommand.PrivMsg, "PRIVMSG" },
            { IrcCommand.Quit, "QUIT" },
            { IrcCommand.Rehash, "REHASH" },
            { IrcCommand.Restart, "RESTART" },
            { IrcCommand.Rules, "RULES" },
            { IrcCommand.Server, "SERVER" },
            { IrcCommand.Service, "SERVICE" },
            { IrcCommand.ServList, "SERVLIST" },
            { IrcCommand.SQuery, "SQUERY" },
            { IrcCommand.SQuit, "SQUIT" },
            { IrcCommand.SetName, "SETNAME" },
            { IrcCommand.Silence, "SILENCE" },
            { IrcCommand.Stats, "STATS" },
            { IrcCommand.Summon, "SUMMON" },
            { IrcCommand.Time, "TIME" },
            { IrcCommand.Topic, "TOPIC" },
            { IrcCommand.Trace, "TRACE" },
            { IrcCommand.UhNames, "UHNAMES" },
            { IrcCommand.User, "USER" },
            { IrcCommand.UserHost, "USERHOST" },
            { IrcCommand.UserIp, "USERIP" },
            { IrcCommand.Users, "USERS" },
            { IrcCommand.Version, "VERSION" },
            { IrcCommand.Wallops, "WALLOPS" },
            { IrcCommand.Watch, "WATCH" },
            { IrcCommand.Who, "WHO" },
            { IrcCommand.WhoIs, "WHOIS" },
            { IrcCommand.WhoWas, "WHOWAS" }
        };

        public static string ToCommandString(this IrcCommand command)
        {
            return ircCommands[command];
        }

        public static bool TryParse(string message, out IrcCommand command, out string parameter)
        {
            string commandString = message;
            int position = message.IndexOf(' ');

            if (position >= 0)
            {
                parameter = commandString.Substring(position + 1);
                commandString = commandString.Remove(position).ToUpper();
            }
            else
            {
                parameter = string.Empty;
            }
            
            var matches = ircCommands
                .Where((x) => x.Value == commandString)
                .Select((x) => x.Key);

            command = matches.FirstOrDefault();
            return matches.Any();
        }
    }

}