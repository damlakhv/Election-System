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
    public partial class BallotBoxAdmin : Form
    {
        public BallotBoxAdmin()
        {
            InitializeComponent();
        }

        private void Form14_Load(object sender, EventArgs e)
        {
            LoadBallotBoxes();
            CustomizeDataGridView();
        }

        private void LoadBallotBoxes()
        {
            using (var context = new ElectionEntities())
            {
                var ballotBoxes = context.Ballot_box // join 'le city'nin nameini aldık, oysa ballot box'ta sadece city id var.
                    .Join(context.City, 
                          ballotBox => ballotBox.CityID, 
                          city => city.ID, 
                          (ballotBox, city) => new
                          {
                              BallotBoxID = ballotBox.ID,
                              VoterCount = ballotBox.VoterCount,
                              VoteCount = ballotBox.VoteCount,
                              CityName = city.CityName 
                          })
                    .ToList();

                dataGridView1.DataSource = ballotBoxes; 
            }
        }
        private void CustomizeDataGridView()
        {
                dataGridView1.Columns["BallotBoxID"].HeaderText = "Ballot Box ID";
            if (dataGridView1.Columns.Contains("VoterCount"))
                dataGridView1.Columns["VoterCount"].HeaderText = "Voter Count";
            if (dataGridView1.Columns.Contains("VoteCount"))
                dataGridView1.Columns["VoteCount"].HeaderText = "Vote Count";
                dataGridView1.Columns["CityName"].HeaderText = "City Name";

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please enter a valid City Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string cityName = textBox1.Text.Trim();

            using (var context = new ElectionEntities())
            {
                var city = context.City.FirstOrDefault(c => c.CityName == cityName);

                if (city == null)
                {
                    MessageBox.Show("City not found. Please enter a valid City Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var newBallotBox = new Ballot_box
                {
                    VoterCount = 0, 
                    VoteCount = 0,  
                    CityID = city.ID 
                };

                context.Ballot_box.Add(newBallotBox);
                context.SaveChanges();

                MessageBox.Show("Ballot Box added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadBallotBoxes();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a Ballot Box to edit.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please enter a valid City Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int selectedBallotBoxID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["BallotBoxID"].Value);
            string cityName = textBox1.Text.Trim();

            using (var context = new ElectionEntities())
            {
                var city = context.City.FirstOrDefault(c => c.CityName == cityName);

                if (city == null)
                {
                    MessageBox.Show("No city found with the given name. Please check the city name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var ballotBox = context.Ballot_box.FirstOrDefault(b => b.ID == selectedBallotBoxID);

                if (ballotBox == null)
                {
                    MessageBox.Show("The selected Ballot Box could not be found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ballotBox.CityID = city.ID;

                context.SaveChanges();

                MessageBox.Show("Ballot Box's city has been successfully updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadBallotBoxes();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a Ballot Box to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int selectedBallotBoxID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["BallotBoxID"].Value);

            using (var context = new ElectionEntities())
            {
                var ballotBox = context.Ballot_box.FirstOrDefault(b => b.ID == selectedBallotBoxID);

                if (ballotBox != null)
                {
                    var confirmResult = MessageBox.Show(
                        "Deleting this Ballot Box will also remove all associated votes. This action will result in data loss and is not recommended until the election has concluded. Are you sure you want to proceed?",
                        "Confirm Deletion",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirmResult == DialogResult.Yes)
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            try
                            {
                                
                                var votes = context.Vote.Where(v => v.BoxID == ballotBox.ID).ToList();

                                foreach (var vote in votes)
                                {
                                    
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

                                    var voter = context.Voter.FirstOrDefault(v => v.ID == vote.VoterID);
                                    if (voter != null)
                                    {
                                        voter.HasVoted = false;
                                        voter.VoteTimestamp = null;
                                    }

                                    context.Vote.Remove(vote);
                                }

                                context.Ballot_box.Remove(ballotBox);

                                context.SaveChanges();
                                transaction.Commit();

                                MessageBox.Show("Ballot Box and all associated votes have been successfully deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                LoadBallotBoxes();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                MessageBox.Show($"An error occurred while deleting: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

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

        private void Form14_Resize(object sender, EventArgs e)
        {
            int addX = button3.Location.X;
            int addY = button3.Location.Y;


            button2.Location = new Point(addX + button3.Width + 10, addY);
        }
    }
}
