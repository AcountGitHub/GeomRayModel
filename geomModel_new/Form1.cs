using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace geomModel_new
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            DataTable dt = new DataTable();
            dt.Columns.Add("c, м/с");
            dt.Columns.Add("h, м");
            dataGridView1.DataSource = dt;
            double[] c = { 1545, 1540, 1550, 1565 };
            double[] h = { 0, 40, 70, 100 };
            for (int i = 0; i < c.Length; i++)
                dt.Rows.Add(c[i], h[i]);
            openFileDialog1.FileName = "mbb.txt";
            openFileDialog1.Filter = "Текстові файли (*.txt)|*.txt|Всі файли (*.*)|*.*";
            chart1.ChartAreas[0].AxisX.Title = "Швидкість звуку    c, м/с";
            chart1.ChartAreas[0].AxisY.Title = "Глибина H, м";
            chart2.ChartAreas[0].AxisX.Title = "Відстань L, м";
            chart2.ChartAreas[0].AxisY.Title = "Глибина H, м";
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
        }

        private int bn1 = 0;

        private void button5_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            List<string> s = new List<string>();
            string sr;
            FileStream fin = new FileStream(openFileDialog1.FileName, FileMode.Open);
            StreamReader fstr_in = new StreamReader(fin);
            while ((sr = fstr_in.ReadLine()) != null)
                s.Add(sr);
            fstr_in.Close();
            string[,] smas = new string[s.Count, 2];
            for (int i = 0; i < s.Count; i++)
            {
                string[] sm = s[i].Split(',');
                for (int j = 0; j < sm.Length; j++)
                    smas[i, j] = sm[j];
            }
            DataTable dmas = new DataTable();
            dmas.Columns.Add("c, м/с");
            dmas.Columns.Add("h, м");
            double[] c1 = new double[s.Count];
            double[] h1 = new double[s.Count];
            for (int i = 0; i < s.Count; i++)
            {
                if (!Double.TryParse(smas[i, 1], System.Globalization.NumberStyles.Number,
                    System.Globalization.NumberFormatInfo.CurrentInfo, out h1[i]))
                    h1[i] = Double.Parse(smas[i, 1].Replace('.', ','));
                if (!Double.TryParse(smas[i, 0], System.Globalization.NumberStyles.Number,
                    System.Globalization.NumberFormatInfo.CurrentInfo, out c1[i]))
                    c1[i] = Double.Parse(smas[i, 0].Replace('.', ','));
            }
            for (int min = 0; min < h1.Length - 1; min++)
            {
                for (int i = min + 1; i < h1.Length; i++)
                    if (h1[i] < h1[min])
                    {
                        double hr = h1[i];
                        double cr = c1[i];
                        h1[i] = h1[min];
                        c1[i] = c1[min];
                        h1[min] = hr;
                        c1[min] = cr;
                    }
            }
            chart1.Series["ch"].Points.Clear();
            for (int i = 0; i < c1.Length; i++)
                chart1.Series["ch"].Points.AddXY(c1[i], -h1[i]);
            if (c1.Length > 30)
            {
                List<double> cmin = new List<double>();
                List<double> hmin = new List<double>();
                List<int> ki = new List<int>();
                double eps = 0.1;
                bool ismsh = true;
                int k1 = rozb(0, h1.Length - 1, c1, h1, eps, ref ismsh);
                if (ismsh)
                    ki.Add(k1);
                while (ismsh)
                {
                    k1 = rozb(0, ki[ki.Count - 1], c1, h1, eps, ref ismsh);
                    if (ismsh)
                        ki.Add(k1);
                }
                List<int> ksrt = sortByH(ki, h1);
                for (int i = 0; i < ksrt.Count; i++)
                {
                    ismsh = true;
                    int kscount = ksrt.Count;
                    while (ismsh && kscount > i)
                    {
                        k1 = rozb(ksrt[i], ksrt[kscount - 1], c1, h1, eps, ref ismsh);
                        if (ismsh)
                        {
                            if (!ki.Contains(k1))
                                ki.Add(k1);
                            kscount--;
                        }
                    }
                    ksrt = sortByH(ki, h1);
                }
                ismsh = true;
                k1 = rozb(ksrt[ksrt.Count - 1], h1.Length - 1, c1, h1, eps, ref ismsh);
                if (ismsh)
                    ki.Add(k1);
                while (ismsh)
                {
                    k1 = rozb(ksrt[ksrt.Count - 1], ki[ki.Count - 1], c1, h1, eps, ref ismsh);
                    if (ismsh)
                    {
                        if (!ki.Contains(k1))
                            ki.Add(k1);
                        else
                            ismsh = false;
                    }
                }
                List<int> ksrt1 = sortByH(ki, h1);
                ksrt1.Add(h1.Length - 1);
                for (int i = 0; i < ksrt1.Count; i++)
                {
                    ismsh = true;
                    int kscount = ksrt1.Count;
                    while (ismsh && kscount > i)
                    {
                        k1 = rozb(ksrt1[i], ksrt1[kscount - 1], c1, h1, eps, ref ismsh);
                        if (ismsh)
                        {
                            if (!ki.Contains(k1))
                                ki.Add(k1);
                            kscount--;
                        }
                    }
                    ksrt1 = sortByH(ki, h1);
                }
                cmin.Add(c1[0]);
                hmin.Add(h1[0]);
                for (int i = 0; i < ksrt1.Count; i++)
                {
                    cmin.Add(c1[ksrt1[i]]);
                    hmin.Add(h1[ksrt1[i]]);
                }
                cmin.Add(c1[c1.Length - 1]);
                hmin.Add(h1[h1.Length - 1]);
                double[] c = new double[cmin.Count];
                double[] h = new double[hmin.Count];
                cmin.CopyTo(c);
                hmin.CopyTo(h);
                for (int i = 0; i < c.Length; i++)
                    dmas.Rows.Add(c[i], h[i]);
            }
            else
            {
                for (int i = 0; i < c1.Length; i++)
                    dmas.Rows.Add(c1[i], h1[i]);
            }
            dataGridView1.DataSource = dmas;
            for (int i = 0; i < chart1.Series.Count; i++)
            {
                if (chart2.Series[i].Name.StartsWith("pr"))
                    chart2.Series[i].Points.Clear();
            }
            chart2.Series["dn"].Points.Clear();
            chart2.Series["pov"].Points.Clear();
            chart2.Series["gas"].Points.Clear();
            chart2.Series["obj"].Points.Clear();
        }

        private int rozb(int begin, int end, double[] c, double[] h, double eps, ref bool ind)
        {
            double k0 = (c[end] - c[begin]) / (h[end] - h[begin]);
            int size = end - begin + 1;
            double[] c0 = new double[c.Length];
            for (int i = begin; i < begin + size; i++)
                c0[i] = c[begin] + k0 * h[i - begin];
            double[] dc0 = new double[c.Length];
            for (int i = begin; i < begin + size; i++)
                dc0[i] = Math.Abs(c[i] - c0[i]);
            int imax = 0;
            for (int i = begin; i < begin + size; i++)
                if (dc0[imax] < dc0[i] && dc0[i] > eps)
                    imax = i;
            if (imax == 0) ind = false;
            return imax;
        }

        private List<int> sortByH(List<int> k, double[] h)
        {
            List<int> ksort = new List<int>();
            int[] arrk = new int[k.Count];
            k.CopyTo(arrk);
            for (int i = 0; i < arrk.Length - 1; i++)
            {
                for (int j = i + 1; j < arrk.Length; j++)
                {
                    if (h[arrk[j]] < h[arrk[i]])
                    {
                        int r = arrk[i];
                        arrk[i] = arrk[j];
                        arrk[j] = r;
                    }
                }
            }
            ksort.AddRange(arrk);
            return ksort;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dataGridView1.DataSource;
            double[] c = new double[dt.Rows.Count];
            double[] h = new double[dt.Rows.Count];
            chart1.Series["ch"].Points.Clear();
            for (int i = 0; i < c.Length; i++)
            {
                Double.TryParse(dt.Rows[i]["c, м/с"].ToString(), System.Globalization.NumberStyles.Number,
                   System.Globalization.NumberFormatInfo.CurrentInfo, out c[i]);
                Double.TryParse(dt.Rows[i]["h, м"].ToString(), System.Globalization.NumberStyles.Number,
                   System.Globalization.NumberFormatInfo.CurrentInfo, out h[i]);
                chart1.Series["ch"].Points.AddXY(c[i], -h[i]);
            }
            for (int i = 0; i < chart1.Series.Count; i++)
            {
                if (chart2.Series[i].Name.StartsWith("pr"))
                    chart2.Series[i].Points.Clear();
            }
            chart2.Series["dn"].Points.Clear();
            chart2.Series["pov"].Points.Clear();
            chart2.Series["gas"].Points.Clear();
            chart2.Series["obj"].Points.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            chart2.ChartAreas[0].AxisX.Title = "Відстань L, м";
            if (bn1 > 0)
            {
                chart2.Series.Add("pr" + bn1.ToString());
                chart2.Series["pr" + bn1.ToString()].Color = Color.Blue;
                chart2.Series["pr" + bn1.ToString()].ChartType = SeriesChartType.Line;
            }
            DataTable dt = (DataTable)dataGridView1.DataSource;
            double[] c = new double[dt.Rows.Count];
            double[] h = new double[dt.Rows.Count];
            chart1.Series["ch"].Points.Clear();
            for (int i = 0; i < c.Length; i++)
            {
                Double.TryParse(dt.Rows[i]["c, м/с"].ToString(), System.Globalization.NumberStyles.Number,
                   System.Globalization.NumberFormatInfo.CurrentInfo, out c[i]);
                Double.TryParse(dt.Rows[i]["h, м"].ToString(), System.Globalization.NumberStyles.Number,
                   System.Globalization.NumberFormatInfo.CurrentInfo, out h[i]);
                chart1.Series["ch"].Points.AddXY(c[i], -h[i]);
            }
            chart1.ChartAreas[0].AxisX.ScaleView.Zoom(c.Min(), c.Max());
            double Hg;
            double hobj = 65;
            double lobj = 1000;
            Double.TryParse(textBox1.Text, System.Globalization.NumberStyles.Number,
                   System.Globalization.NumberFormatInfo.CurrentInfo, out Hg);
            double Tetgr1, Tetgr2, ftet;
            Double.TryParse(textBox2.Text, System.Globalization.NumberStyles.Number,
                   System.Globalization.NumberFormatInfo.CurrentInfo, out Tetgr1);
            Double.TryParse(textBox14.Text, System.Globalization.NumberStyles.Number,
                   System.Globalization.NumberFormatInfo.CurrentInfo, out Tetgr2);
            Double.TryParse(textBox15.Text, System.Globalization.NumberStyles.Number,
                   System.Globalization.NumberFormatInfo.CurrentInfo, out ftet);
            double[] arrXn;
            double[] arrYn;
            Ray.SetProfileGasObj(h, c, Hg, hobj, lobj);
            double Tetgr = Tetgr1;
            double dtet = (Tetgr2 - Tetgr1) / ftet;
            for (Tetgr = Tetgr1; Tetgr <= Tetgr2; Tetgr += dtet)
            {
                Ray r = new Ray(Tetgr);
                r.Layer();
                //r.Layer();
                r.VisualizRay(out arrXn, out arrYn);
                if (Tetgr > Tetgr1)
                {
                    chart2.Series.Add("pr" + bn1.ToString() + Tetgr.ToString());
                    chart2.Series["pr" + bn1.ToString() + Tetgr.ToString()].Color = Color.Blue;
                    chart2.Series["pr" + bn1.ToString() + Tetgr.ToString()].ChartType = SeriesChartType.Line;
                }
                for (int j = 0; j < arrXn.Length; j++)
                {
                    if (Tetgr > Tetgr1)
                        chart2.Series["pr" + bn1.ToString() + Tetgr.ToString()].Points.AddXY(arrXn[j], arrYn[j]);
                    else
                        chart2.Series["pr" + bn1.ToString()].Points.AddXY(arrXn[j], arrYn[j]);
                }
                chart2.Series["pov"].Points.Clear();
                chart2.Series["dn"].Points.Clear();
                chart2.Series["obj"].Points.Clear();
                chart2.Series["pov"].Points.AddXY(arrXn[0], -h.Min());
                chart2.Series["pov"].Points.AddXY(arrXn.Last(), -h.Min());
                chart2.Series["dn"].Points.AddXY(arrXn[0], -h.Max());
                chart2.Series["dn"].Points.AddXY(arrXn.Last(), -h.Max());
                chart2.Series["obj"].Points.AddXY(0, -Hg);
            }
            bn1++;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < chart2.Series.Count; i++)
            {
                if (chart2.Series[i].Name.StartsWith("pr"))
                    chart2.Series[i].Points.Clear();
                if (chart2.Series[i].Name.StartsWith("prt"))
                {
                    chart2.Series.RemoveAt(i);
                    i--;
                }
                if (chart2.Series[i].Name.StartsWith("probj"))
                {
                    chart2.Series.RemoveAt(i);
                    i--;
                }
            }
            chart2.Series["dn"].Points.Clear();
            chart2.Series["pov"].Points.Clear();
            chart2.Series["gas"].Points.Clear();
            chart2.Series["obj"].Points.Clear();
        }
    }
}
