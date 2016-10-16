using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCHLStats
{
    internal static class Utilities
    {
        public static NHLTeam GetNHLTeamFromString(string teamName)
        {
            return (NHLTeam)Enum.Parse(typeof(NHLTeam), teamName);
        }

        public static NCHLTeam GetNCHLTeamFromString(string teamName)
        {
            return (NCHLTeam)Enum.Parse(typeof(NCHLTeam), teamName);
        }

        public static PlayerPosition GetPlayerPositionFromString(string position)
        {
            return (PlayerPosition)Enum.Parse(typeof(PlayerPosition), position);
        }
    }
}
