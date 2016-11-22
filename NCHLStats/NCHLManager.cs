using System;
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
        public Dictionary<int, List<Player>> PlayersForWeek { get; protected set; }
        //public Dictionary<int, Dictionary<int, List<Player>>> PlayersForWeek { get; protected set; }
        public List<Player> Players { get; protected set; }
        public bool MasterMode { get; set; }
        public int CurrentWeek { get; set; }
        public int CurrentQuarter { get; set; }

        internal StatsManager()
        {
            Players = new List<Player>();
        }
        
        internal void LoadNCHLDB()
        {
            using (StreamReader sr = new StreamReader("DB NCHL.csv"))
            {
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
        }

        internal void LoadJSONs()
        {
            using (StreamReader jsonReader = new StreamReader(string.Format("SeasonStats.json")))
            {
                string s = jsonReader.ReadLine();
                object o = JsonConvert.DeserializeObject(s, typeof(List<Player>));
            }

            for (int i = 1; i <= 27; i++)
            {
                if (!File.Exists(string.Format("Week{0}Stats.json", i)))
                    break;

                using (StreamReader jsonReader = new StreamReader(string.Format("Week{0}Stats.json", i)))
                {
                    string s = jsonReader.ReadLine();
                    var weekPlayers = JsonConvert.DeserializeObject(s, typeof(List<Player>)) as List<Player>;
                    PlayersForWeek.Add(i, weekPlayers);
                }
            }            
        }

        public void SaveSeasonReport(NCHLTeam team)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("SeasonReport.txt"))
                {
                    foreach (PlayerPosition pos in Enum.GetValues(typeof(PlayerPosition)))
                    {
                        StringBuilder sb = new StringBuilder();

                        List<Player> players;

                        if (pos != PlayerPosition.G)
                            players = Players.Where(p => p.NCHLTeam == team && p.Pos == pos).OrderBy(pl => pl.PctSystem).Reverse().ToList();
                        else
                            players = Players.Where(p => p.NCHLTeam == team && p.Pos == pos).OrderBy(pl => pl.TOI).Reverse().ToList();

                        // Write Position
                        sb.AppendLine(string.Format("{0}", pos));
                        sb.Append("--------------------------------");
                        if (pos != PlayerPosition.G)
                            sb.Append("[GP|P|PIM|Hits|BkS|TkA|SH]");
                        sb.AppendLine();

                        if (players.Count > 0)
                        {

                            // Get max string lenght of the player
                            int maxPlayerNameLenght = players.Max(p => p.Name.Length);


                            foreach (Player playerToWriteCSV in players)
                            {
                                // Write Name
                                sb.Append(playerToWriteCSV.Name);

                                // Fill the rest with dots
                                int dotsToWrite = maxPlayerNameLenght - playerToWriteCSV.Name.Length;
                                for (int i = 0; i < dotsToWrite; i++)
                                {
                                    sb.Append(".");
                                }

                                // Create bar
                                string bar = string.Empty;
                                int maxScore;
                                int valueToUse;

                                if (pos != PlayerPosition.G)
                                {
                                    maxScore = 100;
                                    valueToUse = (int)playerToWriteCSV.PctSystem;
                                }
                                else
                                {
                                    maxScore = players.Max(p => p.TOI);
                                    valueToUse = playerToWriteCSV.TOI;
                                }

                                for (int i = maxScore / 10; i <= maxScore; i += maxScore / 10)
                                {
                                    if (valueToUse >= i)
                                        bar += "█";
                                    else
                                        bar += "░";
                                }

                                // Fill gap properly between number and bar
                                int valueStringLenght = valueToUse.ToString().Length;
                                string gap = string.Empty;
                                for (int i = (4 - valueStringLenght); i > 0; i--)
                                {
                                    gap += " ";
                                }

                                // Show stats at right of player (not goalies)
                                string stats = string.Empty;
                                if (pos != PlayerPosition.G)
                                {
                                    stats = string.Format("[{0}|{1}|{2}|{3}|{4}|{5}|{6}]",
                                        playerToWriteCSV.GP,
                                        playerToWriteCSV.P,
                                        playerToWriteCSV.PIM,
                                        playerToWriteCSV.Hits,
                                        playerToWriteCSV.BkS,
                                        playerToWriteCSV.TkA,
                                        playerToWriteCSV.SH);
                                }

                                sb.Append(string.Format("{0}{1} {2} {3}", gap, valueToUse, bar, stats));

                                sb.AppendLine();
                            }

                            sw.WriteLine(sb.ToString());
                        }
                    }
                }
            }
            catch
            {
            }
        }


        public void SaveReportData(NCHLTeam team, bool isWeekStats)
        {
            try
            {
                string fileName;
                if (isWeekStats)
                    fileName = string.Format("Week{0}Report.xml", CurrentWeek);
                else
                    fileName = string.Format("Quarter{0}Report.xml", CurrentQuarter);

                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    foreach (PlayerPosition pos in Enum.GetValues(typeof(PlayerPosition)))
                    {
                        StringBuilder sb = new StringBuilder();

                        List<Player> players;
                        if (pos != PlayerPosition.G)
                            players = Players.Where(p => p.NCHLTeam == team && p.Pos == pos).OrderBy(pl => pl.PctSystem).Reverse().ToList();
                        else
                            players = Players.Where(p => p.NCHLTeam == team && p.Pos == pos).OrderBy(pl => pl.TOI).Reverse().ToList();

                        // Write Position
                        sb.AppendLine(string.Format("{0}", pos));
                        sb.Append("--------------------------------");
                        if (pos != PlayerPosition.G)
                            sb.Append("[GP|P|PIM|Hits|BkS|TkA|SH]");
                        sb.AppendLine();

                        if (players.Count > 0)
                        {

                            // Get max string lenght of the player
                            int maxPlayerNameLenght = players.Max(p => p.Name.Length);


                            foreach (Player playerToWriteCSV in players)
                            {
                                // Write Name
                                sb.Append(playerToWriteCSV.Name);

                                // Fill the rest with dots
                                int dotsToWrite = maxPlayerNameLenght - playerToWriteCSV.Name.Length;
                                for (int i = 0; i < dotsToWrite; i++)
                                {
                                    sb.Append(".");
                                }

                                // Create bar
                                string bar = string.Empty;
                                int maxScore;
                                int valueToUse;

                                if (pos != PlayerPosition.G)
                                {
                                    maxScore = 100;
                                    valueToUse = (int)playerToWriteCSV.PctSystem;
                                }
                                else
                                {
                                    maxScore = players.Max(p => p.TOI);
                                    valueToUse = playerToWriteCSV.TOI;
                                }

                                for (int i = maxScore / 10; i <= maxScore; i += maxScore / 10)
                                {
                                    if (valueToUse >= i)
                                        bar += "█";
                                    else
                                        bar += "░";
                                }

                                // Fill gap properly between number and bar
                                int valueStringLenght = valueToUse.ToString().Length;
                                string gap = string.Empty;
                                for (int i = (4 - valueStringLenght); i > 0; i--)
                                {
                                    gap += " ";
                                }

                                // Show stats at right of player (not goalies)
                                string stats = string.Empty;
                                if (pos != PlayerPosition.G)
                                {
                                    stats = string.Format("[{0}|{1}|{2}|{3}|{4}|{5}|{6}]",
                                        playerToWriteCSV.GP,
                                        playerToWriteCSV.P,
                                        playerToWriteCSV.PIM,
                                        playerToWriteCSV.Hits,
                                        playerToWriteCSV.BkS,
                                        playerToWriteCSV.TkA,
                                        playerToWriteCSV.SH);
                                }

                                sb.Append(string.Format("{0}{1} {2} {3}", gap, valueToUse, bar, stats));

                                sb.AppendLine();
                            }

                            sw.WriteLine(sb.ToString());
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public void SaveLeagueData(bool isWeekStats)
        {
            try
            {
                string fileName;
                if (isWeekStats)
                    fileName = string.Format("Week{0}Stats.xml", CurrentWeek);
                else
                    fileName = string.Format("Quarter{0}Stats.xml", CurrentQuarter);

                using (XmlWriter writer = XmlWriter.Create(fileName))
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
                }

                using (StreamWriter jsonWriter = new StreamWriter(string.Format("Week{0}Stats.json", CurrentWeek)))
                {
                    jsonWriter.Write(JsonConvert.SerializeObject(Players));
                }

                LoadJSONs();


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
                        //if (jsonBiosPlayers.people.First().currentTeam != null)
                            //currentPlayer.NHLTeam = Utilities.GetNHLTeamFromString(jsonBiosPlayers.people.First().currentTeam.triCode);
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
