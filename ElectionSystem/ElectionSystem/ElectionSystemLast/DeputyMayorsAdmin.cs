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
    public partial class DeputyMayorsAdmin : Form
    {
        public DeputyMayorsAdmin()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
        string.IsNullOrWhiteSpace(textBox2.Text) ||
        string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Please enter valid Candidate Name, Party Name, and Deputy Mayor ID.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string candidateName = textBox3.Text.Trim();
            string partyName = textBox2.Text.Trim();
            string deputyMayorID = textBox1.Text.Trim();

            using (var context = new ElectionEntities())
            {
                var candidateCitizen = context.Citizen
                    .Where(c => (c.FirstName + " " + c.LastName).Equals(candidateName))
                    .FirstOrDefault();

                if (candidateCitizen == null)
                {
                    MessageBox.Show("No Candidate found with the provided name. Please check the name.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var eligibleCandidate = context.EligibleToVote
                    .FirstOrDefault(x => x.CitizenID == candidateCitizen.ID);

                if (eligibleCandidate == null)
                {
                    MessageBox.Show("The provided Candidate is not eligible to vote. Cannot add Deputy Mayor.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var politicalParty = context.PoliticalParty
                    .FirstOrDefault(p => p.PartyName.Equals(partyName));

                if (politicalParty == null)
                {
                    MessageBox.Show("No Political Party found with the provided name. Please check the party name.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var existingDeputy = context.DeputyMayor
                    .FirstOrDefault(d => d.ID == deputyMayorID);

                if (existingDeputy != null)
                {
                    MessageBox.Show("A Deputy Mayor with this ID already exists.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var newDeputy = new DeputyMayor
                {
                    ID = deputyMayorID,
                    EligibleToVoteID = eligibleCandidate.ID,
                    PartyID = politicalParty.ID,
                    CandidateID = eligibleCandidate.ID
                };

                context.DeputyMayor.Add(newDeputy);
                context.SaveChanges();

                MessageBox.Show("Deputy Mayor added successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadDeputyMayors();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a Deputy Mayor to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox2.Text) ||
                string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Please enter valid Candidate Name, Party Name, and Deputy Mayor ID.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newCandidateName = textBox3.Text.Trim();
            string newPartyName = textBox2.Text.Trim();
            string selectedDeputyMayorID = dataGridView1.SelectedRows[0].Cells["deputyMayorID"].Value.ToString();

            using (var context = new ElectionEntities())
            {
                var deputyMayor = context.DeputyMayor.FirstOrDefault(d => d.ID == selectedDeputyMayorID);
                if (deputyMayor == null)
                {
                    MessageBox.Show("Deputy Mayor not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var candidateCitizen = context.Citizen
                    .Where(c => (c.FirstName + " " + c.LastName).Equals(newCandidateName))
                    .FirstOrDefault();

                if (candidateCitizen == null)
                {
                    MessageBox.Show("No Candidate found with the provided name. Please check the name.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var eligibleCandidate = context.EligibleToVote
                    .FirstOrDefault(x => x.CitizenID == candidateCitizen.ID);

                if (eligibleCandidate == null)
                {
                    MessageBox.Show("The provided Candidate is not eligible to vote. Cannot edit Deputy Mayor.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var politicalParty = context.PoliticalParty
                    .FirstOrDefault(p => p.PartyName.Equals(newPartyName));

                if (politicalParty == null)
                {
                    MessageBox.Show("No Political Party found with the provided name. Please check the party name.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                deputyMayor.PartyID = politicalParty.ID;
                deputyMayor.CandidateID = eligibleCandidate.ID;
                deputyMayor.EligibleToVoteID = eligibleCandidate.ID;

                context.SaveChanges();

                MessageBox.Show("Deputy Mayor updated successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadDeputyMayors();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a deputy mayor to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedDeputyMayorID = dataGridView1.SelectedRows[0].Cells["DeputyMayorID"].Value.ToString();

            using (var context = new ElectionEntities())
            {
                var deputyMayor = context.DeputyMayor.FirstOrDefault(dm => dm.ID == selectedDeputyMayorID);

                if (deputyMayor != null)
                {
                    var confirmResult = MessageBox.Show($"Are you sure you want to delete the deputy mayor with ID: {selectedDeputyMayorID}?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (confirmResult == DialogResult.Yes)
                    {
                        context.DeputyMayor.Remove(deputyMayor);
                        context.SaveChanges();

                        MessageBox.Show("Deputy Mayor deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadDeputyMayors();
                    }
                }
                else
                {
                    MessageBox.Show("Deputy Mayor not found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void LoadDeputyMayors()
        {
            using (var context = new ElectionEntities())
            {
                var deputyMayors = context.DeputyMayor
                    .Select(dm => new
                    {
                        DeputyMayorID = dm.ID,
                        PartyName = dm.PoliticalParty.PartyName, 
                        CandidateName = context.Citizen
                            .Where(c => c.ID == context.EligibleToVote
                                .Where(e => e.ID == dm.CandidateID)
                                .Select(e => e.CitizenID)
                                .FirstOrDefault())
                            .Select(c => c.FirstName + " " + c.LastName)
                            .FirstOrDefault() 
                    })
                    .ToList();

                dataGridView1.DataSource = deputyMayors;
            }
        }

        private void CustomizeDataGridView()
        {
            if (dataGridView1.Columns.Contains("DeputyMayorID"))
                dataGridView1.Columns["DeputyMayorID"].HeaderText = "Deputy Mayor ID";

            if (dataGridView1.Columns.Contains("PartyName"))
                dataGridView1.Columns["PartyName"].HeaderText = "Political Party";

            if (dataGridView1.Columns.Contains("CandidateName"))
                dataGridView1.Columns["CandidateName"].HeaderText = "Candidate Name";

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void Form13_Load(object sender, EventArgs e)
        {
            CustomizeDataGridView();
            LoadDeputyMayors();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AdminPage form6 = new AdminPage();

            form6.Show();

            this.Hide();
        }

        private void Form13_Resize(object sender, EventArgs e)
        {
            int addX = button3.Location.X;
            int addY = button3.Location.Y;


            button2.Location = new Point(addX + button3.Width + 10, addY);
        }
    }
}
