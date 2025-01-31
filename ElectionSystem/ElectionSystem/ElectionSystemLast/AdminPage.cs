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
    public partial class AdminPage : Form
    {
        public AdminPage()
        {
            InitializeComponent();
           

        }


        private void button1_Click(object sender, EventArgs e)
        {
            CitizensAdmin form7 = new CitizensAdmin();

            form7.Show();

            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            EligibleToVoteAdmin form8 = new EligibleToVoteAdmin();

            form8.Show();

            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            NotEligibleToVoteAdmin form9 = new NotEligibleToVoteAdmin();

            form9.Show();

            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            VotersAdmin form10 = new VotersAdmin();

            form10.Show();

            this.Hide();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            CandidatesAdmin form11 = new CandidatesAdmin();

            form11.Show();

            this.Hide();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            PoliticalPartiesAdmin form12 = new PoliticalPartiesAdmin();

            form12.Show();

            this.Hide();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            BallotBoxAdmin form14 = new BallotBoxAdmin();

            form14.Show();

            this.Hide();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            VotesAdmin form15 = new VotesAdmin();

            form15.Show();

            this.Hide();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            DeputyMayorsAdmin form13 = new DeputyMayorsAdmin();

            form13.Show();

            this.Hide();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ElectionAdmin form16 = new ElectionAdmin();

            form16.Show();

            this.Hide();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            CityAdmin form17 = new CityAdmin();

            form17.Show();

            this.Hide();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            RegionAdmin form18 = new RegionAdmin();

            form18.Show();

            this.Hide();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            HomePage form1 = new HomePage();

            form1.Show();

            this.Hide();
        }

        private void Form6_Load(object sender, EventArgs e)
        {

        }

        private void panel3_Resize(object sender, EventArgs e)
        {
           
        }

        private void Form6_Resize(object sender, EventArgs e)
        {
            
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
