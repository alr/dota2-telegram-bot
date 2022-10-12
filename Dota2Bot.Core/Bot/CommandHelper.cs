using System;
using System.Collections.Generic;
using System.Linq;
using Dota2Bot.Core.Bot.Commands;

namespace Dota2Bot.Core.Bot;

public static class CommandHelper
{
    public static (string Cmd, string Args)? Parse(string message)
    {
        if (string.IsNullOrEmpty(message) || message.Length < 2)
            return (null, null);

        var data = message
            .Substring(1) // remove slash
            .Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries); // split by cmd and args

        var cmd = data[0].Split('@')[0].ToLower().Trim(); // remove bot name
        var args = data.Length == 2 ? data[1].Trim() : null;

        return (cmd, args);
    }
    
    public static List<IBotCmd> GetCommands()
    {
        var commands = new List<IBotCmd>();

        var type = typeof(IBotCmd);
        var types = type.Assembly.GetTypes()
            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
            .ToList();

        foreach (var commandType in types)
        {
            var constructorInfo = commandType.GetConstructors().First();

            var parameters = constructorInfo.GetParameters();
            var ctorParams = parameters.Select(x => (object)null).ToArray();

            commands.Add(constructorInfo.Invoke(ctorParams) as IBotCmd);
        }

        return commands;
    }
}
