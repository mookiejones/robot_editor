using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace robot_editor.Interfaces
{
    public interface IImage
    {
        ImageSource ImageSource { get; }
        BitmapImage Bitmap { get; }
        Icon Icon { get; }
    }
}
