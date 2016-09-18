using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Media3D;
using System.Threading;
using System.Diagnostics;

namespace Math2016
{
    class Program
    {
        static void Main(string[] args)
        {
            args = new string[1];
            args[0] = @"C:\Users\Jingj\OneDrive\C\测试用例_数据_答案\sample_case002_input.txt";     
            Case c = new Case(args[0],"test.txt");
            Stopwatch s = new Stopwatch();
            s.Start();
            
            for (int i = 0; i < c.tCount; i++)
            {
                c.Init(i);
                c.Process(i);
                c.t[i].Pos = c.CalcCenter();
                c.t[i].pCnt = c.gPos.Count;
                Console.WriteLine($"{c.gPos.Count}\t{i}/{c.tCount}\tTime Remaining:{TimeSpan.FromMilliseconds((double)s.ElapsedMilliseconds/(double)(i+1)*(double)(c.tCount-i-1))}");
            }
            string[] test_out = new string[c.tCount];
            for (int i = 0; i < c.tCount; i++)
            {
                test_out[i] = $"{c.t[i].Pos.X},{c.t[i].Pos.Y},{c.t[i].Pos.Z},{c.t[i].pCnt}";
            }
            File.WriteAllLines("text4.csv", test_out);
            //Console.Read();

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
        public Vector3D Pos;
        public List<TtoS> d = new List<TtoS>();
        internal int pCnt;

        public Terminal(int id)
        {
            this.tid = id;
        }
    }

    public class Case
    {
        public int sCount;
        public int tCount;
        public List<Vector3D> sPos =new List<Vector3D>();
        public List<Vector3D> gPos = new List<Vector3D>();
        public List<Terminal> t = new List<Terminal>();
        string outname;
        public Case(string filename,string outfilename)
        {
            outname = outfilename;
            string[] lines = File.ReadAllLines(filename);
            sCount = int.Parse(lines[0]);
            tCount = int.Parse(lines[1]);
            for (int i = 0; i < sCount; i++)
            {
                string[] xyz = lines[i + 3].Split('\t');
                sPos.Add(new Vector3D(double.Parse(xyz[0]), double.Parse(xyz[1]), double.Parse(xyz[2])));
                Console.WriteLine($"Importing {sPos.Last()}");
            };
            for (int i = 0; i < tCount; i++)
            {
                string[] xyz = lines[i + 3 + sCount].Split('\t');
                Terminal tt = new Terminal(i);
                for (int j = 0; j < sCount; j++)
                {
                    tt.d.Add(new TtoS() { index = j, d = double.Parse(xyz[j]) * 3e8 });
                };
                tt.d.Sort();
                for (int j = 0; j < sCount; j++)
                {
                    if (j >= 3) tt.d[j].d = tt.d[j].d * 1.0;
                }
                t.Add(tt);
            };

        }

        public void Init(int i)
        {
            double o1o2 = (sPos[t[i].d[0].index] - sPos[t[i].d[1].index]).Length;
            Sphere A = new Sphere(t[i].d[0].d, sPos[t[i].d[0].index]);
            Sphere B = new Sphere(t[i].d[1].d, sPos[t[i].d[1].index]);
            this.gPos = Sphere.intersect(A, B, 1);
            return;
        }
        public void Process(int indexOfTerminal)
        {
            for (int i = 0; i < sCount; i++)
            {
                List<Vector3D> result = new List<Vector3D>();
                 for(int j=0;j<gPos.Count;j++)
                  {
                      if (isIn(indexOfTerminal, i, j)) result.Add(gPos[j]);
                  };
                if (result.Count == 0) return;
                else gPos = result;
            }
        }


        private bool isIn(int indexOfTerminal,int indexOfStation,int indexOfGrid)
        {
                if ((sPos[t[indexOfTerminal].d[indexOfStation].index] - gPos[indexOfGrid]).Length >t[indexOfTerminal].d[indexOfStation].d)
                    return false;
                else
                    return true;
        }

        public Vector3D CalcCenter()
        {
            double sumX = 0;
            double sumY = 0;
            double sumZ = 0;
            Parallel.ForEach<Vector3D>(this.gPos, (p) =>
            {
                sumX += p.X/this.gPos.Count;
                sumY += p.Y /this.gPos.Count;
                sumZ += p.Z/this.gPos.Count;
            });
            return new Vector3D(sumX, sumY ,sumZ);
        }

    }
    class Sphere
    {

        
        public double r;
        public Vector3D c;
        public Sphere(double radius,Vector3D p)
        {
            this.r = radius;
            this.c = p;
        }
        public static bool isAinB(Sphere A, Sphere B)
        {
            double r1 = A.r;
            double r2 = B.r;
            double d = (A.c - B.c).Length;
            if ((d + r1) <= r2) return true;
            else return false;
        }
        public static List<Vector3D> intersect(Sphere A,Sphere B,double grid)
        {
            double r1 = A.r;
            double r2 = B.r;
            double d = (A.c - B.c).Length;
            double cos = (r1 * r1 + d * d - r2 * r2) / (2 * r1 * d);
            double sin = Math.Sqrt(1 - cos * cos);
            double cross = r1 + r2 - d;
            List<Vector3D> result = new List<Vector3D>();
            if (isAinB(A, B))
            {
                int div = (int)(A.r / grid * 2);
                for (int i = 0; i < div + 1; i++) for (int j = 0; j < div + 1; j++) for (int k = 0; k < div + 1; k++)
                            result.Add(new Vector3D(A.c.X-A.r+(double)i/(double)div*2*A.r, A.c.Y - A.r + (double)i / (double)div * 2 * A.r, A.c.Z - A.r + (double)i / (double)div * 2 * A.r));
            }
            else
            {
                double Z = A.r + B.r - (A.c - B.c).Length;
                double R = r1 * sin;
                int nZ = (int)(Z / grid);
                int nR = (int)(R / grid);
                Vector3D UniZ = (B.c - A.c);
                Vector3D UniY = new Vector3D(0, 0, 0);
                Vector3D UniX = new Vector3D(0, 0, 0);

                UniZ.Normalize();
                Vector3D startPoint = A.c + UniZ * r1 * cos;
                if (Vector3D.AngleBetween(new Vector3D(0, 0, 1), UniZ) == 0 || Vector3D.AngleBetween(new Vector3D(0, 0, 1), UniZ) == 180)
                {
                    UniY = new Vector3D(1, 0, 0);
                }
                else if (Vector3D.AngleBetween(new Vector3D(0, 0, 1), UniZ) == 90) UniY = new Vector3D(0, 0, 1);
                else
                {
                    UniY = new Vector3D(0, 0, 1) - UniZ * (Math.Cos(Vector3D.AngleBetween(new Vector3D(0, 0, 1), UniZ) / 180 * Math.PI));
                    UniY.Normalize();
                }
                UniX = Vector3D.CrossProduct(UniY, UniZ);
                UniX.Normalize();

                for (int i = 0; i < nZ + 1; i++)
                    for (int j = -nR; j < nR + 1; j++)
                        for (int k = -nR; k < nR + 1; k++)
                            result.Add(startPoint + (double)i / (double)nZ * Z * UniZ + (double)j / (double)nR * R * UniX + (double)k / (double)nR * R * UniY);

            }
            
            
           
            return result;
        }
        public bool isIn(Vector3D p)
        {
            if ((p - c).Length <= r)
                return true;
            else
                return false;
        }





    }

    
   
}
