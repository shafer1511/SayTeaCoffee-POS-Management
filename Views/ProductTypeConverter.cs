using System;
using System.Globalization;
using System.Windows.Data;
using TraSuaApp.Models;

namespace TraSuaApp.Views
{
    public class ProductTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Ép kiểu giá trị nhận được thành chuỗi (string)
            string type = value as string;

            // So sánh chữ xem nó là loại nào để ghép icon vào
            if (type == "Nước Uống")
            {
                return "🍹 Nước Uống";
            }
            else if (type == "Topping")
            {
                return "🍨 Topping";
            }

            // Nếu không khớp cái nào ở trên thì mới ra Khác
            return "❓ Khác";

            //if (value is Product product)
            //{
            //    if (value is Drink)
            //        return "🍹 Nước Uống";
            //    else if (value is Topping)
            //        return "🍨 Topping";
            //}
            //return "❓ Khác";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
