using System;
using System.Collections.Generic;
using System.Windows;
using TraSuaApp.Models;

namespace TraSuaApp.Views
{
    public partial class InvoiceWindow : Window
    {
        public InvoiceWindow(string maBill, string chiNhanh, string thuNgan, List<CartItem> danhSachMon, double tongTienGoc, double tienGiam, double tienThucTra)
        {
            InitializeComponent();

            // Gán thông tin Header
            txtStore.Text = $"Chi nhánh: {chiNhanh} | Số Bill: {maBill}";
            txtCashier.Text = $"Thu ngân: {thuNgan}";
            txtDate.Text = $"Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";

            // Gán thông tin Footer (Tiền bạc)

             txtTotal.Text = $"{tongTienGoc:N0} đ"; 
             txtDiscount.Text = $"-{tienGiam:N0} đ";
             txtFinal.Text = $"{tienThucTra:N0} đ";

             lvInvoice.ItemsSource = danhSachMon; 
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show("Đã kết nối máy in! Hóa đơn đang được in...", "Thành công");
            this.Close(); 
        }
    }
}