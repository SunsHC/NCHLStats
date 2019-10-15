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
        public List<Player> Players { get; protected set; }
        public bool MasterMode { get; set; }
        public int CurrentWeek { get; set; }

        internal StatsManager()
        {
            Players = new List<Player>();
            PlayersForWeek = new Dictionary<int, List<Player>>();
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
            for (int i = 1; i <= 27; i++)
            {
                if (!File.Exists($"WeekStats\\Week{i}Stats.json"))
                    break;

                using (StreamReader jsonReader = new StreamReader($"WeekStats\\Week{i}Stats.json"))
                {
                    string s = jsonReader.ReadLine();
                    var weekPlayers = JsonConvert.DeserializeObject(s, typeof(List<Player>)) as List<Player>;
                    PlayersForWeek.Add(i, weekPlayers);
                }
            }            
        }

        public void SaveGraph(NCHLTeam team)
        {

            try
            {
                foreach (PlayerPosition playPos in Enum.GetValues(typeof(PlayerPosition)))
                {
                    using (StreamWriter sw = new StreamWriter($"Graphs\\Graph{team}-{playPos}.txt"))
                    {
                        StringBuilder sb = new StringBuilder();
                        Dictionary<int, Player> playersToScan = new Dictionary<int, Player>();

                        // 1- Get players to scan
                        foreach (KeyValuePair<int, List<Player>> p in PlayersForWeek)
                        {
                            foreach (Player player in p.Value)
                            {
                                if (player.NCHLTeam == team && player.Pos == playPos && !playersToScan.ContainsKey(player.Id))
                                    playersToScan.Add(player.Id, player);
                            }
                        }

                        List<Tuple<double, string>> playSorting = new List<Tuple<double, string>>();

                        // 2- Get their stats
                        foreach (KeyValuePair<int, Player> p in playersToScan)
                        {
                            Player playerPos = Players.FirstOrDefault(pp => pp.Id == p.Key);
                            if (playerPos != null && (playerPos.Pos != playPos || playerPos.NCHLTeam != team))
                                continue;

                            string currentPlayerName = p.Value.Name;
                            int currentPlayerId = p.Key;
                            Dictionary<int, double> playerStatForWeek = new Dictionary<int, double>();

                            foreach (KeyValuePair<int, List<Player>> pfw in PlayersForWeek)
                            {
                                Player player = pfw.Value.FirstOrDefault(play => play.Id == currentPlayerId);
                                double pctSystem = -0.01;
                                if (player != null)
                                {
                                    if (playPos == PlayerPosition.G)
                                        pctSystem = player.TOI;
                                    else
                                        pctSystem = player.PctSystem;
                                }

                                playerStatForWeek.Add(pfw.Key, pctSystem);
                            }


                            // 3- Get last 3 weeks stats average

                            playerStatForWeek.TryGetValue(playerStatForWeek.Count, out double thisWeekPlayerStat);
                            playerStatForWeek.TryGetValue(playerStatForWeek.Count - 1, out double oneWeekAgoPlayerStat);
                            playerStatForWeek.TryGetValue(playerStatForWeek.Count - 2, out double twoWeeksAgoPlayerStat);
                            playerStatForWeek.TryGetValue(playerStatForWeek.Count - 3, out double threeWeeksAgoPlayerStat);
                            playerStatForWeek.TryGetValue(playerStatForWeek.Count - 4, out double fourWeeksAgoPlayerStat);
                            playerStatForWeek.TryGetValue(playerStatForWeek.Count - 5, out double fiveWeeksAgoPlayerStat);




                            double ThreeWeekAvg = (thisWeekPlayerStat + oneWeekAgoPlayerStat + twoWeeksAgoPlayerStat) / (playerStatForWeek.Count < 3 ? playerStatForWeek.Count : 3);


                            double ThreeWeekStdVar = 0;
                            if (playerStatForWeek.Count > 1)
                            {
                                ThreeWeekStdVar = Math.Sqrt((Math.Pow(thisWeekPlayerStat - ThreeWeekAvg, 2) +
                                    Math.Pow(oneWeekAgoPlayerStat - ThreeWeekAvg, 2) +
                                    Math.Pow(twoWeeksAgoPlayerStat - ThreeWeekAvg, 2)) / ((playerStatForWeek.Count < 3 ? playerStatForWeek.Count : 3) - 1));
                            }

                            double SixWeekAvg = (thisWeekPlayerStat +
                                oneWeekAgoPlayerStat +
                                twoWeeksAgoPlayerStat +
                                threeWeeksAgoPlayerStat + 
                                fourWeeksAgoPlayerStat + 
                                fiveWeeksAgoPlayerStat) / (playerStatForWeek.Count < 6 ? playerStatForWeek.Count : 6);

                            double SixWeekStdVar = 0;

                            if (playerStatForWeek.Count > 1)
                            {
                                SixWeekStdVar = Math.Sqrt((Math.Pow(thisWeekPlayerStat - SixWeekAvg, 2) +
                                    Math.Pow(oneWeekAgoPlayerStat - SixWeekAvg, 2) +
                                    Math.Pow(twoWeeksAgoPlayerStat - SixWeekAvg, 2) +
                                    Math.Pow(threeWeeksAgoPlayerStat - SixWeekAvg, 2) +
                                    Math.Pow(fourWeeksAgoPlayerStat - SixWeekAvg, 2) +
                                    Math.Pow(fiveWeeksAgoPlayerStat - SixWeekAvg, 2)) / ((playerStatForWeek.Count < 6 ? playerStatForWeek.Count : 6) - 1));
                            }
                            

                            string displayedTitle = $"{currentPlayerName}\nLast three weeks average: {(int)ThreeWeekAvg} std.var: {(int)ThreeWeekStdVar}\nLast six weeks average: {(int)SixWeekAvg} std.var: {(int)SixWeekStdVar}\n";


                            // 4- Generate player graph
                            string graph;
                            if (playPos == PlayerPosition.G)
                                graph = BarGraph.Generate(playerStatForWeek, displayedTitle, 1, playerStatForWeek.Count, 1, 0, 300, 30);
                            else
                                graph = BarGraph.Generate(playerStatForWeek, displayedTitle, 1, playerStatForWeek.Count, 1, 0, 100, 10);                            

                            playSorting.Add(new Tuple<double, string>(ThreeWeekAvg, graph));
                        }

                        // 5- Sort stat from best to worst
                        List<Tuple<double, string>> orderedList = playSorting.OrderByDescending(playSort => playSort.Item1).ToList();

                        foreach (Tuple<double, string> tp in orderedList)
                        {
                            sb.Append(tp.Item2);
                        }

                        sw.WriteLine(sb.ToString());
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
                    fileName = $"WeekReports\\Week{CurrentWeek}Report{team.ToString()}.xml";
                else
                    fileName = $"SeasonReports\\SeasonReport{team.ToString()}.xml";

                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    foreach (PlayerPosition pos in Enum.GetValues(typeof(PlayerPosition)))
                    {
                        StringBuilder sb = new StringBuilder();

                        List<Player> players;
                        if (pos != PlayerPosition.G)
                        {
                            players = Players.Where(p => p.NCHLTeam == team && p.Pos == pos).OrderBy(pl => pl.PctSystem).Reverse().ToList();
                        }
                        else
                        {
                            players = Players.Where(p => p.NCHLTeam == team && p.Pos == pos).OrderBy(pl => pl.TOI).Reverse().ToList();
                        }

                        // Write Position
                        sb.AppendLine($"{pos}");
                        sb.Append("--------------------------------");
                        if (pos != PlayerPosition.G)
                            sb.Append("[GP|P|PIM|Hits|BkS|TkA|SH|TOIGP]");
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
                                    stats = $"{playerToWriteCSV.GP}|{playerToWriteCSV.P}|{playerToWriteCSV.PIM}|{playerToWriteCSV.Hits}|{playerToWriteCSV.BkS}|{playerToWriteCSV.TkA}|{playerToWriteCSV.SH}|{playerToWriteCSV.TOIPG}]";
                                }

                                sb.Append($"{gap}{valueToUse} {bar} {stats}");

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
                    fileName = $"WeekStats\\Week{CurrentWeek}Stats.xml";
                else
                    fileName = "SeasonStats\\SeasonStats.xml";

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
                        writer.WriteElementString("TOIPG", player.TOIPG.ToString());

                        if (MasterMode)
                        {                            
                            writer.WriteElementString("PctSystem", player.PctSystem.ToString());
                        }

                        writer.WriteEndElement();                        
                    }
                    writer.WriteEndElement();

                    writer.WriteEndDocument();
                }

                using (StreamWriter jsonWriter = new StreamWriter($"WeekStats\\Week{CurrentWeek}Stats.json"))
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
            string scoringJsonHTMLLink = string.Format("http://www.nhl.com/stats/rest/skaters?isAggregate=true&reportType=basic&isGame=true&reportName=skatersummary&sort=[{{%22property%22:%22points%22,%22direction%22:%22DESC%22}},{{%22property%22:%22goals%22,%22direction%22:%22DESC%22}},{{%22property%22:%22assists%22,%22direction%22:%22DESC%22}}]&cayenneExp=gameDate%3E=%22{0}-{1}-{2}%22%20and%20gameDate%3C=%22{3}-{4}-{5}%22%20and%20gameTypeId=2",
                startDate.Year, startDate.Month, startDate.Day, endDate.Year, endDate.Month, endDate.Day);

            string defensiveJsonHTMLLink = string.Format("http://www.nhl.com/stats/rest/skaters?isAggregate=true&reportType=basic&isGame=true&reportName=realtime&sort=[{{%22property%22:%22hits%22,%22direction%22:%22DESC%22}}]&cayenneExp=gameDate%3E=%22{0}-{1}-{2}%22%20and%20gameDate%3C=%22{3}-{4}-{5}%22%20and%20gameTypeId=2",
                startDate.Year, startDate.Month, startDate.Day, endDate.Year, endDate.Month, endDate.Day);

            string timeOnIceJsonHTMLLink = string.Format("http://www.nhl.com/stats/rest/skaters?isAggregate=true&reportType=basic&isGame=true&reportName=timeonice&sort=[{{%22property%22:%22timeOnIce%22,%22direction%22:%22DESC%22}}]&cayenneExp=gameDate%3E=%22{0}-{1}-{2}%22%20and%20gameDate%3C=%22{3}-{4}-{5}%22%20and%20gameTypeId=2",
                startDate.Year, startDate.Month, startDate.Day, endDate.Year, endDate.Month, endDate.Day);

            string goalieJsonHTMLLink = string.Format("http://www.nhl.com/stats/rest/goalies?isAggregate=true&reportType=goalie_basic&isGame=true&reportName=goaliesummary&sort=[{{%22property%22:%22wins%22,%22direction%22:%22DESC%22}}]&cayenneExp=gameDate%3E=%22{0}-{1}-{2}%22%20and%20gameDate%3C=%22{3}-{4}-{5}%22%20and%20gameTypeId=2",
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

                UpdatePlayersStatisticsFromJsonList(jsonScoringPlayers.data as List<JsonPlayer>, (StatsType.GP | StatsType.P | StatsType.PIM | StatsType.TOIPG));
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

                    //using (WebClient client = new WebClient())
                    //{                        
                    //    //string bioJsonHTMLLink = string.Format("http://statsapi.web.nhl.com/api/v1/people/{0}", jsonPlayerId);
                    //    //string htmlCode = client.DownloadString(bioJsonHTMLLink);
                    //    //JsonBios jsonBiosPlayers = JsonConvert.DeserializeObject<JsonBios>(htmlCode);
                    //    //if (jsonBiosPlayers.people.First().currentTeam != null)
                    //    //    currentPlayer.NHLTeam = Utilities.GetNHLTeamFromString(jsonBiosPlayers.people.First().currentTeam.triCode);
                    //}                                        
                }
                else
                {
                    currentPlayer = new Player(jsonPlayerId);
                    Players.Add(currentPlayer);

                    if (p.playerName != null)
                        currentPlayer.Name = p.playerName;
                    if (p.playerPositionCode != null)
                        currentPlayer.Pos = Utilities.GetPlayerPositionFromString(p.playerPositionCode);
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
                    if (p.timeOnIcePerGame != null && (statsType & StatsType.TOIPG) == StatsType.TOIPG)
                        currentPlayer.TOIPG += (int)Math.Round(Convert.ToDouble(p.timeOnIcePerGame) / 60);
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
