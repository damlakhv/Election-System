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
    public partial class NotEligibleToVoteAdmin : Form
    {
        public NotEligibleToVoteAdmin()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text)||string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Please enter a valid ID and Reason.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newID =textBox1.Text.Trim();
            string reason =textBox2.Text.Trim();

            using (var context=new ElectionEntities())
            {
                var existingCitizen=context.Citizen.FirstOrDefault(c=>c.ID == newID);

                if (existingCitizen== null)
                {
                    MessageBox.Show("No Citizen exists with this ID. Please add the Citizen first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var existingEligible =context.EligibleToVote.FirstOrDefault(x =>x.ID ==newID);

                if (existingEligible!= null)
                {
                    MessageBox.Show("This ID is already eligible to vote. Cannot add to NotEligibleToVote.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var existingNotEligible= context.Not_eligible_to_vote.FirstOrDefault(x => x.ID == newID);

                if (existingNotEligible!= null)
                {
                    MessageBox.Show("This ID is already listed as not eligible to vote.","Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var newNotEligible =new Not_eligible_to_vote
                {
                    ID=newID,
                    CitizenID =newID,
                    MainReason =reason
                };

                context.Not_eligible_to_vote.Add(newNotEligible);
                context.SaveChanges();

                MessageBox.Show("Not eligible voter added successfully.","Success",MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadNotEligibleToVote();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            if (dataGridView1.SelectedRows.Count ==0)
            {
                MessageBox.Show("Please select a record to delete.","Warning",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return;
            }

            string selectedID=dataGridView1.SelectedRows[0].Cells["ID"].Value.ToString();

            using (var context =new ElectionEntities())
            {
                var notEligibleRecord = context.Not_eligible_to_vote.FirstOrDefault(n =>n.ID ==selectedID);

                if (notEligibleRecord != null)
                {
                    var confirmResult = MessageBox.Show($"Are you sure you want to delete the record with ID: {selectedID}?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (confirmResult==DialogResult.Yes)
                    {
                        try
                        {
                            context.Not_eligible_to_vote.Remove(notEligibleRecord);
                            context.SaveChanges();

                            MessageBox.Show("Record deleted successfully.","Success",MessageBoxButtons.OK,MessageBoxIcon.Information);

                            LoadNotEligibleToVote();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"An error occurred while deleting the record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a not eligible to vote citizen to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("No record selected to save changes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string enteredID=textBox1.Text.Trim(); 
            string selectedID=dataGridView1.SelectedRows[0].Cells["ID"].Value.ToString(); 

            if (!enteredID.Equals(selectedID))
            {
                MessageBox.Show("You cannot change the ID.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Text= selectedID; 
                return;
            }

            using (var context =new ElectionEntities())
            {
                var notEligible= context.Not_eligible_to_vote.FirstOrDefault(n =>n.ID ==selectedID);
                if (notEligible != null)
                {
                    notEligible.MainReason = textBox2.Text.Trim();
                    context.SaveChanges();
                    MessageBox.Show("Record updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadNotEligibleToVote();
                }
                
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void LoadNotEligibleToVote()
        {
            using (var context = new ElectionEntities())
            {
                var notEligibleVoters = context.Not_eligible_to_vote
                    .Select(n=>new
                    {
                        ID = n.ID,
                        
                        MainReason = n.MainReason
                    })
                    .ToList();
                dataGridView1.DataSource = notEligibleVoters;
            }
        }

        private void CustomizeNotEligibleDataGridView()
        {
            dataGridView1.Columns["ID"].HeaderText="Not Eligible ID";
            dataGridView1.Columns["MainReason"].HeaderText ="Main Reason";
            dataGridView1.AutoSizeColumnsMode= DataGridViewAutoSizeColumnsMode.Fill;

        }

        private void Form9_Load(object sender, EventArgs e)
        {
            LoadNotEligibleToVote();
            CustomizeNotEligibleDataGridView();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AdminPage form6 = new AdminPage();

            form6.Show();

            this.Hide();
        }

        private void Form9_Resize(object sender, EventArgs e)
        {
            int addX = button3.Location.X;
            int addY = button3.Location.Y;


            button2.Location = new Point(addX + button3.Width + 10, addY);
        }
    }
}
