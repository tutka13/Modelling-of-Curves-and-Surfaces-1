using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BezierovoOrezavanie
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int stupenPolynomu;

        List<double> Koeficienty;
        List<Point> RiadiaceVrcholy;
        List<double> Korene;

        double start_a, end_b;
        double width, height;
        int epsilon;
        SolidColorBrush OranzovaFarba;
        
        public MainWindow()
        {
            InitializeComponent();
            Koeficienty = new List<double>();
            RiadiaceVrcholy = new List<Point>();
            Korene = new List<double>();

            width = g.Width;
            height = g.Height;
            epsilon = 10;

            OranzovaFarba = new SolidColorBrush(Colors.DarkOrange);
            OranzovaFarba.Opacity = 0.8;
        }
        public void PolynomProcessing()
        {
            string InputText;
            InputText = Polynom.Text.Replace(" ", "").Replace(".", ",");

            List<char> CharList = new List<char>(InputText.ToCharArray());
            List<string> StringMonomials = new List<string>();

            Koeficienty.Clear();
            Korene.Clear();

            //vytvorenie monomov
            while (CharList.Count > 0)
            {
                int index = 0;
                for (int i = 1; i < CharList.Count; i++)
                {
                    if (CharList[i] == '+' || CharList[i] == '-')
                    {
                        index = i;
                        break;
                    }
                }

                if (index > 0)        // odstranenie monomu 
                {
                    StringMonomials.Add(new string(CharList.ToArray(), 0, index));
                    CharList.RemoveRange(0, index);                                     //skopiruje dany monom a zmaze sa z listu charov
                }
                else                //bez znamienka
                {
                    StringMonomials.Add(new string(CharList.ToArray()));               //skopiruje uz cely zvysok pola
                    CharList.Clear();
                }
            }

            for (int i = 0; i < StringMonomials.Count; i++)
            {
                int tPosition = StringMonomials[i].IndexOf('t');

                if (tPosition == -1) //t tam nie je, mame konstantny clen
                {
                    if (Koeficienty.Count == 0)
                    {
                        Koeficienty.Add(Convert.ToDouble(StringMonomials[i]));
                    }
                    else
                    {
                        Koeficienty[0] = Convert.ToDouble(StringMonomials[i]);
                    }
                }
                else //mame nejaku mocninu t
                {
                    string[] part = StringMonomials[i].Split('t');
                    double koeficient;
                    int exponent;

                    if (part[0] == "+" || String.IsNullOrEmpty(part[0]))
                    {
                        koeficient = 1.0;
                    }
                    else if (part[0] == "-")
                    {
                        koeficient = -1.0;
                    }
                    else
                    {
                        koeficient = Convert.ToDouble(part[0]);
                    }


                    if (String.IsNullOrEmpty(part[1]))
                    {
                        exponent = 1;
                    }
                    else
                    {
                        part[1] = part[1].Replace("^", "");
                        exponent = Convert.ToInt32(part[1]);
                    }

                    if (Koeficienty.Count - 1 < exponent)
                    {
                        Koeficienty.InsertRange(Koeficienty.Count, new double[exponent - Koeficienty.Count + 1]);
                    }

                    Koeficienty[exponent] = koeficient;

                }
            }
            stupenPolynomu = Koeficienty.Count - 1;
            if (stupenPolynomu == 0)
            {
                if (Koeficienty[0] != 0)
                {
                    MessageBox.Show("Vstupný polynóm je konštantný a nemá korene");
                }
                else
                {
                    MessageBox.Show("Vstupný polynóm je konštantný a rovný nule");
                }
            }
        }
        public void IntervalProcessing()
        {
            string[] Input = Interval.Text.Replace(" ", "").Replace(".", ",").Split(';');       //bodkociarka sluzi na rozdelenie dolnej a hornej hranice intervalu
            start_a = Convert.ToDouble(Input[0]);
            end_b = Convert.ToDouble(Input[1]);
        }
        public double Binomial(int n, int k)    // rekurentny vzorec pre kombinacne cislo
        {
            if (k == 0)
            {
                return 1.0;
            }
            if (n == k)
            {
                return 1.0;
            } 
            return Binomial(n - 1, k) + Binomial(n - 1, k - 1);
        }
        public double Power(double k, int l)
        {
            if (l == 1)
            {
                return k;
            }
            if (l == 0)
            {
                return 1.0;
            }
            if (k == 0)
            {
                return 0;
            }
            return k * Power(k, l - 1);
        }
        public double PolarFormX(double a, double b, int k)    // vypocet polarnej formy pre X suradnicu
        {
            return (k * a) / stupenPolynomu + ((stupenPolynomu - k) * b) / stupenPolynomu;
        }
        public double PolarFormY(double a, double b, int k)     // vypocet polarnej formy pre Y suradnicu
        {
            double isum = 0;
            for (int i = stupenPolynomu; i > 0; i--)
            {
                double jsum = 0;
                for (int j = Math.Min(i, k); i - j <= stupenPolynomu - k && j >= 0; j--)
                {
                    jsum += Binomial(k, j) * Binomial(stupenPolynomu - k, i - j) * Power(a, j) * Power(b, i - j);
                }
                isum += Koeficienty[i] / Binomial(stupenPolynomu, i) * jsum;
            }
            isum += Koeficienty[0]; ;
            return isum;
        }
        public List<Point> CreateVertices(double a, double b)       // vytvorenie riadiacich vrcholov pre polynom - bezierovu krivku
        {
            List<Point> CV = new List<Point>();
            for (int i = 0; i <= stupenPolynomu; i++)
            {
                Point V = new Point();
                V.X = PolarFormX(a, b, stupenPolynomu - i);
                V.Y = PolarFormY(a, b, stupenPolynomu - i);
                CV.Add(V);
            }
            return CV;
        }
        public double TransformX(double x)  // transformacia suradnice x a skalovanie a posunutie do stredu canvasu
        {
            return (x - start_a) / (end_b - start_a) * width * 0.9 + width * 0.05;
        }
        public double TransformY(double y)  // transformacia suradnice y a skalovanie a posunutie do stredu canvasu
        {
            double yMax;                    // najde max a min hodnotu y spomedzi riadiacich vrcholov
            double yMin;
            if (RiadiaceVrcholy.Count > 0)
            {
                yMax = RiadiaceVrcholy[0].Y;
                yMin = RiadiaceVrcholy[0].Y;

                for (int i = 1; i < RiadiaceVrcholy.Count; i++)
                {
                    if (yMax < RiadiaceVrcholy[i].Y)
                    {
                        yMax = RiadiaceVrcholy[i].Y;
                    }
                    if (yMin > RiadiaceVrcholy[i].Y)
                    {
                        yMin = RiadiaceVrcholy[i].Y;
                    }    
                }
            }
            else    // pripad konstanty
            {
                yMax = Math.Max(0, Koeficienty[0]);
                yMin = Math.Min(0, Koeficienty[0]);
            }

            if (yMax == yMin)       // pripad  0
            {
                yMax += 1;
                yMin -= 1;
            }
            return (yMax - y) / (yMax - yMin) * height * 0.9 + height * 0.05;
        }
        public Point TransformPoint(Point P)
        {
            Point A = new Point();
            A.X = TransformX(P.X);
            A.Y = TransformY(P.Y);
            return A;
        }
        public List<Point> TransformListPoint(List<Point> CV)       // transformuje body v liste
        {
            List<Point> TransformovaneBody = new List<Point>();
            for (int i = 0; i < CV.Count; i++)
            {
                Point A = new Point();
                A.X = TransformX(CV[i].X);
                A.Y = TransformY(CV[i].Y);
                TransformovaneBody.Add(A);
            }
            return TransformovaneBody;
        }
        public void DrawPoints(List<Point> CV)  // vykresli vsetky body v liste
        {
            for (int i = 0; i < CV.Count; i++)
            {
                Ellipse E = new Ellipse();
                E.Fill = OranzovaFarba;
                E.Width = 4;
                E.Height = 4;
                Canvas.SetLeft(E, TransformX(CV[i].X) - 2);
                Canvas.SetTop(E, TransformY(CV[i].Y) - 2);
                Canvas.SetZIndex(E, 3);
                g.Children.Add(E);
            }
        }
        public void DrawLine(Point V1, Point V2, SolidColorBrush farba) // vykresli usecku
        {
            Line L = new Line();
            L.Stroke = farba;
            L.X1 = V1.X;
            L.X2 = V2.X;
            L.Y1 = V1.Y;
            L.Y2 = V2.Y;
            Canvas.SetZIndex(L, 1);
            g.Children.Add(L);
        }
        public void DrawLinesandAxis()
        {
            if (RiadiaceVrcholy.Count > 1)
            {
                //vykreslenie riadiaceho polygonu
                for (int i = 0; i < RiadiaceVrcholy.Count - 1; i++)
                {
                    DrawLine(TransformPoint(RiadiaceVrcholy[i]), TransformPoint(RiadiaceVrcholy[i + 1]), OranzovaFarba);
                }
            }

            //vykreslenie suradnicovych osi
            Point O = TransformPoint(new Point(0, 0));

            DrawLine(new Point(0, O.Y), new Point(width, O.Y), new SolidColorBrush(Colors.DarkGray));
            DrawLine(new Point(O.X, 0), new Point(O.X, height), new SolidColorBrush(Colors.DarkGray));

            //vykreslenie jednotiek na x-ovej osi
            for (int i = 0; i <= 10; i++)
            {
                Point P = new Point();
                P.X = start_a + i* (end_b - start_a) / 10;
                P.Y = 0.0;

                Ellipse E = new Ellipse();
                E.Fill = new SolidColorBrush(Colors.Gray);
                E.Width = 2;
                E.Height = 2;
                Canvas.SetLeft(E,TransformX(P.X) - 1);
                Canvas.SetTop(E, TransformY(P.Y) - 1);
                Canvas.SetZIndex(E, 4);
                g.Children.Add(E);

                TextBlock Cislo = new TextBlock();
                Cislo.Text = Convert.ToString(Math.Round(P.X,2));
                Cislo.FontSize = 10;
                Cislo.Foreground = new SolidColorBrush(Colors.Gray);
                Canvas.SetLeft(Cislo, TransformX(P.X) - 4);
                Canvas.SetTop(Cislo, TransformY(P.Y) + 2);
                Canvas.SetZIndex(Cislo, 2);
                g.Children.Add(Cislo);
            }
        }

        public List<List<Point>> CalculatePoints(List<Point> CV)    // rozdeli krivku a prepocita body na vykreslovanie
        {
            List<List<Point>> b = new List<List<Point>>();
            List<List<Point>> R = new List<List<Point>>();
            Point M;
            b.Add(CV);

            for (int i = 1; i < stupenPolynomu + 1; i++)
            {
                b.Add(new List<Point>());
                for (int j = 0; j < stupenPolynomu + 1 - i; j++)
                {
                    M = new Point();
                    M.X = b[i - 1][j].X / 2 + b[i - 1][j + 1].X / 2;
                    M.Y = b[i - 1][j].Y / 2 + b[i - 1][j + 1].Y / 2;
                    b[i].Add(M);
                }
            }

            R.Add(new List<Point>());
            for (int i = 0; i < stupenPolynomu + 1; i++)
            {
                R[0].Add(b[i][0]);
            }

            R.Add(new List<Point>());
            for (int i = 0; i < stupenPolynomu + 1; i++)
            {
                R[1].Add(b[i][stupenPolynomu + 1 - i - 1]);
            }
            return R;
        }

        public double MaxDistance(List<Point> CV)           // vypocita maximalnu vzdialenost medzi bodmi v liste
        {
            double d, max = 0;
            for (int i = 0; i < CV.Count - 1; i++)
            {
                d = Math.Sqrt(Math.Pow(CV[i + 1].X - CV[i].X, 2) + Math.Pow(CV[i + 1].Y - CV[i].Y, 2));
                if (d > max)
                {
                    max = d;
                }
            }
            return max;
        }

        public void DrawCurve(List<Point> CV)           // vykreslenie krivky
        {
            List<List<Point>> BezieroveListy;
            bool bodySuEpsilonBlizko = true;

            if (MaxDistance(TransformListPoint(CV)) >= epsilon)
            {
               BezieroveListy = CalculatePoints(CV);
               DrawCurve(BezieroveListy[0]);
               DrawCurve(BezieroveListy[1]);
               bodySuEpsilonBlizko = false;
            }

            if (bodySuEpsilonBlizko)
            {
                for (int i = 0; i < stupenPolynomu; i++)
                {
                    DrawLine(TransformPoint(CV[i]), TransformPoint(CV[i + 1]), new SolidColorBrush(Colors.Blue));
                }
            }
        }
        private double IntersectionWithAxisX(Point A, Point B)
        {
            double t = -A.Y / (B.Y - A.Y);
            return A.X + t * (B.X - A.X);
        }

        private void BezierCutting(List<Point> CV, double a, double b)
        {
            double epsilonStop = 0.0001;
            double epsilonSplit = 0.000001;
            double tMin = CV[CV.Count - 1].X;
            double tMax = CV[0].X;

            for (int i = 0; i < CV.Count; i++)
            {
                for (int j = i; j < CV.Count; j++)
                {
                    if (CV[i].Y * CV[j].Y <= 0)     // ak su z opacnych polrovin
                    {
                        double intersection = IntersectionWithAxisX(CV[i], CV[j]);
                        if (intersection < tMin)
                        {
                            tMin = intersection;
                        }
                        if (intersection > tMax)
                        {
                            tMax = intersection;
                        }
                    }
                }
            }
            if (tMax < tMin)
            {
                return;
            }
            else if (tMax - tMin < epsilonStop)     // dlzka intervalu je mensia ako epsilonStopn
            {
                Korene.Add(tMax);                   // interval je maly, a preto ho prehlasime za jeden bod
            }
            else // dlzka intervalu je vacsia ako epsilonStopn
            {
                if ((b - a) - (tMax + epsilonStop / 4 - (tMin - epsilonStop / 4)) > epsilonSplit)       // a zaroven sa zmenila od poslednej iteracie o viac ako epsilonSplit
                {
                    BezierCutting(CreateVertices(tMin - epsilonStop / 4, tMax + epsilonStop / 4), tMin - epsilonStop / 4, tMax + epsilonStop / 4);  // prehladavame mensi rozsireny interval
                }
                else  // a zaroven sa zmenila od poslednej iteracie o menej ako epsilonSplit
                {
                    BezierCutting(CreateVertices(tMin - epsilonStop / 4, (a + b) / 2), tMin - epsilonStop / 4, (a + b) / 2);    // interval rozdelime na dva a hladame korene v oboch
                    BezierCutting(CreateVertices(((a + b) / 2) - epsilonStop / 2, tMax + epsilonStop / 4), ((a + b) / 2) - epsilonStop / 2, tMax + epsilonStop / 4);
                }
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            g.Children.Clear();
            RiadiaceVrcholy.Clear();
            Korene.Clear();
            Koeficienty.Clear();
            Roots.Content = "";
        }

        private void FindRoots_Click(object sender, RoutedEventArgs e)
        {
            g.Children.Clear();
            RiadiaceVrcholy.Clear();
            Korene.Clear();
            Koeficienty.Clear();

            IntervalProcessing();
            PolynomProcessing();
            RiadiaceVrcholy = CreateVertices(start_a, end_b);
            DrawPoints(RiadiaceVrcholy);
            DrawLinesandAxis();
            DrawCurve(RiadiaceVrcholy);
            BezierCutting(RiadiaceVrcholy, start_a, end_b);
            DrawandWriteRoots();
        }

        private void DrawandWriteRoots()        // nakresli a vypise korene
        {
            for (int i = 0; i < Korene.Count; i++)
            {
                Ellipse E = new Ellipse();
                E.Fill = new SolidColorBrush(Colors.Red);
                E.Width = 6;
                E.Height = 6;
                Canvas.SetLeft(E, TransformX(Korene[i]) - 3);
                Canvas.SetTop(E, TransformY(0) - 3);
                Canvas.SetZIndex(E, 5);
                g.Children.Add(E);
            }

            Korene.Sort();
            Roots.Content = "";
            for (int i = 0; i < Korene.Count - 1; i++)
            {
                if (Math.Abs(Korene[i] - Korene[i + 1]) < 0.0001)   // ak su korene prilis blizko
                {
                    Korene[i + 1] = Korene[i];
                    Korene.RemoveAt(i);
                    i--;
                }
            }
            if (Korene.Count > 0)
            {
                for (int i = 0; i < Korene.Count; i++)
                {
                    Roots.Content += "t" + i.ToString() + " = " + Math.Round(Korene[i], 4).ToString() + "\n";
                }
            }
            else
            {
                Roots.Content += "Nenašli sme žiadne korene.";
            }
        }
    }
}
