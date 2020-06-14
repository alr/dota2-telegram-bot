using System;
using System.Collections.Generic;
using System.Text;

namespace Dota2Bot.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Markdown(this string value)
        {
            return value
                .Replace("*", @"\*")
                .Replace("_", @"\_")
                //.Replace("[", @"\[")
                //.Replace("]", @"\]")
                //.Replace("(", @"\(")
                //.Replace(")", @"\)")
                .Replace("`", @"\`");
        }
    }
}
