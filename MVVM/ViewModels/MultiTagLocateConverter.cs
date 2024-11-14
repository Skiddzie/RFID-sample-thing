using System;
using System.Globalization;

namespace ZebraRFIDXamarinDemo.ViewModels
{
    public class MultiTagLocateConverter : IValueConverter
    {
        float[][] colors = new float[4][];

        public MultiTagLocateConverter()
        {
            colors[0] = new float[] { 0, 0, 1 };
            colors[1] = new float[] { 0, 1, 0 };
            colors[2] = new float[] { 1, 1, 0 };
            colors[3] = new float[] { 1, 0, 0 };
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int svalue = (int)value;

            if (targetType == typeof(Color))
            {
                int i1 = 0;
                int i2 = 0;
                float fValue = (float)svalue * 3 / 100;
                i1 = (int)Math.Floor(fValue);
                i2 = i1 + 1;
                if (svalue == 100)
                    i1 = i2 = 3;
                float f1 = fValue - (float)i1;

                float r = (colors[i2][0] - colors[i1][0]) * f1 + colors[i1][0];
                float g = (colors[i2][1] - colors[i1][1]) * f1 + colors[i1][1];
                float b = (colors[i2][2] - colors[i1][2]) * f1 + colors[i1][2];

                var c = Color.FromRgb(r, g, b);
                return c;// Color.Accent;
            }
            else if (targetType == typeof(Rect))
                return new Rect(0, 0, (float)svalue / 100, 1);
            else
                throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
