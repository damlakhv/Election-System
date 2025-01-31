using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ElectionSystem
{
    public partial class CityAdmin : Form
    {
        public CityAdmin()
        {
            InitializeComponent();
        }

        private void Form17_Load(object sender, EventArgs e)
        {
            LoadCities();
            CustomizeDataGridView();

        }

        private void LoadCities()
        {
            using (var context = new ElectionEntities())
            {
                var cities = context.City
                    .Select(c => new
                    {
                        CityID = c.ID,
                        CityName = c.CityName,
                        RegionName = context.Region.FirstOrDefault(r => r.ID == c.RegionID).RegionName
                    })
                    .ToList();

                dataGridView1.DataSource = cities;
            }
        }
        private void CustomizeDataGridView()
        {
            if (dataGridView1.Columns.Contains("CityID"))
                dataGridView1.Columns["CityID"].HeaderText = "City ID";

            if (dataGridView1.Columns.Contains("CityName"))
                dataGridView1.Columns["CityName"].HeaderText = "City Name";

            if (dataGridView1.Columns.Contains("RegionName"))
                dataGridView1.Columns["RegionName"].HeaderText = "Region Name";

            if (dataGridView1.Columns.Contains("CityID"))
                dataGridView1.Columns["CityID"].ReadOnly = true;

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Please enter valid City Name and Region Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string cityName = textBox1.Text.Trim();
            string regionName = textBox2.Text.Trim();

            using (var context = new ElectionEntities())
            {
                var region = context.Region.FirstOrDefault(r => r.RegionName == regionName);

                if (region == null)
                {
                    MessageBox.Show("The specified Region does not exist. Please check the Region Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var newCity = new City
                {
                    CityName = cityName,
                    RegionID = region.ID
                };

                context.City.Add(newCity);
                context.SaveChanges();

                MessageBox.Show("City added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadCities();
                textBox1.Clear();
                textBox2.Clear();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a city to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedCityID = dataGridView1.SelectedRows[0].Cells["CityID"].Value?.ToString();

            if (string.IsNullOrEmpty(selectedCityID))
            {
                MessageBox.Show("City ID not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var context = new ElectionEntities())
            {
                var city = context.City.FirstOrDefault(c => c.ID.ToString() == selectedCityID);

                if (city == null)
                {
                    MessageBox.Show("City not found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                
                var ballotBoxes = context.Ballot_box.Where(bb => bb.CityID == city.ID).ToList();

                if (ballotBoxes.Any())
                {
                    MessageBox.Show(
                        $"The city \"{city.CityName}\" cannot be deleted because it still has associated Ballot Boxes. The election might still be ongoing.",
                        "Cannot Delete City",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                var confirmResult = MessageBox.Show(
                    $"Deleting the city \"{city.CityName}\" will also delete:\n\n" +
                    "- Candidates associated with this city\n" +
                    "- Deputy Mayors of those candidates\n" +
                    "- Results associated with this city\n\n" +
                    "This action is irreversible. Do you want to proceed?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (confirmResult != DialogResult.Yes)
                {
                    return;
                }

                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        
                        var resultsToDelete = context.Result.Where(r => r.CityID == city.ID).ToList();
                        if (resultsToDelete.Any())
                        {
                            context.Result.RemoveRange(resultsToDelete);
                        }

                       
                        var candidatesToDelete = context.Candidate.Where(c => c.CityID == city.ID).ToList();
                        foreach (var candidate in candidatesToDelete)
                        {
                            var deputyMayor = context.DeputyMayor.FirstOrDefault(dm => dm.CandidateID == candidate.ID);
                            if (deputyMayor != null)
                            {
                                context.DeputyMayor.Remove(deputyMayor);
                            }
                        }
                        if (candidatesToDelete.Any())
                        {
                            context.Candidate.RemoveRange(candidatesToDelete);
                        }

                      
                        context.City.Remove(city);

                        context.SaveChanges();
                        transaction.Commit();

                        MessageBox.Show("City and all related data have been deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadCities();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"An error occurred while deleting the city: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a city to edit.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedCityID = dataGridView1.SelectedRows[0].Cells["CityID"].Value.ToString();
            string newCityName = textBox1.Text.Trim();
            string newRegionName = textBox2.Text.Trim();

            if (string.IsNullOrWhiteSpace(newCityName) || string.IsNullOrWhiteSpace(newRegionName))
            {
                MessageBox.Show("Please enter valid City Name and Region Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var context = new ElectionEntities())
            {
                var city = context.City.FirstOrDefault(c => c.ID.ToString() == selectedCityID);
                if (city != null)
                {
                    var region = context.Region.FirstOrDefault(r => r.RegionName == newRegionName);
                    if (region == null)
                    {
                        MessageBox.Show("The specified Region does not exist. Please check the Region Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    city.CityName = newCityName;
                    city.RegionID = region.ID;

                    context.SaveChanges();

                    MessageBox.Show("City updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadCities();
                    textBox1.Clear();
                    textBox2.Clear();
                }
                else
                {
                    MessageBox.Show("City not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            AdminPage form6 = new AdminPage();

            form6.Show();

            this.Hide();
        }

        private void Form17_Resize(object sender, EventArgs e)
        {
            int addX = button3.Location.X;
            int addY = button3.Location.Y;


            button2.Location = new Point(addX + button3.Width + 10, addY);
        }
    }
}
