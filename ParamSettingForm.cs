using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HalconDotNet;
using System.IO;
using System.Runtime.InteropServices;
using System.IO.Ports;
using System.Threading;
using MySql.Data.MySqlClient;
using AxXJaiLT400CLLib;

namespace DALSA.SaperaLT.Demos.NET.CSharp.MultiBoardSyncGrabDemo
{
    public partial class ParamSettingForm : Form
    {

        bool isFirtOpen = true;
        private Bitmap OriginImage;            //原始位图
        private Point MouseDownPoint;         //鼠标按下坐标
        private Point MouseMovePoint;         //鼠标移动坐标
        private Point MouseUpPoint;           //鼠标弹起坐标
        private Point TempStart_Point;        //记录下矩形当前位置
        private bool IsMoving;               //移动状态
        private bool StartDraw;              //开始画ROI对象
        private bool FinishedDraw;           //完成画ROI对象
        private bool BlockWidth;             //锁定ROI宽度
        private bool BlockHeight;            //锁定ROI高度
        private Rectangle[] CaptureRectangle1; 
        private Rectangle CaptureRectangle;       //当前ROI对象    
        private Rectangle[] m_OperateRectangle;     //ROI周边8个小矩形框
        private SolidBrush SolidBrushObject;       //画刷对象
        private int cap_samples_num;
        private int Rect_dist;
        private int square;
        private int maga;

        [System.Runtime.InteropServices.DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filepath);

        [System.Runtime.InteropServices.DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);
        StringBuilder temp = new StringBuilder(1024);
        string strSec = "";
        private string parstrFilePath = Application.StartupPath + "\\par.ini";//定义参数保存文档



        public static string ToCRCString(byte[] byteData)
        {
            byte[] CRC = new byte[2];

            UInt16 wCrc = 0xFFFF;
            for (int i = 0; i < byteData.Length; i++)
            {
                wCrc ^= Convert.ToUInt16(byteData[i]);
                for (int j = 0; j < 8; j++)
                {
                    if ((wCrc & 0x0001) == 1)
                    {
                        wCrc >>= 1;
                        wCrc ^= 0xA001;//异或多项式
                    }
                    else
                    {
                        wCrc >>= 1;
                    }
                }
            }

            CRC[1] = (byte)((wCrc & 0xFF00) >> 8);//高位在后
            CRC[0] = (byte)(wCrc & 0x00FF);       //低位在前
            string a = Convert.ToString(CRC[0], 16);
            string CRCString = (Convert.ToString(CRC[0], 16).PadLeft(2, '0') + Convert.ToString(CRC[1], 16).PadLeft(2, '0')).ToUpper();
            return CRCString;
            // return CRC;

        }

     // [DllImport("kernel32")]
       // private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

     // [DllImport("kernel32")]
      //  private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        IniFiles cinifile = new IniFiles(Application.StartupPath + @"\MyConfig.INI");

        AutoSizeFormClass asc = new AutoSizeFormClass();//控件自适应类声明

        //MultiBoardSyncGrabDemoDlg MD =new MultiBoardSyncGrabDemoDlg();
        private bool SerialPortIsReady=false;
        private bool tryingConnection;
        

        private static System.Diagnostics.Process p;
        // [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        // private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        // [DllImport("user32.dll", CharSet = CharSet.Auto)]
        //  public static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        // public const int WM_CLOSE = 0x10;
        private bool ConnectionIsReady;
        private string conectionstring;

        struct CapMsg
        {
            public int X;
            public int Y;
            public int CapLength;
            public int CapWidth;
            public int numberCap;
            public int CapDist;
            public Rectangle[] CapRectangle;

            public void getReactangle(int OX, int OY, int CapNum, int Width, int dist, int Lgth)
            {

                X = OX;
                Y = OY;
                numberCap = CapNum;
                CapRectangle = new Rectangle[numberCap];
                CapLength = Lgth;
                CapWidth = Width;
                CapDist = dist;
                for (int i = 0; i < numberCap; i++)
                {
                    CapRectangle[i] = new Rectangle(X + i * CapDist, Y, Lgth, Lgth);
                }


            }


        };

        public ParamSettingForm()
        {
            InitializeComponent();
            MouseDownPoint = new Point(0, 0);
            MouseMovePoint = new Point(0, 0);
            MouseUpPoint = new Point(0, 0);
            TempStart_Point = new Point(0, 0);
            StartDraw = false;
            FinishedDraw = false;
            IsMoving = false;
            BlockWidth = false;
            BlockHeight = false;
            SolidBrushObject = new SolidBrush(Color.YellowGreen);
            m_OperateRectangle = new Rectangle[8];
            cap_samples_num = 6;
            Rect_dist = 100;
            cap_samples_num = Convert.ToInt32(cap_samples_textbox.Text);
            square = Convert.ToInt32(Width_textbox.Text);
            maga = 1;
           // CapMsg CM3;
            Graphics CG = DisplayImage_pictureBox.CreateGraphics();
            for (int i = 0; i < 8; i++)
            {
                m_OperateRectangle[i] = new Rectangle(0, 0, 10, 10);
            }
            CaptureRectangle1 = new Rectangle[cap_samples_num];
            for (int i = 0; i <= cap_samples_num-1; i++)
            {
                CaptureRectangle1[i] = new Rectangle(0, 0, 10, 10);
            }
            CapMsg CM1;
            CapMsg CM2;

            CM1.CapDist = 70;
            CM1.CapLength = 50;
            CM1.CapWidth = 50;
            CM1.numberCap = 6;

            CM2.CapDist = 70;
            CM2.CapLength = 50;
            CM2.CapWidth = 50;
            CM2.numberCap = 6;

            trackBar1.Maximum = Convert.ToInt32(textBox10.Text);
            trackBar1.Minimum = Convert.ToInt32(textBox9.Text);
            trackBar1.SmallChange = (trackBar1.Maximum - trackBar1.Minimum) / 5;
            trackBar1.LargeChange = (trackBar1.Maximum - trackBar1.Minimum) / 5+1;
            trackBar1.TickFrequency= (trackBar1.Maximum - trackBar1.Minimum) / 5;


            //
        }
        /// <summary>
        /// 窗体加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoad(object sender, EventArgs e)
        {
            PublicClass.serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);
            timer1.Start();
            timer2.Enabled = true;
            string[] ports = SerialPort.GetPortNames();
            comboBox1.Items.AddRange(ports);
            comboBox2.Items.AddRange(ports);
            comboBox3.Items.AddRange(ports);

            if (PublicClass.serialPort1.IsOpen)
            {
                comboBox1.Text = PublicClass.serialPort1Name;
                string datapacket = "01 03 00 00 00 01 84 0A";
                byte[] array = HexStringToByteArray(datapacket);
                PublicClass.serialPort1.Write(array, 0, array.Length);
            }
            if (PublicClass.serialPort2.IsOpen)
            {
                comboBox2.Text = PublicClass.serialPort2Name;
            }
            if (PublicClass.serialPort3.IsOpen)
            {
                comboBox3.Text = PublicClass.serialPort3Name;
            }

            //comboBox1.Text = PublicClass.serialPort1Name;
            //comboBox2.Text = PublicClass.serialPort2Name;
            // comboBox3.Text = PublicClass.serialPort3Name;
            asc.controllInitializeSize(this);
            //加载布匹图像
            LoadImage("demo.jpg");
            //设置双缓存
            SetDoubleBuffering();
            //在文本框里面加载数据
            //验证ini路径是否存在，放在这里有点问题？
            cinifile.readfromini();

            //从ini文本读取数据
            cap_samples_textbox.Text = cinifile.IniReadValue("检测框信息", "采样个数");
            dist_textbox.Text = cinifile.IniReadValue("检测框信息", "采样周期");
            Width_textbox.Text = cinifile.IniReadValue("检测框信息", "采样宽度");
            PointX.Text = cinifile.IniReadValue("检测框信息", "采样起点X");
            PointY.Text = cinifile.IniReadValue("检测框信息", "采样起点Y");

            cap_samples_textbox.Text = cinifile.IniReadValue("检测框信息", "采样个数");
            dist_textbox.Text = cinifile.IniReadValue("检测框信息", "采样周期");
            Width_textbox.Text = cinifile.IniReadValue("检测框信息", "采样宽度");
            PointX.Text = cinifile.IniReadValue("检测框信息", "采样起点X");
            PointY.Text = cinifile.IniReadValue("检测框信息", "采样起点Y");
            

            //参数类获得参数。
            PublicClass.Cap_NUM = int.Parse(cinifile.IniReadValue("检测框信息", "采样个数"));
            PublicClass.Period = Convert.ToInt32(cinifile.IniReadValue("检测框信息", "采样周期"));
            
            PublicClass.Square = Convert.ToInt32(cinifile.IniReadValue("检测框信息", "采样宽度"));
            PublicClass.OriginX = Convert.ToInt32(cinifile.IniReadValue("检测框信息", "采样起点X"));
            PublicClass.OriginY = Convert.ToInt32(cinifile.IniReadValue("检测框信息", "采样起点Y"));
            
            square = Convert.ToInt32(Width_textbox.Text) * DisplayImage_pictureBox.Width / OriginImage.Width;
            Rect_dist = Convert.ToInt32(dist_textbox.Text) * DisplayImage_pictureBox.Width / OriginImage.Width;
            CaptureRectangle.X = Convert.ToInt32(PointX.Text) * DisplayImage_pictureBox.Width / OriginImage.Width;//;OriginImage.Width / 2;
            CaptureRectangle.Width = square;

            square = Convert.ToInt32(Width_textbox.Text) * DisplayImage_pictureBox.Height / OriginImage.Height;
            CaptureRectangle.Y = Convert.ToInt32(PointY.Text) * DisplayImage_pictureBox.Height / OriginImage.Height;
            CaptureRectangle.Height = square;

            TempStart_Point = CaptureRectangle.Location;
            FinishedDraw = true;
            IsMoving = false;
            this.DisplayImage_pictureBox.Invalidate();
           
        }


        /// <summary>
        /// 加载图像
        /// </summary>
        /// <param name="ImagePath"></param>
        private void LoadImage(string ImagePath)
        {
            OriginImage = new Bitmap(ImagePath);
            this.DisplayImage_pictureBox.Image = OriginImage;
        }


        /// <summary>
        /// 设置双缓存,防止画ROI的时候,图片闪烁
        /// </summary>
        private void SetDoubleBuffering()
        {
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.UpdateStyles();
        }



        /// <summary>
        /// 改变鼠标样式
        /// </summary>
        private void ChangeCursor(Point loc, bool DrawOrMove)
        {
            if (DrawOrMove)
            {
                if (m_OperateRectangle[0].Contains(loc) || m_OperateRectangle[7].Contains(loc))
                    this.Cursor = Cursors.SizeNWSE;
                else if (m_OperateRectangle[1].Contains(loc) || m_OperateRectangle[6].Contains(loc))
                    this.Cursor = Cursors.SizeNS;
                else if (m_OperateRectangle[2].Contains(loc) || m_OperateRectangle[5].Contains(loc))
                    this.Cursor = Cursors.SizeNESW;
                else if (m_OperateRectangle[3].Contains(loc) || m_OperateRectangle[4].Contains(loc))
                    this.Cursor = Cursors.SizeWE;
                else if (CaptureRectangle.Contains(loc))
                    this.Cursor = Cursors.SizeAll;
                else
                    this.Cursor = Cursors.Default;
            }

        }

        /// <summary>
        /// MouseState=1:当鼠标双击ROI之外的图像区域时候,保留已存在ROI区域;
        /// MouseState=2:根据鼠标在8个矩形框位置来确定是改变宽度还是高度;
        /// </summary>
    /*    private void KeepMouseState(int MouseState, MouseEventArgs e)
        {

            if (m_OperateRectangle[0].Contains(e.Location))
            {
                MouseMovePoint.X = this.CaptureRectangle.Right;
                MouseMovePoint.Y = this.CaptureRectangle.Bottom;
            }
            else if (m_OperateRectangle[1].Contains(e.Location))
            {
                MouseMovePoint.Y = this.CaptureRectangle.Bottom;
                BlockWidth = true;//锁定ROI宽度

            }
            else if (m_OperateRectangle[2].Contains(e.Location))
            {
                MouseMovePoint.X = this.CaptureRectangle.X;
                MouseMovePoint.Y = this.CaptureRectangle.Bottom;
            }
            else if (m_OperateRectangle[3].Contains(e.Location))
            {
                MouseMovePoint.X = this.CaptureRectangle.Right;
                BlockHeight = true;//锁定ROI高度
            }
            else if (m_OperateRectangle[4].Contains(e.Location))
            {
                MouseMovePoint.X = this.CaptureRectangle.X;
                BlockHeight = true;//锁定ROI高度
            }
            else if (m_OperateRectangle[5].Contains(e.Location))
            {
                MouseMovePoint.X = this.CaptureRectangle.Right;
                MouseMovePoint.Y = this.CaptureRectangle.Y;
            }
            else if (m_OperateRectangle[6].Contains(e.Location))
            {
                MouseMovePoint.Y = this.CaptureRectangle.Y;
                BlockWidth = true;//锁定ROI宽度

            }
            else if (m_OperateRectangle[7].Contains(e.Location))
            {
                MouseMovePoint = this.CaptureRectangle.Location;

            }
            else if (this.CaptureRectangle.Contains(e.Location))
            {

            }
            else
            {
                if (MouseState == 1)//当在ROI矩形外按下鼠标,保留现有的ROI,防止ROI被刷掉
                {
                    StartDraw = false;
                }

            }

        }


        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                StartDraw = true;
                BlockWidth = false;
                BlockHeight = false;
                KeepMouseState(1, e);
                MouseDownPoint = new Point(e.X, e.Y);
                //通过鼠标移动坐标位置来判断是改变ROI大小还是移动ROI操作
                if (FinishedDraw)
                {
                    if (this.CaptureRectangle.Contains(e.Location))
                    {
                        IsMoving = true;
                    }
                    else
                    {
                        IsMoving = false;
                    }
                }
            }
        }


        /// <summary>
        /// 鼠标弹起事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseUp(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                StartDraw = false;
                TempStart_Point = CaptureRectangle.Location;//记录下鼠标弹起时,ROI当前位置
                CutImage();
            }
        }


        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {

            ChangeCursor(e.Location, true);

            if (StartDraw)
            {

                KeepMouseState(2, e);
            }



            if (StartDraw)
            {
                if (IsMoving)
                {
                    this.CaptureRectangle.X = TempStart_Point.X + e.X - MouseDownPoint.X;
                    this.CaptureRectangle.Y = TempStart_Point.Y + e.Y - MouseDownPoint.Y;
                    if (this.CaptureRectangle.X < 0) this.CaptureRectangle.X = 0;
                    if (this.CaptureRectangle.Y < 0) this.CaptureRectangle.Y = 0;
                    if (this.CaptureRectangle.Right > OriginImage.Width) this.CaptureRectangle.X = OriginImage.Width - this.CaptureRectangle.Width - 1;
                    if (this.CaptureRectangle.Bottom > OriginImage.Height) this.CaptureRectangle.Y = OriginImage.Height - this.CaptureRectangle.Height - 1;

                }
                else
                {

                    if (Math.Abs(e.X - MouseMovePoint.X) > 1 || Math.Abs(e.Y - MouseMovePoint.Y) > 1)
                    {
                        if ((e.X >= 0 && e.X <= this.OriginImage.Width) && (e.Y >= 0 && e.Y <= this.OriginImage.Height))
                        {
                            //当前坐标在图像区域
                        }
                        else
                        {
                            //当前坐标不在图像区域
                            return;
                        }

                        if (!BlockWidth)
                        {
                            CaptureRectangle.X = MouseMovePoint.X - e.X < 0 ? MouseMovePoint.X : e.X;//以CaptureRectangle的Left或Right为正负分界线,MouseMovePoint.X - e.X < 0:正向方向改变大小,MouseMovePoint.X - e.X > 0:负向方向改变大小
                            CaptureRectangle.Width = Math.Abs(MouseMovePoint.X - e.X);
                        }


                        if (!BlockHeight)
                        {
                            CaptureRectangle.Y = MouseMovePoint.Y - e.Y < 0 ? MouseMovePoint.Y : e.Y;//以CaptureRectangle的Top或Bttom为正负分界线,MouseMovePoint.Y - e.Y < 0:正向方向改变大小,MouseMovePoint.Y - e.Y > 0:负向方向改变大小
                            CaptureRectangle.Height = Math.Abs(MouseMovePoint.Y - e.Y);
                        }


                    }

                }

                //使画布无效,从而执行OnPaint()
                this.DisplayImage_pictureBox.Invalidate();

            }


        }

*/
        /// <summary>
        /// 控件Paint事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPaint(object sender, PaintEventArgs e)
        {

            if (FinishedDraw)
            {
                Graphics CG = DisplayImage_pictureBox.CreateGraphics();
                Pen p = new Pen(Color.Yellow, 5);
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

                for (int i = 0; i <=cap_samples_num - 1;i++ )
                {
                    CaptureRectangle1[i].X = this.CaptureRectangle.X*5 + Rect_dist*i;
                    CaptureRectangle1[i].Y = this.CaptureRectangle.Y;
                    CaptureRectangle1[i].Width = this.CaptureRectangle.Width*5;
                    CaptureRectangle1[i].Height = this.CaptureRectangle.Height*5;
                }

                if (cap_samples_num > 0)
                {

                    for (int i = 0; i < cap_samples_num; i++)
                    {

                        e.Graphics.DrawRectangle(p, CaptureRectangle1[i]);

                    }
                }



             
            }

        }


        /// <summary>
        /// 绘制8个小矩形框
        /// </summary>
        /// <param name="g"></param>
        protected virtual void DrawOperationBox(Graphics g)
        {
          
            m_OperateRectangle[0].X = this.CaptureRectangle.X - 5;
            m_OperateRectangle[0].Y = this.CaptureRectangle.Y - 5;
            m_OperateRectangle[1].X = this.CaptureRectangle.X + this.CaptureRectangle.Width / 2 - 5;
            m_OperateRectangle[1].Y = this.CaptureRectangle.Y = this.CaptureRectangle.Y - 7;
            m_OperateRectangle[2].X = this.CaptureRectangle.Right - 5;
            m_OperateRectangle[2].Y = this.CaptureRectangle.Y - 5;
            m_OperateRectangle[3].X = this.CaptureRectangle.X - 7;
            m_OperateRectangle[3].Y = this.CaptureRectangle.Y + this.CaptureRectangle.Height / 2 - 5;
            m_OperateRectangle[4].X = this.CaptureRectangle.Right - 2;
            m_OperateRectangle[4].Y = this.CaptureRectangle.Y + this.CaptureRectangle.Height / 2 - 5;
            m_OperateRectangle[5].X = this.CaptureRectangle.X - 5;
            m_OperateRectangle[5].Y = this.CaptureRectangle.Bottom - 5;
            m_OperateRectangle[6].X = this.CaptureRectangle.X + this.CaptureRectangle.Width / 2 - 5;
            m_OperateRectangle[6].Y = this.CaptureRectangle.Bottom - 2;
            m_OperateRectangle[7].X = this.CaptureRectangle.Right - 5;
            m_OperateRectangle[7].Y = this.CaptureRectangle.Bottom - 5;
          

            if (this.CaptureRectangle.Width > 10 && this.CaptureRectangle.Height > 10)
            {
                SolidBrushObject.Color = Color.YellowGreen;
                foreach (Rectangle rect in m_OperateRectangle)
                {
                    g.FillRectangle(SolidBrushObject, rect);
                }
                
              
            }

        }


        /// <summary>
        /// 画矩形
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDrawRectangle(object sender, EventArgs e)
        {
            int mag = 1;
            square= mag * Convert.ToInt32(Width_textbox.Text)*DisplayImage_pictureBox.Width/ OriginImage.Width;
            Rect_dist = mag* Convert.ToInt32(dist_textbox.Text) * DisplayImage_pictureBox.Width / OriginImage.Width;

            CaptureRectangle.X = mag*Convert.ToInt32(PointX.Text) * DisplayImage_pictureBox.Height / OriginImage.Height;//;OriginImage.Width / 2;
            CaptureRectangle.Width =square;

            square = mag * Convert.ToInt32(Width_textbox.Text) * DisplayImage_pictureBox.Height / OriginImage.Height;
            CaptureRectangle.Y = mag * Convert.ToInt32(PointY.Text) * DisplayImage_pictureBox.Height / OriginImage.Height;//OriginImage.Height / 2;
            CaptureRectangle.Height = square;

            TempStart_Point = CaptureRectangle.Location;
            FinishedDraw = true;
            IsMoving = false;
            this.DisplayImage_pictureBox.Invalidate();
            

        }


      


        /// <summary>
        /// 保存halcon Hobject图像到本地文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConvertHalcon(object sender, EventArgs e)
        {
            Bitmap CutBitmap = new Bitmap("demo.bmp");//CutImage();

            HObject Image = BitmapToHobject(CutBitmap.Width, CutBitmap.Height, CutBitmap);

            string FileName = DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒fff毫秒");
            HOperatorSet.WriteImage(Image, "bmp", 0, Directory.GetCurrentDirectory() + "/" + FileName + ".bmp");

        }
        

        /// <summary>
        /// Bitmap位图转halcon Hobject图像类型
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="MyBitmap"></param>
        /// <returns></returns>
        public HObject BitmapToHobject(int Width, int Height, System.Drawing.Bitmap MyBitmap)
        {
            HObject Image;

            System.Drawing.Imaging.BitmapData BitmapData = MyBitmap.LockBits(new Rectangle(0, 0, Width, Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            unsafe
            {
                //图像数据排列BGR
                byte* Inptr = (byte*)(BitmapData.Scan0.ToPointer());
                byte[] R_OutBuffer = new byte[Width * Height];
                byte[] G_OutBuffer = new byte[Width * Height];
                byte[] B_OutBuffer = new byte[Width * Height];
                fixed (byte* R_Outptr = R_OutBuffer, G_Outptr = G_OutBuffer, B_Outptr = B_OutBuffer)
                {

                    for (int i = 0; i < Height; i++)
                    {
                        for (int j = 0; j < Width; j++)
                        {
                            int Index = (i * Width + j) * 4;
                            B_OutBuffer[Index / 4] = (byte)Inptr[Index + 0];
                            G_OutBuffer[Index / 4] = (byte)Inptr[Index + 1];
                            R_OutBuffer[Index / 4] = (byte)Inptr[Index + 2];
                        }

                    }
                    MyBitmap.UnlockBits(BitmapData);

                    HOperatorSet.GenImage3(out Image, "byte", Width, Height, new IntPtr(R_Outptr), new IntPtr(G_Outptr), new IntPtr(B_Outptr));

                    return Image;

                }
            }

        }

        private void cap_samples_TextChanged(object sender, EventArgs e)
        {

            if (cap_samples_textbox.Text=="")
              cap_samples_textbox.Text = "6";
           cap_samples_num= Convert.ToInt32(cap_samples_textbox.Text);
            PublicClass.Cap_NUM = cap_samples_num;
           CapMsg CM3 ;
           CM3.numberCap = cap_samples_num;
           CM3.CapLength = 50;
           CM3.CapDist = 10;

           CM3.X = 50;
           CM3.Y = 50;
           CM3.CapWidth = 40;

          
           if (cap_samples_num > 0)
           {
               CaptureRectangle1 = new Rectangle[cap_samples_num];
               for (int i = 0; i < cap_samples_num-1; i++)
               {
                   CaptureRectangle1[i] = new Rectangle(20 + i * Rect_dist, OriginImage.Height/2, CaptureRectangle.Width, CaptureRectangle.Height);
               }
           }

            OnDrawRectangle(null,null);

        }

        private void samplesWidth_TextChanged(object sender, EventArgs e)
        {

            try
            {
                Rect_dist = maga*Convert.ToInt32(dist_textbox.Text) * DisplayImage_pictureBox.Width / 8192;
                PublicClass.Period = Rect_dist;
                OnDrawRectangle(null, null);
            }
            catch (Exception)
            {
               
            }
        }

        private void Length_textbox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                CaptureRectangle.Width = maga * Convert.ToInt32(Width_textbox.Text) * DisplayImage_pictureBox.Width / 8192;
                CaptureRectangle.Height = maga * Convert.ToInt32(Width_textbox.Text) * DisplayImage_pictureBox.Height / 4908;
                PublicClass.Width = Convert.ToInt32(Width_textbox.Text);
                OnDrawRectangle(null, null);
            }
            catch (Exception)
            {

               
            }
        }

    

        private void PointY_TextChanged(object sender, EventArgs e)
        {
            try
            {
                CaptureRectangle.Y = maga * Convert.ToInt32(PointY.Text) * DisplayImage_pictureBox.Height / 4908;
                PublicClass.OriginY = maga * Convert.ToInt32(PointY.Text);
                OnDrawRectangle(null, null);
            }
            catch (Exception)
            {


            }
        }
	

        private void PointX_TextChanged(object sender, EventArgs e)
        {
            try
            {
                CaptureRectangle.X = maga * Convert.ToInt32(PointX.Text) * DisplayImage_pictureBox.Width / 8192;
                PublicClass.OriginX = maga * Convert.ToInt32(PointX.Text);
                OnDrawRectangle(null, null);
            }
            catch (Exception)
            {

               
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            writeRectMsgtoini();

        }

        private void writeRectMsgtoini()
        {

            try
            {
                cinifile.IniWriteValue("检测框信息", "采样个数", cap_samples_textbox.Text.Trim());
                cinifile.IniWriteValue("检测框信息", "采样周期", dist_textbox.Text.Trim());
                cinifile.IniWriteValue("检测框信息", "采样宽度", Width_textbox.Text.Trim());
                cinifile.IniWriteValue("检测框信息", "采样起点X", PointX.Text.Trim());
                cinifile.IniWriteValue("检测框信息", "采样起点Y", PointY.Text.Trim());
                PublicClass.parChanged = true;
            }
            catch (Exception)
            {

               
            }
     
           
        }

        private void setting_SizeChanged(object sender, EventArgs e)
        {
            asc.controllInitializeSize(this);
        }
        private void StartKiller()
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; //3秒启动 
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
           // KillMessageBox();
            //停止Timer 
          //  ((System.Windows.Forms.Timer)sender).Stop();
        }

        //private void KillMessageBox()
        //{
        //    //按照MessageBox的标题，找到MessageBox的窗口 
        //    IntPtr ptr = FindWindow(null, "提示");
        //    if (ptr != IntPtr.Zero)
        //    {
        //        //找到则关闭MessageBox窗口 
        //        PostMessage(ptr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        //    }
        //}

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {


            

            try
            {
                if (PublicClass.serialPort1.IsOpen)
                {
                    byte[] readBuffer = new byte[PublicClass.serialPort1.ReadBufferSize];
                    PublicClass.serialPort1.Read(readBuffer, 0, readBuffer.Length);

                    string readstr1 = Encoding.UTF8.GetString(readBuffer);
                    int index = readstr1.IndexOf("\0");
                    string readstr = readstr1.Substring(0, index);
                    string stratChar = readstr.Substring(0, 1);
                    string endChar = readstr.Substring(readstr.Length - 1, 1);
                    if (stratChar == "*" && endChar == "#")
                    {
                        string a = readstr.Substring(1, readstr.Length - 2);
                        if (a == "0164D4")
                        {
                        }
                        //    if (tryingConnection)
                        //    {
                        //        ConnectionIsReady = true;
                        //        StartKiller();
                        //        MessageBox.Show("通信成功", "提示");
                        //    }
                        //}
                        else
                        {
                            if (a == "026594")
                            {

                            }
                            else
                            {
                                a = a.Substring(0, a.Length - 4);
                                int index1 = a.IndexOf("!");
                                string strX = a.Substring(1, index1 - 1);
                                string strY = a.Substring(index1 + 1, a.Length - 3 - strX.Length);
                  

                            }
                        }

                    }

                }

            }
            catch (Exception err)
            {
               // throw err;
            }
        }
        public void serialPortSelectfun()
        {
            //int cc = 1;
            //if (serialPort1.IsOpen)
            //    serialPort1.Close();
            //try
            //{
            //    serialPort1.PortName = comboBox1.Text;
             // serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);
            //    serialPort1.Open();
            //    SerialPortIsReady = true;
            //    StartKiller();
            //    MessageBox.Show("串口打开成功", "提示");
            //    tryingConnection = true;
            //   // timer3.Enabled = true;
            //    serialPort1.WriteLine("*00003F1B#");

            //}
            //catch
            //{
            //    StartKiller();
            //    MessageBox.Show("串口打开失败" + cc.ToString(), "提示");

            //}
        }

        private void btn_openserial_Click(object sender, EventArgs e)
        {
            

            //if (true)
            //{
            //    serialPort1.Open();
            //    if (serialPort1.IsOpen)
            //    MessageBox.Show("串口打开成功", "提示");
            //    return;
            //}
            
            
            //else
            //{
            //    serialPortSelectfun();
            //    if (SerialPortIsReady)
            //    {
            //        SerialPortIsReady = true;
            //       // btnOpenClosePort.Text = "关闭串口";
            //        comboBox1.Enabled = false;
            //    }

            //}
        }

        private void btn_closeserial_Click(object sender, EventArgs e)
        {
        }
        //    if (serialPort1.IsOpen)
        //    {
        //        try
        //        {

        //            serialPort1.Close();
        //            // StartKiller();
        //            MessageBox.Show("串口关闭成功", "提示");
        //            comboBox1.Enabled = true;
        //            //  btnOpenClosePort.Text = "打开串口";
        //        }
        //        catch
        //        {
        //            // StartKiller();
        //            MessageBox.Show("串口打开失败", "提示");
        //        }
        //    }
        //}


      
        public static byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }
        private void btn_startmotor_Click(object sender, EventArgs e)
        {
            //0x01 0x05 0x07 0xD0 0xFF 0x00 0x8C 0xB7
            string datapacket = "01 05 07 D0 FF 00 8C B7";  // FE 05 00 01 00 00 88 05
            //string datapacket = "01 03 00 00 00 01 84 0A";
            byte[] array = HexStringToByteArray(datapacket);
            if (PublicClass.serialPort1.IsOpen)
            {
                PublicClass.serialPort1.Write(array, 0, array.Length);
            }
            Thread.Sleep(10);
        }

        private void btn_stopmotor_Click(object sender, EventArgs e)
        {
            //0x01 0x05 0x07 0xD0 0x00 0x00 0xCD 0x47
            string datapacket = "01 05 07 D0 00 00 CD 47";  // FE 05 00 01 00 00 88 05
            byte[] array = HexStringToByteArray(datapacket);
            PublicClass.serialPort1.Write(array, 0, array.Length);
            Thread.Sleep(500);

        }

        private void button11_Click(object sender, EventArgs e)
        {
           // 0x01 0x05 0x07 0xD1 0xFF 0x00 0xDD 0x77
            string datapacket = "01 05 07 D1 FF 00 DD 77";  // FE 05 00 01 00 00 88 05
            byte[] array = HexStringToByteArray(datapacket);
            if (PublicClass.serialPort1.IsOpen)
            {
                PublicClass.serialPort1.Write(array, 0, array.Length);
            }
            Thread.Sleep(10);


        }

        private void button12_Click(object sender, EventArgs e)
        {
            // 0x01 0x05 0x07 0xD1 0x00 0x00 0x9C 0x87
            //开始打标 0x01 0x05 0x07 0xD2 0xFF 0x00 0x2D 0x77
            //打标结束T  0x01 0x01 0x07 0xD0 0x00 0x10 0x3D 0x4B
            //打标机运行结束0x01 0x01 0x02 0x04 0x00 0xBB 0x3C(打标机运行中)或
            // R ： 0x01 0x01 0x02 0x00 0x00 0xB9 0xFC(打标机运行结束)
            string datapacket = "01 05 07 D1 00 00 9C 87";  // FE 05 00 01 00 00 88 05
            byte[] array = HexStringToByteArray(datapacket);
            if (PublicClass.serialPort1.IsOpen)
            {
                PublicClass.serialPort1.Write(array, 0, array.Length);
            }
            Thread.Sleep(10);
        }

        private void btn_setvalue_Click(object sender, EventArgs e)
        {
            // 0x01 0x06 0x00 0x00 0x01 0xC2 0x09 0xCB
            string tempSpeed = Convert.ToInt32(textBox7.Text).ToString("X").PadLeft(4, '0'); ;
            // 0x01 0x06 0x00 0x00 0x01 0xC2 0x09 0xCB
            string datapacket = "01060000" + tempSpeed;  // FE 05 00 01 00 00 88 05
            byte[] array = HexStringToByteArray(datapacket);
            string CRCString = ToCRCString(array);
            array = HexStringToByteArray(datapacket + CRCString);
            if(PublicClass.serialPort1.IsOpen)
            {
                PublicClass.serialPort1.Write(array, 0, array.Length);
                PublicClass.speed = Convert.ToInt32(textBox7.Text);
            }
           
            Thread.Sleep(10);
        }

        private void btn_sendcmd_Click(object sender, EventArgs e)
        {
         
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //if (true)
            //{
            //    serialPort2.Open();
            //    if (serialPort2.IsOpen)
            //        MessageBox.Show("串口打开成功", "提示");
            //    return;
            //}
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //if (serialPort2.IsOpen)
            //{
            //    try
            //    {

            //        serialPort2.Close();
            //        // StartKiller();
            //        MessageBox.Show("串口关闭成功", "提示");
            //        comboBox1.Enabled = true;
            //        //  btnOpenClosePort.Text = "打开串口";
            //    }
            //    catch
            //    {
            //        // StartKiller();
            //        MessageBox.Show("串口打开失败", "提示");
            //    }
            //}
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            trackBar1.Minimum =Convert.ToInt32(textBox9.Text);
            trackBar1.SmallChange = (trackBar1.Maximum - trackBar1.Minimum) / 5;
            trackBar1.LargeChange = (trackBar1.Maximum - trackBar1.Minimum) / 5 + 1;
            trackBar1.TickFrequency = (trackBar1.Maximum - trackBar1.Minimum) / 5;
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
           
            trackBar1.Maximum = Convert.ToInt32(textBox10.Text);
            trackBar1.SmallChange = (trackBar1.Maximum - trackBar1.Minimum) / 5;
            trackBar1.LargeChange = (trackBar1.Maximum - trackBar1.Minimum) / 5 + 1;
            trackBar1.TickFrequency = (trackBar1.Maximum - trackBar1.Minimum) / 5;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

            textBox4.Text = trackBar1.Value.ToString();
        }

        private void button9_Click(object sender, EventArgs e)
        {


          

            AxXJaiLT400CL AxJ400Cl0 = new AxXJaiLT400CL();
            AxXJaiLT400CL AxJ400Cl1 = new AxXJaiLT400CL();
            ((System.ComponentModel.ISupportInitialize)(AxJ400Cl0)).BeginInit();
            AxJ400Cl0.Visible = false;
            this.Controls.Add(AxJ400Cl0);
            ((System.ComponentModel.ISupportInitialize)(AxJ400Cl0)).EndInit();
            AxJ400Cl1.Visible = false;
            Thread.Sleep(500);
            ((System.ComponentModel.ISupportInitialize)(AxJ400Cl1)).BeginInit();
            this.Controls.Add(AxJ400Cl1);
            ((System.ComponentModel.ISupportInitialize)(AxJ400Cl1)).EndInit();

            AxJ400Cl0.ShowControlWindow = true;
            AxJ400Cl0.ShowCommunicationWindow = true;
            AxJ400Cl1.ShowControlWindow = true;
            AxJ400Cl1.ShowCommunicationWindow = true;
           

            //if (p == null)
            //{
            //    p = new System.Diagnostics.Process();
            //    p.StartInfo.FileName = Application.StartupPath+ "\\Win32_SerialCom\\SerialCom.exe";
            //    p.Start();
            //    //73 65 74 20 35 34 30 30 0D:5400
            //    //73 73 66 20 31 32 0D:12
            //}
            //else
            //{
            //    if (p.HasExited) //是否正在运行
            //    {
            //        p.Start();
            //    }
            //}
            //p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;


        }

        private void button2_Click(object sender, EventArgs e)
        {
            //MySqlCommand cmd = null;
            //MySqlDataReader reader = null;
            //List<string> list_ColName = new List<string>();
            //List<Type> list_ColType = new List<Type>();
            //List<string> list_finally = new List<string>();

            //string sql1 = DateTime.Now.Date.ToString("yyyyMMdd0");
            //// sql1 = sql1 + MetersCounter.ToString();
            ////"2019022104";
            //bool bfirstinsert = true;
            //string sql = "";
            //using (MySqlConnection cnn = new MySqlConnection(conectionstring))
            //{
            //    cnn.Open();
            //    // if (!cnn.Open()) { return list_ColName; }

            //    if (bfirstinsert)
            //    {
            //        //主键数值，初次嵌入

            //        sql = "insert into ClothClass(ClothClassNumber) values(" + ClothClsaaNumber.Text + ")";
            //        list_finally.Add(sql);
                    

            //        sql = "insert into Clothdetectdetail(ClothNumber) values(" + ClothNumber.Text +")";
            //        list_finally.Add(sql);



            //        sql = "insert into ClothCylinder(CylinderNumber) values('" + CylinderNumber.Text + "')";
            //        list_finally.Add(sql);

            //        sql = "insert into ClothEquipment(EquipmentNumber) values('" + EquipmentNumber.Text + "')";
            //        list_finally.Add(sql);

            //        sql = "insert into ClothSource(SourceOfCloth) values(" + "'origin3'" + ")";
            //        list_finally.Add(sql);


            //        //更新指定主键列数值下的各个列值

            //        sql = "UPDATE ClothClass SET ClothColor = '" + ClothColor.Text + "' WHERE ClothClassNumber = " + ClothClsaaNumber.Text;
            //        list_finally.Add(sql);

            //        sql = "update Clothdetectdetail set BatchNumber=" + BatchNumber.Text + " WHERE ClothNumber = " + ClothNumber.Text;
            //        list_finally.Add(sql);

            //        sql = "update Clothdetectdetail set TotalVolumn=" + TotalVolumn.Text + " WHERE ClothNumber = " + ClothNumber.Text;
            //        list_finally.Add(sql);

            //        sql = "update Clothdetectdetail set CylinderNumber='" + CylinderNumber.Text + "' WHERE ClothNumber = " + ClothNumber.Text;
            //        list_finally.Add(sql);

            //        sql = "update Clothdetectdetail set EquipmentNumber='" + EquipmentNumber.Text + "' WHERE ClothNumber = " + ClothNumber.Text;
            //        list_finally.Add(sql);

            //        sql = "update Clothdetectdetail set LabelMeters=" + LabelMeters.Text + " WHERE ClothNumber = " + ClothNumber.Text;
            //        list_finally.Add(sql);

            //        //sql = "update " + "Clothdetectdetail" + " set " + column_name + "=" + value + " where  MeterNumber=" + listtype[2].ToString();
            //       // list_finally.Add(sql);



            //        //批量执行语句
            //        MD.ExecuteSqlTran(list_finally);

            //        MD.DisplayMessageBox("插入detail数据成功", 1);

            //        //cmd.CommandText = sql;

            //        //cmd.ExecuteNonQuery();

            //        //MD.DisplayMessageBox("插入result数据表成功！", 1);


            //        //sql = "insert into clothdetectdetail(ClothNumber) values(" + sql1 + ")";


            //        //cmd = new MySqlCommand(sql, cnn);

            //        //cmd.ExecuteNonQuery();


                    //bfirstinsert = false;

                    //sql = "insert into clothdetectresult(ClothNumber,IntervalMeter,MeterNumber,BoxNumber) values(" +
                    //      sql1 + ",0.1,0.2,2)";

                    //cmd.CommandText = sql;

                    //cmd.ExecuteNonQuery();

                    //MD.DisplayMessageBox("插入result数据表成功！", 1);




               // }





           // }
        }


        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isFirtOpen)
            {              
                return;
                
            }
            PublicClass.serialPort1Name = comboBox1.Text;
            if (PublicClass.serialPort1.IsOpen)
                PublicClass.serialPort1.Close();
            try
            {
                PublicClass.serialPort1.PortName = comboBox1.Text;
                PublicClass.serialPort1.Open();
                label24.Text = "打开";
                button13.Text = "关闭串口";
            }
            catch
            {
                MessageBox.Show("串口打开失败！");
                label24.Text = "关闭";
                button13.Text = "打开串口";
            }
        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isFirtOpen)
            {
               
                return;
            }
            PublicClass.serialPort2Name = comboBox2.Text;
            if (PublicClass.serialPort2.IsOpen)
                PublicClass.serialPort2.Close();
            try
            {
                PublicClass.serialPort2.PortName = comboBox2.Text;
                PublicClass.serialPort2.Open();
                label31.Text = "打开";
                button14.Text = "关闭串口";
            }
            catch
            {
                MessageBox.Show("串口打开失败！");
                label31.Text = "关闭";
                button14.Text = "打开串口";
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isFirtOpen)
            {
               
                return;
            }
            PublicClass.serialPort3Name = comboBox3.Text;
            if (PublicClass.serialPort3.IsOpen)
                PublicClass.serialPort3.Close();
            try
            {
                PublicClass.serialPort3.PortName = comboBox3.Text;
                PublicClass.serialPort3.Open();
                label33.Text = "打开";
                button15.Text = "关闭串口";
            }
            catch
            {
                MessageBox.Show("串口打开失败！");
                label33.Text = "关闭";
                button15.Text = "打开串口";
            }

        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (button13.Text == "关闭串口")
            {
                try
                {
                    PublicClass.serialPort1.Close();
                    button13.Text = "打开串口";

                }
                catch
                {
                    MessageBox.Show("串口关闭失败！");
                }


            }
            else
            {
                try
                {
                    PublicClass.serialPort1.PortName = comboBox1.Text;
                    PublicClass.serialPort1.Open();
                    label24.Text = "打开";
                    button13.Text = "关闭串口";
                }
                catch
                {
                    MessageBox.Show("串口打开失败！");
                    label24.Text = "关闭";
                    button13.Text = "打开串口";
                }
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (button14.Text == "关闭串口")
            {
                try
                {
                    PublicClass.serialPort1.Close();
                    button14.Text = "打开串口";

                }
                catch
                {
                    MessageBox.Show("串口关闭失败！");
                }


            }
            else
            {
                try
                {
                    PublicClass.serialPort3.PortName = comboBox2.Text;
                    PublicClass.serialPort3.Open();
                    label31.Text = "打开";
                    button14.Text = "关闭串口";
                }
                catch
                {
                    MessageBox.Show("串口打开失败！");
                    label31.Text = "关闭";
                    button14.Text = "打开串口";
                }
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (button15.Text == "关闭串口")
            {
                try
                {
                    PublicClass.serialPort1.Close();
                    button15.Text = "打开串口";

                }
                catch
                {
                    MessageBox.Show("串口关闭失败！");
                }


            }
            else
            {
                try
                {
                    PublicClass.serialPort3.PortName = comboBox3.Text;
                    PublicClass.serialPort3.Open();
                    label33.Text = "打开";
                    button15.Text = "关闭串口";
                }
                catch
                {
                    MessageBox.Show("串口打开失败！");
                    label33.Text = "关闭";
                    button15.Text = "打开串口";
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
           
            if (PublicClass.serialPort1.IsOpen)
            {
                button13.Text = "关闭串口";
                label24.Text = "打开";
            }
            else
            {
                button13.Text = "打开串口";
                label24.Text = "关闭";
            }
            if (PublicClass.serialPort2.IsOpen)
            {
                button14.Text = "关闭串口";
                label31.Text = "打开";
            }
            else
            {
                button14.Text = "打开串口";
                label31.Text = "关闭";
            }
            if (PublicClass.serialPort3.IsOpen)
            {
                button15.Text = "关闭串口";
                label33.Text = "打开";
            }
            else
            {
                button15.Text = "打开串口";
                label33.Text = "关闭";
            }
            textBox11.Text = PublicClass.speed.ToString();

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Enabled = false;
            if (isFirtOpen)
            {
                isFirtOpen = false;
            }
        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void button16_Click(object sender, EventArgs e)
        {
            PublicClass.serialPort1Name = comboBox1.Text;
            PublicClass.serialPort2Name = comboBox2.Text;
            PublicClass.serialPort3Name = comboBox3.Text;

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
                      
            
            
        }

        private void button2_Click_1(object sender, EventArgs e)
        {

            try
            {
                
                // PublicClass.minExposureTime = Convert.ToInt32(textBoxMinExposueTime.Text);
                // PublicClass.maxExposureTime = Convert.ToInt32(textBoxMaxExposueTime.Text);
                PublicClass.mag = Convert.ToDouble(textBox_Mag.Text);
                PublicClass.Thresh = Convert.ToInt32(textBox_Gray.Text);
                PublicClass.dynthresh = Convert.ToInt32(textBox_DynThresh.Text);
               // PublicClass.ra = Convert.ToInt32(textBox_Ra.Text);
                PublicClass.defectArea = Convert.ToDouble(textBox_Area.Text);
                PublicClass.defectWidth = Convert.ToDouble(textBox_DefectWidth.Text);
                PublicClass.defectHeight = Convert.ToDouble(textBox_DefectWidth.Text);

                PublicClass.rightside = Convert.ToDouble(textBox_RightSide.Text);
                PublicClass.leftside = Convert.ToDouble(textBox_LeftSide.Text);
                PublicClass.EdgeRollslope = Convert.ToDouble(textBox_EdgeSlope.Text);
                PublicClass.imperfectBorderWidth = Convert.ToDouble(textBox_imBorderWidth.Text);
                PublicClass.clothSideUnDetectWidth = Convert.ToDouble(textBox_SideUnDetectWidth.Text);
                PublicClass.EdgeRollslope = Convert.ToDouble(textBox_EdgeSlope.Text);

                writeRectMsgtoini();
                PublicClass.parChanged = true;
                SaveParmeterToDisk1();
                

            }
            catch
            {
                MessageBox.Show("参数有误！");
            }


           
        }

        private void button3_Click_1(object sender, EventArgs e)
        {

        }

        void SaveParmeterToDisk1()//保存检测到ini文件        {
        {
            try

            {
                strSec = Path.GetFileNameWithoutExtension(parstrFilePath);

                WritePrivateProfileString(strSec, "minExposureTime", PublicClass.minExposureTime.ToString().Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "maxExposureTime", PublicClass.maxExposureTime.ToString().Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "thresh", PublicClass.Thresh.ToString().Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "dynThresh", PublicClass.dynthresh.ToString().Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "ra", PublicClass.ra.ToString().Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "Area", PublicClass.defectArea.ToString().Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "sideWidth", PublicClass.sideWidth.ToString().Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "portName", PublicClass.serialPort1Name.ToString().Trim(), parstrFilePath);

                WritePrivateProfileString(strSec, "mag", PublicClass.mag.ToString().Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "rightside", PublicClass.rightside.ToString().Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "leftside", PublicClass.leftside.ToString().Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "clothSideUnDetectWidth", PublicClass.clothSideUnDetectWidth.ToString().Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "imperfectBorderWidth", PublicClass.imperfectBorderWidth.ToString().Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "EdgeRollslope", PublicClass.EdgeRollslope.ToString().Trim(), parstrFilePath);

                WritePrivateProfileString(strSec, "defectArea", PublicClass.defectArea.ToString().Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "defectHeight", PublicClass.defectHeight.ToString().Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "defectWidth", PublicClass.defectWidth.ToString().Trim(), parstrFilePath);

                WritePrivateProfileString(strSec, "defectArea", PublicClass.defectArea.ToString().Trim(), parstrFilePath);




               // listBox1.Items.Add("参数设置成功！" + " " + DateTime.Now.ToLongTimeString().ToString());
               // try { listBox1.TopIndex = listBox1.Items.Count - 1; }
               // catch
                //{// WriteErrorFile("写表格出错");
               // }
            }
            catch
            {
               // listBox1.Items.Add("参数设置失败！" + " " + DateTime.Now.ToLongTimeString().ToString());
                try
                {
                //    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                catch
                {
                    // WriteErrorFile("写表格出错");
                }
                // WriteErrorFile("参数设置出错");
            }
        }

        private void textBox_LeftSide_TextChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)

                PublicClass.detectDefectsFlag = 1;
            else
            {
                PublicClass.detectDefectsFlag = 0;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

            if (checkBox2.Checked)

                PublicClass.edgeRollFlag = 1;
            else
            {
                PublicClass.edgeRollFlag = 0;
            }

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)

                PublicClass.imperfectBorderFlag = 1;
            else
            {
                PublicClass.imperfectBorderFlag = 0;
            }

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)

                PublicClass.otherFlag = 1;
            else
            {
                PublicClass.otherFlag = 0;
            }

        }

        private void button3_Click_2(object sender, EventArgs e)
        {
         //   string tempSpeed = Convert.ToInt32(textBox7.Text).ToString("X").PadLeft(4, '0'); ;
            // 0x01 0x06 0x00 0x00 0x01 0xC2 0x09 0xCB
            string datapacket = "01 03 00 1E 00 02 A4 0D"; 
            //01 03 00 1E 00 02 CD AB   
            byte[] array = HexStringToByteArray(datapacket);
          //  string CRCString = ToCRCString(array);
         //   array = HexStringToByteArray(datapacket + CRCString);
            if (PublicClass.serialPort1.IsOpen)
            {
                PublicClass.serialPort1.Write(array, 0, array.Length);
               // PublicClass.speed = Convert.ToInt32(textBox7.Text);
            }

            Thread.Sleep(10);
        }
    }
}
