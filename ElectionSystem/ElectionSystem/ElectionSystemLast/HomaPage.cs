using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Windows.Forms.DataVisualization.Charting;
using ElectionSystem;

namespace ElectionSystem
{
    public partial class HomaPage : Form
    {
        ElectionEntities ElectionEntities = new ElectionEntities();
        public HomaPage()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            VoterLoginPage form2 = new VoterLoginPage();

            form2.Show();

            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AdminPage form6 = new AdminPage();

            form6.Show();

            this.Hide();
        }

        private void HomePage_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            GeneralResultPieChart();
            LoadChart3();
            VoteDistributionColumnChart();

            LoadWinnerDetailsToDataGridView();

        }

        private void GeneralResultPieChart()
        {
            using (var context = new ElectionEntities())
            {
                var results = context.Database.SqlQuery<GENERAL_RESULT_VIEW>(
                    "SELECT ID, TotalVoteCount FROM GENERAL_RESULT_VIEW").ToList();

                var totalVotes = context.Database.SqlQuery<int>(
                    "SELECT SUM(TotalVoteCount) FROM GENERAL_RESULT_VIEW").FirstOrDefault();

                chart1.Series.Clear();
                Series series = new Series("s1")
                {
                    ChartType = SeriesChartType.Pie,
                    IsValueShownAsLabel = true,
                };
                chart1.Series.Add(series);

                // Belirttiğiniz renkler
                List<Color> customColors = new List<Color>
        {
            ColorTranslator.FromHtml("#54bb52"), // Yeşil
            ColorTranslator.FromHtml("#bd4242"), // Kırmızı
            ColorTranslator.FromHtml("#617bc3")  // Mavi
        };

                int colorIndex = 0; // Renk listesi için index

                foreach (var result in results)
                {
                    double percentage = ((double)result.TotalVoteCount / totalVotes * 100);
                    var piePartyName = context.Database.SqlQuery<string>(
                        $"SELECT PartyName FROM PoliticalParty p WHERE p.ID = {result.ID}").FirstOrDefault();

                    // Veri noktasını ekle
                    int pointIndex = chart1.Series["s1"].Points.AddXY(piePartyName, result.TotalVoteCount);
                    var dataPoint = chart1.Series["s1"].Points[pointIndex];

                    // Renk atama
                    if (colorIndex < customColors.Count)
                    {
                        dataPoint.Color = customColors[colorIndex];
                        colorIndex++;
                    }

                    dataPoint.Label = $"{percentage:F2}%";
                    dataPoint.LegendText = piePartyName;
                }

                chart1.Titles.Clear(); 
                Title title = new Title
                {
                    Text = "General Election Results",
                    Font = new Font("Arial", 16, FontStyle.Bold),
                    ForeColor = Color.Black
                };
                chart1.Titles.Add(title);
                chart1.Legends[0].Docking = Docking.Bottom;
            }
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }
        private void VoteDistributionColumnChart()
        {
            this.Controls.Add(chart2);
            ChartArea chartArea = new ChartArea("MainArea")
            {
                AxisY = { Minimum = 0, Title = "Vote Count", TitleFont = new Font("Arial", 16, FontStyle.Bold) }, 
                AxisX = { Title = "City", Interval = 1, TitleFont = new Font("Arial", 16, FontStyle.Bold) } 
            };
            chartArea.AxisX.LabelStyle.Font = new Font("Arial", 14);
            chartArea.AxisX.LabelStyle.Angle = -45;
            chart2.ChartAreas.Add(chartArea);

            Series AQP = new Series("Ant Queen Party")
            {
                ChartType = SeriesChartType.Column,
                ChartArea = "MainArea",
                IsValueShownAsLabel = true,
                Color = ColorTranslator.FromHtml("#54bb52")
            };

            Series WAP = new Series("Worker Ants Party")
            {
                ChartType = SeriesChartType.Column,
                ChartArea = "MainArea",
                IsValueShownAsLabel = true,
                Color = ColorTranslator.FromHtml("#bd4242")
            };

            Series MWP = new Series("Mealworm Party")
            {
                ChartType = SeriesChartType.Column,
                ChartArea = "MainArea",
                IsValueShownAsLabel = true,
                Color = ColorTranslator.FromHtml("#617bc3")
            };

            using (var context = new ElectionEntities())
            {
                var cities = context.Database.SqlQuery<string>("SELECT CityName FROM City ORDER BY CityName").ToList();

                foreach (var city in cities)
                {
                    var AQPVotes = context.Database.SqlQuery<int?>(
                        "SELECT VoteCount FROM CHECK_RESULT_FOR_CLUSTER_COLUMNS WHERE CityName = @p0 AND PartyName = 'Ant Queen Party'", city
                    ).FirstOrDefault() ?? 0;

                    var WAPVotes = context.Database.SqlQuery<int?>(
                        $"SELECT VoteCount FROM CHECK_RESULT_FOR_CLUSTER_COLUMNS WHERE CityName = @p0 AND PartyName = 'Worker Ants Party'", city
                    ).FirstOrDefault() ?? 0;

                    var MWPVotes = context.Database.SqlQuery<int?>(
                        $"SELECT VoteCount FROM CHECK_RESULT_FOR_CLUSTER_COLUMNS WHERE CityName = @p0 AND PartyName = 'Mealworm Party'", city
                    ).FirstOrDefault() ?? 0;

                    // If party didn't participate in that city, vote is 0, natürlich.
                    AQP.Points.AddXY(city, AQPVotes);
                    WAP.Points.AddXY(city, WAPVotes);
                    MWP.Points.AddXY(city, MWPVotes);
                }
            }

            chart2.Series.Add(AQP);
            chart2.Series.Add(WAP);
            chart2.Series.Add(MWP);

            Legend legend = new Legend("Legend");
            chart2.Legends.Add(legend);
            chart2.Series["Series1"].IsVisibleInLegend = false;

            Title chartTitle = new Title
            {
                Text = "Vote Distribution Of Cities",
                Font = new Font("Arial", 14, FontStyle.Bold)
            };
            chart2.Titles.Add(chartTitle);

            chart2.ChartAreas[0].Position = new ElementPosition(5, 5, 90, 85);
            chart2.ChartAreas[0].InnerPlotPosition = new ElementPosition(10, 10, 80, 80);
        }




        private void LoadChart3()
        {
            using (var context = new ElectionEntities())
            {
                var results = context.Election
                    .GroupBy(e => e.ElectionDate.Year)
                    .Select(group => new
                    {
                        Year = group.Key,
                        AvgParticipationRate = group.Average(e => e.ParticipationRate ?? 0)
                    })
                    .OrderBy(result => result.Year)
                    .ToList();

                chart3.Series.Clear();

                Series series = new Series
                {
                    Name = "Participation Rate by Year",
                    ChartType = SeriesChartType.Line,
                    BorderWidth = 3,
                    IsValueShownAsLabel = true,
                    Color = Color.Green,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };

                foreach (var result in results)
                {
                    series.Points.AddXY(result.Year, result.AvgParticipationRate);
                }

                chart3.Series.Add(series);

                chart3.Titles.Clear();
                Title title = new Title
                {
                    Text = "Participation Rate by Year",
                    Font = new Font("Arial", 14, FontStyle.Bold),
                    ForeColor = Color.Black
                };
                chart3.Titles.Add(title);

                chart3.Invalidate();
            }
        }


        private void LoadWinnerDetailsToDataGridView()
        {

            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
 
            dataGridView1.Columns.Add("CityName", "City");
            dataGridView1.Columns.Add("WinningParty", "Winning Party");
            dataGridView1.Columns.Add("WinningCandidate", "Winning Candidate");

            using (var context = new ElectionEntities())
            {
              
                var cities = context.City
                    .OrderBy(c => c.CityName)
                    .ToList();

                foreach (var city in cities)
                {
                  
                    var results = context.Result
                        .Where(r => r.CityID == city.ID)
                        .ToList();

                    if (results.Count == 0)
                    {
                        dataGridView1.Rows.Add(city.CityName, "No Winner", "No Candidate");
                        continue;
                    }
                    var winningResult = results
                        .OrderByDescending(r => r.VoteCount)
                        .FirstOrDefault();

                    var winningParty = context.PoliticalParty
                        .FirstOrDefault(p => p.ID == winningResult.PartyID)?.PartyName ?? "Unknown Party";

                    var winningCandidate = context.Candidate
                        .Where(c => c.CityID == city.ID && c.PartyID == winningResult.PartyID)
                        .Select(c => context.Citizen.FirstOrDefault(ct => ct.ID == c.ID).FirstName + " " + context.Citizen.FirstOrDefault(ct => ct.ID == c.ID).LastName)
                        .FirstOrDefault() ?? "No Candidate";
                    dataGridView1.Rows.Add(city.CityName, winningParty, winningCandidate);
                }
            }
            
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }



        private void chart3_Click(object sender, EventArgs e)
        {

        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
