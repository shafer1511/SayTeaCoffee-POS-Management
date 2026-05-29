using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Microsoft.Win32;
using TraSuaApp.Models;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace TraSuaApp.Views
{
    public partial class AdminWindow : Window
    {
        string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";
        public string QuyenHan { get; set; }
        public string MaChiNhanh { get; set; }

        private List<AdminDetailedInvoiceRow> _adminDetailedInvoiceRows = new List<AdminDetailedInvoiceRow>();

        public AdminWindow(string role, string storeId) {
            InitializeComponent();

            QuyenHan = role;
            MaChiNhanh = storeId;

            // Chạy hàm thiết lập giao diện
            ThietLapGiaoDienTheoQuyen();

            LoadBranches();
            LoadEmployees();
            LoadBranchComboBox();
            LoadMenu();
            LoadPosBranchComboBox();
            LoadVouchers();
            LoadReports();
            if (QuyenHan == "ADMIN")
                InitAdminDetailedReportTab();

            SyncEmpWorkTimeFromShift();
            UpdateEmpAccountFieldsVisibility();
        }

        private static string GetEmpRoleSelected(ComboBox cb)
        {
            if (cb?.SelectedItem is ComboBoxItem item)
                return item.Content?.ToString()?.Trim() ?? "";
            return "";
        }

        private bool IsCashierRoleSelected() => GetEmpRoleSelected(cbEmpRole) == "Thu ngân";

        private void cbEmpRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            UpdateEmpAccountFieldsVisibility();
        }

        private void UpdateEmpAccountFieldsVisibility()
        {
            if (spEmpAccountFields == null) return;
            spEmpAccountFields.Visibility = IsCashierRoleSelected() ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SyncEmpWorkTimeFromShift()
        {
            if (cbEmpShift?.SelectedItem is not ComboBoxItem sel) return;
            string shift = sel.Content?.ToString() ?? "Ca Sáng";
            string hours = shift switch
            {
                "Ca Chiều" => "12:00 - 17:00",
                "Ca Tối" => "17:00 - 22:00",
                _ => "06:00 - 11:00"
            };
            txtEmpWorkTime.Text = hours;
            txtEmpWorkTime.Foreground = System.Windows.Media.Brushes.Black;
        }

        private void cbEmpShift_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            SyncEmpWorkTimeFromShift();
        }

        private static string NextProductIdByPrefix(MySqlConnection conn, string prefix)
        {
            int max = GetMaxProductNumericSuffix(conn, null, prefix);
            return prefix + (max + 1).ToString("D2");
        }

        private static int GetMaxProductNumericSuffix(MySqlConnection conn, MySqlTransaction? trans, string prefix)
        {
            string q = prefix == "D"
                ? "SELECT id FROM Products WHERE id LIKE 'D%' ORDER BY CAST(SUBSTRING(id, 2) AS UNSIGNED) DESC LIMIT 1"
                : "SELECT id FROM Products WHERE id LIKE 'T%' ORDER BY CAST(SUBSTRING(id, 2) AS UNSIGNED) DESC LIMIT 1";
            using MySqlCommand cmd = trans == null ? new MySqlCommand(q, conn) : new MySqlCommand(q, conn, trans);
            object? o = cmd.ExecuteScalar();
            if (o == null || o == DBNull.Value) return 0;
            string maxId = o.ToString() ?? "";
            if (maxId.Length < 2 || !int.TryParse(maxId.Substring(1), out int n)) return 0;
            return n;
        }
        // Hàm đóng mở theo từng role
        private void ThietLapGiaoDienTheoQuyen()
        {
            if (QuyenHan == "MANAGER")
            {
                // 1. Đổi tiêu đề Window để hiển thị rõ chi nhánh đang quản lý
                this.Title = $"SAY TEA COFFEE | QUẢN TRỊ CHI NHÁNH - {MaChiNhanh}";

                // 2. Ẩn các Tab không dành cho Manager 
                if (tabBranchManagement != null)   tabBranchManagement.Visibility = Visibility.Collapsed; // Giấu tab Quản lý chi nhánh
                if (tabGlobalMenu != null) tabGlobalMenu.Visibility = Visibility.Collapsed; // Giấu tab menu toàn hệ thống

                // Giấu thêm tab Hệ thống 
                tabSystem.Visibility = Visibility.Collapsed;  
                if (tabEmployeeManagement != null)
                {
                    tabEmployeeManagement.Visibility = Visibility.Visible;
                    tabEmployeeManagement.IsSelected = true; // tự động chuyển sang tab nhân viên
                }
            }
            else if (QuyenHan == "ADMIN")
            {
                this.Title = "SAY TEA COFFEE | TRUNG TÂM ĐIỀU HÀNH CHUỖI (ADMIN) - BỘ PHẬN TỔNG";
                if (tabEmployeeManagement != null)
                    tabEmployeeManagement.Visibility = Visibility.Visible; 
            }

            if (tabAdminDetailedReport != null)
                tabAdminDetailedReport.Visibility = QuyenHan == "ADMIN" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void InitAdminDetailedReportTab()
        {
            PopulateAdminReportBranchListsFromGrid();
            var today = DateTime.Today;
            dpAdminReportFrom.SelectedDate = new DateTime(today.Year, today.Month, 1);
            dpAdminReportTo.SelectedDate = today;
            _adminDetailedInvoiceRows.Clear();
        }

        private void PopulateAdminReportBranchListsFromGrid()
        {
            var list = new List<StoreItem>();
            if (dgBranches.ItemsSource is IEnumerable<StoreItem> seq)
                list.AddRange(seq);
            icAdminReportBranches.ItemsSource = list;
            cbAdminReportBranch.ItemsSource = list;
            if (list.Count > 0)
                cbAdminReportBranch.SelectedIndex = 0;
        }

        private void AdminMainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Kiểm tra xem có đúng là chuyển Tab không
            if (e.Source is not TabControl tc || tc.SelectedItem is not TabItem ti) return;

            // Nếu chuyển sang tab Báo Cáo Chi Tiết (Admin)
            if (ReferenceEquals(ti, tabAdminDetailedReport))
            {
                if (QuyenHan == "ADMIN")
                {
                    try
                    {
                        LoadBranches();
                        PopulateAdminReportBranchListsFromGrid();
                    }
                    catch { }
                }
            }

            // Nếu chuyển sang tab Hộp Thư Sự Cố 
            else if (ReferenceEquals(ti, tabReports))
            {
                LoadReports(); // Ép tải lại dữ liệu mới nhất từ MySQL ngay khi vừa mở tab
            }
        }

        private void btnAdminReportBranchCard_Click(object sender, RoutedEventArgs e)
        {
            if (QuyenHan != "ADMIN") return;
            if (sender is not Button btn || btn.Tag is not StoreItem store) return;
            if (!dpAdminReportFrom.SelectedDate.HasValue || !dpAdminReportTo.SelectedDate.HasValue)
            {
                MessageBox.Show("Vui lòng chọn đủ \"Từ ngày\" và \"Đến ngày\" ở khung Lọc báo cáo bên phải trước khi xem chi tiết chi nhánh.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var d1 = dpAdminReportFrom.SelectedDate.Value.Date;
            var d2 = dpAdminReportTo.SelectedDate.Value.Date;
            if (d1 > d2)
            {
                MessageBox.Show("Từ ngày không được lớn hơn Đến ngày.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var win = new BranchDetailReportWindow(connectionString, store.Id, store.Name, d1, d2);
            win.Owner = this;
            win.ShowDialog();
        }

        private void btnAdminReportSyncBranches_Click(object sender, RoutedEventArgs e)
        {
            if (QuyenHan != "ADMIN") return;
            LoadBranches();
            PopulateAdminReportBranchListsFromGrid();
            MessageBox.Show("Đã đồng bộ danh sách chi nhánh từ cơ sở dữ liệu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnAdminReportView_Click(object sender, RoutedEventArgs e)
        {
            if (QuyenHan != "ADMIN") return;
            if (cbAdminReportBranch.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn chi nhánh.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!dpAdminReportFrom.SelectedDate.HasValue || !dpAdminReportTo.SelectedDate.HasValue)
            {
                MessageBox.Show("Vui lòng chọn đủ khoảng ngày.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var d1 = dpAdminReportFrom.SelectedDate.Value.Date;
            var d2 = dpAdminReportTo.SelectedDate.Value.Date;
            if (d1 > d2)
            {
                MessageBox.Show("Từ ngày không được lớn hơn Đến ngày.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string storeId = cbAdminReportBranch.SelectedValue.ToString();
            var rows = new List<AdminDetailedInvoiceRow>();

            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = @"SELECT invoice_id, store_id, staff_name, created_at, total_origin, voucher_code, discount_amount, final_total
                                   FROM Invoices
                                   WHERE store_id = @sid AND DATE(created_at) BETWEEN @d1 AND @d2
                                   ORDER BY created_at DESC";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@sid", storeId);
                        cmd.Parameters.AddWithValue("@d1", d1.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@d2", d2.ToString("yyyy-MM-dd"));
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                rows.Add(new AdminDetailedInvoiceRow
                                {
                                    InvoiceId = reader["invoice_id"].ToString(),
                                    StoreId = reader["store_id"].ToString(),
                                    StaffName = reader["staff_name"] == DBNull.Value ? "" : reader["staff_name"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["created_at"]),
                                    TotalOrigin = Convert.ToInt32(reader["total_origin"]),
                                    VoucherCode = reader["voucher_code"] == DBNull.Value ? "" : reader["voucher_code"].ToString(),
                                    DiscountAmount = Convert.ToInt32(reader["discount_amount"]),
                                    FinalTotal = Convert.ToInt32(reader["final_total"])
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không tải được báo cáo: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            _adminDetailedInvoiceRows = rows;
            // 1. Lấy tên chi nhánh đang được chọn trên ComboBox để truyền qua cửa sổ mới
            string storeName = "Chi nhánh";
            if (cbAdminReportBranch.SelectedItem is StoreItem selectedStore)
            {
                storeName = selectedStore.Name;
            }

            // 2. Khởi tạo và bật cửa sổ Dashboard cực xịn của team bạn lên
            BranchSalesDashboardWindow dashboardWin = new BranchSalesDashboardWindow(
                storeId,
                storeName,
                d1, // Từ ngày (đã lấy ở đầu hàm)
                d2, // Đến ngày (đã lấy ở đầu hàm)
                _adminDetailedInvoiceRows // Truyền nguyên danh sách vừa bốc từ MySQL sang
            );

            dashboardWin.Owner = this; // Giúp cửa sổ báo cáo luôn nằm đè lên trên cửa sổ Admin
            dashboardWin.ShowDialog();
        
        }

        private void btnAdminReportExport_Click(object sender, RoutedEventArgs e)
        {
            if (QuyenHan != "ADMIN") return;
            if (_adminDetailedInvoiceRows == null || _adminDetailedInvoiceRows.Count == 0)
            {
                MessageBox.Show("Chưa có dữ liệu báo cáo. Vui lòng bấm \"XEM BÁO CÁO\" trước.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dlg = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = $"bao_cao_chi_tiet_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("invoice_id,store_id,staff_name,created_at,total_origin,voucher_code,discount_amount,final_total");
                foreach (var r in _adminDetailedInvoiceRows)
                {
                    string esc(string? x) => (x ?? "").Replace("\"", "\"\"");
                    sb.Append('"').Append(esc(r.InvoiceId)).Append("\",");
                    sb.Append('"').Append(esc(r.StoreId)).Append("\",");
                    sb.Append('"').Append(esc(r.StaffName)).Append("\",");
                    sb.Append('"').Append(r.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)).Append("\",");
                    sb.Append(r.TotalOrigin.ToString(CultureInfo.InvariantCulture)).Append(',');
                    sb.Append('"').Append(esc(r.VoucherCode)).Append("\",");
                    sb.Append(r.DiscountAmount.ToString(CultureInfo.InvariantCulture)).Append(',');
                    sb.Append(r.FinalTotal.ToString(CultureInfo.InvariantCulture));
                    sb.AppendLine();
                }
                File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));
                MessageBox.Show("Đã export file CSV thành công.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không ghi được file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBranches()
        {
            // Tạo một danh sách trống để chứa các chi nhánh lấy từ Database
            List<StoreItem> danhSachChiNhanh = new List<StoreItem>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open(); // Mở cửa database

                    // Lấy 2 cột id và name từ bảng Stores
                    string query = "SELECT id, name FROM Stores";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // Lấy kết quả đọc được
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) // Cứ mỗi lần đọc được 1 dòng
                            {
                                // Tạo một StoreItem mới và nhét vào danh sách
                                danhSachChiNhanh.Add(new StoreItem()
                                {
                                    Id = reader["id"].ToString(),
                                    Name = reader["name"].ToString()
                                });
                            }
                        }
                    }
                    dgBranches.ItemsSource = danhSachChiNhanh;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải danh sách chi nhánh: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RemovePlaceholder(object sender, RoutedEventArgs e) {
            if (sender is TextBox tb && tb.Text == tb.Tag?.ToString()) { tb.Text = ""; tb.Foreground = System.Windows.Media.Brushes.Black; }
        }
        private void AddPlaceholder(object sender, RoutedEventArgs e) {
            if (sender is TextBox tb && string.IsNullOrWhiteSpace(tb.Text)) { tb.Text = tb.Tag?.ToString(); tb.Foreground = System.Windows.Media.Brushes.Gray; }
        }
        private void LoadVouchers()
        {
            // Tạo một danh sách rỗng để chứa các Voucher
            List<VoucherItem> danhSachVoucher = new List<VoucherItem>();

            string query = "SELECT voucher_code, discount_percent, max_discount_amount, expiry_date FROM Vouchers ORDER BY expiry_date DESC";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new VoucherItem()
                                {
                                    Code = reader["voucher_code"].ToString(),
                                    DiscountPercent = Convert.ToDecimal(reader["discount_percent"]),
                                    MaxDiscount = Convert.ToInt32(reader["max_discount_amount"]),
                                    ExpiryDate = Convert.ToDateTime(reader["expiry_date"])
                                };

                                danhSachVoucher.Add(item);
                            }
                        }
                    }
                    dgVouchers.ItemsSource = danhSachVoucher;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải danh sách Voucher: " + ex.Message);
                }
            }
        }
        private void btnCreateBranch_Click(object sender, RoutedEventArgs e)
        {
            string ten = txtNewBranch.Text.Trim();
            string tk = txtStaffUser.Text.Trim();
            string mk = txtStaffPass.Text.Trim();

            if (string.IsNullOrWhiteSpace(ten) || string.IsNullOrWhiteSpace(tk) || string.IsNullOrWhiteSpace(mk))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();


                    // 1A. Kiểm tra xem Tên chi nhánh đã tồn tại chưa
                    string checkStoreQuery = "SELECT COUNT(*) FROM Stores WHERE name = @name";
                    using (MySqlCommand cmdCheckStore = new MySqlCommand(checkStoreQuery, conn))
                    {
                        cmdCheckStore.Parameters.AddWithValue("@name", ten);
                        if (Convert.ToInt32(cmdCheckStore.ExecuteScalar()) > 0)
                        {
                            MessageBox.Show("Tên chi nhánh này đã tồn tại! Vui lòng chọn tên khác.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return; // Dừng luôn, không cho chạy tiếp
                        }
                    }

                    // 1B. Kiểm tra xem Tài khoản đã tồn tại chưa
                    string checkUserQuery = "SELECT COUNT(*) FROM Accounts WHERE username = @user";
                    using (MySqlCommand cmdCheckUser = new MySqlCommand(checkUserQuery, conn))
                    {
                        cmdCheckUser.Parameters.AddWithValue("@user", tk);
                        if (Convert.ToInt32(cmdCheckUser.ExecuteScalar()) > 0)
                        {
                            MessageBox.Show("Tài khoản này đã có người sử dụng! Vui lòng chọn tên đăng nhập khác.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return; // Dừng luôn, không cho chạy tiếp
                        }
                    }

                    // BẮT ĐẦU TRANSACTION ĐỂ ĐẢM BẢO DỮ LIỆU ĐỒNG BỘ

                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 2A. Đếm và tạo ID tự động
                            string newId = "";
                            string countQuery = "SELECT COUNT(*) FROM Stores";
                            using (MySqlCommand cmdCount = new MySqlCommand(countQuery, conn, transaction))
                            {
                                int count = Convert.ToInt32(cmdCount.ExecuteScalar());
                                newId = "S" + (count + 1).ToString("D2");
                            }

                            // 2B. Lưu Chi Nhánh
                            string insertStore = "INSERT INTO Stores (id, name) VALUES (@id, @name)";
                            using (MySqlCommand cmdStore = new MySqlCommand(insertStore, conn, transaction))
                            {
                                cmdStore.Parameters.AddWithValue("@id", newId);
                                cmdStore.Parameters.AddWithValue("@name", ten);
                                cmdStore.ExecuteNonQuery();
                            }

                            // 2C. Lưu Tài Khoản
                            string insertAccount = "INSERT INTO Accounts (username, password, role, store_id) VALUES (@user, @pass, 'MANAGER', @id)";
                            using (MySqlCommand cmdAcc = new MySqlCommand(insertAccount, conn, transaction))
                            {
                                cmdAcc.Parameters.AddWithValue("@user", tk);
                                cmdAcc.Parameters.AddWithValue("@pass", mk);
                                cmdAcc.Parameters.AddWithValue("@id", newId);
                                cmdAcc.ExecuteNonQuery();
                            }

                            // 2D. NẾU MỌI THỨ TRƠN TRU -> XÁC NHẬN GHI VÀO Ổ CỨNG (COMMIT)
                            transaction.Commit();

                            MessageBox.Show("Đã mở chi nhánh " + newId + " thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                            LoadBranches();
                            LoadBranchComboBox();     // Cập nhật ComboBox bên tab Quản lý nhân viên
                            LoadPosBranchComboBox();  // Cập nhật ComboBox bên tab Vào bán hàng

                            txtNewBranch.Clear();
                            txtStaffUser.Clear();
                            txtStaffPass.Clear();
                        }
                        catch (Exception exTransaction)
                        {
                            // 2E. NẾU LỖI GIỮA CHỪNG -> QUAY XE, XÓA HẾT MỌI THAO TÁC TRONG KHỐI NÀY (ROLLBACK)
                            transaction.Rollback();
                            MessageBox.Show("Lỗi trong quá trình tạo chi nhánh, đã hủy bỏ toàn bộ thao tác để bảo vệ dữ liệu. Lỗi: " + exTransaction.Message, "Lỗi Nghiệp Vụ", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể kết nối cơ sở dữ liệu: " + ex.Message, "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void LoadPosBranchComboBox()
        {
            try
            {
                // Xóa sạch dữ liệu cũ
                cbSalesBranch.ItemsSource = null;

                List<PosBranchItem> danhSach = new List<PosBranchItem>();
                string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // Lấy cả ID và Tên chi nhánh 
                    string query = "SELECT id, name FROM Stores";

                    // Nếu là Manager thì chỉ lấy đúng chi nhánh của họ
                    if (QuyenHan == "MANAGER")
                    {
                        query = "SELECT id, name FROM Stores WHERE id = @id";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        if (QuyenHan == "MANAGER")
                        {
                            cmd.Parameters.AddWithValue("@id", MaChiNhanh);
                        }

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                danhSach.Add(new PosBranchItem()
                                {
                                    BranchId = reader["id"].ToString(),
                                    // Nối tên và mã
                                    BranchName = $"{reader["name"].ToString()} ({reader["id"].ToString()})"
                                });
                            }
                        }
                    }
                }

                cbSalesBranch.ItemsSource = danhSach;

                cbSalesBranch.DisplayMemberPath = "BranchName"; // Yêu cầu UI hiển thị Tên
                cbSalesBranch.SelectedValuePath = "BranchId";   // Yêu cầu Code ngầm giữ ID

                // Mặc định chọn dòng đầu tiên để không bị trống
                if (danhSach.Count > 0)
                {
                    cbSalesBranch.SelectedIndex = 0;
                }

                // Phân quyền tương tác
                if (QuyenHan == "MANAGER")
                {
                    cbSalesBranch.IsEnabled = false; // Manager không được đổi
                }
                else
                {
                    cbSalesBranch.IsEnabled = true;  // Admin được phép đổi
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi nhánh POS: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void dgBranches_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        }

        private void cbEmployeeBranch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Đảm bảo ComboBox đã có dữ liệu rồi mới Load
            if (cbEmployeeBranch.SelectedValue != null)
            {
                LoadEmployees();
            }
            // LoadEmployeesForSelectedBranch();
        }
        private void LoadEmployees()
        {
            // chống lỗi khi giao diện chưa load xong
            if (cbEmployeeBranch.SelectedValue == null)
            {
                return; // Thoát hàm luôn, không chạy đoạn code bên dưới nữa
            }
            List<EmployeeItem> danhSachNV = new List<EmployeeItem>();
            string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Chỉ lấy nhân viên của đúng chi nhánh mà Manager này đang quản lý
                    string query = "SELECT id, full_name, shift, work_hours, is_clocked_in, job_role FROM Employees WHERE store_id = @storeId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@storeId", cbEmployeeBranch.SelectedValue.ToString());

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                danhSachNV.Add(new EmployeeItem()
                                {
                                    EmpId = reader["id"].ToString(),
                                    Name = reader["full_name"].ToString(),
                                    Shift = reader["shift"].ToString(),
                                    WorkTime = reader["work_hours"].ToString(),
                                    JobRole = reader["job_role"] == DBNull.Value || reader["job_role"] == null
                                        ? "Phục vụ"
                                        : reader["job_role"].ToString(),
                                    PresentStatus = Convert.ToBoolean(reader["is_clocked_in"]) ? "Đã chấm công" : "Chưa chấm công"
                                });
                            }
                        }
                    }
                    dgEmployees.ItemsSource = danhSachNV;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải danh sách nhân viên: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void LoadBranchComboBox()
        {
            List<StoreItem> danhSachCN = new List<StoreItem>();
            string connString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand("SELECT id, name FROM Stores", conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        danhSachCN.Add(new StoreItem()
                        {
                            Id = reader["id"].ToString(),
                            Name = reader["name"].ToString()
                        });
                    }
                }
            }

            cbEmployeeBranch.ItemsSource = danhSachCN;
            cbEmployeeBranch.DisplayMemberPath = "Name"; // Chữ hiện lên màn hình
            cbEmployeeBranch.SelectedValuePath = "Id";   

            if (QuyenHan == "MANAGER")
            {
                cbEmployeeBranch.SelectedValue = MaChiNhanh; 
                cbEmployeeBranch.IsEnabled = false;          // Khóa cứng không cho bấm
            }
            else if (QuyenHan == "ADMIN")
            {
                cbEmployeeBranch.IsEnabled = true;           // ADMIN được chọn tự do
                if (danhSachCN.Count > 0) cbEmployeeBranch.SelectedIndex = 0;
            }
        }
        private void LoadReports()
        {
            List<SupportReportItem> danhSachBaoCao = new List<SupportReportItem>();
            string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Lấy hết sự cố, ưu tiên đưa thằng "Chưa xử lý" lên trên, và sắp xếp theo ngày gần nhất
                    string query = "SELECT id, store_id, report_type, message, created_at, is_resolved FROM Support_Reports ORDER BY is_resolved ASC, created_at DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            danhSachBaoCao.Add(new SupportReportItem()
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                StoreId = reader["store_id"].ToString(),
                                ReportType = reader["report_type"].ToString(),
                                Message = reader["message"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["created_at"]),
                                IsResolved = Convert.ToBoolean(reader["is_resolved"])
                            });
                        }
                    }

                    dgReports.ItemsSource = danhSachBaoCao;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải danh sách sự cố: " + ex.Message, "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void LoadMenu()
        {
            List<ProductItem> danhSachMon = new List<ProductItem>();
            string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Lấy dữ liệu từ các cột theo đúng thiết kế của bạn
                    string query = "SELECT id, name, type, base_price FROM Products";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            danhSachMon.Add(new ProductItem()
                            {
                                Id = reader["id"].ToString(),
                                Name = reader["name"].ToString(),
                                Type = reader["type"].ToString(),
                                BasePrice = Convert.ToDecimal(reader["base_price"])
                            });
                        }
                    }
                    dgGlobalMenu.ItemsSource = danhSachMon;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải Menu: " + ex.Message);
                }
            }
        }
        private void btnAddEmployee_Click(object sender, RoutedEventArgs e) 
        {
            // 1. Lấy dữ liệu từ các ô nhập liệu 
            string ten = txtEmpName.Text.Trim(); // Ô tên nhân viên
            string caLam = cbEmpShift.Text.Trim();     // Ô ca làm
            string gioLam = txtEmpWorkTime.Text.Trim(); // Ô thời gian làm
            string vaiTro = GetEmpRoleSelected(cbEmpRole);
            if (string.IsNullOrWhiteSpace(vaiTro))
                vaiTro = "Phục vụ";
            bool laThuNgan = vaiTro == "Thu ngân";
            string taiKhoan = txtTaiKhoanNV.Text.Trim();
            string matKhau = txtMatKhauNV.Text;

            if (string.IsNullOrWhiteSpace(ten))
            {
                MessageBox.Show("Vui lòng nhập tên nhân viên!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (laThuNgan && (string.IsNullOrWhiteSpace(taiKhoan) || string.IsNullOrWhiteSpace(matKhau)))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên, Tài khoản và Mật khẩu cho nhân viên!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Kết nối Database
            string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    if (laThuNgan)
                    {
                        // 3. Kiểm tra xem Tài khoản đăng nhập này đã có ai dùng chưa
                        string checkUser = "SELECT COUNT(*) FROM Accounts WHERE username = @user";
                        using (MySqlCommand cmdCheck = new MySqlCommand(checkUser, conn))
                        {
                            cmdCheck.Parameters.AddWithValue("@user", taiKhoan);
                            if (Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0)
                            {
                                MessageBox.Show("Tài khoản đăng nhập này đã tồn tại! Vui lòng chọn tên khác.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }

                    // 4. BẮT ĐẦU GIAO DỊCH KÉP (TRANSACTION)
                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // A. Tự động đếm và tạo Mã Nhân Viên (NV001, NV002...)
                            string newId = "";
                            string countQuery = "SELECT COUNT(*) FROM Employees";
                            using (MySqlCommand cmdCount = new MySqlCommand(countQuery, conn, transaction))
                            {
                                int count = Convert.ToInt32(cmdCount.ExecuteScalar());
                                newId = "NV" + (count + 1).ToString("D3");
                            }

                            // B. Lưu vào bảng Hồ sơ Nhân sự (Employees)
                            string insertEmp = "INSERT INTO Employees (id, store_id, full_name, shift, work_hours, job_role) VALUES (@id, @storeId, @name, @shift, @hours, @jobRole)";
                            using (MySqlCommand cmdEmp = new MySqlCommand(insertEmp, conn, transaction))
                            {
                                cmdEmp.Parameters.AddWithValue("@id", newId);
                                cmdEmp.Parameters.AddWithValue("@storeId", cbEmployeeBranch.SelectedValue.ToString()); // Gắn NV này vào đúng chi nhánh đang đăng nhập
                                cmdEmp.Parameters.AddWithValue("@name", ten);
                                cmdEmp.Parameters.AddWithValue("@shift", caLam);
                                cmdEmp.Parameters.AddWithValue("@hours", gioLam);
                                cmdEmp.Parameters.AddWithValue("@jobRole", vaiTro);
                                cmdEmp.ExecuteNonQuery();
                            }

                            // C. Chỉ thu ngân: lưu vào bảng Tài khoản cấp quyền STAFF (Accounts)
                            if (laThuNgan)
                            {
                                string insertAcc = "INSERT INTO Accounts (username, password, role, store_id, full_name) VALUES (@user, @pass, 'STAFF', @storeId, @name)";
                                using (MySqlCommand cmdAcc = new MySqlCommand(insertAcc, conn, transaction))
                                {
                                    cmdAcc.Parameters.AddWithValue("@user", taiKhoan);
                                    cmdAcc.Parameters.AddWithValue("@pass", matKhau);
                                    cmdAcc.Parameters.AddWithValue("@storeId", cbEmployeeBranch.SelectedValue.ToString());
                                    cmdAcc.Parameters.AddWithValue("@name", ten);
                                    cmdAcc.ExecuteNonQuery();
                                }
                            }

                            // D. Hoàn tất giao dịch
                            transaction.Commit();

                            MessageBox.Show(
                                laThuNgan
                                    ? $"Đã thêm nhân viên {ten} ({newId}) và cấp tài khoản STAFF thành công!"
                                    : $"Đã thêm nhân viên {ten} ({newId}) thành công!",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Gọi hàm LoadEmployees() để bảng nhân viên hiện cập nhật ngay lập tức
                            LoadEmployees();

                            // Xóa trắng form để nhập người tiếp theo
                            txtEmpName.Text = "";
                            txtTaiKhoanNV.Text = "";
                            txtMatKhauNV.Text = "";
                            SyncEmpWorkTimeFromShift();
                        }
                        catch (Exception exTrans)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Lỗi tạo nhân viên, đã hoàn tác để bảo vệ dữ liệu: " + exTrans.Message, "Lỗi Nghiệp Vụ", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kết nối Database: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            //var store = GetSelectedBranch();
            //if (store == null) {
            //    MessageBox.Show("Vui lòng chọn chi nhánh trước khi thêm nhân viên!"); return;
            //}
            //if (txtEmpName.Text == txtEmpName.Tag?.ToString() || string.IsNullOrWhiteSpace(txtEmpName.Text) ||
            //    txtEmpWorkTime.Text == txtEmpWorkTime.Tag?.ToString() || string.IsNullOrWhiteSpace(txtEmpWorkTime.Text)) {
            //    MessageBox.Show("Vui lòng nhập đủ thông tin nhân viên!"); return;
            //}
            //string empId = SystemManager.Instance.GenerateNextEmployeeId();
            //string shift = (cbEmpShift.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Ca Sáng";

            //SystemManager.Instance.AddEmployee(new Employee(store.Id, empId, txtEmpName.Text.Trim(), shift, txtEmpWorkTime.Text.Trim(), chkEmpPresent.IsChecked == true));
            //MessageBox.Show($"Đã thêm nhân viên thành công với mã: {empId}!");
            //txtEmpName.Text = txtEmpName.Tag?.ToString();
            //txtEmpWorkTime.Text = txtEmpWorkTime.Tag?.ToString();
            //chkEmpPresent.IsChecked = false;
            //cbEmpShift.SelectedIndex = 0;
            //LoadEmployeesForSelectedBranch();
        }

        private void btnToggleEmployee_Click(object sender, RoutedEventArgs e)
        {
            // 1. Lấy nhân viên đang được chọn trên bảng
            EmployeeItem nvDangChon = dgEmployees.SelectedItem as EmployeeItem;

            if (nvDangChon == null)
            {
                MessageBox.Show("Vui lòng click chọn một nhân viên trên bảng trước khi chuyển trạng thái!", "Nhắc nhở", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

        
            string idNhanVien = nvDangChon.EmpId;

            string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Lệnh SQL đảo ngược trạng thái Boolean: is_clocked_in = NOT is_clocked_in
                    string updateQuery = "UPDATE Employees SET is_clocked_in = NOT is_clocked_in WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idNhanVien);
                        cmd.ExecuteNonQuery();
                    }

                    // Tải lại bảng ngay lập tức để thấy chữ "Chưa chấm công" biến thành "Đã chấm công"
                    LoadEmployees();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi cập nhật trạng thái: " + ex.Message, "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            //if (dgEmployees.SelectedItem is Employee emp) {
            //    SystemManager.Instance.ToggleEmployeePresent(emp.EmpId, emp.StoreId);
            //    LoadEmployeesForSelectedBranch();
            //    MessageBox.Show($"Đã cập nhật trạng thái nhân viên {emp.Name}.");
            //} else {
            //    MessageBox.Show("Vui lòng chọn nhân viên để thay đổi trạng thái!");
            //}
        }

        private void btnDeleteEmployee_Click(object sender, RoutedEventArgs e) 
        {
            string idCanXoa = txtDelEmployeeId.Text.Trim();

            // Kiểm tra nếu ô trống hoặc người dùng chưa nhập gì
            if (string.IsNullOrWhiteSpace(idCanXoa) || idCanXoa == "Nhập ID cần xóa")
            {
                MessageBox.Show("Vui lòng nhập Mã Nhân Viên (Ví dụ: NV001) vào ô trống để xóa!", "Nhắc nhở", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Hỏi lại cho chắc chắn
            MessageBoxResult xacNhan = MessageBox.Show($"Bạn có chắc chắn muốn xóa vĩnh viễn nhân viên có mã '{idCanXoa}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (xacNhan == MessageBoxResult.Yes)
            {
                string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM Employees WHERE id = @id AND store_id = @storeId";

                        using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", idCanXoa);
                            cmd.Parameters.AddWithValue("@storeId", cbEmployeeBranch.SelectedValue.ToString()); // Lấy ID chi nhánh của Manager đang đăng nhập

                            // ExecuteNonQuery trả về số dòng bị tác động (số dòng bị xóa)
                            int soDongBiXoa = cmd.ExecuteNonQuery();

                            if (soDongBiXoa > 0) // Nếu > 0 tức là đã tìm thấy và xóa thành công
                            {
                                MessageBox.Show($"Đã xóa thành công hồ sơ nhân viên {idCanXoa}!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                                LoadEmployees(); // Tải lại bảng danh sách

                                txtDelEmployeeId.Text = ""; // Xóa trắng ô nhập liệu
                            }
                            else
                            {
                                // Nếu = 0 tức là nhập sai mã, hoặc mã đó của chi nhánh khác
                                MessageBox.Show($"Không tìm thấy nhân viên '{idCanXoa}' thuộc chi nhánh của bạn. Vui lòng kiểm tra lại mã!", "Lỗi Xóa", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi cơ sở dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            //var store = GetSelectedBranch();
            //if (store == null) {
            //    MessageBox.Show("Vui lòng chọn chi nhánh trước!"); return;
            //}
            //string empId = txtDelEmployeeId.Text;
            //if (empId == txtDelEmployeeId.Tag?.ToString() || string.IsNullOrWhiteSpace(empId)) {
            //    MessageBox.Show("Nhập mã nhân viên cần xóa!"); return;
            //}
            //SystemManager.Instance.DeleteEmployee(empId.Trim(), store.Id);
            //LoadEmployeesForSelectedBranch();
            //MessageBox.Show("Đã xóa nhân viên.");
        }

        private void btnDeleteBranch_Click(object sender, RoutedEventArgs e) {
            StoreItem chiNhanhDangChon = dgBranches.SelectedItem as StoreItem;

            // Kiểm tra xem người dùng đã chọn dòng nào chưa (lỡ họ chưa chọn mà bấm xóa)
            if (chiNhanhDangChon == null)
            {
                MessageBox.Show("Vui lòng click chọn một chi nhánh trên bảng danh sách bên trái trước khi bấm xóa!", "Hướng dẫn", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string storeId = chiNhanhDangChon.Id;
            string storeName = chiNhanhDangChon.Name;

            // Không bao giờ cho phép xóa Trụ sở chính (Nơi chứa tài khoản Admin)
            if (storeId == "S01")
            {
                MessageBox.Show("S01 là Chi Nhánh Trung Tâm chứa tài khoản ADMIN gốc. Bạn KHÔNG THỂ xóa chi nhánh này để bảo vệ hệ thống!", "Cảnh Báo Chớp Đỏ", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Bật hộp thoại hỏi cho thật chắc chắn (Xóa là mất luôn không cứu được)
            MessageBoxResult xacNhan = MessageBox.Show($"Bạn có chắc chắn muốn xóa vĩnh viễn chi nhánh '{storeName}' ({storeId}) không?\n\nLƯU Ý: Toàn bộ tài khoản quản lý và kho hàng của chi nhánh này sẽ bị xóa sạch!",
                                                       "Xác nhận xóa nguy hiểm",
                                                       MessageBoxButton.YesNo,
                                                       MessageBoxImage.Warning);

            // Nếu người dùng chọn Yes thì mới làm
            if (xacNhan == MessageBoxResult.Yes)
            {
                string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();

                        // Xóa (MySQL sẽ tự lo phần dọn dẹp còn lại nhờ Cascade)
                        string deleteQuery = "DELETE FROM Stores WHERE id = @id";
                        using (MySqlCommand cmdDelete = new MySqlCommand(deleteQuery, conn))
                        {
                            cmdDelete.Parameters.AddWithValue("@id", storeId);
                            cmdDelete.ExecuteNonQuery();
                        }

                        // Thông báo và Cập nhật lại cái bảng ngay lập tức
                        MessageBox.Show($"Đã xóa sổ chi nhánh {storeId} thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadBranches();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi trong quá trình xóa: " + ex.Message, "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        private void btnManageStore_Click(object sender, RoutedEventArgs e)
        {
            if (cbSalesBranch.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn chi nhánh trước khi vào bán hàng!", "Nhắc nhở", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Chống spam 
            // Duyệt qua tất cả các cửa sổ đang mở trong ứng dụng
            foreach (Window window in Application.Current.Windows)
            {
                // Nếu tìm thấy một cửa sổ nào đó là PosWindow
                if (window is PosWindow openedPos)
                {
                    // Lỡ người dùng đang thu nhỏ (Minimize) thì bung nó lên lại
                    if (openedPos.WindowState == WindowState.Minimized)
                    {
                        openedPos.WindowState = WindowState.Normal;
                    }

                    // Mang cửa sổ đó lên trên cùng, nhấp nháy cho người dùng thấy
                    openedPos.Activate();

                    // Dừng hàm ngay lập tức, không chạy xuống dưới để tạo cửa sổ mới nữa
                    return;
                }
            }

            string selectedBranch = cbSalesBranch.SelectedValue.ToString();
            string tenNguoiDung = "Nhân viên";
            if (this.QuyenHan == "ADMIN")
            {
                tenNguoiDung = "Quản Trị Viên (Admin)";
            }
            else if (this.QuyenHan == "MANAGER")
            {
                tenNguoiDung = $"Quản lý Chi nhánh";
            }

            PosWindow pos = new PosWindow(tenNguoiDung, selectedBranch);
            pos.Show();

        }

        private void btnAddGlobalProduct_Click(object sender, RoutedEventArgs e) 
        {
            string loai = cbType.Text; // "Nước Uống" hoặc "Topping"
            string ten = txtNewName.Text.Trim();
            string giaText = txtNewPrice.Text.Trim();

            if (string.IsNullOrWhiteSpace(ten) || string.IsNullOrWhiteSpace(giaText))
            {
                MessageBox.Show("Vui lòng nhập Tên món và Giá bán!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra xem giá bán có phải là số hợp lệ không
            if (!decimal.TryParse(giaText, out decimal giaBan))
            {
                MessageBox.Show("Giá bán phải là một con số hợp lệ!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // ID: Dxx = Nước uống, Txx = Topping
                    bool isTopping = string.Equals(loai, "Topping", StringComparison.OrdinalIgnoreCase);
                    string prefix = isTopping ? "T" : "D";
                    string newId = NextProductIdByPrefix(conn, prefix);
                    string tenMonMoi = txtNewName.Text.Trim();

                    // Vô Database hỏi xem có ai tên này chưa?
                    string checkQuery = "SELECT COUNT(*) FROM Products WHERE name = @name";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@name", tenMonMoi);

                        // Thực thi và đếm số lượng
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            // Nếu lớn hơn 0 nghĩa là đã có món này rồi -> Báo lỗi và DỪNG LẠI
                            MessageBox.Show("Tên món này đã tồn tại trong hệ thống! Vui lòng đặt tên khác.", "Cảnh báo trùng lặp", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return; // Lệnh return này sẽ kết thúc hàm ngay lập tức, không chạy xuống phần INSERT bên dưới nữa
                        }
                    }
                    // Thêm vào Database
                    string insertQuery = "INSERT INTO Products (id, name, type, base_price) VALUES (@id, @name, @type, @price)";
                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", newId);
                        cmd.Parameters.AddWithValue("@name", ten);
                        cmd.Parameters.AddWithValue("@type", loai);
                        cmd.Parameters.AddWithValue("@price", giaBan);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Đã thêm món mới thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    // Lấy số 100 từ ô TextBox
                    int soLuongBanDau = int.Parse(txtNewStock.Text);

                    // bỏ vào kho của chi nhánh nào? (S01 hay S02?)
                    string query2 = "INSERT INTO branch_inventory (store_id, product_id, stock) VALUES ('S01', @id, @soLuong)";
                    using (MySqlCommand cmd2 = new MySqlCommand(query2, conn))
                    {
                        cmd2.Parameters.AddWithValue("@id", newId); // Ví dụ: M21
                        cmd2.Parameters.AddWithValue("@soLuong", soLuongBanDau);
                        cmd2.ExecuteNonQuery();
                    }
                    LoadMenu(); // Cập nhật lại bảng

                    // Xóa trắng form
                    txtNewName.Clear();
                    txtNewPrice.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi thêm món: " + ex.Message, "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnDelGlobalProduct_Click(object sender, RoutedEventArgs e)
        {
            string idCanXoa = txtDelId.Text.Trim();

            if (string.IsNullOrWhiteSpace(idCanXoa) || idCanXoa == "Nhập ID cần xóa")
            {
                MessageBox.Show("Vui lòng nhập Mã Món (ví dụ: D01, T01, M01) vào ô trống để xóa!", "Nhắc nhở", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Cảnh báo lần cuối vì xóa món là mất luôn trên toàn hệ thống
            MessageBoxResult xacNhan = MessageBox.Show($"Bạn có chắc chắn muốn xóa vĩnh viễn món có mã '{idCanXoa}' khỏi Menu toàn chuỗi không?",
                                                       "Xác nhận xóa nguy hiểm",
                                                       MessageBoxButton.YesNo,
                                                       MessageBoxImage.Warning);

            if (xacNhan == MessageBoxResult.Yes)
            {
                string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();

                        string deleteQuery = "DELETE FROM Products WHERE id = @id";
                        using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", idCanXoa);

                            int soDongBiXoa = cmd.ExecuteNonQuery();

                            if (soDongBiXoa > 0)
                            {
                                MessageBox.Show($"Đã xóa sổ món {idCanXoa} thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                                LoadMenu(); // Cập nhật lại cái bảng Menu ngay lập tức

                                txtDelId.Text = ""; // Xóa trắng ô nhập liệu
                            }
                            else
                            {
                                MessageBox.Show($"Không tìm thấy món nào có mã '{idCanXoa}' trong hệ thống. Vui lòng kiểm tra lại!", "Lỗi Xóa", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi cơ sở dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        private void btnAddVoucher_Click(object sender, RoutedEventArgs e) {
            string code = txtVCode.Text.Trim();

            // Kiểm tra nhập liệu cơ bản
            if (string.IsNullOrEmpty(code) || !dpVExpiry.SelectedDate.HasValue)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Mã Code và Hạn sử dụng!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra % giảm giá (Phải từ 0.5 đến 50)
            if (!decimal.TryParse(txtVPct.Text, out decimal percent) || percent < 0.5m || percent > 50m)
            {
                MessageBox.Show("% Giảm giá phải là số từ 0.5 đến 50!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Kiểm tra số tiền giảm tối đa
            if (!int.TryParse(txtVMax.Text, out int maxAmount) || maxAmount <= 0)
            {
                MessageBox.Show("Số tiền giảm tối đa phải lớn hơn 0!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Đẩy xuống Database
            string query = "INSERT INTO Vouchers (voucher_code, discount_percent, max_discount_amount, expiry_date) VALUES (@code, @percent, @max, @date)";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@code", code.ToUpper()); // Tự động in hoa mã code cho đẹp
                        cmd.Parameters.AddWithValue("@percent", percent);
                        cmd.Parameters.AddWithValue("@max", maxAmount);
                        cmd.Parameters.AddWithValue("@date", dpVExpiry.SelectedDate.Value.ToString("yyyy-MM-dd"));

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Thêm Voucher thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Reset form và tải lại bảng
                        txtVCode.Clear(); txtVPct.Clear(); txtVMax.Clear(); dpVExpiry.SelectedDate = null;
                        LoadVouchers();
                    }
                }
                catch (MySqlException ex)
                {
                    if (ex.Number == 1062) // Lỗi trùng khóa chính (Trùng mã Code)
                        MessageBox.Show("Mã Code này đã tồn tại! Vui lòng đặt mã khác.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show("Lỗi Database: " + ex.Message);
                }
            }
        }

        private void btnDelVoucher_Click(object sender, RoutedEventArgs e) 
        {
            string code = txtDelVoucher.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Vui lòng nhập Mã Code cần xóa!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Hỏi lại cho chắc ăn
            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa Voucher '{code}' không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.No) return;

            string query = "DELETE FROM Vouchers WHERE voucher_code = @code";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@code", code);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Đã xóa Voucher thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            txtDelVoucher.Clear();
                            LoadVouchers(); // Tải lại bảng
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy Mã Code này trong hệ thống!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa: " + ex.Message);
                }
            }
        }

        private void btnResolveReport_Click(object sender, RoutedEventArgs e)
        {
            // Lấy sự cố đang được chọn trên bảng
            if (dgReports.SelectedItem is SupportReportItem r)
            {
                if (r.IsResolved)
                {
                    MessageBox.Show("Báo cáo này đã được xử lý từ trước!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        // Cập nhật trạng thái thành TRUE (Đã xử lý)
                        string query = "UPDATE Support_Reports SET is_resolved = TRUE WHERE id = @id";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", r.Id);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Đã đánh dấu xử lý xong sự cố!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Tải lại bảng để nó tự nhảy trạng thái và rớt xuống dưới
                        LoadReports();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi cập nhật sự cố: " + ex.Message, "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng click chọn 1 báo cáo trên bảng trước khi bấm xử lý!", "Nhắc nhở", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnResetSystem_Click(object sender, RoutedEventArgs e) 
        {
            if (MessageBox.Show("XÓA MỌI THỨ?\n\nHành động này sẽ xóa sạch toàn bộ Menu, Hóa đơn, Nhân viên và Chi nhánh.\nChỉ giữ lại duy nhất tài khoản Admin gốc (S01).", "CẢNH BÁO NGUY HIỂM", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {

                string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();

                        // Tắt tạm thời bảo vệ khóa ngoại để có thể xóa sạch mọi thứ
                        using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", conn)) { cmd.ExecuteNonQuery(); }

                        // Dọn sạch rác ở TẤT CẢ các bảng
                        string[] tables = { "Invoice_Details", "Invoices", "branch_inventory", "Support_Reports", "Products", "Vouchers", "Accounts", "Employees", "Stores" };
                        foreach (string table in tables)
                        {
                            using (MySqlCommand cmd = new MySqlCommand($"TRUNCATE TABLE {table}", conn)) { cmd.ExecuteNonQuery(); }
                        }

                        // Bơm lại Admin gốc để không bị kẹt
                        using (MySqlCommand cmd = new MySqlCommand("INSERT INTO Stores (id, name) VALUES ('S01', 'Chi Nhánh Trung Tâm')", conn)) { cmd.ExecuteNonQuery(); }
                        using (MySqlCommand cmd = new MySqlCommand("INSERT INTO Accounts (username, password, role, store_id) VALUES ('admin', '123', 'ADMIN', 'S01')", conn)) { cmd.ExecuteNonQuery(); }

                        // Bật lại khóa ngoại cho hệ thống an toàn
                        using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1;", conn)) { cmd.ExecuteNonQuery(); }

                        MessageBox.Show("Hệ thống đã được reset hoàn toàn về trạng thái ban đầu!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Gọi từng hàm mới để vẽ lại màn hình trắng trơn
                        LoadBranches();
                        LoadEmployees();
                        LoadBranchComboBox();
                        LoadMenu();
                        LoadPosBranchComboBox();
                        LoadVouchers();
                        LoadReports();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi reset hệ thống: " + ex.Message, "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnAutoGenGlobal_Click(object sender, RoutedEventArgs e)
        {
            // Tạo sẵn một menu giả lập 20 món cực hấp dẫn
            var mockMenu = new List<(string Name, string Type, decimal BasePrice)>
    {
        ("Trà Sữa Truyền Thống", "Nước Uống", 30000), ("Trà Sữa Thái Xanh", "Nước Uống", 35000),
        ("Trà Sữa Oolong Nướng", "Nước Uống", 40000), ("Trà Đào Cam Sả", "Nước Uống", 35000),
        ("Trà Vải Nhiệt Đới", "Nước Uống", 35000), ("Trà Đen Macchiato", "Nước Uống", 45000),
        ("Matcha Đá Xay", "Nước Uống", 50000), ("Sữa Tươi Trân Châu Đường Đen", "Nước Uống", 45000),
        ("Cà Phê Sữa Đá", "Nước Uống", 25000), ("Bạc Xỉu", "Nước Uống", 29000),
        ("Trân Châu Đen", "Topping", 5000), ("Trân Châu Trắng", "Topping", 7000),
        ("Thạch Trái Cây", "Topping", 5000), ("Thạch Phô Mai", "Topping", 10000),
        ("Kem Macchiato", "Topping", 15000), ("Pudding Trứng", "Topping", 8000),
        ("Khúc Bạch", "Topping", 12000), ("Hạt Sen Bùi", "Topping", 10000),
        ("Trà Dâu Tằm", "Nước Uống", 38000), ("Trà Sữa Socola", "Nước Uống", 35000)
    };

            string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    var storeIds = new List<string>();
                    using (var cmdStores = new MySqlCommand("SELECT id FROM Stores", conn))
                    using (var rd = cmdStores.ExecuteReader())
                    {
                        while (rd.Read())
                            storeIds.Add(rd.GetString(0));
                    }
                    if (storeIds.Count == 0)
                    {
                        MessageBox.Show("Chưa có chi nhánh nào trong hệ thống.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    int insertedCount = 0;
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        int dSeq = GetMaxProductNumericSuffix(conn, trans, "D");
                        int tSeq = GetMaxProductNumericSuffix(conn, trans, "T");
                        string insertProduct = "INSERT INTO Products (id, name, type, base_price) VALUES (@Id, @Name, @Type, @BasePrice)";
                        string insertInv = "INSERT INTO branch_inventory (store_id, product_id, stock) VALUES (@s, @p, 100)";

                        foreach (var item in mockMenu)
                        {
                            using (var chk = new MySqlCommand("SELECT COUNT(*) FROM Products WHERE name = @n", conn, trans))
                            {
                                chk.Parameters.AddWithValue("@n", item.Name);
                                if (Convert.ToInt32(chk.ExecuteScalar()) > 0)
                                    continue;
                            }

                            bool isTop = string.Equals(item.Type, "Topping", StringComparison.OrdinalIgnoreCase);
                            string newId;
                            if (isTop)
                            {
                                tSeq++;
                                newId = "T" + tSeq.ToString("D2");
                            }
                            else
                            {
                                dSeq++;
                                newId = "D" + dSeq.ToString("D2");
                            }

                            using (var cmd = new MySqlCommand(insertProduct, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@Id", newId);
                                cmd.Parameters.AddWithValue("@Name", item.Name);
                                cmd.Parameters.AddWithValue("@Type", item.Type);
                                cmd.Parameters.AddWithValue("@BasePrice", item.BasePrice);
                                cmd.ExecuteNonQuery();
                            }

                            foreach (string sid in storeIds)
                            {
                                using var cmdKho = new MySqlCommand(insertInv, conn, trans);
                                cmdKho.Parameters.AddWithValue("@s", sid);
                                cmdKho.Parameters.AddWithValue("@p", newId);
                                try { cmdKho.ExecuteNonQuery(); }
                                catch (MySqlException mex) when (mex.Number == 1062) { /* đã có dòng kho */ }
                            }

                            insertedCount++;
                        }

                        trans.Commit();
                    }

                    MessageBox.Show($"Đã thêm {insertedCount} món mẫu mới (tên đã có trong hệ thống thì bỏ qua). ID nước: Dxx, ID topping: Txx — mỗi chi nhánh kho 100.", "Hoàn tất", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadMenu();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tạo món mẫu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
       
        }

        // HÀM ĐĂNG XUẤT CHO ADMIN
        private void btnLogout_Click(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Đăng xuất", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                MainWindow.ShowLoginAndCloseAllOtherWindows();
            }
        }
        private void dgGlobalMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Lấy dòng (món) mà người dùng vừa click vào
            if (dgGlobalMenu.SelectedItem is ProductItem monDangChon)
            {
                // Tự động bốc cái Mã Món đẩy xuống ô Textbox Xóa
                txtDelId.Text = monDangChon.Id;
            }
        }
        private void dgVouchers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Kiểm tra xem DataGrid có đang chọn dòng nào không
            // Và dòng đó có đúng là chứa dữ liệu kiểu VoucherItem không
            if (dgVouchers.SelectedItem is VoucherItem selectedVoucher)
            {
                // Lôi cái Code từ dòng được chọn và gán thẳng vào ô đó
                txtDelVoucher.Text = selectedVoucher.Code;
            }
        }
        private void cbSalesBranch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
 
}