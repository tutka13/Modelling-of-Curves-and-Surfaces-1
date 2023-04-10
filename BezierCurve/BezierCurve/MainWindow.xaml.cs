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

namespace BezierCurve
{
    public partial class MainWindow : Window
    {
        List<Point> ControlVertices;
        List<Line> ControlPolygone;

        List<Point> BezierVertices;
        List<Line> BezierLines;

        List<Line> CasteljauLines;

        List<Line> NodesLines;
        List<Line> NodesPolygone;
        double cw, ch;
        public MainWindow()
        {
            InitializeComponent();
            ControlVertices = new List<Point>();
            ControlPolygone = new List<Line>();

            BezierLines = new List<Line>();
            BezierVertices = new List<Point>();

            CasteljauLines = new List<Line>();

            NodesLines = new List<Line>();
            NodesPolygone = new List<Line>();

            cw = g.Width;
            ch = g.Height;
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
        public void g_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (BezierCurve.IsChecked == true)                                  // pridavat body viem iba pri bezierovej krivke
            {
                g.Children.Clear();
                ControlVertices.Add(e.GetPosition(g));
                DrawPoints(ControlVertices);
                DrawLines(ControlVertices, g, Colors.Blue, ControlPolygone);

                if (ControlVertices.Count > 2)                                  // ak mame aspon dva body, kresli sa krivka
                {
                    DrawCurve(ControlVertices, g);
                    if (IsVisualised.IsChecked == true)                         // ak mame zapnutu vizualizaciu, kreslime casteljaua
                    {
                        DrawCasteljau(ControlVertices, g);
                    }
                }    
            }
        }
        public void g_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (BezierCurve.IsChecked == true)                                  // ostranovat body viem iba pri bezierovej krivke
            {
                for (int i = 0; i < ControlVertices.Count; i++)
                {
                    if (Math.Abs(ControlVertices[i].X - e.GetPosition(g).X) < 15 && Math.Abs(ControlVertices[i].Y - e.GetPosition(g).Y) < 15)       // ak kliknem niekde v blizkosti riadiaceho vrchola
                    {
                        ControlVertices.Remove(ControlVertices[i]);
                        g.Children.Clear();

                        DrawPoints(ControlVertices);
                        DrawLines(ControlVertices, g, Colors.Blue, ControlPolygone);

                        if (ControlVertices.Count > 2)
                        {
                            DrawCurve(ControlVertices, g);

                            if (IsVisualised.IsChecked == true)
                            {
                                DrawCasteljau(ControlVertices, g);
                            }
                        }
                    }
                }
            }
        }
        public void g_MouseMove(object sender, MouseEventArgs e)
        {
            if (BezierCurve.IsChecked == true)
            {
                g.Children.Clear();
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
                DrawPoints(ControlVertices);
                DrawLines(ControlVertices, g, Colors.Blue, ControlPolygone);

                if (ControlVertices.Count > 2)
                {
                    DrawCurve(ControlVertices, g);
                    if (IsVisualised.IsChecked == true)
                    {
                        DrawCasteljau(ControlVertices, g);
                    }
                }
            }
        }
        public void DrawLines(List<Point> CV, Canvas C, Color Color, List<Line> Lines)      // nakresli vsetky usecky na bodoch
        {
            for (int i = 0; i < CV.Count - 1; i++)
            {
                Line L = new Line();
                L.Stroke = new SolidColorBrush(Color);
                L.StrokeThickness = 1;

                L.X1 = CV[i].X;
                L.Y1 = CV[i].Y;
                L.X2 = CV[i + 1].X;
                L.Y2 = CV[i + 1].Y;

                Canvas.SetZIndex(L, 2);
                Lines.Add(L);
                C.Children.Add(L);
            }
        }
        public void DrawPoint(double X, double Y, Canvas C, Color Color)        // nakresli bod na danej pozicii X, Y
        {
            Ellipse e = new Ellipse();
            e.Fill = new SolidColorBrush(Color);
            e.Width = 6;
            e.Height = 6;
            Canvas.SetLeft(e, X - 3);
            Canvas.SetTop(e, Y - 3);
            Canvas.SetZIndex(e, 5);
            C.Children.Add(e);
        }
        public void DrawPoints(List<Point> CV)                    // nakresli vsetky body v liste
        {
            for (int i = 0; i < CV.Count; i++)
            {
                DrawPoint(CV[i].X, CV[i].Y, g, Colors.DarkBlue);
            }
        }            
        public List<Point> CalculatePoints(List<Point> og_CV, List<Point> CV, double start_int, double end_int)
        {
            double epsilon;                                                         // vypocita list bodov pre adaptivne zjemnovanie 
            epsilon = Convert.ToDouble(SliderEpsilon.Value);
            List<Point> overall_CP = new List<Point>();
            List<Point> CP = new List<Point>();

            if (MaxDistance(CV) > epsilon)                                          // ak je maximalna vzdialenost vacsia ako epsilon, zjemnime krivku
            {
                List<List<double>> variables = new List<List<double>>();
                for (int k = 0; k < 2; k++)
                {
                    for (int i = 0; i < CV.Count; i++)
                    {
                        List<double> inside_list = new List<double>();
                        for (int j = 0; j < CV.Count - 1; j++)
                        {
                            if (i <= j)
                            {
                                inside_list.Add(start_int + k * (end_int - start_int) * 0.5);
                            }
                            else
                            {
                                inside_list.Add(end_int - (1 - k) * (end_int - start_int) * 0.5);
                            }
                        }
                        variables.Add(inside_list);
                    }
                }

                                                                                        //stredny vrchol je dvakrat pre lavy aj pravy interval
                variables.Remove(variables[variables.Count / 2]);

                for (int i = 0; i < variables.Count; i++)               // vypocitame vrcholy
                {
                    List<Point> new_CV = new List<Point>();
                    List<Point> old_CV = new List<Point>(new List<Point>(og_CV));
                    Point P = new Point();

                    List<double> fi = variables[i];
                    for (int j = 0; j < CV.Count - 1; j++)
                    {
                        for (int k = 0; k < CV.Count - j - 1; k++)
                        {
                            P.X = (1 - fi[j]) * old_CV[k].X + fi[j] * old_CV[k + 1].X;
                            P.Y = (1 - fi[j]) * old_CV[k].Y + fi[j] * old_CV[k + 1].Y;
                            new_CV.Add(P);
                        }

                        old_CV.Clear();
                        old_CV.AddRange(new_CV);
                        new_CV.Clear();
                    }
                    CP.Add(old_CV[0]);
                }

                List<Point> left_CP = CP.GetRange(0, (CP.Count + 1) / 2);       // rozdelime vrcholy do spravnych intervalov
                List<Point> right_CP = new List<Point>();

                right_CP.Add(CP[(CP.Count - 1) / 2]);
                right_CP.AddRange(CP.GetRange((CP.Count + 1) / 2, (CP.Count - 1) / 2));

                overall_CP.AddRange(CalculatePoints(og_CV, left_CP, start_int, start_int + (end_int - start_int) / 2));     // rekurzivne zavolame
                overall_CP.AddRange(CalculatePoints(og_CV, right_CP, start_int + (end_int - start_int) / 2, end_int));

                return overall_CP;
            }
            return CV;
        }
        public void DrawCurve(List<Point> CV, Canvas c)                 // vykresli krivku
        {
            if (BezierCurve.IsChecked == true)                          // ak bezierovu
            {
                BezierVertices = CalculatePoints(CV, CV, 0, 1);         // vypocita body na bezierovej krivke
                DrawLines(BezierVertices, g, Colors.Red, BezierLines);  // nakresli krivku

                if (IsVisualised.IsChecked == true)
                {
                    DrawCasteljau(ControlVertices, g);
                }
            }
            if (NodesCurve.IsChecked == true)                           // ak uzlovu
            {
                double a, b, s;
                a = Convert.ToDouble(SliderA.Value);
                b = Convert.ToDouble(SliderB.Value);
                s = Convert.ToDouble(SliderS.Value);
                List<double> var_V0 = new List<double>() { -s / 2, -s / 2, -s / 2 };
                List<double> var_V1 = new List<double>() { -s / 2, -s / 2, s / 2 };
                List<double> var_V2 = new List<double>() { -s / 2, s / 2, s / 2 };
                List<double> var_V3 = new List<double>() { s / 2, s / 2, s / 2 };

                List<List<double>> variables = new List<List<double>>() { var_V0, var_V1, var_V2, var_V3 };

                CV.Clear();
                for (int i = 0; i < 4; i++)                             // vypocita riadiace vrcholy krivky podla vstupnych parametrov pomocou polarnej formy
                {
                    List<double> t = variables[i];
                    Point P = new Point();
                    P.X = a - (b / 3) * (t[0] * t[1] + t[1] * t[2] + t[0] * t[2])  + (cw / 2);
                    P.Y = (t[0] * t[1] * t[2]) * (P.X - (cw / 2)) + (ch / 2);
                    CV.Add(P);
                }

                DrawPoints(CV);                                     // nakresli ich
                DrawLines(CV, g, Colors.Blue, NodesPolygone);       // nakresli riadiaci polygon

                if (CV.Count > 3)
                {
                    List<Point> new_CV = new List<Point>();

                    new_CV = CalculatePoints(CV, CV, 0, 1);         // vypocita body na uzlovej krivke
                    DrawLines(new_CV, g, Colors.Red, NodesLines);   // nakresli krivku

                    if (IsVisualised.IsChecked == true)
                    {
                        DrawCasteljau(ControlVertices, g);
                    }
                } 
            }
        }

        public void DrawCasteljau(List<Point> CV, Canvas C)
        {
            double t;
            t = Convert.ToDouble(SliderT.Value);                // precita si hodnotu t zo slider-a
            List<Point> old_CV = new List<Point>();
            old_CV.AddRange(CV);
            List<Point> new_CV = new List<Point>();
            Point tangent_end = new Point();

            for (int i = 0; i < CV.Count - 1; i++)              // prepocita casteljau body
            {
                for (int j = 0; j < CV.Count - i - 1; j++)
                {
                    Point P = new Point();
                    P.X = (1 - t) * old_CV[j].X + t * old_CV[j + 1].X;
                    P.Y = (1 - t) * old_CV[j].Y + t * old_CV[j + 1].Y;
                    new_CV.Add(P);
                }
                DrawLines(new_CV, g, Colors.DarkGray, CasteljauLines);  // nakresli casteljau ciary
                    
                if (new_CV.Count == 2)              // vypocita dotykovy vektor
                {
                   tangent_end = new_CV[1];
                }

                old_CV.Clear();
                old_CV.AddRange(new_CV);
                new_CV.Clear();
            }

            Vector tan = new Vector();
            Vector nor = new Vector();

            if (t == 1)
            {
               tan.X = old_CV[0].X - CV[CV.Count - 2].X;
               tan.Y = old_CV[0].Y - CV[CV.Count - 2].Y;
            }
            else
            {
                tan.X = tangent_end.X - old_CV[0].X;
                tan.Y = tangent_end.Y - old_CV[0].Y;
            }
                
            nor.X = -tan.Y;
            nor.Y = tan.X;

            tan.Normalize();
            tan = tan * 70;

            nor.Normalize();
            nor = nor * 70;

            Line l1 = new Line();           // vykreslenie dotycnice
            l1.Stroke = new SolidColorBrush(Colors.Black);
            l1.StrokeThickness = 1;
            l1.X1 = old_CV[0].X;
            l1.Y1 = old_CV[0].Y;
            l1.X2 = old_CV[0].X + tan.X;
            l1.Y2 = old_CV[0].Y + tan.Y;

            Canvas.SetZIndex(l1, 4);
            C.Children.Add(l1);

            Line l2 = new Line();           // vykreslenie normaly
            l2.Stroke = new SolidColorBrush(Colors.Gray);
            l2.StrokeThickness = 1;
            l2.X1 = old_CV[0].X;
            l2.Y1 = old_CV[0].Y;
            l2.X2 = old_CV[0].X + nor.X;
            l2.Y2 = old_CV[0].Y + nor.Y;
            Canvas.SetZIndex(l2, 3);  
            C.Children.Add(l2);
                
            DrawPoint(old_CV[0].X, old_CV[0].Y, g, Colors.Red);         // vykresli vysledny bod na krivke
        }

        private void IsVisualised_Unchecked(object sender, RoutedEventArgs e)       // prestane vykreslovat casteljaua
        {
            g.Children.Clear();
            CasteljauLines.Clear();

            DrawPoints(ControlVertices);
            DrawLines(ControlVertices, g, Colors.Blue, ControlPolygone);

            if (ControlVertices.Count > 2)
            {
                DrawCurve(ControlVertices, g);
            }
        }

        private void SliderEpsilon_PreviewMouseMove(object sender, MouseEventArgs e)       // zmena epsilon
        {
            if (BezierCurve.IsChecked == true)
            {
                g.Children.Clear();                             // vymazu sa ciary krivky a prepocitaju
                BezierLines.Clear();

                DrawPoints(ControlVertices);
                DrawLines(ControlVertices, g, Colors.Blue, ControlPolygone);

                if (ControlVertices.Count > 2)
                {
                    DrawCurve(ControlVertices, g);
                    if (IsVisualised.IsChecked == true)
                    {
                        DrawCasteljau(ControlVertices, g);
                    }
                }
            }
            if (NodesCurve.IsChecked == true)               // to iste pre uzlovu krivku
            {
                g.Children.Clear();
                NodesLines.Clear();
                DrawCurve(ControlVertices, g);
                if (IsVisualised.IsChecked == true)
                {
                    DrawCasteljau(ControlVertices, g);
                }
                
            }
        }

        private void IsVisualised_Checked(object sender, RoutedEventArgs e)     // nakresli casteljaua
        {
            if (ControlVertices.Count > 2)
            {
                DrawCasteljau(ControlVertices, g);
            }
        }

        private void SliderT_PreviewMouseMove(object sender, MouseEventArgs e) // zmena parametra t
        {
            ParameterT.Content = Convert.ToDecimal(SliderT.Value);
            if (IsVisualised.IsChecked == true)             // zmena parametra t funguje aj pre bezierovu, aj pre uzlovu krivku
            { 
                g.Children.Clear();

                DrawPoints(ControlVertices);
                DrawLines(ControlVertices, g, Colors.Blue, ControlPolygone);
                if (ControlVertices.Count > 2)
                {
                    DrawCurve(ControlVertices, g);
                    DrawCasteljau(ControlVertices, g);
                }
            }
        }

        private void SliderS_PreviewMouseMove(object sender, MouseEventArgs e) // zmena parametra s
        {
            ParameterS.Content = Convert.ToDecimal(SliderS.Value);
            if (NodesCurve.IsChecked == true)
            {
                g.Children.Clear();
                //DrawLines(ControlVertices, g, Colors.Blue, ControlPolygone);  // toto netreba, lebo polygon sa kresli pri krivke
                DrawCurve(ControlVertices, g);

                if (IsVisualised.IsChecked == true)
                {
                    DrawCasteljau(ControlVertices, g);
                }   
            }
        }

        private void NodesCurve_Checked(object sender, RoutedEventArgs e) // nakresli uzlovu krivku
        {
            BezierCurve.IsChecked = false;
            g.Children.Clear();
            ControlVertices.Clear();

            DrawCurve(ControlVertices, g);
   
        }

        private void SliderA_PreviewMouseMove(object sender, MouseEventArgs e) // zmena paramaetra a
        {
            if (NodesCurve.IsChecked == true)
            {
                g.Children.Clear();
                ControlVertices.Clear();
                DrawCurve(ControlVertices, g);

                if (IsVisualised.IsChecked == true)
                {
                    DrawCasteljau(ControlVertices, g);
                }
            }
        }

        private void SliderB_PreviewMouseMove(object sender, MouseEventArgs e) // zmena paramaetra b
        {
            if (NodesCurve.IsChecked == true)
            {
                g.Children.Clear();
                ControlVertices.Clear();
                DrawCurve(ControlVertices, g);

                if (IsVisualised.IsChecked == true)
                {
                    DrawCasteljau(ControlVertices, g);
                }

            }
        }

        private void BezierCurve_Checked(object sender, RoutedEventArgs e)
        {
            g.Children.Clear();
            if (ControlVertices != null)
            {
                ControlVertices.Clear();                                      // vymaze prvky listov
            }
            if (ControlPolygone != null)
            {
                ControlPolygone.Clear();                                      
            }
            if (CasteljauLines != null)
            {
                CasteljauLines.Clear();                                      
            }
            if (BezierVertices != null)
            {
                BezierVertices.Clear();                                      
            }
            if (BezierLines != null)
            {
                BezierLines.Clear();                                      
            }
            if (NodesPolygone != null)
            {
                NodesPolygone.Clear();                                     
            }
            if (NodesLines != null)
            {
                NodesLines.Clear();
            }
        }

        public void Reset_Click(object sender, RoutedEventArgs e)
        {
            ControlVertices.Clear();                                      // vymaze prvky listov
            ControlPolygone.Clear();
            CasteljauLines.Clear();
            BezierVertices.Clear();
            BezierLines.Clear();
            NodesLines.Clear();
            NodesPolygone.Clear();
            g.Children.Clear();                                          // vymaze vsetko z canvasu
        }
    }
}
