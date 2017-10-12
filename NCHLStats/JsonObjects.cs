﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCHLStats
{
    public class JsonPlayers
    {
        public List<JsonPlayer> data;
    }

    public class JsonPlayer
    {
        public string playerId;
        public string teamAbbrev;
        public string points;
        public string playerName;
        public string playerPositionCode;
        public string penaltyMinutes;
        public string gamesPlayed;
        public string blockedShots;
        public string hits;
        public string takeaways;
        public string shTimeOnIce;
        public string timeOnIce;
    }

    public class JsonBios
    {
        public List<JsonBio> people;
    }

    public class JsonBio
    {
        public string id;
        public string currentAge;
        public JsonCurrentTeam currentTeam;
    }

    public class JsonCurrentTeam
    {
        public string name;
    }
}
