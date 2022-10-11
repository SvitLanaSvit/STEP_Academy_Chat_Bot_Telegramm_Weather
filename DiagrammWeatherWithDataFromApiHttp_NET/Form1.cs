using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using static DiagrammWeatherWithDataFromApiHttp_NET.MyClass;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using Brushes = System.Drawing.Brushes;

namespace DiagrammWeatherWithDataFromApiHttp_NET
{
    public partial class Form1 : Form
    {
        public delegate void TextDelegate(string str);
        public Form1()
        {
            InitializeComponent();
            //cartesianChart1.Series = new SeriesCollection
            //{
            //    new LineSeries
            //    {
            //        Title = "Series 1",
            //        Values = new ChartValues<double> {4, 6, 5, 2, 7}
            //    }
            //};

            //cartesianChart1.AxisX.Add(new Axis
            //{
            //    Title = "Days",
            //    Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" }
            //});

            //cartesianChart1.AxisY.Add(new Axis
            //{
            //    Title = "Sales",
            //    LabelFormatter = value => value.ToString("C") // zeigt Euro
            //});

            //cartesianChart1.LegendLocation = LegendLocation.Right; // zeigr rechts Name von Linie

            ////modifying the series collection will animate and update the chart
            //cartesianChart1.Series.Add(new LineSeries
            //{
            //    Values = new ChartValues<double> { 5, 3, 2, 4, 5 },
            //    LineSmoothness = 0, //straight lines, 1 really smooth lines
            //    PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
            //    PointGeometrySize = 50,
            //    PointForeground = Brushes.Gray
            //});

            //cartesianChart1.DataClick += CartesianChart1_DataClick;

            GetTemp();
        }

        private void CartesianChart1_DataClick(object sender, ChartPoint chartPoint)
        {
            MessageBox.Show("You clicked (" + chartPoint.X + "," + chartPoint.Y + ")");
        }

        private async void GetTemp()
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage())
                {
                    httpRequestMessage.Method = HttpMethod.Get;
                    httpRequestMessage.RequestUri = new Uri("https://api.openweathermap.org/data/2.5/forecast?lat=52.03333&lon=8.53333&units=metric&appid=1405ae0f1cae8233da346faaf7af9466");
                    HttpResponseMessage responseMessage = await client.SendAsync(httpRequestMessage);
                    string content = await responseMessage.Content.ReadAsStringAsync();

                    MyWeather myWeather = JsonSerializer.Deserialize<MyWeather>(content);
                    this.BeginInvoke(new TextDelegate(UpdateForm), $"{myWeather.City.Name}");
                    var weatherDateTime = myWeather.List.Select(t => new { Time=t.DtTxt }).ToList();
                    var weatherTemp = myWeather.List.Select(t => new { Temp = t.Main.Temp }).ToList();
                    var weatherLocal = myWeather.List.Select(t => new { Info = $"{t.DtTxt} : {t.Main.Temp}" }).ToList();
                    StringBuilder sbTemp = new StringBuilder();
                    foreach (var item in weatherLocal)
                    {
                        sbTemp.AppendLine(item.ToString());
                    }
                    textBox1.Text = sbTemp.ToString();

                    StringBuilder sb = new StringBuilder();
                    foreach (var weather in weatherDateTime)
                    {
                        DateTime date = DateTime.Parse(weather.Time);//.ToLocalTime();
                        sb.AppendLine(date.ToString());
                    }
                    string[] arr = sb.ToString().Split('\n');

                    List<double> arrDataDouble = new List<double>();
                    foreach (var weather in weatherTemp)
                    {
                        double temp = double.Parse(weather.Temp.ToString());
                        arrDataDouble.Add(temp);
                    }
                    comboBox1.DataSource = arrDataDouble;

                    ChartValues<double> chartValues = new ChartValues<double>();
                    foreach(var weather in arrDataDouble)
                    {
                        chartValues.Add(weather);
                    }

                    cartesianChart1.AxisX.Add(new Axis
                    {
                        Title = "Days",
                        Labels = arr
                    });

                    cartesianChart1.Series = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Title = "Temp in Bielefeld",
                            Values = chartValues
                        }
                    };                

                    //cartesianChart1.AxisY.Add(new Axis
                    //{
                    //    Title = "Temperatur, C",
                    //    //LabelFormatter = value => value.ToString("C") // zeigt Euro
                    //});

                    cartesianChart1.LegendLocation = LegendLocation.Right;
                    cartesianChart1.DataClick += CartesianChart1_DataClick;
                }
            }
        }

        private void UpdateForm(string text)
        {
            this.Text = text;
        }
    }
}