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
    public partial class VotingPage : Form
    {

        private string voterID;
        
        public VotingPage(string id)
        {
            InitializeComponent();
            this.voterID = id;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var selectedItem = listView1.SelectedItems[0];

                string selectedCandidateID = selectedItem.Tag.ToString();
 
            }
        }
        private void LoadCandidatesByCity(string voterID)
        {
            using (var context = new ElectionEntities())
            {
                //PROCEDURE KULLANDIK
                var candidates = context.Database.SqlQuery<GetCandidatesByCity_Result>(
                    "GetCandidatesByCity @VoterID",
                    new System.Data.SqlClient.SqlParameter("@VoterID", voterID)
                ).ToList();
                listView1.Items.Clear();
                listView1.Font = new Font("Segoe UI", 14, FontStyle.Regular);

                foreach (var candidate in candidates)
                {
                    var item = new ListViewItem(candidate.CandidateName); 
                    item.SubItems.Add(candidate.PartyName);              
                    item.Tag = candidate.CandidateID;
                    listView1.Items.Add(item);
                    
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a candidate before voting!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedItem = listView1.SelectedItems[0];
            string selectedCandidateID = selectedItem.Tag.ToString();

            using (var context = new ElectionEntities())
            {
                var voter = context.Voter.FirstOrDefault(v => v.ID == voterID); // Check if the selected first ID is equal to voterID
                if (voter.HasVoted)
                {
                    MessageBox.Show("You have already voted! You cannot vote again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int boxID = (int)voter.BoxID;

                try
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        var vote = new Vote
                        {
                            ElectionID = 1,
                            VoterID = voterID,
                            CandidateID = selectedCandidateID,
                            BoxID = boxID
                        };
                        context.Vote.Add(vote);

                        voter.HasVoted = true;
                        voter.VoteTimestamp = DateTime.Now;

                        var candidate = context.Candidate.FirstOrDefault(c => c.ID == selectedCandidateID);
                        if (candidate == null)
                        {
                            throw new Exception("Candidate not found!");
                        }

                        var partyVotes = context.PartyVotes.FirstOrDefault(pv => pv.ElectionID == 1 && pv.BoxID == boxID && pv.PartyID == candidate.PartyID);
                        if (partyVotes != null)
                        {
                            partyVotes.VoteCount++;
                        }
                        else
                        {
                            throw new Exception("PartyVotes record not found for this BoxID and PartyID.");
                        }

                        var result = context.Result.FirstOrDefault(r => r.ElectionID == 1 && r.PartyID == candidate.PartyID && r.CityID == candidate.CityID);
                        if (result != null)
                        {
                            result.VoteCount++;
                        }
                        else
                        {
                            throw new Exception("Result record not found for this CityID and PartyID.");
                        }

                        // Update ParticipationRate
                        var totalVoters = context.Voter.Count();
                        var votersVoted = context.Voter.Count(v => v.HasVoted);
                        var participationRate = totalVoters > 0 ? (decimal)votersVoted / totalVoters * 100 : 0;

                        var election = context.Election.FirstOrDefault(x => x.ID == 1);
                        if (election != null)
                        {
                            election.ParticipationRate = Math.Round(participationRate, 2);
                        }

                        context.SaveChanges();
                        transaction.Commit();

                        MessageBox.Show("Your vote has been successfully cast!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            LoadCandidatesByCity(voterID);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CandidateDetails form4 = new CandidateDetails(voterID);
            form4.Show();
            this.Hide();
        }
    }
}
