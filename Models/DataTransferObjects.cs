using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraSuaApp.Models
{
    public class StoreItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class EmployeeItem
    {
        public string EmpId { get; set; }
        public string Name { get; set; }
        public string Shift { get; set; }
        public string WorkTime { get; set; }
        public string JobRole { get; set; }
        public string PresentStatus { get; set; } // Hiển thị "Đã chấm công" hoặc "Chưa chấm công"
    }
    public class ProductItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal BasePrice { get; set; }    // Sẽ hứng dữ liệu từ cột base_price
        public string Type { get; set; }    // Sẽ hứng dữ liệu từ cột type
    }
    public class PosBranchItem
    {
        public string BranchId { get; set; }   // Để ngầm lưu S01, S02
        public string BranchName { get; set; } // Để hiển thị lên màn hình
    }
    public class VoucherItem
    {
        public string Code { get; set; }
        public decimal DiscountPercent { get; set; }
        public int MaxDiscount { get; set; }
        public DateTime ExpiryDate { get; set; }

        public string DisplayText => $"{Code} - Giảm {DiscountPercent}% (Tối đa {MaxDiscount:N0}đ)";
    }

    public class AdminDetailedInvoiceRow
    {
        public string InvoiceId { get; set; } = "";
        public string StoreId { get; set; } = "";
        public string StaffName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int TotalOrigin { get; set; }
        public string VoucherCode { get; set; } = "";
        public int DiscountAmount { get; set; }
        public int FinalTotal { get; set; }
    }
    public class SupportReportItem
    {
        public int Id { get; set; }
        public string StoreId { get; set; }
        public string ReportType { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }

        // Biến phụ để hiển thị chữ "Đã xử lý" / "Chưa xử lý" lên giao diện cho đẹp
        public string StatusText => IsResolved ? "Đã xử lý" : "Chưa xử lý";
    }
    public class PosProductItem
    {
        public string DisplayName { get; set; }
        public string Id { get; set; }
        public decimal BasePrice { get; set; }
        public int Stock { get; set; }
    }
}
