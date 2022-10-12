using System;
using Dota2Bot.Core.Extensions;

namespace Dota2Bot.Core.Bot.Commands.TimeBased;

public class YearCmd : BaseTimeReportCmd
{
    public override string Cmd => "year";
    public override string Description => "year stats";

    protected override DateTime GetDateStart(string timezone)
    {
        var local = DateTime.UtcNow.ConvertToTimezone(timezone).StartOfYear();
        return local.ConvertFromTimezone(timezone);
    }

    protected override string ParseHeroName(string args)
    {
        return args;
    }
}