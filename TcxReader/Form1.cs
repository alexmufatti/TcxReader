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
using System.Xml.Linq;
using System.Xml.Serialization;
using TcxReader.data.xml;

namespace TcxReader
{
    public partial class Form1 : Form
    {
        private List<Activity> _activities = new List<Activity>();

        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.ShowDialog(this);
            if (!string.IsNullOrEmpty(fd.FileName))
            {
                _loadFile(fd.FileName);
            }
        }

        private void _loadFile(string file)
        {
            if (File.Exists(file))
            {
                try
                {
                    XElement root = XElement.Load(file);

                    XNamespace ns1 = "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2";

                    _activities = (from activityElement in root.Descendants(ns1 + "Activities")
                                   select new Activity
                                   {
                                       Laps =
                                       (from lapElement in
                                            activityElement.Descendants(ns1 + "Lap")
                                        select new Lap
                                           {
                                               TotalTimeSeconds = lapElement.Element(ns1 + "TotalTimeSeconds") != null ? Convert.ToDouble((string)lapElement.Element(ns1 + "TotalTimeSeconds").Value) : 0.00,
                                               DistanceMeters = lapElement.Element(ns1 + "DistanceMeters") != null ? Convert.ToDouble((string)lapElement.Element(ns1 + "DistanceMeters").Value) : 0.00,
                                               MaximumSpeed = lapElement.Element(ns1 + "MaximumSpeed") != null ? Convert.ToDouble((string)lapElement.Element(ns1 + "DistanceMeters").Value) : 0.00,
                                               Tracks = (from trackElement in
                                                             lapElement.Descendants(ns1 + "Track")
                                                         select new Track
                                                            {
                                                                TrackPoints = (from trackPointElement in
                                                                                   trackElement.Descendants(ns1 + "Trackpoint")
                                                                               select new Trackpoint
                                                                               {
                                                                                   AltitudeMeters = trackPointElement.Element(ns1 + "AltitudeMeters") != null ? Convert.ToDouble((string)trackPointElement.Element(ns1 + "AltitudeMeters").Value) : 0.00,
                                                                                   DistanceMeters = trackPointElement.Element(ns1 + "DistanceMeters") != null ? Convert.ToDouble((string)trackPointElement.Element(ns1 + "DistanceMeters").Value) : 0.00,
                                                                                   Time = trackPointElement.Element(ns1 + "Time") != null ? DateTime.ParseExact((string)trackPointElement.Element(ns1 + "Time").Value, "yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture) : DateTime.MinValue


                                                                               }

                                                   ).ToList()

                                                            }

                                   ).ToList()
                                           }

                                   ).ToList()
                                   }).ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message);
                }

                if (radioButton1.Checked)
                    _drawDistanceChart();
                else
                    _drawTimeChart();

            }
        }


        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                trackBar1.Value = 100;
                _drawDistanceChart();
            }
            else
            {
                trackBar1.Value = 60;
                _drawTimeChart();
            }
        }

        private void _drawTimeChart()
        {
            _cleanChart();
            foreach (var area in chart1.ChartAreas)
            {
                area.AxisX.Interval = 10;
            }
            foreach (var a in _activities)
            {
                double prevDistance = -1;
                DateTime prevTime = DateTime.MinValue;
                double prevHeight = -1;
                long ticks = 0;

                double maxSlopePlus = 0;

                double maxSlopeMinus = 0;

                foreach (var l in a.Laps)
                {
                    foreach (var t in l.Tracks)
                    {


                        foreach (var tp in t.TrackPoints)
                        {
                            if (ticks == 0)
                                ticks = tp.Time.Ticks;
                            chart1.Series[0].Points.Add(new System.Windows.Forms.DataVisualization.Charting.DataPoint() { YValues = new double[] { tp.AltitudeMeters }, XValue = TimeSpan.FromTicks(tp.Time.Ticks - ticks).TotalMinutes });
                            if (prevTime != DateTime.MinValue && prevHeight != -1 && tp.Time - prevTime > new TimeSpan(0,0, trackBar1.Value))
                            {
                                double y = ((tp.AltitudeMeters - prevHeight) / (tp.DistanceMeters - prevDistance)) * 100;
                                if (y < 100 && y > -100)
                                {
                                    chart1.Series[1].Points.Add(new System.Windows.Forms.DataVisualization.Charting.DataPoint() { YValues = new double[] { y }, XValue = TimeSpan.FromTicks(tp.Time.Ticks - ticks).TotalMinutes });

                                    if (y > 0)
                                        maxSlopePlus = y > maxSlopePlus ? y : maxSlopePlus;
                                    else
                                        maxSlopeMinus = y < maxSlopePlus ? y : maxSlopePlus;
                                }
                                
                                prevHeight = tp.AltitudeMeters;
                                prevDistance = tp.DistanceMeters;
                                prevTime = tp.Time;
                            }

                            if (prevDistance == -1 && prevHeight == -1)
                            {
                                prevHeight = tp.AltitudeMeters;
                                prevDistance = tp.DistanceMeters;
                                prevTime = tp.Time;
                            }
                        }
                    }
                }
                label2.Text = maxSlopePlus.ToString("0.00\\%");
                label4.Text = maxSlopeMinus.ToString("0.00\\%");
            }
        }

        private void _drawDistanceChart()
        {
            _cleanChart();
            foreach (var area in chart1.ChartAreas)
            {
                area.AxisX.Interval = 1000;
            }
            foreach (var a in _activities)
            {
                double prevDistance = -1;
                double prevHeight = -1;

                double maxSlopePlus = 0;

                double maxSlopeMinus = 0;

                foreach (var l in a.Laps)
                {
                    foreach (var t in l.Tracks)
                    {


                        foreach (var tp in t.TrackPoints)
                        {
                            chart1.Series[0].Points.Add(new System.Windows.Forms.DataVisualization.Charting.DataPoint() { YValues = new double[] { tp.AltitudeMeters }, XValue = tp.DistanceMeters });
                            if (prevDistance != -1 && prevHeight != -1 && tp.DistanceMeters - prevDistance > trackBar1.Value)
                            {
                                double y = ((tp.AltitudeMeters - prevHeight) / (tp.DistanceMeters - prevDistance)) * 100;
                                if (y < 100 && y > -100)
                                {
                                    if (y > 0)
                                        maxSlopePlus = y > maxSlopePlus ? y : maxSlopePlus;
                                    else
                                        maxSlopeMinus = y < maxSlopeMinus ? y : maxSlopeMinus;
                                    chart1.Series[1].Points.Add(new System.Windows.Forms.DataVisualization.Charting.DataPoint() { YValues = new double[] { y }, XValue = tp.DistanceMeters });
                                }
                                
                                prevHeight = tp.AltitudeMeters;
                                prevDistance = tp.DistanceMeters;
                            }

                            if (prevDistance == -1 && prevHeight == -1)
                            {
                                prevHeight = tp.AltitudeMeters;
                                prevDistance = tp.DistanceMeters;
                            }
                        }
                    }
                }
                label2.Text = maxSlopePlus.ToString("0.00\\%");
                label4.Text = maxSlopeMinus.ToString("0.00\\%");
            }



        }

        private void _cleanChart()
        {
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            
            if (radioButton1.Checked)
            {
                _drawDistanceChart();
                label5.Text = trackBar1.Value.ToString() + "m";
            }
            else
            {
                _drawTimeChart();
                label5.Text = trackBar1.Value.ToString()+"s";
            }

        }
    }
}
