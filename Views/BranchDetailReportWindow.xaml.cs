using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using MySql.Data.MySqlClient;

namespace TraSuaApp.Views
{
    public partial class BranchDetailReportWindow : Window
    {
        private readonly string _connectionString;
        private readonly string _storeId;
        private readonly string _storeDisplayName;
        private readonly DateTime _dateFrom;
        private readonly DateTime _dateTo;

        private const int LowStockThreshold = 50;

        public BranchDetailReportWindow(string connectionString, string storeId, string storeDisplayName, DateTime dateFrom, DateTime dateTo)
        {
            InitializeComponent();
            _connectionString = connectionString;
            _storeId = storeId;
            _storeDisplayName = storeDisplayName;
            _dateFrom = dateFrom.Date;
            _dateTo = dateTo.Date;
            Loaded += (_, _) => LoadData();
        }

        private void LoadData()
        {
            txtBranchName.Text = _storeDisplayName;
            txtReportPeriod.Text = $"Từ {_dateFrom:dd/MM/yyyy} đến {_dateTo:dd/MM/yyyy}";

            var top = new List<BranchReportTopSellerRow>();
            var low = new List<BranchReportLowStockRow>();
            var drinks = new List<BranchReportMenuRow>();
            var toppings = new List<BranchReportMenuRow>();
            decimal totalRev = 0;
            int totalOrders = 0;

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                using (var cmd = new MySqlCommand(
                           @"SELECT COALESCE(SUM(final_total),0), COUNT(*) FROM Invoices
                             WHERE store_id = @sid AND DATE(created_at) BETWEEN @d1 AND @d2", conn))
                {
                    cmd.Parameters.AddWithValue("@sid", _storeId);
                    cmd.Parameters.AddWithValue("@d1", _dateFrom.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@d2", _dateTo.ToString("yyyy-MM-dd"));
                    using var r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        totalRev = Convert.ToDecimal(r[0]);
                        totalOrders = Convert.ToInt32(r[1]);
                    }
                }

                using (var cmd = new MySqlCommand(
                           @"SELECT d.product_id, MAX(d.product_name) AS pname, SUM(d.quantity) AS sqty
                             FROM Invoice_Details d
                             INNER JOIN Invoices i ON d.invoice_id = i.invoice_id
                             WHERE i.store_id = @sid AND DATE(i.created_at) BETWEEN @d1 AND @d2
                             GROUP BY d.product_id
                             ORDER BY sqty DESC
                             LIMIT 5", conn))
                {
                    cmd.Parameters.AddWithValue("@sid", _storeId);
                    cmd.Parameters.AddWithValue("@d1", _dateFrom.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@d2", _dateTo.ToString("yyyy-MM-dd"));
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        top.Add(new BranchReportTopSellerRow
                        {
                            Id = r["product_id"].ToString(),
                            Name = r["pname"].ToString(),
                            SoldQty = Convert.ToInt32(r["sqty"])
                        });
                    }
                }

                using (var cmd = new MySqlCommand(
                           @"SELECT p.id, p.name, IFNULL(b.stock,0) AS st
                             FROM Products p
                             LEFT JOIN branch_inventory b ON p.id = b.product_id AND b.store_id = @sid
                             WHERE IFNULL(b.stock,0) < @th
                             ORDER BY IFNULL(b.stock,0) ASC", conn))
                {
                    cmd.Parameters.AddWithValue("@sid", _storeId);
                    cmd.Parameters.AddWithValue("@th", LowStockThreshold);
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        low.Add(new BranchReportLowStockRow
                        {
                            Id = r["id"].ToString(),
                            Name = r["name"].ToString(),
                            Stock = Convert.ToInt32(r["st"])
                        });
                    }
                }

                using (var cmd = new MySqlCommand(
                           @"SELECT p.id, p.name, p.type, p.base_price, IFNULL(b.stock,0) AS st
                             FROM Products p
                             LEFT JOIN branch_inventory b ON p.id = b.product_id AND b.store_id = @sid
                             ORDER BY p.type, p.name", conn))
                {
                    cmd.Parameters.AddWithValue("@sid", _storeId);
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        var row = new BranchReportMenuRow
                        {
                            Id = r["id"].ToString(),
                            Name = r["name"].ToString(),
                            BasePrice = Convert.ToDecimal(r["base_price"]),
                            Stock = Convert.ToInt32(r["st"])
                        };
                        var typ = r["type"].ToString();
                        if (typ == "Topping")
                            toppings.Add(row);
                        else
                            drinks.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tải được báo cáo chi nhánh: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            txtTotalRevenue.Text = totalRev.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
            txtTotalOrders.Text = totalOrders.ToString(CultureInfo.InvariantCulture);
            dgTopSelling.ItemsSource = top;
            dgLowStock.ItemsSource = low;
            dgMenuDrinks.ItemsSource = drinks;
            dgMenuToppings.ItemsSource = toppings;
        }

        private void btnSupply_Click(object sender, RoutedEventArgs e)
        {
            if (dgLowStock.SelectedItem is not BranchReportLowStockRow row)
            {
                MessageBox.Show("Vui lòng chọn một sản phẩm trong danh sách sắp hết hàng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int qty = 100;
            if (!string.IsNullOrWhiteSpace(txtRestockQty.Text))
            {
                if (int.TryParse(txtRestockQty.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) && parsed > 0)
                    qty = parsed;
            }

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();
                using var trans = conn.BeginTransaction();
                try
                {
                    using (var upd = new MySqlCommand(
                               @"UPDATE branch_inventory SET stock = stock + @q
                                 WHERE store_id = @s AND product_id = @p", conn, trans))
                    {
                        upd.Parameters.AddWithValue("@q", qty);
                        upd.Parameters.AddWithValue("@s", _storeId);
                        upd.Parameters.AddWithValue("@p", row.Id);
                        int n = upd.ExecuteNonQuery();
                        if (n == 0)
                        {
                            using var ins = new MySqlCommand(
                                @"INSERT INTO branch_inventory (store_id, product_id, stock)
                                  VALUES (@s, @p, @q)", conn, trans);
                            ins.Parameters.AddWithValue("@s", _storeId);
                            ins.Parameters.AddWithValue("@p", row.Id);
                            ins.Parameters.AddWithValue("@q", qty);
                            ins.ExecuteNonQuery();
                        }
                    }
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không cập nhật kho được: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show($"Đã bổ sung {qty} cho sản phẩm {row.Name}.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadData();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e) => Close();
    }

    public class BranchReportTopSellerRow
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int SoldQty { get; set; }
    }

    public class BranchReportLowStockRow
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int Stock { get; set; }
    }

    public class BranchReportMenuRow
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal BasePrice { get; set; }
        public int Stock { get; set; }
    }
}
