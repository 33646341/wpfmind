using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mind
{
    public interface NodeTextBox
    {
        void btnTextBox_KeyDown(object sender, KeyEventArgs e);
    }
    public class NewTreeView : TreeView
        {
            private bool isToggleIcon;//是否是切换图标模式 
            protected Font STFont;//二级和三级节点的字体  
            public bool IsToggleIcon
            {
                get
                {
                    return isToggleIcon;
                }
                set
                {
                    isToggleIcon = value;
                }
            }

            public NewTreeView()
            {
                this.isToggleIcon = true;
                this.ShowLines = false;
                this.FullRowSelect = true;
                this.ShowPlusMinus = true;
                this.ItemHeight = 32;
                //this.Control.Add();
                this.Font = new Font("宋体", 16, FontStyle.Bold);
                this.STFont = new Font("宋体", 14, FontStyle.Regular);
        }

            protected override void OnAfterSelect(TreeViewEventArgs e)
            {
                //节点选中后触发    
                if (false == isToggleIcon)//图标分级效果
                {
                    e.Node.SelectedImageIndex = e.Node.Level;//当前节点
                    TreeNodeCollection nodes = e.Node.Nodes;
                    foreach (TreeNode node in nodes)
                    {
                        node.ImageIndex = node.Level; //子节点                  
                    }
                }
            }

            protected override void OnAfterExpand(TreeViewEventArgs e)
            {
                //节点展开后触发
                if (true == isToggleIcon)//图标切换效果
                {
                    //e.Node.
                    //e.Node为展开后下一级的父节点
                    //切换模式下image0为展开前的图标，image1为展开后的图标
                    e.Node.SelectedImageIndex = 1;//展开后的图标
                                                  //SelectedImageIndex为选中期间的image索引，ImageIndex为不选状态下的image索引
                    e.Node.ImageIndex = 1;
                    //展开后的下一级节点
                    TreeNodeCollection nodes = e.Node.Nodes;
                    foreach (TreeNode node in nodes)
                    {
                        node.ImageIndex = 0;//展开前的图标
                        if (0 == node.Nodes.Count)//展开后的下一级节点是叶子节点
                        {
                            node.SelectedImageIndex = 2;//叶子节点图标
                            node.ImageIndex = 2;
                        }
                    }
                    e.Node.BackColor = this.BackColor;
                }
                //展开后设置下一级字体
                TreeNodeCollection newNodes = e.Node.Nodes;
                foreach (TreeNode newNode in newNodes)
                {
                    newNode.NodeFont = STFont;
                }
            }
            protected override void OnAfterCollapse(TreeViewEventArgs e)
            {
                //节点折叠后触发
                if (true == isToggleIcon)//图标切换效果
                {
                    //e.Node为被折叠一级节点的父节点
                    //切换模式下image0为折叠后的图标，image1为折叠前的图标
                    e.Node.SelectedImageIndex = 0;//折叠后的图标                
                    e.Node.ImageIndex = 0;
                }
            }
        }
    }

