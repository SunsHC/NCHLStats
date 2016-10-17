﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Collections.ObjectModel;
using System.Net;
using Newtonsoft.Json;

namespace NCHLStats
{
    internal class StatsManager
    {
        public List<Player> Players { get; protected set; }
        public bool MasterMode { get; set; }

        internal StatsManager()
        {
            Players = new List<Player>();
        }
        
        internal void LoadNCHLDB()
        {
            StreamReader sr = new StreamReader("DB NCHL.csv");

            do
            {
                string[] line = sr.ReadLine().Split(',');

                Player player = Players.Where(p => p.Id == Convert.ToInt32(line[3])).FirstOrDefault();

                if (player != null)
                {
                    player.NCHLTeam = Utilities.GetNCHLTeamFromString(line[1]);
                    player.Pos = Utilities.GetPlayerPositionFromString(line[2]);
                }

            } while (sr.Peek() >= 0);
        }

        public void SaveLeagueData()
        {
            try
            {
                using (XmlWriter writer = XmlWriter.Create("Stats.xml"))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Players");

                    foreach (Player player in Players)
                    {
                        writer.WriteStartElement("Player");

                        writer.WriteElementString("Name", player.Name);
                        writer.WriteElementString("NCHLTeam", player.NCHLTeam.ToString());
                        writer.WriteElementString("NHLTeam", player.NHLTeam.ToString());
                        writer.WriteElementString("Pos", player.Pos.ToString());
                        writer.WriteElementString("GP", player.GP.ToString());
                        writer.WriteElementString("P", player.P.ToString());
                        writer.WriteElementString("PIM", player.PIM.ToString());
                        writer.WriteElementString("Hits", player.Hits.ToString());
                        writer.WriteElementString("BkS", player.BkS.ToString());
                        writer.WriteElementString("TkA", player.TkA.ToString());
                        writer.WriteElementString("SH", player.SH.ToString());
                        writer.WriteElementString("TOI", player.TOI.ToString());

                        if (MasterMode)
                        {
                            writer.WriteElementString("PctSystem", player.PctSystem.ToString());
                        }

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    writer.WriteEndDocument();

                    JsonConvert.SerializeObject(Players);
                }

                using (StreamWriter jsonWriter = new StreamWriter("Stats.json"))
                {
                    jsonWriter.Write(JsonConvert.SerializeObject(Players));
                }
            }
            catch
            {
            }
        }

        internal void RetrieveWebData(DateTime startDate, DateTime endDate)
        {
            string scoringJsonHTMLLink = string.Format("http://www.nhl.com/stats/rest/individual/skaters/game/skatersummary?cayenneExp=gameDate%3E=%22{0}-{1}-{2}T05:00:00.000Z%22%20and%20gameDate%3C=%22{3}-{4}-{5}T05:00:00.000Z%22%20and%20gameTypeId=2",
                startDate.Year, startDate.Month, startDate.Day, endDate.Year, endDate.Month, endDate.Day);

            string defensiveJsonHTMLLink = string.Format("http://www.nhl.com/stats/rest/individual/skaters/game/realtime?cayenneExp=gameDate%3E=%22{0}-{1}-{2}T05:00:00.000Z%22%20and%20gameDate%3C=%22{3}-{4}-{5}T05:00:00.000Z%22%20and%20gameTypeId=2",
                startDate.Year, startDate.Month, startDate.Day, endDate.Year, endDate.Month, endDate.Day);

            string timeOnIceJsonHTMLLink = string.Format("http://www.nhl.com/stats/rest/individual/skaters/game/timeonice?cayenneExp=gameDate%3E=%22{0}-{1}-{2}T05:00:00.000Z%22%20and%20gameDate%3C=%22{3}-{4}-{5}T05:00:00.000Z%22%20and%20gameTypeId=2",
                startDate.Year, startDate.Month, startDate.Day, endDate.Year, endDate.Month, endDate.Day);

            string goalieJsonHTMLLink = string.Format("http://www.nhl.com/stats/rest/individual/goalies/game/goaliesummary?cayenneExp=gameDate%3E=%22{0}-{1}-{2}T05:00:00.000Z%22%20and%20gameDate%3C=%22{3}-{4}-{5}T05:00:00.000Z%22%20and%20gameTypeId=2%20and%20playerPositionCode=%22G%22",
                startDate.Year, startDate.Month, startDate.Day, endDate.Year, endDate.Month, endDate.Day);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(string.Format("Obtention des données du {0} inclusivement au {1} exclusivement en cours...", startDate.ToShortDateString(), endDate.ToShortDateString()));

            Progresser retreiverProgress = new Progresser(14);
            using (WebClient client = new WebClient())
            {
                string htmlCode = client.DownloadString(scoringJsonHTMLLink);
                retreiverProgress.Update();

                JsonPlayers jsonScoringPlayers = JsonConvert.DeserializeObject<JsonPlayers>(htmlCode);
                retreiverProgress.Update();

                // Create player list before setting their stats value
                CreatePlayersListFromJsonList(jsonScoringPlayers.data as List<JsonPlayer>);
                retreiverProgress.Update();

                UpdatePlayersStatisticsFromJsonList(jsonScoringPlayers.data as List<JsonPlayer>, (StatsType.GP | StatsType.P | StatsType.PIM));
                retreiverProgress.Update();

                htmlCode = client.DownloadString(defensiveJsonHTMLLink);
                retreiverProgress.Update();

                JsonPlayers jsonDefensivePlayers = JsonConvert.DeserializeObject<JsonPlayers>(htmlCode);
                retreiverProgress.Update();

                UpdatePlayersStatisticsFromJsonList(jsonDefensivePlayers.data as List<JsonPlayer>, (StatsType.Hits | StatsType.BkS | StatsType.TkA));
                retreiverProgress.Update();

                htmlCode = client.DownloadString(timeOnIceJsonHTMLLink);
                retreiverProgress.Update();

                JsonPlayers jsonTimeOnIcePlayers = JsonConvert.DeserializeObject<JsonPlayers>(htmlCode);
                retreiverProgress.Update();

                UpdatePlayersStatisticsFromJsonList(jsonTimeOnIcePlayers.data as List<JsonPlayer>, (StatsType.SH | StatsType.TOI));
                retreiverProgress.Update();

                htmlCode = client.DownloadString(goalieJsonHTMLLink);
                retreiverProgress.Update();

                JsonPlayers jsonGoaliesPlayers = JsonConvert.DeserializeObject<JsonPlayers>(htmlCode);
                retreiverProgress.Update();

                // Create player list before setting their stats value
                CreatePlayersListFromJsonList(jsonGoaliesPlayers.data as List<JsonPlayer>);
                retreiverProgress.Update();

                UpdatePlayersStatisticsFromJsonList(jsonGoaliesPlayers.data as List<JsonPlayer>, (StatsType.TOI));
                retreiverProgress.Update();
            }
        }


        private void CreatePlayersListFromJsonList(List<JsonPlayer> jsonPlayers)
        {
            foreach (JsonPlayer p in jsonPlayers)
            {
                int jsonPlayerId = Convert.ToInt32(p.playerId);
                Player currentPlayer;
                if (Players.Select(q => q.Id).Contains(jsonPlayerId))
                {
                    currentPlayer = Players.Where(r => r.Id == jsonPlayerId).First();

                    using (WebClient client = new WebClient())
                    {                        
                        string bioJsonHTMLLink = string.Format("http://statsapi.web.nhl.com/api/v1/people/{0}", jsonPlayerId);
                        string htmlCode = client.DownloadString(bioJsonHTMLLink);
                        JsonBios jsonBiosPlayers = JsonConvert.DeserializeObject<JsonBios>(htmlCode);
                        if (jsonBiosPlayers.people.First().currentTeam != null)
                            currentPlayer.NHLTeam = Utilities.GetNHLTeamFromString(jsonBiosPlayers.people.First().currentTeam.triCode);
                    }                                        
                }
                else
                {
                    currentPlayer = new Player(jsonPlayerId);
                    Players.Add(currentPlayer);

                    if (p.playerName != null)
                        currentPlayer.Name = p.playerName;
                    if (p.playerPositionCode != null)
                        currentPlayer.Pos = Utilities.GetPlayerPositionFromString(p.playerPositionCode);
                    if (p.teamAbbrev != null)
                        currentPlayer.NHLTeam = Utilities.GetNHLTeamFromString(p.teamAbbrev);
                }
            }
        }

        private void UpdatePlayersStatisticsFromJsonList(List<JsonPlayer> jsonPlayers, StatsType statsType)
        {
            foreach (JsonPlayer p in jsonPlayers)
            {
                int jsonPlayerId = Convert.ToInt32(p.playerId);
                Player currentPlayer;
                if (Players.Select(q => q.Id).Contains(jsonPlayerId))
                {
                    currentPlayer = Players.Where(r => r.Id == jsonPlayerId).First();

                    if (p.gamesPlayed != null && (statsType & StatsType.GP) == StatsType.GP)
                        currentPlayer.GP += Convert.ToInt32(p.gamesPlayed);
                    if (p.points != null && (statsType & StatsType.P) == StatsType.P)
                        currentPlayer.P += Convert.ToInt32(p.points);
                    if (p.penaltyMinutes != null && (statsType & StatsType.PIM) == StatsType.PIM)
                        currentPlayer.PIM += Convert.ToInt32(p.penaltyMinutes);
                    if (p.shTimeOnIce != null && (statsType & StatsType.SH) == StatsType.SH)
                        currentPlayer.SH += Convert.ToInt32(p.shTimeOnIce);
                    if (p.hits != null && (statsType & StatsType.Hits) == StatsType.Hits)
                        currentPlayer.Hits += Convert.ToInt32(p.hits);
                    if (p.takeaways != null && (statsType & StatsType.TkA) == StatsType.TkA)
                        currentPlayer.TkA += Convert.ToInt32(p.takeaways);
                    if (p.blockedShots != null && (statsType & StatsType.BkS) == StatsType.BkS)
                        currentPlayer.BkS += Convert.ToInt32(p.blockedShots);
                    if (p.timeOnIce != null && (statsType & StatsType.TOI) == StatsType.TOI)
                        currentPlayer.TOI += (int)Math.Round(Convert.ToDouble(p.timeOnIce) / 60);
                }
            }
        }

        internal void GetSystemRankings()
        {
            foreach (PlayerPosition pos in Enum.GetValues(typeof(PlayerPosition)))
            {
                if (pos == PlayerPosition.G)
                    continue;

                List<Player> playersToScan = Players.Where(p => p.Pos == pos && p.NCHLTeam != NCHLTeam.AGL).ToList();

                Stats statsForPosition = new Stats(playersToScan);
                statsForPosition.GeneratePlayerStats();
            }
        }
    }
}
