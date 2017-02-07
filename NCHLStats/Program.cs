using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json;


namespace NCHLStats
{
    class Program
    {
        static void Main(string[] args)
        {
            //Dictionary<int, double> dict = new Dictionary<int, double>();
            //dict.Add(1, 95);
            //dict.Add(2, 65);
            //BarGraph bg = new BarGraph(dict, 1, 2, 1, 0, 100, 10);



            StatsManager manager = new StatsManager();

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("*******************************************");
            Console.WriteLine("*****Generation des stats de la NHL...*****");
            Console.WriteLine("*******************************************");

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Entrer le type de fichier a generer (1)Semaine (2)Saison ");
            string mode = Console.ReadLine();
            string[] modeArray = mode.Split('.');
            int modeNumber = Convert.ToInt32(modeArray[0]);
            if (modeArray.Length > 1)
                manager.MasterMode = modeArray[1] == "1992";

            if (modeNumber == 1)
            {
                Console.WriteLine();

                Console.Write("Semaine (1-27): ");
                int week = Convert.ToInt32(Console.ReadLine());

                manager.CurrentWeek = week;

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Entrer la date du mardi ");

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
            }
            else if (modeNumber == 2)
            {
                Console.WriteLine();

                Console.Write("Quart (1-4): ");
                int quarter = Convert.ToInt32(Console.ReadLine());

                manager.CurrentQuarter = quarter;

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Entrer la date du premier match de la saison ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Jour (1-31): ");
                int day = Convert.ToInt32(Console.ReadLine());

                Console.Write("Mois (1-12): ");
                int month = Convert.ToInt32(Console.ReadLine());

                Console.Write("An (2000-2099): ");
                int year = Convert.ToInt32(Console.ReadLine());

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(string.Format("Entrer la date du premier mardi du quart {0} ", quarter + 1));

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Jour (1-31): ");
                int endDay = Convert.ToInt32(Console.ReadLine());

                Console.Write("Mois (1-12): ");
                int endMonth = Convert.ToInt32(Console.ReadLine());

                Console.Write("An (2000-2099): ");
                int endYear = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine();

                DateTime startDate = new DateTime(year, month, day, new System.Globalization.GregorianCalendar());
                DateTime endDate = new DateTime(endYear, endMonth, endDay, new System.Globalization.GregorianCalendar());

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
            }
        }        
    }
}
