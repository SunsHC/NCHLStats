using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCHLStats
{
    internal class Stats
    {
        List<Stat> _playerStat = new List<Stat>();

        double maxValP;
        double maxValPIM;
        double maxValHits;
        double maxValBkS;
        double maxValTkA;
        double maxValSH;
        double maxValPts;
        double minValPts;

        public Stats(List<Player> p)
        {
            foreach (Player player in p)
            {
                _playerStat.Add(new Stat(player));
            }
        }

        public void GeneratePlayerStats()
        {
            foreach (Stat p in _playerStat)
            {
                p.GetNormalizedStats();

                if (p.normalizedP > maxValP)
                    maxValP = p.normalizedP;

                if (p.normalizedPIM > maxValPIM)
                    maxValPIM = p.normalizedPIM;

                if (p.normalizedHits > maxValHits)
                    maxValHits = p.normalizedHits;

                if (p.normalizedBkS > maxValBkS)
                    maxValBkS = p.normalizedBkS;

                if (p.normalizedTkA > maxValTkA)
                    maxValTkA = p.normalizedTkA;

                if (p.normalizedSH > maxValSH)
                    maxValSH = p.normalizedSH;
            }

            foreach (Stat p in _playerStat)
            {
                p.GetSystemStats(maxValP, maxValPIM, maxValHits, maxValBkS, maxValTkA, maxValSH);

                p.player.PtsSystem = p.globalNote;

                if (p.player.PtsSystem > maxValPts)
                    maxValPts = p.player.PtsSystem;

                if (minValPts == 0 || p.player.PtsSystem < minValPts)
                    minValPts = p.player.PtsSystem;
            }

            var list = (from p in _playerStat
                       where (p.globalNote > 3 && (p.player.Pos == PlayerPosition.C || p.player.Pos == PlayerPosition.L || p.player.Pos == PlayerPosition.R)) ||
                       (p.globalNote > 4 && (p.player.Pos == PlayerPosition.D))
                       orderby p.globalNote
                            select p).ToList();

            int i = 1;

            foreach (Stat p in list)
            {
                p.player.PctSystem = Math.Round(((double)i / list.Count) * 100, 0);
                i++;
                //p.player.Qt = 100 - ((maxValPts - p.player.Pts) / (maxValPts - minValPts)) * 100;
            }
        }
    }

    internal class Stat
    {
        public Player player;

        public double normalizedP;
        public double normalizedPIM;
        public double normalizedHits;
        public double normalizedBkS;
        public double normalizedTkA;
        public double normalizedSH;

        public double systemP;
        public double systemPIM;
        public double systemHits;
        public double systemBkS;
        public double systemTkA;
        public double systemSH;

        public double globalNote;

        //TODO SETTINGS
        //Ratios
        private const int D_DEF = 8;
        private const int D_OFF = 9;
        private const int D_PEN = 3;
        private const int F_DEF = 6;
        private const int F_OFF = 12;
        private const int F_PEN = 2;

        //Defensive Ratios
        private const double D_DEF_HITS = 0.2;
        private const double D_DEF_BKS = 0.27;
        private const double D_DEF_TKA = 0.2;
        private const double D_DEF_SH = 0.33;
        private const double F_DEF_HITS = 0.25;
        private const double F_DEF_BKS = 0.15;
        private const double F_DEF_TKA = 0.33;
        private const double F_DEF_SH = 0.27;

        private const double DEF_BASE = 0.5;
        private const double DEF_RATIO = 0.5;

        public Stat(Player p)
        {
            player = p;
        }

        public void GetNormalizedStats()
        {
            if (player.GP > 0)
            {
                normalizedP = (player.P * 82) / player.GP;
                normalizedPIM = (player.PIM * 82) / player.GP;
                normalizedHits = (player.Hits * 82) / player.GP;
                normalizedBkS = (player.BkS * 82) / player.GP;
                normalizedTkA = (player.TkA * 82) / player.GP;
                normalizedSH = (player.SH * 82) / player.GP;
            }
        }

        public void GetSystemStats(double P, double PIM, double Hits, double BkS, double TkA, double SH)
        {
            if (player.GP > 0)
            {
                if (player.Pos == PlayerPosition.C || player.Pos == PlayerPosition.L || player.Pos == PlayerPosition.R)
                    systemP = (normalizedP / P) * F_OFF;
                else
                    systemP = (normalizedP / P) * D_OFF;

                if (player.Pos == PlayerPosition.C || player.Pos == PlayerPosition.L || player.Pos == PlayerPosition.R)
                    systemPIM = (normalizedPIM / PIM) * F_PEN;
                else
                    systemPIM = (normalizedPIM / PIM) * D_PEN;

                if (player.Pos == PlayerPosition.C || player.Pos == PlayerPosition.L || player.Pos == PlayerPosition.R)
                {
                    systemHits = DEF_BASE * (F_DEF_HITS * F_DEF) +
                        ((normalizedHits / Hits) * DEF_RATIO * (F_DEF_HITS * F_DEF));

                    systemBkS = DEF_BASE * (F_DEF_BKS * F_DEF) +
                        ((normalizedBkS / BkS) * DEF_RATIO * (F_DEF_BKS * F_DEF));

                    systemTkA = DEF_BASE * (F_DEF_TKA * F_DEF) +
                        ((normalizedTkA / TkA) * DEF_RATIO * (F_DEF_TKA * F_DEF));

                    systemSH = DEF_BASE * (F_DEF_SH * F_DEF) +
                        ((normalizedSH / SH) * DEF_RATIO * (F_DEF_SH * F_DEF));
                }
                else
                {
                    systemHits = DEF_BASE * (D_DEF_HITS * D_DEF) +
                        ((normalizedHits / Hits) * DEF_RATIO * (D_DEF_HITS * D_DEF));

                    systemBkS = DEF_BASE * (D_DEF_BKS * D_DEF) +
                        ((normalizedBkS / BkS) * DEF_RATIO * (D_DEF_BKS * D_DEF));

                    systemTkA = DEF_BASE * (D_DEF_TKA * D_DEF) +
                        ((normalizedTkA / TkA) * DEF_RATIO * (D_DEF_TKA * D_DEF));

                    systemSH = DEF_BASE * (D_DEF_SH * D_DEF) +
                        ((normalizedSH / SH) * DEF_RATIO * (D_DEF_SH * D_DEF));
                }
            }

            globalNote = Math.Round(systemP + systemPIM + systemHits + systemBkS + systemTkA + systemSH, 2); 
        }
    }     
}
