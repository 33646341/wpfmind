using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace WpfMind
{
    public class PropertyGridDemoModel
    {
        [Category("Category1")]
        public string String { get; set; }

        [Category("Category2")]
        public int 圆角半径 { get; set; }

        [Category("Category2")]
        public bool Boolean { get; set; }

        [Category("Category1")]
        public Gender Enum { get; set; }

        public HorizontalAlignment HorizontalAlignment { get; set; }

        public VerticalAlignment VerticalAlignment { get; set; }

        public ImageSource ImageSource { get; set; }
    }

    public enum Gender
    {
        Male,
        Female
    }
}