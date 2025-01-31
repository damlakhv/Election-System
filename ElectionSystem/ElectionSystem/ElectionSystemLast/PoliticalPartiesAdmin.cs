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
    public partial class PoliticalPartiesAdmin : Form
    {
        public PoliticalPartiesAdmin()
        {
            InitializeComponent();
        }

        private void Form12_Load(object sender, EventArgs e)
        {
            LoadPoliticalParties();
            CustomizeDataGridView();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void LoadPoliticalParties()
        {
            using (var context = new ElectionEntities())
            {
                var parties = context.PoliticalParty
                    .Select(p => new
                    {
                        PartyID = p.ID,
                        PartyName = p.PartyName,
                        FoundationDate = p.FoundationDate
                    })
                    .ToList();

                dataGridView1.DataSource = parties;
            }
        }

        private void CustomizeDataGridView()
        {
            if (dataGridView1.Columns.Contains("PartyID"))
                dataGridView1.Columns["PartyID"].HeaderText = "Party ID";

            if (dataGridView1.Columns.Contains("PartyName"))
                dataGridView1.Columns["PartyName"].HeaderText = "Party Name";

            if (dataGridView1.Columns.Contains("FoundationDate"))
                dataGridView1.Columns["FoundationDate"].HeaderText = "Foundation Date";

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }



        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Please enter valid Party ID and Party Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string partyID = textBox1.Text.Trim();
            string partyName = textBox2.Text.Trim();
            DateTime foundationDate = dateTimePicker1.Value;

            using (var context = new ElectionEntities())
            {
                var existingParty = context.PoliticalParty.FirstOrDefault(p => p.ID == partyID);
                if (existingParty != null)
                {
                    MessageBox.Show("A party with this ID already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var newParty = new PoliticalParty
                {
                    ID = partyID,
                    PartyName = partyName,
                    FoundationDate = foundationDate
                };

                context.PoliticalParty.Add(newParty);
                context.SaveChanges();

                MessageBox.Show("Political Party added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadPoliticalParties();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a party to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedPartyID = dataGridView1.SelectedRows[0].Cells["PartyID"].Value.ToString();

            using (var context = new ElectionEntities())
            {
                var party = context.PoliticalParty.FirstOrDefault(p => p.ID == selectedPartyID);
                if (party != null)
                {
                    var confirmResult = MessageBox.Show(
                        $"Deleting the party \"{party.PartyName}\" will also delete:\n" +
                        "- All candidates belonging to this party\n" +
                        "- All votes cast for this party's candidates\n" +
                        "- All associated records in PartyVotes and Results tables\n\n" +
                        "This action is irreversible. Do you want to proceed?",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirmResult == DialogResult.Yes)
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            try
                            {
                                // Silinecek Candidates
                                var candidatesToDelete = context.Candidate.Where(c => c.PartyID == selectedPartyID).ToList();

                                foreach (var candidate in candidatesToDelete)
                                {
                                    // Silinecek Votes
                                    var votesToDelete = context.Vote.Where(v => v.CandidateID == candidate.ID).ToList();
                                    context.Vote.RemoveRange(votesToDelete);

                                    // PartyVotes ve Results Güncellemesi
                                    foreach (var vote in votesToDelete)
                                    {
                                        // PartyVotes güncelle veya sil
                                        var partyVote = context.PartyVotes.FirstOrDefault(pv =>
                                            pv.ElectionID == vote.ElectionID &&
                                            pv.BoxID == vote.BoxID &&
                                            pv.PartyID == selectedPartyID);

                                        if (partyVote != null)
                                        {
                                            partyVote.VoteCount -= 1;
                                            if (partyVote.VoteCount <= 0)
                                            {
                                                context.PartyVotes.Remove(partyVote);
                                            }
                                        }
                                        var result = context.Result.FirstOrDefault(r =>
                                            r.ElectionID == vote.ElectionID &&
                                            r.PartyID == selectedPartyID &&
                                            r.CityID == candidate.CityID);

                                        if (result != null)
                                        {
                                            result.VoteCount -= 1;
                                            if (result.VoteCount <= 0)
                                            {
                                                context.Result.Remove(result);
                                            }
                                        }
                                    }
                                }

                                foreach (var candidate in candidatesToDelete)
                                {
                                    var deputyMayor = context.DeputyMayor.FirstOrDefault(dm => dm.CandidateID == candidate.ID);
                                    if (deputyMayor != null)
                                    {
                                        context.DeputyMayor.Remove(deputyMayor);
                                    }
                                }

                                context.Candidate.RemoveRange(candidatesToDelete);

                                var partyVotesToDelete = context.PartyVotes.Where(pv => pv.PartyID == selectedPartyID).ToList();
                                context.PartyVotes.RemoveRange(partyVotesToDelete);

                                var resultsToDelete = context.Result.Where(r => r.PartyID == selectedPartyID).ToList();
                                context.Result.RemoveRange(resultsToDelete);

                                context.PoliticalParty.Remove(party);

                                context.SaveChanges();
                                transaction.Commit();

                                MessageBox.Show("Political Party and all associated data have been successfully deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                LoadPoliticalParties();
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
                    MessageBox.Show("Political Party not found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a party to edit.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedPartyID = dataGridView1.SelectedRows[0].Cells["PartyID"].Value.ToString();

            string partyID = textBox1.Text.Trim();
            string partyName = textBox2.Text.Trim();
            DateTime foundationDate = dateTimePicker1.Value;

            if (!partyID.Equals(selectedPartyID))
            {
                MessageBox.Show("You cannot change the Party ID.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Text = selectedPartyID;
                return;
            }

            using (var context = new ElectionEntities())
            {
                var party = context.PoliticalParty.FirstOrDefault(p => p.ID == selectedPartyID);
                if (party != null)
                {
                    party.PartyName = partyName;
                    party.FoundationDate = foundationDate;

                    context.SaveChanges();

                    MessageBox.Show("Political Party updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadPoliticalParties();
                }
                else
                {
                    MessageBox.Show("Political Party not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            AdminPage form6 = new AdminPage();

            form6.Show();

            this.Hide();
        }

        private void Form12_Resize(object sender, EventArgs e)
        {
            int addX = button3.Location.X;
            int addY = button3.Location.Y;


            button2.Location = new Point(addX + button3.Width + 10, addY);
        }
    }
}
