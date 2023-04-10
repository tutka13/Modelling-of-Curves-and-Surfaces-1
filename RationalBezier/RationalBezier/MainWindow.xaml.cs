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

namespace RationalBezier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<double> Weights;                   //vahy
        List<Point> ControlVertices;            //riadiace vrhcoly
        List<Line> ControlPolygone;             //riadiaca lomena ciara

        List<Point> CurvePoints;                //list pre body na krivke
        List<Line> CurveLines;                  //usecky, z ktorych pozostava krivka

        List<Line> CasteljauLines;              //usecky pri vizualizacii Casteljaovho algoritmu

        int clickCount;                         //rata pocet klikov do canvasu
        Point S;                                //stred elipsy
        bool WeightsAreCorrect;                 //pomocna premenna na kontrolu chyb v WeightsTextBoxe

        List<Point> Weights_CV;                 //k riadiacim vrcholom pridame vahy

        List<Point> Ellipse1;                   //pre vymodelovanie elipsy budeme potrebovat 4 listy
        List<Point> Ellipse2;
        List<Point> Ellipse3;
        List<Point> Ellipse4;

        Point A, B, C;                          //body trojuholnika

        public MainWindow()
        {                                        //vytvorenie listov a nastavenie pociatocnych hodnot
            InitializeComponent();
            Weights = new List<double>();
            ControlVertices = new List<Point>();
            ControlPolygone = new List<Line>();

            CurvePoints = new List<Point>();
            CurveLines = new List<Line>();

            CasteljauLines = new List<Line>();
            Weights_CV = new List<Point>();

            ParameterT.Content = 0.5;
            WeightsAreCorrect = true;
            clickCount = 0;

            Ellipse1 = new List<Point>();
            Ellipse2 = new List<Point>();
            Ellipse3 = new List<Point>();
            Ellipse4 = new List<Point>();

        }
        double DotProduct(Vector A, Vector B)   // skalarny sucin
        {
            double w = A.X * B.X + A.Y * B.Y;
            return w;
        }
        public void DrawPoint(double X, double Y, Color Color)        // nakresli bod na danej pozicii X, Y
        {
            Ellipse e = new Ellipse();
            e.Fill = new SolidColorBrush(Color);
            e.Width = 5;
            e.Height = 5;
            Canvas.SetLeft(e, X - 2.5);
            Canvas.SetTop(e, Y - 2.5);
            Canvas.SetZIndex(e, 7);
            g.Children.Add(e);
        }
        void DrawPoints(List<Point> CV)                    // nakresli vsetky body v liste
        {
            for (int i = 0; i < CV.Count; i++)
            {
                DrawPoint(CV[i].X, CV[i].Y, Colors.DarkBlue);
            }
        }
        void DrawLine(Point A, Point B, Color Color, double thickness, List<Line> Lines)      //vykresli usecku medzi 2 bodmi a prida ju do listu
        {
            Line L = new Line();
            L.Stroke = new SolidColorBrush(Color);
            L.StrokeThickness = thickness;

            L.X1 = A.X;
            L.Y1 = A.Y;
            L.X2 = B.X;
            L.Y2 = B.Y;

            Canvas.SetZIndex(L, 2);
            Lines.Add(L);
            g.Children.Add(L);
        }
        void DrawLines(List<Point> CV, Color Color, double thickness, List<Line> Lines)      // nakresli vsetky usecky na bodoch a prida ich do listu
        {
            for (int i = 0; i < CV.Count - 1; i++)
            {
                Line L = new Line();
                L.Stroke = new SolidColorBrush(Color);
                L.StrokeThickness = thickness;

                L.X1 = CV[i].X;
                L.Y1 = CV[i].Y;
                L.X2 = CV[i + 1].X;
                L.Y2 = CV[i + 1].Y;

                Canvas.SetZIndex(L, 2);
                Lines.Add(L);
                g.Children.Add(L);
            }
        }
        void DrawControlPolygone()  //vykresli riadiaci polygon
        {
            foreach (Line L in ControlPolygone)
            {
                g.Children.Add(L);
            }
        }
       
        void RedrawCanvas()     // metoda sa pouziva, ked sa vymaze a znova nakresli canvas
        {
            if (BezierCurve.IsChecked == true)
            {
                g.Children.Clear();                                         
                CurvePoints.Clear();
                CurveLines.Clear();

                DrawPoints(ControlVertices);

                DrawControlPolygone();

                DrawCurve(ControlVertices);

                if (IsVisualised.IsChecked == true)                         //ak mame zapnutu vizualizaciu, kreslime casteljaua
                {
                    DrawCasteljau(ControlVertices);
                }
            }
            if (Ellipse.IsChecked == true)
            {
                g.Children.Clear();                                        
                CurvePoints.Clear();
                CurveLines.Clear();

                DrawPoint(S.X, S.Y, Colors.DarkBlue);

                DrawPoints(Ellipse1);
                DrawPoints(Ellipse2);
                DrawPoints(Ellipse3);
                DrawPoints(Ellipse4);

                DrawControlPolygone();

                DrawCurve(Ellipse1);
                DrawCurve(Ellipse2);
                DrawCurve(Ellipse3);
                DrawCurve(Ellipse4);

                if (IsVisualised.IsChecked == true)
                {
                    EllipseCasteljau();
                }
            }
            if (Incircle.IsChecked == true)
            {
                g.Children.Clear(); 
                CurvePoints.Clear();
                CurveLines.Clear();

                DrawPoint(A.X, A.Y, Colors.DarkBlue);
                DrawPoint(B.X, B.Y, Colors.DarkBlue);
                DrawPoint(C.X, C.Y, Colors.DarkBlue);

                DrawControlPolygone();

                DrawIncircle(A, B, C);
            }
        }
       
        void RedrawCanvas2()    // pri zmene riadiaceho polygonu, metodach mousemove a mouse_rightbuttondown, sa pouzije tato metoda
        {
            if (BezierCurve.IsChecked == true)
            {
                g.Children.Clear();
                ControlPolygone.Clear();
                CurvePoints.Clear();
                CurveLines.Clear();
                CasteljauLines.Clear();

                DrawPoints(ControlVertices);
                DrawLines(ControlVertices, Colors.DarkBlue, 1, ControlPolygone);

                if (ControlVertices.Count > 2)
                {
                    DrawCurve(ControlVertices);
                    if (IsVisualised.IsChecked == true)
                    {
                        DrawCasteljau(ControlVertices);
                    }
                }
            }         
        }
        void DrawEllipse(double a1, double b1, double a2, double b2, double a3, double b3, List<Point> Ellipse) // metoda vykresli cast elipsy pomocou racionalnej bezierovej krivky
        {
            Point A = new Point();
            A.X = a1;
            A.Y = b1;
            Point AB = new Point();
            AB.X = a2;
            AB.Y = b2;
            Point B = new Point();
            B.X = a3;
            B.Y = b3;

            Ellipse.Add(A);
            Ellipse.Add(AB);
            Ellipse.Add(B);

            DrawPoints(Ellipse);
            DrawLine(AB, B, Colors.DarkBlue, 1, ControlPolygone);
            DrawLine(A, AB, Colors.DarkBlue, 1, ControlPolygone);
            DrawCurve(Ellipse);
        }

        void DrawIncircle(Point A, Point B, Point C)
        {
            Point Ta, Tb, Tc;
            double a, b, c, ra, rb, rc;

            a = Math.Sqrt(Math.Pow(B.X - C.X, 2) + Math.Pow(B.Y - C.Y, 2));
            b = Math.Sqrt(Math.Pow(A.X - C.X, 2) + Math.Pow(A.Y - C.Y, 2));
            c = Math.Sqrt(Math.Pow(A.X - B.X, 2) + Math.Pow(A.Y - B.Y, 2));
            clickCount = 0;

            ra = (b + c - a) / 2;
            rb = (a + c - b) / 2;
            rc = (a + b - c) / 2;

            Ta.X = (rb * C.X + rc * B.X) / (rb + rc);
            Ta.Y = (rb * C.Y + rc * B.Y) / (rb + rc);

            Tb.X = (ra * C.X + rc * A.X) / (ra + rc);
            Tb.Y = (ra * C.Y + rc * A.Y) / (ra + rc);

            Tc.X = (rb * A.X + ra * B.X) / (ra + rb);
            Tc.Y = (rb * A.Y + ra * B.Y) / (ra + rb);

            DrawPoint(Ta.X, Ta.Y, Colors.DarkBlue);
            DrawPoint(Tb.X, Tb.Y, Colors.DarkBlue);
            DrawPoint(Tc.X, Tc.Y, Colors.DarkBlue);

            List<Point> all_vertices = new List<Point>();
            all_vertices.Add(A);
            all_vertices.Add(Tc);
            all_vertices.Add(B);
            all_vertices.Add(Ta);
            all_vertices.Add(C);
            all_vertices.Add(Tb);

            for (int i = 0; i < 3; i++)
            {
                double angle;
                int prev;
                if (i == 0)
                {
                    prev = all_vertices.Count - 1;
                }
                else
                {
                    prev = 2 * i - 1;
                }

                Vector prev_side = all_vertices[prev] - all_vertices[2 * i];
                Vector next_side = all_vertices[2 * i] - all_vertices[2 * i + 1];
                angle = Math.PI - Math.Acos(DotProduct(prev_side, next_side) / (prev_side.Length * next_side.Length));

                //vaha pri vrchole trojuholnika = sin(angle/2)
                List<Point> curve_vertices = new List<Point>() { all_vertices[prev], all_vertices[2 * i], all_vertices[2 * i + 1] };
                List<double> curve_weights = new List<double>() { 1, Math.Sin(angle / 2), 1 };

                DrawIncircle(curve_vertices, curve_weights);
            }
        }

        void g_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            clickCount++;
            if (BezierCurve.IsChecked == true)                                  // v rezime bezierovej krivky
            {
                ControlVertices.Add(e.GetPosition(g));
                DrawPoints(ControlVertices);

                CreateWeights();
                if (ControlVertices.Count > 1)                                  //ak mame aspon dva body, kresli sa riadiaca lomena ciara 
                {
                    DrawLine(ControlVertices[ControlVertices.Count - 2], ControlVertices[ControlVertices.Count - 1], Colors.DarkBlue, 1, ControlPolygone);
                }
                   
                if (ControlVertices.Count > 2)                                  //ak mame aspon tri body, kresli sa krivka, pri kazdom vlozeni bodu sa prekresluje plocha
                {
                    RedrawCanvas();
                }
            }
            
            if (Ellipse.IsChecked == true && clickCount < 3)                    // v rezime elipsy
            {   
                WeightsTextBox.Text = "Váhy nemožno meniť.";
                double a, b;                                                    //velkosti poloosi

                if (clickCount == 1)
                {
                    g.Children.Clear();
                    ControlVertices.Clear();
                    ControlPolygone.Clear();
                    Weights.Clear();
                    Ellipse1.Clear();
                    Ellipse2.Clear();
                    Ellipse3.Clear();
                    Ellipse4.Clear();

                    CurvePoints.Clear();
                    CurveLines.Clear();

                    S = e.GetPosition(g);                                       //tento bod je stred
                    DrawPoint(S.X, S.Y, Colors.DarkBlue);
                }

                if (clickCount == 2)
                {
                    Point P = e.GetPosition(g);                                                 // tento bod udava polomer                  
                    DrawPoint(P.X, P.Y, Colors.DarkBlue);
                    DrawLine(P, S, Colors.DarkBlue, 1, ControlPolygone);                        // vykresli polomer

                    a = Math.Abs(P.X - S.X);
                    b = Math.Abs(P.Y - S.Y);

                    DrawEllipse(S.X + a, S.Y, S.X + a, S.Y - b, S.X, S.Y -b, Ellipse1);         // vykresli segment elipsy
                    DrawEllipse(S.X, S.Y - b, S.X - a, S.Y - b, S.X - a, S.Y, Ellipse2);
                    DrawEllipse(S.X - a, S.Y, S.X - a, S.Y + b, S.X, S.Y + b, Ellipse3);
                    DrawEllipse(S.X, S.Y + b, S.X + a, S.Y + b, S.X + a, S.Y, Ellipse4);

                    if (IsVisualised.IsChecked == true)
                    {
                        EllipseCasteljau();
                    }
                    clickCount = 0;
                }
            }

            if (Incircle.IsChecked == true && clickCount < 4)                                 // v rezime vpisanej kruznice               
            {
                WeightsTextBox.Text = "Váhy nemožno meniť.";

                if (clickCount == 1)
                {
                    g.Children.Clear();
                    Ellipse1.Clear();
                    Ellipse2.Clear();
                    Ellipse3.Clear();
                    Weights.Clear();
                    CurvePoints.Clear();
                    CurveLines.Clear();
                    ControlVertices.Clear();
                    ControlPolygone.Clear();
                }

                ControlVertices.Add(e.GetPosition(g));                              // pri kazdom kliku < 4 sa vykresli bod
                DrawPoints(ControlVertices);

                if (ControlVertices.Count == 2)                                  // ak mame dva body, kresli sa riadiaca lomena ciara 
                {
                    DrawLine(ControlVertices[ControlVertices.Count - 2], ControlVertices[ControlVertices.Count - 1], Colors.DarkBlue, 1, ControlPolygone);                    
                }
                if (ControlVertices.Count == 3)                                 // ak mame tri body, kresli sa riadiaca lomena ciara a krivka
                {   
                    A = ControlVertices[ControlVertices.Count - 3];
                    B = ControlVertices[ControlVertices.Count - 2];
                    C = ControlVertices[ControlVertices.Count - 1];
                    DrawLine(B, C, Colors.DarkBlue, 1, ControlPolygone);
                    DrawLine(A, C, Colors.DarkBlue, 1, ControlPolygone);

                    DrawIncircle(A, B, C);                   
                }
            }
        }
        public void DrawIncircle(List<Point> CP, List<double> w)
        {
            List<Point> vynorene_points = new List<Point>();            //zoznam vynorenych riadiacich vrcholov

            //vynorenie bodov CP
            for (int i = 0; i < CP.Count; i++)
            {
                vynorene_points.Add(new Point(CP[i].X * w[i], CP[i].Y * w[i]));
            }

            //pocitanie bodu krivky pre t parametre
            for (int k = 0; k <= 100; k++)
            {
                double t = k / 100.0;

                //vrcholy
                List<Point> old_V = new List<Point>();
                old_V.AddRange(vynorene_points);
                List<Point> new_V = new List<Point>();

                Vector tan = new Vector(); //dotykovy vektor

                //vahy
                List<double> old_W = new List<double>();
                old_W.AddRange(w);
                List<double> new_W = new List<double>();

                //algoritmus casteljau
                for (int i = 0; i < CP.Count - 1; i++)
                {
                    for (int j = 0; j < CP.Count - i - 1; j++)
                    {
                        Point P = new Point();
                        P.X = (1 - t) * old_V[j].X + t * old_V[j + 1].X;
                        P.Y = (1 - t) * old_V[j].Y + t * old_V[j + 1].Y;
                        new_V.Add(P);
                        new_W.Add((1 - t) * old_W[j] + t * old_W[j + 1]);
                    }
                    old_V.Clear();
                    old_V.AddRange(new_V);

                    old_W.Clear();
                    old_W.AddRange(new_W);
                    if (IsVisualised.IsChecked == true && t == Math.Round(SliderT.Value, 2))
                    {
                        //ak je zaskrtnuta moznost vykreslenia algoritmu casteljau, vykresli sa aktualny level
                        for (int j = 0; j < old_V.Count - 1; j++)
                        {
                            Point S = old_V[j];
                            Point E = old_V[j + 1];

                            S.X = S.X / old_W[j];
                            S.Y = S.Y / old_W[j];

                            E.X = E.X / old_W[j + 1];
                            E.Y = E.Y / old_W[j + 1];
                            DrawLine(S, E, Colors.Gray, 1, CasteljauLines);
                        }
                        if (old_V.Count == 2)
                        {
                            //ak sme v predposlednom leveli najdeme dotycnicu
                            tan.X = old_V[1].X / old_W[1] - old_V[0].X / old_W[0];
                            tan.Y = old_V[1].Y / old_W[1] - old_V[0].Y / old_W[0];
                        }
                    }

                    new_V.Clear();
                    new_W.Clear();
                }

                //vynorenie vysledneho bodu z casteljau algoritmu a ulozenie do zoznamu CurvePoints
                Point result;
                if (old_W[0] != 0)
                {
                    result = new Point(old_V[0].X / old_W[0], old_V[0].Y / old_W[0]);
                }
                else
                {
                    result = old_V[0];
                }
                CurvePoints.Add(result);

                //ak je urceny dotykovy vektor - najdenie normaloveho a vykreslenie vektorov + daneho bodu 
                if (tan != new Vector())
                {
                    Vector nor = new Vector();

                    nor.X = -tan.Y;
                    nor.Y = tan.X;

                    tan.Normalize();
                    tan = tan * 70;

                    nor.Normalize();
                    nor = nor * 70;

                    DrawLine(result, result + tan, Colors.Red, 1.5, CasteljauLines);      // vykreslenie dotycnice
                    DrawLine(result, result + nor, Colors.Black, 1.5, CasteljauLines);    // vykreslenie normaly
                    DrawPoint(result.X, result.Y, Colors.Red);                            // vykresli vysledny bod na krivke

                    tan = new Vector();
                }
            }
            //vykreslenie krivky
            DrawLines(CurvePoints, Colors.MediumVioletRed, 2, CurveLines);
        }
        public void g_MouseMove(object sender, MouseEventArgs e)    // iba v rezime bezierkovej krivky ma zmysel
        {
            if (BezierCurve.IsChecked == true)
            {
                if (e.LeftButton == MouseButtonState.Pressed)       // ak je stlacene lave tlacidlo myse a nachadzam sa v blizkosti bodu, zmenim poziciu bodu a canvas sa prekresli
                {
                    for (int i = 0; i < ControlVertices.Count; i++)
                    {
                        if (Math.Abs(ControlVertices[i].X - e.GetPosition(g).X) < 5 && Math.Abs(ControlVertices[i].Y - e.GetPosition(g).Y) < 5)
                        {
                            ControlVertices[i] = e.GetPosition(g);
                        }
                    }
                }
                RedrawCanvas2();
            }
        }
        private void g_MouseRightButtonDown(object sender, MouseButtonEventArgs e)   // iba v rezime bezierkovej krivky ma zmysel
        {
            if (BezierCurve.IsChecked == true)
            {
                for (int i = 0; i < ControlVertices.Count; i++)
                {
                    if (Math.Abs(ControlVertices[i].X - e.GetPosition(g).X) <= 10 && Math.Abs(ControlVertices[i].Y - e.GetPosition(g).Y) <= 10) //ak kliknem blizko bodu, odstranim ho z listu
                    {
                        ControlVertices.Remove(ControlVertices[i]);
                    }
                }
                RedrawCanvas2();
            }
        }
        private void CreateWeights()      // vytvorenie vah
        {
            if (BezierCurve.IsChecked == true)
            {
                Weights.Add(1);                                 // pridanie jednotkovych vah
                string textik = WeightsTextBox.Text; ;          // v stringu si ulozim aktualny obsah textboxu

                if (textik.Length > 1)                          // podmienky na pridavanie medzier a vah do textboxu
                {
                    if (textik[textik.Length - 1] == ' ')
                    {
                        WeightsTextBox.Text += Convert.ToString(1) + " ";
                    }
                    else
                    {
                        WeightsTextBox.Text += " " + Convert.ToString(1) + " ";
                    }
                }
                else
                {
                    WeightsTextBox.Text += Convert.ToString(1) + " ";
                }
            }
        }
        void AddWeights(List<Point> CV)
        {
            Weights_CV.Clear();
            CurvePoints.Clear();
            CurveLines.Clear();

            if (BezierCurve.IsChecked == true)
            {
                for (int i = 0; i < CV.Count; i++)                      // body najprv prenasobime prislusnymi vahami
                {
                    Point value;
                    value.X = CV[i].X * Weights[i];
                    value.Y = CV[i].Y * Weights[i];
                    Weights_CV.Add(value);
                }
            }
            if (Ellipse.IsChecked == true)
            {
                Weights.Clear();
                Weights.Add(1);
                Weights.Add(1);
                Weights.Add(2);

                Weights_CV.Add(CV[0]);
                Weights_CV.Add(CV[1]);

                Point value;
                value.X = CV[2].X * 2;
                value.Y = CV[2].Y * 2;
                Weights_CV.Add(value);
            }
            if (Incircle.IsChecked == true)     // vahy sa zrataju uz v metode DrawIncircle
            {
                DrawIncircle(A, B, C);
            }
        }
        void DrawCurve(List<Point> CV)                             // metoda na kreslenie krivky, k-krat spustime Casteljauov algoritmus, k = 100
        {
            AddWeights(CV);
            for (double t = 0; t <= 1; t = t + 0.01)              // pre kazde k sa vytvori niekolko listov, pomocou kt ziskame bod na krivke
            {
                List<Point> old_CV = new List<Point>();
                old_CV.AddRange(Weights_CV);
                List<Point> new_CV = new List<Point>();

                List<double> old_W = new List<double>();
                old_W.AddRange(Weights);
                List<double> new_W = new List<double>();

                for (int i = 0; i < CV.Count - 1; i++)              // znaci uroven
                {
                    for (int j = 0; j < CV.Count - i - 1; j++)      // znaci pocet bodov v urovni
                    {
                        double w;
                        Point P = new Point();
                        P.X = (1 - t) * old_CV[j].X + t * old_CV[j + 1].X;
                        P.Y = (1 - t) * old_CV[j].Y + t * old_CV[j + 1].Y;
                        w = (1 - t) * old_W[j] + t * old_W[j + 1];
                        new_CV.Add(P);
                        new_W.Add(w);
                    }
                    old_CV.Clear();
                    old_CV.AddRange(new_CV);
                    new_CV.Clear();

                    old_W.Clear();
                    old_W.AddRange(new_W);
                    new_W.Clear();
                }
                Point result;
                if (old_W[0] != 0)
                {
                    result = new Point(old_CV[0].X / old_W[0], old_CV[0].Y / old_W[0]);
                }
                else
                {
                    result = old_CV[0];
                } 
                CurvePoints.Add(result);
            }
            DrawLines(CurvePoints, Colors.MediumVioletRed, 2, CurveLines);  // nakoniec nakresli lomene ciary na krivke
        }
        public void DrawCasteljau(List<Point> CV)               // racionalny Casteljauov algoroitmus
        {
            double t;
            t = Convert.ToDouble(SliderT.Value);                // precita si hodnotu t zo slider-a
            AddWeights(CV);

            List<Point> old_CV = new List<Point>();
            old_CV.AddRange(Weights_CV);
            List<Point> new_CV = new List<Point>();
            Point tangent_end = new Point();

            List<double> old_W = new List<double>();
            old_W.AddRange(Weights);
            List<double> new_W = new List<double>();

            List<Point> Weights_old_CV = new List<Point>();
            Weights_old_CV.AddRange(Weights_CV);
            List<Point> Weights_new_CV = new List<Point>();

            for (int i = 0; i < CV.Count - 1; i++)                  // prepocita casteljau body
            {
                for (int j = 0; j < CV.Count - i - 1; j++)
                {
                    Point P = new Point();
                    P.X = (1 - t) * old_CV[j].X + t * old_CV[j + 1].X;
                    P.Y = (1 - t) * old_CV[j].Y + t * old_CV[j + 1].Y;
                    new_CV.Add(P);

                    double w;
                    w = (1 - t) * old_W[j] + t * old_W[j + 1];
                    new_W.Add(w);

                    Point wP = new Point();
                    wP.X = P.X / w;
                    wP.Y = P.Y / w;
                    Weights_new_CV.Add(wP);
                }
                DrawLines(Weights_new_CV, Colors.DarkGray, 1, CasteljauLines);  // nakresli casteljau ciary

                if (new_CV.Count == 2)                              // vypocita dotykovy vektor
                {
                    tangent_end.X = new_CV[1].X / new_W[1];
                    tangent_end.Y = new_CV[1].Y / new_W[1];
                }

                old_CV.Clear();
                old_CV.AddRange(new_CV);
                new_CV.Clear();

                old_W.Clear();
                old_W.AddRange(new_W);
                new_W.Clear();

                Weights_old_CV.Clear();
                Weights_old_CV.AddRange(Weights_new_CV);
                Weights_new_CV.Clear();
            }

            Point result;                                           // vypocita hladany bod, vynorenim
            if (old_W[0] != 0)
            {
                result = new Point(old_CV[0].X / old_W[0], old_CV[0].Y / old_W[0]);
            }
            else
            {
                result = old_CV[0];
            }     
          
            Vector tan = new Vector();
            Vector nor = new Vector();

            if (t == 1)
            {
                tan.X = result.X - Weights_CV[CV.Count - 2].X;
                tan.Y = result.Y - Weights_CV[CV.Count - 2].Y;
            }
            else
            {
                tan.X = tangent_end.X - result.X;
                tan.Y = tangent_end.Y - result.Y;
            }

            nor.X = -tan.Y;
            nor.Y = tan.X;

            tan.Normalize();
            tan = tan * 70;

            nor.Normalize();
            nor = nor * 70;

            DrawLine(result, result + tan, Colors.Red, 1.5, CasteljauLines);      // vykreslenie dotycnice
            DrawLine(result, result + nor, Colors.Black, 1.5, CasteljauLines);    // vykreslenie normaly
            DrawPoint(result.X, result.Y, Colors.Red);                            // vykresli vysledny bod na krivke
        }
        void EllipseCasteljau()         // nakresli Casteljau algoritmus pre vsetky 4 segmenty
        {
            DrawCasteljau(Ellipse1);
            DrawCasteljau(Ellipse2);
            DrawCasteljau(Ellipse3);
            DrawCasteljau(Ellipse4);
        }

        private void IsVisualised_Checked(object sender, RoutedEventArgs e)     // vykresli Castejlauov algoritmus, kazdy rezim so svojou miernou upravou
        {
            if (BezierCurve.IsChecked == true)
            {
                DrawCasteljau(ControlVertices);
            }
            if (Ellipse.IsChecked == true)
            {
                EllipseCasteljau();
            }
            if (Incircle.IsChecked == true)
            {
                g.Children.Clear();
                DrawPoint(A.X, A.Y, Colors.DarkBlue);
                DrawPoint(B.X, B.Y, Colors.DarkBlue);
                DrawPoint(C.X, C.Y, Colors.DarkBlue);
                DrawControlPolygone();
                DrawIncircle(A, B, C);
            }  
        }

        private void IsVisualised_Unchecked(object sender, RoutedEventArgs e)   // prekresli plochu
        {
            RedrawCanvas();
        }

        private void SliderT_PreviewMouseMove(object sender, MouseEventArgs e)      // pri zmene parametra t sa prekresli vizualizacia Casteljauovho algoritmu a teda aj cela plocha
        {
            ParameterT.Content = Math.Round(SliderT.Value, 2);
            if (IsVisualised.IsChecked == true)
            {
                RedrawCanvas();
            }
        }

        private void BezierCurve_Checked(object sender, RoutedEventArgs e)     // pri zmene rezimu sa zmaze plocha a vymazu sa listy
        {
            if (ControlVertices != null)
            {
                Reset();
            }
        }
        private void Ellipse_Checked(object sender, RoutedEventArgs e)        // pri zmene rezimu sa zmaze plocha a vymazu sa listy
        {
            if (ControlVertices != null)
            {
                Reset();
            }
        }

        private void Incircle_Checked(object sender, RoutedEventArgs e)       // pri zmene rezimu sa zmaze plocha a vymazu sa listy
        {
            if (ControlVertices != null)
            {
                Reset();
            }
        }

        void Reset()        // metoda reset, nastavi vsetko na pociatocne hodnoty
        {
            g.Children.Clear();
            ListClear();
            WeightsTextBox.Text = "";
           
            ParameterT.Content = 0.5;
            S = new Point();
            A = new Point();
            B = new Point();
            C = new Point();
            clickCount = 0;
        }

        void ListClear()        // vymaze vsetky listy
        {
            ControlVertices.Clear();
            ControlPolygone.Clear();

            CurvePoints.Clear();
            CurveLines.Clear();

            CasteljauLines.Clear();

            Weights.Clear();
            Weights_CV.Clear();

            Ellipse1.Clear();
            Ellipse2.Clear();
            Ellipse3.Clear();
            Ellipse4.Clear();
        }
        private void Reset_Click(object sender, RoutedEventArgs e)      // reset button
        {
            if (ControlVertices != null)
            {
                Reset();
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)       //aktualizacia vah - podobne ako v minulej ulohe, kontroluje chyby v textboxe
        {
            string textik = WeightsTextBox.Text; ;              //v stringu si ulozim aktualny obsah textboxu
            int j = 0;                                          //ktoru poziciu v zozname prepisujem
            WeightsAreCorrect = true;

            string pamataj = "";
            for (int i = 0; i < textik.Length; i++)             //v tomto cykle vyberiem z textoveho okna len cele cisla
            {
                if (Char.IsDigit(textik[i]))
                {
                    if (j + 1 > Weights.Count)                  //ak je v textboxe viac cisel nez treba
                    {
                        WeightsAreCorrect = false;
                        MessageBox.Show("Zadal si priveľa váh!");
                        break;
                    }
                    pamataj += Convert.ToString(textik[i]);      //zretazim cisla ako text
                    Weights[j] = Convert.ToInt32(pamataj);       //priradim vahu na miesto v liste
                }
                else if (textik[i] == ' ')
                {
                    pamataj = "";                               //vycistim pomocnu premennu, ktora si pamata hodnotu vahy a posuniem sa
                    j += 1;
                }
                else                                            //ak pouzivatel zada hocijaky iny znak do textboxu
                {
                    WeightsAreCorrect = false;
                    MessageBox.Show("Váhy zadaj len pomocou celých kladných čísel a oddeľ ich medzerou!");
                    break;
                }
            }

            if (Weights.Count > j + 1 || (Weights.Count > j && textik[textik.Length - 1] == ' '))   //podmienka pre nespravny pocet vah
            {
                WeightsAreCorrect = false;
                MessageBox.Show("Zadal si málo váh!");
            }

            if (WeightsAreCorrect)                              //ak su vahy zadane spravne, prekresli sa canvas
            {
                RedrawCanvas();
            }
        }
    }
}
