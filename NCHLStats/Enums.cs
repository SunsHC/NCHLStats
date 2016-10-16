using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCHLStats
{
    [Flags]
    public enum StatsType
    {
        GP = 1,
        P = 2,
        PIM = 4,
        SH = 8,
        Hits = 16,
        TkA = 32,
        BkS = 64,
        TOI = 128
    }

    public enum PlayerPosition
    {
        G,
        D,
        C,
        L,
        R
    }

    public enum NCHLTeam
    {
        AGL,
        SUN,
        REB,
        PAC,
        RAM,
        BUC
    }

    public enum NHLTeam
    {
        NON,
        ANA,
        ARI,
        BOS,
        BUF,
        CAR,
        CBJ,
        CGY,
        CHI,
        COL,
        DAL,
        DET,
        EDM,
        FLA,
        LAK,
        MIN,
        MTL,
        NJD,
        NSH,
        NYI,
        NYR,
        OTT,
        PHI,
        PIT,
        SJS,
        STL,
        TBL,
        TOR,
        VAN,
        WPG,
        WSH
    }
}
