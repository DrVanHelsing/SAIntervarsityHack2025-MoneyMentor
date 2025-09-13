using System.Globalization;

namespace FinanceBuddy.Converters;

public class CategoryToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int categoryId)
        {
            return categoryId switch
            {
                1 => Color.FromArgb("#FF6B6B"), // Transport - Red
                2 => Color.FromArgb("#4ECDC4"), // Food - Teal  
                3 => Color.FromArgb("#45B7D1"), // Health - Blue
                4 => Color.FromArgb("#96CEB4"), // Entertainment - Green
                5 => Color.FromArgb("#FFEAA7"), // Utilities - Yellow
                _ => Color.FromArgb("#DDA0DD")  // Default - Plum
            };
        }
        return Color.FromArgb("#512BD4"); // Fallback to Primary
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CategoryToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int categoryId)
        {
            return categoryId switch
            {
                1 => "??", // Transport
                2 => "??", // Food
                3 => "??", // Health
                4 => "??", // Entertainment  
                5 => "?", // Utilities
                _ => "??"  // Default
            };
        }
        return "??";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}