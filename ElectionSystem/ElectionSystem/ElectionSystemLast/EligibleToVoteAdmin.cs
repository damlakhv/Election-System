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
    public partial class EligibleToVoteAdmin : Form
    {
        public EligibleToVoteAdmin()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an eligible voter to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string selectedEligibleID =dataGridView1.SelectedRows[0].Cells["ID"].Value.ToString();

            using (var context=new ElectionEntities())
            {
                var eligible =context.EligibleToVote.FirstOrDefault(x =>x.ID ==selectedEligibleID);

                if (eligible !=null)
                {
                    var confirmResult = MessageBox.Show($"Are you sure you want to delete the eligible voter with ID: {selectedEligibleID}?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (confirmResult ==DialogResult.Yes)
                    {
                        using (var transaction= context.Database.BeginTransaction())
                        {
                            try
                            {
                                var voter =context.Voter.FirstOrDefault(v => v.EligibleToVoteID == eligible.ID);
                                if (voter !=null)
                                {
                                    var votes= context.Vote.Where(v => v.VoterID == voter.ID).ToList();
                                    foreach (var vote in votes)
                                    {
                                        context.Vote.Remove(vote);

                                        var candidate= context.Candidate.FirstOrDefault(c => c.ID ==vote.CandidateID);
                                        if (candidate!= null)
                                        {
                                            var partyVotes= context.PartyVotes.FirstOrDefault(pv=>pv.ElectionID==vote.ElectionID &&pv.BoxID ==vote.BoxID &&pv.PartyID== candidate.PartyID);

                                            if (partyVotes!= null)
                                            {
                                                partyVotes.VoteCount--;
                                                if (partyVotes.VoteCount<= 0)
                                                {
                                                    context.PartyVotes.Remove(partyVotes);
                                                }
                                            }

                                            var result =context.Result.FirstOrDefault(r =>
                                                r.ElectionID== vote.ElectionID &&
                                                r.PartyID== candidate.PartyID &&
                                                r.CityID == candidate.CityID);

                                            if (result!= null)
                                            {
                                                result.VoteCount--;
                                                if (result.VoteCount<= 0)
                                                {
                                                    context.Result.Remove(result);
                                                }
                                            }
                                        }
                                    }

                                    context.Voter.Remove(voter);
                                }

                                var candidates=context.Candidate.Where(c =>c.EligibleToVoteID ==eligible.ID).ToList();
                                context.Candidate.RemoveRange(candidates);
                                var deputyMayors =context.DeputyMayor.Where(d => d.EligibleToVoteID ==eligible.ID).ToList();
                                context.DeputyMayor.RemoveRange(deputyMayors);
                                context.EligibleToVote.Remove(eligible);

                                context.SaveChanges();
                                transaction.Commit();

                                MessageBox.Show("Eligible voter and all related data deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadEligibleToVote(); 
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                MessageBox.Show($"An error occurred while deleting:{ex.Message}","Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }else
                {
                    MessageBox.Show("Eligible voter not found in the database.","Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        
        

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {    MessageBox.Show("Please enter a valid ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string newID =textBox1.Text.Trim();
            using (var context =new ElectionEntities())
            {
                var existingCitizen =context.Citizen.FirstOrDefault(c => c.ID==newID);

                if (existingCitizen== null)
                {   MessageBox.Show("No Citizen exists with this ID. Please add the Citizen first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var existingEligible =context.EligibleToVote.FirstOrDefault(x =>x.ID == newID);

                if (existingEligible !=null)
                {    MessageBox.Show("This ID is already eligible to vote.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var existingNotEligible=context.Not_eligible_to_vote.FirstOrDefault(x=>x.ID ==newID);

                if (existingNotEligible!=null)
                {
                    MessageBox.Show("This ID is listed as not eligible to vote. Cannot add to EligibleToVote.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var newEligible=new EligibleToVote
                {
                   ID = newID,
                    CitizenID =newID
                };

                context.EligibleToVote.Add(newEligible);
                context.SaveChanges();

                MessageBox.Show("Eligible voter added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadEligibleToVote();
            }
        }

        private void LoadEligibleToVote()
        {
            using (var context = new ElectionEntities())
            { 
                var eligible= context.EligibleToVote
                    .Select(e=> new
                    {
                        ID =e.ID,
                    })
                    .ToList();
                dataGridView1.DataSource = eligible;
            }
        }

        private void CustomizeDataGridView()
        {
            dataGridView1.Columns["ID"].HeaderText="Eligible ID";
           
            dataGridView1.AutoSizeColumnsMode= DataGridViewAutoSizeColumnsMode.Fill;
            
            
        }

        private void Form8_Load(object sender, EventArgs e)
        {
            LoadEligibleToVote();
            CustomizeDataGridView();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            AdminPage form6=new AdminPage();

            form6.Show();

            this.Hide();
        }

        private void Form8_Resize(object sender, EventArgs e)
        {
            int addX = button3.Location.X;
            int addY = button3.Location.Y;


            button2.Location = new Point(addX + button3.Width + 10, addY);
        }
    }
}
