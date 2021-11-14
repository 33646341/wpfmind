using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HandyControl.Controls;
using HandyControl.Themes;
using HandyControl.Tools;
using Mind;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FontStyle = System.Drawing.FontStyle;

namespace WpfMind
{

    public partial class MainWindow
    {
        #region 属性字段
        Stack<(string, JObject, DateTime time)> systemStack = new Stack<(string, JObject, DateTime time)>();
        bool recalled = false;
        MyJref myJref;
        private bool isNodeOp = true;//增删时是true，其他操作是false
        private bool isPainting = false;
        public bool isediting = true; 
        //public bool ischangingstyle = false;
        public delegate void TmpChanged(object sender);
        public PropertyGridDemoModel crtModel;
        #endregion
        #region 节点增删
        private void Btn_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            RoundButton s = sender as RoundButton;
            if (e.KeyData == System.Windows.Forms.Keys.Tab)//子节点
            {
                AddSubTopic(s.Jref);
            }
            if (e.KeyData == System.Windows.Forms.Keys.Enter)//兄弟节点
            {
                AddBroTopic(s.Jref);
            }
            if (e.KeyData == System.Windows.Forms.Keys.Delete)
            {
                DelTopic(s.Jref);
            }
        }
        private void AddSubTopic(JObject newNodeJref)
        {
            if (isLeaf(newNodeJref))
            {
                newNodeJref.Add(new JProperty("children", new JObject(new JProperty("attached", new JArray()))));
            }
            var children = newNodeJref.SelectToken("children.attached") as JArray;
            JObject onenewnode = new JObject();
            var num = children.Count();
            string title = newNodeJref["title"].ToString();
            if (Regex.IsMatch(title, "中心主题"))
            {
                onenewnode["title"] = "分支主题" + (num + 1).ToString();
            }
            else
            {
                onenewnode["title"] = "子主题" + (num + 1).ToString();
            }
            children.Add(onenewnode);
            myJref.tempJref = onenewnode;
            content_tb.Text = JsonConvert.SerializeObject(newNodeJref.Root, Formatting.Indented);
        }

        private void AddBroTopic(JObject newNodeJref)//生成兄弟节点
        {
            //JObject  ob= (JObject)newNode.Jref.Parent;
            int num = newNodeJref.Parent.Count;
            JObject onenewnode = new JObject();
            string title = newNodeJref["title"].ToString();
            if (Regex.IsMatch(title, "中心主题"))
            {
                AddSubTopic(newNodeJref);
                return;
            }
            else if (Regex.IsMatch(title, "分支主题"))
            {
                onenewnode["title"] = "分支主题" + (num + 1).ToString();
            }
            else
            {
                onenewnode["title"] = "子主题" + (num + 1).ToString();
            }
            newNodeJref.AddAfterSelf(onenewnode);
            myJref.tempJref = onenewnode;
            content_tb.Text = JsonConvert.SerializeObject(newNodeJref.Root, Formatting.Indented);
        }

        private void DelTopic(JObject newNodeJref)//删除节点
        {
            Console.WriteLine("" + newNodeJref["title"]);
            var ob = newNodeJref.Root;

            if (newNodeJref.AfterSelf().Count() != 0)
            {
                myJref.tempJref = (JObject)newNodeJref.AfterSelf().First();
            }
            else if (newNodeJref.BeforeSelf().Count() != 0)
            {
                myJref.tempJref = (JObject)newNodeJref.BeforeSelf().Last();
            }
            else
            {
                foreach (var anc in newNodeJref.Ancestors())
                {
                    if (anc.Type == JTokenType.Object)
                    {
                        JObject obj = (JObject)anc;
                        if (obj.ContainsKey("children"))
                        {
                            var c = obj.SelectToken("children.attached") as JArray;
                            if (c.Contains(newNodeJref))
                            {
                                myJref.tempJref = obj;
                            }
                        }
                    }
                }
            }
            //删除节点
            newNodeJref.Remove();
            content_tb.Text = JsonConvert.SerializeObject(ob, Formatting.Indented);
        }
        #endregion 
        private void btnClickMe_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Button source = e.Source as Button;
            if (source != null)
            {
                // Change the TextBox color when it loses focus.
                e.Handled = true;

            }
        }
        //撤销
        private void Recall()
        {//加datetime记录时间
            if (systemStack.Count() == 0)
            {
                systemStack.Push((content_tb.Text, myJref.tempJref, DateTime.Now));
            }
            if ((DateTime.Now - systemStack.Peek().Item3) > TimeSpan.FromSeconds(2))
            {
                systemStack.Push((content_tb.Text, myJref.tempJref, DateTime.Now));
            }
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (systemStack.Count > 1)
            {
                if (content_tb.Text == systemStack.Peek().Item1)
                {
                    systemStack.Pop();
                }
                recalled = true;
                myJref.tempJref = systemStack.Peek().Item2;
                content_tb.Text = systemStack.Peek().Item1;

            }
        }
        //监听面板变化事件
        private void model_OnChanged(object sender)
        {
            Console.WriteLine("in model_OnChanged function");
            isNodeOp = false;
            WriteStyles(myJref.tempJref);
            isNodeOp = true;
        }
        //写样式
        private void WriteStyles(JObject Jref)
        {
            //    var properties = (JObject)Jref.SelectToken(@"style.properties");
            //    Console.WriteLine(properties?["fo:font-weight"] ?? "0");
            //    读取Demomodel，逐项比对是否一致
            //Console.WriteLine("in WriteStyles function");
            if (isediting) {
                //Console.WriteLine("isediting = true");
            if (DemoModel != null)
                {
                    //Console.WriteLine("DemoModel != null");
                    JObject styleproperty = new JObject();
                styleproperty["fo:font-radius"] = DemoModel.圆角半径;
                styleproperty["fo:font-size"] = DemoModel.大小.ToString() + "pt";
                styleproperty["fo:font-style"] = DemoModel.字体样式.ToString();
                styleproperty["fo:font-family"] = DemoModel.字体.ToString();
                styleproperty["fo:color"] = DemoModel.颜色.ToString();

                if (!Jref.ContainsKey("style"))
                {
                    Jref.Add(new JProperty("style", new JObject()));
                    var ob = (JObject)Jref["style"];
                    if (!ob.ContainsKey("properties"))
                    {
                        ob.Add(new JProperty("properties", new JObject()));
                    }
                }
                    Console.WriteLine("myJref.tempJref is changing,title: " + myJref.tempJref["title"]) ;
                    Jref["style"]["properties"] = styleproperty;
                    Console.WriteLine("Jref is changed,title: " + myJref.tempJref["title"]);
                }
                content_tb.Text = JsonConvert.SerializeObject(Jref.Root, Formatting.Indented);
        }
        }
        //读样式
        private void ReadStyles(RoundButton newNode)
        {
            //Console.WriteLine("in ReadStyles function");
            var properties = (JObject)newNode.Jref.SelectToken(@"style.properties");
            if (properties != null)
            {
                float fontsize = 0;
                FontStyle tmpstyle=new FontStyle();
                string fontfamily = "";
                foreach (var property in properties.Properties())
                {
                    //JProperty,Name,Value
                    JProperty pro = (JProperty)property;
                    switch (pro.Name.ToString())
                    {
                        case ("fo:font-radius"): { newNode.Radius = Convert.ToInt32(pro.Value.ToString()); break; }
                        case ("fo:font-size"): {
                                Regex size = new Regex("[0-9]+");
                                string sz = size.Match(pro.Value.ToString()).Value;
                                fontsize = (float)Convert.ToInt32(sz);
                                break; }
                        case ("fo:font-style"): {tmpstyle = (FontStyle)Enum.Parse(typeof(FontStyle), pro.Value.ToString()); break; } 
                        case ("fo:font-family"): { fontfamily = pro.Value.ToString(); break; }
                        case ("fo:color"): { newNode.ForeColor =Color.FromName(pro.Value.ToString()); break; }
                    }
                }
                Font newfont = new Font(fontfamily, fontsize, tmpstyle);
                //Console.WriteLine("Begin changing newNode.Font");
                newNode.Font = newfont;
                //Console.WriteLine("Finish changing");
            }
        }
        //监听节点切换事件;
        private void JrefChanged(object sender)
        {
            Console.WriteLine("in JrefChanged function");
            //if (ischangingstyle == true)
            //{
                if (myJref.tempJref != null)
                {
                if ((JObject)myJref.tempJref["style"] != null)
                {
                    Console.WriteLine("JrefChanged:=>myJref has title : " + myJref.tempJref["title"].ToString());
                    if (!((JObject)myJref.tempJref["style"]).ContainsKey("properties"))
                    {
                        InitializeJref(myJref.tempJref);
                    }
                }
                else
                {
                    myJref.tempJref.Add(new JProperty("style", new JObject()));
                    InitializeJref(myJref.tempJref);
                    foreach (var proper in (myJref.tempJref["style"]["properties"]).Children())
                    {
                        JProperty jobproper = (JProperty)proper;
                        //Console.WriteLine(jobproper.Value.ToString());
                    }
                }
                JObject jstyle = (JObject)myJref.tempJref["style"]["properties"];
                Regex size = new Regex("[0-9]+");
                string sz = size.Match(jstyle["fo:font-size"].ToString()).Value;
                //tmpmodel.FontSize = Convert.ToInt32(sz);
                PropertyGridDemoModel tmpmodel = new PropertyGridDemoModel
                {
                    圆角半径 = (int)jstyle["fo:font-radius"],
                    字体 = (Family)Enum.Parse(typeof(Family), jstyle["fo:font-family"].ToString()),
                    大小 = Convert.ToInt32(sz),
                    字体样式 = (FontStyle)Enum.Parse(typeof(FontStyle), jstyle["fo:font-style"].ToString()),
                    颜色 = (ModelColor)Enum.Parse(typeof(ModelColor), jstyle["fo:color"].ToString())
                };
                tmpmodel.OnChanged += new ModelChanged(model_OnChanged);
                isediting = false;
                DemoModel = tmpmodel;
                isediting = true;

                //foreach (var proper in (myJref.tempJref["style"]["properties"]).Children())
                //{
                //    JProperty jobproper = (JProperty)proper;
                //    Console.WriteLine(jobproper.Value.ToString());
                //}

            }
            //}

        }
        //初始化样式
        private void InitializeJref(JObject Jref)
        {
            JObject styleproperty = new JObject();
            styleproperty["fo:font-radius"] = 15;
            styleproperty["fo:font-size"] = "9pt";
            styleproperty["fo:font-style"] = System.Drawing.FontStyle.Regular.ToString();
            styleproperty["fo:font-family"] = Family.微软雅黑.ToString();
            styleproperty["fo:color"] = ModelColor.Black.ToString();
            JObject tmpstyle = new JObject();
            tmpstyle.Add(new JProperty("properties", styleproperty));
            Jref["style"] = tmpstyle;
        }

        private void NewMenustrip(RoundButton newNode)
        {
            //新建一个上下文菜单，绑定菜单
            System.Windows.Forms.ContextMenuStrip newmenustrip = new System.Windows.Forms.ContextMenuStrip();
            newNode.ContextMenuStrip = newmenustrip;
            //建个菜单项
            System.Windows.Forms.ToolStripMenuItem tsmi1 = new System.Windows.Forms.ToolStripMenuItem("删除");
            System.Windows.Forms.ToolStripMenuItem tsmi2 = new System.Windows.Forms.ToolStripMenuItem("主题");
            System.Windows.Forms.ToolStripMenuItem tsmi3 = new System.Windows.Forms.ToolStripMenuItem("子主题");
            System.Windows.Forms.ToolStripMenuItem tsmi4 = new System.Windows.Forms.ToolStripMenuItem("拷贝样式");
            System.Windows.Forms.ToolStripMenuItem tsmi5 = new System.Windows.Forms.ToolStripMenuItem("粘贴样式");
            //System.Windows.Forms.ToolStripMenuItem tsmi6 = new System.Windows.Forms.ToolStripMenuItem("重设样式");
            tsmi1.Click += DemoClick;
            tsmi2.Click += DemoClick;
            tsmi3.Click += DemoClick;
            tsmi4.Click += DemoClick;
            tsmi5.Click += DemoClick;
            //tsmi6.Click += DemoClick;
            //添加主菜单
            newmenustrip.Items.Add(tsmi1);
            newmenustrip.Items.Add(tsmi2);
            newmenustrip.Items.Add(tsmi3);
            newmenustrip.Items.Add(tsmi4);
            newmenustrip.Items.Add(tsmi5);
            //newmenustrip.Items.Add(tsmi6);
        }
        private void DemoClick(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripMenuItem toolStripMenuItem = sender as System.Windows.Forms.ToolStripMenuItem;
            switch (toolStripMenuItem.Text)
            {
                case ("删除"): { DelTopic(myJref.tempJref); break; }
                case ("主题"): {AddBroTopic(myJref.tempJref); break; }
                case ("子主题"): { AddSubTopic(myJref.tempJref); break; }
                case ("拷贝样式"): {
                        CopyStyle(crtModel, DemoModel);break; }
                case ("粘贴样式"): {
                        DemoModel=crtModel;
                        DemoModel.OnChanged += model_OnChanged;
                        break; }
                //case ("重设样式"): {
                //        //ischangingstyle = true; 
                //        InitializeJref(myJref.tempJref);
                //        //ischangingstyle = true;
                //        CopyStyle(crtModel, DemoModel);
                //        break; }
            }
        }

        private void CopyStyle(PropertyGridDemoModel crtModel,PropertyGridDemoModel Dmodel)
        {
            crtModel.圆角半径 = Dmodel.圆角半径;
            crtModel.字体 = Dmodel.字体;
            crtModel.大小 = Dmodel.大小;
            crtModel.字体样式 = Dmodel.字体样式;
            crtModel.颜色 = Dmodel.颜色;
        }
        private void InitializeModel(PropertyGridDemoModel model)
        {
            model = new PropertyGridDemoModel
            {
                圆角半径 = 25,
                字体 = Family.微软雅黑,
                大小 = 9,
                字体样式 = System.Drawing.FontStyle.Regular,
                颜色 = ModelColor.Black
            };
            model.圆角半径 = 28;
            model.OnChanged += new ModelChanged(model_OnChanged);
        }
        //监听Jref的类
        class MyJref
        {
            private bool isJrefChanged=false;
            public event TmpChanged OnMyChange;
            private JObject TempJref;
            public JObject tempJref
            {
                get
                {
                    return TempJref;
                }
                set
                {//&&TempJref!=value
                    if (TempJref!=null )
                    {
                        if (TempJref["title"] != value["title"] || TempJref["style"] != value["style"])
                        {
                            if (TempJref["title"] != value["title"]){
                                Console.WriteLine("title not equal");
                            }
                            if (TempJref["style"] != value["style"])
                            {
                                Console.WriteLine("style not equal");
                            }
                            isJrefChanged = true;
                        }
                    }
                    TempJref = value;
                    if (isJrefChanged) {
                        OnMyChange(this);
                        isJrefChanged = false;
                    }
                }
            }
        }
    }
}
