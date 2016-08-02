using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkDynamics
{
    public class Edge : IComparable
    {
        public Vertex from { get; private set; }
        public Vertex to { get; private set; }

        double weight;

        public int numId { get; private set; }
        public string sId { get; private set; }

        private static int count;

        public Edge(Vertex from, Vertex to)
        {
            sId = from.vId.ToString() + " " + to.vId.ToString();
            numId = count;
            count++;

            this.from = from;
            this.to = to;

            weight = 1d;
        }

        public Edge(Vertex from, Vertex to, double weight)
            : this(from, to)
        {
            this.weight = weight;
        }

        public int CompareTo(object o)
        {
            if (o == null) return 1;

            Edge otherEdge = o as Edge;
            if (otherEdge != null)
                return this.weight.CompareTo(otherEdge.weight);
            else
                throw new ArgumentException("Object is not a Vertex");
        }

        public override int GetHashCode()
        {
            return this.sId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.sId.CompareTo(((Edge)obj).sId) == 0;
        }
    }
}
