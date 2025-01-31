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
    public partial class RegionAdmin : Form
    {
        public RegionAdmin()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please enter a valid Region Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string regionName = textBox1.Text.Trim();

            using (var context = new ElectionEntities())
            {
                var newRegion = new Region
                {
                    RegionName = regionName
                };

                context.Region.Add(newRegion);
                context.SaveChanges();

                MessageBox.Show("Region added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadRegions();
                textBox1.Clear();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            string selectedRegionID = dataGridView1.SelectedRows[0].Cells["RegionID"].Value.ToString();
            string newRegionName = textBox1.Text.Trim();
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a region to edit.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(newRegionName))
            {
                MessageBox.Show("Please enter a valid Region Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var context = new ElectionEntities())
            {
                var region = context.Region.FirstOrDefault(r => r.ID.ToString() == selectedRegionID);

                if (region != null)
                {
                    region.RegionName = newRegionName;

                    context.SaveChanges();

                    MessageBox.Show("Region updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadRegions();
                    textBox1.Clear();
                }
                else
                {
                    MessageBox.Show("Region not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a region to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string selectedRegionID = dataGridView1.SelectedRows[0].Cells["RegionID"].Value.ToString();

            using (var context = new ElectionEntities())
            {
                var region = context.Region.FirstOrDefault(r => r.ID.ToString() == selectedRegionID);

                if (region != null)
                {
                    var confirmResult = MessageBox.Show($"Are you sure you want to delete the region: {region.RegionName}?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (confirmResult == DialogResult.Yes)
                    {
                        context.Region.Remove(region);
                        context.SaveChanges();

                        MessageBox.Show("Region deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadRegions();
                    }
                }
                else
                {
                    MessageBox.Show("Region not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form18_Load(object sender, EventArgs e)
        {
            LoadRegions();
            CustomizeDataGridView();
        }

        private void LoadRegions()
        {
            using (var context = new ElectionEntities())
            {
                var regions = context.Region
                    .Select(r => new
                    {
                        RegionID = r.ID,
                        RegionName = r.RegionName
                    })
                    .ToList();

                dataGridView1.DataSource = regions;
            }
        }
        private void CustomizeDataGridView()
        {
            if (dataGridView1.Columns.Contains("RegionID"))
                dataGridView1.Columns["RegionID"].HeaderText = "Region ID";

            if (dataGridView1.Columns.Contains("RegionName"))
                dataGridView1.Columns["RegionName"].HeaderText = "Region Name";

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AdminPage form6 = new AdminPage();

            form6.Show();

            this.Hide();
        }

        private void Form18_Resize(object sender, EventArgs e)
        {
            int addX = button3.Location.X;
            int addY = button3.Location.Y;


            button2.Location = new Point(addX + button3.Width + 10, addY);
        }
    }
}
