using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkDynamics
{
    public class VertexData
    // VertexData - contains information about color and size of vertices {vertex #, #inbound, #outbound}
    {
        public Color onColor { get; set; }
        public Color offColor { get; set; }
        public Color borderColor { get; set; }
        public UInt16 onSize { get; set; }
        public UInt16 offSize { get; set; }
        public UInt16 borderSize { get; set; }

        public UInt16 vertexNum { get; set; }
        public UInt16 inBound { get; set; }
        public UInt16 outBound { get; set; }
        public Int16 conncetivity { get; set; }

        private static UInt16 Num;
        public static List<int> rank = new List<int>();


        //public Color offColorL { get; set; }
        //public Color outColorL { get; set; }
        //public Color inColorL { get; set; }
        //public Color symColorL { get; set; }

        public VertexData()
        {
            onColor = Color.Red;
            offColor = Color.Black;
            borderColor = Color.Black;
            onSize = 6;
            offSize = 2;
            borderSize = 0;

            vertexNum = Num;
            Num++;
            inBound = 0;
            outBound = 0;

            conncetivity = -1;
            //rank = null;

        }


        public static void addRank(int index) { rank.Add(index); }
        public static List<int> getRank() { return rank; }
        public static int getRank(int index) { return rank[index]; }
        public static void ClearRank() { rank.Clear(); }
        public static void Clear() { Num = 0; }

    }   
}
