using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Net;

namespace Stock_Tracker_v0._1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            StockListRequest();
        }

        List<string> stockList = new List<string>();

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbStockSymb.Text) || string.IsNullOrEmpty(tbQty1.Text))
            {
                MessageBox.Show("Fill the fields first!");
            }
            else
            {
                if (GetStockData(tbStockSymb.Text) == 0)
                {
                    MessageBox.Show("Ticket doesn't exist");
                }
                else
                {                   

                    string connetionString = "Server = localhost; Database = Stocks; Integrated Security = True;";
                    string sql = "INSERT INTO [Purchase history] ([Ticket], [Quantity], [Date]) VALUES(@Ticket, @Qty, @Date)";

                    using (SqlConnection conn = new SqlConnection(connetionString))
                    {
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@Ticket", tbStockSymb.Text.ToUpper());
                        cmd.Parameters.AddWithValue("@Qty", tbQty1.Text);
                        cmd.Parameters.AddWithValue("@Date", dtpBuy.Value.ToShortDateString());
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }

                    UpdatePortfolio(tbStockSymb.Text.ToUpper(), Convert.ToInt32(tbQty1.Text));
                    tbStockSymb.Text = "";
                    tbQty1.Text = "";
                    StockListRequest();

                }
            }
        }

        private void btnSell_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(cbStockSymb.Text) || nupQty.Value == 0)
            {
                MessageBox.Show("Fill the fields first!");
            }
            else
            {
                string connetionString = "Server = localhost; Database = Stocks; Integrated Security = True;";
                string sql = "INSERT INTO [Purchase history] ([Ticket], [Quantity], [Date]) VALUES(@Ticket, @Qty, @Date)";

                using (SqlConnection conn = new SqlConnection(connetionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Ticket", cbStockSymb.Text);
                    cmd.Parameters.AddWithValue("@Qty", -1*nupQty.Value);
                    cmd.Parameters.AddWithValue("@Date", dtpSell.Value.ToShortDateString());
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                UpdatePortfolio(cbStockSymb.Text.ToUpper(), nupQty.Value);
                cbStockSymb.Text = "";
                nupQty.Value = 0;
                StockListRequest();
            }
        }

        public void StockListRequest()
        {
            string connetionString = "Server = localhost; Database = Stocks; Integrated Security = True;";
            string sql = "SELECT Ticket from Portfolio;";
            stockList.Clear();
            cbStockSymb.Items.Clear();

            using (SqlConnection conn = new SqlConnection(connetionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    stockList.Add(reader.GetString(0));
                    cbStockSymb.Items.Add(reader.GetString(0));
                }
                conn.Close();
            }
        }

        private static int GetStockData(string ticket)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadString("https://query2.finance.yahoo.com/v10/finance/quoteSummary/" + ticket + "?modules=assetProfile");
                }
                return 1;
            }
            catch (Exception)
            {

                return 0;
            }
        }

        public void UpdatePortfolio(string ticket, decimal qty)
        {
            string sql = "INSERT INTO Portfolio ([Ticket], [Quantity]) VALUES (@Ticket, @Qty)";

            for (int i = 0; i < stockList.Count; i++)
            {
                if (ticket == stockList[i])
                {
                    sql = "UPDATE Portfolio SET [Ticket] = @Ticket, [Quantity] = (SELECT SUM([Quantity]) FROM[Purchase history] WHERE[Ticket] = @Ticket) WHERE [Ticket] = @Ticket";
                }
            }

            string connetionString = "Server = localhost; Database = Stocks; Integrated Security = True;";

            using (SqlConnection conn = new SqlConnection(connetionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Ticket", ticket);
                cmd.Parameters.AddWithValue("@Qty", qty);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
    }
}
