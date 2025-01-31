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
    public partial class VotersAdmin : Form
    {
        public VotersAdmin()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void LoadVoters()
        {
            using (var context = new ElectionEntities())
            {
                var voter = context.Voter
                    .Select(v => new
                    {
                        ID = v.ID,
                        
                        VoterPassword = v.VoterPassword,
                        HasVoted = v.HasVoted,
                        VoteTimestamp = v.VoteTimestamp,
                        BoxID = v.BoxID
                    })
                    .ToList();

                dataGridView1.DataSource = voter;
            }
        }
        private void CustomizeDataGridView()
        {
            if (dataGridView1.Columns.Contains("ID"))
                dataGridView1.Columns["ID"].HeaderText = "Voter ID";

            if (dataGridView1.Columns.Contains("VoterPassword"))
                dataGridView1.Columns["VoterPassword"].HeaderText = "Password";

            if (dataGridView1.Columns.Contains("HasVoted"))
                dataGridView1.Columns["HasVoted"].HeaderText = "Has Voted";

            if (dataGridView1.Columns.Contains("VoteTimestamp"))
                dataGridView1.Columns["VoteTimestamp"].HeaderText = "Vote Time";

            if (dataGridView1.Columns.Contains("BoxID"))
                dataGridView1.Columns["BoxID"].HeaderText = "Ballot Box ID";
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) || string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Please enter a valid ID, Password, and Box ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newID = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();
            int boxID = int.Parse(textBox3.Text.Trim());

            

            using (var context = new ElectionEntities())
            {
                var existingCitizen = context.Citizen.FirstOrDefault(c => c.ID == newID);

                if (existingCitizen == null)
                {
                    MessageBox.Show("No Citizen exists with this ID. Please add the Citizen first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                
                var existingEligible = context.EligibleToVote.FirstOrDefault(x => x.ID == newID);

                if (existingEligible == null)
                {
                    MessageBox.Show("This ID is not listed as eligible to vote. Cannot add to Voter.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var existingVoter = context.Voter.FirstOrDefault(v => v.ID == newID);

               
                var newVoter = new Voter
                {
                    ID = existingEligible.ID, 
                    EligibleToVoteID = existingEligible.ID, 
                    VoterPassword = password,
                    HasVoted = false, 
                    VoteTimestamp = null, 
                    BoxID = boxID 
                };

                context.Voter.Add(newVoter);
                context.SaveChanges();

                MessageBox.Show("Voter added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadVoters();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string enteredID = textBox1.Text.Trim(); 
            string selectedID = dataGridView1.SelectedRows[0].Cells["ID"].Value.ToString(); 
            int boxID = int.Parse(textBox3.Text.Trim());

            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a voter to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!enteredID.Equals(selectedID))
            {
                MessageBox.Show("You cannot change the Voter ID.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Text = selectedID; // ID'yi eski haline döndür
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox2.Text) || string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Please fill in all required fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var context = new ElectionEntities())
            {
                string newPassword = textBox2.Text.Trim();
                var existingPassword = context.Voter.FirstOrDefault(v => v.VoterPassword == newPassword && v.ID != selectedID);
                
                var voter = context.Voter.FirstOrDefault(v => v.ID == selectedID);
                if (voter != null)
                {
                    voter.VoterPassword = newPassword;
                    voter.BoxID = boxID;

                    context.SaveChanges();

                    MessageBox.Show("Voter updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadVoters();
                }
                
            }
        }

        private void button2_Click(object sender, EventArgs e)

        {   

            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a voter to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedVoterID = dataGridView1.SelectedRows[0].Cells["ID"].Value.ToString();

            using (var context = new ElectionEntities())
            {
                var voter = context.Voter.FirstOrDefault(v => v.ID == selectedVoterID);

                if (voter != null)
                {
                    var confirmResult = MessageBox.Show($"Are you sure you want to delete the voter with ID: {selectedVoterID}?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (confirmResult == DialogResult.Yes)
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            try
                            {
                                var votes = context.Vote.Where(v => v.VoterID == voter.ID).ToList();
                                foreach (var vote in votes)
                                {
                                    context.Vote.Remove(vote);

                                    var candidate = context.Candidate.FirstOrDefault(c => c.ID == vote.CandidateID);
                                    if (candidate != null)
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
                                    }
                                }
                                context.Voter.Remove(voter);

                                context.SaveChanges();
                                transaction.Commit();

                                MessageBox.Show("Voter and all related data deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                LoadVoters(); 
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
                    MessageBox.Show("Voter not found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

        private void Form10_Load(object sender, EventArgs e)
        {
            CustomizeDataGridView();
            LoadVoters();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AdminPage form6 = new AdminPage();

            form6.Show();

            this.Hide();
        }

        private void Form10_Resize(object sender, EventArgs e)
        {
            int addX = button3.Location.X;
            int addY = button3.Location.Y;


            button2.Location = new Point(addX + button3.Width + 10, addY);
        }
    }
}
