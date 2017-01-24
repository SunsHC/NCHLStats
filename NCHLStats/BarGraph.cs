using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCHLStats
{
    internal class BarGraph
    {
        public static string Generate(Dictionary<int, double> XYData, string title, int minX, int maxX, int scaleX, int minY, int maxY, int scaleY)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(title + "\n");

            for (int i = maxY; i > minY; i -= scaleY)
            {
                if (i - scaleY < 10)
                    sb.Append(string.Format("{0}  ", i - scaleY));
                else if (i - scaleY >= 10 && i - scaleY < 100)
                    sb.Append(string.Format("{0} ", i - scaleY));
                else if (i - scaleY >= 100)
                    sb.Append(string.Format("{0}", i - scaleY));

                for (int j = minX; j <= maxX; j += scaleX)
                {
                    if (XYData.ContainsKey(j))
                    {
                        
                        double yData = XYData[j];

                        if (yData >= i - scaleY)
                            sb.Append("██_");
                        else
                            sb.Append("___");
                    }
                }

                sb.Append("\n");
            }

            sb.Append("   ");
            for (int i = minX; i <= maxX; i += scaleX)
            {
                if (i < 10)
                    sb.Append(string.Format("{0}  ", i));
                else
                    sb.Append(string.Format("{0} ", i));
            }
            sb.Append("\n\n");

            return sb.ToString();
        }
    }
}
