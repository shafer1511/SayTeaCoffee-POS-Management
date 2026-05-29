using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using TraSuaApp.Models;

namespace TraSuaApp.Views
{
    public partial class BranchSalesDashboardWindow : Window
    {
        private readonly IReadOnlyList<AdminDetailedInvoiceRow> _rows;

        public BranchSalesDashboardWindow(
            string storeId,
            string storeName,
            DateTime dateFrom,
            DateTime dateTo,
            IReadOnlyList<AdminDetailedInvoiceRow> rows)
        {
            InitializeComponent();
            _rows = rows ?? Array.Empty<AdminDetailedInvoiceRow>();

            Title = $"Báo cáo bán hàng — {storeName} ({storeId})";
            txtBranchDashboardTitle.Text = $"{storeName}  ({storeId})";
            txtBranchDashboardPeriod.Text = $"Kỳ báo cáo: từ {dateFrom:dd/MM/yyyy} đến {dateTo:dd/MM/yyyy}";

            int n = _rows.Count;
            long sumOrigin = 0, sumDiscount = 0, sumNet = 0;
            foreach (var r in _rows)
            {
                sumOrigin += r.TotalOrigin;
                sumDiscount += r.DiscountAmount;
                sumNet += r.FinalTotal;
            }

            txtDashKpiOrders.Text = n.ToString("N0", CultureInfo.InvariantCulture);
            txtDashKpiNet.Text = sumNet.ToString("N0");
            txtDashKpiDiscount.Text = sumDiscount.ToString("N0");
            txtDashKpiAvg.Text = n > 0 ? (sumNet / (double)n).ToString("N0") : "0";
            txtDashKpiGrossLine.Text = $"Tổng tiền gốc (trước giảm giá): {sumOrigin:N0} đ";

            dgInvoices.ItemsSource = _rows.ToList();
        }

        private void btnExportCsv_Click(object sender, RoutedEventArgs e)
        {
            if (_rows.Count == 0)
            {
                MessageBox.Show("Không có hóa đơn để export.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
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
                foreach (var r in _rows)
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
    }
}
