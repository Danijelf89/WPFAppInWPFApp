using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WpfAppAITest.Models
{
    public class ScreenModel
    {
        public Screen Scrren { get; set; }

        public string Name { get; set; }

        public int Index { get; set; }

        public BitmapSource Image { get; set; }
    }
}
