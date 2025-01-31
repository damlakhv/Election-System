using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Reflection;
namespace ElectionSystem
{
    public partial class CandidateDetails : Form
    {

        private string voterID;
        ElectionEntities ElectionEntities = new ElectionEntities();
        public CandidateDetails(string id)
        {
            InitializeComponent();
            this.voterID = id;
        }
        private void LoadCandidatesByCity(string voterID)
        {
            using (var context = new ElectionEntities())
            {
                var candidates = context.Database.SqlQuery<GetCandidatesWithDeputyByCity_Result>(
                    "GetCandidatesWithDeputyByCity @VoterID",
                    new System.Data.SqlClient.SqlParameter("@VoterID", voterID)
                ).ToList();
                dataGridView1.DataSource =candidates; //to connect the data

                if (dataGridView1.Columns.Contains("CandidateID"))
                    dataGridView1.Columns["CandidateID"].Visible = false;

                if (dataGridView1.Columns.Contains("DeputyMayorID"))
                    dataGridView1.Columns["DeputyMayorID"].Visible = false;
            }
        }

        private void CustomizeDataGridView()
        {
            if (dataGridView1.Columns.Contains("CandidateName"))
            dataGridView1.Columns["CandidateName"].HeaderText = "Candidate Name";
            if (dataGridView1.Columns.Contains("PartyName"))
              dataGridView1.Columns["PartyName"].HeaderText = "Political Party";
            if (dataGridView1.Columns.Contains("DeputyMayorName"))
            dataGridView1.Columns["DeputyMayorName"].HeaderText = "Deputy Mayor";
            
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form4_Load(object sender, EventArgs e)
        {
            CustomizeDataGridView();
            LoadCandidatesByCity(voterID);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            VotingPage form5 = new VotingPage(voterID);
            form5.Show();
            this.Hide();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            VoterPage form3 = new VoterPage(voterID);
            form3.Show();
            this.Hide();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
