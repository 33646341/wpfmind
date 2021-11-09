using HandyControl.Themes;
using HandyControl.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HandyControl.Controls;
using System.Windows.Data;
using System;
using System.Globalization;
using System.Windows.Controls.Primitives;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Size = System.Drawing.Size;
using Point = System.Drawing.Point;
using System.Linq;
using Pen = System.Drawing.Pen;
using Color = System.Drawing.Color;
using Mind;
//using RoundButton = System.Windows.Controls.Button;

namespace WpfMind
{
    public partial class MainWindow
    {
        System.Windows.Forms.PictureBox img;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
            
            // 画布控件初始化（winform版迁移）
            img = new System.Windows.Forms.PictureBox();
            img.Paint += img_Paint;
            pictureBoxHost.Child = img;

            // 加载一个模板
            content_tb.Text = DocHelper.readFromTemplate();

            // 属性窗口赋值
            DemoModel = new PropertyGridDemoModel
            {
                String = "TestString",
                Enum = Gender.Female,
                Boolean = true,
                Integer = 98,
                VerticalAlignment = VerticalAlignment.Stretch
            };

        }

        Action<Graphics> paint = (g) => { };//多播委托
        private int bottomOfLastLeaf; // 对于所有的叶子节点，保存它的底边y坐标，方便计算下一个叶子节点相对位置。
        float scale = 1; // 缩放比例

        // 绘制方法的入口
        void draw()
        {
            paint = null;// 刷新上一次的绘制
            bottomOfLastLeaf = -15; // 纵坐标初始点重置
            string content = content_tb.Text;

            try
            {
                img.Controls.Clear();
                var data = JsonConvert.DeserializeObject(content) as JArray;
                RoundButton sentinelNode = new RoundButton();
                sentinelNode.isRootTopic = true;
                //sentinelNode.Location = new Point((img.Width - sentinelNode.Width) / 2, (img.Height - sentinelNode.Width) / 2);
                //sentinelNode.Location = new Point(0, 0);
                sentinelNode.Left = (int)-sentinelNode.Width - 30; // 只看横坐标，纵坐标反正都要修正

                //img.SuspendLayout();
                img.Scale(new SizeF(1 / scale, 1 / scale));
                renderChildrenNodes(sentinelNode, data);
                img.Scale(new SizeF(scale, scale));
                //img.ResumeLayout();

                //img.Invalidate();
                img.Refresh();

                //img.SizeMode = PictureBoxSizeMode.Zoom;
                Console.WriteLine(img.ClientRectangle.Size);
            }
            catch { }
        }

        private void renderChildrenNodes(RoundButton parentNode, JArray childrenNodes)
        {
            if (parentNode.isRootTopic)// 根节点前的初始辅助节点 做一点修正
            {
                var obj = childrenNodes[0] as JObject;
                var rootTopic = obj["rootTopic"] as JObject;
                NewMethod(parentNode, 0, rootTopic);
                //Console.WriteLine(obj["rootTopic"]["title"]);
            }
            else
            {
                int child_index = 0;
                int area_top = 0;
                foreach (var node in childrenNodes)
                {
                    // 遍历到第一个孩子前，记录区域上界
                    if (child_index == 0) area_top = bottomOfLastLeaf + 15;

                    NewMethod(parentNode, child_index, node);
                    child_index += 1;

                    // 遍历到最后一个孩子后，修正双亲节点的纵坐标位置（垂直居中）
                    if (child_index == childrenNodes.Count)
                    {
                        parentNode.Top = (int)((area_top + bottomOfLastLeaf) / 2 - parentNode.Height / 2);
                    }

                }
            }
        }

        bool ctrlPressed; // 是否按下ctrl键

        private void NewMethod(RoundButton parentNode, int child_index, JToken node)
        {
            RoundButton newNode = new RoundButton();
            newNode.Jref = (JObject)node;
            ////添加上下文菜单
            //NewMenustrip(newNode);

            // 绘制形状
            img.Controls.Add(newNode);
            newNode.Size = new Size(153, 60);
            //newNode.Location = new Point(parentNode.rightPoint.X + 30, (img.Height - newNode.Width) / 2 + (newNode.Height + 10) * child_index);
            newNode.Location = new Point(parentNode.rightPoint.X + 30, (newNode.Height + 10) * child_index);

            if (bottomOfLastLeaf > 0)
            {
                newNode.Top = bottomOfLastLeaf + 15;
            }
            else
            {
                newNode.Top = 0;
            }

            //newNode.Font = new Font("微软雅黑", 15.73109F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(134)));
            newNode.NormalColor = Color.White;
            newNode._baseColor = _baseColor;
            newNode.PressedColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            newNode.HoverColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            newNode.Text = node["title"].ToString();//按钮文本
            //newNode.Radius = 15;
            // 绘制结束

            // 拖拽事件
            newNode.AllowDrop = true;
            newNode.MouseDown += (s, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left && e.Clicks == 1)
                {
                    var b = s as RoundButton; b.DoDragDrop(b, (System.Windows.Forms.DragDropEffects)DragDropEffects.All);
                }
            };
            newNode.DragEnter += (s, e) =>
            {
                var target = s as RoundButton;
                var source = e.Data.GetData(typeof(RoundButton)) as RoundButton;
                if (target.Jref.AncestorsAndSelf().Where(jo => jo == source.Jref).Count() == 0)// 禁止拖拽到自身或者自身的后代
                    e.Effect = (System.Windows.Forms.DragDropEffects)DragDropEffects.Move;
            };
            newNode.DragDrop += (s, e) =>
            {
                var target = s as RoundButton;
                var source = e.Data.GetData(typeof(RoundButton)) as RoundButton;
                //MessageBox.Show($"把`{source.Text}`放置到`{target.Text}`的孩子节点");
                if (!target.Jref.ContainsKey("children"))
                {
                    target.Jref.Add(new JProperty("children", new JObject(new JProperty("attached", new JArray()))));
                }
                var c = target.Jref.SelectToken("children.attached") as JArray;
                c.Add(source.Jref);
                source.Jref.Remove();
                content_tb.Text = JsonConvert.SerializeObject(target.Jref.Root, Formatting.Indented);

            };
            // 拖拽结束

            // 缩放事件
            newNode.KeyDown += (_, args) =>
            {

                ctrlPressed = args.Control;


            };
            // 缩放结束
            //tab/enter键的处理
            newNode.KeyDown += (_, args) =>
            {
                //Btn_KeyPress(_, args);
            };

            // 重命名事件
            newNode.KeyPress += (s, e) =>
            {
                var target = s as RoundButton;
                //title_tb.Text = target.Text;
                //title_tb.Visible = true;

            };
            // 重命名结束

            // 画父子节点间的连线
            if (!parentNode.isRootTopic)// 根节点前的初始辅助节点到根节点 不需要画线
            {
                paint += (g) =>
                {
                    Point pp1 = parentNode.rightPoint;   // Start point
                    Point cc1 = parentNode.rightPoint + new Size(15, 0);   // First control point
                    Point cc2 = newNode.leftPoint + new Size(-15, 0);  // Second control point
                    Point pp2 = newNode.leftPoint;  // Endpoint
                    Pen penpen = new Pen(Color.FromArgb(255, 0, 0, 255), 3);
                    g.DrawBezier(penpen, pp1, cc1, cc2, pp2);
                };
            }

            newNode.Focus();
            //tempbtn = newNode;
            if (!isLeaf((JObject)node))
            {
                var ccc = node["children"]["attached"] as JArray;//遍历子节点
                ///套娃开始
                renderChildrenNodes(newNode, ccc);
                ///套娃结束
            }
            else
            {
                // 对于所有的叶子节点，保存它的底边y坐标，方便计算下一个叶子节点相对位置。
                bottomOfLastLeaf = newNode.Bottom;
            }
        }
        bool isLeaf(JObject node) => node.SelectToken("children.attached[0]") == null;

        private void img_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;  //图片柔顺模式选择
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;//高质量
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;//再加一点
            if (paint != null) paint(g);
        }

        private void btnClickMe_Click(object sender, RoutedEventArgs e)
        {
            draw();
            //lbResult.Items.Add(pnlMain.FindResource("strPanel").ToString());
            //lbResult.Items.Add("hi");

            //lbResult.Items.Add(this.FindResource("strWindow").ToString());
            //lbResult.Items.Add(Application.Current.FindResource("strApp").ToString());
        }

        #region Change Theme
        private void ButtonConfig_OnClick(object sender, RoutedEventArgs e) => PopupConfig.IsOpen = true;

        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button)
            {
                PopupConfig.IsOpen = false;
                if (button.Tag is ApplicationTheme tag)
                {
                    ((App)Application.Current).UpdateTheme(tag);

                }
                else if (button.Tag is System.Windows.Media.Brush accentTag)
                {
                    ((App)Application.Current).UpdateAccent(accentTag);

                    ///
                    if (accentTag is SolidColorBrush br)
                    {
                        var c = br.Color;
                        _baseColor = Color.FromArgb(c.A, c.R, c.G, c.B);
                    }
                    else if (accentTag is LinearGradientBrush b)
                    {
                        var c = b.GradientStops[0].Color;
                        _baseColor = Color.FromArgb(c.A, c.R, c.G, c.B);
                    }

                    ///

                }
                else if (button.Tag is "Picker")
                {
                    var picker = SingleOpenHelper.CreateControl<ColorPicker>();
                    var window = new PopupWindow
                    {
                        PopupElement = picker,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        AllowsTransparency = true,
                        WindowStyle = WindowStyle.None,
                        MinWidth = 0,
                        MinHeight = 0,
                        Title = "Select Accent Color"
                    };

                    picker.SelectedColorChanged += delegate
                    {
                        ((App)Application.Current).UpdateAccent(picker.SelectedBrush);
                        ///
                        var c = picker.SelectedBrush.Color;
                        _baseColor = Color.FromArgb(c.A, c.R, c.G, c.B);
                        ///

                        window.Close();
                    };
                    picker.Canceled += delegate { window.Close(); };
                    window.Show();
                }
            }

            img.Visible = false;
            img.Visible = true;
            draw();

        }
        #endregion

        Color _baseColor;
        private void btn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //var s = (ToggleButton)sender;
            //if ((bool)s.IsChecked)
            //{

            //}
        }

        private void btn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var s = (ToggleButton)sender;
            //if ((bool)s.IsChecked)
            //{
            textvalue.Text = text_tb.Text;
            textvalue.Visibility = Visibility.Visible;
            text_tb.Visibility = Visibility.Collapsed;
            s.IsChecked = false;
            //}
        }

        private void btn_Checked(object sender, RoutedEventArgs e)
        {
            var s = (ToggleButton)sender;
            Console.WriteLine(s.IsChecked);
            textvalue.Visibility = Visibility.Collapsed;
            text_tb.Text = textvalue.Text;
            text_tb.Visibility = Visibility.Visible;
        }

        private void content_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (img != null)
                draw();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            pictureBoxHost.Margin = new Thickness(300, 0, (bool)prop_btn.IsChecked ? 300 : 0, 0);
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            pictureBoxHost.Margin = new Thickness(0, 0, (bool)prop_btn.IsChecked ? 300 : 0, 0);
        }

        private void ToggleButton_Checked_1(object sender, RoutedEventArgs e)
        {
            pictureBoxHost.Margin = new Thickness(0, 0, 300, 0);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            content_tb.Text = DocHelper.read();
        }

        public static readonly DependencyProperty DemoModelProperty = DependencyProperty.Register(
    "DemoModel", typeof(PropertyGridDemoModel), typeof(MainWindow), new PropertyMetadata(default(PropertyGridDemoModel)));

        public PropertyGridDemoModel DemoModel
        {
            get => (PropertyGridDemoModel)GetValue(DemoModelProperty);
            set => SetValue(DemoModelProperty, value);
        }
    }
}
