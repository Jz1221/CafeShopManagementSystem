using System;
using System.Windows.Forms;

namespace CafeShopManagementSystem
{
    public partial class CashierMainForm : Form
    {
        public CashierMainForm()
        {
            InitializeComponent();
        }

        private void close_Click_Click(object sender, EventArgs e)
        {
            DialogResult check = MessageBox.Show("Are you sure you want to exit?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (check == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void logout_btn_Click(object sender, EventArgs e)
        {
            DialogResult check = MessageBox.Show("Are you sure you want to log out?",
                "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (check == DialogResult.Yes)
            {
                Form1 loginForm = new Form1();
                loginForm.Show();

                this.Hide();
            }

        }

        private void order_btn_Click(object sender, EventArgs e)
        {
            adminDashboardForm2.Visible = false;
            cashierOrderForm1.Visible = true;
        }

        private void username_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string rules = "Cashier Rules:\n\n" +
                   "1. Always greet customers politely.\n" +
                   "2. Handle cash and card transactions carefully.\n" +
                   "3. No fighting or 1 by 1 with cashiers.\n" +
                   "4. Keep the cash register secure at all times.\n" +
                   "5. Report any suspicious activities immediately, no rasuah.\n" +
                   "6. Maintain a clean and organized workspace.\n\n" +
                   "Please follow these rules strictly.";

            MessageBox.Show(rules, "Cashier Rules", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dashboard_btn_Click(object sender, EventArgs e)
        {
            adminDashboardForm2.Visible = true;
            cashierOrderForm1.Visible = false;
        }

        private void adminDashboardForm2_Load(object sender, EventArgs e)
        {

        }
    }
}
