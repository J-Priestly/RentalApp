using System.Globalization;

namespace RentalApp.Converters;


// Converts the numeric rating (1–5) to a Unicode star string

public class RatingToStarsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var rating = value switch
        {
            int i    => i,
            double d => (int)Math.Round(d),
            float f  => (int)Math.Round(f),
            _        => 0
        };

        rating = Math.Clamp(rating, 0, 5);
        return new string('★', rating) + new string('☆', 5 - rating);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
