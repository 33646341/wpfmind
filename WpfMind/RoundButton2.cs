using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfMind
{
    public partial class RoundButton : System.Windows.Forms.Button
    {
        public Point leftPoint { get => new Point(Left, (int)(Top + Height / 2)); set => Console.WriteLine("hihihihi"); }
        public Point rightPoint { get => new Point((int)(Margin.Left + Width), (int)(Margin.Top + Height / 2)); set => Console.WriteLine("hihihihi"); }
        public bool isRootTopic { get; set; }
        public JObject Jref { get; set; }
        //public int Left { get => (int)Margin.Left; set => Canvas.SetLeft(this,value); }
        //public int Top { get => (int)Margin.Top; set => Canvas.SetTop(this, value); }
        //public Size Size { get; internal set; }
        //public Point Location { get; internal set; }

        public RoundButton()
        {

        }
    }
}
