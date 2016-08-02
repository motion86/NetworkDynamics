using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkDynamics
{
    public class Vertex : IComparable
    {
        public List<Edge> edges { get; private set; }
        private HashSet<Vertex> connects;

        public int vId { get; private set; }
        public int degree { get; private set; }
        public int inDegree { get; private set; }

        double signal;
        bool state;

        private static int count;

        public Vertex()
        {
            state = false;
            signal = 0;
            degree = 0;
            inDegree = 0;
            vId = count;
            count++;

            edges = new List<Edge>(5);
        }

        public bool addEdge(Vertex to)
        {
            if (!connects.Contains(to))
            {
                this.edges.Add(new Edge(this, to));
                this.degree++;
                to.incrInDeg();
                return true;
            }
            else return false;
        }

        public void incrInDeg()
        {
            inDegree++;
        }

        public int CompareTo(object o)
        {
            if (o == null) return 1;

            Vertex otherVertex = o as Vertex;
            if (otherVertex != null)
                return this.vId.CompareTo(otherVertex.vId);
            else
                throw new ArgumentException("Object is not a Vertex");
        }

        public override int GetHashCode()
        {
            return this.vId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.vId == ((Vertex)obj).vId;
        }
    }
}
