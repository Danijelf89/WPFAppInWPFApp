using MaterialDesignThemes.Wpf;
using System.Globalization;
using System.Windows.Data;

namespace WpfAppAITest.Converters
{
    public class BoolToMicrophoneIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isRecording)
            {
                return isRecording ? PackIconKind.MicrophoneOff : PackIconKind.Microphone;
            }
            return PackIconKind.Microphone;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
