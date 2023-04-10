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

namespace B_splajn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Menitko Menic;                       //nastroj na hybanie bodmi

        public bool NodesAreCorrect;                //pomocna premenna pre chybove hlasky

        public List<Vrchol> ControlVertices;        //riadiace vrhcoly
        public List<Line> ControlPolygone;          //riadiaca lomena ciara
        public List<int> Nodes;                     //uzlovy vektor
        public List<Segment> Spline;                //zoznam segmentov splajnu

        public static int p;                        //stupen krivky
        public static int LOD;                      //pocet vzoriek
        public static double t;                     //parameter t

        public MainWindow()
        {
            InitializeComponent();              //vytvorenie listov a nastavenie kompenentov pre stupen p = 3 a pocet vzoriek 30
            NodesAreCorrect = true;
            ControlVertices = new List<Vrchol>();
            ControlPolygone = new List<Line>();
            Nodes = new List<int>();
            Spline = new List<Segment>();

            p = 3;
            pStupenKrivky.Content = Convert.ToString(p);
            nPocetVrcholov.Content = 0;
            PocetVrcholov.Content = 0;

            CreateNodes();
            mPocetUzlov.Content = Nodes.Count;
            PocetUzlov.Content = Nodes.Count;

            LOD = 50;
            LODTextBox.Text = Convert.ToString(LOD);

            ParameterT.Content = "";
            EditSliderT();
        }
        private void CreateNodes()      // vytvorenie uzlov (na zaciatku programu)
        {
            NodesTextBox.Text = "";
            for (int i = 0; i < p + 1; i++)
            {
                Nodes.Add(i);
                NodesTextBox.Text += Convert.ToString(i) + ", ";
            }
        }

        private void Nodes_KeyDown(object sender, KeyEventArgs e)   // zmenu uzlovej postupnosti treba potvrdit enterom
        {
            if (e.Key == Key.Enter)
            {
                ChangeNodes();

                if (NodesAreCorrect)
                {
                    RedrawSpline();
                }
            }
        }

        void g_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Menic == null)   // ak nemenim poziciu uz existujuceho bodu
            {
                if (NodesAreCorrect)
                {
                    Vrchol A = new Vrchol(e.GetPosition(g), g, this);       // pridanie bodu do canvasu a listu
                    ControlVertices.Add(A);

                    nPocetVrcholov.Content = ControlVertices.Count;         // prepisanie komponentov 
                    PocetVrcholov.Content = ControlVertices.Count;
                    AddNode();

                    if (ControlVertices.Count > 1)                          // kreslenie riadiaceho polygonu
                    {
                        DrawLine(ControlVertices[ControlVertices.Count - 2].V, ControlVertices[ControlVertices.Count - 1].V, Colors.DarkBlue, ControlPolygone);
                    }

                    if (ControlVertices.Count > p)                          // kreslenie krivky
                    {
                        Segment S = new Segment(ControlVertices.Count - p - 1, ControlVertices, Nodes, g);
                        Spline.Add(S);
                        EditSliderT();                                      // nastavenie hranic parametra t
                    }
                }
            }
        }

        void EditSliderT()      // nastavenie hranic parametra t
        {
            SliderT.Minimum = Nodes[p];
            SliderT.Maximum = Nodes[Nodes.Count - p - 1];
            ParameterT.Content = "";
        }

        void g_MouseMove(object sender, MouseEventArgs e)
        {
            if (Menic != null)
            {
                Menic(e.GetPosition(g));     //menim polohu vrchola

                for (int i = 0; i < ControlPolygone.Count; i++)  //prekresluju sa uz existujuce ciary spajajuce riad.vrcholy
                {
                    ControlPolygone[i].X1 = ControlVertices[i].V.X;
                    ControlPolygone[i].Y1 = ControlVertices[i].V.Y;
                    ControlPolygone[i].X2 = ControlVertices[i + 1].V.X;
                    ControlPolygone[i].Y2 = ControlVertices[i + 1].V.Y;
                }
                RedrawSpline();
            }
        }

        void g_MouseUp(object sender, MouseButtonEventArgs e)       // po zdvihnuti sa prestane cokolvek diat
        {
            Menic = null;
        }

        public void RedrawSpline()
        {
            //tato metoda je potrebna, ak pouzivatel zmeni uzlovu postupnost, stupen krivky alebo polohu bodov
            if (Spline != null)
            {
                // najprv vymazem usecky, pomocou ktorych sa kreslili splajny a potom aj segmenty
                foreach (Segment S in Spline)
                {
                    foreach (Line L in S.SegmentLines)
                        g.Children.Remove(L);
                }
                Spline.Clear();

                // vytvorim tolko segmentov, kolko prislucha k danemu poctu vrcholov
                for (int i = 0; i < ControlVertices.Count - p; i++)
                {
                    Spline.Add(new Segment(i, ControlVertices, Nodes, g));
                }
            }
        }
        public void ChangeNodes()
        {
            string textik = NodesTextBox.Text;          //v stringu si ulozim aktualny obsah textboxu
            int j = 0;                                  //ktoru poziciu v zozname prepisujem
            NodesAreCorrect = true;

            string pamataj = "";
            for (int i = 0; i < textik.Length; i++)     //v tomto cykle vyberiem z textoveho okna len cele cisla
            {
                if (Char.IsDigit(textik[i]))
                {
                    if (j + 1 > Nodes.Count)            //ak je v textboxe viac cisel(uzlov) nez treba
                    {
                        NodesAreCorrect = false;
                        MessageBox.Show("Zadal si priveľa uzlov!");
                        break;
                    }

                    pamataj += Convert.ToString(textik[i]);     //zretazim cisla ako text
                    Nodes[j] = Convert.ToInt32(pamataj);        //priradim uzol na miesto v uzlovej postupnosti
                }
                else if (textik[i] == ',')
                {
                    pamataj = "";                           //vycistim pomocnu premennu, ktora si pamata hodnotu uzla a posuniem sa
                    j += 1;
                }
                else if (textik[i] == ' ')
                {

                }
                else // ak pouzivatel zada hocijaky iny znak do textboxu
                {
                    NodesAreCorrect = false;
                    MessageBox.Show("Postupnost zadaj len pomocou cisel a oddeluj ich 1 ciarkou");
                    break;
                }
            }

            //test, ci je nova postupnost vektorov neklesajuca
            for (int i = 1; i < Nodes.Count; i++)
            {
                if (Nodes[i] < Nodes[i - 1])
                {
                    NodesAreCorrect = false;
                    MessageBox.Show("Postupnosť hodnôt uzlov musí byť neklesajúca!");
                }
            }

            for (int i = 0; i < Nodes.Count - p - 1; i++)
            {
                if (Nodes[i] == Nodes[i + p + 1])
                {
                    NodesAreCorrect = false;
                    MessageBox.Show("Násobnosť uzlov môže byť maximálne p+1!");
                }
            }

            //test, ci nova postupnost neobsahuje menej uzlov nez treba, opat zavisi od toho, ci je na konci ciarka
            if (Nodes.Count > j + 1 || (Nodes.Count > j && textik[textik.Length - 1] == ','))
            {
                NodesAreCorrect = false;
                MessageBox.Show("Zadal si málo uzlov!");
            }
            if (NodesAreCorrect)
            {
                EditSliderT();
            }
        }
        public void AddNode()
        {
            int node;               // pridam uzol do listu a do textboxu
            node = Nodes[Nodes.Count - 1] + 1;
            Nodes.Add(node);
            if (NodesTextBox.Text[NodesTextBox.Text.Length - 1] == ' ')          //chcem, aby uzly boli oddelene len 1 ciarkou
            { NodesTextBox.Text += Convert.ToString(node); }
            else NodesTextBox.Text += ", " + Convert.ToString(node);

            PocetUzlov.Content = Nodes.Count;
            mPocetUzlov.Content = Nodes.Count;
        }
        public void RemoveNode()
        {
            NodesTextBox.Text = "";         // odstranim uzol z textboxu
            Nodes.Remove(Nodes[Nodes.Count - 1]);

            PocetUzlov.Content = Nodes.Count;
            mPocetUzlov.Content = Nodes.Count;

            for (int i = 0; i < Nodes.Count; i++)
            {
                NodesTextBox.Text += Convert.ToString(Nodes[i]) + ", ";
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ControlVertices.Clear();  // vymaze prvky listov a nastavi pociatocne hodnoty
            ControlPolygone.Clear();
            Nodes.Clear();
            g.Children.Clear();
            nPocetVrcholov.Content = 0;
            PocetVrcholov.Content = 0;
            CreateNodes();
            mPocetUzlov.Content = Nodes.Count;
            PocetUzlov.Content = Nodes.Count;
            EditSliderT();
        }

        private void DeBoor()   // vizualizuje sa bod na krivke
        {
            t = Convert.ToDouble(SliderT.Value);
            List<Point> PartofControlVertices = new List<Point>();  // na j-ty segment potrebujem riadice body j az j + p

            for (int j = 0; j < Spline.Count; j++)
            {
                if (Nodes[p + j] <= t && t <= Nodes[p + j + 1])
                {
                    for (int i = j; i < p + 1 + j; i++)             //j-ty segment je dany j  az j + p  bodmi
                    {
                        Point V = new Point();
                        V = ControlVertices[i].V;
                        PartofControlVertices.Add(V);
                    }
                    Point P = new Point();
                    for (int k = 0; k < PartofControlVertices.Count; k++)
                    {
                        P.X += PartofControlVertices[k].X * N(p, j + k, t, Nodes);
                        P.Y += PartofControlVertices[k].Y * N(p, j + k, t, Nodes);
                    }
                    DrawPoint(P.X, P.Y, Colors.DarkBlue);
                }
            }
        }
        private void IsVisualised_Checked(object sender, RoutedEventArgs e)
        {
            DeBoor();
        }
        public double N(int p, int i, double u, List<int> KnotsList) // vypocet N fcii
        {
            //p = horny index fcie N = stupen
            //i = dolny index fcie N = poradove cislo fcie (od 0 po n)
            //u = hodnota parametra
            //KnotsList = uzlova postupnost

            double value, alpha, beta; //koeficienty

            //najprv sa musim pozriet na korektnost koeficientov:
            if (KnotsList[i + p] - KnotsList[i] == 0) //ak je menovatel prveho koeficientu =0
            {
                alpha = 0;
            }
            else
            {
                alpha = (u - KnotsList[i]) / (KnotsList[i + p] - KnotsList[i]);
            }
            if (KnotsList[i + p + 1] - KnotsList[i + 1] == 0) //ak je menovatel druheho koeficinetu =0
            {
                beta = 0;
            }
            else
            {
                beta = (KnotsList[i + p + 1] - u) / (KnotsList[i + p + 1] - KnotsList[i + 1]);
            }

            //ak sme "najhlbsie" v rekurzii
            if (p == 0)
            {
                if (KnotsList[i] <= u && u < KnotsList[i + 1])   // ak parameter patri intervalu <U[i],U[i+1]), na ktorom "zije" fcia N0
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            value = alpha * N(p - 1, i, u, KnotsList) + beta * N(p - 1, i + 1, u, KnotsList);
            return value;
        }

        private void IsVisualised_Unchecked(object sender, RoutedEventArgs e)   // ak vypneme vizualizaciu, prekresli sa plocha
        {
            g.Children.Clear();
            DrawPoints(ControlVertices);

            if (ControlVertices.Count > 1)
            {
                DrawAllLines(ControlPolygone);
            }
            if (ControlVertices.Count > p)
            {
                RedrawSpline();
            }
        }
        public void DrawLine(Point A, Point B, Color Color, List<Line> Lines)    // metoda na vykreslovanie useciek
        {
            Line L = new Line();
            L.Stroke = new SolidColorBrush(Color);
            L.StrokeThickness = 1;

            L.X1 = A.X;
            L.Y1 = A.Y;
            L.X2 = B.X;
            L.Y2 = B.Y;

            Canvas.SetZIndex(L, 1);
            Lines.Add(L);
            g.Children.Add(L);
        }
        public void DrawAllLines(List<Line> Lines)      // vykreslenie vsetkych useciek
        {
            foreach (Line L in Lines)
            {
                g.Children.Add(L);
            }
        }
        public void DrawPoint(double X, double Y, Color Color)        // nakresli bod na danej pozicii X, Y
        {
            Ellipse e = new Ellipse();
            e.Fill = new SolidColorBrush(Color);
            e.Width = 6;
            e.Height = 6;
            Canvas.SetLeft(e, X - 3);
            Canvas.SetTop(e, Y - 3);
            Canvas.SetZIndex(e, 3);
            g.Children.Add(e);
        }
        public void DrawPoints(List<Vrchol> CV)                    // nakresli vsetky body v liste
        {
            for (int i = 0; i < CV.Count; i++)
            {
                DrawPoint(CV[i].V.X, CV[i].V.Y, Colors.DarkBlue);
            }
        }
        private void SliderT_PreviewMouseMove(object sender, MouseEventArgs e)  // zmena hodnoty na slider-i
        {
            ParameterT.Content = Convert.ToDecimal(SliderT.Value);
            g.Children.Clear();

            DrawPoints(ControlVertices);

            if (ControlVertices.Count > 1)
            {
                DrawAllLines(ControlPolygone);
            }
            if (ControlVertices.Count > p)
            {
                RedrawSpline();
            }

            if (IsVisualised.IsChecked == true)
            {
                DeBoor();
            }
        }

        private void DecreaseIter_Click(object sender, RoutedEventArgs e)   //zmena stupna krivky
        {
            if (p > 1)
            {
                p--;
                RemoveNode();
                CurveDegreeTextBox.Text = Convert.ToString(p);      // prepise sa zmenena hodnota LOD v textboxe
                pStupenKrivky.Content = p;
                ChangeNodes();
                EditSliderT();
                if (NodesAreCorrect)
                {
                    RedrawSpline();
                }

            }

            if (p == 1)
            {
                MessageBox.Show("Minimálny stupeň krivky je 1!");
            }
        }

        private void IncreaseIter_Click(object sender, RoutedEventArgs e)       // zmena stupna krivky
        {
            p++;
            pStupenKrivky.Content = p;
            CurveDegreeTextBox.Text = Convert.ToString(p);
            AddNode();
            ChangeNodes();
            EditSliderT();
            if (NodesAreCorrect)
            {
                RedrawSpline();
            }
        }

        private void LODDecreaseIter_Click(object sender, RoutedEventArgs e)    // zmena poctu LOD
        {
            if (LOD > 2)
            {
                LOD--;
                LODTextBox.Text = Convert.ToString(LOD);       // prepise sa zmenena hodnota LOD v textboxe
                RedrawSpline();
            }

            if (LOD == 2)
            {
                MessageBox.Show("Minimálny počet vzoriek na segment je 2!");
            }
        }

        private void LODIncreaseIter_Click(object sender, RoutedEventArgs e)    // zmena poctu LOD
        {
            LOD++;
            LODTextBox.Text = Convert.ToString(LOD);      // prepise sa zmenena hodnota LOD v textboxe
            RedrawSpline();
        }
    }

    public delegate void Menitko(Point KtoryBudemMenit);

    public class Segment
    {
        Canvas g;
        private int j;                              // j-ty segment - index segmentu 
        private List<Point> PartofControlVertices;  // na j-ty segment potrebujem riadice body j az j + p
        private Point[] SegmentPoints;              // body na segmente
        public List<Line> SegmentLines;             // usecky z ktorych sa sklada segment

        public Segment(int index, List<Vrchol> CVList, List<int> KnotsList, Canvas C)
        {
            g = C;
            j = index;
            PartofControlVertices = new List<Point>();
            SegmentPoints = new Point[MainWindow.LOD + 1];                     //aby sme mali usecku rozdelenu na LOD dielov, musime poznat jej LOD+1 bodov
            SegmentLines = new List<Line>();

            for (int j = index; j < MainWindow.p + 1 + index; j++)             //j-ty segment je dany j  az j + p  bodmi
            {
                Point V = new Point();
                V = CVList[j].V;
                PartofControlVertices.Add(V);
            }
            CountSegment(KnotsList);
        }
        public Point FindSegment(List<int> KnotsList)
        {
            Point P = new Point();
            if (KnotsList[MainWindow.p + j] <= MainWindow.t && MainWindow.t <= KnotsList[MainWindow.p + j + 1])
            {
                //P = new Point();
                P.X = 0;
                P.Y = 0;
                for (int k = 0; k < MainWindow.p + 1; k++)     // j-ty segment pouziva fcie Nj, ...Nj+p
                {
                    P.X += PartofControlVertices[k].X * N(MainWindow.p, j + k, MainWindow.t, KnotsList);
                    P.Y += PartofControlVertices[k].Y * N(MainWindow.p, j + k, MainWindow.t, KnotsList);
                }
                return P;
            }
            return P;
        }

        public void CountSegment(List<int> KnotsList)
        {
            double u, step;                         // parameter u pojde hodnotami KnotsList[p+j] do KnotsList[p+j+1] po dielikoch 1/LOD
            for (int i = 0; i < MainWindow.LOD + 1; i++)           // kolko bodov LOD potrebujem
            {
                step = i / (double)MainWindow.LOD; //krok
                u = (1 - step) * KnotsList[MainWindow.p + j] + step * KnotsList[MainWindow.p + j + 1];
                for (int k = 0; k < MainWindow.p + 1; k++)     // j-ty segment pouziva fcie Nj, ...Nj+p
                {
                    SegmentPoints[i].X += PartofControlVertices[k].X * N(MainWindow.p, j + k, u, KnotsList);
                    SegmentPoints[i].Y += PartofControlVertices[k].Y * N(MainWindow.p, j + k, u, KnotsList);
                }
            }

            for (int i = 0; i < MainWindow.LOD - 1; i++)       // vykresli usecky
            {
                Line l = new Line();
                l.Stroke = new SolidColorBrush(Colors.MediumVioletRed);
                l.StrokeThickness = 2;
                Canvas.SetZIndex(l, 2);
                SegmentLines.Add(l);
                SegmentLines[i].X1 = SegmentPoints[i].X;
                SegmentLines[i].Y1 = SegmentPoints[i].Y;
                SegmentLines[i].X2 = SegmentPoints[i + 1].X;
                SegmentLines[i].Y2 = SegmentPoints[i + 1].Y;
                g.Children.Add(l);
            }
        }


        public double N(int p, int i, double u, List<int> KnotsList) // vypocet N fcii
        {
            //p = horny index fcie N = stupen
            //i = dolny index fcie N = poradove cislo fcie (od 0 po n)
            //u = hodnota parametra
            //KnotsList = uzlova postupnost

            double value, alpha, beta; //koeficienty

            //najprv sa musim pozriet na korektnost koeficientov:
            if (KnotsList[i + p] - KnotsList[i] == 0) //ak je menovatel prveho koeficientu =0
            {
                alpha = 0;
            }
            else
            {
                alpha = (u - KnotsList[i]) / (KnotsList[i + p] - KnotsList[i]);
            }
            if (KnotsList[i + p + 1] - KnotsList[i + 1] == 0) //ak je menovatel druheho koeficinetu =0
            {
                beta = 0;
            }
            else
            {
                beta = (KnotsList[i + p + 1] - u) / (KnotsList[i + p + 1] - KnotsList[i + 1]);
            }

            //ak sme "najhlbsie" v rekurzii
            if (p == 0)
            {
                if (KnotsList[i] <= u && u < KnotsList[i + 1])   // ak parameter patri intervalu <U[i],U[i+1]), na ktorom "zije" fcia N0
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            value = alpha * N(p - 1, i, u, KnotsList) + beta * N(p - 1, i + 1, u, KnotsList);
            return value;
        }
    }
    public class Vrchol
    {
        private MainWindow okno;
        private Canvas g;
        private double polomer = 3;

        public Point V;
        private Ellipse eliV;

        public Vrchol(Point A, Canvas C, MainWindow w)
        {
            V = A;
            g = C;
            okno = w;

            eliV = KresliVrchol();      //nakreslim bod ako elipsu a zapamatam si ju
            eliV.MouseDown += eliV_MouseDown;
        }
        private Ellipse KresliVrchol()
        {
            Ellipse E = new Ellipse();
            E.Fill = new SolidColorBrush(Colors.DarkBlue);
            E.Width = 2 * polomer;
            E.Height = 2 * polomer;
            Canvas.SetLeft(E, V.X - polomer);
            Canvas.SetTop(E, V.Y - polomer);
            Canvas.SetZIndex(E, 2);

            g.Children.Add(E);

            return E;
        }

        public void eliV_MouseDown(object sender, MouseButtonEventArgs e) //po kliknuti na elipsu je Menic!=null a bude hybat touto elipsou
        {
            okno.Menic += ZmenPoziciuV;     //po kliknuti na elipsu nou hybem
        }
        public void ZmenPoziciuV(Point P)
        {
            V = P;                                  //zmeni suradnice vrchola
            Canvas.SetLeft(eliV, P.X - polomer);    //aktualizuje poziciu elipsy
            Canvas.SetTop(eliV, P.Y - polomer);
        }
    }
}
        