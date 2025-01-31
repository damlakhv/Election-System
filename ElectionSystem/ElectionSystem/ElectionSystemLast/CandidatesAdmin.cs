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
    public partial class CandidatesAdmin : Form
    {
        public CandidatesAdmin()
        {
            InitializeComponent();
        }

        private void LoadCandidates()
        {
            using (var context = new ElectionEntities())
            {
                var candidates = context.Candidate
                    .Select(c => new
                    {
                        CandidateID = c.ID,
                        ElectionID = c.ElectionID,
                        PartyName = c.PoliticalParty.PartyName, 
                        CityName = c.City.CityName              
                    })
                    .ToList();

                dataGridView1.DataSource = candidates;
            }
        }

        private void CustomizeDataGridView()
        {
            
                dataGridView1.Columns["CandidateID"].HeaderText = "Candidate ID";
              dataGridView1.Columns["ElectionID"].HeaderText = "Election ID";
                dataGridView1.Columns["PartyName"].HeaderText = "Political Party";
                dataGridView1.Columns["CityName"].HeaderText = "City Name";

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }


        private void Form11_Load(object sender, EventArgs e)
        {
            LoadCandidates();
            CustomizeDataGridView();
        }

        private void button2_Click(object sender, EventArgs e) // DELETE BUTTON
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a candidate to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedCandidateID = dataGridView1.SelectedRows[0].Cells["CandidateID"].Value.ToString();

            using (var context = new ElectionEntities())
            {
                var candidate = context.Candidate.FirstOrDefault(c => c.ID == selectedCandidateID);

                if (candidate != null)
                {
                    var confirmResult = MessageBox.Show(
                        $"Are you sure you want to delete the candidate with ID: {selectedCandidateID}? " +
                        "This will also delete all associated votes and update related records.",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (confirmResult == DialogResult.Yes)
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            try
                            {
                               
                                var deputyMayor = context.DeputyMayor.FirstOrDefault(dm => dm.CandidateID == candidate.ID);
                                if (deputyMayor != null)
                                {
                                    context.DeputyMayor.Remove(deputyMayor);
                                }

                                
                                var votesToDelete = context.Vote.Where(v => v.CandidateID == candidate.ID).ToList();
                                foreach (var vote in votesToDelete)
                                {
                                    
                                    var partyVotes = context.PartyVotes.FirstOrDefault(pv =>
                                        pv.ElectionID == vote.ElectionID &&
                                        pv.BoxID == vote.BoxID &&
                                        pv.PartyID == candidate.PartyID);

                                    if (partyVotes != null)
                                    {
                                        partyVotes.VoteCount--;
                                        if (partyVotes.VoteCount <= 0)
                                        {
                                            context.PartyVotes.Remove(partyVotes);
                                        }
                                    }

                                    var result = context.Result.FirstOrDefault(r =>
                                        r.ElectionID == vote.ElectionID &&
                                        r.PartyID == candidate.PartyID &&
                                        r.CityID == candidate.CityID);

                                    if (result != null)
                                    {
                                        result.VoteCount--;
                                        if (result.VoteCount <= 0)
                                        {
                                            context.Result.Remove(result);
                                        }
                                    }

                                    
                                    context.Vote.Remove(vote);
                                }

                                
                                context.Candidate.Remove(candidate);

                                context.SaveChanges();
                                transaction.Commit();

                                MessageBox.Show("Candidate and all associated data deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                LoadCandidates();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                MessageBox.Show($"An error occurred while deleting: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Candidate not found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        

        private void button3_Click(object sender, EventArgs e) // ADD BUTTON
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) ||
         string.IsNullOrWhiteSpace(textBox3.Text) || string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("Please enter valid Candidate ID, Election ID, Party Name, and City Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string candidateID = textBox1.Text.Trim();
            int electionID = int.Parse(textBox2.Text.Trim());
            string partyName = textBox3.Text.Trim();
            string cityName = textBox4.Text.Trim();



            using (var context = new ElectionEntities())
            {
                var existingCitizen = context.Citizen.FirstOrDefault(c => c.ID == candidateID);

                if (existingCitizen == null)
                {
                    MessageBox.Show("No Citizen exists with this ID. Please add the Citizen first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var existingEligible = context.EligibleToVote.FirstOrDefault(x => x.ID == candidateID);

                if (existingEligible == null)
                {
                    MessageBox.Show("This ID is not listed as eligible to vote. Cannot add to Candidate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var existingCandidate = context.Candidate.FirstOrDefault(c => c.ID == candidateID);

                if (existingCandidate != null)
                {
                    MessageBox.Show("This ID is already registered as a candidate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var party = context.PoliticalParty.FirstOrDefault(p => p.PartyName == partyName);

                if (party == null)
                {
                    MessageBox.Show("No Political Party found with the provided name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var city = context.City.FirstOrDefault(c => c.CityName == cityName);

                if (city == null)
                {
                    MessageBox.Show("No City found with the provided name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var newCandidate = new Candidate
                {
                    ID = candidateID, 
                    EligibleToVoteID = existingEligible.ID, 
                    ElectionID = electionID,
                    PartyID = party.ID, 
                    CityID = city.ID 
                };

                context.Candidate.Add(newCandidate);
                context.SaveChanges();

                MessageBox.Show("Candidate added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadCandidates();
            }
        }

        private void button1_Click(object sender, EventArgs e) // UPDATE BUTTON
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a candidate to edit.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedCandidateID = dataGridView1.SelectedRows[0].Cells["CandidateID"].Value.ToString();

            string candidateID = textBox1.Text.Trim();
            int electionID = int.Parse(textBox2.Text.Trim());
            string partyName = textBox3.Text.Trim();
            string cityName = textBox4.Text.Trim();

            if (!selectedCandidateID.Equals(candidateID))
            {
                MessageBox.Show("You cannot change the Candidate ID.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Text = selectedCandidateID; // Convert id to the original.
                return;
            }

            using (var context = new ElectionEntities())
            {
                var candidate = context.Candidate.FirstOrDefault(c => c.ID == candidateID);
                if (candidate == null)
                {
                    MessageBox.Show("Candidate not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var party = context.PoliticalParty.FirstOrDefault(p => p.PartyName == partyName);
                if (party == null)
                {
                    MessageBox.Show("Invalid Party Name. Please check and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var city = context.City.FirstOrDefault(c => c.CityName == cityName);
                if (city == null)
                {
                    MessageBox.Show("Invalid City Name. Please check and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                candidate.ElectionID = electionID;
                candidate.PartyID = party.ID; 
                candidate.CityID = city.ID;  

                context.SaveChanges();

                MessageBox.Show("Candidate updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadCandidates();
            }
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

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            AdminPage form6 = new AdminPage();

            form6.Show();

            this.Hide();
        }

        private void Form11_Resize(object sender, EventArgs e)
        {
            int addX = button3.Location.X;
            int addY = button3.Location.Y;


            button2.Location = new Point(addX + button3.Width + 10, addY);
        }
    }
}
