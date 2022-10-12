using System;
using Dota2Bot.Core.Extensions;

namespace Dota2Bot.Core.Bot.Commands.TimeBased;

public class WeekCmd : BaseTimeReportCmd
{
    public override string Cmd => "week";
    public override string Description => "weekly stats";

    protected override DateTime GetDateStart(string timezone)
    {
        var local = DateTime.UtcNow.ConvertToTimezone(timezone).StartOfWeek(DayOfWeek.Monday);
        return local.ConvertFromTimezone(timezone);
    }

    protected override string ParseHeroName(string args)
    {
        return args;
    }
}