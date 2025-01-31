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
    public partial class CitizensAdmin : Form
    {
        public CitizensAdmin()
        {
            InitializeComponent();
        }
        
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void LoadCitizens()
        {
            using (var context = new ElectionEntities())
            {
                
                var citizens = context.Citizen
                    .Select(c => new
                    {
                        ID = c.ID,
                        FirstName = c.FirstName,
                        SecondName = c.SecondName, 
                        LastName = c.LastName,
                        Birthday = c.Birthday
                    })
                    .ToList();

                dataGridView1.DataSource = citizens;
            }
        }
        private void CitizenListPageDataGridView()
        {
            dataGridView1.Columns["ID"].HeaderText = "Citizen ID";
            dataGridView1.Columns["FirstName"].HeaderText = "First Name";
            dataGridView1.Columns["SecondName"].HeaderText = "Middle Name"; 
            dataGridView1.Columns["LastName"].HeaderText = "Last Name";
            dataGridView1.Columns["Birthday"].HeaderText = "Birth Date";

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            
        }

       

        private void button2_Click(object sender, EventArgs e) // DELETE BUTTON
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a citizen to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedCitizenID = dataGridView1.SelectedRows[0].Cells["ID"].Value.ToString();
            using (var context = new ElectionEntities())
            {
                var citizen = context.Citizen.FirstOrDefault(c => c.ID == selectedCitizenID);

                if (citizen != null)
                {
                    var confirmResult = MessageBox.Show($"Are you sure you want to delete the citizen with ID: {selectedCitizenID}?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (confirmResult == DialogResult.Yes)
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            try
                            {
                                var eligibleToVote = context.EligibleToVote.Where(x => x.CitizenID == selectedCitizenID).ToList();

                                foreach (var eligible in eligibleToVote)
                                {
                                    var voter = context.Voter.FirstOrDefault(v => v.EligibleToVoteID== eligible.ID);
                                    if (voter != null)
                                    {
                                        var votes = context.Vote.Where(v => v.VoterID==voter.ID).ToList();
                                        foreach (var vote in votes)
                                        {
                                            context.Vote.Remove(vote);

                                            var candidate = context.Candidate.FirstOrDefault(c => c.ID== vote.CandidateID);
                                            if (candidate != null)
                                            {
                                                var partyVotes = context.PartyVotes.FirstOrDefault(pv=>pv.ElectionID == vote.ElectionID &&pv.BoxID == vote.BoxID &&pv.PartyID == candidate.PartyID);

                                                if (partyVotes != null)
                                                {
                                                    partyVotes.VoteCount--;
                                                    if (partyVotes.VoteCount <= 0)
                                                    {
                                                        context.PartyVotes.Remove(partyVotes);
                                                    }
                                                }
                                                var result= context.Result.FirstOrDefault(r =>
                                                    r.ElectionID == vote.ElectionID &&
                                                    r.PartyID==candidate.PartyID &&
                                                    r.CityID ==candidate.CityID);

                                                if (result!=null)
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
                                    var candidates= context.Candidate.Where(c =>c.EligibleToVoteID== eligible.ID).ToList();
                                    context.Candidate.RemoveRange(candidates);
                                    var deputyMayors =context.DeputyMayor.Where(d => d.EligibleToVoteID== eligible.ID).ToList();
                                    context.DeputyMayor.RemoveRange(deputyMayors);
                                    context.EligibleToVote.Remove(eligible);
                                }

                                var notEligibleToVote = context.Not_eligible_to_vote.Where(n =>n.CitizenID==selectedCitizenID).ToList();
                                context.Not_eligible_to_vote.RemoveRange(notEligibleToVote);

                                context.Citizen.Remove(citizen);
                                context.SaveChanges();
                                transaction.Commit();

                                MessageBox.Show("Citizen and all related data deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                LoadCitizens();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                MessageBox.Show($"An error occurred while deleting:{ex.Message}","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Citizen not found in the database.","Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private void button1_Click(object sender, EventArgs e) // EDIT BUTTON
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a citizen to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("No citizen selected to save changes.","Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string enteredID = textBox1.Text.Trim(); 
            string selectedID = dataGridView1.SelectedRows[0].Cells["ID"].Value.ToString(); 

            if (!enteredID.Equals(selectedID))
            {
                MessageBox.Show("You cannot change the Citizen ID.","Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Text = selectedID; 
                return;
            }

            using (var context= new ElectionEntities())
            {
                var citizen = context.Citizen.FirstOrDefault(c =>c.ID==selectedID);
                if (citizen !=null)
                {
                    citizen.FirstName= textBox2.Text.Trim();
                    citizen.SecondName= string.IsNullOrWhiteSpace(textBox3.Text)? null:textBox3.Text.Trim();
                    citizen.LastName= textBox4.Text.Trim();
                    citizen.Birthday =dateTimePicker1.Value;

                    context.SaveChanges();
                    MessageBox.Show("Citizen updated successfully.","Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadCitizens();
                }
                else
                {
                    MessageBox.Show("Citizen not found.","Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e) // ADD BUTTON
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text)|| string.IsNullOrWhiteSpace(textBox2.Text)|| string.IsNullOrWhiteSpace(textBox4.Text))   
            {
                MessageBox.Show("Please fill in all required fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var context = new ElectionEntities())
            {
                
                string newCitizenID = textBox1.Text.Trim();
                var existingCitizen = context.Citizen.FirstOrDefault(c => c.ID == newCitizenID);
                if (existingCitizen != null)
                {
                    MessageBox.Show("A citizen with this ID already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                
                var newCitizen = new Citizen
                {
                    ID = newCitizenID,
                    FirstName = textBox2.Text.Trim(),
                    SecondName = string.IsNullOrWhiteSpace(textBox3.Text) ? null : textBox3.Text.Trim(),
                    LastName = textBox4.Text.Trim(),
                    Birthday = dateTimePicker1.Value
                };
                context.Citizen.Add(newCitizen);
                context.SaveChanges();
                MessageBox.Show("Citizen added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadCitizens();              
            }
        }

        private void Form7_Load(object sender, EventArgs e)
        {
            LoadCitizens();
            CitizenListPageDataGridView();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AdminPage form6 = new AdminPage();

            form6.Show();

            this.Hide();
        }

        private void Form7_Resize(object sender, EventArgs e)
        {
            
            int addX = button3.Location.X;
            int addY = button3.Location.Y;

            
            button2.Location = new Point(addX + button3.Width + 10, addY);
        }
    }
}
