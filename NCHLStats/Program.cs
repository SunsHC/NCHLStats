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
            StatsManager manager = new StatsManager();

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("*******************************************");
            Console.WriteLine("*****Generation des stats de la NHL...*****");
            Console.WriteLine("*******************************************");

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Entrer le type de fichier a generer (1)Semaine (2)Quart de saison ");
            string mode = Console.ReadLine();
            string[] modeArray = mode.Split('.');
            int modeNumber = Convert.ToInt32(modeArray[0]);
            if (modeArray.Length > 1)
                manager.MasterMode = modeArray[1] == "a";

            if (modeNumber == 1)
            {
                //manager.LoadJSON();

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
                    manager.SaveReportData(NCHLTeam.SUN, true);
            }
            else if (modeNumber == 2)
            {
                Console.WriteLine();

                Console.Write("Quart (1-4): ");
                int quarter = Convert.ToInt32(Console.ReadLine());

                manager.CurrentQuarter = quarter;

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Entrer la date du mardi du debut du quart ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Jour (1-31): ");
                int day = Convert.ToInt32(Console.ReadLine());

                Console.Write("Mois (1-12): ");
                int month = Convert.ToInt32(Console.ReadLine());

                Console.Write("An (2000-2099): ");
                int year = Convert.ToInt32(Console.ReadLine());

                Console.Write("Nombre de semaines dans le quart (6-7): ");
                int weeksInQuarter = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine();

                DateTime startDate = new DateTime(year, month, day, new System.Globalization.GregorianCalendar());
                DateTime endDate = startDate.AddDays(7 * weeksInQuarter);

                manager.RetrieveWebData(startDate, endDate);

                manager.LoadNCHLDB();

                if (manager.MasterMode)
                    manager.GetSystemRankings();

                manager.SaveLeagueData(false);

                if (manager.MasterMode)
                    manager.SaveReportData(NCHLTeam.SUN, false);
            }
        }        
    }
}
