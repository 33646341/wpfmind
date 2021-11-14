using HandyControl.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using Color = System.Drawing.Color;
using FontFamily = System.Drawing.FontFamily;

namespace WpfMind
{
    public delegate void ModelChanged(object sender);
    public class PropertyGridDemoModel
    {
        public event ModelChanged OnChanged;
        private int radius;
        private Family fontfamily;
        private int fontsize;
        private System.Drawing.FontStyle fontstyle;
        private ModelColor color;
        [Category("样式")]
        public int 圆角半径 {
            get { return radius; }
            set
            {
                Console.WriteLine("圆角半径："+value.ToString());
                radius = value;
                if (OnChanged != null)
                {
                    OnChanged(this);
                }
            }
        }
        [Category("文本")]
        public Family 字体
        {
            get { return fontfamily; }
            set { fontfamily = value;
                if (OnChanged != null)
                {
                    OnChanged(this);
                }
            }
        }
        public int 大小
        {
            get { return fontsize; }
            set
            {
                fontsize = value;
                if (OnChanged != null)
                {
                    OnChanged(this);
                }
            }
        }
        public System.Drawing.FontStyle 字体样式
        {
            get { return fontstyle; }
            set { fontstyle = value;
                if (OnChanged != null)
                {
                    OnChanged(this);
                }
            }
        }
        public ModelColor 颜色
        {
            get { return color; }
            set { color = value;
                if (OnChanged != null)
                {
                    OnChanged(this);
                }
            }
        }
    }
    public enum Family
    {
        等线,
        方正粗黑宋简,
        方正舒体,
        方正姚体,
        仿宋,
        汉仪蝶语体简,
        黑体,
        华文彩云,
        华文仿宋,
        华文琥珀,
        华文楷体,
        华文隶书,
        华文宋体,
        华文细黑,
        华文新魏,
        华文行楷,
        华文中宋,
        楷体,
        隶书,
        微软雅黑,
        新宋,
        幼圆,
        宋体
    }
    public enum ModelColor
    {
        AliceBlue,
        AntiqueWhite,
        Aqua,
        Aquamarine,
        Azure,
        Beige,
        Bisque,
        Black,
        BlanchedAlmond,
        Blue,
        BlueViolet,
        Brown,
        BurlyWood,
        CadetBlue,
        Chartreuse,
        Chocolate,
        Coral,
        CornflowerBlue,
        Cornsilk,
        Crimson,
        Cyan,
        DarkBlue,
        DarkCyan,
        DarkGoldenrod,
        DarkGray,
        DarkGreen,
        DarkKhaki,
        DarkMagenta,
        DarkOliveGreen,
        DarkOrange,
        DarkOrchid,
        DarkRed,
        DarkSalmon,
        DarkSeaGreen,
        DarkSlateBlue,
        DarkSlateGray,
        DarkTurquoise,
        DarkViolet,
        DeepPink,
        DeepSkyBlue,
        DimGray,
        DodgerBlue,
        Firebrick,
        FloralWhite,
        ForestGreen,
        Fuchsia,
        Gainsboro,
        GhostWhite,
        Gold,
        Goldenrod,
        Gray,
        Green,
        GreenYellow,
        Honeydew,
        HotPink,
        IndianRed,
        Indigo,
        Ivory,
        Khaki,
        Lavender,
        LavenderBlush,
        LawnGreen,
        LemonChiffon,
        LightBlue,
        LightCoral,
        LightCyan,
        LightGoldenrodYellow,
        LightGray,
        LightGreen,
        LightPink,
        LightSalmon,
        LightSeaGreen,
        LightSkyBlue,
        LightSlateGray,
        LightSteelBlue,
        LightYellow,
        Lime,
        LimeGreen,
        Linen,
        Magenta,
        Maroon,
        MediumPurple,
        MediumAquamarine,
        MediumBlue,
        MediumOrchid,
        MediumSeaGreen,
        MediumSlateBlue,
        MediumSpringGreen,
        MediumTurquoise,
        MediumVioletRed,
        MidnightBlue,
        MintCream,
        MistyRose,
        Moccasin,
        NavajoWhite,
        Navy,
        OldLace,
        Olive,
        OliveDrab,
        Orange,
        OrangeRed,
        Orchid,
        PaleGoldenrod,
        PaleGreen,
        PaleTurquoise,
        PaleVioletRed,
        PapayaWhip,
        PeachPuff,
        Peru,
        Pink,
        Plum,
        PowderBlue,
        Purple,
        Red,
        RosyBrown,
        RoyalBlue,
        SaddleBrown,
        Salmon,
        SandyBrown,
        SeaGreen,
        SeaShell,
        Sienna,
        Silver,
        SkyBlue,
        SlateBlue,
        SlateGray,
        Snow,
        SpringGreen,
        SteelBlue,
        Tan,
        Teal,
        Thistle,
        Tomato,
        Transparent,
        Turquoise,
        Violet,
        Wheat,
        White,
        WhiteSmoke,
        Yellow,
        YellowGreen
    }
}