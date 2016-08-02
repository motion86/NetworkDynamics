using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkDynamics
{
    class ColorTools
    {
        private static Color[] colors = new Color[] { Color.Black, Color.Blue, Color.Cyan, Color.Yellow, Color.Red, Color.Transparent};
        public static Color IntToGradient(int i, int dynamicRange)
            // IntToGradient - pics a color from a color map with the specified dynamic range.
        {
            float coeff = (i * (colors.Length-2) / (float)dynamicRange);
            int index = (int)coeff;

            Color c0 = colors[index];
            Color c1 = colors[index + 1];

            float fraction = coeff - (int)coeff;

            byte R = (byte)((1 - fraction) * c0.R + fraction * c1.R);
            byte G = (byte)((1 - fraction) * c0.G + fraction * c1.G);
            byte B = (byte)((1 - fraction) * c0.B + fraction * c1.B);

            return Color.FromArgb(255, R, G, B);
        }

        public static Color getColor(int i)
        {
            return Color.FromArgb(getRGB(i));
        }

        private static int getRGB(int index)
        {
            int[] p = getPattern(index);
            return getElement(p[0]) << 16 | getElement(p[1]) << 8 | getElement(p[2]);
        }

        private static int getElement(int index)
        {
            int value = index - 1;
            int v = 0;
            for (int i = 0; i < 8; i++)
            {
                v = v | (value & 1);
                v <<= 1;
                value >>= 1;
            }
            v >>= 1;
            return v & 0xFF;
        }

        private static int[] getPattern(int index)
        {
            int n = (int)Math.Pow(index, (1.0 / 3.0));
            index -= (n * n * n);
            int[] p = new int[3];
            foreach (int i in LinearAlgebra.range(p.Length))
                p[i] = n;
           
            if (index == 0)
            {
                return p;
            }
            index--;
            int v = index % 3;
            index = index / 3;
            if (index < n)
            {
                p[v] = index % n;
                return p;
            }
            index -= n;
            p[v] = index / n;
            p[++v % 3] = index % n;
            return p;
        }
    }
}
