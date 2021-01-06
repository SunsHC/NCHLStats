using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace NCHLStats
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("*******************************************");
                Console.WriteLine("*****Generation des stats de la NHL...*****");
                Console.WriteLine("*******************************************");

                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Gray;

                // Stats de la semaine
                StatsManager manager = new StatsManager();
                manager.MasterMode = false;

                Console.Write("Semaine (1-27): ");
                int week = Convert.ToInt32(Console.ReadLine());
                manager.CurrentWeek = week;

                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"Entrer la date du mardi correspondant au début de la semaine {week}");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Jour (1-31): ");
                int day = Convert.ToInt32(Console.ReadLine());

                Console.Write("Mois (1-12): ");
                int month = Convert.ToInt32(Console.ReadLine());

                Console.Write("An (2000-2099): ");
                int year = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine();

                DateTime startDate = new DateTime(year, month, day, new System.Globalization.GregorianCalendar());
                DateTime endDate = startDate.AddDays(7);

                manager.RetrieveWebData(startDate, endDate);

                manager.LoadNCHLDB();

                if (manager.MasterMode)
                    manager.GetSystemRankings();

                manager.SaveLeagueData(true);

                if (manager.MasterMode)
                {
                    foreach (NCHLTeam team in Enum.GetValues(typeof(NCHLTeam)))
                    {
                        if (team == NCHLTeam.AGL)
                            continue;

                        manager.SaveReportData(team, true);
                        manager.SaveGraph(team);
                    }
                }

                manager = new StatsManager();
                manager.MasterMode = false;

                using (StreamReader textReader = new StreamReader("DateDebutSaison.txt"))
                {
                    string[] date = textReader.ReadLine().Split('-');
                    year = Convert.ToInt32(date[0]);
                    month = Convert.ToInt32(date[1]);
                    day = Convert.ToInt32(date[2]);

                    startDate = new DateTime(year, month, day, new System.Globalization.GregorianCalendar());
                }

                int endDay = DateTime.Now.Day;
                int endMonth = DateTime.Now.Month;
                int endYear = DateTime.Now.Year;

                Console.WriteLine();

                endDate = new DateTime(endYear, endMonth, endDay, new System.Globalization.GregorianCalendar());

                manager.RetrieveWebData(startDate, endDate);

                manager.LoadNCHLDB();

                if (manager.MasterMode)
                    manager.GetSystemRankings();

                manager.SaveLeagueData(false);

                if (manager.MasterMode)
                {
                    foreach (NCHLTeam team in Enum.GetValues(typeof(NCHLTeam)))
                    {
                        if (team == NCHLTeam.AGL)
                            continue;

                        manager.SaveReportData(team, false);
                    }
                }

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine();
                Console.WriteLine();
                Console.Write($"Les stats ont été générées. Appuyer sur Entrée pour quitter.");
                Console.Read();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine();
                Console.Write($"Une erreur est survenue. Appuyer sur Entrée pour quitter.\n\n" +
                    $"{ex.Message}\n" +
                    $"{ex.StackTrace}");
                Console.Read();
            }
            
        }        
    }
}
