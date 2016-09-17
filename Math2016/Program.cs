using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Math2016
{
    class Program
    {
        static void Main(string[] args)
        {
            args = new string[1];
            args[0] = @"C:\Users\Jingj\OneDrive\C\竞赛用例_数据\case001_input.txt";

            string[] lines = File.ReadAllLines(args[0]);
            Case c = new Case();
            c.sCount = int.Parse(lines[0]);
            c.tCount = int.Parse(lines[1]);
            for(int i=0;i<c.sCount;i++)
            {
                string[] xyz = lines[i + 3].Split('\t');
                c.sPos.Add(new P3D(double.Parse(xyz[0]),double.Parse(xyz[1]),double.Parse(xyz[2])));
                Console.WriteLine($"Importing {c.sPos.Last()}");
            };
            Parallel.For(0, c.tCount, i =>
            {
                string[] xyz = lines[i + 3+c.sCount].Split('\t');
                Terminal t = new Terminal(i);
                Parallel.For(0, c.sCount, j =>
                {
                    t.d.Add(new TtoS() {index=j, d=double.Parse(xyz[j]) * 3e8 });
                    
                });
                t.d.Sort();
                c.t.Add(t);
               
            });
            c.init(c.t[0]);


            Console.Read();

        }
    }

    public class TtoS:IComparable
    {
        public int index { get; set; }
        public double d { get; set; }
        public int CompareTo(object obj)
        {
            TtoS o = obj as TtoS;
            if (this.d > o.d) return 1;
            else if (this.d < o.d) return -1;
            else return 0;
        }
        
    }

    public class Terminal
    {
        public int tid;
        public P3D Pos;
        public List<TtoS> d = new List<TtoS>();
        public Terminal(int id)
        {
            this.tid = id;
        }
    }

    public class Case
    {
        public int sCount;
        public int tCount;
        public List<P3D> sPos =new List<P3D>();
        public List<P3D> gPos = new List<P3D>();
        public List<Terminal> t = new List<Terminal>();
        public Case()
        {


        }

        public void init(Terminal t)
        {
            P3D min = new P3D(sPos[t.d[0].index].X-t.d[0].d<= sPos[t.d[1].index].X - t.d[1].d? sPos[t.d[0].index].X - t.d[0].d: sPos[t.d[1].index].X - t.d[1].d, sPos[t.d[0].index].Y - t.d[0].d <= sPos[t.d[1].index].Y - t.d[1].d ? sPos[t.d[0].index].Y - t.d[0].d : sPos[t.d[1].index].Y - t.d[1].d, sPos[t.d[0].index].Z - t.d[0].d <= sPos[t.d[1].index].Z - t.d[1].d ? sPos[t.d[0].index].Z - t.d[0].d : sPos[t.d[1].index].Z - t.d[1].d);
            P3D max = new P3D(sPos[t.d[0].index].X +t.d[0].d >= sPos[t.d[1].index].X +t.d[1].d ? sPos[t.d[0].index].X +t.d[0].d : sPos[t.d[1].index].X +t.d[1].d, sPos[t.d[0].index].Y +t.d[0].d >= sPos[t.d[1].index].Y +t.d[1].d ? sPos[t.d[0].index].Y +t.d[0].d : sPos[t.d[1].index].Y +t.d[1].d, sPos[t.d[0].index].Z +t.d[0].d >= sPos[t.d[1].index].Z +t.d[1].d ? sPos[t.d[0].index].Z +t.d[0].d : sPos[t.d[1].index].Z +t.d[1].d);
            int xdiv = (int)(max.X - min.X);
            int ydiv = (int)(max.Y - min.Y);
            int zdiv = (int)(max.Z - min.Z);
            double incX = (max.X - min.X) / xdiv;
            double incY = (max.Y - min.Y) / ydiv;
            double incZ = (max.Z - min.Z) / zdiv;
            for(int i=0;i<xdiv+1;i++)for(int j=0;j<ydiv+1;j++)for(int k=0;i<zdiv+1;k++)
            gPos.Add(new P3D(min.X+i*incX,min.Y+i*incY,min.Z+i*incZ));


        }

        public P3D CalcCenter()
        {
            double sumX = 0;
            double sumY = 0;
            double sumZ = 0;
            Parallel.ForEach<P3D>(this.gPos, (p) =>
            {
                sumX += p.X;
                sumY += p.Y;
                sumZ += p.Z;
            });
            return new P3D(sumX / this.gPos.Count, sumY / this.gPos.Count, sumZ / this.gPos.Count);
        }

    }

    


    class Sphere
    {
        public double r;
        public P3D c;
        public Sphere(double radius,P3D p)
        {
            this.r = radius;
            this.c = p;
        }
        public static List<P3D> intersect(Sphere A,Sphere B,double grid)
        {
            List<P3D> result = new List<P3D>();


            return result;
        }

    }

    public class P3D
    {
        public double X;
        public double Y;
        public double Z;
        public P3D(double a, double b, double c)
        {
            X = a;
            Y = b;
            Z = c;

        }
        public static P3D operator+(P3D a,P3D b)
        {
            return new P3D(a.X+b.X,a.Y+b.Y,a.Z+b.Z); 
        }
        public static P3D operator -(P3D a, P3D b)
        {
            return new P3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        public static P3D operator *(P3D a, double b)
        {
            return new P3D(a.X*b, a.Y*b, a.Z*b);
        }
        public static P3D operator *(double b,P3D a)
        {
            return new P3D(a.X * b, a.Y * b, a.Z * b);
        }
        public static P3D operator /(P3D a, double b)
        {
            return new P3D(a.X/b, a.Y/b, a.Z/b);
        }
        public static double operator ~(P3D a)
        {
            return (Math.Sqrt(a.X*a.X+a.Y*a.Y+a.Z*a.Z));
        }
        public override string ToString()
        {
            return $"{X},\t{Y},\t{Z}";
        }
    }
}
