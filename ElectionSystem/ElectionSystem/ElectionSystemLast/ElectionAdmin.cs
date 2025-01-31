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
    public partial class ElectionAdmin : Form
    {
        public ElectionAdmin()
        {
            InitializeComponent();
        }

        private void Form16_Load(object sender, EventArgs e)
        {
            LoadElections();
            CustomizeDataGridView();
        }

        private void LoadElections()
        {
            using (var context = new ElectionEntities())
            {
                var elections = context.Election
                    .Select(e => new
                    {
                        ElectionID = e.ID,
                        ElectionType = e.ElectionType,
                        ElectionDate = e.ElectionDate,
                        ParticipationRate = e.ParticipationRate
                    })
                    .ToList();

                dataGridView1.DataSource = elections;
            }
        }
        private void CustomizeDataGridView()
        {
                dataGridView1.Columns["ElectionID"].HeaderText = "Election ID";

                dataGridView1.Columns["ElectionType"].HeaderText = "Type";

                dataGridView1.Columns["ElectionDate"].HeaderText = "Date";

            if (dataGridView1.Columns.Contains("ParticipationRate"))
                dataGridView1.Columns["ParticipationRate"].HeaderText = "Participation %";

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || dateTimePicker1.Value == null)
            {
                MessageBox.Show("Please enter a valid Election Type and Date.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string electionType = textBox1.Text.Trim();
            DateTime electionDate = dateTimePicker1.Value;

            using (var context = new ElectionEntities())
            {
                var newElection = new Election
                {
                    ElectionType = electionType,
                    ElectionDate = electionDate,
                    ParticipationRate = null 
                };

                context.Election.Add(newElection);
                context.SaveChanges();

                MessageBox.Show("Election added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadElections();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an election to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int selectedElectionID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ElectionID"].Value);

            if (selectedElectionID == 1)
            {
                MessageBox.Show("Election with ID 1 cannot be deleted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var context = new ElectionEntities())
            {
                var election = context.Election.FirstOrDefault(x => x.ID == selectedElectionID);

                if (election != null)
                {
                    var confirmResult = MessageBox.Show(
                        "Deleting this election will also remove all related records. Are you sure you want to proceed?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            using (var transaction = context.Database.BeginTransaction())
                            {
                                context.Election.Remove(election);

                                context.SaveChanges();
                                transaction.Commit();

                                MessageBox.Show("Election and all related data deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadElections();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"An error occurred while deleting: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an election to edit.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int selectedElectionID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ElectionID"].Value);

            if (selectedElectionID == 1)
            {
                MessageBox.Show("Election with ID 1 cannot be edited.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var context = new ElectionEntities())
            {
                var election = context.Election.FirstOrDefault(x => x.ID == selectedElectionID);

                if (election != null)
                {
                    election.ElectionType = textBox1.Text.Trim();
                    election.ElectionDate = dateTimePicker1.Value;

                    context.SaveChanges();
                    MessageBox.Show("Election updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadElections();
                }
                else
                {
                    MessageBox.Show("Election not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            AdminPage form6 = new AdminPage();

            form6.Show();

            this.Hide();
        }

        private void Form16_Resize(object sender, EventArgs e)
        {
            int addX = button3.Location.X;
            int addY = button3.Location.Y;


            button2.Location = new Point(addX + button3.Width + 10, addY);
        }
    }
}
