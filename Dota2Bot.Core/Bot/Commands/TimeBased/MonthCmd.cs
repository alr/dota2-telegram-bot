using System;
using Dota2Bot.Core.Extensions;

namespace Dota2Bot.Core.Bot.Commands.TimeBased;

public class MonthCmd : BaseTimeReportCmd
{
    public override string Cmd => "month";
    public override string Description => "monthly stats";

    protected override DateTime GetDateStart(string timezone)
    {
        var local = DateTime.UtcNow.ConvertToTimezone(timezone).StartOfMonth();
        return local.ConvertFromTimezone(timezone);
    }

    protected override string ParseHeroName(string args)
    {
        return args;
    }
}