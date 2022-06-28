using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Numerics;
using System.IO;

namespace GK2
{
    public partial class GK2 : Form
    {
        public static GK2 instance;
        Color sphereColor = Color.Green;
        Color lightColor = Color.White;
        bool useTexture = true;
        bool useInterpolation = false;
        bool moveVertices = false;
        bool stationaryCamera = true;
        Vector3 lightPos;
        Vector3 cameraPos;
        (int i, int j) selectedVertex;
        BitmapOptimized colorTexture;
        BitmapOptimized normalTexture;
        BitmapOptimized bitMap;
        float kd = 0.5f;
        float ks = 0.5f;
        float mCoeff = 1;
        const float ambience = 0.0f;
        float k = 0.01f;
        const float radius = (937 / 2);
        public const int holeRadius = 50;
        public const int holeOffset = 100;
        Vector3[,] m;
        Point prevMousePos;
        Point center = new Point((int)radius+1, (int)radius+1);
        public GK2()
        {
            InitializeComponent();
            instance = this;
            lightPos = new Vector3(1f*radius, 0, 800);
            colorTexture = new BitmapOptimized(new Bitmap(Directory.GetCurrentDirectory() + "\\ziemia.jpg"));
            bitMap = new BitmapOptimized(937, 937);
            normalTexture = new BitmapOptimized(new Bitmap(Directory.GetCurrentDirectory() + "\\normal2.jpg"));
            pictureBox1.Image = bitMap.Bitmap;
            cameraPos = new Vector3(radius * 2f, 0f, 1400f);
            m = new Vector3[0, 0];
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var blackBrush = new SolidBrush(Color.Black);
            Pen pen = new Pen(blackBrush);
            if (!moveVertices)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    var p1 = V2P(m[m.GetLength(0) - 1, j]);
                    var p2 = V2P(m[m.GetLength(0) - 1, (j + 1) % m.GetLength(1)]);
                    var p3 = center;
                    ScanLineColoring(new List<Vector3>() { new Vector3(p1.X, p1.Y, m[m.GetLength(0) - 1, j].Z), new Vector3(p2.X, p2.Y, m[m.GetLength(0) - 1, (j + 1) % m.GetLength(1)].Z), new Vector3(p3.X, p3.Y, radius) });
                }
                Parallel.For(1, m.GetLength(0), (i, state) =>
                {
                    Parallel.For(0, m.GetLength(1), (j, state2) =>
                    {
                        var p1 = V2P(m[i, j]);
                        var p2 = V2P(m[i - 1, j]);
                        var p3 = V2P(m[i - 1, (j + 1) % m.GetLength(1)]);
                        if (i != 0) ScanLineColoring(new List<Vector3>() { new Vector3(p1.X, p1.Y, m[i, j].Z), new Vector3(p2.X, p2.Y, m[i - 1, j].Z), new Vector3(p3.X, p3.Y, m[i - 1, (j + 1) % m.GetLength(1)].Z) });
                        p1 = V2P(m[i, j]);
                        p2 = V2P(m[i, (j + 1) % m.GetLength(1)]);
                        p3 = V2P(m[i - 1, (j + 1) % m.GetLength(1)]);
                        if (i != 0) ScanLineColoring( new List<Vector3>() { new Vector3(p1.X, p1.Y, m[i, j].Z), new Vector3(p2.X, p2.Y, m[i, (j + 1) % m.GetLength(1)].Z), new Vector3(p3.X, p3.Y, m[i - 1, (j + 1) % m.GetLength(1)].Z) });

                    });
                });
            }
            for (int i = 1; i < m.GetLength(0); i++)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    e.Graphics.DrawLine(pen, V2P(m[i, j]), V2P(m[(i - 1) % m.GetLength(0), j]));
                    e.Graphics.DrawLine(pen, V2P(m[i, j]), V2P(m[(i - 1) % m.GetLength(0), (j + 1) % m.GetLength(1)]));
                }
            }
            for (int i = 0; i < m.GetLength(0); i++)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    e.Graphics.DrawLine(pen, V2P(m[i, j]), V2P(m[i, (j + 1) % m.GetLength(1)]));
                }
            }
            for (int j = 0; j < m.GetLength(1); j++)
            {
                e.Graphics.DrawLine(pen, V2P(m[m.GetLength(0) - 1, j]), center);
            }
        }

        private Vector3[,] ComputeVertices(int quality = 7)
        {
            Vector3[,] ret = new Vector3[quality, quality * 4];
            for(int level = 0; level < quality; level++)
            {
                for (int around = 0; around < quality * 4; around++)
                {
                    double lon = (around + 0.5*level) * Math.PI * 2 / (quality * 4);
                    double lat = level                * Math.PI * 2 / (quality * 4);
                    ret[level, around] = new Vector3(
                        (float)(Math.Cos(lat) * Math.Cos(lon)),
                        (float)(Math.Cos(lat) * Math.Sin(lon)),
                        (float) Math.Sin(lat))* radius;
                }
            }
            return ret;
        }

        private Point V2P(Vector3 v)=> new Point((int)(v.X + radius), (int)(v.Y + radius));

        private void ScanLineColoring(List<Vector3> shape)
        {
            var P = shape.Select((point, index) => (point.X, point.Y, index)).OrderBy(shap => shap.Y).ToArray();
            List<(double x, double m, int y_max)> AET = new List<(double, double, int)>();
            int yMin = (int)P[0].Y;
            int yMax = (int)P[P.Length-1].Y;
            int ind = 0;
            Vector3 P1 = shape[0];
            Vector3 P2 = shape[1];
            Vector3 P3 = shape[2];
            for (int y = yMin; y <= yMax; ++y)
            {
                while (P[ind].Y == y - 1)
                {
                    var curr = P[ind];
                    var prev = Array.Find(P, p => p.index == (curr.index - 1 + P.Length) % P.Length);
                    var next = Array.Find(P, p => p.index == (curr.index + 1 + P.Length) % P.Length);
                    if (prev.Y <= curr.Y)
                    {
                        AET.RemoveAll(item => item.y_max == curr.Y);
                    }
                    else
                    {
                        double m = (curr.Y - prev.Y) / (curr.X - prev.X);
                        if (m != 0) AET.Add((P[ind].X + 1 / m, m, (int)prev.Y));
                    }
                    if (next.Y > curr.Y)
                    {
                        double m = (curr.Y - next.Y) / (curr.X - next.X);
                        if (m != 0)
                            AET.Add((P[ind].X + 1 / m, m, ((int)next.Y)));
                    }
                    else
                    {
                        AET.RemoveAll(item => item.y_max == curr.Y);
                    }
                    ind++;
                }
                AET.Sort((item1, item2) => item1.x.CompareTo(item2.x));
                
                if(useInterpolation)
                {
                    for (int i = 0; i < AET.Count; i += 2)
                    {
                        Color c1 = GetColor(shape[0] - new Vector3(radius, radius, 0));
                        Color c2 = GetColor(shape[1] - new Vector3(radius, radius, 0));
                        Color c3 = GetColor(shape[2] - new Vector3(radius, radius, 0));
                        //do testowania czy interpolacja działa poprawnie:
                        //c1 = Color.Red;
                        //c2 = Color.Green;
                        //c3 = Color.Blue;
                        Parallel.For((int)AET[i].x, (int)AET[i + 1].x, (j, state) =>
                        {
                            long x = j;
                            float z = (P3.Z * (x - P1.X) * (y - P2.Y) + P1.Z * (x - P2.X) * (y - P3.Y) + P2.Z * (x - P3.X) * (y - P1.Y) - P2.Z * (x - P1.X) * (y - P3.Y) - P3.Z * (x - P2.X) * (y - P1.Y) - P1.Z * (x - P3.X) * (y - P2.Y))
                        / ((x - P1.X) * (y - P2.Y) + (x - P2.X) * (y - P3.Y) + (x - P3.X) * (y - P1.Y) - (x - P1.X) * (y - P3.Y) - (x - P2.X) * (y - P1.Y) - (x - P3.X) * (y - P2.Y));
                            Vector3 PS = new Vector3(x, y, z);
                            Vector3 v0 = P2 - P1, v1 = P3 - P1, v2 = PS - P1;
                            float d00 = Vector3.Dot(v0, v0);
                            float d01 = Vector3.Dot(v0, v1);
                            float d11 = Vector3.Dot(v1, v1);
                            float d20 = Vector3.Dot(v2, v0);
                            float d21 = Vector3.Dot(v2, v1);
                            float denom = d00 * d11 - d01 * d01;
                            double v = (d11 * d20 - d01 * d21) / denom;
                            double w = (d00 * d21 - d01 * d20) / denom;
                            float u = (float)(1f - v - w);
                            Vector3 col = (float)u * new Vector3(c1.R, c1.G, c1.B) + (float)v * new Vector3(c2.R, c2.G, c3.B) + (float)w * new Vector3(c3.R, c3.G, c3.B);
                            int R = (int)Math.Round(u * c1.R) + (int)Math.Round(v * c2.R) + (int)Math.Round(w * c3.R);
                            int G = (int)Math.Round(u * c1.G) + (int)Math.Round(v * c2.G) + (int)Math.Round(w * c3.G);
                            int B = (int)Math.Round(u * c1.B) + (int)Math.Round(v * c2.B) + (int)Math.Round(w * c3.B);
                            bitMap.SetPixel((int)x, y, Color.FromArgb(Utility.Clamp0_255(R), Utility.Clamp0_255(G), Utility.Clamp0_255(B)));
                        });
                    }
                }
                else
                {
                    for (int i = 0; i < AET.Count; i += 2)
                    {
                        Parallel.For((int)AET[i].x, (int)AET[i + 1].x, (j, state) =>
                        {
                            int x = j;
                            float z = (P3.Z * (x - P1.X) * (y - P2.Y) + P1.Z * (x - P2.X) * (y - P3.Y) + P2.Z * (x - P3.X) * (y - P1.Y) - P2.Z * (x - P1.X) * (y - P3.Y) - P3.Z * (x - P2.X) * (y - P1.Y) - P1.Z * (x - P3.X) * (y - P2.Y))
                        / ((x - P1.X) * (y - P2.Y) + (x - P2.X) * (y - P3.Y) + (x - P3.X) * (y - P1.Y) - (x - P1.X) * (y - P3.Y) - (x - P2.X) * (y - P1.Y) - (x - P3.X) * (y - P2.Y));
                            Color c = GetColor(new Vector3(x, y, z) - new Vector3(radius, radius, 0));
                            bitMap.SetPixel(x, y, c);
                        });
                    }
                }
                for (int i = 0; i < AET.Count; ++i)
                {
                    var (x, m, y_max) = AET[i];
                    AET[i] = (x + 1 / m, m, y_max);
                }
            }
    
        }
        private Color GetColor(Vector3 point)
        {
            var p = point;
            var (ntexx, ntexy) = ((int)(normalTexture.Width * ((point.X + radius) / 937)),
                (int)(normalTexture.Height * ((point.Y + radius) / 937)));
            Color c;
            if (!useTexture)
            {
                c = sphereColor;
            }
            else
            {
                var (ctexx, ctexy) = ((int)(colorTexture.Width * ((point.X+radius)/937)),
                    (int)(colorTexture.Height * ((point.Y+radius) / 937)));
                c = colorTexture.GetPixel(ctexx, ctexy);
            }

            var normalTextureColor = normalTexture.GetPixel(ntexx, ntexy);

            var texn = Utility.ChangeBase(new Vector3(normalTextureColor.R-143, normalTextureColor.G-143, normalTextureColor.B-143) / 143 * new Vector3(1, -1, 1), Vector3.Normalize(p));
            Vector3 n;
            n = Vector3.Normalize((p * k) + texn * (1 - k));
            if ((p.X-holeOffset) * (p.X- holeOffset) + p.Y * p.Y <= holeRadius * holeRadius)
            {
                var newP = Utility.NewPosRight(p);
                n = Vector3.Normalize((newP * k) + texn * (1 - k));
            }
            else if ((p.X + holeOffset) * (p.X + holeOffset) + p.Y * p.Y <= holeRadius * holeRadius)
            {
                var newP = Utility.NewPosLeft(p);
                n = Vector3.Normalize((newP * k) + texn * (1 - k));
            }
            else if (p.X * p.X + (p.Y - holeOffset) * (p.Y - holeOffset) <= holeRadius * holeRadius)
            {
                var newP = Utility.NewPosUp(p);
                n = Vector3.Normalize((newP * k) + texn * (1 - k));
            }
            else if (p.X * p.X + (p.Y + holeOffset) * (p.Y + holeOffset) <= holeRadius * holeRadius)
            {
                var newP = Utility.NewPosDown(p);
                n = Vector3.Normalize((newP * k) + texn * (1 - k));
            }
            else if (p.X * p.X + p.Y * p.Y <= 200 * 200) n = Vector3.Normalize(new Vector3(0, 0, 1) * k + texn * (1 - k));
            n = Vector3.Normalize(n);
            var l = Vector3.Normalize(lightPos - p);

            var diffusion = Vector3.Dot(n, l);
            diffusion = diffusion <= 0 ? 0 : (diffusion * kd);

            var r = Utility.Reflection(n, l);
            var v = new Vector3(0,0,1);
            if(!stationaryCamera)
            {
                v = Vector3.Normalize(cameraPos);
            }
            if (Vector3.Dot(p, v) < 0) return Color.Black;
            var reflection = Vector3.Dot(v, r);
            var reflect = reflection <= 0.03 ? 0 : (ks * Math.Pow(reflection, mCoeff));

            var intensity = Utility.Clamp0(ambience + diffusion + reflect);

            return Color.FromArgb(
                (255 << 24) +
                (Utility.Clamp0_255((int)((lightColor.R * c.R) / 255 * intensity)) << 16) +
                (Utility.Clamp0_255((int)((lightColor.G * c.G) / 255 * intensity)) << 8) +
                (Utility.Clamp0_255((int)((lightColor.B * c.B) / 255 * intensity)) << 0));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (moveVertices) return;
            lightPos = Utility.RotatePoint(lightPos, new Vector3(0, 0, lightPos.Z), (double)numericUpDown1.Value);
            cameraPos = Utility.RotatePoint(cameraPos, new Vector3(0, 0, cameraPos.Z), 2*(double)numericUpDown1.Value);
            pictureBox1.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            unpausebutton.Enabled = true;
            button1.Enabled = false;
            timer1.Enabled = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            useInterpolation = !useInterpolation;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            mCoeff = trackBar1.Value;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            ks = trackBar2.Value / 100f;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            kd = trackBar3.Value / 100f;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            useTexture = !useTexture;
        }

        private void trackBar1_MouseDown(object sender, MouseEventArgs e)
        {
            //timer1.Enabled = false;
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            //timer1.Enabled = true;
        }

        private void trackBar2_MouseDown(object sender, MouseEventArgs e)
        {
            //timer1.Enabled = false;
        }

        private void trackBar2_MouseUp(object sender, MouseEventArgs e)
        {
            //timer1.Enabled = true;
        }

        private void trackBar3_MouseDown(object sender, MouseEventArgs e)
        {
            //timer1.Enabled = false;
        }

        private void trackBar3_MouseUp(object sender, MouseEventArgs e)
        {
            //timer1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ColorDialog MyDialog = new ColorDialog();
            MyDialog.AllowFullOpen = false;
            MyDialog.ShowHelp = true;
            MyDialog.Color = lightColor;

            if (MyDialog.ShowDialog() == DialogResult.OK)
                lightColor = MyDialog.Color;
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            lightPos.Z = trackBar4.Value;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ColorDialog MyDialog = new ColorDialog();
            MyDialog.AllowFullOpen = false;
            MyDialog.ShowHelp = true;
            MyDialog.Color = sphereColor;

            if (MyDialog.ShowDialog() == DialogResult.OK)
                sphereColor = MyDialog.Color;
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            k = trackBar5.Value / 100f;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            var mousePos = pictureBox1.PointToClient(new Point(MousePosition.X, MousePosition.Y));
            if (mousePos.X > 2 * radius || mousePos.Y > 2 * radius) return;
            for (int i = 0; i < m.GetLength(0); i++)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    var vertex = m[i, j];
                    var dx = V2P(vertex).X - mousePos.X;
                    var dy = V2P(vertex).Y - mousePos.Y;
                    if (dx * dx + dy * dy <= 20)
                    {
                        selectedVertex = (i, j);
                        return;
                    }
                }
            }
            if((center.X - mousePos.X) * (center.X - mousePos.X) + (center.Y - mousePos.Y) * (center.Y - mousePos.Y) <= 20)
            {
                selectedVertex = (-2, -2);
                return;
            }


            selectedVertex = (-1, -1);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            var delta = new Point(MousePosition.X - prevMousePos.X, MousePosition.Y - prevMousePos.Y);
            prevMousePos = MousePosition;
            if (selectedVertex.i != -1 && moveVertices)
            {
                if(selectedVertex.i == -2)
                {
                    center = new Point(center.X + delta.X, center.Y + delta.Y);
                    pictureBox1.Invalidate();
                    return;
                }
                m[selectedVertex.i, selectedVertex.j] = new Vector3(m[selectedVertex.i, selectedVertex.j].X + delta.X, m[selectedVertex.i, selectedVertex.j].Y + delta.Y, m[selectedVertex.i, selectedVertex.j].Z);
                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            selectedVertex = (-1, -1);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            m = ComputeVertices((int)numericUpDown2.Value);
            mCoeff = trackBar1.Value;
            ks = trackBar2.Value / 100f;
            kd = trackBar3.Value / 100f;
            k = trackBar5.Value / 100f;
            lightPos.Z = trackBar4.Value;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            moveVertices = checkBox3.Checked;
            selectedVertex = (-1, -1);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            m = ComputeVertices((int)numericUpDown2.Value);
        }

        private void unpausebutton_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            unpausebutton.Enabled = false;
            timer1.Enabled = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            stationaryCamera = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            stationaryCamera = false;
        }
    }
}
