using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCHLStats
{

    internal class Player
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public PlayerPosition Pos { get; set; }
        public NCHLTeam NCHLTeam { get; set; }
        public NHLTeam NHLTeam { get; set; }
        public int GP { get; set; }
        public int P { get; set; }
        public int PIM { get; set; }
        public int SH { get; set; }
        public int Hits { get; set; }
        public int TkA { get; set; }
        public int BkS { get; set; }
        public int TOI { get; set; }

        public double PtsSystem { get; set; }
        public double PctSystem { get; set; }

        public Player(int id)
        {
            this.Id = id;
        }
    }
}
