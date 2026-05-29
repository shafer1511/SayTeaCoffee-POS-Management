namespace TraSuaApp.Models
{
    public class CartItem
    {
        public string MaMon { get; set; }
        public string TenMon { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }

        public string Size { get; set; }
        public string Duong { get; set; }
        public string Da { get; set; }

        public decimal ThanhTien => DonGia * SoLuong;

        public string DisplayText
        {
            get
            {
                string text = $"{TenMon} (x{SoLuong}) - {ThanhTien:N0}đ";
                if (!string.IsNullOrEmpty(Size))
                {
                    text += $"\n   [Size {Size} | {Duong} Đường | {Da} Đá]";
                }
                return text;
            }
        }
    }
}
