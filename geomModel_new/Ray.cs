using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace geomModel_new
{
    class Ray
    {
        public static double[] H;               //масив глибин профілю швидкості звуку
        public static double[] C;               //масив швидкостей профілю швидкості звуку
        public static double[] K;               //масив коефіцієнтів зміни швидкості звуку за глибиною
        public double[] R;                      //масив радіусів кіл
        public double[] Xr;                     //масив абсцис центрів кіл
        public double[] BeginAngl;              //масив початкових кутів кожного водного шару
        public double[] EndAngl;                //масив кінцевих кутів кожного водного шару
        public static double[] Yr;              //масив ординат центрів кіл
        public double[] dX;                     //масив зміщень по осі Ox променя у водному шарі 
        double Xlast;                           //поточна кінцева відстань на яку розповсюдився промінь по осі Ox
        public double TetRad;                   //початковий кут променя в радіанах
        public static double Hgas;              //глибина ГАС
        public static double Hobj;              //глибина об'єкта
        public static double Lobj;              //відстань між ГАС та об'єктом по осі Ox
        public double L;                        //довжина траєкторії променя
        public double Time;                     //час ходу променя
        public double Rgas;                     //радіус початкової дуги кола
        public double Xrgas;                    //абсциса центра початкової дуги кола
        public double Yrgas;                    //ордината центра початкової дуги кола
        List<int> SeqI;                         //послідовність проходження по водним шарам
        double Angle;                           //поточний гострий кут
        int I;                                  //номер поточного водного шару
        public static int iGas;                 //номер водного шару, в якому розміщено ГАС
        bool Nedkm0;                            //true, якщо промінь не досяг верхньої межі водного шару
        bool Nedkb0;                            //true, якщо промінь не досяг нижньої межі водного шару
        static int iObj;                        //номер водного шару, в якому розміщено об'єкт
        bool inLobj;                            //логічна змінна вказує чи промінь пройшов відстань до об'єкта

        //конструктор ініціалізації приймає один параметр, яким задається початковий кут променя
        public Ray(double tgrd)
        {
            TetRad = tgrd * Math.PI / 180;           //присвоєння значення початкового кута в радіанах
            Xlast = 0;                               //поточна кінцева відстань, на яку розповсюдився промінь спочатку дорівнює 0
            R = new double[H.Length - 1];            //виділення пам'яті для масиву радіусів кіл 
            Xr = new double[R.Length];               //виділення пам'яті для масиву абсцис кіл
            BeginAngl = new double[R.Length];        //виділення пам'яті для масиву початкових кутів кожного водного шару
            EndAngl = new double[R.Length];          //виділення пам'яті для масиву кінцевих кутів кожного водного шару
            I = iGas;                                //початковим водним шаром є водний шар, в якому розташовується ГАС
            inLobj = false;                          //спочатку відстань, яку пройшов промінь дорівнює 0
            L = Time = 0;                            //початкові значення довжини таєкторії та часу ходу променя дорівнюють 0
            Angle = TetRad;                          //поточний гострий кут дорівнює початковому куту променя
            Rgas = 0;                                //значення радіуса початкової дуги кола
            Xrgas = 0; Yrgas = 0;                    //початкові значення координат центра кола, дуга якого проходить через ГАС
            dX = new double[R.Length];               //виділення пам'яті для масиву зміщень променя по осі Ox
            Nedkm0 = Nedkb0 = false;                 //початкові значення встановлюються в false
            SeqI = new List<int>();               //виділення пам'яті для послідовності проходження водних шарів
        }  

        //статичний метод, який приймає значення профілю швидкості звуку, глибину ГАС та об'єкта і відстань між ними
        public static void SetProfileGasObj(double[] h, double[] c, double hg, double hobj, double lobj)
        {
            H = new double[h.Length];                 //виділення пам'яті для масиву глибин
            C = new double[c.Length];                 //виділення пам'яті для масиву швидкостей звуку
            K = new double[h.Length - 1];             //виділення пам'яті для масиву коефіцієнтів зміни швидкості звуку
            Yr = new double[h.Length - 1];            //виділення пам'яті для ординат центрів кіл
            for(int i=0; i<h.Length;i++)              //цикл присвоєння значень профілю швидкості звуку відповідним полям класу
            {
                H[i] = h[i];                          //присвоєння значень глибин профілю швидкості звуку
                C[i] = c[i];                          //присвоєння значень швидкостей звуку
            }
            for(int i=0; i<H.Length-1;i++)            //цикл розрахунку коефіцієнтів зміни швидкостей звуку та ординат центрів кіл
            {
                K[i] = (C[i + 1] - C[i]) / (H[i + 1] - H[i]);
                Yr[i]= C[i] / K[i] - H[i];
            }
            Hgas = hg; Hobj = hobj;                   //присвоєння значень глибин ГАС та об'єкта
            Lobj = lobj;                              //присвоєння значення відстані між ГАС та об'єктом
            bool isobj = false;                       //логічний індикатор визначення водного шару, в якому розміщується об'єкт
            if (Hobj == 0)                            //якщо глибина об'єкта дорівнює 0
            {
                iObj = 0;                             //номер водного шару об'єкта дорівнє 0
                isobj = true;                         //індикатор встановлюємо в true
            }
            if (Hgas == 0) iGas = 0;                  //якщо глибина ГАС дорівнює 0, то номер водного шару ГАС дорівнює 0
            else
            {
                bool isgas = false;                            //логічні індикатори визначення водних шарів ГАС та об'єкта
                for (int i = 0; i < K.Length; i++)             //цикл пошуку водних шарів, в яких знаходяться ГАС та об'єкт
                {
                    if (!isgas && Hgas > H[i] && Hgas <= H[i + 1])       //знаходження водного шару, в якому розташовується ГАС
                    {
                        iGas = i;
                        isgas=true;
                    }
                    if (!isobj && Hobj > H[i] && Hobj <= H[i + 1])       //знаходження водного шару, в якому розташовується об'єкт
                    {
                        iObj = i;
                        isobj=true;
                    }
                    if (isgas && isobj)                                  //якщо обидва водні шари знайдено, то вихід із циклу
                        break;
                }
            }
        }

        public void Layer()
        {
            //if (I < 0) SeqI.Add(I + 1); else SeqI.Add(I);
            double fib = 0;
            double fie = 0;
            if (Xlast == 0 && I == iGas)
            {
                C[I] += K[I] * (Hgas - H[I]);
                Rgas = R[I] = C[I] / (Math.Abs(K[I]) * Math.Sin(TetRad));
                Yrgas = Yr[I] = C[I] / K[I] - Hgas;
                Xrgas = Xr[I] = C[I] / (Math.Tan(TetRad) * K[I]);
                SeqI.Add(I);
            }
            /*else if(I<0)
            {
                I++;
                fib = 1;
                if (K[I] < 0)
                {
                    fie = Math.PI - BeginAngl[I];
                    R[I] = C[I] / (Math.Abs(K[I]) * Math.Sin(fie));
                    Xr[I] = C[I] / (K[I] * Math.Tan(fie)) + Xlast;
                }
            }*/
            if (K[I] < 0)                            
            {                                          
                if (Angle < Math.PI/2)                  
                {
                    double fi_endg = Angle;
                    double fi_beging = Math.Asin(Math.Abs(Yr[I] + H[I + 1]) / R[I]);
                    Angle = fi_beging;
                    dX[I] = R[I] * Math.Abs(Math.Cos(fi_endg) - Math.Cos(fi_beging));
                    Xlast += dX[I];
                    if (Xlast > Lobj && Xr[I] + R[I] * Math.Cos(fi_endg) <= Lobj)
                        inLobj = true;
                    else if (Xlast <= Lobj)
                    {
                        double lri = Math.Abs(R[I] * (fi_endg - fi_beging));
                        Time += 2 * lri / (C[I + 1] + C[I]);
                        L += lri;
                    }
                    BeginAngl[I] = fi_beging; EndAngl[I] = fi_endg;
                    I++;
                }
                else
                {
                    double fi_endg = Angle;
                    double fi_beging;
                    if (Math.Abs(Yr[I] + H[I]) < R[I])
                    {
                        Angle = Math.Asin(Math.Abs(Yr[I] + H[I]) / R[I]);
                        fi_beging = Math.PI - Angle;
                    }
                    else
                    {
                        Angle = Math.Asin(Math.Abs(Yr[I] + H[I + 1]) / R[I]);
                        fi_beging = Angle;
                        Nedkm0 = true;
                    }
                    BeginAngl[I] = fi_beging; EndAngl[I] = fi_endg;
                    dX[I] = R[I] * Math.Abs(Math.Cos(fi_endg) - Math.Cos(fi_beging));
                    Xlast += dX[I];
                    if (Xlast > Lobj && Xr[I] + R[I] * Math.Cos(fi_endg) <= Lobj)
                        inLobj = true;
                    else if (Xlast <= Lobj)
                    {
                        double lri = Math.Abs(R[I] * (fi_endg - fi_beging));
                        Time += 2 * lri / (C[I + 1] + C[I]);
                        L += lri;
                    }
                    if (Nedkm0) I++; else I--;
                }
            }
            else if(K[I]>0) 
            {
                if (Angle < Math.PI/2) 
                {
                    double fi_beging = Math.PI + Angle;
                    double fi_endg;
                    if (Math.Abs(Yr[I] + H[I + 1]) <= R[I])
                    {
                        Angle = Math.Asin(Math.Abs(Yr[I] + H[I + 1]) / R[I]);
                        fi_endg = Math.PI + Angle;
                    }
                    else
                    {
                        Angle = Math.Asin(Math.Abs(Yr[I] + H[I]) / R[I]);
                        fi_endg = 2 * Math.PI - Angle;
                        Nedkb0 = true;
                    }
                    EndAngl[I] = fi_endg; BeginAngl[I] = fi_beging;
                    dX[I] = R[I] * Math.Abs(Math.Cos(fi_endg) - Math.Cos(fi_beging));
                    Xlast += dX[I];
                    if (Xlast > Lobj && Xr[I] + R[I] * Math.Cos(fi_beging) <= Lobj)
                        inLobj = true;
                    else if (Xlast <= Lobj)
                    {
                        double lri = Math.Abs(R[I] * (fi_endg - fi_beging));
                        Time += 2 * lri / (C[I + 1] + C[I]);
                        L += lri;
                    }
                    if (Nedkb0) I--; else I++;
                }
                else
                {
                    double fi_beging = Math.PI + Angle;
                    Angle = Math.Asin(Math.Abs(Yr[I] + H[I]) / R[I]);
                    double fi_endg = 2 * Math.PI - Angle;
                    BeginAngl[I] = fi_beging; EndAngl[I] = fi_endg;
                    dX[I] = R[I] * Math.Abs(Math.Cos(fi_endg) - Math.Cos(fi_beging));
                    Xlast += dX[I];
                    if (Xlast > Lobj && Xr[I] + R[I] * Math.Cos(fi_beging) <= Lobj)
                        inLobj = true;
                    else if (Xlast <= Lobj)
                    {
                        double lri = Math.Abs(R[I] * (fi_endg - fi_beging));
                        Time += 2 * lri / (C[I + 1] + C[I]);
                        L += lri;
                    }
                    I--;
                }
            }
        }

        public void VisualizRay(out double[] arrXn, out double[] arrYn)
        {
            List<double> Xn = new List<double>();
            List<double> Yn = new List<double>();
            foreach (int tempI in SeqI)
            {
                double dfi = Math.Abs(EndAngl[tempI] - BeginAngl[tempI]) / 180;
                if (K[tempI] < 0)
                {
                    for (double fi = EndAngl[tempI]; fi >= BeginAngl[tempI]; fi -= dfi)
                    {
                        Xn.Add(Xrgas + Rgas * Math.Cos(fi));
                        Yn.Add(Yrgas + Rgas * Math.Sin(fi));
                    }
                }
                else if (K[tempI] > 0)
                {
                    for (double fi = BeginAngl[tempI]; fi <= EndAngl[tempI]; fi += dfi)
                    {
                        Xn.Add(Xrgas + Rgas * Math.Cos(fi));
                        Yn.Add(Yrgas + Rgas * Math.Sin(fi));
                    }
                }
            }
            arrXn = new double[Xn.Count];
            arrYn = new double[Yn.Count];
            Xn.CopyTo(arrXn);
            Yn.CopyTo(arrYn);
        }
    }
}
