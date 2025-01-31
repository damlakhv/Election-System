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
using ElectionSystem;
namespace ElectionSystem
{
    public partial class VoterLoginPage : Form
    {
        ElectionEntities ElectionEntities = new ElectionEntities();
        public VoterLoginPage()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.PasswordChar = '*';
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string enteredID = textBox1.Text.Trim(); 
            if (string.IsNullOrEmpty(enteredID))
            {
                MessageBox.Show("Enter an ID number.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var context = new ElectionEntities())
            {
               
                var citizen = context.Citizen.FirstOrDefault(c => c.ID == enteredID);
                if (citizen == null)
                {
                    MessageBox.Show("There is no citizen with this ID number.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var notEligible = context.Not_eligible_to_vote.FirstOrDefault(n => n.ID == enteredID);
                if (notEligible != null)
                {
                    MessageBox.Show($"This citizen cannot vote! Reason: {notEligible.MainReason}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var eligible = context.EligibleToVote.FirstOrDefault(x => x.ID == enteredID);
                if (eligible != null)
                {
                    VoterPage form3 = new VoterPage(enteredID); 
                    form3.Show();
                    this.Hide();
                    
                }
                else
                {
                    MessageBox.Show("This person is either not ciziten or cannot vote.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            HomePage form1 = new HomePage();

            form1.Show();

            this.Hide();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            CenterGroupBox();
        }
        private void CenterGroupBox()
        {
            groupBox1.Location = new Point(
                (this.ClientSize.Width - groupBox1.Width) / 2,
                (this.ClientSize.Height - groupBox1.Height) / 2
            );
            groupBox1.Anchor = AnchorStyles.None; 
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
