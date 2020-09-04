using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ShpRead;
using System.IO;
using System.Data.OleDb;
namespace ShpRead
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        List<PointShp> listPointShp = new List<PointShp>();
        List<LinesShp> listLineShp = new List<LinesShp>();
        OleDbDataAdapter myadapter;
        DataSet myds;
        ShpRead.Shp kl = new Shp();

        //自定义添加成员变量 2018.7.12
        int flag;   //绘图标识
        int flag1 = 0; //移动标识
        int flag2 = 0; //关闭标识
        //平移量
        int leftX;
        int rightX;
        int upY;
        int downY;
        List<LinesShp> listLineShp1 = new List<LinesShp>(); // 存一份坐标点未变化的线
        //缩放量
        float zoomX;
        float zoomY;

        //输入框移动变量
        private Point m_lastPoint;
        private Point m_lastMPoint;

        //动态输入框
        TextBox tex = new TextBox();
  
        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //点击菜单打开时执行此函数， 完成
            /*
             * 1 打开shp文件
             * 2 获取shp文件中的shp（点或线）和标注
             * 3 坐标变换，变换成屏幕坐标以后，将点坐标和点的标注保存到listPointShp列表中，线对象保存到
             * listLineShp中；
             * 4 坐标变换时纵横方向的变换比例要相同，不要出现拉伸或压缩图像的效果；屏幕坐标系
             *   原点位于左上角，x向右增加，y向下增加；地图坐标原点位于左下角，x向右增加，y向上增加
             * 4 把shp文件头的内容分段填写"地图概况"选项卡内
             * 5 调用FIllProptyTable函数将shp属性表显示在"属性表"内的GridView内。
             * */


            /*
             * 个人注释  --2018.7.8
             * 1.启动打开文件对话框，获得找到shp的filename
             * 2.调用OpenShpFile(filename)
             * 3.调用OpenShpFile()返回值来提示是否打开的是shp文件
             * 
             */

            flag1 = 1;
            int state=-1;
            OpenFileDialog ofd = new OpenFileDialog();
            DialogResult result =   ofd.ShowDialog();
            ofd.Filter = "shapefile(*.shp)|*.shp|All files(*.*)|*.*";
            if (result == DialogResult.OK)
            {
                String filename = ofd.FileName;
                //MessageBox.Show(filename);
                state=  kl.OpenShpFile(filename);
                //若打开的是shp文件
                if (state == 1)
                {
                    textBox1.Text = "文件代码:" + kl.GetFileCode() + "\r\n";
                    textBox1.Text += "文件长度:" + kl.GetFileLength() + "\r\n";
                    textBox1.Text += "文件版本:" + kl.GetFileVer() + "\r\n";
                    textBox1.Text += "shp类型:" + kl.GetFileType() + "\r\n";
                    textBox1.Text += "x最小值" + kl.GetXMin() + "\r\n";
                    textBox1.Text += "x最大值" + kl.GetXMax() + "\r\n";
                    textBox1.Text += "y最小值" + kl.GetYMin() + "\r\n";
                    textBox1.Text += "y最大值" + kl.GetYMax() + "\r\n";
                    textBox1.Text += "z最小值" + kl.GetZMin() + "\r\n";
                    textBox1.Text += "z最大值" + kl.GetZMax() + "\r\n";
                    textBox1.Text += "m最小值" + kl.GetMMin() + "\r\n";
                    textBox1.Text += "M最大值" + kl.GetMMax() + "\r\n";

                    object shpContent; //shp内容
                    int shptype;  //shp类型
                    int recordid; //记录ID



                    while (kl.GetNextShp(out shptype, out shpContent, out recordid) != 0)
                    {
                        #region 若是点shp
                        if (shptype == 1)
                        {
                            
                            /*
                             * 
                             * 此处补充代码，完成相应功能
                             * 
                             * 
                             */
                            PointShp point = (PointShp)shpContent;
                            listPointShp.Add(point);
                            flag = 1;                        
                        }

                        #endregion 点处理完成

                        #region 若是线shp
                        else if (shptype == 3)
                        {

                            /*
                             * 
                             * 此处补充代码，完成相应功能
                             * 
                             * 
                             */
                            LinesShp line = (LinesShp)shpContent;

                            LinesShp line1 = new LinesShp();

                            //需要克隆一份，存在两个不同内存空间，使得不会同时变化数据 -- 深拷贝
                            line1.Points = (PointF[])line.Points.Clone();
                            line1.label = line.label;
                            listLineShp1.Add(line1);

                            //线的点映射变化，不放在绘制线的循环里，不然循环压力太重
                            float changeX = pictureBox1.Width / (float)(kl.GetXMax());
                            float changeY = pictureBox1.Height / (float)kl.GetYMax();
                            float minx = (float)kl.GetXMin() * changeX;
                            float miny = (float)kl.GetYMin() * changeY;
                            for (int i = 0; i < line.Points.Length; i++)
                            {

                                line.Points[i].X = line.Points[i].X * changeX - minx;
                                line.Points[i].Y = -line.Points[i].Y * changeY + miny+30;
                            }
                            listLineShp.Add(line);                           
                            flag = 3;                            
                        }
                        #endregion

                    }

                    //重置参数
                    zoomX = 0;
                    zoomY = 0;
                    leftX = 0;
                    rightX = 0;
                    upY = 0;
                    downY = 0;
                    kl.CloseShpFile();
                    FillProptyTable();        
                    pictureBox1.Invalidate();
                
                }
                //若打开的不是shp文件
                if (state == -1)
                {
                    MessageBox.Show("您打开的不是shp类型文件");
                }
            }
      
        }

        public void FillProptyTable()
        {
            /*
             * 填充datagridview，用来填充的数据集来自Shp的成员函数GetDataSet
             * 
             * */

            myds = kl.GetDataSet();
            dataGridView1.DataSource = myds.Tables["dataTable"];

        }
        

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            
            /*
             * 地图显示，将点、线显示到地图选项卡中，颜色和字体可以任选，也可以
             * 自定义
             * */
            
            Graphics g = Graphics.FromImage(this.pictureBox1.Image);
            g.Clear(Color.White);
            SolidBrush sBrush = new SolidBrush(Color.Red);
            SolidBrush sBrush1 = new SolidBrush(Color.Black);
            Pen pen = new Pen(Color.Red,0.01f); //要用很细的画笔
            Font font = new Font("宋体", 1f);
            Font font1 = new Font("宋体",0.1f); //字体也尽量小
            
            //如何将西安80坐标系转成屏幕坐标  ---  平移和伸缩

            /*
             *  + - 坐标值 -- 平移
             *  * / 坐标值 -- 伸缩
             *  通过 g.ScaleTransform（dx,dy) 放大坐标系
             */

            //绘制点
            if (listPointShp.Count > 0&&flag==1)
            {    
                
                g.TranslateTransform(0, pictureBox1.Height);
                g.ScaleTransform(6f+zoomX,22f+zoomY);
                
                foreach (PointShp p in listPointShp)
                {
                    float x = 0;
                    float y =0;
                    float changeX = pictureBox1.Width / (float)(kl.GetXMax());
                    float changeY = pictureBox1.Height / (float)kl.GetYMax();
                    float minx = (float)kl.GetXMin() * changeX;
                    float miny = (float)kl.GetYMin() * changeY;
                    if (flag1 == 1)
                    {
                        x = p.pf.X*changeX-minx+rightX+leftX;
                        y = -p.pf.Y*changeY+miny+downY+upY;
                    }

                    PointF pointf = new PointF(x, y);
                    g.FillEllipse(sBrush, pointf.X, pointf.Y,1f,1f);
                    g.DrawString(p.label,font, sBrush1, pointf.X+2,pointf.Y);                  
                   
                }       
            }

            //绘制线
            if (listLineShp.Count > 0&&flag==3)
            {

                g.TranslateTransform(0, pictureBox1.Height);      
                g.ScaleTransform(100f+zoomX, 50f+zoomY);

            
                foreach (LinesShp l in listLineShp)
                {   
                    g.DrawLines(pen, l.Points);
                    g.DrawString(l.label, font1, sBrush1, l.Points[0].X, l.Points[0].Y);
                }              
            }

            g.Dispose();
            Graphics g1 = e.Graphics;
            g1.DrawImage(pictureBox1.Image, new Point(0, 0));
            pen.Dispose();
            sBrush.Dispose();
            sBrush1.Dispose();
           
        }

        //订阅滚轮事件
        private void Form1_Load(object sender, EventArgs e)
        {
          
        this.MouseWheel += new MouseEventHandler(Form1_MouseWheel);
        this.pictureBox1.Image = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height);
        

        }

        
        

        //上
        private void button1_Click(object sender, EventArgs e)
        {
            if (flag1 == 1)
            {
                upY -= 5;

                for (int i = 0; i < listLineShp.Count; i++)
                {
                    listLineShp[i].Points = (PointF[])listLineShp1[i].Points.Clone(); // 最原始版本的一份克隆
                    float changeX = pictureBox1.Width / (float)(kl.GetXMax());
                    float changeY = pictureBox1.Height / (float)kl.GetYMax();
                    float minx = (float)kl.GetXMin() * changeX;
                    float miny = (float)kl.GetYMin() * changeY;
                    for (int j = 0; j < listLineShp[i].Points.Length; j++)
                    {

                        listLineShp[i].Points[j].X = listLineShp[i].Points[j].X * changeX - minx + (leftX + rightX) / 5;
                        listLineShp[i].Points[j].Y = -listLineShp[i].Points[j].Y * changeY + miny + 30 + (upY + downY) / 5;
                        

                    }     
                }

                //Console.WriteLine("上"+upY);
                pictureBox1.Invalidate();
            }
        }

        //下
        private void button2_Click(object sender, EventArgs e)
        {
            if (flag1 == 1)
            {
                downY += 5;

                for (int i = 0; i < listLineShp.Count; i++)
                {
                    listLineShp[i].Points = (PointF[])listLineShp1[i].Points.Clone(); // 最原始版本的一份克隆

                    float changeX = pictureBox1.Width / (float)(kl.GetXMax());
                    float changeY = pictureBox1.Height / (float)kl.GetYMax();
                    float minx = (float)kl.GetXMin() * changeX;
                    float miny = (float)kl.GetYMin() * changeY;
                    for (int j = 0; j < listLineShp[i].Points.Length; j++)
                    {

                        listLineShp[i].Points[j].X = listLineShp[i].Points[j].X * changeX - minx+ (leftX + rightX) / 5;
                        listLineShp[i].Points[j].Y = -listLineShp[i].Points[j].Y * changeY + miny + 30 + (upY + downY) / 5;
                        

                    }
                    //Console.WriteLine("下"+downY);
                }

                pictureBox1.Invalidate();
            }
        }

        //左
        private void button3_Click(object sender, EventArgs e)
        {
            if (flag1 == 1)
            {
                leftX -= 5;
               
                for (int i = 0; i < listLineShp.Count; i++)
                {
                    listLineShp[i].Points = (PointF[])listLineShp1[i].Points.Clone(); // 最原始版本的一份克隆

                    float changeX = pictureBox1.Width / (float)(kl.GetXMax());
                    float changeY = pictureBox1.Height / (float)kl.GetYMax();
                    float minx = (float)kl.GetXMin() * changeX;
                    float miny = (float)kl.GetYMin() * changeY;
                    for (int j = 0; j < listLineShp[i].Points.Length; j++)
                    {

                        listLineShp[i].Points[j].X = listLineShp[i].Points[j].X * changeX - minx + (leftX + rightX) / 5;
                        listLineShp[i].Points[j].Y = -listLineShp[i].Points[j].Y * changeY + miny + 30 + (upY + downY) / 5;
                      

                    }

                    //Console.WriteLine("左"+leftX);
                }
                pictureBox1.Invalidate();
            }
        }

        //右
        private void button4_Click(object sender, EventArgs e)
        {
            if (flag1 == 1)
            {
                rightX += 5;

                 for (int i = 0; i < listLineShp.Count; i++)
                {
                    listLineShp[i].Points = (PointF[])listLineShp1[i].Points.Clone(); // 最原始版本的一份克隆

                    float changeX = pictureBox1.Width / (float)(kl.GetXMax());
                    float changeY = pictureBox1.Height / (float)kl.GetYMax();
                    float minx = (float)kl.GetXMin() * changeX;
                    float miny = (float)kl.GetYMin() * changeY;
                    for (int j = 0; j < listLineShp[i].Points.Length; j++)
                    {

                        listLineShp[i].Points[j].X = listLineShp[i].Points[j].X * changeX - minx + (leftX + rightX) / 5;
                        listLineShp[i].Points[j].Y = -listLineShp[i].Points[j].Y * changeY + miny + 30 + (upY + downY) / 5;
                    
                    }

                   //Console.WriteLine("右"+rightX);
                }
                pictureBox1.Invalidate();
            }
        }

        //基于滚轮的缩放
        void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                toolStripStatusLabel1.Text = "正在向上滚动滑轮";

                //点放大
                if (flag == 1)
                {
                    zoomX += 1;
                    zoomY += 1;
                }
                //线放大
                if (flag == 3)
                {
                    zoomX += 5;
                    zoomY += 5;
                }
                pictureBox1.Invalidate();
            }
            else
            {
                toolStripStatusLabel1.Text = "正在向下滚动滑轮";
                //this.Text = "正在向下滚动滑轮;
                //线缩小
                if (flag == 3)
                {
                    if (zoomX != -45 && zoomY != -45)
                    {
                        zoomX -= 5;
                        zoomY -= 5;
                        
                    }
                    else
                    {
                        MessageBox.Show("已经达到最小", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    }

                }
                //点缩小
                if (flag == 1)
                {
                    if (zoomX != -5 && zoomY != -5)
                    {
                        zoomX -= 1;
                        zoomY -= 1;
                    }
                    else
                    {
                        MessageBox.Show("已经达到最小", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    }
                }
                pictureBox1.Invalidate();
            }
        }

        //放大
        private void button5_Click(object sender, EventArgs e)
        {
            //点放大
            if (flag == 1)
            {
                zoomX += 1;
                zoomY += 1;
            }
            //线放大
            if (flag == 3)
            {
                zoomX += 5;
                zoomY += 5;
            }
            pictureBox1.Invalidate();
        }


        //缩小
        private void button6_Click(object sender, EventArgs e)
        {
            //点缩小
            if (flag == 1)
            {
                if (zoomX != -5 && zoomY != -5)
                {
                    zoomX -= 1;
                    zoomY -= 1;
                }
                else
                {
                    MessageBox.Show("已经达到最小", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                }
            }
            //线缩小
            if (flag == 3)
            {
                if (zoomX != -45 && zoomY != -45)
                {
                    zoomX -= 5;
                    zoomY -= 5;
                }
                else
                {
                    MessageBox.Show("已经达到最小", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                }
            }
            pictureBox1.Invalidate();

        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (flag2 == 0)
            {
                DialogResult result = MessageBox.Show("确认退出吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Stop);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("确认退出吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Stop);
            
            if (result == DialogResult.Yes)
            {
                flag2++;
                this.Close();
            }
        }

        //动态添加标签
        private void button7_Click(object sender, EventArgs e)
        {
            tex  = new TextBox();
            tex.Font = new Font("宋体", 20, FontStyle.Bold);
            //tex.Text = "我是地图标题";
            //外边框
            tex.BorderStyle = BorderStyle.FixedSingle;
            tex.BackColor = Color.White;
            tex.ImeMode = System.Windows.Forms.ImeMode.On;
            tex.MouseDown += new MouseEventHandler(tex_MouseDown);
            tex.MouseMove += new MouseEventHandler(tex_MouseMove);
            tex.Size = new Size(300, 50);
            tex.Location = new Point(50, 50);
            pictureBox1.Controls.Add(tex);

        }
        

        //文本框按下时
        private void tex_MouseDown(object sender, MouseEventArgs e)
        {
            m_lastMPoint = Control.MousePosition;
            m_lastPoint = (sender as TextBox).Location;
        }

        //文本框移动时
        private void tex_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                tex.Location = new Point(m_lastPoint.X + Control.MousePosition.X - m_lastMPoint.X, m_lastPoint.Y + Control.MousePosition.Y - m_lastMPoint.Y);
            }
        }

        //图片另存为
        private void button8_Click(object sender, EventArgs e)
        {


            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "图片文件（*.jpg）|*.jpg";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                String pictureName = sfd.FileName;
                if (pictureBox1.Image != null)
                {

                    Graphics g = Graphics.FromImage(this.pictureBox1.Image);
                    g.DrawString(tex.Text, new Font("宋体", 20, FontStyle.Bold), new SolidBrush(Color.Black), tex.Location);
                    ////********************图片另存*********************************
                    using (MemoryStream mem = new MemoryStream())
                    {                       
                        //这句很重要，不然不能正确保存图片或出错（关键就这一句）
                        Bitmap bmp = new Bitmap(pictureBox1.Image);
                        //保存到内存
                        //bmp.Save(mem, pictureBox1.Image.RawFormat );
                        //保存到磁盘文件
                        bmp.Save(@pictureName, pictureBox1.Image.RawFormat);
                        bmp.Dispose();
                        MessageBox.Show("照片另存成功！", "系统提示");
                    }
                    ////********************图片另存*********************************

                }
            }
        }
    }
}
