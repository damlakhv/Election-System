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
    public partial class VoterPage : Form
    {
        private string voterID;
        public VoterPage(string id)
        {
            InitializeComponent();
            voterID = id;
        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void LoadVoterInformation(string id)
        {
            
            using (ElectionEntities context = new ElectionEntities())
            {
                //PROCEDURE IS USED
                var result = context.Database.SqlQuery<VoterInfo>(
                    "GetVoterInformation @VoterID",
                    new System.Data.SqlClient.SqlParameter("@VoterID", id)
                ).FirstOrDefault();
                
                if (result != null)
                {
                    label4.Text = $"Name: {result.FullName}\n" +
                                  $"Birthday: {result.Birthday:yyyy-MM-dd}\n" +
                                  $"BallotBoxID: {result.BallotBoxID}\n" +
                                  $"City: {result.CityName}";
                }
                else
                { label4.Text = "No voter found for the given ID.";
                }
            }
        }

        

        private void button2_Click(object sender, EventArgs e)
        {
            CandidateDetails form4 = new CandidateDetails(voterID);
            form4.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            VotingPage form5 = new VotingPage(voterID);
            form5.Show();
            this.Hide();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            LoadVoterInformation(voterID);
        }
       

        private void Form3_Resize_1(object sender, EventArgs e)
        {
           
            panel3.Width = this.ClientSize.Width / 2;
            int buttonWidth = this.ClientSize.Width / 4; 
            int buttonHeight = this.ClientSize.Height / 14; 

            int rightHalfStart = this.ClientSize.Width / 2;

            button2.Width = buttonWidth;
            button2.Height = buttonHeight;
            button2.Location = new Point(
                rightHalfStart + (this.ClientSize.Width / 4) - (button2.Width / 2), 
                (this.ClientSize.Height / 2) - buttonHeight - 10 
            );

            // İkinci butonu ortala
            button3.Width = buttonWidth;
            button3.Height = buttonHeight;
            button3.Location = new Point(
                rightHalfStart + (this.ClientSize.Width / 4) - (button3.Width / 2), 
                (this.ClientSize.Height / 2) + 10 
            );

          
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button13_Click(object sender, EventArgs e)
        {
            VoterLoginPage form2 = new VoterLoginPage();

            form2.Show();

            this.Hide();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
