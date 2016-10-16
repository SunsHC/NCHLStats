using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCHLStats
{
    internal class Progresser
    {
        private int _dataRetrievalProgressCounter = 0;
        private int _dataRetrievalProgressUpdatePoints = 0;

        public Progresser(int numberUpdatePoints)
        {
            _dataRetrievalProgressUpdatePoints = numberUpdatePoints;

            Console.Write(string.Format("\r{0}%  [", 0));
            for (int i = 1; i <= 10; i++)
            {
                Console.Write("░");
            }
            Console.Write("]");
        }

        public void Update()
        {
            double progressPctDouble = (++_dataRetrievalProgressCounter / (double)_dataRetrievalProgressUpdatePoints);
            progressPctDouble *= 100;
            int progressPctInt = (int)progressPctDouble;

            Console.Write(string.Format("\r{0}%  [", progressPctInt));
            for (int i = 1; i <= 10; i++ )
            {
                if ((double)i <= (progressPctDouble / 10))
                    Console.Write("█");
                else
                    Console.Write("░");
            }
            Console.Write("]");
        }

    }
}
