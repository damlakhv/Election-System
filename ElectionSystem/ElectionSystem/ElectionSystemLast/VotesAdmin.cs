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
    public partial class VotesAdmin : Form
    {
        public VotesAdmin()
        {
            InitializeComponent();
        }

        private void Form15_Load(object sender, EventArgs e)
        {
            CustomizeDataGridView();
            LoadVotes();
        }

        private void LoadVotes()
        {
            using (var context = new ElectionEntities())
            {
                var votes = context.Vote
                    .Select(v => new
                    {
                        VoteID = v.ID,
                        ElectionID = v.ElectionID,
                        VoterID = v.VoterID,
                        CandidateName = context.Citizen
                            .Where(c => c.ID == context.Candidate
                                .Where(candidate => candidate.ID == v.CandidateID)
                                .Select(candidate => candidate.EligibleToVoteID)
                                .FirstOrDefault())
                            .Select(citizen => citizen.FirstName + " " + citizen.LastName)
                            .FirstOrDefault(),
                        BoxID = v.BoxID
                    })
                    .ToList();

                dataGridView1.DataSource = votes;
            }
        }
        private void CustomizeDataGridView()
        {
            if (dataGridView1.Columns.Contains("VoteID"))
                dataGridView1.Columns["VoteID"].HeaderText = "Vote ID";

            if (dataGridView1.Columns.Contains("ElectionID"))
                dataGridView1.Columns["ElectionID"].HeaderText = "Election ID";

            if (dataGridView1.Columns.Contains("VoterID"))
                dataGridView1.Columns["VoterID"].HeaderText = "Voter ID";

            if (dataGridView1.Columns.Contains("CandidateName"))
                dataGridView1.Columns["CandidateName"].HeaderText = "Candidate Name";

            if (dataGridView1.Columns.Contains("BoxID"))
                dataGridView1.Columns["BoxID"].HeaderText = "Ballot Box ID";

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a vote to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int selectedVoteID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["VoteID"].Value);

            using (var context = new ElectionEntities())
            {
                var vote = context.Vote.FirstOrDefault(v => v.ID == selectedVoteID);

                if (vote == null)
                {
                    MessageBox.Show("Vote not found in the database!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    using (var transaction = context.Database.BeginTransaction())
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
                        }

                        if (candidate != null)
                        {
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

                        var voter = context.Voter.FirstOrDefault(v => v.ID == vote.VoterID);
                        if (voter != null)
                        {
                            voter.HasVoted = false;
                            voter.VoteTimestamp = null;
                        }

                        context.SaveChanges();
                        transaction.Commit();

                        MessageBox.Show("Vote deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadVotes();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the vote: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AdminPage form6 = new AdminPage();

            form6.Show();

            this.Hide();
        }
    }
}
