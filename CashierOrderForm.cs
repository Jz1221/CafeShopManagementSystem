using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;


namespace CafeShopManagementSystem
{
    public partial class CashierOrderForm : UserControl
    {
        public static int getCustID;

        SqlConnection connect = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\User\OneDrive\Documents\cafe.mdf;Integrated Security=True;Connect Timeout=30");
        public CashierOrderForm()
        {
            InitializeComponent();

            displayAvailableProducts();
            displayAllOrders();

            displayTotalPrice();
        }

        public void displayAvailableProducts()
        {
            CashierOrderFormProdData allProds = new CashierOrderFormProdData();

            List<CashierOrderFormProdData> listData = allProds.availableProductsData();

            cashierOrderForm_menuTable.DataSource = listData;
        }

        public void displayAllOrders()
        {
            CashierOrdersData allOrders = new CashierOrdersData();

            List<CashierOrdersData> listData = allOrders.ordersListData();

            cashierOrderForm_orderTable.DataSource = listData;
        }

        private float totalPrice;
        public void displayTotalPrice()
        {
            if (connect.State == ConnectionState.Closed)
            {
                try
                {
                    connect.Open();

                    // Update SQL query to multiply prod_price by qty
                    string selectData = "SELECT SUM(prod_price * qty) FROM orders WHERE customer_id = @custId";

                    using (SqlCommand cmd = new SqlCommand(selectData, connect))
                    {
                        cmd.Parameters.AddWithValue("@custId", getCustID);

                        object result = cmd.ExecuteScalar();

                        if (result == DBNull.Value || result == null)
                        {
                            totalPrice = 0;
                        }
                        else
                        {
                            totalPrice = Convert.ToSingle(result);
                        }

                        // Show total price only if more than 2 items exist
                        if (getTotalItems() > 2)
                        {
                            cashierOrderForm_orderPrice.Text = "Total: RM " + totalPrice.ToString("F2");
                        }
                        else
                        {
                            cashierOrderForm_orderPrice.Text = "Total: RM 0.00";  // Hide price if <= 2 items
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connect.Close();
                }
            }
        }

        public int getTotalItems()
        {
            int itemCount = 0;

            if (connect.State == ConnectionState.Closed)
            {
                try
                {
                    connect.Open();

                    string query = "SELECT COUNT(*) FROM orders WHERE customer_id = @custId";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@custId", getCustID);
                        itemCount = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connect.Close();
                }
            }

            return itemCount;
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private int rowIndex = 0;

        private void button4_Click(object sender, EventArgs e)
        {
            
        }
        private void cashierOrderForm_addBtn_Click(object sender, EventArgs e)
        {
            if (getCustID == 0)  // If there's no active customer, generate one
            {
                IDGenerator();
                getCustID = idGen;
            }

            if (cashierOrderForm_type.SelectedIndex == -1 || cashierOrderForm_productID.SelectedIndex == -1
                || string.IsNullOrEmpty(cashierOrderForm_prodName.Text) || cashierOrderForm_quantity.Value == 0
                || string.IsNullOrEmpty(cashierOrderForm_price.Text))
            {
                MessageBox.Show("Please choose a product first", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (connect.State == ConnectionState.Closed)
            {
                try
                {
                    connect.Open();

                    float getPrice = 0;
                    string selectOrder = "SELECT prod_price FROM products WHERE prod_id = @prodID";

                    using (SqlCommand getOrder = new SqlCommand(selectOrder, connect))
                    {
                        getOrder.Parameters.AddWithValue("@prodID", cashierOrderForm_productID.Text.Trim());

                        using (SqlDataReader reader = getOrder.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                object rawValue = reader["prod_price"];
                                if (rawValue != DBNull.Value)
                                {
                                    getPrice = Convert.ToSingle(rawValue);
                                }
                            }
                        }
                    }

                    float totalPrice = getPrice * ((int)cashierOrderForm_quantity.Value);

                    cashierOrderForm_orderPrice.Text = totalPrice.ToString("F2"); // Format with 2 decimal places

                    // **Step 1: Check if product already exists in the order table**
                    string checkExistingOrder = "SELECT COUNT(*) FROM orders WHERE customer_id = @customerID AND prod_id = @prodID";

                    using (SqlCommand checkCmd = new SqlCommand(checkExistingOrder, connect))
                    {
                        checkCmd.Parameters.AddWithValue("@customerID", getCustID);
                        checkCmd.Parameters.AddWithValue("@prodID", cashierOrderForm_productID.Text.Trim());

                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            // **Step 2: Delete existing record before inserting new one**
                            string deleteExistingOrder = "DELETE FROM orders WHERE customer_id = @customerID AND prod_id = @prodID";

                            using (SqlCommand deleteCmd = new SqlCommand(deleteExistingOrder, connect))
                            {
                                deleteCmd.Parameters.AddWithValue("@customerID", getCustID);
                                deleteCmd.Parameters.AddWithValue("@prodID", cashierOrderForm_productID.Text.Trim());
                                deleteCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    // **Step 3: Insert new order**
                    string insertOrder = "INSERT INTO orders (customer_id, prod_id, prod_name, prod_type, qty, prod_price, order_date) " +
                                         "VALUES(@customerID, @prodID, @prodName, @prodType, @qty, @prodPrice, @orderDate)";

                    DateTime today = DateTime.Today;

                    using (SqlCommand cmd = new SqlCommand(insertOrder, connect))
                    {
                        cmd.Parameters.AddWithValue("@customerID", getCustID);
                        cmd.Parameters.AddWithValue("@prodID", cashierOrderForm_productID.Text.Trim());
                        cmd.Parameters.AddWithValue("@prodName", cashierOrderForm_prodName.Text);
                        cmd.Parameters.AddWithValue("@prodType", cashierOrderForm_type.Text.Trim());
                        cmd.Parameters.AddWithValue("@qty", cashierOrderForm_quantity.Value);
                        cmd.Parameters.AddWithValue("@prodPrice", totalPrice);
                        cmd.Parameters.AddWithValue("@orderDate", today);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Order added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh total price after adding an order
                    displayTotalPrice();
                    displayAllOrders();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Connection failed: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connect.Close();
                }
            }
        }


        private int idGen = 0;
        public void IDGenerator()
        {
            using (SqlConnection connect = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\User\OneDrive\Documents\cafe.mdf;Integrated Security=True;Connect Timeout=30"))
            {
                connect.Open();
                string selectID = "SELECT MAX(customer_id) FROM customers";

                using (SqlCommand cmd = new SqlCommand(selectID, connect))
                {
                    object result = cmd.ExecuteScalar();

                    if (result != DBNull.Value)
                    {
                        int temp = Convert.ToInt32(result);

                        if (temp == 0)
                        {
                            idGen = 1;
                        }
                        else
                        {
                            idGen = temp + 1;
                        }
                    }
                    else
                    {
                        idGen = 1;
                    }
                    getCustID = idGen;
                }
            }
        }

        private void cashierOrderForm_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            cashierOrderForm_productID.SelectedIndex = -1;
            cashierOrderForm_productID.Items.Clear();
            cashierOrderForm_prodName.Text = "";
            cashierOrderForm_price.Text = "";

            string selectedValue = cashierOrderForm_type.SelectedItem as string;

            if (selectedValue != null)
            {

                try
                {

                    using (SqlConnection connect = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\User\OneDrive\Documents\cafe.mdf;Integrated Security=True;Connect Timeout=30"))
                    {
                        connect.Open();
                        string selectData = $"SELECT * FROM products WHERE prod_type = '{selectedValue}' AND prod_status = @status AND date_delete IS NULL";

                        using (SqlCommand cmd = new SqlCommand(selectData, connect))
                        {
                            cmd.Parameters.AddWithValue("@status", "Available");

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string value = reader["prod_id"].ToString();

                                    cashierOrderForm_productID.Items.Add(value);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Message: " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private void cashierOrderForm_productID_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedValue = cashierOrderForm_productID.SelectedItem as string;

            if (selectedValue != null)
            {
                try
                {
                    using (SqlConnection connect = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\User\OneDrive\Documents\cafe.mdf;Integrated Security=True;Connect Timeout=30"))
                    {
                        connect.Open();
                        string selectData = $"SELECT * FROM products WHERE prod_id = '{selectedValue}' AND prod_status = @status AND date_delete IS NULL";

                        using (SqlCommand cmd = new SqlCommand(selectData, connect))
                        {
                            cmd.Parameters.AddWithValue("@status", "Available");

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string prodName = reader["prod_name"].ToString();
                                    string prodPrice = reader["prod_price"].ToString();

                                    cashierOrderForm_prodName.Text = prodName;
                                    cashierOrderForm_price.Text = prodPrice;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void cashierOrderForm_amount_TextChanged(object sender, EventArgs e)
        {

        }

        private void cashierOrderForm_amount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    float getAmount = Convert.ToSingle(cashierOrderForm_amount.Text);

                    float getChange = (getAmount - totalPrice);

                    if (getChange <= -1)
                    {
                        cashierOrderForm_amount.Text = "";
                        cashierOrderForm_change.Text = "";
                    }
                    else
                    {
                        cashierOrderForm_change.Text = getChange.ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Invalid", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cashierOrderForm_amount.Text = "";
                    cashierOrderForm_change.Text = "";
                }
            }
        }

        private void cashierOrderForm_payBtn_Click(object sender, EventArgs e)
        {
            if (cashierOrderForm_amount.Text == "" || cashierOrderForm_orderTable.Rows.Count <= 0)
            {
                MessageBox.Show("Something went wrong. Please check the order details.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Parse Amount Received
            if (!float.TryParse(cashierOrderForm_amount.Text, out float amountReceived))
            {
                MessageBox.Show("Invalid amount entered.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Parse Total Price
            if (!float.TryParse(cashierOrderForm_orderPrice.Text.Replace("RM ", ""), out float totalPrice))
            {
                MessageBox.Show("Invalid total price.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Calculate Change
            float change = amountReceived - totalPrice;

            if (change < 0)
            {
                MessageBox.Show("Insufficient amount received!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Display Change in UI
            cashierOrderForm_change.Text = "" + change.ToString("");

            // Confirm Payment
            if (MessageBox.Show("Are you sure you want to proceed with payment?", "Confirmation Message",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (connect.State == ConnectionState.Closed)
                {
                    try
                    {
                        connect.Open();

                        IDGenerator();  // Generates new customer ID
                        displayTotalPrice();  // Updates total price display

                        // Insert into customers table
                        string insertData = "INSERT INTO customers (customer_id, total_price, amount, change, date) " +
                                            "VALUES(@custID, @totalprice, @amount, @change, @date)";

                        DateTime today = DateTime.Today;

                        using (SqlCommand cmd = new SqlCommand(insertData, connect))
                        {
                            cmd.Parameters.AddWithValue("@custID", idGen);
                            cmd.Parameters.AddWithValue("@totalprice", totalPrice);
                            cmd.Parameters.AddWithValue("@amount", amountReceived); // Use parsed amount
                            cmd.Parameters.AddWithValue("@change", change); // Use calculated change
                            cmd.Parameters.AddWithValue("@date", today);

                            cmd.ExecuteNonQuery();
                        }

                        // Remove orders from the database
                        DeleteOrdersByCustomerID(idGen);

                        // Clear UI table
                        cashierOrderForm_orderTable.DataSource = null;
                        cashierOrderForm_orderTable.Rows.Clear();

                        // Reset input fields
                        cashierOrderForm_amount.Text = "";
                        cashierOrderForm_change.Text = "";
                        cashierOrderForm_orderPrice.Text = "";

                        MessageBox.Show("Payment successful! Ready for next order.", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Connection failed: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        connect.Close();
                    }
                }
            }
        }

        private void DeleteOrdersByCustomerID(int custID)
        {
            using (SqlConnection connect = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\User\OneDrive\Documents\cafe.mdf;Integrated Security=True;Connect Timeout=30"))
            {
                string query = "DELETE FROM orders WHERE customer_id = @customerID";
                SqlCommand cmd = new SqlCommand(query, connect);
                cmd.Parameters.AddWithValue("@customerID", custID);
                connect.Open();
                cmd.ExecuteNonQuery();
                connect.Close();
            }
        }

        private void cashierOrderForm_clearBtn_Click(object sender, EventArgs e)
        {
            // Confirm before clearing
            if (MessageBox.Show("Are you sure you want to clear the order?", "Confirmation Message",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Clear UI input fields
                cashierOrderForm_amount.Text = "";
                cashierOrderForm_change.Text = "";
                cashierOrderForm_orderPrice.Text = "";

                // Clear the order table
                cashierOrderForm_orderTable.DataSource = null;
                cashierOrderForm_orderTable.Rows.Clear();

                MessageBox.Show("Order cleared successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void cashierOrderForm_removeBtn_Click(object sender, EventArgs e)
        {
            if (cashierOrderForm_orderTable.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an item to remove.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ask for confirmation before removing the item
            if (MessageBox.Show("Are you sure you want to remove the selected item?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    // Get the selected row
                    DataGridViewRow selectedRow = cashierOrderForm_orderTable.SelectedRows[0];
                    string productID = selectedRow.Cells["prod_id"].Value.ToString(); // Get the Product ID

                    // Remove from database (if needed)
                    if (connect.State == ConnectionState.Closed)
                        connect.Open();

                    string deleteQuery = "DELETE FROM orders WHERE prod_id = @prodID AND customer_id = @custID";
                    using (SqlCommand cmd = new SqlCommand(deleteQuery, connect))
                    {
                        cmd.Parameters.AddWithValue("@prodID", productID);
                        cmd.Parameters.AddWithValue("@custID", getCustID);
                        cmd.ExecuteNonQuery();
                    }

                    // Remove from DataGridView (UI)
                    cashierOrderForm_orderTable.Rows.Remove(selectedRow);

                    // Refresh total price after removal
                    displayTotalPrice();

                    MessageBox.Show("Item removed successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error removing item: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connect.Close();
                }
            }
        }

        private void cashierOrderForm_orderPrice_Click(object sender, EventArgs e)
        {
            displayTotalPrice();
        }
    }
}
