using HandyControl.Controls;
using HandyControl.Themes;
using HandyControl.Tools;
using Mind;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Application = System.Windows.Application;
using Color = System.Drawing.Color;
using DragDropEffects = System.Windows.DragDropEffects;
using FontFamily = System.Drawing.FontFamily;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using TextBox = HandyControl.Controls.TextBox;
using TreeNode = System.Windows.Forms.TreeNode;
//using RoundButton = System.Windows.Controls.Button;

namespace WpfMind
{

    public partial class MainWindow
    {
   
        private System.Windows.Forms.PictureBox img;
        private Point lastLocation; // 画布上次被放置的位置

        public MainWindow()
        {
            
            InitializeComponent();

            this.DataContext = this;
            // 画布控件初始化（winform版迁移）
            img = new System.Windows.Forms.PictureBox();
            img.Paint += img_Paint;
            var imgPanel = new System.Windows.Forms.Panel();
            pictureBoxHost.Child = imgPanel;
            imgPanel.Controls.Add(img);
            img.Width = 3000;
            img.Height = 3000;

            //当前选中节点赋初值null
            //H，2021/11/14/11:48
            myJref = new MyJref
            {
                tempJref = null
            };

            myJref.OnMyChange += new TmpChanged(JrefChanged);
            // 加载一个模板
            content_tb.Text = DocHelper.readFromTemplate();

            // 自动居中对齐
            Show();
            toCenter_Click(this, new RoutedEventArgs());

            // 面板赋值，设置监听事件
            //H，2021/11/14/11:48
            DemoModel = new PropertyGridDemoModel
            {
                圆角半径 = 15,
                字体 = Family.微软雅黑,
                大小 = 9,
                字体样式 = System.Drawing.FontStyle.Regular,
                颜色 = ModelColor.Black
            };

            //    DemoModel.OnChanged += new ModelChanged(model_OnChanged);
            //InitializeModel(crtModel);
            crtModel = new PropertyGridDemoModel
            {
                圆角半径 = 25,
                字体 = Family.微软雅黑,
                大小 = 9,
                字体样式 = System.Drawing.FontStyle.Regular,
                颜色 = ModelColor.Black
            };
            CopyStyle(crtModel, DemoModel);


            img.MouseWheel += (s, args) =>
            {
                if (ctrlPressed)
                {
                    float scale_ori = scale;
                    scale += ((float)args.Delta) / 1200;
                    if (scale < 0.1 || scale > 5) scale = scale_ori;
                    draw();
                }
            };

            // 缩放事件
            img.KeyDown += (_, args) => ctrlPressed = args.Control;
            img.KeyUp += (_, args) =>
            {
                ctrlPressed = args.Control;
                Console.WriteLine($"keyup: ctrlPressed={ctrlPressed}");
            };
            // 缩放结束

            // 拖放事件
            System.Windows.Forms.MouseEventHandler drag = (s, args) =>
            {
                var si = new Size(args.X + img.Left - lastLocation.X, args.Y + img.Top - lastLocation.Y);
                lastLocation = new Point(args.X + img.Left, args.Y + img.Top);
                img.Location += si;
                // 2021.11.2 feature:无限的画布
                img.Width = (int)(pictureBoxHost.ActualWidth*inchesToPixels - img.Left);
                img.Height = (int)(pictureBoxHost.ActualHeight * inchesToPixels - img.Top);
                //img.Size = new Size(splitContainer1.Panel2.Height - img.Top, splitContainer1.Panel2.Width - img.Left);
                Console.WriteLine($"si={si}");
            };
            img.MouseDown += (s, args) => { lastLocation = new Point(args.X + img.Left, args.Y + img.Top); img.MouseMove += drag; };
            img.MouseUp += (s, args) => img.MouseMove -= drag;

            // 2021.11.2 feature:无限的画布拖动（针对画框外区域）
            System.Windows.Forms.MouseEventHandler drag1 = (s, args) =>
            {
                var si = new Size(args.X - lastLocation.X, args.Y - lastLocation.Y);
                lastLocation = new Point(args.X, args.Y);
                img.Location += si;
                // 2021.11.2 feature:无限的画布
                img.Width = (int)(pictureBoxHost.ActualWidth * inchesToPixels - img.Left);
                img.Height = (int)(pictureBoxHost.ActualHeight * inchesToPixels - img.Top);
                //img.Size = new Size(splitContainer1.Panel2.Height - img.Top, splitContainer1.Panel2.Width - img.Left);
                Console.WriteLine($"si={si}");
            };
            pictureBoxHost.Child.MouseDown += (s, args) => { lastLocation = new Point(args.X, args.Y); pictureBoxHost.Child.MouseMove += drag1; };
            pictureBoxHost.Child.MouseUp += (s, args) => pictureBoxHost.Child.MouseMove -= drag1;
            // 拖放结束
        }

        private Action<Graphics> paint = (g) => { };//多播委托
        private int bottomOfLastLeaf; // 对于所有的叶子节点，保存它的底边y坐标，方便计算下一个叶子节点相对位置。
        private float scale = 1; // 缩放比例
        static string answer = "";
        #region wjc 大纲模式遍历
        private string dfsShow(JObject obj, TreeNode node)
        {
            answer += "  " + obj["title"].ToString();

            if (obj.Property("children") == null)
                return answer;
            else
            {
                foreach (JObject son in obj["children"]["attached"])
                {
                    TreeNode node0 = node.Nodes.Add(son["title"].ToString());
                    answer += "\r\n" + "  ";
                    dfsShow(son, node0);
                }
            }
            return answer;
        }
        #endregion
        // 绘制方法的入口
        private void draw()
        {
            //Console.WriteLine("in drawing function");
            paint = null;// 刷新上一次的绘制
            bottomOfLastLeaf = -15; // 纵坐标初始点重置
            maxDepth = 0; // 统计绘制最大深度重置
            string content = content_tb.Text;

            try
            {
                //设置isPainting变量使绘制时节点的GotFocus事件不接受信号
                //H，2021/11/14/11:48
                isPainting = true;

                img.Controls.Clear();
                //Console.WriteLine("img ctrs cleared");
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
                //绘制完成，isPainting完成
                //H，2021/11/14/11:48
                isPainting = false;
                //img.ResumeLayout();

                //img.Invalidate();
                img.Refresh();
                //wjc 每次绘制时调用大纲模式遍历结点
                NewMethod1();

                Console.WriteLine("最大深度" + maxDepth);
                Console.WriteLine("bottomOfLastLeaf=" + bottomOfLastLeaf);
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

        private bool ctrlPressed; // 是否按下ctrl键
        #region wjc:文本和字体更改的键盘响应
        private void btnTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            System.Windows.Forms.TextBox s = sender as System.Windows.Forms.TextBox; 
            if (e.KeyValue == 17)//子节点
            {
                AddFontStyle(((RoundButton)s.Tag));
            }
            if (e.KeyValue == 13)
            { 
                string content = s.Text;
                ((RoundButton)s.Tag).Text = s.Text;
                ((RoundButton)s.Tag).Visible = false;
                //更改json文件内容
                ((RoundButton)s.Tag).Jref["title"] =((RoundButton)s.Tag).Text;
                content_tb.Text = JsonConvert.SerializeObject(((RoundButton)s.Tag).Jref.Root, Formatting.Indented);
            }
        }
        Font font;
        private void AddFontStyle(RoundButton newNode)
        {
            if ((newNode.Jref) != null)
            {
                newNode.myFont = font;
                newNode.Jref.Add(new JProperty("fontName", font.Name));
                newNode.Jref.Add(new JProperty("fontSize", font.Size));
                newNode.Jref.Add(new JProperty("fontStyle", font.Style));
            }
            content_tb.Text = JsonConvert.SerializeObject(newNode.Jref.Root, Formatting.Indented);
        }
        #endregion
        int maxDepth = 0;
        private void NewMethod(RoundButton parentNode, int child_index, JToken node)
        {
            //Console.WriteLine("in NewMethod function");
            RoundButton newNode = new RoundButton();
            newNode.Jref = (JObject)node;
            int depth = System.Text.RegularExpressions.Regex.Matches(newNode.Jref.Path, ".children.attached").Count;
            maxDepth = depth > maxDepth ? depth : maxDepth;

            ////添加上下文菜单
            NewMenustrip(newNode);

            // 绘制形状
            img.Controls.Add(newNode);
            newNode.Size = new Size(153, 60);
            newNode.Location = new Point(parentNode.rightPoint.X + 30, (newNode.Height + 10) * child_index);

            if (bottomOfLastLeaf > 0)
            {
                newNode.Top = bottomOfLastLeaf + 15;
            }
            else
            {
                newNode.Top = 0;
            }
            if (newNode.myFont != null)
                newNode.Font = newNode.myFont;
            //newNode.Font = new Font("微软雅黑", 15.73109F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(134)));
            newNode.NormalColor = Color.White;
            newNode._baseColor = _baseColor;
            newNode.PressedColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            newNode.HoverColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            newNode.Text = node["title"].ToString();//按钮文本

            //
            //读样式
            //H，2021/11/14/11:48
            ReadStyles(newNode);

            // wjc:增加按钮文本更改事件
            newNode.btnTextBox.KeyDown+= new System.Windows.Forms.KeyEventHandler(btnTextBox_KeyDown);
            // 拖拽事件
            newNode.AllowDrop = true;
            newNode.MouseDown += (s, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left && e.Clicks == 1)
                {
                    var b = s as RoundButton; 
                    //crtjrf = b.Jref;
                    b.DoDragDrop(b, (System.Windows.Forms.DragDropEffects)DragDropEffects.All);
                }
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    var b = s as RoundButton;
                    b.Focus();
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
            newNode.GotFocus += (s, e) =>
            {
                // wjc
                //btnTextBox_KeyDown(_, args);
                
                if (isPainting != true)//是Painting的话就不操作
                {
                    RoundButton btn = s as RoundButton;
                    myJref.tempJref = btn.Jref;
                    Console.WriteLine("myJref.tempJref has title : " + myJref.tempJref["title"]);
                }
                Console.WriteLine("Gotfocus has title : " + newNode.Jref["title"]);
            };
            if (myJref.tempJref.Path == newNode.Jref.Path&&isNodeOp==true)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    //Keyboard.Focus(pictureBoxHost);
                    newNode.Focus();
                }));
                //Keyboard.Focus(pictureBoxHost);
                //panel.Focus();
            }

            //tab/enter/del键的处理，H
            newNode.KeyDown += Btn_KeyDown;
            //if (myJref.tempJref?.Path == newNode.Jref.Path)
            //{
            //    newNode.Focus();
            //}
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

                    Pen penpen;
                    switch (ThemeManager.Current.ApplicationTheme.GetValueOrDefault())
                    {
                        case ApplicationTheme.Light:
                            penpen = new Pen(Color.FromArgb(255, 0, 0, 255), 3);
                            break;
                        case ApplicationTheme.Dark:
                            penpen = new Pen(Color.FromArgb(255, 255, 255, 255), 3);
                            break;
                        default:
                            penpen = new Pen(Color.FromArgb(255, 0, 0, 255), 3);
                            break;
                    }
                    g.DrawBezier(penpen, pp1, cc1, cc2, pp2);
                };
            }


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

        private bool isLeaf(JObject node) => node.SelectToken("children.attached[0]") == null;

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
        }

        #region Change Theme
        private void ButtonConfig_OnClick(object sender, RoutedEventArgs e) => PopupConfig.IsOpen = true;

        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is System.Windows.Controls.Button button)
            {
                PopupConfig.IsOpen = false;
                if (button.Tag is ApplicationTheme tag)
                {
                    ((App)System.Windows.Application.Current).UpdateTheme(tag);

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

        private Color _baseColor = Color.Azure;
        private void btn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //outline.Children.Add(ddd);
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
            //改变文本
            //H，2021/11/14/11:48
            if (myJref != null)
            {
                if (myJref.tempJref == null)//初始时加载
                {
                    Console.WriteLine("content_tb is initializing");
                    var data = JsonConvert.DeserializeObject(content_tb.Text) as JArray;
                    var obj = data[0] as JObject;
                    var rootTopic = obj["rootTopic"] as JObject;
                    myJref.tempJref = rootTopic;
                    Console.WriteLine("myJref has the value of rootTopic,not null anymore");
                }
            }
            if (img != null)
            {
                //Console.WriteLine("Begin drawing");
                draw();

                //撤销操作，H
                if (recalled == false)//除了撤销外的其它操作
                {
                    Recall();//记录修改
                }
                else//撤销操作
                {
                    recalled = false;
                }
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            pictureBoxHost.Margin = new Thickness(300, 0, (bool)prop_btn.IsChecked ? 300 : 0, 0);
        }

        private void NewMethod1()
        {
            answer = "";
            var outLineTree = new mind.NewTreeView();
            treeHost.Child = outLineTree;
            outLineTree.Width = 1000;
            outLineTree.Height = 1000;
            outLineTree.BorderStyle = BorderStyle.None;
            //读取json文件内容
            string message = content_tb.Text;
            var obj1 = JsonConvert.DeserializeObject(message) as JArray;
            var obj = obj1[0] as JObject;
            JObject root = obj["rootTopic"] as JObject;
            TreeNode node = outLineTree.Nodes.Add(root["title"].ToString());
            dfsShow(root, node);
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

        //DemoModel注册，H
        public static readonly DependencyProperty DemoModelProperty = DependencyProperty.Register(
    "DemoModel", typeof(PropertyGridDemoModel), typeof(MainWindow), new PropertyMetadata(default(PropertyGridDemoModel)));
        public PropertyGridDemoModel DemoModel
        {
            get => (PropertyGridDemoModel)GetValue(DemoModelProperty);
            set => SetValue(DemoModelProperty, value);
            //Dispatcher.Invoke((Action)(()=> btnClickMe_Click(this,new RoutedEventArgs()))); }
        }

        const double inchesToPixels = 1.2357;
        private void toCenter_Click(object sender, RoutedEventArgs e)
        {
            var aw = (int)(pictureBoxHost.ActualWidth * inchesToPixels);//有偏移，原因未知
            Console. WriteLine("aw=" + aw);
            var ah = (int)pictureBoxHost.ActualHeight;
            var iw = maxDepth * (153 + 30) + 153;
            var ih = bottomOfLastLeaf;
            img.Location = new Point((aw - iw) / 2, (ah - ih) / 2);
         }

        //wjc 操作面板设计
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            //保存：保存当下更改
            draw();
            HandyControl.Controls.MessageBox.Show("保存成功");
        }

        private void SaveAsBtn_Click(object sender, RoutedEventArgs e)
        {
            DocHelper.create(content_tb.Text);
        }

        private void PullInBtn_Click(object sender, RoutedEventArgs e)
        {
            content_tb.Text = DocHelper.read();
        }

        private void PushOutBtn_Click(object sender, RoutedEventArgs e)
        {
            DocHelper.create(content_tb.Text);
            HandyControl.Controls.MessageBox.Show("导出成功");
        }

        private void BackOut_Click(object sender, RoutedEventArgs e)
        {
            DocHelper.PerformClick(button);
        }

        private void ReDo_Click(object sender, RoutedEventArgs e)
        {
            content_tb.Text = DocHelper.readFromTemplate();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            DelTopic(myJref.tempJref);
        }

        private void Topic_Click(object sender, RoutedEventArgs e)
        {
            AddBroTopic(myJref.tempJref);
        }

        private void SonTopic_Click(object sender, RoutedEventArgs e)
        {
            AddSubTopic(myJref.tempJref);
        }

        private void MindMap_Click(object sender, RoutedEventArgs e)
        {
            draw();
            HandyControl.Controls.MessageBox.Show("重新加载思维导图成功");
        }

        private void OutLineBtn_Click(object sender, RoutedEventArgs e)
        {
            DocHelper.PerformClick(outLine); 
        }

        private void MagnifyBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ZoomOutBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShortcutBtn_Click(object sender, RoutedEventArgs e)
        {
            PopupWindow popup = new PopupWindow()
            {
                MinWidth = 400,
                Title = "快捷键助手",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowInTaskbar = true,
                AllowsTransparency = true,
                WindowStyle = WindowStyle.None
            };
            System.Windows.Controls.TextBox txtUsername = new System.Windows.Controls.TextBox();
            txtUsername.Text = "Tab: 增加兄弟节点" +"\r\n"+ "Enter: 增加子节点" + "\r\n" + "Delete: 删除节点" + "\r\n";
            popup.PopupElement = txtUsername;
            popup.ShowDialog();

        }

        private void AboutUsBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.TextBox txtUsername = new System.Windows.Controls.TextBox();
            txtUsername.Text = "项目: 思维导图" + "\r\n" + "团队负责人: 仇钧超" + "\r\n" + "团队成员: 王继承，胡一鸣" + "\r\n";
            PopupWindow popup = new PopupWindow()
            {
                MinWidth = 400,
                Title = "关于@我们",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowInTaskbar = true,
                AllowsTransparency = true,
                WindowStyle = WindowStyle.None,
            };
            popup.PopupElement = txtUsername;      
            popup.ShowDialog();
        }

        private void newBtm_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
