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
                c.t[i].pCnt = c.gPos.Count();
                Console.WriteLine($"{c.gPos.Count()}\t{i}/{c.tCount}\tTime Remaining:{TimeSpan.FromMilliseconds((double)s.ElapsedMilliseconds/(double)(i+1)*(double)(c.tCount-i-1))}");
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
        public Grid gPos;
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
            this.gPos = Sphere.Intersect(A, B, 1);
            return;
        }
        public void Process(int indexOfTerminal)
        {
            for (int indexOfStation = 0; indexOfStation < sCount; indexOfStation++)
            {
                Grid result = new Grid();
                result = gPos;

                Parallel.For(0, gPos.XCount + 1, i =>
                {
                    Parallel.For(0, gPos.YCount + 1, j =>
                    {
                        Parallel.For(0, gPos.ZCount + 1, k =>
                        {
                            if (isIn(indexOfTerminal, indexOfStation, gPos.GetPoint(i, j, k)))
                                result.Flag[i, j, k] = true;
                            else result.Flag[i, j, k] = false;

                        });
                    });
                });
                bool testflag = false;
                Parallel.For(0, gPos.XCount + 1, i =>
                {
                    Parallel.For(0, gPos.YCount + 1, j =>
                    {
                        Parallel.For(0, gPos.ZCount + 1, k =>
                        {
                            testflag = testflag | result.Flag[i, j, k];

                        });
                    });
                });
                if (testflag == false) return;
                else gPos = result;
            }
        }


        private bool isIn(int indexOfTerminal,int indexOfStation,Vector3D testPoint)
        {
                if ((sPos[t[indexOfTerminal].d[indexOfStation].index] - testPoint).Length >t[indexOfTerminal].d[indexOfStation].d)
                    return false;
                else
                    return true;
        }

        public Vector3D CalcCenter()
        {
            double sumX = 0;
            double sumY = 0;
            double sumZ = 0;
            int Cnt = gPos.Count();
            for (int i = 0; i < gPos.XCount + 1; i++) for (int j = 0; j < gPos.YCount + 1; j++) for (int k = 0; k < gPos.ZCount + 1; k++)
                    {
                        if (gPos.Flag[i, j, k])
                        {
                            Vector3D p = gPos.GetPoint(i, j, k);
                            sumX += p.X / Cnt;
                            sumY += p.Y / Cnt;
                            sumZ += p.Z / Cnt;
                        }
                    }
                
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
        public static Grid Intersect(Sphere A,Sphere B,double grid)
        {
            double r1 = A.r;
            double r2 = B.r;
            double d = (A.c - B.c).Length;
            double cos = (r1 * r1 + d * d - r2 * r2) / (2 * r1 * d);
            double sin = Math.Sqrt(1 - cos * cos);
            double cross = r1 + r2 - d;
            
            if (isAinB(A, B))
            {
                return new Grid(2 * A.r, 2 * A.r, 2 * A.r, new Vector3D(0, 0, 1), A.c - new Vector3D(0, 0, A.r), 1);
            }
            else
            {
                double Z = A.r + B.r - (A.c - B.c).Length;
                double R = r1 * sin;
                int nZ = (int)(Z / grid);
                int nR = (int)(R / grid);
                Vector3D UniZ = (B.c - A.c);
                UniZ.Normalize();
                Vector3D startPoint = A.c + UniZ * r1 * cos;
                return new Grid(2 * R, 2 * R, Z, UniZ, startPoint, 1);

            }
        }
        public bool isIn(Vector3D p)
        {
            if ((p - c).Length <= r)
                return true;
            else
                return false;
        }





    }

    public class Grid
    {

        public Vector3D O;
        public Vector3D X;
        public Vector3D Y;
        public Vector3D Z;
        public int XCount;
        public int YCount;
        public int ZCount;
        public double Xdiv;
        public double Ydiv;
        public double Zdiv;
        public double Height;
        public double Width;
        public double Length;
        public double epsilon;
        public bool[,,] Flag;
        public List<Vector3D> Points;
        public Grid(double L,double W,double H,Vector3D z,Vector3D Origin,double e)
        {
            XCount =(int)( L / e);
            YCount = (int)(W / e);
            ZCount = (int)(H / e);
            Xdiv = L / XCount;
            Ydiv = W / YCount;
            Zdiv = H / ZCount;
            Length = L;
            Width = W;
            Height = H;
            epsilon = e;
            Flag = new bool[XCount+1,YCount+1,ZCount+1];
            for (int i = 0; i < XCount + 1; i++) for (int j = 0; j < YCount + 1; j++) for (int k = 0; k < ZCount + 1; k++)
                        Flag[i, j, k] = true;
            O = Origin;
            Z = z;
            Z.Normalize();
            if (Vector3D.AngleBetween(new Vector3D(0, 0, 1), Z) == 0 || Vector3D.AngleBetween(new Vector3D(0, 0, 1), Z) == 180)
            {
                Y = new Vector3D(1, 0, 0);
            }
            else if (Vector3D.AngleBetween(new Vector3D(0, 0, 1), Z) == 90) Y = new Vector3D(0, 0, 1);
            else
            {
                Y = new Vector3D(0, 0, 1) - Z * (Math.Cos(Vector3D.AngleBetween(new Vector3D(0, 0, 1), Z) / 180 * Math.PI));
                Y.Normalize();
            }
            X = Vector3D.CrossProduct(Y, Z);
            X.Normalize();
            Points = new List<Vector3D>();

        }
        public Grid()
        { }
        public Vector3D GetPoint(int i, int j,int k)
        {
            return (O+i*Xdiv*X-0.5*Length*X+j*Ydiv*Y - 0.5 * Width * Y + k*Zdiv*Z);
        }
        public int Count()
        {
            int sum = 0;
            for (int i = 0; i < XCount + 1; i++) for (int j = 0; j < YCount + 1; j++) for (int k = 0; k < ZCount + 1; k++)
                        if (Flag[i, j, k] == true) sum++;
            return sum;
        }
        

    }

    
   
}
