using System.Linq;
using System.Windows;
//using TraSuaApp.Services;
using TraSuaApp.Views;
using MySql.Data.MySqlClient;

namespace TraSuaApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static void ShowLoginAndCloseAllOtherWindows()
        {
            var loginWindow = new MainWindow();
            loginWindow.Show();

            var others = Application.Current.Windows
                .Cast<Window>()
                .Where(w => w != loginWindow)
                .ToList();

            foreach (Window w in others)
            {
                w.Close();
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // 1. Khai báo dây kết nối (Nhớ đổi 123456 thành mật khẩu MySQL của bạn)
            string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Đổi câu lệnh SQL để lấy thêm store_id
                    string query = "SELECT role, store_id FROM Accounts WHERE username = @user AND password = @pass";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // Lấy dữ liệu từ TextBox (nhớ thay đúng tên txtTaiKhoan và txtPassword của bạn)
                        cmd.Parameters.AddWithValue("@user", txtUsername.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", txtPassword.Password);

                        // Dùng Reader để đọc được nhiều cột
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // Nếu tìm thấy tài khoản (đọc được dữ liệu)
                            {
                                string role = reader["role"].ToString();
                                string storeId = reader["store_id"].ToString();
                                string username = txtUsername.Text.Trim();

                                // Phân luồng đường đi
                                if (role == "ADMIN" || role == "MANAGER")
                                {
                                    // Mở màn hình quản trị và truyền Quyền + ID Chi Nhánh sang
                                    Views.AdminWindow adminScreen = new Views.AdminWindow(role, storeId);
                                    adminScreen.Show();
                                    this.Close();
                                }
                                else if (role == "STAFF")
                                {
                                    // Nhân viên thì chỉ được vào bán hàng
                                    Views.PosWindow posScreen = new Views.PosWindow(username, storeId);
                                    posScreen.Show();
                                    this.Close();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Sai tài khoản hoặc mật khẩu. Vui lòng thử lại!", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể kết nối tới cơ sở dữ liệu. Lỗi: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}