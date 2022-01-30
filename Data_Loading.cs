using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Stock_Data_Loading
{
    class Program
    {
        static void Main(string[] args)
        {
            string connetionString = "Server = localhost; Database = Stocks; Integrated Security = True;";
            string sql = "SELECT Ticket from Portfolio;";
            string sql2 = "BEGIN IF NOT EXISTS (SELECT * FROM [Stocks].[dbo].[Dividends] WHERE [Ticket] = @Ticket AND [Ex-dividend date] = @ExDividendDate) BEGIN INSERT INTO Dividends([Ticket], [Amount], [Ex-dividend date]) VALUES(@Ticket, @Amount * (SELECT [Quantity] FROM [Stocks].[dbo].[Portfolio] WHERE [Ticket] = @Ticket), @ExDividendDate) END END;";
            List<string> listStocks = new List<string>();
            string resList = "";
            
            using (SqlConnection conn = new SqlConnection(connetionString))
            {
                
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    listStocks.Add(reader.GetString(0));
                }
                conn.Close();
            }

            for (int i = 0; i < listStocks.Count; i++)
            {
                resList += GetStockData(listStocks[i]);
            }

            List<string> result = resList.Substring(0, resList.Length - 1).Split(' ').ToList();

            using (SqlConnection conn = new SqlConnection(connetionString))
            {
                SqlCommand cmd = new SqlCommand(sql2, conn);
                conn.Open();
                   
                for (int i = 0; i < result.Count; i=i+3)
                {
                cmd.Parameters.AddWithValue("@Ticket", result[i].ToString());
                cmd.Parameters.AddWithValue("@ExDividendDate", Convert.ToDateTime(result[i + 1]));
                cmd.Parameters.AddWithValue("@Amount", Convert.ToDecimal(result[i + 2]));

                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

            }
                conn.Close();
            }
        }
        private static string GetStockData(string ticket)
        {
            string s = "";

            using (WebClient client = new WebClient())
            {
                s = client.DownloadString("https://query2.finance.yahoo.com/v10/finance/quoteSummary/" + ticket + "?modules=assetProfile,balanceSheetHistory,balanceSheetHistoryQuarterly,calendarEvents,cashflowStatementHistory,cashflowStatementHistoryQuarterly,defaultKeyStatistics,earnings,earningsHistory,earningsTrend,financialData,fundOwnership,incomeStatementHistory,incomeStatementHistoryQuarterly,indexTrend,industryTrend,insiderHolders,insiderTransactions,institutionOwnership,majorDirectHolders,majorHoldersBreakdown,netSharePurchaseActivity,price,quoteType,recommendationTrend,secFilings,sectorTrend,summaryDetail,summaryProfile,symbol,upgradeDowngradeHistory,fundProfile,topHoldings,fundPerformance");
            }

            dynamic data = JObject.Parse(s);

                        DateTime lastDivDate = data.quoteSummary.result[0].defaultKeyStatistics.lastDividendDate.fmt;
            decimal lastDivValue = data.quoteSummary.result[0].defaultKeyStatistics.lastDividendValue.raw;

            return ticket + " " + lastDivDate.ToShortDateString() + " " + lastDivValue.ToString()+ " ";
        }
    }
}
