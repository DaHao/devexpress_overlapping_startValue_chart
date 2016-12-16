using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraCharts;


namespace devexpress_overlapping_startValue_chart
{
    public partial class Form1 : Form
    {
        Series s { get { return chartControl1.Series["Series 1"]; } }
        Series ss { get { return chartControl1.Series["Series 2"]; } }
        Series sss { get { return chartControl1.Series["Series 3"]; } }
        Series ssss { get { return chartControl1.Series["Series 4"]; } }

        AxisRange s1_AxisRange = null, s2_AxisRange = null;

        List<DateTime> dateList = new List<DateTime>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (Series s in chartControl1.Series)
                s.Points.Clear();

            Do();
        }

        /// <summary>
        /// 取得DrawAmpChart資料
        /// </summary>
        private void Do()
        {
            dateList = GetDate();

            #region "塞資料"
            Random rn = new Random();

            List<SeriesPoint> SPs = new List<SeriesPoint>();
            List<SeriesPoint> SPs2 = new List<SeriesPoint>();

            //開始的價錢(參考價)
            double s1_startValue = rn.Next(1, 10);
            double s2_startValue = rn.Next(1, 10);

            //亂數填值
            foreach (DateTime dt in dateList)
            {
                double v1, v2, v1_RF, v2_RF;
                do
                {
                    v1 = s1_startValue + Math.Round((rn.NextDouble() - 0.5), 2);
                    v2 = s2_startValue + Math.Round((rn.NextDouble() - 0.5), 2);

                    v1_RF = Math.Abs(v1 - s1_startValue) / s1_startValue;
                    v2_RF = Math.Abs(v2 - s2_startValue) / s2_startValue;
                }
                while (!(v1_RF <= 0.1 && v2_RF <= 0.1));

                SeriesPoint sp = new SeriesPoint();
                sp.DateTimeArgument = dt;
                sp.Values = new double[] { v1 };
                SPs.Add(sp);

                SeriesPoint sp2 = new SeriesPoint();
                sp2.DateTimeArgument = dt;
                sp2.Values = new double[] { v2 };
                SPs2.Add(sp2);
            }

            s.Points.AddRange(SPs.ToArray());
            ss.Points.AddRange(SPs2.ToArray());
            #endregion

            #region "設定AxisRange"
            s1_startValue = (SPs.First() as SeriesPoint).Values[0];
            s2_startValue = (SPs2.First() as SeriesPoint).Values[0];

            double s1_HighValue = SPs.Max(x => x.Values[0]);
            double s2_HighValue = SPs2.Max(x => x.Values[0]);

            double s1_LowValue = SPs.Min(x => x.Values[0]);
            double s2_LowValue = SPs2.Min(x => x.Values[0]);

            s1_AxisRange = new AxisRange(s1_startValue, s1_HighValue, s1_LowValue);
            s2_AxisRange = new AxisRange(s2_startValue, s2_HighValue, s2_LowValue);
            #endregion

            //計算s1、s2的漲跌幅填入s2、s4，s1跟s2的起伏應該要與s3、s4一致
            CaluAmp(SPs, SPs2);

            AdjustAxis();
        }

        /// <summary>
        /// 調整AxisValue
        /// </summary>
        private void AdjustAxis()
        {
            ////比例調整，取最高及最低的倍率
            double percentH = Math.Max(s1_AxisRange.HighProportion, s2_AxisRange.HighProportion);
            double percentL = Math.Min(s1_AxisRange.LowProportion, s2_AxisRange.LowProportion);

            // 各乘上一相同比例的倍率，使原點會重合
            double s1_axisH = s1_AxisRange.GetAxisValue(percentH);
            double s1_axisL = s1_AxisRange.GetAxisValue(percentL);

            double s2_axisH = s2_AxisRange.GetAxisValue(percentH);
            double s2_axisL = s2_AxisRange.GetAxisValue(percentL);

            (chartControl1.Diagram as XYDiagram).AxisY.WholeRange.SetMinMaxValues(s1_axisL, s1_axisH);
            (chartControl1.Diagram as XYDiagram).SecondaryAxesY[0].WholeRange.SetMinMaxValues(s2_axisL, s2_axisH);


            labelControl1.Text = "倍率：(上)" + percentH + ", (下)" + percentL;
            labelControl2.Text = "s1:   " + s1_AxisRange.HighValue + "   " + s1_AxisRange.LowValue + "   s2:   " + s2_AxisRange.HighValue + ", " + s2_AxisRange.LowValue;
        }

        /// <summary>
        /// 計算漲跌，方便對照
        /// </summary>
        /// <param name="SPs"></param>
        /// <param name="SPs2"></param>
        private void CaluAmp(List<SeriesPoint> SPs, List<SeriesPoint> SPs2)
        {
            List<SeriesPoint> SPs3 = new List<SeriesPoint>();
            List<SeriesPoint> SPs4 = new List<SeriesPoint>();

            double s1_startValue = s1_AxisRange.StartValue;
            double s2_startValue = s2_AxisRange.StartValue;

            foreach (SeriesPoint sp in SPs)
            {
                double v = (sp.Values[0] - s1_startValue) / s1_startValue;
                SPs3.Add(new SeriesPoint(sp.DateTimeArgument, v));
            }

            foreach (SeriesPoint sp in SPs2)
            {
                double v = (sp.Values[0] - s2_startValue) / s2_startValue;
                SPs4.Add(new SeriesPoint(sp.DateTimeArgument, v));
            }

            sss.Points.AddRange(SPs3.ToArray());
            ssss.Points.AddRange(SPs4.ToArray());
        }

        private List<DateTime> GetDate()
        {
            List<DateTime> dateList = new List<DateTime>();

            DateTime StartDate = new DateTime(2016, 10, 12);
            int i = 0;
            while (i < 30)
            {
                if (!(StartDate.DayOfWeek == DayOfWeek.Saturday ||
                    StartDate.DayOfWeek == DayOfWeek.Sunday))
                {
                    dateList.Add(StartDate);
                    i++;
                }
                StartDate = StartDate.AddDays(1);
            }

            return dateList;
        }
    }

    /// <summary>
    /// 寫成class，專業度++
    /// </summary>
    public class AxisRange
    {
        public double StartValue { get; set; }
        public double HighValue { get; set; }
        public double LowValue { get; set; }

        /// <summary>
        ///漲跌幅上升1, 所需要的成交值
        /// </summary>
        public double UnitValue
        {
            //這個公式得自漲跌幅公式，推導如下
            //漲跌幅 = (成交價 - 參考價) / 參考價
            //要求漲跌幅 == 1，因為漲跌幅以百分比表示，所以代0.01
            //參考價就是StartValue
            //0.01 = (成交價 - startValue) / startValue
            //因為成交價一定是基於startValue之上或之下，所以成交價可表示成(startValue ± N)
            //0.01 = (startValue ± N - startValue) / startValue
            //0.01 = ±N / startValue
            //±N = startValue * 0.01
            get { return StartValue * 0.01; }
        }
        public double HighProportion { get { return GetProportion(HighValue); } }
        public double LowProportion { get { return GetProportion(LowValue); } }

        public AxisRange(double s, double h, double l)
        {
            this.StartValue = s;
            this.HighValue = h;
            this.LowValue = l;
        }

        /// <summary>
        /// 算出從StartValue到最高/最低值 需要幾個單位值
        /// 如果想提高預留空間，可在回傳值加上一常數
        /// </summary>
        /// <param name="d">最高/最低</param>
        /// <returns></returns>
        private double GetProportion(double d)
        {
            double v = d - StartValue;
            return Math.Round(v / UnitValue, 0, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 根據倍率，算出Axis值
        /// </summary>
        /// <param name="p">倍率</param>
        /// <returns></returns>
        public double GetAxisValue(double p)
        {
            return StartValue + UnitValue * p;
        }
    }
}
