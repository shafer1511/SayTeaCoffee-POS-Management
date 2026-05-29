using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TraSuaApp.Models;

namespace TraSuaApp.Views
{
    public partial class PosWindow : Window
    {
        //private List<Product> _fullMenu;
        //private List<CartItem> _cart = new List<CartItem>();

        //private double _totalBill = 0;
        //private double _discount = 0;
        private ObservableCollection<CartItem> _gioHang = new ObservableCollection<CartItem>();

        private string _maChiNhanh;
        private string _tenNhanVien;

        public string TenDangNhap { get; set; }
        public string MaChiNhanh { get; set; }

        private VoucherItem _appliedVoucher = null;

        // Thêm 2 tham số (user, storeId)
        public PosWindow(string user, string storeId)
        {
            InitializeComponent();

            _maChiNhanh = storeId;
            _tenNhanVien = LayTenThatNhanVien(user);

            //TenDangNhap = user;
            //MaChiNhanh = storeId;

            txtUserInfo.Text = "Nhân viên: " + _tenNhanVien;

            this.Title = $"SAY TEA COFFEE | BÁN HÀNG - CHI NHÁNH {_maChiNhanh}";

            lbCart.ItemsSource = _gioHang;

            LoadPosMenu();
            LoadValidVouchers();

        }
        private void LoadPosMenu()
        {
            List<PosProductItem> danhSachNuoc = new List<PosProductItem>();
            List<PosProductItem> danhSachTopping = new List<PosProductItem>();

            string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Lấy thông tin món và số lượng kho của đúng chi nhánh này
                    // Hàm IFNULL dùng để xử lý trường hợp món mới tạo, chưa có trong bảng Kho thì mặc định là 0
                    string query = @"
                SELECT p.id, p.name, p.base_price, p.type, IFNULL(b.stock, 0) as stock 
                FROM Products p 
                LEFT JOIN branch_inventory b ON p.id = b.product_id AND b.store_id = @storeId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@storeId", _maChiNhanh);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new PosProductItem()
                                {
                                    Id = reader["id"].ToString(),
                                    DisplayName = reader["name"].ToString(),
                                    BasePrice = Convert.ToDecimal(reader["base_price"]),
                                    Stock = Convert.ToInt32(reader["stock"])
                                };

                                // Tách làm 2 danh sách dựa vào loại
                                if (reader["type"].ToString() == "Nước Uống")
                                {
                                    danhSachNuoc.Add(item);
                                }
                                else if (reader["type"].ToString() == "Topping")
                                {
                                    danhSachTopping.Add(item);
                                }
                            }
                        }
                    }

                    // Gán vào 2 DataGrid
                    dgDrinks.ItemsSource = danhSachNuoc;
                    dgToppings.ItemsSource = danhSachTopping;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải thực đơn POS: " + ex.Message);
                }
            }
        }
        private void LoadValidVouchers()
        {
            List<VoucherItem> danhSachVoucher = new List<VoucherItem>();

            // Câu lệnh SQL có thêm điều kiện WHERE để lọc bỏ các mã đã hết hạn
            string query = "SELECT voucher_code, discount_percent, max_discount_amount, expiry_date FROM Vouchers WHERE expiry_date >= CURDATE()";

            string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            danhSachVoucher.Add(new VoucherItem()
                            {
                                Code = reader["voucher_code"].ToString(),
                                DiscountPercent = Convert.ToDecimal(reader["discount_percent"]),
                                MaxDiscount = Convert.ToInt32(reader["max_discount_amount"]),
                                ExpiryDate = Convert.ToDateTime(reader["expiry_date"])
                            });
                        }
                    }

                    // Đổ vào ComboBox
                    cbVouchers.ItemsSource = danhSachVoucher;
                    cbVouchers.DisplayMemberPath = "DisplayText"; 
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải Voucher: " + ex.Message);
                }
            }
        }
        private string LayTenThatNhanVien(string idDangNhap)
        {
            // Mặc định cứ lấy chính ID làm tên (Áp dụng cho Admin/Manager nếu không tìm thấy trong bảng nhân viên)
            string tenHienThi = idDangNhap;

            string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT full_name FROM Accounts WHERE username = @user";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", idDangNhap);
                        object result = cmd.ExecuteScalar();

                        // Nếu tìm thấy nhân viên này trong Database, ta sẽ lấy tên thật đè lên cái ID
                        if (result != null && result != DBNull.Value)
                        {
                            tenHienThi = result.ToString();
                        }
                    }
                }
                catch (Exception)
                {
                    // Lỡ có lỗi kết nối DB thì cứ im lặng và xài tạm cái ID đăng nhập cho an toàn
                }
            }

            return tenHienThi;
        }
        private void LoadMenuData() 
        {
        
        }

        private void dgMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            if (dg == null) return;

            if (dg.SelectedItem is PosProductItem p)
            {
                // Chỉ được chọn 1 bên: Chọn Nước thì hủy chọn Topping và ngược lại
                if (dg == dgDrinks) dgToppings.SelectedItem = null;
                else dgDrinks.SelectedItem = null;

                // Mở khóa toàn bộ khu vực chọn Size/Đường/Đá
                pnlCustom.IsEnabled = true;

                // Hiển thị tên món đang chọn 
                txtSelectedProduct.Text = $"Đang chọn: {p.DisplayName} ({p.BasePrice:N0}đ)";

                if (btnAddToCart != null) btnAddToCart.IsEnabled = true; // Hiển thị thêm vào giỏ hàng

                // Kiểm tra xem người dùng đang click vào bảng Nước hay bảng Topping
                bool isDrink = (dg == dgDrinks);

                // Bật/tắt các nút tùy chọn (Nước thì bật sáng, Topping thì khóa lại)
                rbSizeS.IsEnabled = rbSizeM.IsEnabled = rbSizeL.IsEnabled = isDrink;
                rbS100.IsEnabled = rbS50.IsEnabled = rbS0.IsEnabled = isDrink;
                rbI100.IsEnabled = rbI50.IsEnabled = rbI0.IsEnabled = isDrink;
            }
            //DataGrid dg = sender as DataGrid;
            //if (dg.SelectedItem is Product p) {
            //    if (dg == dgDrinks) dgToppings.SelectedItem = null;
            //    else dgDrinks.SelectedItem = null;

            //    pnlCustom.IsEnabled = true;
            //    txtSelectedProduct.Text = $"Đang chọn: {p.Name} ({p.BasePrice:N0}đ)";

            //    bool isDrink = p is Drink;
            //    rbSizeS.IsEnabled = rbSizeM.IsEnabled = rbSizeL.IsEnabled = isDrink;
            //    rbS100.IsEnabled = rbS50.IsEnabled = rbS0.IsEnabled = isDrink;
            //    rbI100.IsEnabled = rbI50.IsEnabled = rbI0.IsEnabled = isDrink;
            //}
        }

        private void btnAddToCart_Click(object sender, RoutedEventArgs e) 
        {
            PosProductItem monDangChon = dgDrinks.SelectedItem as PosProductItem;
            if (monDangChon == null)
            {
                monDangChon = dgToppings.SelectedItem as PosProductItem;
            }

            // Nếu chưa chọn món nào ở cả 2 bảng thì la lên
            if (monDangChon == null)
            {
                MessageBox.Show("Vui lòng click chọn một món từ danh sách trước!", "Chưa chọn món", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // nếu chưa chọn hoặc kho = 0 thì không cho thêm vào
            if (monDangChon != null && monDangChon.Stock <= 0)
            {
                MessageBox.Show("Món này đã hết hàng trong kho! Vui lòng chọn món khác.", "Hết hàng", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Lệnh return này sẽ kết thúc hàm ngay lập tức, không cho chạy xuống đoạn thêm vào giỏ bên dưới.
            }
            int soLuongMua = 1; // Mặc định là 1 nếu khách không nhập gì
            if (int.TryParse(txtQty.Text, out int parsedValue) && parsedValue > 0)
            {
                soLuongMua = parsedValue;
            }


            // Đếm xem món này đã có bao nhiêu ly trong giỏ rồi
            int soLuongDaCoTrongGio = _gioHang.Where(x => x.MaMon == monDangChon.Id).Sum(x => x.SoLuong);

            // Tổng số lượng muốn mua = Đã có trong giỏ + Mới nhập thêm
            int tongKiemTra = soLuongMua + soLuongDaCoTrongGio;

            // Nếu tổng này vượt kho thì chặn ngay
            if (tongKiemTra > monDangChon.Stock)
            {
                MessageBox.Show($"Trong kho hiện chỉ có {monDangChon.Stock} phần.\nGiỏ hàng của bạn đã có sẵn {soLuongDaCoTrongGio} phần, không thể thêm {soLuongMua} phần nữa!", "Quá giới hạn kho", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Đá văng ra khỏi hàm
            }
            // Bắt đầu lấy thông tin để cho vào giỏ
            CartItem itemMoi = new CartItem()
            {
                MaMon = monDangChon.Id,
                TenMon = monDangChon.DisplayName,
                DonGia = monDangChon.BasePrice,
               SoLuong = 1 // Tạm thời set cứng là 1, tí nữa chúng ta sẽ móc từ TextBox số lượng sau
            };

            // Xử lý logic cộng tiền Size và các tùy chọn (Chỉ áp dụng cho Thức uống)
            if (dgDrinks.SelectedItem != null)
            {
                if (rbSizeM.IsChecked == true)
                {
                    itemMoi.Size = "M";
                    itemMoi.DonGia += 5000; // Size M cộng 5k
                }
                else if (rbSizeL.IsChecked == true)
                {
                    itemMoi.Size = "L";
                    itemMoi.DonGia += 10000; // Size L cộng 10k
                }
                else
                {
                    itemMoi.Size = "S";
                }

                // Tương tự cho Đường và Đá
                if (rbS50.IsChecked == true) itemMoi.Duong = "50%";
                else if (rbS0.IsChecked == true) itemMoi.Duong = "0%";
                else itemMoi.Duong = "100%";

                if (rbI50.IsChecked == true) itemMoi.Da = "50%";
                else if (rbI0.IsChecked == true) itemMoi.Da = "0%";
                else itemMoi.Da = "100%";
            }

            itemMoi.SoLuong = soLuongMua;
            //itemMoi.ThanhTien = itemMoi.DonGia * soLuongMua;

            // Bỏ món đó vào giỏ
            _gioHang.Add(itemMoi);

            // Cập nhật lại tổng tiền 
            TinhTongTien();

        }

        private void btnRemoveCartItem_Click(object sender, RoutedEventArgs e) 
        {
            // Kiểm tra xem nhân viên đã chọn món nào trong ListBox giỏ hàng chưa
            CartItem monCầnXóa = lbCart.SelectedItem as CartItem;

            if (monCầnXóa == null)
            {
                MessageBox.Show("Vui lòng chọn một món trong giỏ hàng để xóa!", "Nhắc nhở", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Xóa món đó khỏi danh sách giỏ hàng
            _gioHang.Remove(monCầnXóa);

            // Tiền bị giảm đi, nên phải tính lại tổng tiền ngay lập tức
            TinhTongTien();
        }

        private void UpdateCartUI()
        {
            
        }

        private void btnApplyVoucher_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra giỏ hàng có món nào chưa
            if (_gioHang == null || _gioHang.Count == 0)
            {
                MessageBox.Show("Giỏ hàng đang trống, không thể áp dụng Voucher!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Lấy voucher đang được chọn trong ComboBox
            _appliedVoucher = cbVouchers.SelectedItem as VoucherItem;
            if (_appliedVoucher == null)
            {
                MessageBox.Show("Vui lòng chọn một mã Voucher!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Tính toán tiền ngay lập tức (Gọi lại hàm tính tổng tiền của bạn)
            TinhTongTien();
            MessageBox.Show($"Đã áp dụng mã {_appliedVoucher.Code} thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnCheckout_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra giỏ hàng
            if (_gioHang == null || _gioHang.Count == 0)
            {
                MessageBox.Show("Giỏ hàng đang trống! Vui lòng chọn món trước khi chốt đơn.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Thông tin cơ bản ban đầu
            string invoiceId = "HD" + DateTime.Now.ToString("yyMMddHHmmss");

            string storeId = this._maChiNhanh;  
            string staffName = this._tenNhanVien; // Lấy chính cái tên "manager" mà cửa sổ này đang giữ

            // Tính toán tiền 
            decimal tongTienGoc = _gioHang.Sum(x => x.DonGia * x.SoLuong);
            decimal tienGiam = 0;
            string maVoucher = null;

            if (_appliedVoucher != null)
            {
                maVoucher = _appliedVoucher.Code;
                tienGiam = tongTienGoc * ((decimal)_appliedVoucher.DiscountPercent / 100m);
                if (tienGiam > _appliedVoucher.MaxDiscount) tienGiam = _appliedVoucher.MaxDiscount;
            }
            decimal tienThucTra = tongTienGoc - tienGiam;

            // KẾT NỐI DATABASE VÀ CHỐT ĐƠN
            string connectionString = "Server=127.0.0.1; Database=SayTeaCoffee; Uid=root; Pwd=123456;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Ự ĐỘNG TRA CỨU TÊN CHI NHÁNH TỪ DATABASE
                string tenChiNhanh = "Chi nhánh không xác định"; // Tên mặc định nếu không tìm thấy
                string queryStoreName = "SELECT name FROM stores WHERE id = @sId";
                using (MySqlCommand cmdName = new MySqlCommand(queryStoreName, conn))
                {
                    cmdName.Parameters.AddWithValue("@sId", storeId);
                    object result = cmdName.ExecuteScalar();
                    if (result != null)
                    {
                        tenChiNhanh = result.ToString(); 
                    }
                }

                // BẮT ĐẦU TRANSACTION CHỐT ĐƠN 
                MySqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Lưu Hóa Đơn tổng
                    string sqlInvoice = @"INSERT INTO Invoices (invoice_id, store_id, staff_name, total_origin, voucher_code, discount_amount, final_total) 
                                  VALUES (@id, @store, @staff, @origin, @voucher, @discount, @final)";
                    using (MySqlCommand cmdInvoice = new MySqlCommand(sqlInvoice, conn, transaction))
                    {
                        cmdInvoice.Parameters.AddWithValue("@id", invoiceId);
                        cmdInvoice.Parameters.AddWithValue("@store", storeId);
                        cmdInvoice.Parameters.AddWithValue("@staff", staffName);
                        cmdInvoice.Parameters.AddWithValue("@origin", tongTienGoc);
                        cmdInvoice.Parameters.AddWithValue("@voucher", string.IsNullOrEmpty(maVoucher) ? (object)DBNull.Value : maVoucher);
                        cmdInvoice.Parameters.AddWithValue("@discount", tienGiam);
                        cmdInvoice.Parameters.AddWithValue("@final", tienThucTra);
                        cmdInvoice.ExecuteNonQuery();
                    }

                    // Lưu chi tiết & Trừ kho
                    string sqlDetail = @"INSERT INTO Invoice_Details (invoice_id, product_id, product_name, size, sugar, ice, quantity, unit_price, subtotal) 
                                 VALUES (@invId, @pId, @pName, @size, @sugar, @ice, @qty, @price, @sub)";
                    string sqlUpdateStock = @"UPDATE branch_inventory SET stock = stock - @qty WHERE store_id = @store AND product_id = @pId";

                    foreach (var item in _gioHang)
                    {
                        using (MySqlCommand cmdDetail = new MySqlCommand(sqlDetail, conn, transaction))
                        {
                            cmdDetail.Parameters.AddWithValue("@invId", invoiceId);
                            cmdDetail.Parameters.AddWithValue("@pId", item.MaMon);
                            cmdDetail.Parameters.AddWithValue("@pName", item.TenMon);
                            cmdDetail.Parameters.AddWithValue("@size", item.Size ?? "-");
                            cmdDetail.Parameters.AddWithValue("@sugar", item.Duong ?? "-");
                            cmdDetail.Parameters.AddWithValue("@ice", item.Da ?? "-");
                            cmdDetail.Parameters.AddWithValue("@qty", item.SoLuong);
                            cmdDetail.Parameters.AddWithValue("@price", item.DonGia);
                            cmdDetail.Parameters.AddWithValue("@sub", item.ThanhTien);
                            cmdDetail.ExecuteNonQuery();
                        }

                        using (MySqlCommand cmdStock = new MySqlCommand(sqlUpdateStock, conn, transaction))
                        {
                            cmdStock.Parameters.AddWithValue("@qty", item.SoLuong);
                            cmdStock.Parameters.AddWithValue("@store", storeId);
                            cmdStock.Parameters.AddWithValue("@pId", item.MaMon);
                            cmdStock.ExecuteNonQuery();
                        }
                    }

                    // Chốt Transaction
                    transaction.Commit();
                    MessageBox.Show("Chốt đơn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // MỞ CỬA SỔ HÓA ĐƠN
                    // Truyền ĐỦ 7 Tham số (Đã ép kiểu List và Double, và dùng biến tenChiNhanh tra cứu từ DB)
                    InvoiceWindow billWindow = new InvoiceWindow(
                        invoiceId,
                        tenChiNhanh,
                        staffName,
                        _gioHang.ToList(),
                        (double)tongTienGoc,
                        (double)tienGiam,
                        (double)tienThucTra
                    );
                    billWindow.ShowDialog();

                    // RESET GIAO DIỆN
                    _gioHang.Clear();
                    _appliedVoucher = null;
                    cbVouchers.SelectedIndex = -1;
                    TinhTongTien();
                    LoadPosMenu();

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Lỗi khi chốt đơn: " + ex.Message, "Lỗi Nghiêm Trọng", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnReport_Click(object sender, RoutedEventArgs e) 
        {
            ReportWindow reportWin = new ReportWindow(this._maChiNhanh);
            reportWin.ShowDialog();
            //ReportWindow reportWin = new ReportWindow();
            //reportWin.ShowDialog();
        }

        // HÀM ĐĂNG XUẤT CHO NHÂN VIÊN STAFF
        private void btnLogout_Click(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Bạn có muốn đăng xuất khỏi ca làm việc?", "Đăng xuất", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                // 1. Tạo và mở cửa sổ Đăng Nhập mới lên
                MainWindow loginWindow = new MainWindow();
                loginWindow.Show();

                // 2. Thu thập danh sách TẤT CẢ các cửa sổ đang mở (ngoại trừ cái loginWindow vừa mở)
                var cacCuaSoCu = System.Windows.Application.Current.Windows
                                    .Cast<Window>()
                                    .Where(w => w != loginWindow)
                                    .ToList();

                // 3. Đóng sạch sẽ toàn bộ cửa sổ cũ
                foreach (Window w in cacCuaSoCu)
                {
                    w.Close();
                }
                //SystemManager.Instance.CurrentUser = null; 
                //SystemManager.Instance.CurrentStoreId = null;

                //MainWindow login = new MainWindow();
                //login.Show(); // Bật lại màn hình Login
                //this.Close(); // Tắt POS đi
            }
        }
        private void TinhTongTien()
        {
            decimal tongTien = 0;

            // Duyệt qua tất cả các món đang có trong giỏ
            foreach (var item in _gioHang)
            {
                tongTien += item.ThanhTien; // ThanhTien = DonGia * SoLuong (đã định nghĩa ở Khuôn)
            }
            // Tính số tiền được giảm nếu có voucher
            decimal tienDuocGiam = 0;
            if (_appliedVoucher != null)
            {
                // Tính ra số tiền giảm: Tiền gốc * (% giảm / 100)
                // Ví dụ: Nhập 10% (Tức là số 10) thì phải chia 100. Còn nếu trong DB lưu là 0.1 thì khỏi chia.
                tienDuocGiam = tongTien * (_appliedVoucher.DiscountPercent / 100);

                // Kiểm tra xem tiền giảm có bị lố giới hạn tối đa không
                if (tienDuocGiam > _appliedVoucher.MaxDiscount)
                {
                    tienDuocGiam = _appliedVoucher.MaxDiscount;
                }
            }

            // Tính tiền khách cần trả
            decimal tienCanThanhToan = tongTien - tienDuocGiam;

            txtDiscountAmount.Text = $"Giảm giá: {tienDuocGiam:N0} đ";
            txtTotal.Text = $"CẦN THANH TOÁN: {tienCanThanhToan:N0} đ";
        }

        private void rbSizeS_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancelVoucher_Click(object sender, RoutedEventArgs e)
        {
            // Nếu vốn dĩ chưa áp dụng voucher nào thì khỏi cần làm gì
            if (_appliedVoucher == null)
            {
                return;
            }

            _appliedVoucher = null;

            // Trả cái ComboBox về trạng thái mặc định (không chọn dòng nào)
            cbVouchers.SelectedIndex = -1;

            // Gọi lại hàm tính tiền (Vì _appliedVoucher đã bằng null, hàm này sẽ tự động reset tiền giảm về 0)
            TinhTongTien();

            MessageBox.Show("Đã gỡ bỏ Voucher!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
  
    
}