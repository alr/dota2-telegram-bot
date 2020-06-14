using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dota2Bot.Core.Domain
{
    public static class PlayerSlot
    {
        private static bool GetBit(byte byteval, int idx)
        {
            return (byteval & (1 << idx)) != 0;
        }

        //https://wiki.teamfortress.com/wiki/WebAPI/GetMatchDetails
        public static bool IsRadiant(int slot)
        {
            return GetBit((byte) slot, 7) == false;
        }
    }
}
