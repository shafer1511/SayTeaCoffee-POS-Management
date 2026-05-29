using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace TraSuaApp.Views
{
    public partial class ReportWindow : Window
    {
        private string _storeId; // Biến lưu chi nhánh đang báo cáo

        public ReportWindow(string storeId)
        {
            InitializeComponent();
            _storeId = storeId;
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                MessageBox.Show("Vui lòng nhập mô tả chi tiết sự cố!");
                return;
            }

            string type = cbReportType.Text;
            string message = txtMessage.Text;
            string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO Support_Reports (store_id, report_type, message, created_at) VALUES (@store, @type, @msg, @date)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@store", _storeId);
                        cmd.Parameters.AddWithValue("@type", type);
                        cmd.Parameters.AddWithValue("@msg", message);
                        cmd.Parameters.AddWithValue("@date", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Báo cáo đã được gửi tới Trung tâm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi gửi báo cáo: " + ex.Message, "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}