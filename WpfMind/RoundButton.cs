using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
namespace Mind
{
    public interface MindNode
    {
        Point leftPoint { get; set; }
        Point rightPoint { get; set; }
        bool isRootTopic { get; set; }
         JObject Jref { get; set; }
    }
    public interface NodeTextBox
    {
        void btnTextBox_KeyDown(object sender, KeyEventArgs e);
        Font myFont { get; set; }
    }
    public enum ControlState { Hover, Normal, Pressed }
    public class RoundButton : Button, MindNode, NodeTextBox
    {
        private int radius;//半径 
        public TextBox btnTextBox = new TextBox();
        public Color _baseColor = Color.FromArgb(51, 161, 224);//基颜色
        private Color _hoverColor = Color.FromArgb(51, 0, 224);//基颜色
        private Color _normalColor = Color.FromArgb(0, 161, 224);//基颜色
        private Color _pressedColor = Color.FromArgb(51, 161, 0);//基颜色
        //圆形按钮的半径属性
        [CategoryAttribute("布局"), BrowsableAttribute(true), ReadOnlyAttribute(false)]
        public int Radius
        {
            set
            {
                radius = value;
                this.Invalidate();
            }
            get
            {
                return radius;
            }
        }
        [DefaultValue(typeof(Color), "51, 161, 224")]
        public Color NormalColor
        {
            get
            {
                return this._normalColor;
            }
            set
            {
                this._normalColor = value;
                this.Invalidate();
            }
        }
        //  [DefaultValue(typeof(Color), "220, 80, 80")]
        public Color HoverColor
        {
            get
            {
                return this._hoverColor;
            }
            set
            {
                this._hoverColor = value;
                this.Invalidate();
            }
        }

        //  [DefaultValue(typeof(Color), "251, 161, 0")]
        public Color PressedColor
        {
            get
            {
                return this._pressedColor;
            }
            set
            {
                this._pressedColor = value;
                this.Invalidate();
            }
        }
        public ControlState ControlState { get; set; }
        public Point leftPoint { get => new Point(Left, Top + Height / 2); set => Console.WriteLine("hihihihi"); }
        public Point rightPoint { get => new Point(Left + Width, Top + Height / 2); set => Console.WriteLine("hihihihi"); }
        public bool isRootTopic { get; set; }
        public JObject Jref { get; set; }
        public Font myFont { get; set; }

        protected override void OnMouseEnter(EventArgs e)//鼠标进入时
        {
            base.OnMouseEnter(e);
            ControlState = ControlState.Hover;//正常
        }
        protected override void OnMouseLeave(EventArgs e)//鼠标离开
        {
            base.OnMouseLeave(e);
            ControlState = ControlState.Normal;//正常
        }
        protected override void OnMouseDown(MouseEventArgs e)//鼠标按下
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && e.Clicks == 1)//鼠标左键且点击次数为1
            {
                ControlState = ControlState.Pressed;//按下的状态
            }
            if (e.Button == MouseButtons.Left && e.Clicks == 2)//鼠标左键且点击次数为2
            {
                //提供对textbox的监控事件
                ControlState = ControlState.Pressed;//按下的状态
                btnTextBox.Text =this.Text;
                btnTextBox.Visible = true;
                btnTextBox.BackColor = this._normalColor;
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)//鼠标弹起
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left && e.Clicks == 1)
            {
                if (ClientRectangle.Contains(e.Location))//控件区域包含鼠标的位置
                {
                    ControlState = ControlState.Hover;
                    this.Invalidate();
                }
                else
                {
                    ControlState = ControlState.Normal;
                }
            }
        }
        public RoundButton()
        {
            Radius = 15;

            //初始化控件时，就自带一个隐藏的文本框
            btnTextBox.Size = new Size(153, 60);
            this.Controls.Add(btnTextBox);
            btnTextBox.Tag =this;
            btnTextBox.ImeMode = ImeMode.NoControl;
            btnTextBox.Visible = false;
            btnTextBox.Multiline = true;
            btnTextBox.Font = this.myFont;
            btnTextBox.Text = this.Text;
            //btnTextBox.KeyDown += new KeyEventHandler(btnTextBox_KeyDown);

            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.ControlState = ControlState.Normal;
            this.SetStyle(
             ControlStyles.UserPaint |  //控件自行绘制，而不使用操作系统的绘制
             ControlStyles.AllPaintingInWmPaint | //忽略擦出的消息，减少闪烁。
             ControlStyles.OptimizedDoubleBuffer |//在缓冲区上绘制，不直接绘制到屏幕上，减少闪烁。
             ControlStyles.ResizeRedraw | //控件大小发生变化时，重绘。                  
             ControlStyles.SupportsTransparentBackColor, true);//支持透明背景颜色
        }

        private Color GetColor(Color colorBase, int a, int r, int g, int b)
        {
            int a0 = colorBase.A;
            int r0 = colorBase.R;
            int g0 = colorBase.G;
            int b0 = colorBase.B;
            if (a + a0 > 255) { a = 255; } else { a = Math.Max(a + a0, 0); }
            if (r + r0 > 255) { r = 255; } else { r = Math.Max(r + r0, 0); }
            if (g + g0 > 255) { g = 255; } else { g = Math.Max(g + g0, 0); }
            if (b + b0 > 255) { b = 255; } else { b = Math.Max(b + b0, 0); }

            return Color.FromArgb(a, r, g, b);
        }

        //重写OnPaint
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);
            base.OnPaintBackground(e);
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;

            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            var path = GetRoundedRectPath(rect, radius);

            this.Region = new Region(path);

            Color baseColor;
            //Color borderColor;
            //Color innerBorderColor = this._baseColor;//Color.FromArgb(200, 255, 255, 255); ;

            switch (ControlState)
            {
                case ControlState.Hover:
                    baseColor = this.HoverColor;
                    break;
                case ControlState.Pressed:
                    baseColor = this.PressedColor;
                    break;
                case ControlState.Normal:
                    baseColor = this.NormalColor;
                    break;
                default:
                    baseColor = this.NormalColor;
                    break;
            }

            using (SolidBrush b = new SolidBrush(baseColor))
            {
                // 外框色
                if (Focused)
                {
                    //e.Graphics.FillPath(b, path);
                    e.Graphics.FillPath(new SolidBrush(Color.FromArgb(255, 46, 189, 255)), path);
                }
                else
                {
                    e.Graphics.FillPath(b, path);
                }

                // 内填充色
                //e.Graphics.FillPath(new SolidBrush(Color.Azure), GetRoundedRectPath(new Rectangle(3, 3, Width - 6, Height - 6), radius-4));
                e.Graphics.FillPath(new SolidBrush(_baseColor), GetRoundedRectPath(new Rectangle(3, 3, Width - 6, Height - 6), radius-4));


                //Font fo = new Font("宋体", 10.5F); 当然是采用属性设置的字体
                Brush brush = new SolidBrush(this.ForeColor);
                StringFormat gs = new StringFormat();
                gs.Alignment = StringAlignment.Center; //居中
                gs.LineAlignment = StringAlignment.Center;//垂直居中
                e.Graphics.DrawString(this.Text, Font, brush, rect, gs);
                //  e.Graphics.DrawPath(p, path);
            }
        }
        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {

            int diameter = radius;
            Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));
            GraphicsPath path = new GraphicsPath();
            path.AddArc(arcRect, 180, 90);
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
        }


        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Tab)
            {
                OnKeyDown(new KeyEventArgs(Keys.Tab));
                return false;
            }
            else if (keyData == Keys.Enter)
            {
                return false;
            }
            return base.ProcessDialogKey(keyData);
        }

        public void btnTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                this.Text = btnTextBox.Text;
                btnTextBox.Visible = false;
                //更改json文件内容
                this.Jref["title"] = this.Text;
            }
        }
    }
}
