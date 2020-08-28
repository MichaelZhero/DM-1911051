using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

using DALSA.SaperaLT.SapClassBasic;
using DALSA.SaperaLT.SapClassGui;

using MySql.Web;
using MySql.Data.MySqlClient;
using System.Windows.Forms.DataVisualization.Charting;
using HalconDotNet;
using System.Diagnostics;
using System.Windows.Forms.VisualStyles;
using System.Threading;
using System.IO.Ports;
using System.Xml.Schema;
using AxXJaiLT400CLLib;
using System.Configuration;

//using DevExpress.Utils.OAuth.Provider;
//using XJaiLT400CLLib;




namespace DALSA.SaperaLT.Demos.NET.CSharp.MultiBoardSyncGrabDemo
{
    public partial class MultiBoardSyncGrabDemoDlg : Form
    {

        // Delegate to display number of frame acquired 
        // Delegate is needed because .NEt framework does not support  cross thread control modification
        private delegate void delegate_DisplayFrameAcquired(int number, int cameraflag, bool trash);//显示帧率        
        private delegate void delegate_DisplayAberrationChart(double[] Colordiff, double meter, bool isshow);//显示图表
        private delegate void LoadParas(int num);   //加载参数
        private delegate void deget_standard_lab(int actionid, HTuple path);//计算标准lab
        private static Predicate<string> pre = new Predicate<string>(MyPredicate);

        [System.Runtime.InteropServices.DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filepath);

        [System.Runtime.InteropServices.DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);
        StringBuilder temp = new StringBuilder(1024);
        string strSec = "";
        private string parstrFilePath = Application.StartupPath + "\\par.ini";//定义参数保存文档
        private string myconfFilePath = Application.StartupPath + "\\MyConfig.ini";

        Dictionary<string, HTuple> DispResultDict = new Dictionary<string, HTuple>();
        Dictionary<string, string> DetectResultDict = new Dictionary<string, string>();
        Dictionary<string, string> DetectDetailDict = new Dictionary<string, string>();
        Dictionary<string, string> DetectReportDict = new Dictionary<string, string>();

        //启动线程
        MessageBoxHelper MsgBoxHelper = new MessageBoxHelper();

        //双相机通信模块对象
        AxXJaiLT400CL AxJ400Cl0 = new AxXJaiLT400CL();
        AxXJaiLT400CL AxJ400Cl1 = new AxXJaiLT400CL();


        public const int START_MOTOR = 1001;
        public const int STOP_MOTOR = 1002;
        public const int START_ALARM = 1003;
        public const int CANCEL_ALARM = 1004;
        bool clothDetailIsready = false;
        int exposureTime = 10;//曝光时间定义

        Stopwatch swStopwatch= new Stopwatch();

        //写入数据库委托
        private delegate void delegate_WriteToSqlbydict(string table_namew, string ClothNumber, Dictionary<string, string> LSring, int begin_index, int end_index);

        private delegate List<string> delegate_WriteToSql(string tablenaem, List<string> inputstring, int begin_index, int end_index, bool issuccess);
        //读取数据库委托
        private delegate List<string> delegate_ReadFromSql(string tablenaem, List<string> inputstring, bool issuccess);
        //调用外部程序声明，软件为数据库管理软件
        private static System.Diagnostics.Process ExternalSqlManager;

        //批量填入数据库语句
        List<string> MySQlColumnlist = new List<string>();
        List<string> MySQlColumnlist1 = new List<string>();


        //消息弹框
        MessageBoxForm messageboxForm;

        //加载配置文件
        IniFiles cinifile = new IniFiles(Application.StartupPath + @"\MyConfig.INI");

        //控件自适应屏幕类声明
        AutoSizeFormClass AutoSizeForm = new AutoSizeFormClass();

        //Halcon类声明
        HDevelopExport HalconProc = new HDevelopExport();
        //halcon窗口
        public HTuple hv_ExpDefaultWinHandle;
        //采集图像数量
        private int leftImgCounter = 0;
        private int rightImgCounter = 0;
        private int MetersCounter = 0;
        //图像数据指针
        IntPtr pImage;
        IntPtr pImage1;
        IntPtr pImage2;
        IntPtr pImage3;



        double Color_Value13;
        double Color_Value12;
        double Color_Value23;

        double[] Color_Value = new double[8];
        double[] Defect_Value = new double[13];
        string Imgpath = null;


        Queue<HObject> LeftImg = new Queue<HObject>();//双相机采集图像及合并图像队列
        Queue<HObject> RightImg = new Queue<HObject>();
        Queue<HObject> DetectImgQueue = new Queue<HObject>();
        Queue<HObject> DispImgQueue = new Queue<HObject>();


        HObject ImageTemp = null;
        HObject ImageTemp1 = null;
        HObject ImageTemp2 = null;
        HObject ImageTemp3 = null;
        HObject ImageTemp4 = null;
        HObject ImageTemp5 = null;
        HObject ImageTemp6 = null;
        HObject ImageBoxs = null;


        //六个检测框声明
        HObject ho_Rectangle = null;
        HObject ho_Rectangle1 = null;
        HObject ho_Rectangle2 = null;
        HObject ho_Rectangle3 = null;
        HObject ho_Rectangle4 = null;
        HObject ho_Rectangle5 = null;
        HObject ho_Boxs=null;



        //色差及缺陷检测参数
        HTuple hv_boxNumber, hv_boxWidth, hv_boxHeight;
        HTuple hv_boxBenginX, hv_medianKernal, hv_tupleL, hv_tupleA;
        HTuple hv_tupleB, hv_result, hv_clothAberrationGrad1, hv_clothAberrationGrad2;
        HTuple hv_clothAberrationGrad3, hv_clothAberrationGrad4;
        HTuple hv_standardTupleL, hv_standardTupleA, hv_standardTupleB;
        HTuple hv_Thresh, hv_dynThresh, hv_defectWidth, hv_defectHeight, hv_defectArea, hv_defectNumber, hv_tupleDefectX;
        HTuple hv_tupleDefectY, hv_tupleDefectRadius, hv_clothAberration;
        HTuple hv_WindowHandle = new HTuple(), hv_standardL, hv_standardA;
        HTuple hv_standardB, hv_L, hv_A, hv_B, hv_leftRightAberration2;
        HTuple hv_ClothRegionCoordinateX1= new HTuple(), hv_ClothRegionCoordinateX2= new HTuple();
        private int AberrationGrad, leftRightAberrationGrad;
        string defectstr;//缺陷结果字符串



        //数据库操作：列名、个数、sql语句；检测结果，写入数据库mysql的report、result与detail表-执行语句
        int result_table_counter;
        int detail_table_counter;
        int report_table_counter;

        List<string> ResultColumeName = new List<string>();
        List<string> DetailColumeName = new List<string>();
        List<string> ReportColumeName = new List<string>();
        List<string> AberrationGradColumeName = new List<string>();

        List<string> ResultValueList = new List<string>();
        List<string> DetailValueList = new List<string>();
        List<string> AberrationGradList = new List<string>();
        List<string> ReportValueList = new List<string>();



        //是否保存图像
        private bool isSavedimage;
        private bool bsavedimage;
        private HTuple hv_starttime = 0;
        private HTuple hv_endtime = 0;
        private HTuple intervaltime = 0;
        private bool bimgcoming = false;
        //图像路径、数据库路径
        private HTuple imgpath;
        private HTuple detectedimgpath;
        private string sqlimgpath;
        private string sqldetectedimgpath;

        private bool bfirstinsert = true;



        //相机回调函数：xfer_XferNotify
        void xfer_XferNotify1(object sender, SapXferNotifyEventArgs argsNotify)//static
        {
            MultiBoardSyncGrabDemoDlg GrabDlg = argsNotify.Context as MultiBoardSyncGrabDemoDlg;
            // If grabbing in trash buffer, do not display the image, update the
            // appropriate number of frames on the status bar instead
            // unsafe
            //   {
            if (argsNotify.Trash)
                // return;显示 帧率
                GrabDlg.Invoke(new delegate_DisplayFrameAcquired(GrabDlg.ShowFrameNumber), argsNotify.EventCount, 2, true);
            else
            {

                SapBuffer m_rgb888_buffer = new SapBuffer(1, m_Buffers[1].Width, m_Buffers[1].Height, SapFormat.RGB888, SapBuffer.MemoryType.Default);
                m_rgb888_buffer.Create();

                m_rgb888_buffer.Copy(m_Buffers[1]);
                m_rgb888_buffer.GetAddress(out pImage2);
                //获得原图像信息
                m_Buffers[1].GetAddress(out pImage3);


                //转化为HOBject；
                HTuple path;
                HObject RightImgtemp = null;
                //彩色图像获取
                HOperatorSet.GenImageInterleaved(out RightImgtemp, pImage2, "bgr", m_rgb888_buffer.Width, m_rgb888_buffer.Height, -1, "byte", 0, 0, 0, 0, -1, 0);
                HOperatorSet.CropPart(RightImgtemp, out RightImgtemp, 0, 125, m_rgb888_buffer.Width - 125, m_rgb888_buffer.Height);

                RightImg.Enqueue(RightImgtemp);
                //黑白图像获取
                //HOperatorSet.GenImage1(out ImageTemp, "byte", m_Buffer.Width, m_Buffer.Height, pImage);//取内存数据，生成图像，halcon实现
                GrabDlg.rightImgCounter++;

                GrabDlg.Invoke(new delegate_DisplayFrameAcquired(GrabDlg.ShowFrameNumber), rightImgCounter, 2, false);

                if (LeftImg.Count > 0 && RightImg.Count > 0)//如果左、右相机各有图像
                {
                    Thread detectThread = new Thread(ConcatImgAndStartDetectThread);//定义检测线程
                    detectThread.Start();//开启检测线程
                }


                m_Buffers[1].ReleaseAddress(pImage3);
                m_rgb888_buffer.ReleaseAddress(pImage2);



            }
        }


        void xfer_XferNotify(object sender, SapXferNotifyEventArgs argsNotify)//static
        {
            MultiBoardSyncGrabDemoDlg GrabDlg = argsNotify.Context as MultiBoardSyncGrabDemoDlg;
            // If grabbing in trash buffer, do not display the image, update the
            // appropriate number of frames on the status bar instead

            if (argsNotify.Trash)
                // return;
                GrabDlg.Invoke(new delegate_DisplayFrameAcquired(GrabDlg.ShowFrameNumber), argsNotify.EventCount, 1, true);
            else
            {

                swStopwatch.Start();
                SapBuffer m_rgb888_buffer = new SapBuffer(1, m_Buffers[0].Width, m_Buffers[0].Height, SapFormat.RGB888, SapBuffer.MemoryType.Default);
                swStopwatch.Stop();
                WritetoListbox("buffer转化耗时："+swStopwatch.ElapsedMilliseconds.ToString());
                
                m_rgb888_buffer.Create();
                m_rgb888_buffer.Copy(m_Buffers[0]);
                m_rgb888_buffer.GetAddress(out pImage1);
                m_Buffers[0].GetAddress(out pImage);
                swStopwatch.Restart();
                HObject LeftImgtemp = null;
                HOperatorSet.GenImageInterleaved(out LeftImgtemp, pImage1, "bgr", m_rgb888_buffer.Width, m_rgb888_buffer.Height, -1, "byte", 0, 0, 0, 0, -1, 0);

                HOperatorSet.CropPart(LeftImgtemp, out LeftImgtemp, 0, 0, m_rgb888_buffer.Width - 125, m_rgb888_buffer.Height);
                LeftImg.Enqueue(LeftImgtemp);
                GrabDlg.leftImgCounter++;
                GrabDlg.Invoke(new delegate_DisplayFrameAcquired(GrabDlg.ShowFrameNumber), leftImgCounter, 1, false);
                swStopwatch.Stop();
                WritetoListbox("转换为hobject"+swStopwatch.ElapsedMilliseconds.ToString());

                if (LeftImg.Count > 0 && RightImg.Count > 0)//如果左、右相机各有图像
                {
                    Thread detectThread = new Thread(ConcatImgAndStartDetectThread);//定义检测线程
                    detectThread.Start();//开启检测线程
                }


                m_Buffers[0].ReleaseAddress(pImage);
                m_rgb888_buffer.ReleaseAddress(pImage1);





            }
        }
        /// <summary>
        /// ConcatImgAndStartDetectThread()：1合并图像；2、启动处理线程
        /// </summary>
        private void ConcatImgAndStartDetectThread()
        {
            if (LeftImg.Count > 0 && RightImg.Count > 0)
            {
                HObject ImageTemp11 = null, ImageTemp22 = null, ImageTemp33 = null, TempDetectImg = null; ;
                HObject TempDispImg = null;
                HTuple t1, t2, t3;
                ImageTemp11 = LeftImg.Dequeue();
                ImageTemp22 = RightImg.Dequeue();

                HOperatorSet.CountSeconds(out t1);


                HOperatorSet.ConcatObj(ImageTemp11, ImageTemp22, out ImageTemp33);

                HOperatorSet.TileImages(ImageTemp33, out TempDetectImg, 2, "horizontal");
                HOperatorSet.CopyImage(TempDetectImg, out TempDispImg);
                HOperatorSet.CountSeconds(out t2);
                t3 = t2 - t1;

                 WritetoListbox("合并耗时："+t3.ToString());

                ImageTemp11.Dispose();
                ImageTemp22.Dispose();
                ImageTemp33.Dispose();

                DetectImgQueue.Enqueue(TempDetectImg);

                DispImgQueue.Enqueue(TempDispImg);

               // Convert.ToInt32(PublicClass.markedMeters) + 2

                if (MetersCounter < (Convert.ToInt32(PublicClass.markedMeters) + 2))
                {
                    Thread processth = new Thread(new ThreadStart(ProcessAction));
                    processth.Start();
                }
                else
                {

                   
                    if (PublicClass.serialPort1.IsOpen)
                        ExecutePLCCommand(STOP_MOTOR);
                  //  button_Freeze_Click(null, null);

                }


            }

            if (Math.Abs(leftImgCounter - rightImgCounter) > 1)
            {

                leftImgCounter = 0;
                rightImgCounter = 0;

            }

        }
        /// <summary>
        /// ProcessAction():图像算法处理线程：
        /// 1、计算标准色差；2、计算色差和缺陷检测；3、插入数据库；4、显示图表；
        /// </summary>
        private void ProcessAction()
        {

            HObject ImageTemp44 = null;



            ImageTemp44= DetectImgQueue.Dequeue();



            HTuple tic, toc, tdc;

            HObject ho_ImageWithDefect; 
            HTuple hv_tupleDefectX = null, hv_tupleDefectY = null, hv_defectNumber, hv_isSeperateComputer= new HTuple();
            HTuple hv_tupleDefectRadius = null, hv_minWidth = null, hv_tupleDefectRadius1 = null;
            HTuple hv_maxWidth = null, hv_meanWidth = null, hv_standardL = null;

            HTuple hv_result = null, hv_tupleDefectClass, hv_DetectResult;
            HTuple hv_metersCounter, hv_tupleMessages, hv_tupleMessagesColor, hv_leftDetectSide, hv_rightDetectSide;

            HTuple hv_tupleDefectRow1 = new HTuple(), hv_tupleDefectRow2 = new HTuple();
            HTuple hv_tupleDefectColumn1 = new HTuple(), hv_tupleDefectColumn2 = new HTuple();
            HTuple hv_tupleDetectResult, hv_tupleBackgroundColor;

            hv_DetectResult = new HTuple();
            hv_DetectResult[0] = 0;
            hv_DetectResult[1] = 0;
            hv_DetectResult[2] = 0;
            hv_DetectResult[3] = 0;
            hv_DetectResult[4] = 0;
            hv_DetectResult[5] = 0;
            hv_DetectResult[6] = 0;
            hv_DetectResult[7] = 0;
            hv_DetectResult[8] = 0;
            hv_DetectResult[9] = 0;

            hv_defectNumber = new HTuple();
            //瑕疵半径
            hv_tupleDefectRadius = new HTuple();
            //瑕疵X坐标
            hv_tupleDefectX = new HTuple();
            //瑕疵Y坐标
            hv_tupleDefectY = new HTuple();
            //tupleDefectClass表示瑕疵分类
            hv_tupleDefectClass = new HTuple();

            hv_result = 1;
            if (PublicClass.originpath != "")
                imgpath = PublicClass.originpath + (MetersCounter ) + ".jpg";
            else
            {
                imgpath = "'" + (MetersCounter + 1) + ".jpg";
            }
            if (PublicClass.sqloriginpath != "")
                sqlimgpath = PublicClass.sqloriginpath + (MetersCounter ) + ".jpg'";
            else
            {
                sqlimgpath = "'" + (MetersCounter ) + ".jpg";
            }

            if (chk_saveimg.Checked)
            {
                if (PublicClass.originpath != "")
                    HOperatorSet.WriteImage(ImageTemp44, "jpg", 0, imgpath);
            }


            //加载检测框及缺陷参数；
            updateRectangle();
            LoadDetectParasFromIni();



            //输出结果
            hv_tupleMessages = new HTuple();
            hv_tupleMessagesColor = new HTuple();
            //检测缺陷的个数
            hv_defectNumber = 0;
            //缺陷框坐标
            hv_tupleDefectRow1 = new HTuple();
            hv_tupleDefectRow2 = new HTuple();
            hv_tupleDefectColumn1 = new HTuple();
            hv_tupleDefectColumn2 = new HTuple();

            HOperatorSet.CountSeconds(out tic);
            if (Chk_Algorithms.Checked)
            {
                //计算标准色差
                if (chk_standlab.Checked)
                {
                    //yyyyyy


                  //  HObject ho_Boxs = null;
                   // HTuple hv_isSeperateComputer = new HTuple();


                 //   ho_Boxs.Dispose();

                    get_standard_lab(ImageTemp44, out ho_Boxs, hv_isSeperateComputer, out hv_standardTupleL,
                        out hv_standardTupleA, out hv_standardTupleB, out hv_standardL, out hv_standardA,
                        out hv_standardB, out hv_DetectResult, out hv_result, out hv_defectNumber,
                        out hv_tupleDefectClass, out hv_tupleDefectX, out hv_tupleDefectY,
                        out hv_tupleDefectRow1, out hv_tupleDefectRow2, out hv_tupleDefectColumn1,
                        out hv_tupleDefectColumn2, out hv_minWidth, out hv_maxWidth, out hv_meanWidth,
                        out hv_metersCounter, out hv_tupleMessages, out hv_tupleMessagesColor,
                        out hv_leftDetectSide, out hv_rightDetectSide, out hv_L, out hv_A,
                        out hv_B, out hv_ClothRegionCoordinateX1, out hv_ClothRegionCoordinateX2);

                    

                    //get_standard_lab1(ImageTemp44,out  ImageBoxs,out hv_standardTupleL, out hv_standardTupleA,
                    //    out hv_standardTupleB, out hv_standardL, out hv_standardA, out hv_standardB,
                    //    out hv_result, out hv_tupleDetectResult, out hv_defectNumber, out hv_tupleDefectClass,
                    //    out hv_tupleDefectX, out hv_tupleDefectY, out hv_tupleDefectRow1, out hv_tupleDefectRow2,
                    //    out hv_tupleDefectColumn1, out hv_tupleDefectColumn2, out hv_minWidth,
                    //    out hv_maxWidth, out hv_meanWidth, out hv_metersCounter, out hv_tupleMessages,
                    //    out hv_tupleMessagesColor, out hv_leftDetectSide, out hv_rightDetectSide);



                    detectedimgpath = PublicClass.detectedpath +"LAB_"+ MetersCounter + ".jpg";
                    sqldetectedimgpath = PublicClass.sqldetectedpath + MetersCounter + ".jpg’";


                    if (chk_saveimg.Checked)
                    {
                        if (PublicClass.detectedpath != "")
                            try
                            {
                                HOperatorSet.WriteImage(ImageTemp44, "jpg", 0, detectedimgpath);

                            }

                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                              //  throw;
                            }
                    }

                    hv_tupleBackgroundColor = new HTuple();
                    hv_tupleBackgroundColor[0] = 0;
                    hv_tupleBackgroundColor[1] = 0;
                    hv_tupleBackgroundColor[2] = 0;

                    InitResultMessage(hv_tupleBackgroundColor, hv_tupleDefectRow1,
                      hv_tupleDefectRow2, hv_tupleDefectColumn1, hv_tupleDefectColumn2, hv_minWidth,
                      hv_maxWidth, hv_meanWidth, hv_metersCounter, hv_tupleMessages, hv_tupleMessagesColor,
                      hv_leftDetectSide, hv_rightDetectSide);

                  

                    Thread DisplayImgThread = new Thread(new ThreadStart(DisplayImgAction));
                    DisplayImgThread.Start();



                    HOperatorSet.CountSeconds(out toc);
                    WritetoListbox("色差耗时" + (toc - tic).ToString());

                    HTuple numcounter;
                    HOperatorSet.TupleLength(hv_standardTupleL, out numcounter);
                    if (numcounter == 6)
                        writeLABtoini(hv_standardTupleL, hv_standardTupleA, hv_standardTupleB);

                   


                    if (chk_writetodb.Checked)
                    {
                        //插入开始时间；

                        MetersCounter = 0;
                        leftImgCounter = 0;
                        rightImgCounter = 0;
                        try
                        {
                            //hv_tupleDefectX = 4096;
                            //hv_tupleDefectY = 4096;
                            //hv_tupleDefectClass = 2;
                            //hv_defectNumber = 1;

                            //GenerateDefectInfoSQL(hv_tupleDefectX, hv_tupleDefectClass, hv_defectNumber, hv_tupleDefectY);
                            //this.Invoke(new delegate_WriteToSql(GetTableColumnName), "clothdetectresult", ResultValueList, 0, ResultValueList.Count - 1, true);


                            //如果有缺陷的话，加载进数据库
                            GenerateDefectInfoSQL(hv_tupleDefectX, hv_tupleDefectClass, hv_defectNumber, hv_tupleDefectY);

                        }
                        catch (Exception ex)
                        {

                            WritetoListbox(ex.ToString());
                            WriteLog(ex, "");
                        }


                      
                        double templ = 0;
                        double tempA = 0;
                        double tempB = 0;
                        double tempresult = 0;

                        try
                        {
                            HTuple number1;
                            HOperatorSet.TupleLength(hv_standardL, out number1);
                            if (number1 > 0)
                                templ = hv_standardL[0].D;

                            HOperatorSet.TupleLength(hv_standardA, out number1);
                            if (number1 > 0)
                                tempA = hv_standardA[0].D;
                            HOperatorSet.TupleLength(hv_standardB, out number1);
                            if (number1 > 0)
                                tempB = hv_standardB[0].D;
                            HOperatorSet.TupleLength(hv_result, out number1);
                            if (number1 > 0)
                                tempresult = hv_result[0].D;

                        }
                        catch (Exception ex)
                        {
                            WritetoListbox("标准色差异常");
                            WriteLog(ex, "");
                        }

                        //插入标准lab值；
                        DetailValueList.RemoveRange(20, 4);
                        tempresult = 0;
                        DetailValueList.Insert(20, templ.ToString("0.000"));
                        DetailValueList.Insert(21, tempA.ToString("0.000"));
                        DetailValueList.Insert(22, tempB.ToString("0.000"));
                        DetailValueList.Insert(23, tempresult.ToString("0.000"));

                       // DetailValueList[24] = "'" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "'";
                      //  DetailValueList[25] = "'" + DateTime.Now.ToString("hh:mm:ss") + "'";
                      
                        //to do create delegate to write to sql
                        //结果写入数据库
                        // this.Invoke(new delegate_WriteToSql(GetTableColumnName), "clothdetectdetail", DetailValueList, 20, 25, true);

                        DetectDetailDict["StandardColorL"] = templ.ToString("0.000");
                        DetectDetailDict["StandardColorA"] = tempA.ToString("0.000");
                        DetectDetailDict["StandardColorB"] = tempB.ToString("0.000");
                        DetectDetailDict["StandardColorLAB"] = tempresult.ToString("0.000");
                        DetectDetailDict["Date"] = "'" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "'";
                        DetectDetailDict["StartTime"] = "'" + DateTime.Now.ToString("hh:mm:ss") + "'";

                        this.Invoke(new delegate_WriteToSqlbydict(GetAndExecuteBatchSQL), "clothdetectdetail", PublicClass.clothNumber,
                            DetectDetailDict, 0, detail_table_counter - 1);


                    }

                    //开回调函数更新控件，布匹标准色差计算标志位复位,未找到布匹时，不复位。
                    if (hv_result!=1)
                    {
                        this.Invoke(new delegate_DisplayFrameAcquired(this.ControlerInThread), MetersCounter, 0, true);
                    }
                    
                   

                }
                else

                {

                    HTuple number23;
                    HOperatorSet.TupleLength(hv_standardTupleL, out number23);
                    //计算色差及缺陷
                    if (number23<1)
                    {
                        try
                        {
                            readLABfromini();
                            WritetoListbox("**************读取lab ok");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);

                        }

                    }

                    //hv_defectNumber = 0;
                    //hv_tupleDefectClass = 0;
                    //hv_tupleDefectX = 0;
                    //hv_tupleDefectY = 0;
                    //hv_tupleDefectRadius=0;                


                    //ZZZZZZ

                    HTuple hv_algorithmOfAberration = new HTuple(), hv_result3 = new HTuple();// hv_leftRightAberration2 = new HTuple();


                    //  ho_ImageWithDefect.Dispose(); hv_isSeperateComputer = new HTuple(),

                    get_defect_aberration(ImageTemp44, out ho_ImageWithDefect, hWindowControl2.HalconWindow,
                        hv_standardTupleL, hv_standardTupleA, hv_standardTupleB, hv_isSeperateComputer,
                        hv_algorithmOfAberration, out hv_clothAberration, out hv_leftRightAberration2,
                        out hv_L, out hv_A, out hv_B, out hv_result, out hv_tupleDetectResult,
                        out hv_defectNumber, out hv_tupleDefectClass, out hv_tupleDefectX,
                        out hv_tupleDefectY, out hv_tupleDefectRow1, out hv_tupleDefectRow2,
                        out hv_tupleDefectColumn1, out hv_tupleDefectColumn2, out hv_minWidth,
                        out hv_maxWidth, out hv_meanWidth, out hv_metersCounter, out hv_tupleMessages,
                        out hv_tupleMessagesColor, out hv_leftDetectSide, out hv_rightDetectSide, out hv_ClothRegionCoordinateX1, out hv_ClothRegionCoordinateX2);


                    hv_tupleDefectX = (hv_tupleDefectColumn2 + hv_tupleDefectColumn1) /2;
                    hv_tupleDefectY = (hv_tupleDefectRow2 + hv_tupleDefectRow1) / 2;
                    HOperatorSet.TupleLength(hv_tupleDefectX, out hv_defectNumber);
                   // hv_tupleDefectClass = hv_tupleDefectX;
                    HOperatorSet.TupleGenConst(hv_defectNumber, 0, out hv_tupleDefectClass);
                    WritetoListbox("缺陷个数" + hv_defectNumber.ToString());
                    GenerateDefectInfoSQL(hv_tupleDefectX, hv_tupleDefectClass, hv_defectNumber, hv_tupleDefectY);



                    HOperatorSet.CountSeconds(out toc);
                    WritetoListbox("缺陷耗时" + (toc - tic).ToString());




                    if (chk_writetodb.Checked)
                    {
                        try
                        {
                            GetAlmostResultStringList(hv_minWidth, hv_meanWidth, hv_maxWidth, hv_standardL, hv_result);
                            GetAberrationGrad(hv_clothAberration, out AberrationGrad);
                            GetAberrationGrad(hv_leftRightAberration2,out leftRightAberrationGrad);
                            Color_Value[0] = hv_clothAberration;
                            //批量写入detect数据库表格中
                            //this.Invoke(new delegate_WriteToSql(GetTableColumnName), "clothdetectresult", ResultValueList, 0, 16, true);

                            HOperatorSet.CountSeconds(out toc);
                            WritetoListbox("写入0-16耗时" + (toc - tic).ToString());

                            //hv_tupleDefectX = 4096;
                            //hv_tupleDefectY = 4096;

                           // hv_tupleDefectClass = hv_tupleDefectX;
                            //hv_defectNumber = 1;
                          
                            this.Invoke(new delegate_WriteToSql(GetTableColumnName), "clothdetectresult", ResultValueList, 0, ResultValueList.Count - 1, true);

                            HOperatorSet.CountSeconds(out toc);
                            WritetoListbox("写入17-48耗时" + (toc - tic).ToString());
                                                       

                            for (int j = 0; j <= ResultValueList.Count - 1; j++)
                            {

                                DetectResultDict[ResultColumeName[j]] = ResultValueList[j].ToString();


                            }

                            foreach (var columnstr in ReportColumeName)
                            {

                                try
                                {
                                    DetectReportDict[columnstr] = DetectResultDict[columnstr];
                                }
                                catch (Exception)
                                {

                                    //   throw;
                                }
                            }
                            DetectReportDict["AberrationGrad"] = AberrationGrad.ToString();
                            DetectReportDict["LRAberrationGrad"] = leftRightAberrationGrad.ToString();
                            DetectReportDict["Aberration"] = hv_clothAberration.TupleString("#.2f");
                            DetectReportDict["leftRightAberration"] = hv_leftRightAberration2.TupleString("#.2f");

                            DetectReportDict["FirstDefectDetail"] = "'缺陷类型：" + DetectResultDict["Defect1Class"] + "；位置：(" + DetectResultDict["Defect1PositionX"] + "," + DetectResultDict["Defect1PositionY"] + ")米' ";
                            DetectReportDict["SecondDefectDetail"] = "'缺陷类型：" + DetectResultDict["Defect2Class"] + "；位置：(" + DetectResultDict["Defect2PositionX"] + "," + DetectResultDict["Defect2PositionY"] + ")米' ";
                            DetectReportDict["ThirdDefectDetail"] = "'缺陷类型：" + DetectResultDict["Defect3Class"] + "；位置：(" + DetectResultDict["Defect3PositionX"] + "," + DetectResultDict["Defect3PositionY"] + ")米' ";
                            DetectReportDict["FourthDefectDetail"] = "'缺陷类型：" + DetectResultDict["Defect4Class"] + "；位置：(" + DetectResultDict["Defect4PositionX"] + "," + DetectResultDict["Defect4PositionY"] + ")米' ";
                            DetectReportDict["FifthDefectDetail"] = "'缺陷类型：" + DetectResultDict["Defect5Class"] + "；位置：(" + DetectResultDict["Defect5PositionX"] + "," + DetectResultDict["Defect5PositionY"] + ")米' ";
                            DetectReportDict["SixthDefectDetail"] = "'缺陷类型：" + DetectResultDict["Defect6Class"] + "；位置：(" + DetectResultDict["Defect6PositionX"] + "," + DetectResultDict["Defect6PositionY"] + ")米' ";
                            DetectReportDict["SeventhDefectDetail"] = "'缺陷类型：" + DetectResultDict["Defect7Class"] + "；位置：(" + DetectResultDict["Defect7PositionX"] + "," + DetectResultDict["Defect7PositionY"] + ")米' ";
                            DetectReportDict["EighthDefectDetail"] = "'缺陷类型：" + DetectResultDict["Defect8Class"] + "；位置：(" + DetectResultDict["Defect8PositionX"] + "," + DetectResultDict["Defect8PositionY"] + ")米' ";
                            DetectReportDict["NinethDefectDetail"] = "'缺陷类型：" + DetectResultDict["Defect9Class"] + "；位置：(" + DetectResultDict["Defect9PositionX"] + "," + DetectResultDict["Defect9PositionY"] + ")米' ";
                            DetectReportDict["TenthDefectDetail"] = "'缺陷类型：" + DetectResultDict["Defect10Class"] + "；位置：(" + DetectResultDict["Defect10PositionX"] + "," + DetectResultDict["Defect10PositionY"] + ")米' ";
                           



                            this.Invoke(new delegate_WriteToSqlbydict(GetAndExecuteBatchSQL), "clothdetectreport", PublicClass.clothNumber,
                                    DetectReportDict, 0, report_table_counter - 1);
                         

                        }

                        catch (Exception ex)
                        {

                            WritetoListbox(ex.ToString());
                            WriteLog(ex, "");
                        }

                        HOperatorSet.CountSeconds(out toc);
                        WritetoListbox("写入数据库总耗时" + (toc - tic).ToString()); 
                    }


                    hv_tupleBackgroundColor = new HTuple();
                    hv_tupleBackgroundColor[0] = 80;
                    hv_tupleBackgroundColor[1] = 80;
                    hv_tupleBackgroundColor[2] = 80;

                    //写入字典中，以供DisplayImgThread调用;
                    InitResultMessage(hv_tupleBackgroundColor, hv_tupleDefectRow1,
                        hv_tupleDefectRow2, hv_tupleDefectColumn1, hv_tupleDefectColumn2, hv_minWidth,
                        hv_maxWidth, hv_meanWidth, hv_metersCounter, hv_tupleMessages, hv_tupleMessagesColor,
                        hv_leftDetectSide, hv_rightDetectSide);


                    Thread DisplayImgThread = new Thread(new ThreadStart(DisplayImgAction));
                    DisplayImgThread.Start();

                    HOperatorSet.CountSeconds(out toc);
                    WritetoListbox("开启线程耗时" + (toc - tic).ToString());


                    //注释为直接显示
                    //GetResultMessage(out hv_tupleBackgroundColor, out hv_tupleDefectRow1,
                    //    out hv_tupleDefectRow2, out hv_tupleDefectColumn1, out hv_tupleDefectColumn2, out hv_minWidth,
                    //    out hv_maxWidth, out hv_meanWidth, out hv_metersCounter, out hv_tupleMessages, out hv_tupleMessagesColor,
                    //    out hv_leftDetectSide, out hv_rightDetectSide);

                    //disp_detect_result(ImageTemp44, hWindowControl2.HalconWindow, hv_tupleBackgroundColor, hv_tupleDefectRow1,
                    //    hv_tupleDefectRow2, hv_tupleDefectColumn1, hv_tupleDefectColumn2, hv_minWidth,
                    //    hv_maxWidth, hv_meanWidth, hv_metersCounter, hv_tupleMessages, hv_tupleMessagesColor,
                    //    hv_leftDetectSide, hv_rightDetectSide);

                    // 保存检测图像至数据库和本地图像库
                   
                    try
                    {
                        detectedimgpath = PublicClass.detectedpath + MetersCounter + ".jpg";
                        if (chk_saveimg.Checked)
                        {

                            try
                            {
                                if (PublicClass.detectedpath != "")
                                    HOperatorSet.WriteImage(ho_ImageWithDefect, "jpg", 0, detectedimgpath);
                            }
                            catch (Exception)
                            {

                                //  throw;
                            }
                        }


                        HOperatorSet.CountSeconds(out toc);
                        WritetoListbox("保存图像耗时" + (toc - tic).ToString());

                        //将图像根目录写入数据库mysql中detail表
                        sqldetectedimgpath = PublicClass.SqlImgPath + "/'";

                        DetectDetailDict["ImagePath"] = sqldetectedimgpath;

                        DetailValueList.RemoveAt(37);

                        DetailValueList.Insert(37, sqldetectedimgpath);

                        DetectDetailDict["ImagePath"] = sqldetectedimgpath;

                        this.Invoke(new delegate_WriteToSql(GetTableColumnName), "clothdetectdetail", DetailValueList, 37, 37, true);


                      
                    }
                    catch (Exception ex)
                    {

                        WritetoListbox(ex.ToString());
                        WriteLog(ex, "");
                    }
                                      
                    try
                    {
                        

                        // 缺陷结果赋值
                        GetDetectName(hv_result);

                        WriteToDataGridView();

                        this.Invoke(new delegate_DisplayAberrationChart(this.ShowChartResult), this.Color_Value, MetersCounter, false);

                        HOperatorSet.CountSeconds(out toc);
                        WritetoListbox("色差曲线耗时" + (toc - tic).ToString());


                    }
                    catch (Exception ex)
                    {

                        WritetoListbox(ex.ToString());
                        WriteLog(ex, "");
                    }
                    MetersCounter++;
                    if (chk_writetodb.Checked)
                    {
                                                              
                                             

                        HTuple num;

                       

                        try
                        {
                            //批量写入detect数据库表格中
                          

                            //图像数据曲线显示
                            this.Invoke(new delegate_DisplayAberrationChart(this.ShowChartResult), this.Color_Value, MetersCounter, false);

                            HOperatorSet.CountSeconds(out toc);
                            WritetoListbox("曲线2显示耗时" + (toc - tic).ToString());

                            //显示处理张数
                            this.Invoke(new delegate_DisplayFrameAcquired(this.ShowFrameNumber), MetersCounter, 0, false);

                            HOperatorSet.CountSeconds(out toc);
                            WritetoListbox("显示帧率耗时" + (toc - tic).ToString());

                        }

                        catch (Exception ex)
                        {
                            WritetoListbox("结果显示异常");
                            WriteLog(ex, "");
                        }



                        if (ImageTemp != null)
                            ImageTemp.Dispose();
                        HOperatorSet.CountSeconds(out toc);
                        WritetoListbox("总耗时" + (toc - tic).ToString());


                    }


                }

            }
        }

        private void GenerateDefectInfoSQL(HTuple hv_tupleDefectX, HTuple hv_tupleDefectClass, HTuple hv_defectNumber,
            HTuple hv_tupleDefectY)
        {
            HTuple number1, number2;
            HTuple hv_mm_DefectX = null, hv_mm_DefectY = null;
            HTuple hv_mm_DefectXsss = null, hv_mm_DefectYsss = null;

            HOperatorSet.TupleLength(hv_tupleDefectX, out number1);
            HOperatorSet.TupleLength(hv_tupleDefectClass, out number2);
            int Index_defectNumber = 0;
            bool isDefectRight = true;

           // WritetoListbox("result数组个数:" + ResultValueList.Count);
            //WritetoListbox("Detail数组个数:" + DetailValueList.Count);
            GetColumnIndex("DefectNumber", ResultColumeName, out Index_defectNumber);

            PublicClass.mag = 4908.0 / 1000.0;

            string formatstr = "2f";

            if (number1 > 0)
            {

              //  for (int i = 0; i < 100; i++)
              //  {
                  //  MetersCounter = 10;
                    hv_mm_DefectX =( hv_tupleDefectX-hv_ClothRegionCoordinateX1) / (PublicClass.mag * 1000);
                    hv_mm_DefectY = hv_tupleDefectY / (PublicClass.mag * 1000) + MetersCounter;
                    WritetoListbox("X" + hv_mm_DefectX.TupleString("#.2f") + ";Y:" + hv_mm_DefectY.TupleString("#.2f"));
                    ResultValueList.Clear();
                for (int i = 0; i < result_table_counter; i++)
                {
                    ResultValueList.Add("0");
                }
                //  }
            }


            if (number1.I != number2.I && hv_defectNumber.I != number2.I)
            {
                WritetoListbox("X" + number1.I.ToString() + "; Y:" + number2.I.ToString() + ";defectNumber:" + hv_defectNumber.I.ToString());
                isDefectRight = false;
            }


            if (isDefectRight)
            {
                
                if (hv_defectNumber[0].I > 0)
             
                {
                    int endpoint = (Index_defectNumber + 3 * 10);
                   

                    if (ResultValueList.Count != result_table_counter)
                    {
                        WritetoListbox("个数不匹配:" + ResultValueList.Count.ToString() + ":" + result_table_counter.ToString());
                        
                        ResultValueList.Clear();
                        for (int i = 0; i < result_table_counter; i++)
                        {
                            ResultValueList.Add("0");
                        }

                    }
                   
                    else
                    {

                    ResultValueList.RemoveRange(Index_defectNumber, 1 + 3 * 10);


                    if (hv_defectNumber > 10)
                        hv_defectNumber = 10;

                    ResultValueList.Insert(Index_defectNumber, hv_defectNumber.ToString());

                    for (int i = 0; i < 10; i++)
                    {

                        if(i< hv_defectNumber)
                        { 
                        try
                        {
                            hv_mm_DefectXsss = hv_mm_DefectX[i];
                            hv_mm_DefectYsss = hv_mm_DefectY[i];




                            if (i<number1)
                                ResultValueList.Insert(Index_defectNumber + 3 * i + 1, hv_mm_DefectXsss.TupleString("#.2f"));
                            else
                                 ResultValueList.Insert(Index_defectNumber + 3 * i + 1, 0.ToString());
                             if (i < number1)
                              ResultValueList.Insert(Index_defectNumber + 3 * i + 2, hv_mm_DefectYsss.TupleString("#.2f"));
                             else
                             ResultValueList.Insert(Index_defectNumber + 3 * i + 2, 0.ToString());
                           if (i < number2)
                            ResultValueList.Insert(Index_defectNumber + 3 * i + 3, hv_tupleDefectClass[i].D.ToString()); //需添加缺陷类别
                              else
                            ResultValueList.Insert(Index_defectNumber + 3 * i + 3, 0.ToString());
                          }
                        catch (Exception ex)
                        {
                            ResultValueList.Insert(Index_defectNumber + 3 * i + 1, 0.ToString());
                            ResultValueList.Insert(Index_defectNumber + 3 * i + 2, 0.ToString());
                            ResultValueList.Insert(Index_defectNumber + 3 * i + 3, 0.ToString());

                            WritetoListbox("个数不匹配,缺陷个数为"+ hv_defectNumber.ToString());
                            WriteLog(ex, "");

                        }

                        }

                       else
                        {
                            ResultValueList.Insert(Index_defectNumber + 3 * i + 1, 0.ToString());

                            ResultValueList.Insert(Index_defectNumber + 3 * i + 2, 0.ToString());

                            ResultValueList.Insert(Index_defectNumber + 3 * i + 3, 0.ToString()); 
                        }

                    }
                      //  this.Invoke(new delegate_WriteToSql(GetTableColumnName), "clothdetectresult", ResultValueList, Index_defectNumber, endpoint, true);

                    }



                }
            }
        }
       // 灰度卡实测<0.39 0.39-2.6 2.6-6.7 6.7-12.2 >12.2 
       // 建议值 0.5 0.5-3 3-7 7-12 12+ 
        private void GetAberrationGradAndDetecfInfo(HTuple hv_result)
        {
            Color_Value[0] = hv_clothAberration;


            //获得色差等级
            if (hv_clothAberration > 8)
            {
                Color_Value[0] = 8;
                AberrationGrad = 5;
            }

            







            if (hv_clothAberration > 0 && hv_clothAberration < 1.0)
            {
                AberrationGrad = 1;
            }

            if (hv_clothAberration > 1.0 && hv_clothAberration < 3.0)
            {
                AberrationGrad = 2;
            }

            if (hv_clothAberration > 3.0 && hv_clothAberration < 6.0)
            {
                AberrationGrad = 3;
            }

            if (hv_clothAberration > 6.0 && hv_clothAberration < 8.0)
            {
                AberrationGrad = 4;
            }





            // 缺陷结果赋值
            switch (hv_result[0].I)
            {
                case 1:
                    defectstr = "未找到布匹";
                    break;
                case 2:
                    defectstr = "接缝";
                    break;
                case 3:
                    defectstr = "缺陷";
                    break;
                case 4:
                    defectstr = "断布";
                    break;
                case 5:
                    defectstr = "脏污";
                    break;
                case 0:
                    defectstr = "合格";
                    break;
            }
        }

        private void WriteToDataGridView()        {
         

            try
            {
               


                int index = dataGridView1.Rows.Add();
                if (index > 0)
                {
                    
                    dataGridView1.Rows[index].Cells["检测米数"].Value = MetersCounter.ToString();
                    dataGridView1.Rows[index].Cells["检测时间"].Value = DateTime.Now.ToString();
                    dataGridView1.Rows[index].Cells["色差均值"].Value = Color_Value[0].ToString("0.00");
                    dataGridView1.Rows[index].Cells["缺陷结果"].Value = defectstr;
                   

                }

                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;

                

            }
            catch (Exception ex)
            {
                WritetoListbox("结果显示异常");
                WriteLog(ex, "");
            }
        }

        private void DisplayImgAction()
        {

            HTuple hv_tupleBackgroundColor;
            hv_tupleBackgroundColor = new HTuple();
            hv_tupleBackgroundColor[0] = 80;
            hv_tupleBackgroundColor[1] = 80;
            hv_tupleBackgroundColor[2] = 80;

            HTuple hv_metersCounter = new HTuple(), hv_tupleMessages, hv_tupleMessagesColor, hv_leftDetectSide, hv_rightDetectSide;

            HTuple hv_tupleDefectRow1 = new HTuple(), hv_tupleDefectRow2 = new HTuple();
            HTuple hv_tupleDefectColumn1 = new HTuple(), hv_tupleDefectColumn2 = new HTuple();

            HTuple hv_minWidth, hv_maxWidth = null, hv_meanWidth = null;


            HObject ho_ImageWithDefect;
            ho_ImageWithDefect = null;

            if (DispImgQueue.Count>0)
            {

                HObject displayimg = DispImgQueue.Dequeue();
                // disp_detect_result(displayimg, hWindowControl2.HalconWindow, DispResultDict);
                GetResultMessage(out hv_tupleBackgroundColor, out hv_tupleDefectRow1,
                    out hv_tupleDefectRow2, out hv_tupleDefectColumn1, out hv_tupleDefectColumn2, out hv_minWidth,
                   out hv_maxWidth, out hv_meanWidth, out hv_metersCounter, out hv_tupleMessages, out hv_tupleMessagesColor,
                    out hv_leftDetectSide, out hv_rightDetectSide);

                //  WritetoListbox(hv_metersCounter.ToString());

                


                disp_detect_result(displayimg, ho_Boxs, out ho_ImageWithDefect, hWindowControl2.HalconWindow, hv_tupleBackgroundColor, hv_tupleDefectRow1,
           hv_tupleDefectRow2, hv_tupleDefectColumn1, hv_tupleDefectColumn2, hv_minWidth,
           hv_maxWidth, hv_meanWidth, hv_metersCounter, hv_tupleMessages, hv_tupleMessagesColor,
           hv_leftDetectSide, hv_rightDetectSide); 

           






            if (chk_saveimg.Checked)
            {
                try
                {
                    if (PublicClass.detectedpath != "")
                        HOperatorSet.WriteImage(ho_ImageWithDefect, "jpg", 0, detectedimgpath);
                }
                catch (Exception)
                {

                    WritetoListbox("图像路径异常");
                }

            }
            //todo write ho_ImageWithDefect to disk

            displayimg.Dispose();

            }


        }
        /// <summary>
        /// ShowFrameNumber:显示相机采集图像张数
        /// </summary>
        /// <param name="number"></param>图像个数
        /// <param name="cameraflag"></param>0为左相机采集，1为右相机采集，2、处理图像张数
        /// <param name="trash"></param>是否为缓存图像；
        private void ShowFrameNumber(int number, int cameraflag, bool trash)
        {
            String str;
            DateTime time = DateTime.Now;
            String liststr;


            if (cameraflag == 0)
            {
                if (trash)
                {

                    str = String.Format("再缓存区共采集了: {0}", number);
                    this.StatusLabelInfoTrash.Text = str;
                    liststr = DateTime.Now.ToString() + " 处理缓存: " + number.ToString();
                    listBox1.Items.Add(liststr);
                    listBox1.TopIndex = listBox1.Items.Count - 1;

                }
                else
                {
                    str = String.Format("图像处理 :{0}", number);
                    this.StatusLabelInfo.Text = str;
                    liststr = DateTime.Now.ToString() + " 程序处理 : " + number.ToString();
                    listBox1.Items.Add(liststr);
                    listBox1.TopIndex = listBox1.Items.Count - 1;

                }
            }

            if (cameraflag == 1)
            {
                if (trash)
                {

                    str = String.Format("再缓存区共采集了: {0}", number);
                    this.toolStripLabel6.Text = str;
                    liststr = DateTime.Now.ToString() + " 1号回调缓存: " + leftImgCounter.ToString();
                    listBox1.Items.Add(liststr);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    // this.StatusLabelInfoTrash.Text = str;
                }
                else
                {
                    str = String.Format("左相机共采集 :{0}", number);
                    this.toolStripLabel6.Text = str;
                    liststr = DateTime.Now.ToString() + " 1号回调: " + leftImgCounter.ToString();
                    listBox1.Items.Add(liststr);
                    listBox1.TopIndex = listBox1.Items.Count - 1;

                }

            }

            if (cameraflag == 2)
            {
                if (trash)
                {

                    str = String.Format("再缓存区共采集了: {0}", number);
                    this.StatusLabelInfo1.Text = str;
                    liststr = DateTime.Now.ToString() + " 2号回调缓存: " + rightImgCounter.ToString();
                    listBox1.Items.Add(liststr);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    str = String.Format("右相机共采集 :{0}", number);
                    this.StatusLabelInfo1.Text = str;
                    liststr = DateTime.Now.ToString() + " 2号回调: " + rightImgCounter.ToString();
                    listBox1.Items.Add(liststr);
                    listBox1.TopIndex = listBox1.Items.Count - 1;

                }

            }



        }

        private void ControlerInThread(int number, int cameraflag, bool trash)
        {


            chk_standlab.Checked = false;
            this.Refresh();



        }

        private void ShowChartResult(double[] Color_Value, double meter, bool isShow)
        {
            try
            {
                String str;
                DateTime time = DateTime.Now;
                chart1.Series[0].Points.AddXY(meter, Color_Value[0]);
                chart1.Series[1].Points.AddXY(meter, Color_Value[1]);
                chart1.Series[2].Points.AddXY(meter, Color_Value[2]);
                chart1.Series[3].Points.AddXY(meter, Color_Value[3]);
                chart1.Series[4].Points.AddXY(meter, Color_Value[4]);
                chart1.Series[5].Points.AddXY(meter, Color_Value[5]);
            }
            catch (Exception)
            {

                throw;
            }




            //  
        }


        private void loadparasfromini(int num)
        {
            if (num > 0)
            {
                PublicClass.Cap_NUM = int.Parse(cinifile.IniReadValue("检测框信息", "采样个数"));
                PublicClass.Square = Convert.ToInt32(cinifile.IniReadValue("检测框信息", "采样周期"));
                PublicClass.Width = Convert.ToInt32(cinifile.IniReadValue("检测框信息", "采样宽度"));
                PublicClass.OriginX = Convert.ToInt32(cinifile.IniReadValue("检测框信息", "采样起点X"));
                PublicClass.OriginY = Convert.ToInt32(cinifile.IniReadValue("检测框信息", "采样起点Y"));

                PublicClass.batchNumber = cinifile.IniReadValue("布匹信息", "布匹批号");
                PublicClass.cylinderNumber = cinifile.IniReadValue("布匹信息", "布匹缸号");
                PublicClass.clothClassNumber = cinifile.IniReadValue("布匹信息", "布匹类号");
                // PublicClass.cloth = cinifile.IniReadValue("布匹信息", "布匹颜色");
                PublicClass.volumnNumber = cinifile.IniReadValue("布匹信息", "布匹卷数");
                PublicClass.markedMeters = cinifile.IniReadValue("布匹信息", "布匹米数");
              //  PublicClass.clothNumber = cinifile.IniReadValue("布匹信息", "布匹编号");

                labelClothNumber.Text = PublicClass.clothNumber;
                labelBatchNumber.Text = PublicClass.batchNumber;
                labelCylinderNumber.Text = PublicClass.cylinderNumber;
                labelClassNumber.Text = PublicClass.clothClassNumber;
                labelVolumnNumber.Text = PublicClass.volumnNumber;
                labelTotalVolumnNumber.Text = PublicClass.totalVolumn;
                labelActualMeters.Text = PublicClass.markedMeters;
                labelSourceName.Text = PublicClass.sourceName;
                labelDetectName.Text = PublicClass.detecterName;

            }


        }

        private void ConverttoString(double[] value, List<string> Lstring)
        {
            string hahah;
            for (int i = 0; i < value.GetLength(1); i++)
            {
                hahah = value[0].ToString();
                Lstring.Insert(i + 3, hahah);
            }


            throw new NotImplementedException();
        }



        static void GetSignalStatus(object sender, SapSignalNotifyEventArgs argsSignal)
        {
            MultiBoardSyncGrabDemoDlg GrabDlg = argsSignal.Context as MultiBoardSyncGrabDemoDlg;
            SapAcquisition.AcqSignalStatus signalStatus = argsSignal.SignalStatus;

            GrabDlg.m_IsSignalDetected = (signalStatus != SapAcquisition.AcqSignalStatus.None);
            if (GrabDlg.m_IsSignalDetected == false)
                GrabDlg.StatusLabelInfo.Text = "Online... No camera signal detected";
            else GrabDlg.StatusLabelInfo.Text = "Online... Camera signal detected";


        }




        private static int chao_getseed()
        {
            byte[] bys = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rngsp = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rngsp.GetBytes(bys);
            return BitConverter.ToInt32(bys, 0);

        }

        // Constructor
        public MultiBoardSyncGrabDemoDlg()
        {
            //消息框助手，用于关闭jai相机引发的弹窗问题
            MessageBoxHelper.IsWorking = true;
            MessageBoxHelper.FindAndKillWindow();

            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

            //初始化并完成双相机的串口通信连接
            string AllConfigFiles = Application.StartupPath + "\\cameraconfig\\";

            try
            {
                if (true)
                {

                    ((System.ComponentModel.ISupportInitialize)(this.AxJ400Cl0)).BeginInit();
                    this.Controls.Add(AxJ400Cl0);
                    ((System.ComponentModel.ISupportInitialize)(this.AxJ400Cl0)).EndInit();

                    PublicClass.CameraCrtl0 = AxJ400Cl0.StartCommunication(1, 0);
                    AxJ400Cl0.Visible = false;


                   // 加载user area1，即平场数据的内存区；
                    AxJ400Cl0.EEPROMArea = 1;
                    AxJ400Cl0.LoadSettings();
                    string usersetting = AxJ400Cl0.ReadCurrentEEPROMAreaString();

                    AxJ400Cl0.ReadAllFromFile(AllConfigFiles + "110501.cam");
                    AxJ400Cl0.ReadChromaticAberrationFromFile(AllConfigFiles + "110501.csv",0);
                    

                }
            }
            catch (Exception ex)
            {
                WritetoListbox("相机1异常");
                WriteLog(ex, "");
            }



            try
            {
                if (true)
                {
                    ((System.ComponentModel.ISupportInitialize)(this.AxJ400Cl1)).BeginInit();
                    this.Controls.Add(AxJ400Cl1);
                    ((System.ComponentModel.ISupportInitialize)(this.AxJ400Cl1)).EndInit();
                    PublicClass.CameraCrtl1 = AxJ400Cl1.StartCommunication(1, 1);
                    AxJ400Cl1.Visible = false;


                   // 加载user area1，即平场数据的内存区；
                    AxJ400Cl1.EEPROMArea = 1;
                    AxJ400Cl1.LoadSettings();
                    string usersetting = AxJ400Cl1.ReadCurrentEEPROMAreaString();

                    AxJ400Cl1.ReadAllFromFile(AllConfigFiles+"110502.cam");
                    AxJ400Cl1.ReadChromaticAberrationFromFile(AllConfigFiles + "110502.csv",0);

                }
            }
            catch (Exception ex)
            {
                WritetoListbox("相机2异常");
                WriteLog(ex, "");
            }

        }



        public void InitObject()
        {
            m_AcqD = null;
            m_Acquisition = null;
            m_Buffers = null;
            m_Xfer = null;
            //m_View = null;
            m_IsSignalDetected = true;

            

            //色差曲线数据初始化
            Color_Value[0] = 0;
            Color_Value[1] = 0;
            Color_Value[2] = 0;
            Color_Value[3] = 0;
            Color_Value[4] = 0;
            Color_Value[5] = 0;


            m_Acquisition = new SapAcquisition[2];
            m_Buffers = new SapBufferRoi[2];
            m_Xfer = new SapAcqToBuf[2];


            //数据库字典初始化，主要是result和detail表格的列名及数量。

            result_table_counter = GetColumnNameAndCounter("clothdetectresult", ResultColumeName);
            detail_table_counter = GetColumnNameAndCounter("clothdetectdetail", DetailColumeName);
            report_table_counter = GetColumnNameAndCounter("clothdetectreport", ReportColumeName);


            string tempstr = ResultColumeName[0];

            DetectDetailDict.Clear();
            DetectResultDict.Clear();
            DetectReportDict.Clear();

            //初始化数据库字符串
            for (int i = 0; i < result_table_counter; i++)
            {
                DetectResultDict.Add(ResultColumeName[i], "0");
                ResultValueList.Add(0.ToString());

            }


            for (int i = 0; i < detail_table_counter; i++)
            {
                DetectDetailDict.Add(DetailColumeName[i], "0");
                DetailValueList.Add("0");

            }

            for (int i = 0; i < report_table_counter; i++)
            {
                DetectReportDict.Add(ReportColumeName[i], "0");
                ReportValueList.Add("0");

            }

            //for (
            //    int i = 0; i <= ResultValueList.Count - 1; i++)
            //{
            //    DetectResultDict[ResultColumeName[i]] = ResultValueList[i].ToString();
            //}

            //foreach (var columnstr in ReportColumeName)
            //{

            //    try
            //    {
            //        DetectReportDict[columnstr] = DetectResultDict[columnstr];
            //    }
            //    catch (Exception)
            //    {

            //     //   throw;
            //    }
            //}


            //PublicClass.clothNumber = 1105.ToString();
            //for (int i = 10; i < 55; i++)
            //{
            //    DetectReportDict["MeterNumber"] = i.ToString();

            //    this.Invoke(new delegate_WriteToSqlbydict(GetAndExecuteBatchSQL), "clothdetectreport", PublicClass.clothNumber,
            //        DetectReportDict, 0, report_table_counter - 1);
            //}

           


            //todo :getaberrationgrad


            //DetectResultDict["ClothNumber"] = DateTime.Now.ToString("yyyyMMdd");
            //PublicClass.clothNumber = DetectResultDict["ClothNumber"];
            //DetectResultDict["MeterNumber"] = 2.ToString();



            //int sss;
            //int sss1;
            //int sss2;

            //GetColumnIndex("ClothNumber", ResultColumeName, out sss2);
            //GetColumnIndex("Date", DetailColumeName, out sss2);
            //GetColumnIndex(out sss, out sss1, out sss2);
            string SQltable = "clothdetectresult";
            List<string> list_BatchSql = new List<string>();

            // DetectResultDict["ClothNumber"] = DateTime.Now.ToString("yyyyMMddmmss");
            // PublicClass.clothNumber = DetectResultDict["ClothNumber"];



            //   for(int i=0;i<30;i++)
            //{ 
            //DetectResultDict["MeterNumber"] = i.ToString();
            //this.Invoke(new delegate_WriteToSqlbydict(GetAndExecuteBatchSQL), "clothdetectresult", PublicClass.clothNumber,
            //    DetectResultDict, 0, result_table_counter - 1);

            //}

            //foreach (string key in DetectDetailDict.Keys)
            //{
            //    DetailValueList.Add(key);

            //}

            //foreach (string value in DetectDetailDict.Values)
            //{
            //    DetailValueList.Add(value);
            //}
            // DetailValueList.Add("0");

            //foreach (KeyValuePair<string, string> kvp in DetectDetailDict)
            //{
            //    Console.WriteLine("Key={0},Value{1}",kvp.Key,kvp.Value);
            //}

            // Person p2 = lstPerson.Find(delegate (Person s) { return s.Name.Equals("王五"); });//2、匿名函数
            // Person p3 = lstPerson.Find(s => { return s.Name.Equals("赵六"); });//3、Lambda表达式
            // Person p4 = lstPerson.Find(s => s.Name.Equals("赵六"));//4、Lambda表达式的简洁写法

            //System.Predicate<string>("ClothNumber")
            
            //检测结果信息
            InitResultMessage();



            AcqConfigDlg acConfigDlg = new AcqConfigDlg(null, "", AcqConfigDlg.ServerCategory.ServerAcq);


            //编码器触发采集图像配置文件
            string ConfigFile = Application.StartupPath + "\\cameraconfig\\T_Xtium_JAI-LT-400_1Tap_LineTrigger.ccf";// "E:\\exe\\MySQLV1.01.exe"
            string ConfigFile1 = Application.StartupPath + "\\cameraconfig\\T_Xtium_JAI-LT-400_1Tap_LineTrigger2.ccf";// "E:\\exe\\MySQLV1.01.exe"
            string AllConfigFiles = Application.StartupPath + "\\cameraconfig\\";

            //采集卡内部触发配置文件
            // string ConfigFile  = "E:\\T_JAI_400_contious1.ccf";
            //string ConfigFile1 = "E:\\T_JAI_400_contious2.ccf";


            //指定采集卡地址
            SapLocation Sp2 = new SapLocation("Xtium-CL_MX4_1", 2);
            SapLocation Sp1 = new SapLocation("Xtium-CL_MX4_1", 3);


            //绑定采集卡与配置文件
            m_Acquisition[0] = new SapAcquisition(Sp1, ConfigFile);
            m_Acquisition[1] = new SapAcquisition(Sp2, ConfigFile1);

            m_online = true;
            // Create acquisition object
            if (m_Acquisition[0] != null && !m_Acquisition[0].Initialized)
            {
                bool caijikaStatus;
                caijikaStatus = false;
                try
                {
                    //  if(m_Acquisition[0].Initialized)
                    caijikaStatus = m_Acquisition[0].Create();
                }
                catch (Exception ex)
                {
                    WritetoListbox("相机异常");
                    m_online = false;
                    WriteLog(ex, "");
                }
                // m_Acquisition[0].Initialized;//

                if (caijikaStatus == false)
                {
                    DestroyObjects();
                    WritetoListbox("未找到采集卡");
                }
            }
            // }
            else
            {

                WritetoListbox("未找到采集卡");
                this.Close();
                return;
            }

            AcqConfigDlg acConfigDlg2 = new AcqConfigDlg();//(null, "", AcqConfigDlg.ServerCategory.ServerAcq);
            if (m_Acquisition[1] != null && !m_Acquisition[1].Initialized)
            {

                if (m_Acquisition[1].Create() == false)
                {
                    DestroyObjects();
                }
            }

            else
            {

                WritetoListbox("未找到采集卡");
                return;
            }

            //check to see if both acquision devices support scatter gather.
            bool acq0SupportSG = SapBuffer.IsBufferTypeSupported(m_Acquisition[0].Location, SapBuffer.MemoryType.ScatterGather);
            bool acq1SupportSG = SapBuffer.IsBufferTypeSupported(m_Acquisition[1].Location, SapBuffer.MemoryType.ScatterGather);


            if (!acq0SupportSG || !acq1SupportSG)
            {
                // check if they support scatter gather physical
                bool acq0SupportSGP = SapBuffer.IsBufferTypeSupported(m_Acquisition[0].Location, SapBuffer.MemoryType.ScatterGatherPhysical);
                bool acq1SupportSGP = SapBuffer.IsBufferTypeSupported(m_Acquisition[1].Location, SapBuffer.MemoryType.ScatterGatherPhysical);

                if (!(!acq0SupportSG && !acq1SupportSG && acq0SupportSGP && acq1SupportSGP))
                {

                    WritetoListbox("相机连接失败");
                    m_online = false;
                }
            }
            else
            {
                WritetoListbox("相机连接成功");


            }



            if (!CreateNewObjects(null, false))
                this.Close();//*/


           



            try
            {
                if (m_online)
                    PublicClass.CameraCrtl1 = AxJ400Cl1.StartCommunication(1, 1);

            }

            catch (Exception ex)
            {
                WritetoListbox("2号相机连接异常");
                WriteLog(ex, "");
            }

            try
            {
                if (m_online)
                    PublicClass.CameraCrtl0 = AxJ400Cl0.StartCommunication(1, 0);

            }

            catch (Exception ex)
            {
                WritetoListbox("1号相机连接异常");
                WriteLog(ex, "");
            }

            if (PublicClass.CameraCrtl0 == 0)
            {
                listBox1.Items.Add("相机1通信连接成功" + " " + DateTime.Now.ToString());
                listBox1.TopIndex = listBox1.Items.Count - 1;


            }
            else
            {

            }

            if (PublicClass.CameraCrtl1 == 0)
            {


                listBox1.Items.Add("相机2通信连接成功" + " " + DateTime.Now.ToString());
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }
            else
            {

            }

            try
            {
                if (m_online)
                {
                    m_Xfer[0].Grab();

                    m_Xfer[1].Grab();
                }
            }
            catch (Exception ex)
            {
                WritetoListbox("采集异常");
                WriteLog(ex, "");
            }



        }

        private void GetAndExecuteBatchSQL(string table_namew, string ClothNumber, Dictionary<string, string> LSring, int begin_index, int end_index)
        {
            List<string> list_BatchSql;
            // list_BatchSql = GetBatchSql(SQltable, PublicClass.clothNumber, DetectResultDict, 0, detail_table_counter - 1);
            list_BatchSql = GetBatchSql(table_namew, ClothNumber, LSring, begin_index, end_index);

            ExecuteBatchSql(list_BatchSql);
        }

        public void InitResultMessage(HTuple hv_tupleBackgroundColor,
            HTuple hv_tupleDefectRow1, HTuple hv_tupleDefectRow2, HTuple hv_tupleDefectColumn1,
            HTuple hv_tupleDefectColumn2, HTuple hv_minWidth, HTuple hv_maxWidth, HTuple hv_meanWidth,
            HTuple hv_metersCounter, HTuple hv_tupleMessages, HTuple hv_tupleMessagesColor,
            HTuple hv_LeftDetectSide, HTuple hv_RightDetectSide)
        {

            //HTuple hv_tupleBackgroundColor=new HTuple();

            //HTuple hv_tupleDefectRow1 = new HTuple(), hv_tupleDefectRow2 = new HTuple();
            //HTuple hv_tupleDefectColumn1 = new HTuple(), hv_tupleDefectColumn2 = new HTuple();

            //HTuple MinWidth, MaxWidth, MeanWidth;
            //HTuple hv_metersCounter=new HTuple(), hv_tupleMessages = new HTuple(), hv_tupleMessagesColor = new HTuple(), hv_leftDetectSide = new HTuple(), hv_rightDetectSide = new HTuple();

            //HTuple hv_result = null, hv_tupleDefectClass, hv_DetectResult;


            //hv_tupleBackgroundColor[0] = 80;
            //hv_tupleBackgroundColor[1] = 80;
            //hv_tupleBackgroundColor[2] = 80;

            //hv_tupleDefectRow1 = 40;
            //hv_tupleDefectRow2 = 40;
            //hv_tupleDefectColumn1 = 30;
            //hv_tupleDefectColumn2 = 30;

            //MinWidth = 1800;
            //MaxWidth = 2000;
            //MeanWidth = 1900;

            DispResultDict.Clear();

            try
            {

                DispResultDict.Add("backcolor", 0);
                DispResultDict["backcolor"] = hv_tupleBackgroundColor;

                DispResultDict.Add("hv_tupleDefectRow1", 0);
                DispResultDict["hv_tupleDefectRow1"] = hv_tupleDefectRow1;
                DispResultDict.Add("hv_tupleDefectRow2", 0);
                DispResultDict["hv_tupleDefectRow2"] = hv_tupleDefectRow2;
                DispResultDict.Add("hv_tupleDefectColumn1", 0);
                DispResultDict["hv_tupleDefectColumn1"] = hv_tupleDefectColumn1;
                DispResultDict.Add("hv_tupleDefectColumn2", 0);
                DispResultDict["hv_tupleDefectColumn2"] = hv_tupleDefectColumn2;

                DispResultDict.Add("minwidth", 0);
                DispResultDict["minwidth"] = hv_minWidth;
                DispResultDict.Add("maxwidth", 0);
                DispResultDict["maxwidth"] = hv_maxWidth;
                DispResultDict.Add("MeanWidth", 0);
                DispResultDict["MeanWidth"] = hv_meanWidth;

                DispResultDict.Add("metersCounter", 0);
                DispResultDict["metersCounter"] = hv_metersCounter;

                DispResultDict.Add("hv_tupleMessages", 0);
                DispResultDict["hv_tupleMessages"] = hv_tupleMessages;

                DispResultDict.Add("hv_tupleMessagesColor", 0);
                DispResultDict["hv_tupleMessagesColor"] = hv_tupleMessagesColor;

                DispResultDict.Add("hv_leftDetectSide", 0);
                DispResultDict["hv_leftDetectSide"] = hv_LeftDetectSide;

                DispResultDict.Add("hv_rightDetectSide", 0);
                DispResultDict["hv_rightDetectSide"] = hv_RightDetectSide;
            }
            catch (Exception ex)
            {

                WritetoListbox("数据添加异常");
                WriteLog(ex, "");
            }



        }
        ///
        public void GetResultMessage(out HTuple hv_tupleBackgroundColor,
         out HTuple hv_tupleDefectRow1, out HTuple hv_tupleDefectRow2, out HTuple hv_tupleDefectColumn1,
          out HTuple hv_tupleDefectColumn2, out HTuple hv_minWidth, out HTuple hv_maxWidth, out HTuple hv_meanWidth,
         out HTuple hv_metersCounter, out HTuple hv_tupleMessages, out HTuple hv_tupleMessagesColor,
         out HTuple hv_LeftDetectSide, out HTuple hv_RightDetectSide)
        {


            hv_tupleBackgroundColor = DispResultDict["backcolor"];

            hv_tupleDefectRow1 = DispResultDict["hv_tupleDefectRow1"];
            hv_tupleDefectRow2 = DispResultDict["hv_tupleDefectRow2"];
            hv_tupleDefectColumn1 = DispResultDict["hv_tupleDefectColumn1"];
            hv_tupleDefectColumn2 = DispResultDict["hv_tupleDefectColumn2"];


            hv_minWidth = DispResultDict["minwidth"];
            hv_maxWidth = DispResultDict["maxwidth"];
            hv_meanWidth = DispResultDict["MeanWidth"];


            hv_metersCounter = DispResultDict["metersCounter"];
            hv_tupleMessages = DispResultDict["hv_tupleMessages"];
            hv_tupleMessagesColor = DispResultDict["hv_tupleMessagesColor"];


            hv_LeftDetectSide = DispResultDict["hv_leftDetectSide"];
            hv_RightDetectSide = DispResultDict["hv_rightDetectSide"];




        }

        public void InitResultMessage()
        {

            HTuple hv_tupleBackgroundColor = new HTuple();

            HTuple hv_tupleDefectRow1 = new HTuple(), hv_tupleDefectRow2 = new HTuple();
            HTuple hv_tupleDefectColumn1 = new HTuple(), hv_tupleDefectColumn2 = new HTuple();

            HTuple MinWidth, MaxWidth, MeanWidth;
            HTuple hv_metersCounter = new HTuple(), hv_tupleMessages = new HTuple(), hv_tupleMessagesColor = new HTuple(), hv_leftDetectSide = new HTuple(), hv_rightDetectSide = new HTuple();

            HTuple hv_result = null, hv_tupleDefectClass, hv_DetectResult;
            HTuple hv_minWidth, hv_maxWidth, hv_meanWidth, hv_LeftDetectSide, hv_RightDetectSide;

            hv_tupleBackgroundColor[0] = 80;
            hv_tupleBackgroundColor[1] = 80;
            hv_tupleBackgroundColor[2] = 80;

            hv_tupleDefectRow1 = 40;
            hv_tupleDefectRow2 = 40;
            hv_tupleDefectColumn1 = 30;
            hv_tupleDefectColumn2 = 30;

            hv_minWidth = 1800;
            hv_maxWidth = 2000;
            hv_meanWidth = 1900;
            hv_LeftDetectSide = 40;



            DispResultDict.Clear();
            DispResultDict.Add("backcolor", 0);

            DispResultDict.Add("hv_tupleDefectRow1", 0);
            DispResultDict.Add("hv_tupleDefectRow2", 0);

            DispResultDict.Add("hv_tupleDefectColumn1", 0);
            DispResultDict.Add("hv_tupleDefectColumn2", 0);

            DispResultDict.Add("minwidth", 0);
            DispResultDict.Add("maxwidth", 0);
            DispResultDict.Add("MeanWidth", 0);


            DispResultDict.Add("metersCounter", 0);
            DispResultDict.Add("hv_tupleMessages", 0);
            DispResultDict.Add("hv_tupleMessagesColor", 0);


            DispResultDict.Add("hv_LeftDetectSide", 0);
            DispResultDict.Add("hv_RightDetectSide", 0);



        }
        /// <summary>
        /// 获得数据库列表索引
        ///  </summary>
        /// <param name="sss">DefectNumber</param>
        /// <param name="sss1">ImagePath</param>
        /// <param name="sss2">输入</param>
        ///
        protected void GetColumnIndex(out int sss, out int sss1, out int sss2)
        {
            sss = ResultColumeName.FindIndex(s => s.Equals("DefectNumber"));
            sss1 = ResultColumeName.FindIndex(s => s.Equals("ImagePath"));
            sss2 = ResultColumeName.FindIndex(s => s.Equals("ClothNumber"));

        }

        protected void GetColumnIndex(string sss2, List<string> tablecolumn, out int sss)
        {
            sss = tablecolumn.FindIndex(s => s.Equals(sss2));


        }

        /// <summary>
        /// 将异常打印到LOG文件
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="LogAddress">日志文件地址</param>
        public static void WriteLog(Exception ex, string LogAddress = "")
        {

            //如果日志文件为空，则默认在Debug目录下新建 YYYY-mm-dd_Log.log文件
            if (LogAddress == "")
            {

                LogAddress = Environment.CurrentDirectory + "\\log\\" +
                             DateTime.Now.Year + '-' +
                             DateTime.Now.Month + '-' +
                             DateTime.Now.Day + "_Log.log";
            }


            //把异常信息输出到文件
            StreamWriter sw = new StreamWriter(LogAddress, true);
            sw.WriteLine("当前时间：" + DateTime.Now.ToString());
            sw.WriteLine("异常信息：" + ex.Message);
            sw.WriteLine("异常对象：" + ex.Source);
            sw.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
            sw.WriteLine("触发方法：" + ex.TargetSite);
            sw.WriteLine();
            sw.Close();
        }

        static void test(string[] args)
        {
            Thread th1 = new Thread(new ParameterizedThreadStart(MakeException));
            Thread th2 = new Thread(new ParameterizedThreadStart(MakeException));

            th1.Start("Thread1");
            th2.Start("Thread2");
        }

        /// <summary>
        /// 制造异常
        /// </summary>
        /// <param name="Tag">传入标签</param>
        public static void MakeException(object Tag)
        {
            while (true)
            {
                try
                {
                    throw new Exception("测试异常");
                }
                catch (Exception ex)
                {
                    WriteLog(ex, Tag.ToString());
                }
            }
        }

        public static object locker = new object();
        private bool bOutlinebyThread=false;

        /// <summary>
        /// 将异常打印到LOG文件
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="LogAddress">日志文件地址</param>
        /// <param name="Tag">传入标签（这里用于标识函数由哪个线程调用）</param>
        public static void WriteLog(Exception ex, string Tag = "", string LogAddress = "")
        {
            lock (locker)
            {
                //如果日志文件为空，则默认在Debug目录下新建 YYYY-mm-dd_Log.log文件
                if (LogAddress == "")
                {
                    LogAddress = Environment.CurrentDirectory + '\\' +
                        DateTime.Now.Year + '-' +
                        DateTime.Now.Month + '-' +
                        DateTime.Now.Day + "_Log.log";
                }

                //把异常信息输出到文件
                StreamWriter sw = new StreamWriter(LogAddress, true);
                sw.WriteLine(String.Concat('[', DateTime.Now.ToString(), "] Tag:" + Tag));
                sw.WriteLine("异常信息：" + ex.Message);
                sw.WriteLine("异常对象：" + ex.Source);
                sw.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                sw.WriteLine("触发方法：" + ex.TargetSite);
                sw.WriteLine();
                sw.Close();
            }
        }


        private void WritetoListbox(string e)
        {
            PublicClass.message = e;
            listBox1.Items.Add(PublicClass.message + "  from " + MetersCounter.ToString() + "  at  " + DateTime.Now.ToString());
            listBox1.TopIndex = listBox1.Items.Count - 1;
        }

        private static bool MyPredicate(string v)
        {
            bool result = false;
            if (v == "SourceID")
                result = true;
            return result;
        }


        //*****************************************************************************************
        //
        //					Create and Destroy Object
        //
        //*****************************************************************************************

        // Create new object with acquisition information 
        public bool CreateNewObjects(AcqConfigDlg acConfigDlg, bool Restore)
        {
            Restore = false;
            if (m_online)
            {

                if (!Restore)
                {
                    m_ServerLocation = new SapLocation("Xtium-CL_MX4_1", 3); //acConfigDlg.ServerLocation;
                    m_ConfigFileName = "C:\\Users\\Administrator\\Desktop\\T_Xtium_JAI-LT-400_1Tap_LineTrigger.ccf"; ;// acConfigDlg.ConfigFile;
                }

                if (SapBuffer.IsBufferTypeSupported(m_ServerLocation, SapBuffer.MemoryType.ScatterGather))
                {
                    m_Buffer = new SapBufferWithTrash();
                    m_Buffers[0] = new SapBufferRoi(m_Buffer);
                    m_Buffers[1] = new SapBufferRoi(m_Buffer);
                }
                else
                {
                    m_Buffer = new SapBufferWithTrash();
                    m_Buffers[0] = new SapBufferRoi(m_Buffer);
                    m_Buffers[1] = new SapBufferRoi(m_Buffer);
                }

                m_Xfer[0] = new SapAcqToBuf(m_Acquisition[0], m_Buffers[0]);
                m_Xfer[1] = new SapAcqToBuf(m_Acquisition[1], m_Buffers[1]);
                // m_View = new SapView(m_Buffer);


                //event for view
                m_Xfer[0].Pairs[0].EventType = SapXferPair.XferEventType.EndOfFrame;
                m_Xfer[0].XferNotify += new SapXferNotifyHandler(xfer_XferNotify);
                m_Xfer[0].XferNotifyContext = this;

                m_Xfer[1].Pairs[0].EventType = SapXferPair.XferEventType.EndOfFrame;
                m_Xfer[1].XferNotify += new SapXferNotifyHandler(xfer_XferNotify1);
                m_Xfer[1].XferNotifyContext = this;

                // event for signal status
                //  m_Acquisition[0].SignalNotify += new SapSignalNotifyHandler(GetSignalStatus);
                // m_Acquisition[0].SignalNotifyContext = this;

                //   m_Acquisition[1].SignalNotify += new SapSignalNotifyHandler(GetSignalStatus);
                //  m_Acquisition[1].SignalNotifyContext = this;
            }
            else
            {
                //define off-line object
                m_Buffer = new SapBuffer();
                //  m_View = new SapView(m_Buffer);
                StatusLabelInfo.Text = "Offline...Please load 2 acquisition devices";
            }

            // m_View.SetScalingMode(0.5F, 0.5F);

            // m_ImageBox.View = m_View;
            //  m_ImageBox.View.SetScalingMode(0.5f, 0.5f);
            //  m_ImageBox.Refresh();
            // m_ImageBox.OnSize();
            if (!CreateObjects())
            {
                DisposeObjects();
                return false;
            }

            //   m_ImageBox.OnSize();
            EnableSignalStatus();
            //自动缩小view框大小

            // button_View_Click(null, null);
            UpdateControls();

            return true;
        }

        // Call Create method  
        private bool CreateObjects()
        {

            if (m_online)
            {
                m_Buffer.Count = 1;
                m_Buffer.Width = 2 * m_Acquisition[0].XferParams.Width;
                m_Buffer.Height = m_Acquisition[0].XferParams.Height;
                m_Buffer.Format = m_Acquisition[0].XferParams.Format;
                m_Buffer.PixelDepth = m_Acquisition[0].XferParams.PixelDepth;
            }
            // Create buffer object




            if (m_Buffer != null && !m_Buffer.Initialized)
            {
                if (m_Buffer.Create() == false)
                {
                    DestroyObjects();
                    return false;
                }
                m_Buffer.Clear();
            }

            if (m_online)
            {
                m_Buffers[0].SetRoi(0, 0, m_Acquisition[0].XferParams.Width, m_Acquisition[0].XferParams.Height);
                if (m_Buffers[0] != null && !m_Buffers[0].Initialized)
                {
                    if (m_Buffers[0].Create() == false)
                    {
                        DestroyObjects();
                        return false;
                    }
                }

                m_Buffers[1].SetRoi(m_Acquisition[0].XferParams.Width, 0, m_Acquisition[0].XferParams.Width, m_Acquisition[0].XferParams.Height);
                if (m_Buffers[1] != null && !m_Buffers[1].Initialized)
                {
                    if (m_Buffers[1].Create() == false)
                    {
                        DestroyObjects();
                        return false;
                    }
                }
            }

            // Create view object
            //  if (m_View != null && !m_View.Initialized)
            //  {
            //     if (m_View.Create() == false)
            //     {
            //         DestroyObjects();
            //         return false;
            //     }
            // }
            // Create Xfer object
            if (m_Xfer[0] != null && !m_Xfer[0].Initialized)
            {
                if (m_Xfer[0].Create() == false)
                {
                    DestroyObjects();
                    return false;
                }
            }
            if (m_Xfer[1] != null && !m_Xfer[1].Initialized)
            {
                if (m_Xfer[1].Create() == false)
                {
                    DestroyObjects();
                    return false;
                }
            }
            return true;
        }

        //Call Destroy method
        private void DestroyObjects()
        {
            try
            {
                //stop grabbing
                if (m_Xfer[0] != null && m_Xfer[0].Grabbing)
                    m_Xfer[0].Abort();
                if (m_Xfer[1] != null && m_Xfer[1].Grabbing)
                    m_Xfer[1].Abort();

                if (m_Xfer[0] != null && m_Xfer[0].Initialized)
                    m_Xfer[0].Destroy();
                if (m_Xfer[1] != null && m_Xfer[1].Initialized)
                    m_Xfer[1].Destroy();
                // if (m_View != null && m_View.Initialized)
                //     m_View.Destroy();
                if (m_Buffers[0] != null && m_Buffers[0].Initialized)
                    m_Buffers[0].Destroy();
                if (m_Buffers[1] != null && m_Buffers[1].Initialized)
                    m_Buffers[1].Destroy();
                if (m_Buffer != null && m_Buffer.Initialized)
                    m_Buffer.Destroy();
                if (m_Acquisition[0] != null && m_Acquisition[0].Initialized)
                    m_Acquisition[0].Destroy();
                if (m_Acquisition[1] != null && m_Acquisition[1].Initialized)
                    m_Acquisition[1].Destroy();

            }
            catch (Exception ex)
            {
                WritetoListbox("结果显示异常");
                WriteLog(ex, "");
            }

        }

        private void DisposeObjects()
        {
            try
            {
                if (m_Xfer[1] != null)
                { m_Xfer[1].Dispose(); }
                if (m_Xfer[0] != null)
                { m_Xfer[0].Dispose(); m_Xfer = null; }

                // if (m_View != null)
                //{ m_View.Dispose(); m_View = null; m_ImageBox.View = null; }

                if (m_Buffers[1] != null)
                { m_Buffers[1].Dispose(); }
                if (m_Buffers[0] != null)
                { m_Buffers[0].Dispose(); m_Buffers = null; }

                if (m_Buffer != null)
                { m_Buffer.Dispose(); m_Buffer = null; }

                if (m_Acquisition[1] != null)
                { m_Acquisition[1].Dispose(); }
                if (m_Acquisition[0] != null)
                { m_Acquisition[0].Dispose(); m_Acquisition = null; }
            }
            catch (Exception ex)
            {
                WritetoListbox("结果显示异常");
                WriteLog(ex, "");
            }


        }

        //**********************************************************************************
        //
        //				Window related functions
        //
        //**********************************************************************************


        protected override void OnResize(EventArgs argsResize)
        {
            // if (m_ImageBox != null)
            //  m_ImageBox.OnSize();
            base.OnResize(argsResize);
        }

        private void MultiBoardSyncGrabDemoDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            MessageBoxHelper.IsWorking = false;

            DestroyObjects();
            DisposeObjects();
        }

        //*****************************************************************************************
        //
        //					File Control
        //
        //*****************************************************************************************

        private void button_New_Click(object sender, EventArgs e)
        {
            // m_Buffer.Clear();
            // m_ImageBox.Refresh();
        }



        private void button_View_Click(object sender, EventArgs e)
        {


            // ViewDlg viewDialog = new ViewDlg(m_View, m_ImageBox.ViewRectangle);
            // if(sender==null)
            //  {
            //  viewDialog.button_OK_Click("OK", null);
            //      m_ImageBox.OnSize();
            //      m_ImageBox.Refresh();
            //  }
            //  //if(sender=="view")
            // else {

            //if (viewDialog.ShowDialog() == DialogResult.OK)
            //     m_ImageBox.OnSize();
            //  m_ImageBox.Refresh();
            //  }


        }

        //*****************************************************************************************
        //
        //					General Function
        //
        //*****************************************************************************************

        private void button_Exit_Click(object sender, EventArgs e)
        {
            if (m_online)
            {
                if (m_Xfer[0].Freeze() && m_Xfer[1].Freeze())
                {
                    // if (abort0.ShowDialog() != DialogResult.OK && abort1.ShowDialog() != DialogResult.OK)
                    //  {
                    m_Xfer[0].Abort();
                    m_Xfer[1].Abort();
                    //    }
                    UpdateControls();

                }
            }

            if (PublicClass.serialPort1.IsOpen)
                ExecutePLCCommand(STOP_MOTOR);
            PublicClass.serialPort1.Close();
            Application.Exit();
            Process[] pProcess;
            pProcess = Process.GetProcesses();
            for (int i = 1; i <= pProcess.Length - 1; i++)
            {
                if (pProcess[i].ProcessName == "MySQLV1.01.exe")   //任务管理器应用程序的名
                {
                    pProcess[i].Kill();
                    break;
                }
            }
        }
        private void EnableSignalStatus()
        {
            if (m_Acquisition[0] != null)
            {
                m_IsSignalDetected = (m_Acquisition[0].SignalStatus != SapAcquisition.AcqSignalStatus.None);
                if (m_IsSignalDetected == false)
                {
                    StatusLabelInfo.ForeColor = Color.Red;
                    StatusLabelInfo.Text = "相机状态：离线";
                }
                else
                {
                    StatusLabelInfo.ForeColor = SystemColors.ControlText;
                    StatusLabelInfo.Text = "相机状态： 在线";
                }
                m_Acquisition[0].SignalNotifyEnable = true;
            }
        }

        private void SystemEvents_SessionEnded(object sender, SessionEndedEventArgs e)
        {
            // The FormClosed event is not invoked when logging off or shutting down,
            // so we need to clean up here too.
            DestroyObjects();
            DisposeObjects();
        }

        // Updates the menu items enabling/disabling the proper items depending on the state of the application
        void UpdateControls()
        {
            bool bAcqNoGrab = (m_Xfer[0] != null) && (m_Xfer[0].Grabbing == false);
            bool bAcqGrab = (m_Xfer[0] != null) && (m_Xfer[0].Grabbing == true);
            bool bNoGrab = (m_Xfer[0] == null) || (m_Xfer[0].Grabbing == false);

            // Acquisition Control
            // button_Grab.Enabled = bAcqNoGrab && m_online;
            button_Snap.Enabled = bAcqNoGrab && m_online;
            //  button_Freeze.Enabled = bAcqGrab && m_online;

        }

        //*****************************************************************************************
        //
        //					Acquisition Control
        //
        //****************************************************************************************

        private void button_Snap_Click(object sender, EventArgs e)
        {
            bsavedimage = true;
            AbortDlg abort0 = new AbortDlg(m_Xfer[0]);
            AbortDlg abort1 = new AbortDlg(m_Xfer[1]);
            if (m_Xfer[0].Snap() && m_Xfer[1].Snap())
            {
                if (abort0.ShowDialog() != DialogResult.OK && abort1.ShowDialog() != DialogResult.OK)
                {
                    m_Xfer[0].Abort();
                    m_Xfer[1].Abort();
                    bsavedimage = true;

                }
                UpdateControls();
            }
        }

        private void button_Grab_Click(object sender, EventArgs e)
        {

            loadparasfromini(1);

            //  PublicClass.clothNumber =null;

            if (PublicClass.clothNumber == null)
            {

                //添加完参数后，依然会跳到这里
                DialogResult dr = MessageBox.Show("请单击更换布匹填写待检测布匹的详情！", "更换布匹", MessageBoxButtons.OKCancel);
                // return;
                if (dr == DialogResult.OK)
                {
                    ClothDetailForm fm = new ClothDetailForm();             //设置窗体声明
                    fm.Show();

                }
                else if (dr == DialogResult.Cancel)
                {
                    // button1.Text = "取消了！";
                }

            }
            else
            {

            }

            
            btn_Exit.Enabled = false;

            // MetersCounter = 0;
            //  leftImgCounter = 0;
            //  rightImgCounter = 0;

            //插入标准lab值；
            if (chk_standlab.Checked)
            {
                if (chk_writetodb.Checked)
                {
                    MetersCounter = 0;
                    //检测日期和开始时间

                    int sss;
                    int sss1;
                    int sss2;

                    GetColumnIndex("Date", DetailColumeName, out sss2);
                    DetailValueList[sss2] = "'" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "'";
                    GetColumnIndex("StartTime", DetailColumeName, out sss2);
                    DetailValueList[sss2] = "'" + DateTime.Now.ToString("hh:mm:ss") + "'";
                    //结果写入数据库
                    this.Invoke(new delegate_WriteToSql(GetTableColumnName), "clothdetectdetail", DetailValueList, sss2 - 1, sss2, true);
                }
            }


            toolStripLabel7.Text = DateTime.Now.ToString("hh:mm:ss");
            StatusLabelInfoTrash.Text = "";
            try
            {
                //if (m_Xfer[0].Grab() && m_Xfer[1].Grab())//
                UpdateControls();
            }
            catch (Exception ex)
            {
                WritetoListbox("相机未连接");
                WriteLog(ex, "");
            }




            //执行电机启动 chk_executecmd.Checked &&

            if (chk_executecmd.Checked && PublicClass.serialPort1.IsOpen)
                ExecutePLCCommand(START_MOTOR);

            WritetoListbox("开始检测");

        }
        public static byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }



        /// <summary>
        /// ExecuteCommand 发送串口命令，执行运动
        /// </summary>
        /// <param name="v"></param>命令选择器
        /// 

        private void ExecutePLCCommand(int v)
        {

            string datapacket = "";
            switch (v)
            {
                //启动电机
                case 1001:
                    datapacket = "01 05 07 D0 FF 00 8C B7";
                    break;
                //停止电机
                case 1002:
                    datapacket = "01 05 07 D0 00 00 CD 47";
                    break;
                //启动警报
                case 1003:
                    datapacket = "01 05 07 D1 FF 00 DD 77";
                    break;
                //取消警报
                case 1004:
                    datapacket = "01 05 07 D1 00 00 9C 87";
                    break;
                //开始打标
                case 1005:
                    datapacket = "01 05 07 D2 FF 00 2D 77"; //0x01 0x05 0x07 0xD2 0xFF 0x00 0x2D 0x77
                    break;
                //结束打标
                case 1006:
                    datapacket = "01 05 07 D0 FF 00 8C B7";
                    break;
                //读取脉冲
                case 1007:
                    datapacket = "01 05 07 D0 FF 00 8C B7";
                    break;


            }
            if (datapacket.Length > 2)
            {
                byte[] array = HexStringToByteArray(datapacket);
                PublicClass.serialPort1.Write(array, 0, array.Length);
                Thread.Sleep(10);
            }
        }
        /// <summary>
        /// timer2_Tick  定时器执行算法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>



        /// <summary>
        /// btn_writetodb_Click  手动批量随机写入result数据库，用来完成统计，并写入detail数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_writetodb_Click(object sender, EventArgs e)
        {

            DetailValueList.Add("2020");
            for (int i = 1; i < 39; i++)
            {
                DetailValueList.Add("1");

            }
            if (DetailValueList.Count > 39)
            {
                DetailValueList.RemoveRange(39, DetailValueList.Count - 39);
            }

            // PublicClass.clothNumber = "2019032001";

            // MetersCounter = 20;

            //插入标准lab值；
            DetailValueList.RemoveRange(20, 4);
            DetailValueList.Insert(20, 1.ToString());
            DetailValueList.Insert(21, 2.ToString());
            DetailValueList.Insert(22, 3.ToString());
            DetailValueList.Insert(23, 4.ToString());
            DetailValueList.RemoveAt(24);
            DetailValueList.Insert(24, "'" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "'");
            DetailValueList.RemoveAt(25);
            DetailValueList.Insert(25, "'" + DateTime.Now.ToString("hh:mm:ss") + "'");
            DetailValueList.RemoveAt(26);
            DetailValueList.Insert(26, "'" + DateTime.Now.ToString("hh:mm:ss") + "'");

            //结果写入数据库
            this.Invoke(new delegate_WriteToSql(GetTableColumnName), "clothdetectdetail", DetailValueList, 20, 26, true);

            if (hv_standardTupleL != null)
                writeLABtoini(hv_standardTupleL, hv_standardTupleA, hv_standardTupleB);

            Random rm = new Random();

            ResultValueList.Add("2019022101");
            for (int i = 1; i < 26; i++)
            {
                ResultValueList.Add("1");

            }

            if (ResultValueList.Count > 26)
            {
                ResultValueList.RemoveRange(26, ResultValueList.Count - 26);
            }
            //MetersCounter++;手动插入数据库
            for (MetersCounter = 1; MetersCounter < 30; MetersCounter++)

            {
                //更新米数
                ResultValueList.RemoveAt(2);
                ResultValueList.Insert(2, MetersCounter.ToString());
                //更新图像路径
                ResultValueList.RemoveAt(25);
                sqlimgpath = "'F:/" + MetersCounter.ToString() + ".jpg'";
                ResultValueList.Insert(25, sqlimgpath);
                //更新缺陷类型detect4class
                ResultValueList.RemoveAt(24);
                ResultValueList.Insert(24, rm.Next(1, 5).ToString());
                ////更新缺陷类型detect3class
                ResultValueList.RemoveAt(21);
                ResultValueList.Insert(21, 5.ToString());
                //rm.Next(-1, 2)
                ////更新色差AberrationGrad
                //  ResultValueList.RemoveAt(26);
                //  ResultValueList.Insert(26, rm.Next(1, 5).ToString()); //rm.Next(-1, 2)

                //去除更新六个检测框色差值
                ResultValueList.RemoveRange(4, 6);
                Color_Value[0] = rm.NextDouble();
                Color_Value[1] = rm.NextDouble() + 0.5;
                Color_Value[2] = rm.NextDouble() + 1;
                Color_Value[3] = rm.NextDouble() + 2;
                Color_Value[4] = rm.NextDouble() + 2.5;
                Color_Value[5] = rm.NextDouble() + 3.5;
                ResultValueList.Insert(4, Color_Value[0].ToString());
                ResultValueList.Insert(5, Color_Value[1].ToString());
                ResultValueList.Insert(6, Color_Value[2].ToString());
                ResultValueList.Insert(7, Color_Value[3].ToString());
                ResultValueList.Insert(8, Color_Value[4].ToString());
                ResultValueList.Insert(9, Color_Value[5].ToString());

                // cmdstr = "select * from clothdetectresult";//选择数据库表头

                List<string> MySQlTablelist = new List<string>();

                List<string> MySQlstringlist = new List<string>();


                //结果写入数据库
                this.Invoke(new delegate_WriteToSql(GetTableColumnName), "clothdetectresult", ResultValueList, 0, 25, true);

            }

        }
        /// <summary>
        /// updateRectangle()：
        /// 1、加载检测框参数；2、检测参数赋值；3、色差等级赋值
        /// </summary>
        private void updateRectangle()
        {
            //加载检测框参数
            if (PublicClass.OriginY > 0 || PublicClass.OriginX > 0)
            {
                hv_boxNumber = PublicClass.Cap_NUM;
                //boxWidth:=200框宽度，boxHeight:=200框高度
                hv_boxWidth = PublicClass.Width;
                hv_boxHeight = PublicClass.Width;
                //boxBenginX框起始X坐标
                hv_boxBenginX = PublicClass.OriginX;

                //read from setting form;
                //滤波卷积核大小medianKernal
                hv_medianKernal = PublicClass.mediankernal;
                //六个框的LAB值
                hv_tupleL = new HTuple();
                hv_tupleA = new HTuple();
                hv_tupleB = new HTuple();
                //result检测结果，0表示没有缺陷，1表示未找到布匹，2表示布匹接缝，3表示检测到缺陷
                hv_result = 0;

                hv_dynThresh = 20;
                hv_Thresh = 30;
                hv_defectArea = 5;
                hv_defectWidth = 5;
                hv_defectHeight = 10;
                hv_defectNumber = 0;


                hv_dynThresh = PublicClass.dynthresh;
                hv_dynThresh = PublicClass.Thresh;
                hv_defectArea = PublicClass.defectarea;
                hv_defectWidth = PublicClass.defectWidth;
                hv_defectHeight = PublicClass.defectHeight;
                hv_defectNumber = PublicClass.detecterName;

                //readfromclothdetail;
                hv_clothAberrationGrad1 = 0.5;
                hv_clothAberrationGrad2 = 1.5;
                hv_clothAberrationGrad3 = 3.0;
                hv_clothAberrationGrad4 = 6.0;


            }


            else

            {
                //重新加载配置文件--检测框数据
            }
        }

        /// <summary>
        /// LoadDetectParasFromIni():从配置文件加载检测参数
        /// </summary>
        void LoadDetectParasFromIni()
        {
            try
            {

                strSec = Path.GetFileNameWithoutExtension(parstrFilePath);
                //GetPrivateProfileString(strSec, "exposureTime", "", temp, 1024, parstrFilePath);
                // PublicClass.exposureTime = Convert.ToInt32(temp.ToString());

                GetPrivateProfileString(strSec, "minExposureTime", "", temp, 1024, parstrFilePath);
                PublicClass.minExposureTime = Convert.ToInt32(temp.ToString());


                GetPrivateProfileString(strSec, "maxExposureTime", "", temp, 1024, parstrFilePath);
                PublicClass.maxExposureTime = Convert.ToInt32(temp.ToString());

                GetPrivateProfileString(strSec, "mag", "", temp, 1024, parstrFilePath);
                PublicClass.mag = Convert.ToDouble(temp.ToString());

                GetPrivateProfileString(strSec, "defectWidth", "", temp, 1024, parstrFilePath);
                PublicClass.defectWidth = Convert.ToDouble(temp.ToString());
                GetPrivateProfileString(strSec, "Area", "", temp, 1024, parstrFilePath);
                PublicClass.defectArea = Convert.ToDouble(temp.ToString());

                GetPrivateProfileString(strSec, "leftside", "", temp, 1024, parstrFilePath);
                PublicClass.leftside = Convert.ToDouble(temp.ToString());

                GetPrivateProfileString(strSec, "rightside", "", temp, 1024, parstrFilePath);
                PublicClass.rightside = Convert.ToDouble(temp.ToString());

                GetPrivateProfileString(strSec, "EdgeRollslope", "", temp, 1024, parstrFilePath);
                PublicClass.EdgeRollslope = Convert.ToDouble(temp.ToString());

                GetPrivateProfileString(strSec, "imperfectBorderWidth", "", temp, 1024, parstrFilePath);
                PublicClass.imperfectBorderWidth = Convert.ToDouble(temp.ToString());

                GetPrivateProfileString(strSec, "clothSideUnDetectWidth", "", temp, 1024, parstrFilePath);
                PublicClass.clothSideUnDetectWidth = Convert.ToDouble(temp.ToString());


                GetPrivateProfileString(strSec, "thresh", "", temp, 1024, parstrFilePath);
                PublicClass.Thresh = Convert.ToInt32(temp.ToString());

                GetPrivateProfileString(strSec, "dynThresh", "", temp, 1024, parstrFilePath);
                PublicClass.dynthresh = Convert.ToInt32(temp.ToString());

                /*
                GetPrivateProfileString(strSec, "SavedImgPath", "", temp, 1024, parstrFilePath);
                PublicClass.SavedImgPath = temp.ToString();

                GetPrivateProfileString(strSec, "SqlImgPath", "", temp, 1024, parstrFilePath);
                PublicClass.SqlImgPath = temp.ToString();

                GetPrivateProfileString(strSec, "detectedpath", "", temp, 1024, parstrFilePath);
                PublicClass.detectedpath = temp.ToString();

                GetPrivateProfileString(strSec, "sqldetectedpath", "", temp, 1024, parstrFilePath);
                PublicClass.sqldetectedpath = temp.ToString();
                */

                // listBox1.Items.Add("参数加载成功！ " + DateTime.Now.ToLongTimeString().ToString());

                //GetPrivateProfileString(strSec, "dynThresh", "", temp, 1024, parstrFilePath);
                //PublicClass.dynthresh = Convert.ToInt32(temp.ToString());

                //GetPrivateProfileString(strSec, "dynThresh", "", temp, 1024, parstrFilePath);
                //PublicClass.dynthresh = Convert.ToInt32(temp.ToString());

                //GetPrivateProfileString(strSec, "dynThresh", "", temp, 1024, parstrFilePath);
                //PublicClass.dynthresh = Convert.ToInt32(temp.ToString());

                //GetPrivateProfileString(strSec, "dynThresh", "", temp, 1024, parstrFilePath);
                //PublicClass.dynthresh = Convert.ToInt32(temp.ToString());

                //GetPrivateProfileString(strSec, "dynThresh", "", temp, 1024, parstrFilePath);
                //PublicClass.dynthresh = Convert.ToInt32(temp.ToString());

                //GetPrivateProfileString(strSec, "dynThresh", "", temp, 1024, parstrFilePath);
                //PublicClass.dynthresh = Convert.ToInt32(temp.ToString());


                //    GetPrivateProfileString(strSec, "clothnumber", "", temp, 1024, parstrFilePath);
                // PublicClass.clothNumber = temp.ToString();

                // GetPrivateProfileString(strSec, "clothnumber", "", temp, 1024, parstrFilePath);
                // PublicClass.clothNumber = temp.ToString();


                //  GetPrivateProfileString(strSec, "portName", "", temp, 1024, parstrFilePath);
                // PublicClass.portName = temp.ToString();

            }

            catch (Exception ex)
            {
                WritetoListbox("参数加载失败！ ");
                WriteLog(ex, "");
            }


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

                WritePrivateProfileString(strSec, "SqlImgPath", PublicClass.SqlImgPath.Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "SavedImgPath", PublicClass.SavedImgPath.Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "detectedpath", PublicClass.detectedpath.Trim(), parstrFilePath);
                WritePrivateProfileString(strSec, "sqldetectedpath", PublicClass.sqldetectedpath.Trim(), parstrFilePath);




            }
            catch (Exception ex)
            {
                WritetoListbox("参数加载失败！ ");
                WriteLog(ex, "");
            }
        }


        private void writeLABtoini(HTuple hv_standardTupleL,HTuple hv_standardTupleA,HTuple hv_standardTupleB)
        {
            int tempvalue;
            HTuple temphhh;

            //在这里写入登入信息

            for (int i = 0; i < 6; i++)
            {
                tempvalue = i + 1;
                try
                {
                    temphhh = hv_standardTupleL[i].F;
                    if (((int)(new HTuple(temphhh.TupleGreater(0)))) != 0)
                    {
                        cinifile.IniWriteValue("标准色差信息", "L" + tempvalue.ToString(), hv_standardTupleL[i].F.ToString("0.000"));
                        cinifile.IniWriteValue("标准色差信息", "A" + tempvalue.ToString(), hv_standardTupleA[i].F.ToString("0.000"));
                        cinifile.IniWriteValue("标准色差信息", "B" + tempvalue.ToString(), hv_standardTupleB[i].F.ToString("0.000"));

                        WritetoListbox("L保存成功:" + hv_standardTupleL[i].F.ToString("0.000"));
                        WritetoListbox("A保存成功:" + hv_standardTupleA[i].F.ToString("0.000"));
                        WritetoListbox("B保存成功:" + hv_standardTupleB[i].F.ToString("0.000"));
                    }

                }
                catch (Exception ex)
                {
                    WritetoListbox("保存lab至ini加载失败！ ");
                    WriteLog(ex, "");
                }

            }



        }


        private void readLABfromini()
        {


            int tempvalue;

            hv_standardTupleL = new HTuple(0);
            hv_standardTupleA = new HTuple(0);
            hv_standardTupleB = new HTuple(0);
            HTuple numbbb;

            //在这里写入登入信息

            for (int i = 0; i < 6; i++)
            {
                tempvalue = i + 1;
                hv_standardTupleL[i] = Convert.ToDouble(cinifile.IniReadValue("标准色差信息", "L" + tempvalue.ToString()));

                hv_standardTupleA[i] = Convert.ToDouble(cinifile.IniReadValue("标准色差信息", "A" + tempvalue.ToString()));
                hv_standardTupleB[i] = Convert.ToDouble(cinifile.IniReadValue("标准色差信息", "B" + tempvalue.ToString()));
                HOperatorSet.TupleLength(hv_standardTupleL,out numbbb);
                // if (numbbb) hv_standardTupleL[i]
                //WritetoListbox("读取标准色差L个数为"+ numbbb.ToString());

            }





        }



        public void DisplayMessageBox(string v, int id)
        {
            PublicClass.message = v;
            messageboxForm = new MessageBoxForm(id);
            messageboxForm.Owner = this;
            messageboxForm.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MySqlCommand cmd = null;
            MySqlDataReader reader = null;
            List<string> list_ColName = new List<string>();
            List<Type> list_ColType = new List<Type>();
            List<string> list_finally = new List<string>();
            MetersCounter++;//数据库
            string sql1 = DateTime.Now.Date.ToString("yyyyMMdd0");

            sql1 = sql1 + MetersCounter.ToString();
            //"2019022104";
            bfirstinsert = true;
            string sql = "";
            using (MySqlConnection cnn = new MySqlConnection(PublicClass.mysqlcon))
            {
                cnn.Open();
                // if (!cnn.Open()) { return list_ColName; }

                if (bfirstinsert)
                {
                    sql = "insert into clothdetectdetail(ClothNumber) values(" + sql1 + ")";


                    cmd = new MySqlCommand(sql, cnn);

                    cmd.ExecuteNonQuery();


                    // DisplayMessageBox("插入detail数据成功！",1);

                    bfirstinsert = false;
                    sql = "insert into clothdetectresult(ClothNumber,IntervalMeter,MeterNumber,BoxNumber) values(" + sql1 + ",0.1,0.2,2)";

                    cmd.CommandText = sql;

                    cmd.ExecuteNonQuery();






                }



            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MySqlCommand cmd = null;
            MySqlDataReader reader = null;
            List<string> list_ColName = new List<string>();
            List<Type> list_ColType = new List<Type>();
            List<string> list_finally = new List<string>();
            MetersCounter++;//插入数据库
            string sql1 = DateTime.Now.Date.ToString("yyyyMMdd0");
            sql1 = sql1 + MetersCounter.ToString();
            string username1, username2, username3, username4, username5;
            string sql2 = "1003";
            // Convert.ToInt32(st.Colort_textBox.Text);
            // PublicClass.clothNumber="2019042506";
            //"2019022104";
            bfirstinsert = true;
            string sql = "";
            username1 = "";
            username2 = "";
            username3 = "";
            username4 = "";
            username5 = "";
            MySqlDataReader dr = cmd.ExecuteReader();
            using (MySqlConnection cnn = new MySqlConnection(PublicClass.mysqlcon))
            {
                cnn.Open();


                sql = "SELECT  SUM(AberrationGrad = 1) AS count1, SUM(AberrationGrad = 2) AS count2,SUM(AberrationGrad =  3) AS count3,SUM(AberrationGrad =  4) AS count4, SUM(AberrationGrad = 5) AS count5 FROM clothdetectresult ";// +" WHERE ClothNumber = " + PublicClass.clothNumber; ;
                                                                                                                                                                                                                                      //  string sql = "delete from clothdetectdetail where ClothNumber in (1002018082001,1002018082004) ";

                cmd = new MySqlCommand(sql, cnn);
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {

                    username1 = dr["count1"].ToString();
                    username2 = dr["count2"].ToString();
                    username3 = dr["count3"].ToString();
                    username4 = dr["count4"].ToString();
                    username5 = dr["count5"].ToString();


                    dr.Close();
                }



                sql = "UPDATE Clothdetectdetail SET AberrationGrad1Meters = '" + username1 + "' WHERE ClothNumber = " + PublicClass.clothNumber;
                list_finally.Add(sql);

                sql = "update Clothdetectdetail set AberrationGrad2Meters=" + username2 + " WHERE ClothNumber = " + PublicClass.clothNumber;
                list_finally.Add(sql);

                sql = "update Clothdetectdetail set AberrationGrad3Meters=" + username3 + " WHERE ClothNumber =  " + PublicClass.clothNumber;
                list_finally.Add(sql);

                sql = "update Clothdetectdetail set AberrationGrad4Meters='" + username4 + "' WHERE ClothNumber =  " + PublicClass.clothNumber;
                list_finally.Add(sql);

                sql = "update Clothdetectdetail set AberrationGrad5Meters='" + username5 + "' WHERE ClothNumber =  " + PublicClass.clothNumber;
                list_finally.Add(sql);


                //批量执行语句
                ExecuteBatchSql(list_finally);

            }
        }








        private void StatusTimer_Tick(object sender, EventArgs e)
        {

            toolStripLabel4.Text = DateTime.Now.ToString();

            if (PublicClass.conn.State == ConnectionState.Open)
            {
                toolStripLabel5.ForeColor = SystemColors.ControlText;
                //label4.ForeColor = Color.Blue;
                toolStripLabel5.Text = "数据库状态：连接";
            }
            else
            {
                toolStripLabel5.ForeColor = Color.Red;
                //label4.ForeColor = Color.Blue;
                toolStripLabel5.Text = "数据库状态：断开";

            }


            if (PublicClass.CameraCrtl0 == 0)
            {
                toolStripLabel2.ForeColor = SystemColors.ControlText;
                //label4.ForeColor = Color.Blue;
                toolStripLabel2.Text = "相机1串口：打开";
            }
            else
            {
                toolStripLabel2.ForeColor = Color.Red;
                toolStripLabel2.Text = "相机1串口：关闭";
            }
            if (PublicClass.CameraCrtl1 == 0)
            {
                toolStripLabel3.ForeColor = SystemColors.ControlText;
                //label4.ForeColor = Color.Blue;
                toolStripLabel3.Text = "相机2串口：打开";
            }
            else
            {
                toolStripLabel3.ForeColor = Color.Red;
                toolStripLabel3.Text = "相机2串口：关闭";
            }

            if (PublicClass.parChanged == true)
            {
                PublicClass.parChanged = false;
                SaveParmeterToDisk1();

            }


            if (PublicClass.clothChangedFlag)//新建布匹
            {
                PublicClass.genStandardColorFlag = true;//标准色差置位
                PublicClass.clothChangedFlag = false;
                bfirstinsert = true;
                //clear chart
                chart1.Series[0].Points.Clear();
                dataGridView1.Rows.Clear();
                //
                
                InitChart();
                
                MetersCounter = 0;
                leftImgCounter = 0;
                rightImgCounter = 0;

                chk_standlab.Checked = true;
                //******************************************
                //查看是否存在图片保存目录，如果不存在则创建
                //******************************************
                PublicClass.SavedImgPath = "D:\\布匹色差检测图片存档\\" + DateTime.Now.ToLongDateString().ToString() + "\\" + PublicClass.clothNumber.ToString();//图片保存路径
                PublicClass.SqlImgPath = "'D:/布匹色差检测图片存档/" + DateTime.Now.ToLongDateString().ToString() + "/" + PublicClass.clothNumber.ToString();//图片保存路径

                if (!Directory.Exists(PublicClass.SavedImgPath))//判断是否存在
                {
                    try
                    {
                        PublicClass.originpath = PublicClass.SavedImgPath + "\\原图\\";
                        PublicClass.detectedpath = PublicClass.SavedImgPath + "\\检测图\\";
                        PublicClass.reportpath = PublicClass.SavedImgPath + "\\检测报告\\";
                        PublicClass.sqloriginpath = PublicClass.SqlImgPath + "/原图/";
                        PublicClass.sqldetectedpath = PublicClass.SqlImgPath + "/检测图/";

                        Directory.CreateDirectory(PublicClass.originpath);//创建新路径
                        Directory.CreateDirectory(PublicClass.detectedpath);//创建新路径
                        Directory.CreateDirectory(PublicClass.reportpath);//创建新路径
                    }
                    catch (Exception exception)
                    {

                        WriteLog(exception, "");
                    }

                }

                //**************************************
                //更新label并写入数据库
                //**************************************
                labelClothNumber.Text = PublicClass.clothNumber;
                labelBatchNumber.Text = PublicClass.batchNumber;
                labelCylinderNumber.Text = PublicClass.cylinderNumber;
                labelClassNumber.Text = PublicClass.clothClassNumber;
                labelVolumnNumber.Text = PublicClass.volumnNumber;
                labelTotalVolumnNumber.Text = PublicClass.totalVolumn;
                labelActualMeters.Text = PublicClass.markedMeters;
                labelSourceName.Text = PublicClass.sourceName;
                labelDetectName.Text = PublicClass.detecterName;

                listBox1.Items.Clear();

                if (PublicClass.conn.State != ConnectionState.Open)
                {
                    PublicClass.conn = new MySqlConnection(PublicClass.mysqlcon);

                    PublicClass.conn.Open();

                }
                

                if (PublicClass.conn.State == ConnectionState.Open)
                {

                    List<string> list_ColName = new List<string>();
                    List<Type> list_ColType = new List<Type>();
                    List<string> list_finally = new List<string>();

                    string sql1 = DateTime.Now.Date.ToString("yyyyMMdd0");
                  
               
                    string sql = "";

                    // if (!cnn.Open()) { return list_ColName; }

                    if (bfirstinsert)
                    {
                        //主键数值，初次嵌入

                        if (PublicClass.clothClassNumber != "")
                        {

                            sql = "insert into ClothClass(ClothClassNumber) values('" + PublicClass.clothClassNumber +
                                  "')";

                            list_finally.Add(sql);
                        }


                        if (PublicClass.clothNumber != null)

                            {
                                sql = "insert into Clothdetectdetail(ClothNumber) values(" + PublicClass.clothNumber +
                                      ")";
                                list_finally.Add(sql);
                            }

                            if (PublicClass.cylinderNumber != "")

                            {
                                sql = "insert into ClothCylinder(CylinderNumber) values('" +
                                      PublicClass.cylinderNumber + "')";
                                list_finally.Add(sql);
                            }

                            if (PublicClass.theDeviceNumber != null)

                            {
                                sql = "insert into ClothEquipment(EquipmentNumber) values('" +
                                      PublicClass.theDeviceNumber + "')";
                                list_finally.Add(sql);
                            }

                            if (PublicClass.sourceName != null)

                            {
                                sql = "insert into ClothSource(SourceOfCloth) values('" + PublicClass.sourceName + "')";
                                list_finally.Add(sql);
                            }

                            //更新指定主键列数值下的各个列值

                            // sql = "UPDATE ClothClass SET ClothColor = '" + ClothColor.Text + "' WHERE ClothClassNumber = " + ClothClsaaNumber.Text;
                            //list_finally.Add(sql);
                            if (PublicClass.batchNumber != null)

                            {
                                sql = "update Clothdetectdetail set BatchNumber=" + PublicClass.batchNumber +
                                      " WHERE ClothNumber = " + PublicClass.clothNumber;
                                list_finally.Add(sql);
                            }

                            if (PublicClass.totalVolumn != null)

                            {
                                sql = "update Clothdetectdetail set TotalVolumn=" + PublicClass.totalVolumn +
                                      " WHERE ClothNumber = " + PublicClass.clothNumber;
                                list_finally.Add(sql);
                            }


                            if (PublicClass.volumnNumber != null)

                            {
                                sql = "update Clothdetectdetail set VolumnNumber=" + PublicClass.volumnNumber + " WHERE ClothNumber = " + PublicClass.clothNumber;
                        list_finally.Add(sql);
                            }

                            if (PublicClass.cylinderNumber != "")

                            {
                                sql = "update Clothdetectdetail set CylinderNumber='" + PublicClass.cylinderNumber +
                                      "' WHERE ClothNumber = " + PublicClass.clothNumber;
                                list_finally.Add(sql);
                            }

                            if (PublicClass.theDeviceNumber != null)

                            {
                                sql = "update Clothdetectdetail set EquipmentNumber='" + PublicClass.theDeviceNumber +
                                      "' WHERE ClothNumber = " + PublicClass.clothNumber;
                                list_finally.Add(sql);
                            }

                            if (PublicClass.markedMeters != null)

                            {
                                sql = "update Clothdetectdetail set LabelMeters=" + PublicClass.markedMeters +
                                      " WHERE ClothNumber = " + PublicClass.clothNumber;
                                list_finally.Add(sql);
                            }

                            if (PublicClass.detecterID != null)

                            {

                                sql = "update Clothdetectdetail set OperatorID=" + PublicClass.detecterID +
                                      " WHERE ClothNumber = " + PublicClass.clothNumber;
                                list_finally.Add(sql);
                            }

                            if (PublicClass.sourceID != null)

                            {
                                sql = "update Clothdetectdetail set SourceID=" + PublicClass.sourceID +
                                      " WHERE ClothNumber = " + PublicClass.clothNumber;
                                list_finally.Add(sql);
                            }

                            if (PublicClass.clothClassNumber != "")

                            {
                                sql = "update Clothdetectdetail set ClothClassNumber='" + PublicClass.clothClassNumber +
                                      "' WHERE ClothNumber = " + PublicClass.clothNumber;
                                list_finally.Add(sql);
                            }


                            //批量执行语句
                        ExecuteBatchSql(list_finally);
                        bfirstinsert = false;


                    }
                }
            }

        }



        private void trackBar1_Scroll_1(object sender, EventArgs e)
        {
            int tempExposureTime = PublicClass.minExposureTime + trackBar1.Value * ((PublicClass.maxExposureTime - PublicClass.minExposureTime) / 3);

            //label6.Text = tempExposureTime.ToString() + "μs";
            if (PublicClass.CameraCrtl0 == 0)
            {
                try
                {

                    AxJ400Cl0.GreenExposure = tempExposureTime;
                    AxJ400Cl0.RedExposure = tempExposureTime;
                    AxJ400Cl0.BlueExposure = tempExposureTime;
                    // AxJ400Cl0.WriteAllToCamera();
                    // AxJ400Cl0.StoreSettings();
                    listBox1.Items.Add("相机1亮度调节成功" + " " + DateTime.Now.ToString());
                    listBox1.TopIndex = listBox1.Items.Count - 1;

                }
                catch (Exception ex)
                {
                    WritetoListbox("相机1亮度调节失败,数据写入失败");
                    WriteLog(ex, "");
                }


            }
            else
            {
                listBox1.Items.Add("相机1亮度调节失败,串口未打开" + " " + DateTime.Now.ToString());
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }


            if (PublicClass.CameraCrtl1 == 0)
            {
                try
                {
                    AxJ400Cl1.GreenExposure = tempExposureTime;
                    AxJ400Cl1.RedExposure = tempExposureTime;
                    AxJ400Cl1.BlueExposure = tempExposureTime;
                    // AxJ400Cl1.WriteAllToCamera();
                    //  AxJ400Cl1.StoreSettings();

                    listBox1.Items.Add("相机2亮度调节成功" + " " + DateTime.Now.ToString());
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }

                catch (Exception ex)
                {
                    WritetoListbox("相机2亮度调节失败，数据写入失败");
                    WriteLog(ex, "");
                }



            }
            else
            {
                listBox1.Items.Add("相机2亮度调节失败,串口未打开" + " " + DateTime.Now.ToString());
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //if (PublicClass.conn.State != ConnectionState.Open)
            //{
            //    //数据库没有连接;
            //    MessageBox.Show("更换失败，数据库未连接！");
            //    return;
            //}
            DialogResult result = MessageBox.Show("更换布匹将结束当前检测，确定继续？", "确定更换布匹", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {

                //*********************************
                //将本次检测结果赋予给上次检测记录的记录
                //**********************************        
                PublicClass.tempClothNumber = PublicClass.clothNumber;
                PublicClass.tempBatchNumber = PublicClass.batchNumber;
                PublicClass.tempCylinderNumber = PublicClass.cylinderNumber;
                PublicClass.tempClothClassNumber = PublicClass.clothClassNumber;
                PublicClass.tempMarkedMeters = PublicClass.markedMeters;
                PublicClass.tempVolumnNumber = PublicClass.volumnNumber;
                PublicClass.tempTotalVolumn = PublicClass.totalVolumn;
                PublicClass.tempSourceName = PublicClass.sourceName;
                PublicClass.tempMeters = 0;//保存当面米数
                MetersCounter = 0;//重新计米数
                ClothInfoReset();


                PublicClass.clothNumber = null;//检测编号
                PublicClass.batchNumber = null;//不批批号
                PublicClass.cylinderNumber = null;//缸号
                PublicClass.clothClassNumber = null;//布匹类号
                PublicClass.markedMeters = null;//米数
                PublicClass.volumnNumber = null;//卷数
                PublicClass.totalVolumn = null;//总卷数
                PublicClass.sourceName = null;//货号来源名字
                PublicClass.sourceID = null; //货号来源ID

                ClothDetailForm fm = new ClothDetailForm();             //设置窗体声明
                fm.Show();
            }

        }


        private void SaveLstToTxt(ListBox lst)
        {
            // sfd.Filter = "(*.txt)|*.txt";
            //  if (sfd.ShowDialog() == DialogResult.OK)
            if (true)
            {

           
                string sPath =PublicClass.reportpath+"list_test.txt";
                FileStream fs = new FileStream(sPath, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                int iCount = lst.Items.Count - 1;
                for (int i = 0; i <= iCount; i++)
                {
                    sw.WriteLine(lst.Items[i].ToString());
                }
                sw.Flush();
                sw.Close();
                fs.Close();
           }
        }
        private void button_Freeze_Click(object sender, EventArgs e)
        {
            //停止电机chk_executecmd.Checked&&
            try
            {
                if (PublicClass.serialPort1.IsOpen)
                    ExecutePLCCommand(STOP_MOTOR);


                // AxJ400Cl0.WriteAllToCamera();
                // AxJ400Cl0.StoreSettings();

                int iWidth = Screen.PrimaryScreen.Bounds.Width;
                //屏幕高
                int iHeight = Screen.PrimaryScreen.Bounds.Height;
                //按照屏幕宽高创建位图
                Image img = new Bitmap(iWidth, iHeight);
                //从一个继承自Image类的对象中创建Graphics对象
                Graphics gc = Graphics.FromImage(img);
                //抓屏并拷贝到myimage里
                gc.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(iWidth, iHeight));
                //this.BackgroundImage = img;Guid.NewGuid().ToString()
                //保存位图
                img.Save(PublicClass.reportpath + "form.bmp");
                // img.Save(@"D:\" + PublicClass.clothNumber + ".jpg");


                try
                {
                    chart1.SaveImage(PublicClass.reportpath + "chart.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                }
                catch (Exception ex)
                {
                    WritetoListbox("保存chart.bmp异常");
                    WriteLog(ex, "");
                }

                if (true)
                {
                    try
                    {
                        int allrow = 10;
                        string longstring = "";
                        string sPath = PublicClass.reportpath + "data.txt";
                        FileStream fs = new FileStream(sPath, FileMode.Create);
                        StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

                        allrow = dataGridView1.RowCount;

                        for (int i = 0; i < allrow - 1; i++)
                        {
                            longstring = longstring + dataGridView1.Rows[i].Cells["检测米数"].Value + "\t";
                            longstring = longstring + dataGridView1.Rows[i].Cells["检测时间"].Value + "\t";
                            longstring = longstring + dataGridView1.Rows[i].Cells["缺陷结果"].Value + "\t";
                            longstring = longstring + dataGridView1.Rows[i].Cells["色差均值"].Value + "\t";

                            sw.WriteLine(longstring);
                            longstring = "";
                        }

                        sw.Flush();
                        sw.Close();
                        fs.Close();

                    }
                    catch (Exception ex)
                    {

                        WritetoListbox("写入CSV异常");
                        WriteLog(ex, "");
                    }
                }

                SaveLstToTxt(listBox1);

                btn_Exit.Enabled = true;

                /*
               // AbortDlg abort0 = new AbortDlg(m_Xfer[0]);
               //  AbortDlg abort1 = new AbortDlg(m_Xfer[1]);
                if (m_Xfer[0].Freeze() && m_Xfer[1].Freeze())
                {
                    // if (abort0.ShowDialog() != DialogResult.OK && abort1.ShowDialog() != DialogResult.OK)
                   //  {
                    m_Xfer[0].Abort();
                    m_Xfer[1].Abort();
                 //    }
                    UpdateControls();

                }
                */
                //插入结束时间；

                if (chk_writetodb.Checked)
                {

                    ////////////////////////写入结束时间、、、、、、、、、、
                    DetailValueList.RemoveAt(26);
                    DetailValueList.Insert(26, "'" + DateTime.Now.ToString("hh:mm:ss") + "'");
                    //结果写入数据库
                    this.Invoke(new delegate_WriteToSql(GetTableColumnName), "clothdetectdetail", DetailValueList, 26, 26, true);

                    ///////////////***************//////////// 写入实际米数
                    ///  
                    DetailValueList.RemoveAt(9);
                    DetailValueList.Insert(9, labelActualMeters.Text);
                    DetailValueList.RemoveAt(10);
                    DetailValueList.Insert(10, MetersCounter.ToString());
                    this.Invoke(new delegate_WriteToSql(GetTableColumnName), "clothdetectdetail", DetailValueList, 10, 11, true);

                    //当前米数清零
                    //  leftImgCounter = 0;
                    //  rightImgCounter = 0;
                    // MetersCounter = 0;
                    //*************************//结果色差统计///

                    MySqlCommand cmd = null;
                    MySqlDataReader reader = null;
                    List<string> list_ColName = new List<string>();
                    List<Type> list_ColType = new List<Type>();
                    List<string> list_finally = new List<string>();
                    //MetersCounter++;
                    string sql1 = null;
                    sql1 = sql1 + MetersCounter.ToString();
                    string username1, username2, username3, username4, username5, username6;


                    // bfirstinsert = true;
                    string sql = "";
                    username1 = "";
                    username2 = "";
                    username3 = "";
                    username4 = "";
                    username5 = "";
                    using (MySqlConnection cnn = new MySqlConnection(PublicClass.mysqlcon))
                    {
                        cnn.Open();

                        //detail表中的布匹编号
                        sql = "select sum(Defect3Class='5')from clothdetectresult  ";

                        cmd = new MySqlCommand(sql, cnn);
                        //执行结果赋值到dr，dr为只读
                        MySqlDataReader dr = cmd.ExecuteReader();

                        if (dr.Read())
                        {

                            string username = dr[0].ToString();

                            dr.Close();

                        }

                        // sql = "select count(*)from clothdetectresult where Defect3Class='0'";
                        //sql = "select Defect3Class, count(*) as counts from clothdetectresult group by Defect3Class";
                        // sql= "SELECT  SUM(Defect3Class = 5) AS count1, SUM(Defect3Class = 2) AS count2,SUM(Defect3Class = 3) AS count3,SUM(Defect3Class = 4) AS count4, SUM(Defect3Class = 5) AS count5 FROM clothdetectresult ";

                        //SELECT user_id, brand_id, count(type = 0) as type0, count(type = 1) as type1, count(type = 2) as type2, count(type = 3) as type3 FROM t_alibaba_data t
                        //    where visit_datetime >= 415 && visit_datetime <= 715
                        //group by user_id, brand_id;


                        //色差等级：统计
                        sql = "SELECT  SUM(AberrationGrad = 1) AS count1, SUM(AberrationGrad = 2) AS count2,SUM(AberrationGrad =  3) AS count3,SUM(AberrationGrad =  4) AS count4, SUM(AberrationGrad = 5) AS count5 FROM clothdetectresult  WHERE ClothNumber = " + PublicClass.clothNumber; ;

                        cmd = new MySqlCommand(sql, cnn);
                        dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {

                            username1 = dr["count1"].ToString();
                            username2 = dr["count2"].ToString();
                            username3 = dr["count3"].ToString();
                            username4 = dr["count4"].ToString();
                            username5 = dr["count5"].ToString();

                            dr.Close();
                        }


                        sql = "UPDATE Clothdetectdetail SET AberrationGrad1Meters = '" + username1 + "' WHERE ClothNumber = " + PublicClass.clothNumber;
                        list_finally.Add(sql);

                        sql = "update Clothdetectdetail set AberrationGrad2Meters=" + username2 + " WHERE ClothNumber = " + PublicClass.clothNumber;
                        list_finally.Add(sql);

                        sql = "update Clothdetectdetail set AberrationGrad3Meters=" + username3 + " WHERE ClothNumber =  " + PublicClass.clothNumber;
                        list_finally.Add(sql);

                        sql = "update Clothdetectdetail set AberrationGrad4Meters='" + username4 + "' WHERE ClothNumber =  " + PublicClass.clothNumber;
                        list_finally.Add(sql);

                        sql = "update Clothdetectdetail set AberrationGrad5Meters='" + username5 + "' WHERE ClothNumber =  " + PublicClass.clothNumber;
                        list_finally.Add(sql);

                        sql1 = "一共" + MetersCounter.ToString() + "米，一级色差" + username1 + "米，二级色差" + username2 + "米，三级色差" + username3 + "米，四级色差" + username4 + "米，五级色差" + username5 + "米";
                        sql = " UPDATE clothdetectdetail  SET AberrationDetails = '" + sql1 + "' WHERE ClothNumber = " + PublicClass.clothNumber;
                        list_finally.Add(sql);

                        //批量执行语句
                        ExecuteBatchSql(list_finally);



                        //*************************//统计最大色差与最大最小平均宽度///

                        sql = "SELECT MAX(Box1Color) AS count1,MIN(Box1Color) AS  count2,MAX(MaxWidth) AS count3,MIN(MinWidth) AS  count4, MIN(MeanWidth) AS count5 FROM clothdetectresult WHERE ClothNumber = " + PublicClass.clothNumber;
                        cmd = new MySqlCommand(sql, cnn);
                        dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {



                            username1 = dr["count1"].ToString();
                            username2 = dr["count2"].ToString();
                            username3 = dr["count3"].ToString();
                            username4 = dr["count4"].ToString();
                            username5 = dr["count5"].ToString();


                            dr.Close();
                        }

                        sql = "UPDATE Clothdetectdetail SET  MaxLABDiff=" + username1 + " WHERE ClothNumber = " + PublicClass.clothNumber;
                        list_finally.Add(sql);

                        sql = "update Clothdetectdetail set MaxBDiff=" + username2 + " WHERE ClothNumber = " + PublicClass.clothNumber;
                        list_finally.Add(sql);

                        sql = "update Clothdetectdetail set MaxWidth=" + username3 + " WHERE ClothNumber =  " + PublicClass.clothNumber;
                        list_finally.Add(sql);

                        sql = "update Clothdetectdetail set MinWidth=" + username4 + " WHERE ClothNumber =  " + PublicClass.clothNumber;
                        list_finally.Add(sql);

                        sql = "update Clothdetectdetail set MeanWidth=" + username5 + " WHERE ClothNumber =  " + PublicClass.clothNumber;
                        list_finally.Add(sql);

                        //批量执行语句
                        ExecuteBatchSql(list_finally);


                        //todo:count defectnumber where defectnumber>0;
                        sql = "SELECT  SUM(DefectNumber>0) AS count1 FROM clothdetectresult  WHERE ClothNumber = " + PublicClass.clothNumber; ;
                        cmd = new MySqlCommand(sql, cnn);
                        dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {

                            username1 = dr["count1"].ToString();
                            //  username2 = dr["count2"].ToString();
                            //  username3 = dr["count3"].ToString();
                            //  username4 = dr["count4"].ToString();
                            //  username5 = dr["count5"].ToString();
                            // DisplayMessageBox("读取result数据成功", 1);


                            dr.Close();
                        }

                        sql = "update Clothdetectdetail set QualifiedMeters =" + username1 + " WHERE ClothNumber =  " + PublicClass.clothNumber;
                        list_finally.Add(sql);

                        //  sql1= "一共"+MetersCounter.ToString()+"米，一级色差"+ username1 + "米，二级色差"+ username1 + "米";
                        // sql =" UPDATE clothdetectdetail  SET AberrationDetails = '"+sql1+"' WHERE ClothNumber = " + PublicClass.clothNumber;
                        //  list_finally.Add(sql);

                        sql1 = "一共" + MetersCounter.ToString() + "米，卷边" + username1 + "米，破洞" + username1 + "米";
                        sql = " UPDATE clothdetectdetail  SET DefectDetails = '" + sql1 + "' WHERE ClothNumber = " + PublicClass.clothNumber;
                        list_finally.Add(sql);
                        ExecuteBatchSql(list_finally);


                    }




                }

                WritetoListbox("停止检测");
            }
            catch (Exception ex)
            {
              
                WritetoListbox("停止检测异常");
                WriteLog(ex, "");
            }

        }
           

      

        private void chk_standlab_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_standlab.Checked)
            {
                PublicClass.genStandardColorFlag = true;
                chart1.Series[0].Points.Clear();
                dataGridView1.Rows.Clear();

            }

        }


        //Catch the WM_SYSCOMMAND message and process it.
        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == (int)WindowMessages.wmSysCommand)
                if (msg.WParam.ToInt32() == m_AboutID)
                {
                    AboutBox dlg = new AboutBox();
                    dlg.ShowDialog();
                    Invalidate();
                    Update();

                }

            if (msg.Msg == (int)WindowMessages.WM_GETMINMAXINFO)
            // if (msg.WParam.ToInt32() == m_AboutID)
            {
                MessageBoxHelper.FindAndKillWindow("XJaiLT400CL Control");

            }


            // Call base class function
            base.WndProc(ref msg);
        }



        private void button8_Click(object sender, EventArgs e)
        {
            if (PublicClass.conn.State != ConnectionState.Open)
            {
                MessageBox.Show("无法查看报告，数据库未连接！");
                return;
            }
            SQLForm st = new SQLForm();             //设置窗体声明
            st.Show();
        }

        private void button6_Click_2(object sender, EventArgs e)
        {

            string pdf1filename = Application.StartupPath + "//help//installation_guide.pdf";
            System.Diagnostics.Process.Start(pdf1filename);


        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (PublicClass.conn.State != ConnectionState.Open)
            {
                MessageBox.Show("无法查看报告，数据库未连接！");
                return;
            }
            if (ExternalSqlManager == null)
            {
                ExternalSqlManager = new System.Diagnostics.Process();
                ExternalSqlManager.StartInfo.FileName = Application.StartupPath + "\\MYSQLexe\\MySQLV1.03.exe";// "E:\\exe\\MySQLV1.01.exe"; Application.StartupPath + 
                ExternalSqlManager.Start();
            }
            else
            {
                if (ExternalSqlManager.HasExited) //是否正在运行
                {
                    ExternalSqlManager.Start();
                }
            }
            ExternalSqlManager.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;








            //  DataTable dt = new DataTable(" ");
            //  DataColumn dc1 = new DataColumn("prizename", Type.GetType("System.String"));
            //  DataColumn dc2 = new DataColumn("point", Type.GetType("System.Int16"));
            //  DataColumn dc3 = new DataColumn("number", Type.GetType("System.Int16"));
            //  DataColumn dc4 = new DataColumn("totalpoint", Type.GetType("System.Int64"));
            //  DataColumn dc5 = new DataColumn("prizeid", Type.GetType("System.String"));
            //  dt.Columns.Add(dc1);
            //  dt.Columns.Add(dc2);
            //  dt.Columns.Add(dc3);
            //  dt.Columns.Add(dc4);
            //  dt.Columns.Add(dc5);
            //  for (int i = 0; i < 10; i++)
            //  {
            //      DataRow dr = dt.NewRow();
            //      dr["prizename"] = "娃娃";
            //      dr["point"] = 10;
            //      dr["number"] = 1;
            //      dr["totalpoint"] = 10;
            //      dr["prizeid"] = "001";
            //      dt.Rows.Add(dr);
            //  }

            ////  InsertByDataTable(dt, conectionstring);


            //  cnn =new MySqlConnection(conectionstring);
            //  cnn.Open(); //打开数据库







            //  string sql = "select * from clothdetectresult";//选择数据库表头
            //  MySqlDataAdapter mda = new MySqlDataAdapter(sql, cnn);
            //  DataSet ds = new DataSet();
            //  mda.Fill(ds, "table1");
            //  //this.dataGridView1.DataSource = ds.Tables["table1"];

            //  string sqlstr = "select * from clothdetectresult";
            //  MySqlCommand command = new MySqlCommand(sqlstr, cnn);
            //  command.ExecuteNonQuery();

            //  List<string> list_ColName = new List<string>();
            //  List<Type> list_ColType = new List<Type>();


            //  dataGridView1.DataSource = MySqlHelper.ExecuteDataset(conectionstring, sql).Tables[0].DefaultView;
            //  MySqlCommand mycmd = new MySqlCommand(sqlstr, cnn);
            //  MySqlDataReader myreader = mycmd.ExecuteReader();



            //  while(myreader.Read())
            //  {
            //      string username = myreader.GetString("ClothNumber");
            //      string password = myreader.GetString("BoxNumber");
            //     // MessageBox.Show(username + ":" + password);
            //      Console.WriteLine(username + ":" + password);

            //      string t = myreader.GetString(0);
            //      Type tt = myreader.GetValue(1) as Type;

            //      string ttt = myreader.GetString(1);
            //      list_ColName.Add(t);
            //      list_ColType.Add(tt);
            //     // coltype.Add(ttt);
            //     // my


            //  }
            //  // cnn.Close();
            //  //cnn.Open();
            //  myreader.Close();


            //  //插入一条数据入表格
            //  string sqli = "insert into clothdetectresult(ClothNumber,IntervalMeter,MeterNumber,BoxNumber) values(1001201808001,0.1,0.2,2)";//,IntervalMeter,BoxNumber,MeterNumber,DefectNumber,,0.1,0.2,15,3
            //  MySqlCommand mycmd1 = new MySqlCommand(sqli, cnn);
            //  mycmd1.ExecuteNonQuery();

            //  cnn.Close();




        }

        public static int ExecuteSql(string SqlString)
        {

            using (PublicClass.conn = new MySqlConnection(PublicClass.mysqlcon))
            {
                using (MySqlCommand cmd = new MySqlCommand(SqlString, PublicClass.conn))
                {
                    try
                    {
                        if (PublicClass.conn.State != ConnectionState.Open)
                            PublicClass.conn.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }

                    catch (MySql.Data.MySqlClient.MySqlException ex)
                    {
                        //WritetoListbox("写入数据库异常");
                        WriteLog(ex, "");
                        return 0;
                    }
                }
            }
        }

        //// <summary>
        /// serialPort1_DataReceived  :接收，读取电机速度。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>


        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (PublicClass.serialPort1.IsOpen)
                {
                    byte[] readBuffer = new byte[PublicClass.serialPort1.ReadBufferSize];
                    PublicClass.serialPort1.Read(readBuffer, 0, readBuffer.Length);
                    if (readBuffer.Length >= 7)
                    {

                        if ((Convert.ToString(readBuffer[0], 16).PadLeft(2, '0') + Convert.ToString(readBuffer[1], 16).PadLeft(2, '0') + Convert.ToString(readBuffer[2], 16).PadLeft(2, '0')).ToUpper() == "010302")
                        {
                            string tempString = (Convert.ToString(readBuffer[3], 16).PadLeft(2, '0') + Convert.ToString(readBuffer[4], 16).PadLeft(2, '0')).ToUpper();
                            PublicClass.speed = Convert.ToInt32(tempString, 16);

                        }

                    }


                }

            }
            catch (Exception ex)
            {
                WritetoListbox("串口数据库异常");
                WriteLog(ex, "");
            }
        }



        private void MultiBoardSyncGrabDemoDlg_Load(object sender, EventArgs e)
        {
            //1、连接数据库
            PublicClass.conn = new MySqlConnection(PublicClass.mysqlcon);//连接数据库
            try
            {
                PublicClass.conn.Open();

            }
            catch (Exception ex)
            {
                WritetoListbox("数据库连接异常");
                WriteLog(ex, "");
            }
            //2、串口初始化
            try
            {

                PublicClass.serialPort1.PortName = PublicClass.serialPort1Name;
                PublicClass.serialPort1.BaudRate = 38400;
                PublicClass.serialPort1.StopBits = System.IO.Ports.StopBits.One;
                PublicClass.serialPort1.DataBits = 8;
                PublicClass.serialPort1.Parity = System.IO.Ports.Parity.None;
                PublicClass.serialPort2.PortName = PublicClass.serialPort2Name;
                PublicClass.serialPort2.BaudRate = 9600;
                PublicClass.serialPort2.StopBits = System.IO.Ports.StopBits.One;
                //PublicClass.serialPort2.Handshake = System.IO.Ports.Handshake.XOnXOff;
                PublicClass.serialPort2.DataBits = 8;
                PublicClass.serialPort2.Parity = System.IO.Ports.Parity.None;
                PublicClass.serialPort3.PortName = PublicClass.serialPort3Name;
                PublicClass.serialPort3.BaudRate = 9600;
                PublicClass.serialPort3.StopBits = System.IO.Ports.StopBits.One;
                PublicClass.serialPort3.DataBits = 8;
                PublicClass.serialPort3.Parity = System.IO.Ports.Parity.None;
                PublicClass.serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);

            }
            catch (Exception ex)
            {
                WritetoListbox("串口数据库异常");
                WriteLog(ex, "");
            }

            // PublicClass.serialPort3.Handshake = System.IO.Ports.Handshake.XOnXOff;
            try
            {
                PublicClass.serialPort1.Open();
            }
            catch (Exception ex)
            {
                WritetoListbox("串口1打开异常");
                WriteLog(ex, "");
            }


            listBox1.Items.Add("正在初始化相机 " + DateTime.Now.ToString());
            listBox1.TopIndex = listBox1.Items.Count - 1;

            //3、布匹标签信息重置

            ClothInfoReset();



            //4、初始化相机对象
            InitObject();
            label18.Visible = false;

            //5、从ini加载参数：布匹信息参数、标准色差、检测参数；
            LoadDetectParasFromIni();

            hv_ExpDefaultWinHandle = hWindowControl2.HalconWindow;

            //6、初始化相机曝光参数
            if (exposureTime <= PublicClass.minExposureTime)
                trackBar1.Value = 0;
            if (exposureTime >= PublicClass.maxExposureTime)
                trackBar1.Value = trackBar1.Maximum;
            if (exposureTime > PublicClass.minExposureTime && exposureTime < PublicClass.maxExposureTime)
            {
                for (int i = 0; i < 4; i++)
                {
                    int a = exposureTime - (PublicClass.minExposureTime + ((PublicClass.maxExposureTime - PublicClass.minExposureTime) / 3) * i);
                    int b = (PublicClass.maxExposureTime - PublicClass.minExposureTime) / 6;

                    if (a <= b)
                    {
                        trackBar1.Value = i;
                        break;
                    }

                }

            }

            //label6.Text = PublicClass.minExposureTime.ToString() + "μs";
            //7、开启硬件状态定时器
            StatusTimer.Start();

            //8、初始化图表
            InitChart();
            //9自动缩放窗体至全屏
            AutoSizeForm.controllInitializeSize(this);



            //10、从ini初始化布匹参数及检测框信息
            if (cinifile.ExistINIFile())//验证是否存在文件，存在就读取
                cinifile.loadinifile();

            //init paras;
            PublicClass.Cap_NUM = Convert.ToInt32(cinifile.IniReadValue("检测框信息", "采样个数"));
            PublicClass.Square = Convert.ToInt32(cinifile.IniReadValue("检测框信息", "采样周期"));
            PublicClass.Height = Convert.ToInt32(cinifile.IniReadValue("检测框信息", "采样宽度"));

            PublicClass.Width = Convert.ToInt32(cinifile.IniReadValue("检测框信息", "采样宽度"));
            PublicClass.OriginX = Convert.ToInt32(cinifile.IniReadValue("检测框信息", "采样起点X"));
            PublicClass.OriginY = Convert.ToInt32(cinifile.IniReadValue("检测框信息", "采样起点Y"));


            //1008
            //  GetPrivateProfileString("检测框信息", "采样个数", "", temp, 1024, myconfFilePath);
            // PublicClass.dynthresh = Convert.ToInt32(temp.ToString());

          //  PublicClass.clothNumber = cinifile.IniReadValue("布匹信息", "布匹编号");
            PublicClass.clothClassNumber = cinifile.IniReadValue("布匹信息", "布匹类号");
            PublicClass.batchNumber = cinifile.IniReadValue("布匹信息", "布匹批号");
            PublicClass.totalVolumn = cinifile.IniReadValue("布匹信息", "布匹卷数");
            PublicClass.markedMeters = cinifile.IniReadValue("布匹信息", "布匹米数");
            PublicClass.totalVolumn = cinifile.IniReadValue("布匹信息", "当前卷数");
            PublicClass.cylinderNumber = cinifile.IniReadValue("布匹信息", "布匹缸号");



            labelClothNumber.Text = PublicClass.clothNumber;
            labelBatchNumber.Text = PublicClass.batchNumber;
            labelCylinderNumber.Text = PublicClass.cylinderNumber;
            labelClassNumber.Text = PublicClass.clothClassNumber;
            labelVolumnNumber.Text = PublicClass.volumnNumber;
            labelTotalVolumnNumber.Text = PublicClass.totalVolumn;
            labelActualMeters.Text = PublicClass.markedMeters;
            labelSourceName.Text = PublicClass.sourceName;
            labelDetectName.Text = PublicClass.detecterName;



            HOperatorSet.ReadImage(out ImageTemp, "demo.jpg");
            updateRectangle();
            HOperatorSet.DispObj(ImageTemp, hWindowControl2.HalconWindow);


            //trackBar1.Maximum = Convert.ToInt32(textBox4.Text);
            //trackBar1.Minimum = Convert.ToInt32(textBox3.Text);
            //trackBar1.SmallChange = (trackBar1.Maximum - trackBar1.Minimum) / 5;
            //trackBar1.LargeChange = (trackBar1.Maximum - trackBar1.Minimum) / 5 + 1;
            //trackBar1.TickFrequency = (trackBar1.Maximum - trackBar1.Minimum) / 5;

        }


        private void MultiBoardSyncGrabDemoDlg_Shown(object sender, EventArgs e)
        {

            this.WindowState = FormWindowState.Maximized;
            this.Visible = true;

            if (checkBoxExplosueControl.Checked)
            {
                trackBar1.Enabled = true;

            }
            else
            {
                trackBar1.Enabled = false;

            }


        }

        void ClothInfoReset()
        {
            labelClothNumber.Text = null;
            labelBatchNumber.Text = null;
            labelCylinderNumber.Text = null;
            labelClassNumber.Text = null;
            labelVolumnNumber.Text = null;
            labelTotalVolumnNumber.Text = null;
            labelActualMeters.Text = null;
            labelSourceName.Text = null;
            labelDetectName.Text = PublicClass.detecterName;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxExplosueControl.Checked)
                trackBar1.Enabled = true;
            else
                trackBar1.Enabled = false;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void hWindowControl2_HMouseMove(object sender, HMouseEventArgs e)
        {

        }

        private void InitChart()
        {

            try
            {
                Series Rd1 = chart1.Series[0];
                Rd1.ChartType = SeriesChartType.Line;
                Series Rd2 = chart1.Series[1];
                Rd2.ChartType = SeriesChartType.Line;
                Series Rd3 = chart1.Series[2];
                Rd3.ChartType = SeriesChartType.Line;

                // chart1.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
                chart1.ChartAreas[0].AxisX.Interval = 5;
                chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = false;
                chart1.ChartAreas[0].AxisX.ScrollBar.Enabled = true;
                int markedMeters = Convert.ToInt32(PublicClass.markedMeters);

                if (markedMeters > 0)
                {

                    chart1.ChartAreas[0].AxisX.Maximum = markedMeters;
                    chart1.ChartAreas[0].AxisX.Interval = markedMeters / 20;

                }


                DateTime time = DateTime.Now;
                Random rm = new Random(chao_getseed());
                Series Random1 = chart1.Series[0];
                Series Random2 = chart1.Series[1];
                Series Random3 = chart1.Series[2];
                Series Random4 = chart1.Series[3];
                Series Random5 = chart1.Series[4];
                Series Random6 = chart1.Series[5];

                //Color_Value = 50.0;
                Random1.Points.AddXY(0, 0);
                Random2.Points.AddXY(0, 0);
                Random3.Points.AddXY(0, 0);  //rm.Next(1, 100)
                Random4.Points.AddXY(0, 0);
                Random5.Points.AddXY(0, 0);
                Random6.Points.AddXY(0, 0);
            }
            catch (Exception)
            {

                throw;
            }






        }

        //
        private void halcon_Click(object sender, EventArgs e)
        {
            //  HObject ImageTemp44 = DetectImgQueue.Dequeue();
            HObject ho_ImageWithDefect;
            HTuple hv_tupleDefectX = new HTuple(), hv_tupleDefectY = new HTuple(), hv_defectNumber=new HTuple();
            HTuple hv_tupleDefectRadius = null, hv_minWidth = null, hv_tupleDefectRadius1 = null;
            HTuple hv_maxWidth = null, hv_meanWidth = null, hv_standardL = null;

            HTuple hv_result = null, hv_tupleDefectClass= new HTuple();
            HTuple hv_metersCounter, hv_tupleMessages, hv_tupleMessagesColor, hv_leftDetectSide, hv_rightDetectSide;

            HTuple hv_tupleDefectRow1 = new HTuple(), hv_tupleDefectRow2 = new HTuple();
            HTuple hv_tupleDefectColumn1 = new HTuple(), hv_tupleDefectColumn2 = new HTuple();
            HTuple hv_tupleDetectResult, hv_tupleBackgroundColor;
            int Imageindex = 1;

            HObject ho_DefectRegion, ho_Image = null, ho_Boxs = null;
            HObject ho_ImageDump = null;

            // Local control variables 

            HTuple hv_magnification = null, hv_leftSide = null;
            HTuple hv_rightSide = null, hv_boxNumber = null, hv_boxWidth = null;
            HTuple hv_boxHeight = null, hv_boxBenginX = null, hv_dynThresh = null;
            HTuple hv_medianKernal = null, hv_thresh = null, hv_defectArea = null;
            HTuple hv_defectWidth = null, hv_defectHeight = null, hv_edgeRollSlope = null;
            HTuple hv_imperfectBorderWidth = null, hv_clothAberrationGrad1 = null;
            HTuple hv_clothAberrationGrad2 = null, hv_clothAberrationGrad3 = null;
            HTuple hv_clothAberrationGrad4 = null;
           // HTuple hv_tupleL = null, hv_tupleA = null, hv_tupleB = null;
         //   HTuple hv_standardTupleL = null, hv_standardTupleA = null;
          //  HTuple hv_standardTupleB = null;
         //   HTuple hv_tupleDefectX = null, hv_tupleDefectY = null;
           // HTuple hv_tupleDefectRadius = null, hv_minWidth = null;
          //  HTuple hv_maxWidth = null, hv_meanWidth = null, hv_flag = null;
            HTuple hv_ins = null, hv_windowHandle = new HTuple();
          //  HTuple hv_A = new HTuple(), hv_B = new HTuple(), hv_L = new HTuple();
            HTuple hv_clothAberration = new HTuple();
           // HTuple hv_tupleMessages = new HTuple(), hv_tupleMessagesColor = new HTuple();
           // HTuple hv_tupleDefectRow1 = new HTuple(), hv_tupleDefectRow2 = new HTuple();
           // HTuple hv_tupleDefectColumn1 = new HTuple(), hv_tupleDefectColumn2 = new HTuple();
            HTuple hv_clothSideUnDetectWidth = new HTuple(), hv_isSeperateComputer = new HTuple();
            HTuple hv_algorithmOfAberration = new HTuple();
            HTuple hv_standardA = new HTuple();
            HTuple hv_standardB = new HTuple();
            //  HTuple hv_rightDetectSide = new HTuple(), hv_tupleBackgroundColor = new HTuple();hv_leftRightAberration2 = new HTuple(),
            HTuple hv_result3 = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_DefectRegion);
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_Boxs);
            HOperatorSet.GenEmptyObj(out ho_ImageDump);
            HOperatorSet.GenEmptyObj(out ho_ImageWithDefect);



          

            //  GenerateDefectInfoSQL(hv_tupleDefectX, hv_tupleDefectClass, hv_defectNumber, hv_tupleDefectY);

            //return;


         //   RunHalcon(hWindowControl2.HalconWindow);

          //  return;

            string[] ImageFiles = { "" };

            loadparasfromini(1);

            if (chk_contiouswhenoutline.Checked)
            {

                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    Imgpath = folderBrowserDialog1.SelectedPath;
                    ImageFiles = Directory.GetFiles(Imgpath, "*.jpg");
                     Imageindex = ImageFiles.Length;


                }

            }
            else
            {
                OpenFileDialog opg = new OpenFileDialog();
                opg.InitialDirectory = " F://";
                opg.Filter = "jbg图像|*.jpg|图像文件|*.bmp|所有文件|*.*";
                //  opg.Filter = " 图像文件|*.bmp";
                opg.FilterIndex = 0;

                if (opg.ShowDialog() == DialogResult.OK)
                {
                  
                  Imgpath = opg.FileName;
                  //  ImageFiles[1] = Imgpath;

                }
            }



            HObject  ho_Rectangle, ho_Rectangle1;
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);

            try
            {
               // foreach (string imgtemppath in ImageFiles)
               // {

               // ImageFiles.Length-1

                for (int i = 0; i <= ImageFiles.Length - 1; i++)
                {

                    WritetoListbox(ImageFiles[i]);

                    if (Imageindex == 1)
                    {
                        HOperatorSet.ReadImage(out ho_Image, Imgpath);
                        DetectImgQueue.Enqueue(ho_Image);
                        DispImgQueue.Enqueue(ho_Image);

                    }
                    else
                    {
                        HOperatorSet.ReadImage(out ho_Image, ImageFiles[i]);
                        DetectImgQueue.Enqueue(ho_Image);
                        DispImgQueue.Enqueue(ho_Image);
                    }
                    //离线
                    MetersCounter++;

                    imgpath = PublicClass.originpath + MetersCounter + ".jpg";

                    if (chk_saveimg.Checked)
                    {
                        if (PublicClass.originpath != "")
                            HOperatorSet.WriteImage(ho_Image, "jpg", 0, imgpath);
                    }


                    sqlimgpath = PublicClass.sqloriginpath + MetersCounter + ".jpg'";



                    updateRectangle();
                    LoadDetectParasFromIni();


                    bOutlinebyThread = chk_outlinebythread.Checked;
                    if (bOutlinebyThread)
                    {

                        Thread processth = new Thread(new ThreadStart(ProcessAction));
                        processth.Start(); 

                    }
                    else
                    {

                        hv_result = 1;
                        ///  HOperatorSet.DispObj(ho_Image, hWindowControl2.HalconWindow);
                        // HObject ho_Boxs = null;
                        //  HTuple hv_isSeperateComputer = new HTuple();

                        hv_tupleDetectResult = new HTuple();
                        hv_tupleDetectResult[0] = 0;
                        hv_tupleDetectResult[1] = 0;
                        hv_tupleDetectResult[2] = 0;
                        hv_tupleDetectResult[3] = 0;
                        hv_tupleDetectResult[4] = 0;
                        hv_tupleDetectResult[5] = 0;
                        hv_tupleDetectResult[6] = 0;
                        hv_tupleDetectResult[7] = 0;
                        hv_tupleDetectResult[8] = 0;
                        hv_tupleDetectResult[9] = 0;
                        //瑕疵X坐标
                        hv_tupleDefectX = new HTuple();
                        //瑕疵Y坐标
                        hv_tupleDefectY = new HTuple();
                        //tupleDefectClass表示瑕疵分类,0表示接，1表示周期性缺陷，2表示卷边，3表示缺边，4表示其他瑕疵
                        hv_tupleDefectClass = new HTuple();
                        hv_L = 0;
                        hv_A = 0;
                        hv_B = 0;
                        //clothberration表示色差值
                        hv_clothAberration = 0;
                        hv_minWidth = 0;
                        hv_maxWidth = 0;
                        hv_meanWidth = 0;
                        //输出结果
                        hv_tupleMessages = new HTuple();
                        hv_tupleMessagesColor = new HTuple();
                        //检测缺陷的个数
                        hv_defectNumber = 0;
                        //缺陷框坐标
                        hv_tupleDefectRow1 = new HTuple();
                        hv_tupleDefectRow2 = new HTuple();
                        hv_tupleDefectColumn1 = new HTuple();
                        hv_tupleDefectColumn2 = new HTuple();

                        //*********************输入参数
                        //magnification放大率piexels/mm
                        //LeftSide左边有效参数
                        //RightSide右边有效参数
                        //boxNumber框个数
                        //boxWidth框宽度
                        //boxHeight框高度
                        //boxBenginX框起始X坐标
                        //dynThresh缺陷阈值
                        //medianKernal滤波卷积核大小
                        //defectArea缺陷面积
                        //edgeRollSlope判断卷边的斜率偏差
                        //imperfectBorderWidth判断缺边的宽度
                        //leftSide左边有效区域
                        //rightSide右边有效区域
                        //clothAberrationGrad1-clothAberrationGrad4色差等级分类
                        //布匹边缘不检测宽度clothSideUnDetectWidth
                        //*****************************
                        hv_magnification = 4.8188;
                        hv_leftSide = 41.5040;
                        hv_rightSide = 41.5040;
                        hv_boxNumber = 6;
                        hv_boxWidth = 83;
                        hv_boxHeight = 83;
                        hv_boxBenginX = 200;
                        hv_dynThresh = 15;
                        hv_medianKernal = 20;
                        hv_thresh = 30;
                        hv_defectArea = 0.2157;
                        hv_defectWidth = 1.0393;
                        hv_defectHeight = 1.0393;
                        hv_edgeRollSlope = 0.1;
                        hv_imperfectBorderWidth = 4.15;
                        hv_clothAberrationGrad1 = 0.5;
                        hv_clothAberrationGrad2 = 1.5;
                        hv_clothAberrationGrad3 = 3.0;
                        hv_clothAberrationGrad4 = 6.0;
                        hv_clothSideUnDetectWidth = 20.7;
                        hv_isSeperateComputer = 1;

                        hv_metersCounter = 1;
                        hv_leftSide = hv_leftSide * hv_magnification;
                        hv_rightSide = hv_rightSide * hv_magnification;
                        hv_boxWidth = hv_boxWidth * hv_magnification;
                        hv_boxHeight = hv_boxHeight * hv_magnification;
                        hv_boxBenginX = hv_boxBenginX * hv_magnification;
                        hv_defectArea = (hv_defectArea * hv_magnification) * hv_magnification;
                        hv_defectWidth = hv_defectWidth * hv_magnification;
                        hv_defectHeight = hv_defectHeight * hv_magnification;
                        hv_imperfectBorderWidth = hv_imperfectBorderWidth * hv_magnification;
                        hv_clothSideUnDetectWidth = hv_clothSideUnDetectWidth * hv_magnification;
                        hv_result = 0;
                        hv_algorithmOfAberration = 2;

                        //是否计算标准时，如果是，则将当前图像计算的lab值设为标准，并保存到本地ini文件当中，否则计算当前的色差值
                        PublicClass.genStandardColorFlag = chk_standlab.Checked;
                        if (PublicClass.genStandardColorFlag)
                        {
                            //yyyyyyy



                            //  ho_Boxs.Dispose();
                            get_standard_lab(ho_Image, out ho_Boxs, hv_isSeperateComputer, out hv_standardTupleL,
                                out hv_standardTupleA, out hv_standardTupleB, out hv_standardL, out hv_standardA,
                                out hv_standardB, out hv_tupleDetectResult, out hv_result, out hv_defectNumber,
                                out hv_tupleDefectClass, out hv_tupleDefectX, out hv_tupleDefectY,
                                out hv_tupleDefectRow1, out hv_tupleDefectRow2, out hv_tupleDefectColumn1,
                                out hv_tupleDefectColumn2, out hv_minWidth, out hv_maxWidth, out hv_meanWidth,
                                out hv_metersCounter, out hv_tupleMessages, out hv_tupleMessagesColor,
                                out hv_leftDetectSide, out hv_rightDetectSide, out hv_L, out hv_A,
                                out hv_B, out hv_ClothRegionCoordinateX1, out hv_ClothRegionCoordinateX2);



                            //get_standard_lab1(ho_Image, out ImageBoxs,out hv_standardTupleL, out hv_standardTupleA,
                            //    out hv_standardTupleB, out hv_standardL, out hv_standardA, out hv_standardB,
                            //    out hv_result, out hv_tupleDetectResult, out hv_defectNumber, out hv_tupleDefectClass,
                            //    out hv_tupleDefectX, out hv_tupleDefectY, out hv_tupleDefectRow1, out hv_tupleDefectRow2,
                            //    out hv_tupleDefectColumn1, out hv_tupleDefectColumn2, out hv_minWidth,
                            //    out hv_maxWidth, out hv_meanWidth, out hv_metersCounter, out hv_tupleMessages,
                            //    out hv_tupleMessagesColor, out hv_leftDetectSide, out hv_rightDetectSide);

                            writeLABtoini(hv_standardTupleL,
                                 hv_standardTupleA, hv_standardTupleB);

                            hv_tupleBackgroundColor = new HTuple();
                            hv_tupleBackgroundColor[0] = 0;
                            hv_tupleBackgroundColor[1] = 0;
                            hv_tupleBackgroundColor[2] = 0;


                            disp_detect_result(ho_Image, ho_Boxs, out ho_ImageWithDefect, hWindowControl2.HalconWindow, hv_tupleBackgroundColor, hv_tupleDefectRow1,
                                hv_tupleDefectRow2, hv_tupleDefectColumn1, hv_tupleDefectColumn2, hv_minWidth,
                                hv_maxWidth, hv_meanWidth, hv_metersCounter, hv_tupleMessages, hv_tupleMessagesColor,
                                hv_leftDetectSide, hv_rightDetectSide);




                            defectstr = "标准色差";

                            chk_standlab.Checked = false;
                            PublicClass.genStandardColorFlag = false;


                            double templ = 0;
                            double tempA = 0;
                            double tempB = 0;
                            double tempresult = 0;

                            try
                            {
                                HTuple number1;
                                HOperatorSet.TupleLength(hv_L, out number1);
                                if (number1 > 0)
                                    templ = hv_L;

                                HOperatorSet.TupleLength(hv_A, out number1);
                                if (number1 > 0)
                                    tempA = hv_A;
                                HOperatorSet.TupleLength(hv_B, out number1);
                                if (number1 > 0)
                                    tempB = hv_B;
                                HOperatorSet.TupleLength(hv_B, out number1);
                                if (number1 > 0)
                                    tempresult = hv_B;

                            }
                            catch (Exception ex)
                            {
                                WritetoListbox("标准色差异常");
                                WriteLog(ex, "");
                            }

                            //插入标准lab值；
                            DetailValueList.RemoveRange(20, 4);

                            DetailValueList.Insert(20, templ.ToString("0.000"));
                            DetailValueList.Insert(21, tempA.ToString("0.000"));
                            DetailValueList.Insert(22, tempB.ToString("0.000"));
                            DetailValueList.Insert(23, tempresult.ToString("0.000"));

                            //  DetailValueList[24] = "'" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "'";
                            //  DetailValueList[25] = "'" + DateTime.Now.ToString("hh:mm:ss") + "'";


                            //to do create delegate to write to sql
                            //结果写入数据库
                            // this.Invoke(new delegate_WriteToSql(GetTableColumnName), "clothdetectdetail", DetailValueList, 20, 25, true);

                            DetectDetailDict["StandardColorL"] = templ.ToString("0.000");
                            DetectDetailDict["StandardColorA"] = tempA.ToString("0.000");
                            DetectDetailDict["StandardColorB"] = tempB.ToString("0.000");
                            DetectDetailDict["StandardColorLAB"] = tempresult.ToString("0.000");
                            DetectDetailDict["Date"] = "'" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "'";
                            DetectDetailDict["StartTime"] = "'" + DateTime.Now.ToString("hh:mm:ss") + "'";

                            this.Invoke(new delegate_WriteToSqlbydict(GetAndExecuteBatchSQL), "clothdetectdetail", PublicClass.clothNumber,
                                DetectDetailDict, 0, detail_table_counter - 1);


                        }


                        else
                        {

                            if (hv_standardTupleL == null)
                            {
                                readLABfromini();
                            }


                            DispImgQueue.Enqueue(ho_Image);


                            //   HTuple hv_algorithmOfAberration =new HTuple(), hv_result3= new HTuple(), hv_leftRightAberration2 =new HTuple();

                            // hv_isSeperateComputer = new HTuple(),
                            // ho_ImageWithDefect.Dispose();



                            get_defect_aberration(ho_Image, out ho_ImageWithDefect, hWindowControl2.HalconWindow,
                                hv_standardTupleL, hv_standardTupleA, hv_standardTupleB, hv_isSeperateComputer,
                                hv_algorithmOfAberration, out hv_clothAberration, out hv_leftRightAberration2,
                                out hv_L, out hv_A, out hv_B, out hv_result, out hv_tupleDetectResult,
                                out hv_defectNumber, out hv_tupleDefectClass, out hv_tupleDefectX,
                                out hv_tupleDefectY, out hv_tupleDefectRow1, out hv_tupleDefectRow2,
                                out hv_tupleDefectColumn1, out hv_tupleDefectColumn2, out hv_minWidth,
                                out hv_maxWidth, out hv_meanWidth, out hv_metersCounter, out hv_tupleMessages,
                                out hv_tupleMessagesColor, out hv_leftDetectSide, out hv_rightDetectSide, out hv_ClothRegionCoordinateX1, out hv_ClothRegionCoordinateX2);

                            hv_tupleDefectX = (hv_tupleDefectColumn2 + hv_tupleDefectColumn1) / 2;
                            hv_tupleDefectY = (hv_tupleDefectRow2 + hv_tupleDefectRow1) / 2;
                            HOperatorSet.TupleLength(hv_tupleDefectX, out hv_defectNumber);
                            hv_tupleDefectClass = hv_tupleDefectX;
                                                                                    

                            WritetoListbox(i.ToString() + "米");

                            hv_tupleBackgroundColor = new HTuple();
                            hv_tupleBackgroundColor[0] = 80;
                            hv_tupleBackgroundColor[1] = 80;
                            hv_tupleBackgroundColor[2] = 80;


                            InitResultMessage(hv_tupleBackgroundColor, hv_tupleDefectRow1,
                                hv_tupleDefectRow2, hv_tupleDefectColumn1, hv_tupleDefectColumn2, hv_minWidth,
                                hv_maxWidth, hv_meanWidth, hv_metersCounter, hv_tupleMessages, hv_tupleMessagesColor,
                                hv_leftDetectSide, hv_rightDetectSide);


                            //GetResultMessage(hv_tupleBackgroundColor, hv_tupleDefectRow1,
                            //    hv_tupleDefectRow2, hv_tupleDefectColumn1, hv_tupleDefectColumn2, hv_minWidth,
                            //    hv_maxWidth, hv_meanWidth, hv_metersCounter, hv_tupleMessages, hv_tupleMessagesColor,
                            //    hv_leftDetectSide, hv_rightDetectSide);

                            disp_detect_result(ho_Image, ho_Boxs, out ho_ImageWithDefect, hWindowControl2.HalconWindow,
                                hv_tupleBackgroundColor, hv_tupleDefectRow1, hv_tupleDefectRow2, hv_tupleDefectColumn1,
                                hv_tupleDefectColumn2, hv_minWidth, hv_maxWidth, hv_meanWidth, hv_metersCounter,
                                hv_tupleMessages, hv_tupleMessagesColor, hv_leftDetectSide, hv_rightDetectSide);



                            //disp_detect_result(ho_Image, ImageBoxs,out ho_ImageWithDefect, hWindowControl2.HalconWindow, hv_tupleBackgroundColor, hv_tupleDefectRow1,
                            //    hv_tupleDefectRow2, hv_tupleDefectColumn1, hv_tupleDefectColumn2, hv_minWidth,
                            //    hv_maxWidth, hv_meanWidth, hv_metersCounter, hv_tupleMessages, hv_tupleMessagesColor,
                            //    hv_leftDetectSide, hv_rightDetectSide);


                            //  Thread DisplayImgThread = new Thread(new ThreadStart(DisplayImgAction));
                            //  DisplayImgThread.Start();

                            Thread.Sleep(1000);

                            detectedimgpath = PublicClass.detectedpath + MetersCounter + ".jpg";


                            sqldetectedimgpath = PublicClass.sqldetectedpath + MetersCounter + ".jpg'";

                            detectedimgpath = PublicClass.detectedpath + MetersCounter + ".jpg";
                            if (chk_saveimg.Checked)
                            {

                                try
                                {
                                    if (PublicClass.detectedpath != "")
                                        HOperatorSet.WriteImage(ho_ImageWithDefect, "jpg", 0, detectedimgpath);
                                }
                                catch (Exception)
                                {

                                    //  throw;
                                }
                            }



                            DetailValueList.RemoveAt(37);
                            DetailValueList.Insert(37, sqldetectedimgpath);
                            //结果写入数据库
                            this.Invoke(new delegate_WriteToSql(GetTableColumnName), "clothdetectdetail", DetailValueList, 37, 37, true);


                            GetAberrationGrad(hv_clothAberration,out AberrationGrad);
                            GetAberrationGrad(hv_leftRightAberration2, out leftRightAberrationGrad);
                            Color_Value[0] = hv_clothAberration;
                            // 缺陷结果赋值
                            GetDetectName(hv_result);

                            WriteToDataGridView();


                            this.Invoke(new delegate_DisplayAberrationChart(this.ShowChartResult), this.Color_Value, MetersCounter, false);

                            if (chk_writetodb.Checked)
                            {

                                try
                                {
                                    //hv_tupleDefectX[0] = 1024;
                                    //hv_tupleDefectX[1] = 2048;
                                    //hv_tupleDefectX[2] = 1024;
                                    //hv_tupleDefectX[3] = 2048;

                                    //hv_tupleDefectY[0] = 4096;
                                    //hv_tupleDefectY[1] = 2048;
                                    //hv_tupleDefectY[2] = 1024;
                                    //hv_tupleDefectY[3] = 2048;

                                    //hv_tupleDefectClass[0] = 2;
                                    //hv_tupleDefectClass[1] = 3;
                                    //hv_tupleDefectClass[2] = 2;
                                    //hv_tupleDefectClass[3] = 3;

                                    //hv_defectNumber = 4;






                                    //如果有缺陷的话，加载进数据库
                                    GenerateDefectInfoSQL(hv_tupleDefectX, hv_tupleDefectClass, hv_defectNumber, hv_tupleDefectY);

                                }
                                catch (Exception ex)
                                {

                                    WritetoListbox(ex.ToString());
                                    WriteLog(ex, "");
                                }

                                GetAlmostResultStringList(hv_minWidth, hv_meanWidth, hv_maxWidth, hv_standardL, hv_result);

                                this.Invoke(new delegate_WriteToSql(GetTableColumnName), "clothdetectresult", ResultValueList, 0, result_table_counter - 1, true);

                                for (int j = 0; j <= ResultValueList.Count - 1; j++)
                                {

                                    DetectResultDict[ResultColumeName[j]] = ResultValueList[j].ToString();


                                }

                                foreach (var columnstr in ReportColumeName)
                                {

                                    try
                                    {
                                        DetectReportDict[columnstr] = DetectResultDict[columnstr];
                                    }
                                    catch (Exception)
                                    {

                                        //   throw;
                                    }
                                }


                                DetectReportDict["AberrationGrad"] = AberrationGrad.ToString();
                                DetectReportDict["LRAberrationGrad"] = leftRightAberrationGrad.ToString();
                                DetectReportDict["Aberration"] = hv_clothAberration.TupleString("#.2f");
                                DetectReportDict["leftRightAberration"] = hv_leftRightAberration2.TupleString("#.2f");
                                DetectReportDict["FirstDefectDetail"] = "'缺陷类型：" + DetectResultDict["Defect1Class"]+"；位置：(" + DetectResultDict["Defect1PositionX"]+","+ DetectResultDict["Defect1PositionY"]+"米)' ";
                                DetectReportDict["SecondDefectDetail"] = "'缺陷类型：" + DetectResultDict["Defect2Class"] + "；位置：(" + DetectResultDict["Defect2PositionX"] + "," + DetectResultDict["Defect2PositionY"] + "米)' ";
                                DetectReportDict["ThirdDefectDetail"] = "'缺陷类型：" + DetectResultDict["Defect3Class"] + "；位置：(" + DetectResultDict["Defect3PositionX"] + "," + DetectResultDict["Defect3PositionY"] + "米)' " ;


                                this.Invoke(new delegate_WriteToSqlbydict(GetAndExecuteBatchSQL), "clothdetectreport", PublicClass.clothNumber,
                                    DetectReportDict, 0, report_table_counter - 1);




                            }
                        } 
                    }
                }
            }

            catch (Exception ex)
            {
                WritetoListbox(ex.ToString());
                WriteLog(ex, "");
            }








        }

        private void GetAlmostResultStringList1(HTuple hv_minWidth, HTuple hv_meanWidth, HTuple hv_maxWidth, HTuple hv_standardL,
          HTuple hv_result)
        {
            //布匹宽度信息

            DetectResultDict["MetersNumber"] = MetersCounter.ToString();
            DetectResultDict["sqlimgpath"] = sqlimgpath;
            DetectResultDict["AberrationGrad"] = AberrationGrad.ToString();
            DetectResultDict["minWidth"] = hv_minWidth.ToString();
            DetectResultDict["meanWidth"] = hv_meanWidth.ToString();
            DetectResultDict["maxWidth"] = hv_maxWidth.ToString();

            DetectResultDict["BoxNumber"] = PublicClass.Cap_NUM.ToString();
            DetectResultDict["Box1Color"] = Color_Value[0].ToString("0.000");
            DetectResultDict["Box2Color"] = Color_Value[1].ToString("0.000");
            DetectResultDict["Box3Color"] = Color_Value[2].ToString("0.000");
            DetectResultDict["Box4Color"] = Color_Value[3].ToString("0.000");
            DetectResultDict["Box5Color"] = Color_Value[4].ToString("0.000");
            DetectResultDict["Box6Color"] = Color_Value[5].ToString("0.000");
            DetectResultDict["Box7Color"] = Color_Value[4].ToString("0.000");
            DetectResultDict["Aberration"] = Color_Value[0].ToString("0.000");



            HTuple num;
            //保存每米步lab值  
            HOperatorSet.TupleLength(hv_L, out num);
            if (num > 0)
            {
                ResultValueList.RemoveRange(47, 3);
                ResultValueList.Insert(48, hv_standardL.ToString());
                ResultValueList.Insert(49, hv_standardA.ToString());
                ResultValueList.Insert(50, hv_standardB.ToString());
               // ResultValueList.Insert(37, hv_result.ToString());
            }




        }

        private void GetAlmostResultStringList(HTuple hv_minWidth, HTuple hv_meanWidth, HTuple hv_maxWidth, HTuple hv_standardL,
            HTuple hv_result)
        {

            ResultValueList.RemoveAt(1);
            ResultValueList.Insert(1, 1.ToString());
            ResultValueList.RemoveAt(2);
            ResultValueList.Insert(2, MetersCounter.ToString());
            //更新图像路径
            ResultValueList.RemoveAt(3);
            //  sqlimgpath = "'F:/" + PublicClass.clothNumber + "/" + MetersCounter.ToString() + ".jpg'";
            ResultValueList.Insert(3, sqlimgpath);
            //更新色差AberrationGrad
            ResultValueList.RemoveAt(4);
            ResultValueList.Insert(4, AberrationGrad.ToString());

            //布匹宽度信息

            if (hv_minWidth > 0)
            {
                ResultValueList.RemoveAt(5);
                ResultValueList.Insert(5, hv_minWidth.ToString());
                ResultValueList.RemoveAt(6);
                ResultValueList.Insert(6, hv_meanWidth.ToString());
                ResultValueList.RemoveAt(7);
                ResultValueList.Insert(7, hv_maxWidth.ToString()); 
            }

            //更新缺陷类型detect4class
            ResultValueList.RemoveAt(8);
            ResultValueList.Insert(8, PublicClass.Cap_NUM.ToString());

            // 去除更新六个检测框色差值
            ResultValueList.RemoveRange(9, 8);
            ResultValueList.Insert(9, Color_Value[0].ToString("0.00"));
            ResultValueList.Insert(10, Color_Value[1].ToString("0.00"));
            ResultValueList.Insert(11, Color_Value[2].ToString("0.00"));
            ResultValueList.Insert(12, Color_Value[3].ToString("0.00"));
            ResultValueList.Insert(13, Color_Value[4].ToString("0.00"));
            ResultValueList.Insert(14, Color_Value[5].ToString());

            ResultValueList.Insert(15, Color_Value[4].ToString("0.00"));
            ResultValueList.Insert(16, Color_Value[0].ToString("0.00"));

            HTuple num;
            //保存每米步lab值  num > 0
            HOperatorSet.TupleLength(hv_L, out num);
            if (num>0)
            {
                ResultValueList.RemoveRange(48, 4);
                ResultValueList.Insert(48, hv_L.TupleString("#.2f"));
                ResultValueList.Insert(49, hv_A.TupleString("#.2f"));
                ResultValueList.Insert(50, hv_B.TupleString("#.2f"));
               ResultValueList.Insert(51, hv_leftRightAberration2.TupleString("#.2f"));
            }
        }

        private void WriteDefectInfoToSQL(HTuple hv_defectNumber, HTuple hv_tupleDefectX, HTuple hv_tupleDefectY)
        {
            if (hv_defectNumber > 0)
            {
                if (hv_defectNumber > 10)
                    hv_defectNumber = 10;
                int Index_defectNumber = 17;
                int endpoint = (Index_defectNumber + 3 * hv_defectNumber);
                ResultValueList.RemoveRange(Index_defectNumber, 1 + 3 * hv_defectNumber);
                ResultValueList.Insert(Index_defectNumber, hv_defectNumber.ToString());

                for (int i = 0; i < hv_defectNumber; i++)
                {
                    hv_tupleDefectX[i] = 1;
                    hv_tupleDefectY[i] = 2;

                    if (hv_tupleDefectX[i] > 0)
                        ResultValueList.Insert(Index_defectNumber + 3 * i + 1, hv_tupleDefectX[i].ToString());
                    if (hv_tupleDefectY[i] > 0)
                        ResultValueList.Insert(Index_defectNumber + 3 * i + 1, hv_tupleDefectY[i].ToString());

                    ResultValueList.Insert(Index_defectNumber + 3 * i + 1, 2.ToString()); //需添加缺陷类别
                }

                this.Invoke(new delegate_WriteToSql(GetTableColumnName), "clothdetectresult", ResultValueList,
                    Index_defectNumber, endpoint, true);

                //缺陷米数计算++
            }
        }

        private void GetAberrationGrad(HTuple hv_clothAberration,out int AberrationGrad)
        {
          // 
            AberrationGrad = 0;
          

            //获得色差等级

           
         

            HTuple class11 = 0.5;
            HTuple class12 = 1.5;
            HTuple class13 = 3;
            HTuple class14 = 6;
            string sqlcmd;
            MySqlCommand mysqlcmd;//数据库执行命令
            MySqlDataReader mysqldr;//数据库查询结果
            sqlcmd = "select * from clothaberrationgrad";
            mysqlcmd = SQLForm.getSqlCommand(sqlcmd, PublicClass.conn);
            mysqldr = mysqlcmd.ExecuteReader();
            if (mysqldr.HasRows)
            {
                while (mysqldr.Read())
                {
                    class14 = Convert.ToDouble(mysqldr["clothaberrationgrad1"].ToString());
                    class13 = Convert.ToDouble(mysqldr["clothaberrationgrad2"].ToString());
                    class12 = Convert.ToDouble(mysqldr["clothaberrationgrad3"].ToString());
                    class11 = Convert.ToDouble(mysqldr["clothaberrationgrad4"].ToString());

                    break;
                }
            }
            mysqldr.Close();




            if (hv_clothAberration >= 0 && hv_clothAberration < class11)
            {
                AberrationGrad = 1;
            }

            if (hv_clothAberration > class11 && hv_clothAberration < class12)
            {
                AberrationGrad = 2;
            }

            if (hv_clothAberration > class12 && hv_clothAberration < class13)
            {
                AberrationGrad = 3;
            }

            if (hv_clothAberration > class13 && hv_clothAberration < class14)
            {
                AberrationGrad = 4;
            }

            if ( hv_clothAberration> class14)
            {
                AberrationGrad = 5;
            }

         

            // return AberrationGrad;
        }

        private void GetDetectName(HTuple hv_result)
        {
            switch (hv_result[0].I)
            {
                case 1:
                    defectstr = "未找到布匹";
                    break;
                case 2:
                    defectstr = "接缝";
                    break;
                case 3:
                    defectstr = "缺陷";
                    break;
                case 4:
                    defectstr = "其他缺陷";
                    break;
                case 5:
                    defectstr = "脏污";
                    break;
                case 0:
                    defectstr = "合格";
                    break;
            }
        }


        private void MultiBoardSyncGrabDemoDlg_SizeChanged(object sender, EventArgs e)
        {
            AutoSizeForm.controlAutoSize(this);
        }

        private void Setting_Click(object sender, EventArgs e)
        {
            ParamSettingForm st = new ParamSettingForm();             //设置窗体声明
            st.Show();
        }


        //按数据表中的顺序得到列名
        public List<string> GetTableColumnName(string SQltable, List<string> inputstring1, int begin_index, int end_index, bool isopen)
        {

            MySqlCommand cmd = null;
            MySqlDataReader reader = null;


            List<string> list_ColName = new List<string>();
            List<string> list_BatchSql = new List<string>();


           // Stopwatch.StartNew();

            

            string sql = "show columns from " + SQltable + " ;";


            if (PublicClass.conn.State != ConnectionState.Open)
            {
                PublicClass.conn = new MySqlConnection(PublicClass.mysqlcon);

                try
                {
                    PublicClass.conn.Open();

                }
                catch (Exception ex)
                {
                    WritetoListbox("数据库打开异常");
                    WriteLog(ex, "");
                }
                if (PublicClass.conn.State != ConnectionState.Open)
                {
                    WritetoListbox("数据库执行失败,数据库断开！");
                    return list_ColName;
                }
            }



            try
            {
                cmd = new MySqlCommand(sql, PublicClass.conn);
                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string t = reader.GetString(0);
                        list_ColName.Add(t);


                    }
                }
                reader.Close();



                //获取批量语句
                list_BatchSql = GetBatchSql(SQltable, PublicClass.clothNumber, list_ColName, begin_index, end_index, inputstring1);
                //批量执行语句
                ExecuteBatchSql(list_BatchSql);
                return list_ColName;

            }

            catch (Exception ex)
            {
                WritetoListbox("数据库写入异常1："+ inputstring1.Count.ToString());
                WriteLog(ex, "");
                return list_ColName;
            }

        }

        public int GetColumnNameAndCounter(string SQltable, List<string> list_ColName)
        {

            MySqlCommand cmd = null;
            MySqlDataReader reader = null;


            string sql = "show columns from " + SQltable + " ;";


            if (PublicClass.conn.State != ConnectionState.Open)
            {
                PublicClass.conn = new MySqlConnection(PublicClass.mysqlcon);

                try
                {
                    PublicClass.conn.Open();

                }
                catch (Exception ex)
                {
                    WritetoListbox("数据库打开异常");
                    WriteLog(ex, "");
                }
                if (PublicClass.conn.State != ConnectionState.Open)
                {
                    WritetoListbox("数据库打开失败,数据库断开！");
                    return list_ColName.Count;
                }
            }



            try
            {
                cmd = new MySqlCommand(sql, PublicClass.conn);
                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string t = reader.GetString(0);
                        list_ColName.Add(t);

                    }
                }
                reader.Close();

                return list_ColName.Count;
            }

            catch (Exception ex)
            {
                WritetoListbox("数据批量写入打开异常");
                WriteLog(ex, "");
                return list_ColName.Count;
            }

        }

        private List<string> GetBatchSql(string table_namew, string ClothNumber, List<string> LSring, int begin_index, int end_index, List<string> listtype)
        {

            string mysqlyuju = null;
            string wholeyuju = null;
            //string  = null;
            string column_name = null;
            string value;
            List<string> sqlfing = new List<string>();


            //如果数据中存在该类号后，不进行插入
            // if (table_namew=="clothdetectresult")
            // {

            //   }
            if (table_namew == "clothdetectresult")

            {

                mysqlyuju = "insert into " + table_namew + " (ClothNumber,MeterNumber) values(" + ClothNumber + "," + listtype[2].ToString() + ")";
                sqlfing.Add(mysqlyuju);
            }
            else
            {

                if (bfirstinsert)
                {
                    mysqlyuju = "insert into " + table_namew + " (ClothNumber) values(" + ClothNumber + ")";

                    sqlfing.Add(mysqlyuju);
                    bfirstinsert = false;

                }
            }



            // for (int i = 0; i < LSring.Count; i++)
            for (int i = begin_index; i <= end_index; i++)
            {
                //  table_namew =
                column_name = LSring[i];
                if (i < listtype.Count)
                {
                    value = listtype[i].ToString();

                    if (column_name != "ClothNumber")
                    {
                        if (table_namew == "clothdetectdetail")
                            mysqlyuju = "update " + table_namew + " set " + column_name + "=" + value + " where  ClothNumber=" + ClothNumber;
                        if (table_namew == "clothdetectresult")
                            if (i != 2)
                            {
                                mysqlyuju = "update " + table_namew + " set " + column_name + "=" + value + " where  ClothNumber=" + ClothNumber + " and  MeterNumber=" + listtype[2].ToString();
                            }
                        sqlfing.Add(mysqlyuju);

                    }
                }

            }

            return sqlfing;

        }

        private List<string> GetBatchSql(string table_namew, string ClothNumber, Dictionary<string, string> LSring, int begin_index, int end_index)
        {

            string mysqlyuju = null;
            string wholeyuju = null;
            //string  = null;
            string column_name = null;
            string value;
            List<string> sqlfing = new List<string>();


            if (table_namew != "clothdetectdetail")

            {
                if (bfirstinsert)
                {
                    mysqlyuju = "insert into " + table_namew + " (ClothNumber) values(" + ClothNumber + ")";
                   // " clothdetectdetail "
                    sqlfing.Add(mysqlyuju);
                    bfirstinsert = false;

                }
                mysqlyuju = "insert into " + table_namew + " (ClothNumber,MeterNumber) values(" + ClothNumber + "," + LSring["MeterNumber"] + ")";
                sqlfing.Add(mysqlyuju);
            }





            else
            {

                if (bfirstinsert)
                { 
                mysqlyuju = "insert into " + table_namew + " (ClothNumber) values(" + ClothNumber + ")";

                sqlfing.Add(mysqlyuju);
                    bfirstinsert = false;
                }

               // mysqlyuju = "insert into " + table_namew + " (ClothNumber,MeterNumber) values(" + ClothNumber + "," + LSring["MeterNumber"] + ")";
              //  sqlfing.Add(mysqlyuju);


            }



            foreach (KeyValuePair<string, string> kvp in LSring)
            {
                if (kvp.Key != "ClothNumber")
                {
                    if (table_namew == "clothdetectdetail")
                        mysqlyuju = "update " + table_namew + " set " + kvp.Key + "=" + kvp.Value + " where  ClothNumber=" + ClothNumber;
                    //if (table_namew == "clothdetectreport")
                     //   mysqlyuju = "update " + table_namew + " set " + kvp.Key + "=" + kvp.Value + " where  ClothNumber=" + ClothNumber;

                    if (table_namew == "clothdetectreport")
                    {
                        if (kvp.Key != "MeterNumber")
                        {
                            mysqlyuju = "update " + table_namew + " set " + kvp.Key + "=" + kvp.Value + " where  ClothNumber=" + ClothNumber + " and  MeterNumber=" + LSring["MeterNumber"];

                        }
                        else
                            continue;

                    }


                    if (table_namew == "clothdetectresult")
                    {
                        if (kvp.Key != "MeterNumber")
                        {
                            mysqlyuju = "update " + table_namew + " set " + kvp.Key + "=" + kvp.Value + " where  ClothNumber=" + ClothNumber + " and  MeterNumber=" + LSring["MeterNumber"];

                        }

                    }

                  


                    sqlfing.Add(mysqlyuju);

                }

            }

            return sqlfing;







        }

        private List<string> getsqlstring1(string table_namew, string ClothNumber, List<string> LSring, List<string> listtype)
        {

            string mysqlyuju = null;
            string wholeyuju = null;
            //string  = null;
            string column_name = null;
            string value;
            List<string> sqlfing = new List<string>();

            // mysqlyuju = "insert into " + table_namew + " (ClothNumber,MeterNumber) values(2019022202," + listtype[2].ToString() + ")";
            mysqlyuju = "insert into " + table_namew + " (ClothNumber,MeterNumber) values(" + ClothNumber + "," + listtype[0].ToString() + ")";
            sqlfing.Add(mysqlyuju); //LSring.Count
            for (int i = 1; i < LSring.Count; i++)
            {
                //  table_namew =
                column_name = LSring[i];
                value = listtype[i].ToString();

                //string sql=String.Format("update ID set number=replace(number,"1001","1000") where id='"+textbox.Text.Tostring()+"');
                if (i != 2)
                {
                    mysqlyuju = "update " + table_namew + " set " + column_name + "=" + value + " where  MeterNumber=" + listtype[2].ToString();
                    sqlfing.Add(mysqlyuju);
                }

            }

            return sqlfing;

        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>mysql数据库//static
        /// <param name="SQLStringList">多条SQL语句</param>
        public void ExecuteBatchSql(List<string> SQLStringList)
        {


            if (PublicClass.conn.State != ConnectionState.Open)
            {
                PublicClass.conn = new MySqlConnection(PublicClass.mysqlcon);

                try
                {
                    PublicClass.conn.Open();

                }
                catch (Exception ex)
                {
                    WritetoListbox("数据库打开异常");
                    WriteLog(ex, "");
                }
                if (PublicClass.conn.State != ConnectionState.Open)
                {
                    //DisplayMessageBox("删除数据失败,数据库断开！", 1);
                    listBox1.Items.Add("数据库执行事务失败,数据库断开！" + " " + DateTime.Now.ToString());
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    return;
                }
            }

            //  using (MySqlConnection conn = new MySqlConnection(conectionstring))
            //  {
            // conn.Open();
            // MySqlCommand cmd = new MySqlCommand();
            // 

            MySqlTransaction tx = PublicClass.conn.BeginTransaction();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = PublicClass.conn;
            cmd.Transaction = tx;

            try

            {
                for (int n = 0; n < SQLStringList.Count; n++)
                {
                    string strsql = SQLStringList[n].ToString();
                    if (strsql.Trim().Length > 1)
                    {
                        try
                        {
                            cmd.CommandText = strsql;
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            WritetoListbox(ex.ToString());
                            WriteLog(ex, "");
                        }

                    }
                    //每执行500条数据重启一次事务
                    if (n > 0 && (n % 500 == 0 || n == SQLStringList.Count - 1))
                    {
                        tx.Commit();
                        tx = PublicClass.conn.BeginTransaction();
                    }
                }
                tx.Commit();//原来一次性提交
            }

            catch (System.Data.SqlClient.SqlException ex)
            {
                tx.Rollback();
                WritetoListbox("批量执行异常");
                WriteLog(ex, "");

            }

            // }
        }

        //按英文字母排序得到列名
        public List<string> getSqlColumnName(string _params)
        {
            List<string> columnName = new List<string>();
            string sql = "select Column_name from information_schema.columns where table_schema ='mysql' and table_name ='" + _params + "'";// 
            try
            {
                //  cnn.Open();
                MySqlCommand com = new MySqlCommand(sql, PublicClass.conn);
                MySqlDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    // columnName.Add(dr[0].ToString());
                    columnName.Add(dr.GetString(0));
                }
                dr.Close();
                //cnn.Close();
            }
            catch (Exception ex)
            {
                WritetoListbox("列表异常");
                WriteLog(ex, "");
            }

            return columnName;
        }


        /*
        public void disp_message(HTuple hv_WindowHandle, HTuple hv_String, HTuple hv_CoordSystem,
               HTuple hv_Row, HTuple hv_Column, HTuple hv_Color, HTuple hv_Box)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_GenParamName = null, hv_GenParamValue = null;
            HTuple hv_Color_COPY_INP_TMP = hv_Color.Clone();
            HTuple hv_Column_COPY_INP_TMP = hv_Column.Clone();
            HTuple hv_CoordSystem_COPY_INP_TMP = hv_CoordSystem.Clone();
            HTuple hv_Row_COPY_INP_TMP = hv_Row.Clone();

            // Initialize local and output iconic variables 
            //This procedure displays text in a graphics window.
            //
            //Input parameters:
            //WindowHandle: The WindowHandle of the graphics window, where
            //   the message should be displayed
            //String: A tuple of strings containing the text message to be displayed
            //CoordSystem: If set to 'window', the text position is given
            //   with respect to the window coordinate system.
            //   If set to 'image', image coordinates are used.
            //   (This may be useful in zoomed images.)
            //Row: The row coordinate of the desired text position
            //   A tuple of values is allowed to display text at different
            //   positions.
            //Column: The column coordinate of the desired text position
            //   A tuple of values is allowed to display text at different
            //   positions.
            //Color: defines the color of the text as string.
            //   If set to [], '' or 'auto' the currently set color is used.
            //   If a tuple of strings is passed, the colors are used cyclically...
            //   - if |Row| == |Column| == 1: for each new textline
            //   = else for each text position.
            //Box: If Box[0] is set to 'true', the text is written within an orange box.
            //     If set to' false', no box is displayed.
            //     If set to a color string (e.g. 'white', '#FF00CC', etc.),
            //       the text is written in a box of that color.
            //     An optional second value for Box (Box[1]) controls if a shadow is displayed:
            //       'true' -> display a shadow in a default color
            //       'false' -> display no shadow
            //       otherwise -> use given string as color string for the shadow color
            //
            //It is possible to display multiple text strings in a single call.
            //In this case, some restrictions apply:
            //- Multiple text positions can be defined by specifying a tuple
            //  with multiple Row and/or Column coordinates, i.e.:
            //  - |Row| == n, |Column| == n
            //  - |Row| == n, |Column| == 1
            //  - |Row| == 1, |Column| == n
            //- If |Row| == |Column| == 1,
            //  each element of String is display in a new textline.
            //- If multiple positions or specified, the number of Strings
            //  must match the number of positions, i.e.:
            //  - Either |String| == n (each string is displayed at the
            //                          corresponding position),
            //  - or     |String| == 1 (The string is displayed n times).
            //
            //
            //Convert the parameters for disp_text.
            if ((int)((new HTuple(hv_Row_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleOr(
                new HTuple(hv_Column_COPY_INP_TMP.TupleEqual(new HTuple())))) != 0)
            {

                return;
            }
            if ((int)(new HTuple(hv_Row_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Row_COPY_INP_TMP = 12;
            }
            if ((int)(new HTuple(hv_Column_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Column_COPY_INP_TMP = 12;
            }
            //
            //Convert the parameter Box to generic parameters.
            hv_GenParamName = new HTuple();
            hv_GenParamValue = new HTuple();
            if ((int)(new HTuple((new HTuple(hv_Box.TupleLength())).TupleGreater(0))) != 0)
            {
                if ((int)(new HTuple(((hv_Box.TupleSelect(0))).TupleEqual("false"))) != 0)
                {
                    //Display no box
                    hv_GenParamName = hv_GenParamName.TupleConcat("box");
                    hv_GenParamValue = hv_GenParamValue.TupleConcat("false");
                }
                else if ((int)(new HTuple(((hv_Box.TupleSelect(0))).TupleNotEqual("true"))) != 0)
                {
                    //Set a color other than the default.
                    hv_GenParamName = hv_GenParamName.TupleConcat("box_color");
                    hv_GenParamValue = hv_GenParamValue.TupleConcat(hv_Box.TupleSelect(0));
                }
            }
            if ((int)(new HTuple((new HTuple(hv_Box.TupleLength())).TupleGreater(1))) != 0)
            {
                if ((int)(new HTuple(((hv_Box.TupleSelect(1))).TupleEqual("false"))) != 0)
                {
                    //Display no shadow.
                    hv_GenParamName = hv_GenParamName.TupleConcat("shadow");
                    hv_GenParamValue = hv_GenParamValue.TupleConcat("false");
                }
                else if ((int)(new HTuple(((hv_Box.TupleSelect(1))).TupleNotEqual("true"))) != 0)
                {
                    //Set a shadow color other than the default.
                    hv_GenParamName = hv_GenParamName.TupleConcat("shadow_color");
                    hv_GenParamValue = hv_GenParamValue.TupleConcat(hv_Box.TupleSelect(1));
                }
            }
            //Restore default CoordSystem behavior.
            if ((int)(new HTuple(hv_CoordSystem_COPY_INP_TMP.TupleNotEqual("window"))) != 0)
            {
                hv_CoordSystem_COPY_INP_TMP = "image";
            }
            //
            if ((int)(new HTuple(hv_Color_COPY_INP_TMP.TupleEqual(""))) != 0)
            {
                //disp_text does not accept an empty string for Color.
                hv_Color_COPY_INP_TMP = new HTuple();
            }
            //
            HOperatorSet.DispText(hv_ExpDefaultWinHandle, hv_String, hv_CoordSystem_COPY_INP_TMP,
                hv_Row_COPY_INP_TMP, hv_Column_COPY_INP_TMP, hv_Color_COPY_INP_TMP, hv_GenParamName,
                hv_GenParamValue);

            return;
        }
        */
        // Chapter: Graphics / Text
        // Short Description: Set font independent of OS 


        public void disp_message(HTuple hv_WindowHandle, HTuple hv_String, HTuple hv_CoordSystem,
      HTuple hv_Row, HTuple hv_Column, HTuple hv_Color, HTuple hv_Box)
        {


            // Local control variables 

            HTuple hv_Red, hv_Green, hv_Blue, hv_Row1Part;
            HTuple hv_Column1Part, hv_Row2Part, hv_Column2Part, hv_RowWin;
            HTuple hv_ColumnWin, hv_WidthWin = new HTuple(), hv_HeightWin;
            HTuple hv_MaxAscent, hv_MaxDescent, hv_MaxWidth, hv_MaxHeight;
            HTuple hv_R1 = new HTuple(), hv_C1 = new HTuple(), hv_FactorRow = new HTuple();
            HTuple hv_FactorColumn = new HTuple(), hv_Width = new HTuple();
            HTuple hv_Index = new HTuple(), hv_Ascent = new HTuple(), hv_Descent = new HTuple();
            HTuple hv_W = new HTuple(), hv_H = new HTuple(), hv_FrameHeight = new HTuple();
            HTuple hv_FrameWidth = new HTuple(), hv_R2 = new HTuple();
            HTuple hv_C2 = new HTuple(), hv_DrawMode = new HTuple(), hv_Exception = new HTuple();
            HTuple hv_CurrentColor = new HTuple();

            HTuple hv_Color_COPY_INP_TMP = hv_Color.Clone();
            HTuple hv_Column_COPY_INP_TMP = hv_Column.Clone();
            HTuple hv_Row_COPY_INP_TMP = hv_Row.Clone();
            HTuple hv_String_COPY_INP_TMP = hv_String.Clone();

            // Initialize local and output iconic variables 

            //This procedure displays text in a graphics window.
            //
            //Input parameters:
            //WindowHandle: The WindowHandle of the graphics window, where
            //   the message should be displayed
            //String: A tuple of strings containing the text message to be displayed
            //CoordSystem: If set to 'window', the text position is given
            //   with respect to the window coordinate system.
            //   If set to 'image', image coordinates are used.
            //   (This may be useful in zoomed images.)
            //Row: The row coordinate of the desired text position
            //   If set to -1, a default value of 12 is used.
            //Column: The column coordinate of the desired text position
            //   If set to -1, a default value of 12 is used.
            //Color: defines the color of the text as string.
            //   If set to [], '' or 'auto' the currently set color is used.
            //   If a tuple of strings is passed, the colors are used cyclically
            //   for each new textline.
            //Box: If set to 'true', the text is written within a white box.
            //
            //prepare window
            hv_ExpDefaultWinHandle = hv_WindowHandle;
            HOperatorSet.GetRgb(hv_ExpDefaultWinHandle, out hv_Red, out hv_Green, out hv_Blue);
            HOperatorSet.GetPart(hv_ExpDefaultWinHandle, out hv_Row1Part, out hv_Column1Part,
                out hv_Row2Part, out hv_Column2Part);
            HOperatorSet.GetWindowExtents(hv_ExpDefaultWinHandle, out hv_RowWin, out hv_ColumnWin,
                out hv_WidthWin, out hv_HeightWin);
            HOperatorSet.SetPart(hv_ExpDefaultWinHandle, 0, 0, hv_HeightWin - 1, hv_WidthWin - 1);
            //
            //default settings
            if ((int)(new HTuple(hv_Row_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Row_COPY_INP_TMP = 12;
            }
            if ((int)(new HTuple(hv_Column_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Column_COPY_INP_TMP = 12;
            }
            if ((int)(new HTuple(hv_Color_COPY_INP_TMP.TupleEqual(new HTuple()))) != 0)
            {
                hv_Color_COPY_INP_TMP = "";
            }
            //
            hv_String_COPY_INP_TMP = ((("" + hv_String_COPY_INP_TMP) + "")).TupleSplit("\n");
            //
            //Estimate extentions of text depending on font size.
            HOperatorSet.GetFontExtents(hv_ExpDefaultWinHandle, out hv_MaxAscent, out hv_MaxDescent,
                out hv_MaxWidth, out hv_MaxHeight);
            if ((int)(new HTuple(hv_CoordSystem.TupleEqual("window"))) != 0)
            {
                hv_R1 = hv_Row_COPY_INP_TMP.Clone();
                hv_C1 = hv_Column_COPY_INP_TMP.Clone();
            }
            else
            {
                //transform image to window coordinates
                hv_FactorRow = (1.0 * hv_HeightWin) / ((hv_Row2Part - hv_Row1Part) + 1);
                hv_FactorColumn = (1.0 * hv_WidthWin) / ((hv_Column2Part - hv_Column1Part) + 1);
                hv_R1 = ((hv_Row_COPY_INP_TMP - hv_Row1Part) + 0.5) * hv_FactorRow;
                hv_C1 = ((hv_Column_COPY_INP_TMP - hv_Column1Part) + 0.5) * hv_FactorColumn;
            }
            //
            //display text box depending on text size
            if ((int)(new HTuple(hv_Box.TupleEqual("true"))) != 0)
            {
                //calculate box extents
                hv_String_COPY_INP_TMP = (" " + hv_String_COPY_INP_TMP) + " ";
                hv_Width = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    HOperatorSet.GetStringExtents(hv_ExpDefaultWinHandle, hv_String_COPY_INP_TMP.TupleSelect(
                        hv_Index), out hv_Ascent, out hv_Descent, out hv_W, out hv_H);
                    hv_Width = hv_Width.TupleConcat(hv_W);
                }
                hv_FrameHeight = hv_MaxHeight * (new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                    ));
                hv_FrameWidth = (((new HTuple(0)).TupleConcat(hv_Width))).TupleMax();
                hv_R2 = hv_R1 + hv_FrameHeight;
                hv_C2 = hv_C1 + hv_FrameWidth;
                //display rectangles
                HOperatorSet.GetDraw(hv_ExpDefaultWinHandle, out hv_DrawMode);
                HOperatorSet.SetDraw(hv_ExpDefaultWinHandle, "fill");
                HOperatorSet.SetColor(hv_ExpDefaultWinHandle, "light gray");
                HOperatorSet.DispRectangle1(hv_ExpDefaultWinHandle, hv_R1 + 3, hv_C1 + 3, hv_R2 + 3,
                    hv_C2 + 3);
                HOperatorSet.SetColor(hv_ExpDefaultWinHandle, "white");
                HOperatorSet.DispRectangle1(hv_ExpDefaultWinHandle, hv_R1, hv_C1, hv_R2, hv_C2);
                HOperatorSet.SetDraw(hv_ExpDefaultWinHandle, hv_DrawMode);
            }
            else if ((int)(new HTuple(hv_Box.TupleNotEqual("false"))) != 0)
            {
                hv_Exception = "Wrong value of control parameter Box";
                throw new HalconException(hv_Exception);
            }
            //Write text.
            for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                )) - 1); hv_Index = (int)hv_Index + 1)
            {
                hv_CurrentColor = hv_Color_COPY_INP_TMP.TupleSelect(hv_Index % (new HTuple(hv_Color_COPY_INP_TMP.TupleLength()
                    )));
                if ((int)((new HTuple(hv_CurrentColor.TupleNotEqual(""))).TupleAnd(new HTuple(hv_CurrentColor.TupleNotEqual(
                    "auto")))) != 0)
                {
                    HOperatorSet.SetColor(hv_ExpDefaultWinHandle, hv_CurrentColor);
                }
                else
                {
                    HOperatorSet.SetRgb(hv_ExpDefaultWinHandle, hv_Red, hv_Green, hv_Blue);
                }
                hv_Row_COPY_INP_TMP = hv_R1 + (hv_MaxHeight * hv_Index);
                HOperatorSet.SetTposition(hv_ExpDefaultWinHandle, hv_Row_COPY_INP_TMP, hv_C1);
                HOperatorSet.WriteString(hv_ExpDefaultWinHandle, hv_String_COPY_INP_TMP.TupleSelect(
                    hv_Index));
            }
            //reset changed window settings
            HOperatorSet.SetRgb(hv_ExpDefaultWinHandle, hv_Red, hv_Green, hv_Blue);
            HOperatorSet.SetPart(hv_ExpDefaultWinHandle, hv_Row1Part, hv_Column1Part, hv_Row2Part,
                hv_Column2Part);

            return;
        }
        public void set_display_font(HTuple hv_WindowHandle, HTuple hv_Size, HTuple hv_Font,
            HTuple hv_Bold, HTuple hv_Slant)
        {
            return;

            // Local iconic variables 

            // Local control variables 

            HTuple hv_OS = null, hv_Fonts = new HTuple();
            HTuple hv_Style = null, hv_Exception = new HTuple(), hv_AvailableFonts = null;
            HTuple hv_Fdx = null, hv_Indices = new HTuple();
            HTuple hv_Font_COPY_INP_TMP = hv_Font.Clone();
            HTuple hv_Size_COPY_INP_TMP = hv_Size.Clone();

            // Initialize local and output iconic variables 
            //This procedure sets the text font of the current window with
            //the specified attributes.
            //
            //Input parameters:
            //WindowHandle: The graphics window for which the font will be set
            //Size: The font size. If Size=-1, the default of 16 is used.
            //Bold: If set to 'true', a bold font is used
            //Slant: If set to 'true', a slanted font is used
            //
            HOperatorSet.GetSystem("operating_system", out hv_OS);
            if ((int)((new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleOr(
                new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(-1)))) != 0)
            {
                hv_Size_COPY_INP_TMP = 16;
            }
            if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Win"))) != 0)
            {
                //Restore previous behaviour
                hv_Size_COPY_INP_TMP = ((1.13677 * hv_Size_COPY_INP_TMP)).TupleInt();
            }
            else
            {
                hv_Size_COPY_INP_TMP = hv_Size_COPY_INP_TMP.TupleInt();
            }
            if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("Courier"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Courier";
                hv_Fonts[1] = "Courier 10 Pitch";
                hv_Fonts[2] = "Courier New";
                hv_Fonts[3] = "CourierNew";
                hv_Fonts[4] = "Liberation Mono";
            }
            else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("mono"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Consolas";
                hv_Fonts[1] = "Menlo";
                hv_Fonts[2] = "Courier";
                hv_Fonts[3] = "Courier 10 Pitch";
                hv_Fonts[4] = "FreeMono";
                hv_Fonts[5] = "Liberation Mono";
            }
            else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("sans"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Luxi Sans";
                hv_Fonts[1] = "DejaVu Sans";
                hv_Fonts[2] = "FreeSans";
                hv_Fonts[3] = "Arial";
                hv_Fonts[4] = "Liberation Sans";
            }
            else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("serif"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Times New Roman";
                hv_Fonts[1] = "Luxi Serif";
                hv_Fonts[2] = "DejaVu Serif";
                hv_Fonts[3] = "FreeSerif";
                hv_Fonts[4] = "Utopia";
                hv_Fonts[5] = "Liberation Serif";
            }
            else
            {
                hv_Fonts = hv_Font_COPY_INP_TMP.Clone();
            }
            hv_Style = "";
            if ((int)(new HTuple(hv_Bold.TupleEqual("true"))) != 0)
            {
                hv_Style = hv_Style + "Bold";
            }
            else if ((int)(new HTuple(hv_Bold.TupleNotEqual("false"))) != 0)
            {
                hv_Exception = "Wrong value of control parameter Bold";
                throw new HalconException(hv_Exception);
            }
            if ((int)(new HTuple(hv_Slant.TupleEqual("true"))) != 0)
            {
                hv_Style = hv_Style + "Italic";
            }
            else if ((int)(new HTuple(hv_Slant.TupleNotEqual("false"))) != 0)
            {
                hv_Exception = "Wrong value of control parameter Slant";
                throw new HalconException(hv_Exception);
            }
            if ((int)(new HTuple(hv_Style.TupleEqual(""))) != 0)
            {
                hv_Style = "Normal";
            }
            HOperatorSet.QueryFont(hv_ExpDefaultWinHandle, out hv_AvailableFonts);
            hv_Font_COPY_INP_TMP = "";
            for (hv_Fdx = 0; (int)hv_Fdx <= (int)((new HTuple(hv_Fonts.TupleLength())) - 1); hv_Fdx = (int)hv_Fdx + 1)
            {
                hv_Indices = hv_AvailableFonts.TupleFind(hv_Fonts.TupleSelect(hv_Fdx));
                if ((int)(new HTuple((new HTuple(hv_Indices.TupleLength())).TupleGreater(0))) != 0)
                {
                    if ((int)(new HTuple(((hv_Indices.TupleSelect(0))).TupleGreaterEqual(0))) != 0)
                    {
                        hv_Font_COPY_INP_TMP = hv_Fonts.TupleSelect(hv_Fdx);
                        break;
                    }
                }
            }
            if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual(""))) != 0)
            {
                throw new HalconException("Wrong value of control parameter Font");
            }
            hv_Font_COPY_INP_TMP = (((hv_Font_COPY_INP_TMP + "-") + hv_Style) + "-") + hv_Size_COPY_INP_TMP;
            HOperatorSet.SetFont(hv_ExpDefaultWinHandle, hv_Font_COPY_INP_TMP);

            return;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {


            // string path= System.Configuration.ConfigurationSettings.AppSettings["Setting"];

          //  ConfigurationManager.OpenExeConfiguration()
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming);
          //  ConfigurationSectionGroup
            //根据Key读取<add>元素的Value
            string name = config.AppSettings.SectionInformation.SectionName;
            /// config.AppSettings.Settings["IniPath"].Value;

            //写入<add>元素的Value
            config.AppSettings.Settings["name"].Value = "fx163";
            //增加<add>元素
            config.AppSettings.Settings.Add("url", "http://www.fx163.net");
            //删除<add>元素
            config.AppSettings.Settings.Remove("name");
            //一定要记得保存，写不带参数的config.Save()也可以
            config.Save(ConfigurationSaveMode.Modified);
            //刷新，否则程序读取的还是之前的值（可能已装入内存）
            System.Configuration.ConfigurationManager.RefreshSection("appSettings");
        }

      

        // Procedures 
        // Chapter: Graphics / Text
        // Short Description: This procedure writes a text message. 
        public void disp_message1(HTuple hv_WindowHandle, HTuple hv_String, HTuple hv_CoordSystem,
            HTuple hv_Row, HTuple hv_Column, HTuple hv_Color, HTuple hv_Box)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_GenParamName = null, hv_GenParamValue = null;
            HTuple hv_Color_COPY_INP_TMP = hv_Color.Clone();
            HTuple hv_Column_COPY_INP_TMP = hv_Column.Clone();
            HTuple hv_CoordSystem_COPY_INP_TMP = hv_CoordSystem.Clone();
            HTuple hv_Row_COPY_INP_TMP = hv_Row.Clone();

            // Initialize local and output iconic variables 
            //This procedure displays text in a graphics window.
            //
            //Input parameters:
            //WindowHandle: The WindowHandle of the graphics window, where
            //   the message should be displayed
            //String: A tuple of strings containing the text message to be displayed
            //CoordSystem: If set to 'window', the text position is given
            //   with respect to the window coordinate system.
            //   If set to 'image', image coordinates are used.
            //   (This may be useful in zoomed images.)
            //Row: The row coordinate of the desired text position
            //   A tuple of values is allowed to display text at different
            //   positions.
            //Column: The column coordinate of the desired text position
            //   A tuple of values is allowed to display text at different
            //   positions.
            //Color: defines the color of the text as string.
            //   If set to [], '' or 'auto' the currently set color is used.
            //   If a tuple of strings is passed, the colors are used cyclically...
            //   - if |Row| == |Column| == 1: for each new textline
            //   = else for each text position.
            //Box: If Box[0] is set to 'true', the text is written within an orange box.
            //     If set to' false', no box is displayed.
            //     If set to a color string (e.g. 'white', '#FF00CC', etc.),
            //       the text is written in a box of that color.
            //     An optional second value for Box (Box[1]) controls if a shadow is displayed:
            //       'true' -> display a shadow in a default color
            //       'false' -> display no shadow
            //       otherwise -> use given string as color string for the shadow color
            //
            //It is possible to display multiple text strings in a single call.
            //In this case, some restrictions apply:
            //- Multiple text positions can be defined by specifying a tuple
            //  with multiple Row and/or Column coordinates, i.e.:
            //  - |Row| == n, |Column| == n
            //  - |Row| == n, |Column| == 1
            //  - |Row| == 1, |Column| == n
            //- If |Row| == |Column| == 1,
            //  each element of String is display in a new textline.
            //- If multiple positions or specified, the number of Strings
            //  must match the number of positions, i.e.:
            //  - Either |String| == n (each string is displayed at the
            //                          corresponding position),
            //  - or     |String| == 1 (The string is displayed n times).
            //
            //
            //Convert the parameters for disp_text.
            if ((int)((new HTuple(hv_Row_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleOr(
                new HTuple(hv_Column_COPY_INP_TMP.TupleEqual(new HTuple())))) != 0)
            {

                return;
            }
            if ((int)(new HTuple(hv_Row_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Row_COPY_INP_TMP = 12;
            }
            if ((int)(new HTuple(hv_Column_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Column_COPY_INP_TMP = 12;
            }
            //
            //Convert the parameter Box to generic parameters.
            hv_GenParamName = new HTuple();
            hv_GenParamValue = new HTuple();
            if ((int)(new HTuple((new HTuple(hv_Box.TupleLength())).TupleGreater(0))) != 0)
            {
                if ((int)(new HTuple(((hv_Box.TupleSelect(0))).TupleEqual("false"))) != 0)
                {
                    //Display no box
                    hv_GenParamName = hv_GenParamName.TupleConcat("box");
                    hv_GenParamValue = hv_GenParamValue.TupleConcat("false");
                }
                else if ((int)(new HTuple(((hv_Box.TupleSelect(0))).TupleNotEqual("true"))) != 0)
                {
                    //Set a color other than the default.
                    hv_GenParamName = hv_GenParamName.TupleConcat("box_color");
                    hv_GenParamValue = hv_GenParamValue.TupleConcat(hv_Box.TupleSelect(0));
                }
            }
            if ((int)(new HTuple((new HTuple(hv_Box.TupleLength())).TupleGreater(1))) != 0)
            {
                if ((int)(new HTuple(((hv_Box.TupleSelect(1))).TupleEqual("false"))) != 0)
                {
                    //Display no shadow.
                    hv_GenParamName = hv_GenParamName.TupleConcat("shadow");
                    hv_GenParamValue = hv_GenParamValue.TupleConcat("false");
                }
                else if ((int)(new HTuple(((hv_Box.TupleSelect(1))).TupleNotEqual("true"))) != 0)
                {
                    //Set a shadow color other than the default.
                    hv_GenParamName = hv_GenParamName.TupleConcat("shadow_color");
                    hv_GenParamValue = hv_GenParamValue.TupleConcat(hv_Box.TupleSelect(1));
                }
            }
            //Restore default CoordSystem behavior.
            if ((int)(new HTuple(hv_CoordSystem_COPY_INP_TMP.TupleNotEqual("window"))) != 0)
            {
                hv_CoordSystem_COPY_INP_TMP = "image";
            }
            //
            if ((int)(new HTuple(hv_Color_COPY_INP_TMP.TupleEqual(""))) != 0)
            {
                //disp_text does not accept an empty string for Color.
                hv_Color_COPY_INP_TMP = new HTuple();
            }
            //
            HOperatorSet.DispText(hv_ExpDefaultWinHandle, hv_String, hv_CoordSystem_COPY_INP_TMP,
                hv_Row_COPY_INP_TMP, hv_Column_COPY_INP_TMP, hv_Color_COPY_INP_TMP, hv_GenParamName,
                hv_GenParamValue);

            return;
        }

        // Chapter: Graphics / Text
        // Short Description: Set font independent of OS 
        public void set_display_font1(HTuple hv_WindowHandle, HTuple hv_Size, HTuple hv_Font,
            HTuple hv_Bold, HTuple hv_Slant)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_OS = null, hv_Fonts = new HTuple();
            HTuple hv_Style = null, hv_Exception = new HTuple(), hv_AvailableFonts = null;
            HTuple hv_Fdx = null, hv_Indices = new HTuple();
            HTuple hv_Font_COPY_INP_TMP = hv_Font.Clone();
            HTuple hv_Size_COPY_INP_TMP = hv_Size.Clone();

            // Initialize local and output iconic variables 
            //This procedure sets the text font of the current window with
            //the specified attributes.
            //
            //Input parameters:
            //WindowHandle: The graphics window for which the font will be set
            //Size: The font size. If Size=-1, the default of 16 is used.
            //Bold: If set to 'true', a bold font is used
            //Slant: If set to 'true', a slanted font is used
            //
            HOperatorSet.GetSystem("operating_system", out hv_OS);
            if ((int)((new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleOr(
                new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(-1)))) != 0)
            {
                hv_Size_COPY_INP_TMP = 16;
            }
            if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Win"))) != 0)
            {
                //Restore previous behaviour
                hv_Size_COPY_INP_TMP = ((1.13677 * hv_Size_COPY_INP_TMP)).TupleInt();
            }
            else
            {
                hv_Size_COPY_INP_TMP = hv_Size_COPY_INP_TMP.TupleInt();
            }
            if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("Courier"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Courier";
                hv_Fonts[1] = "Courier 10 Pitch";
                hv_Fonts[2] = "Courier New";
                hv_Fonts[3] = "CourierNew";
                hv_Fonts[4] = "Liberation Mono";
            }
            else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("mono"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Consolas";
                hv_Fonts[1] = "Menlo";
                hv_Fonts[2] = "Courier";
                hv_Fonts[3] = "Courier 10 Pitch";
                hv_Fonts[4] = "FreeMono";
                hv_Fonts[5] = "Liberation Mono";
            }
            else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("sans"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Luxi Sans";
                hv_Fonts[1] = "DejaVu Sans";
                hv_Fonts[2] = "FreeSans";
                hv_Fonts[3] = "Arial";
                hv_Fonts[4] = "Liberation Sans";
            }
            else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("serif"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Times New Roman";
                hv_Fonts[1] = "Luxi Serif";
                hv_Fonts[2] = "DejaVu Serif";
                hv_Fonts[3] = "FreeSerif";
                hv_Fonts[4] = "Utopia";
                hv_Fonts[5] = "Liberation Serif";
            }
            else
            {
                hv_Fonts = hv_Font_COPY_INP_TMP.Clone();
            }
            hv_Style = "";
            if ((int)(new HTuple(hv_Bold.TupleEqual("true"))) != 0)
            {
                hv_Style = hv_Style + "Bold";
            }
            else if ((int)(new HTuple(hv_Bold.TupleNotEqual("false"))) != 0)
            {
                hv_Exception = "Wrong value of control parameter Bold";
                throw new HalconException(hv_Exception);
            }
            if ((int)(new HTuple(hv_Slant.TupleEqual("true"))) != 0)
            {
                hv_Style = hv_Style + "Italic";
            }
            else if ((int)(new HTuple(hv_Slant.TupleNotEqual("false"))) != 0)
            {
                hv_Exception = "Wrong value of control parameter Slant";
                throw new HalconException(hv_Exception);
            }
            if ((int)(new HTuple(hv_Style.TupleEqual(""))) != 0)
            {
                hv_Style = "Normal";
            }
            HOperatorSet.QueryFont(hv_ExpDefaultWinHandle, out hv_AvailableFonts);
            hv_Font_COPY_INP_TMP = "";
            for (hv_Fdx = 0; (int)hv_Fdx <= (int)((new HTuple(hv_Fonts.TupleLength())) - 1); hv_Fdx = (int)hv_Fdx + 1)
            {
                hv_Indices = hv_AvailableFonts.TupleFind(hv_Fonts.TupleSelect(hv_Fdx));
                if ((int)(new HTuple((new HTuple(hv_Indices.TupleLength())).TupleGreater(0))) != 0)
                {
                    if ((int)(new HTuple(((hv_Indices.TupleSelect(0))).TupleGreaterEqual(0))) != 0)
                    {
                        hv_Font_COPY_INP_TMP = hv_Fonts.TupleSelect(hv_Fdx);
                        break;
                    }
                }
            }
            if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual(""))) != 0)
            {
                throw new HalconException("Wrong value of control parameter Font");
            }
            hv_Font_COPY_INP_TMP = (((hv_Font_COPY_INP_TMP + "-") + hv_Style) + "-") + hv_Size_COPY_INP_TMP;
            HOperatorSet.SetFont(hv_ExpDefaultWinHandle, hv_Font_COPY_INP_TMP);

            return;
        }

        // Local procedures 
        public void get_defect_aberration(HObject ho_Image, out HObject ho_ImageWithDefect,
            HTuple hv_windowHandle, HTuple hv_standardTupleL, HTuple hv_standardTupleA,
            HTuple hv_standardTupleB, HTuple hv_isSeperateComputer, HTuple hv_algorithmOfAberration,
            out HTuple hv_clothAberration, out HTuple hv_leftRightAberration, out HTuple hv_L,
            out HTuple hv_A, out HTuple hv_B, out HTuple hv_result, out HTuple hv_tupleDetectResult,
            out HTuple hv_defectNumber, out HTuple hv_tupleDefectClass, out HTuple hv_tupleDefectX,
            out HTuple hv_tupleDefectY, out HTuple hv_tupleDefectRow1, out HTuple hv_tupleDefectRow2,
            out HTuple hv_tupleDefectColumn1, out HTuple hv_tupleDefectColumn2, out HTuple hv_minWidth,
            out HTuple hv_maxWidth, out HTuple hv_meanWidth, out HTuple hv_metersCounter,
            out HTuple hv_tupleMessages, out HTuple hv_tupleMessagesColor, out HTuple hv_leftDetectSide,
            out HTuple hv_rightDetectSide, out HTuple hv_clothRegionCoordinateX1, out HTuple hv_clothRegionCoordinateX2)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_DefectRegion = null, ho_Boxs = null;
            HObject ho_Rectangle4 = null, ho_GrayImage = null, ho_RectangleL = null;
            HObject ho_RectangleR = null, ho_Rectangle = null, ho_ImageR0Left = null;
            HObject ho_ImageG0Left = null, ho_ImageB0Left = null, ho_ImageLLeft = null;
            HObject ho_ImageALeft = null, ho_ImageBLeft = null, ho_ImageR0Right = null;
            HObject ho_ImageG0Right = null, ho_ImageB0Right = null, ho_ImageLRight1 = null;
            HObject ho_ImageARigh = null, ho_ImageBRight = null, ho_ClothRegionLeft = null;
            HObject ho_ClothRegionRight = null, ho_ClothRegion = null, ho_ValidClothRegion = null;
            HObject ho_RegionOpening2 = null;

            // Local copy input parameter variables 
            HObject ho_Image_COPY_INP_TMP;
            ho_Image_COPY_INP_TMP = ho_Image.CopyObj(1, -1);



            // Local control variables 

            HTuple hv_magnification = null, hv_leftSide = null;
            HTuple hv_rightSide = null, hv_boxNumber = null, hv_boxWidth = null;
            HTuple hv_boxHeight = null, hv_boxBenginX = null, hv_dynThresh = null;
            HTuple hv_medianKernal = null, hv_thresh = null, hv_defectArea = null;
            HTuple hv_defectWidth = null, hv_defectHeight = null, hv_edgeRollSlope = null;
            HTuple hv_imperfectBorderWidth = null, hv_clothAberrationGrad1 = null;
            HTuple hv_clothAberrationGrad2 = null, hv_clothAberrationGrad3 = null;
            HTuple hv_clothAberrationGrad4 = null, hv_clothSideUnDetectWidth = null;
            HTuple hv_defectLoactionNumber = null, hv_tupleDefectRadius = null;
            HTuple hv_detectDefectsFlag = null, hv_edgeRollFlag = null;
            HTuple hv_imperfectBorderFlag = null, hv_otherFlag = null;
            HTuple hv_second1 = null, hv_message = new HTuple(), hv_color = new HTuple();
            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_MeanL = new HTuple(), hv_DeviationL = new HTuple();
            HTuple hv_MeanR = new HTuple(), hv_DeviationR = new HTuple();
            HTuple hv_Mean = new HTuple(), hv_Deviation = new HTuple();
            HTuple hv_AreaLeft = new HTuple(), hv_Row = new HTuple();
            HTuple hv_Column = new HTuple(), hv_AreaRight = new HTuple();
            HTuple hv_Row1 = new HTuple(), hv_Row2 = new HTuple();
            HTuple hv_Row15 = new HTuple(), hv_Row25 = new HTuple();
            HTuple hv_colorDetect = new HTuple(), hv_tupleL = new HTuple();
            HTuple hv_tupleA = new HTuple(), hv_tupleB = new HTuple();
            HTuple hv_Length = new HTuple(), hv_Exception1 = new HTuple();
            HTuple hv_j1 = new HTuple(), hv_j2 = new HTuple(), hv_theAberration = new HTuple();
            HTuple hv_aberrations = new HTuple(), hv_j = new HTuple();
            HTuple hv_aberrations1 = new HTuple(), hv_tupleL1 = new HTuple();
            HTuple hv_tupleA1 = new HTuple(), hv_tupleB1 = new HTuple();
            HTuple hv_total = new HTuple(), hv_totalL = new HTuple();
            HTuple hv_totalA = new HTuple(), hv_totalB = new HTuple();
            HTuple hv_k = new HTuple(), hv_zhezhouDetectflag = new HTuple();
            HTuple hv_tupleDefectClass1 = new HTuple(), hv_Exception = null;
            HTuple hv_second2 = null;
            HTuple hv_algorithmOfAberration_COPY_INP_TMP = hv_algorithmOfAberration.Clone();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageWithDefect);
            HOperatorSet.GenEmptyObj(out ho_DefectRegion);
            HOperatorSet.GenEmptyObj(out ho_Boxs);
            HOperatorSet.GenEmptyObj(out ho_Rectangle4);
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_RectangleL);
            HOperatorSet.GenEmptyObj(out ho_RectangleR);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_ImageR0Left);
            HOperatorSet.GenEmptyObj(out ho_ImageG0Left);
            HOperatorSet.GenEmptyObj(out ho_ImageB0Left);
            HOperatorSet.GenEmptyObj(out ho_ImageLLeft);
            HOperatorSet.GenEmptyObj(out ho_ImageALeft);
            HOperatorSet.GenEmptyObj(out ho_ImageBLeft);
            HOperatorSet.GenEmptyObj(out ho_ImageR0Right);
            HOperatorSet.GenEmptyObj(out ho_ImageG0Right);
            HOperatorSet.GenEmptyObj(out ho_ImageB0Right);
            HOperatorSet.GenEmptyObj(out ho_ImageLRight1);
            HOperatorSet.GenEmptyObj(out ho_ImageARigh);
            HOperatorSet.GenEmptyObj(out ho_ImageBRight);
            HOperatorSet.GenEmptyObj(out ho_ClothRegionLeft);
            HOperatorSet.GenEmptyObj(out ho_ClothRegionRight);
            HOperatorSet.GenEmptyObj(out ho_ClothRegion);
            HOperatorSet.GenEmptyObj(out ho_ValidClothRegion);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening2);
            hv_leftRightAberration = new HTuple();
            try
            {
                //*********************输入参数
                //magnification放大率piexels/mm
                //LeftSide左边有效参数
                //RightSide右边有效参数
                //boxNumber框个数
                //boxWidth框宽度
                //boxHeight框高度
                //boxBenginX框起始X坐标
                //dynThresh缺陷阈值
                //medianKernal滤波卷积核大小
                //defectArea缺陷面积
                //edgeRollSlope判断卷边的斜率偏差
                //imperfectBorderWidth判断缺边的宽度
                //leftSide左边有效区域
                //rightSide右边有效区域
                //clothAberrationGrad1-clothAberrationGrad4色差等级分类
                //布匹边缘不检测宽度clothSideUnDetectWidth
                //*****************************
                hv_magnification = 4.8188;
                hv_leftSide = 41.5040;
                hv_rightSide = 41.5040;
                hv_boxNumber = 6;
                hv_boxWidth = 83;
                hv_boxHeight = 83;
                hv_boxBenginX = 200;
                hv_dynThresh = 15;
                hv_medianKernal = 20;
                hv_thresh = 30;
                hv_defectArea = 0.2157;
                hv_defectWidth = 1.0393;
                hv_defectHeight = 1.0393;
                hv_edgeRollSlope = 0.1;
                hv_imperfectBorderWidth = 4.15;
                hv_clothAberrationGrad1 = 0.5;
                hv_clothAberrationGrad2 = 3;
                hv_clothAberrationGrad3 = 6.0;
                hv_clothAberrationGrad4 =12;


               // GetAberrationGrad()

                hv_clothSideUnDetectWidth = 20.7;


                hv_metersCounter = 1;
                hv_leftSide = hv_leftSide * hv_magnification;
                hv_rightSide = hv_rightSide * hv_magnification;
                hv_boxWidth = hv_boxWidth * hv_magnification;
                hv_boxHeight = hv_boxHeight * hv_magnification;
                hv_boxBenginX = hv_boxBenginX * hv_magnification;
                hv_defectArea = (hv_defectArea * hv_magnification) * hv_magnification;
                hv_defectWidth = hv_defectWidth * hv_magnification;
                hv_defectHeight = hv_defectHeight * hv_magnification;
                hv_imperfectBorderWidth = hv_imperfectBorderWidth * hv_magnification;
                hv_clothSideUnDetectWidth = hv_clothSideUnDetectWidth * hv_magnification;
                hv_result = 0;




                //*********************输入参数*******************
                hv_magnification = PublicClass.mag;
                hv_leftSide = PublicClass.leftside * PublicClass.mag;
                hv_rightSide = PublicClass.rightside * hv_magnification;
                hv_boxWidth = PublicClass.Square * hv_magnification;
                hv_boxHeight = PublicClass.Square * hv_magnification;
                hv_boxBenginX = PublicClass.OriginX * hv_magnification;
                hv_defectArea = (PublicClass.defectArea * hv_magnification) * hv_magnification;
                hv_defectWidth = PublicClass.defectWidth * hv_magnification;
                hv_defectHeight = PublicClass.defectHeight * hv_magnification;
                hv_imperfectBorderWidth = PublicClass.imperfectBorderWidth * hv_magnification;
                hv_clothSideUnDetectWidth = PublicClass.clothSideUnDetectWidth * hv_magnification;
                hv_edgeRollSlope = PublicClass.EdgeRollslope;
                hv_algorithmOfAberration = PublicClass.algorithmOfAberration;
                hv_isSeperateComputer = PublicClass.isSeperateComputer;
                hv_ExpDefaultWinHandle = hv_windowHandle;
                hv_metersCounter = MetersCounter;
                //*********************输入参数*********************




                //defectLoactionNumber缺陷位置编号
                hv_defectLoactionNumber = 1;
                //***********************************
                //tupleDetectResult表示各个缺陷的个数,>0为有缺陷
                //tupleDetectResult[0]表示接缝个数，严重，亮红灯
                //tupleDetectResult[1]表示周期性缺陷个数，严重，亮红灯
                //tupleDetectResult[2]表示卷边，严重，亮红灯
                //tupleDetectResult[3]表示缺边个数，黄灯
                //tupleDetectResult[4]表示点瑕疵个数，黄灯
                //***********************************
                hv_tupleDetectResult = new HTuple();
                hv_tupleDetectResult[0] = 0;
                hv_tupleDetectResult[1] = 0;
                hv_tupleDetectResult[2] = 0;
                hv_tupleDetectResult[3] = 0;
                hv_tupleDetectResult[4] = 0;
                hv_tupleDetectResult[5] = 0;
                hv_tupleDetectResult[6] = 0;
                hv_tupleDetectResult[7] = 0;
                hv_tupleDetectResult[8] = 0;
                hv_tupleDetectResult[9] = 0;
                //瑕疵半径
                hv_tupleDefectRadius = new HTuple();
                //瑕疵X坐标
                hv_tupleDefectX = new HTuple();
                //瑕疵Y坐标
                hv_tupleDefectY = new HTuple();
                //tupleDefectClass表示瑕疵分类,0表示接，1表示周期性缺陷，2表示卷边，3表示缺边，4表示其他瑕疵
                hv_tupleDefectClass = new HTuple();

                hv_L = 0;
                hv_A = 0;
                hv_B = 0;
                //clothberration表示色差值
                hv_clothAberration = 0;
                hv_minWidth = 0;
                hv_maxWidth = 0;
                hv_meanWidth = 0;

                //leftDetectSide、rightDetectSide
                hv_leftDetectSide = 0;
                hv_rightDetectSide = 0;
                //输出结果
                hv_tupleMessages = new HTuple();
                hv_tupleMessagesColor = new HTuple();
                //检测缺陷的个数
                hv_defectNumber = 0;
                //缺陷框坐标
                hv_tupleDefectRow1 = new HTuple();
                hv_tupleDefectRow2 = new HTuple();
                hv_tupleDefectColumn1 = new HTuple();
                hv_tupleDefectColumn2 = new HTuple();

                //缺陷检测标志位
                hv_detectDefectsFlag = 1;
                //卷边检测标志位
                hv_edgeRollFlag = 1;
                //缺边检测
                hv_imperfectBorderFlag = 1;
                //其他缺陷检测
                hv_otherFlag = 1;

                hv_clothRegionCoordinateX1 = 0;
                hv_clothRegionCoordinateX2 = 0;

                HOperatorSet.CountSeconds(out hv_second1);
                try
                {
                    hv_message = "图片编号：" + hv_metersCounter;
                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    hv_color = "green";
                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);

                    ho_DefectRegion.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_DefectRegion);
                    ho_Boxs.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_Boxs);
                    HOperatorSet.GetImageSize(ho_Image_COPY_INP_TMP, out hv_Width, out hv_Height);
                    //裁减无效区域
                    ho_Rectangle4.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle4, -10, 148, hv_Height + 10, 8059);
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ReduceDomain(ho_Image_COPY_INP_TMP, ho_Rectangle4, out ExpTmpOutVar_0
                            );
                        ho_Image_COPY_INP_TMP.Dispose();
                        ho_Image_COPY_INP_TMP = ExpTmpOutVar_0;
                    }
                    ho_GrayImage.Dispose();
                    HOperatorSet.Rgb1ToGray(ho_Image_COPY_INP_TMP, out ho_GrayImage);

                    //***********************************获取底部图像，计算灰度值判断图像是否过暗或者过亮
                    ho_RectangleL.Dispose();
                    HOperatorSet.GenRectangle1(out ho_RectangleL, hv_Height - 1000, (hv_Width / 2) - 1000,
                        hv_Height, hv_Width / 2);
                    ho_RectangleR.Dispose();
                    HOperatorSet.GenRectangle1(out ho_RectangleR, hv_Height - 1000, (hv_Width / 2) + 1,
                        hv_Height, (hv_Width / 2) + 1000);
                    ho_Rectangle.Dispose();
                    HOperatorSet.Union2(ho_RectangleL, ho_RectangleR, out ho_Rectangle);
                    HOperatorSet.Intensity(ho_RectangleL, ho_GrayImage, out hv_MeanL, out hv_DeviationL);
                    HOperatorSet.Intensity(ho_RectangleR, ho_GrayImage, out hv_MeanR, out hv_DeviationR);
                    HOperatorSet.Intensity(ho_Rectangle, ho_GrayImage, out hv_Mean, out hv_Deviation);
                    if ((int)(new HTuple(((((hv_MeanL - hv_MeanR)).TupleAbs())).TupleGreater(50))) != 0)
                    {
                        hv_result = 12;
                        hv_message = "左右亮度不均";
                        HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                        hv_color = "green";
                        HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                    }
                    else
                    {
                        if ((int)(new HTuple(hv_Mean.TupleGreater(235))) != 0)
                        {
                            hv_result = 10;
                            hv_message = "标准色差获取失败，图片过亮";
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "red";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                        }
                        else
                        {
                            if ((int)(new HTuple(hv_Mean.TupleLess(20))) != 0)
                            {
                                hv_result = 11;
                                hv_message = "标准色差获取失败，图片过暗";
                                HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                                hv_color = "red";
                                HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                            }
                        }
                    }

                    //***********************************************************************************
                    if ((int)(new HTuple(hv_result.TupleEqual(0))) != 0)
                    {
                        //获取布匹区域
                        ho_ImageR0Left.Dispose(); ho_ImageG0Left.Dispose(); ho_ImageB0Left.Dispose(); ho_ImageLLeft.Dispose(); ho_ImageALeft.Dispose(); ho_ImageBLeft.Dispose(); ho_ImageR0Right.Dispose(); ho_ImageG0Right.Dispose(); ho_ImageB0Right.Dispose(); ho_ImageLRight1.Dispose(); ho_ImageARigh.Dispose(); ho_ImageBRight.Dispose(); ho_ClothRegionLeft.Dispose(); ho_ClothRegionRight.Dispose();
                        get_cloth_region(ho_Image_COPY_INP_TMP, out ho_ImageR0Left, out ho_ImageG0Left,
                            out ho_ImageB0Left, out ho_ImageLLeft, out ho_ImageALeft, out ho_ImageBLeft,
                            out ho_ImageR0Right, out ho_ImageG0Right, out ho_ImageB0Right, out ho_ImageLRight1,
                            out ho_ImageARigh, out ho_ImageBRight, out ho_ClothRegionLeft, out ho_ClothRegionRight,
                            hv_isSeperateComputer);
                        HOperatorSet.AreaCenter(ho_ClothRegionLeft, out hv_AreaLeft, out hv_Row,
                            out hv_Column);
                        HOperatorSet.AreaCenter(ho_ClothRegionRight, out hv_AreaRight, out hv_Row,
                            out hv_Column);
                        ho_ClothRegion.Dispose();
                        HOperatorSet.Union2(ho_ClothRegionLeft, ho_ClothRegionRight, out ho_ClothRegion
                            );
                        if ((int)((new HTuple(hv_AreaLeft.TupleLess(1))).TupleOr(new HTuple(hv_AreaRight.TupleLess(
                            1)))) != 0)
                        {
                            //未找到布匹
                            hv_result = 1;
                            hv_message = "未找到布匹";
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "red";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                        }
                        else
                        {
                            //生成检测区域
                            HOperatorSet.SmallestRectangle1(ho_ClothRegion, out hv_Row1, out hv_clothRegionCoordinateX1,
                                out hv_Row2, out hv_clothRegionCoordinateX2);
                            ho_ValidClothRegion.Dispose();
                            HOperatorSet.ErosionRectangle1(ho_ClothRegion, out ho_ValidClothRegion,
                                hv_clothSideUnDetectWidth * 2, 1);
                            ho_RegionOpening2.Dispose();
                            HOperatorSet.OpeningRectangle1(ho_ValidClothRegion, out ho_RegionOpening2,
                                400, 1);
                            HOperatorSet.SmallestRectangle1(ho_RegionOpening2, out hv_Row15, out hv_leftDetectSide,
                                out hv_Row25, out hv_rightDetectSide);
                        }
                    }


                    if ((int) (new HTuple(hv_result.TupleEqual(0))) != 0)
                    {

                        //*****************获取标准颜色,colorDetect表示是否检测成功
                        hv_colorDetect = 0;
                        try
                        {
                            //获取标准颜色
                            ho_Boxs.Dispose();
                            get_boxes_color(ho_Image_COPY_INP_TMP, ho_ClothRegion, out ho_Boxs, hv_boxNumber,
                                hv_boxBenginX, hv_boxWidth, hv_boxHeight, hv_Width, hv_Height, hv_medianKernal,
                                out hv_tupleL, out hv_tupleA, out hv_tupleB);

                            //查看是否获取成功
                            HOperatorSet.TupleLength(hv_standardTupleL, out hv_Length);
                            if ((int) (new HTuple(hv_Length.TupleEqual(hv_boxNumber))) != 0)
                            {
                                hv_colorDetect = 1;
                            }
                        }
                        // catch (Exception1) 
                        catch (HalconException HDevExpDefaultException2)
                        {
                            HDevExpDefaultException2.ToHTuple(out hv_Exception1);
                            hv_colorDetect = 0;
                        }

                        hv_algorithmOfAberration_COPY_INP_TMP = 2;
                        hv_leftRightAberration = 0;
                        //计算左右色差
                        if ((int) (new HTuple(hv_boxNumber.TupleGreater(5))) != 0)
                        {

                            //smallest_rectangle1 (ClothRegion, Row1, Column1, Row2, Column2)
                            //Width := Column2-Column1
                            //boxDistance := (Width-boxNumber*boxWidth-boxBenginX*2)/(boxNumber-1)
                            //boxBenginX := boxBenginX+Column1
                            //*             if (boxBenginX+(boxNumber/2-1)*(boxWidth+boxDistance)+boxWidth)

                            HTuple end_val230 = (hv_boxNumber / 2) - 2;
                            HTuple step_val230 = 1;
                            for (hv_j1 = 0;
                                hv_j1.Continue(end_val230, step_val230);
                                hv_j1 = hv_j1.TupleAdd(step_val230))
                            {
                                HTuple end_val231 = (hv_boxNumber / 2) - 1;
                                HTuple step_val231 = 1;
                                for (hv_j2 = hv_j1 + 1;
                                    hv_j2.Continue(end_val231, step_val231);
                                    hv_j2 = hv_j2.TupleAdd(step_val231))
                                {
                                    get_algorithm(hv_tupleL.TupleSelect(hv_j1), hv_tupleA.TupleSelect(
                                            hv_j1), hv_tupleB.TupleSelect(hv_j1), hv_tupleL.TupleSelect(hv_j2),
                                        hv_tupleA.TupleSelect(hv_j2), hv_tupleB.TupleSelect(hv_j2),
                                        hv_algorithmOfAberration_COPY_INP_TMP,
                                        out hv_theAberration);
                                    if ((int) (new HTuple(hv_theAberration.TupleGreater(hv_leftRightAberration))) != 0)
                                    {
                                        hv_leftRightAberration = hv_theAberration.Clone();
                                    }
                                }
                            }

                            HTuple end_val238 = hv_boxNumber - 2;
                            HTuple step_val238 = 1;
                            for (hv_j1 = hv_boxNumber / 2;
                                hv_j1.Continue(end_val238, step_val238);
                                hv_j1 = hv_j1.TupleAdd(step_val238))
                            {
                                HTuple end_val239 = hv_boxNumber - 1;
                                HTuple step_val239 = 1;
                                for (hv_j2 = hv_j1 + 1;
                                    hv_j2.Continue(end_val239, step_val239);
                                    hv_j2 = hv_j2.TupleAdd(step_val239))
                                {
                                    get_algorithm(hv_tupleL.TupleSelect(hv_j1), hv_tupleA.TupleSelect(
                                            hv_j1), hv_tupleB.TupleSelect(hv_j1), hv_tupleL.TupleSelect(hv_j2),
                                        hv_tupleA.TupleSelect(hv_j2), hv_tupleB.TupleSelect(hv_j2),
                                        hv_algorithmOfAberration_COPY_INP_TMP,
                                        out hv_theAberration);
                                    if ((int) (new HTuple(hv_theAberration.TupleGreater(hv_leftRightAberration))) != 0)
                                    {
                                        hv_leftRightAberration = hv_theAberration.Clone();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ((int) (new HTuple(hv_boxNumber.TupleEqual(4))) != 0)
                            {
                                get_algorithm(hv_tupleL.TupleSelect(0), hv_tupleA.TupleSelect(0), hv_tupleB.TupleSelect(
                                    0), hv_tupleL.TupleSelect(1), hv_tupleA.TupleSelect(1), hv_tupleB.TupleSelect(
                                    1), hv_algorithmOfAberration_COPY_INP_TMP, out hv_theAberration);
                                if ((int) (new HTuple(hv_theAberration.TupleGreater(hv_leftRightAberration))) != 0)
                                {
                                    hv_leftRightAberration = hv_theAberration.Clone();
                                }

                                get_algorithm(hv_tupleL.TupleSelect(2), hv_tupleA.TupleSelect(2), hv_tupleB.TupleSelect(
                                    2), hv_tupleL.TupleSelect(3), hv_tupleA.TupleSelect(3), hv_tupleB.TupleSelect(
                                    3), hv_algorithmOfAberration_COPY_INP_TMP, out hv_theAberration);
                                if ((int) (new HTuple(hv_theAberration.TupleGreater(hv_leftRightAberration))) != 0)
                                {
                                    hv_leftRightAberration = hv_theAberration.Clone();
                                }
                            }
                        }

                        //计算上下色差
                        if ((int) (new HTuple(hv_colorDetect.TupleEqual(1))) != 0)
                        {
                            hv_aberrations = new HTuple();
                            HTuple end_val262 = hv_boxNumber - 1;
                            HTuple step_val262 = 1;
                            for (hv_j = 0; hv_j.Continue(end_val262, step_val262); hv_j = hv_j.TupleAdd(step_val262))
                            {

                                get_algorithm(hv_standardTupleL.TupleSelect(hv_j), hv_standardTupleA.TupleSelect(
                                        hv_j), hv_standardTupleB.TupleSelect(hv_j), hv_tupleL.TupleSelect(
                                        hv_j), hv_tupleA.TupleSelect(hv_j), hv_tupleB.TupleSelect(hv_j),
                                    hv_algorithmOfAberration_COPY_INP_TMP, out hv_theAberration);
                                if (hv_aberrations == null)
                                    hv_aberrations = new HTuple();
                                hv_aberrations[hv_j] = hv_theAberration;
                            }

                            HOperatorSet.TupleSort(hv_aberrations, out hv_aberrations1);




                            HOperatorSet.TupleSort(hv_tupleL, out hv_tupleL1);
                            HOperatorSet.TupleSort(hv_tupleA, out hv_tupleA1);
                            HOperatorSet.TupleSort(hv_tupleB, out hv_tupleB1);
                            if ((int) (new HTuple(hv_boxNumber.TupleGreater(2))) != 0)
                            {
                                hv_total = 0;
                                hv_totalL = 0;
                                hv_totalA = 0;
                                hv_totalB = 0;
                                HTuple end_val280 = hv_boxNumber - 2;
                                HTuple step_val280 = 1;
                                for (hv_k = 1;
                                    hv_k.Continue(end_val280, step_val280);
                                    hv_k = hv_k.TupleAdd(step_val280))
                                {
                                    hv_total = hv_total + (hv_aberrations1.TupleSelect(hv_k));
                                    hv_totalL = hv_totalL + (hv_tupleL1.TupleSelect(hv_k));
                                    hv_totalA = hv_totalA + (hv_tupleA1.TupleSelect(hv_k));
                                    hv_totalB = hv_totalB + (hv_tupleB1.TupleSelect(hv_k));
                                }

                                hv_clothAberration = hv_total / (hv_boxNumber - 2);
                                hv_L = hv_totalL / (hv_boxNumber - 2);
                                hv_A = hv_totalA / (hv_boxNumber - 2);
                                hv_B = hv_totalB / (hv_boxNumber - 2);
                            }
                            else
                            {
                                hv_clothAberration = hv_aberrations.TupleSelect(0);
                                hv_L = hv_tupleL1.TupleSelect(0);
                                hv_A = hv_tupleA1.TupleSelect(0);
                                hv_B = hv_tupleB1.TupleSelect(0);
                            }

                            hv_message = "L:" + (hv_L.TupleString("#.2f"));
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "green";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                            hv_message = "A:" + (hv_A.TupleString("#.2f"));
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "green";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                            hv_message = "B:" + (hv_B.TupleString("#.2f"));
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "green";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);


                            if ((int) (new HTuple(hv_leftRightAberration.TupleLess(hv_clothAberrationGrad1))) != 0)
                            {
                                hv_message = ("z左右色差：" + (hv_leftRightAberration.TupleString(
                                                  "#.2f"))) + "（等级：Ⅴ）";
                                HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                                hv_color = "green";
                                HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);

                            }
                            else
                            {
                                if ((int) (new HTuple(hv_leftRightAberration.TupleLess(hv_clothAberrationGrad2))) != 0)
                                {
                                    hv_message = ("左右色差：" + (hv_leftRightAberration.TupleString(
                                                      "#.2f"))) + "（等级：Ⅳ）";
                                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                                    hv_color = "lime green";
                                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color,
                                        out hv_tupleMessagesColor);
                                }
                                else
                                {
                                    if ((int) (new HTuple(hv_leftRightAberration.TupleLess(hv_clothAberrationGrad3))) !=
                                        0)
                                    {
                                        hv_message = ("左右色差：" + (hv_leftRightAberration.TupleString(
                                                          "#.2f"))) + "（等级：Ⅲ）";
                                        HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                                        hv_color = "orange";
                                        HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color,
                                            out hv_tupleMessagesColor);
                                    }
                                    else
                                    {
                                        if ((int) (new HTuple(hv_leftRightAberration.TupleLess(hv_clothAberrationGrad4))
                                            ) != 0)
                                        {
                                            hv_message = ("左右色差：" + (hv_leftRightAberration.TupleString(
                                                              "#.2f"))) + "（等级：Ⅱ）";
                                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message,
                                                out hv_tupleMessages);
                                            hv_color = "magenta";
                                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color,
                                                out hv_tupleMessagesColor);

                                        }
                                        else
                                        {
                                            hv_message = ("左右色差" + (hv_leftRightAberration.TupleString(
                                                              "#.2f"))) + "（等级：Ⅰ）";
                                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message,
                                                out hv_tupleMessages);
                                            hv_color = "red";
                                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color,
                                                out hv_tupleMessagesColor);
                                        }
                                    }
                                }
                            }


                            //tuple_max (aberrations, clothAberration)
                            if ((int) (new HTuple(hv_clothAberration.TupleLess(hv_clothAberrationGrad1))) != 0)
                            {
                                hv_message = ("上下色差：" + (hv_clothAberration.TupleString("#.2f"))) + "（等级：Ⅴ）";
                                HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                                hv_color = "green";
                                HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);

                            }
                            else
                            {
                                if ((int) (new HTuple(hv_clothAberration.TupleLess(hv_clothAberrationGrad2))) != 0)
                                {
                                    hv_message = ("上下色差：" + (hv_clothAberration.TupleString("#.2f"))) + "（等级：Ⅳ）";
                                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                                    hv_color = "lime green";
                                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color,
                                        out hv_tupleMessagesColor);
                                }
                                else
                                {
                                    if ((int) (new HTuple(hv_clothAberration.TupleLess(hv_clothAberrationGrad3))) != 0)
                                    {
                                        hv_message = ("上下色差：" + (hv_clothAberration.TupleString(
                                                          "#.2f"))) + "（等级：Ⅲ）";
                                        HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                                        hv_color = "orange";
                                        HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color,
                                            out hv_tupleMessagesColor);
                                    }
                                    else
                                    {
                                        if ((int) (new HTuple(hv_clothAberration.TupleLess(hv_clothAberrationGrad4))) !=
                                            0)
                                        {
                                            hv_message = ("上下色差：" + (hv_clothAberration.TupleString(
                                                              "#.2f"))) + "（等级：Ⅱ）";
                                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message,
                                                out hv_tupleMessages);
                                            hv_color = "magenta";
                                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color,
                                                out hv_tupleMessagesColor);

                                        }
                                        else
                                        {
                                            hv_message = ("上下色差：" + (hv_clothAberration.TupleString(
                                                              "#.2f"))) + "（等级：Ⅰ）";
                                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message,
                                                out hv_tupleMessages);
                                            hv_color = "red";
                                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color,
                                                out hv_tupleMessagesColor);
                                        }
                                    }
                                }
                            }


                        }
                        else
                        {
                            hv_message = "色差检测失败";
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "red";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                        }

                        hv_zhezhouDetectflag =0;
                        if ((int) (new HTuple(hv_result.TupleEqual(0))) != 0)
                        {
                        

                        detect_defects_fun(ho_GrayImage, ho_ClothRegion, ho_ValidClothRegion, hv_tupleMessagesColor,
                            hv_tupleMessages, hv_detectDefectsFlag, hv_edgeRollFlag, hv_imperfectBorderFlag,
                            hv_otherFlag, hv_edgeRollSlope, hv_Width, hv_Height, hv_magnification,
                            hv_imperfectBorderWidth, hv_thresh, hv_defectArea, hv_defectWidth,
                            hv_defectHeight, hv_zhezhouDetectflag, out hv_minWidth, out hv_maxWidth,
                            out hv_meanWidth, out hv_tupleMessages, out hv_tupleMessagesColor,
                            out hv_tupleDefectColumn1, out hv_tupleDefectColumn2, out hv_tupleDefectRow1,
                                out hv_tupleDefectRow2, out hv_tupleDefectClass);
                        }
                    }

                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    hv_result = -1;
                    HOperatorSet.WriteImage(ho_Image_COPY_INP_TMP, "jpeg", 0, hv_metersCounter + ".jpeg");

                }

                if ((int)(new HTuple(hv_result.TupleEqual(0))) != 0)
                {
                    hv_message = "检测结果:合格";
                    hv_color = "green";
                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                }
                else if ((int)(new HTuple(hv_result.TupleEqual(-1))) != 0)
                {
                    hv_message = "检测结果:异常";
                    hv_color = "red";
                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                }


                if ((int)(new HTuple(hv_minWidth.TupleGreater(100))) != 0)
                {
                    hv_color = "green";
                    hv_message = ("最小宽度：" + hv_minWidth) + "mm";
                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                    hv_message = ("最大宽度：" + hv_maxWidth) + "mm";
                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                    hv_message = ("平均宽度：" + hv_meanWidth) + "mm";
                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                }


                HOperatorSet.CountSeconds(out hv_second2);
                hv_message = ("算法耗时：" + (((hv_second2 - hv_second1)).TupleString("#.2f"))) + "s";
                HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                ho_Image_COPY_INP_TMP.Dispose();
                ho_DefectRegion.Dispose();
                ho_Boxs.Dispose();
                ho_Rectangle4.Dispose();
                ho_GrayImage.Dispose();
                ho_RectangleL.Dispose();
                ho_RectangleR.Dispose();
                ho_Rectangle.Dispose();
                ho_ImageR0Left.Dispose();
                ho_ImageG0Left.Dispose();
                ho_ImageB0Left.Dispose();
                ho_ImageLLeft.Dispose();
                ho_ImageALeft.Dispose();
                ho_ImageBLeft.Dispose();
                ho_ImageR0Right.Dispose();
                ho_ImageG0Right.Dispose();
                ho_ImageB0Right.Dispose();
                ho_ImageLRight1.Dispose();
                ho_ImageARigh.Dispose();
                ho_ImageBRight.Dispose();
                ho_ClothRegionLeft.Dispose();
                ho_ClothRegionRight.Dispose();
                ho_ClothRegion.Dispose();
                ho_ValidClothRegion.Dispose();
                ho_RegionOpening2.Dispose();

                return;



            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Image_COPY_INP_TMP.Dispose();
                ho_DefectRegion.Dispose();
                ho_Boxs.Dispose();
                ho_Rectangle4.Dispose();
                ho_GrayImage.Dispose();
                ho_RectangleL.Dispose();
                ho_RectangleR.Dispose();
                ho_Rectangle.Dispose();
                ho_ImageR0Left.Dispose();
                ho_ImageG0Left.Dispose();
                ho_ImageB0Left.Dispose();
                ho_ImageLLeft.Dispose();
                ho_ImageALeft.Dispose();
                ho_ImageBLeft.Dispose();
                ho_ImageR0Right.Dispose();
                ho_ImageG0Right.Dispose();
                ho_ImageB0Right.Dispose();
                ho_ImageLRight1.Dispose();
                ho_ImageARigh.Dispose();
                ho_ImageBRight.Dispose();
                ho_ClothRegionLeft.Dispose();
                ho_ClothRegionRight.Dispose();
                ho_ClothRegion.Dispose();
                ho_ValidClothRegion.Dispose();
                ho_RegionOpening2.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void get_standard_lab(HObject ho_Image, out HObject ho_Boxs, HTuple hv_isSeperateComputer,
            out HTuple hv_standardTupleL, out HTuple hv_standardTupleA, out HTuple hv_standardTupleB,
            out HTuple hv_standardL, out HTuple hv_standardA, out HTuple hv_standardB, out HTuple hv_result,
            out HTuple hv_tupleDetectResult, out HTuple hv_defectNumber, out HTuple hv_tupleDefectClass,
            out HTuple hv_tupleDefectX, out HTuple hv_tupleDefectY, out HTuple hv_tupleDefectRow1,
            out HTuple hv_tupleDefectRow2, out HTuple hv_tupleDefectColumn1, out HTuple hv_tupleDefectColumn2,
            out HTuple hv_minWidth, out HTuple hv_maxWidth, out HTuple hv_meanWidth, out HTuple hv_metersCounter,
            out HTuple hv_tupleMessages, out HTuple hv_tupleMessagesColor, out HTuple hv_leftDetectSide,
            out HTuple hv_rightDetectSide, out HTuple hv_L, out HTuple hv_A, out HTuple hv_B,
            out HTuple hv_clothRegionCoordinateX1, out HTuple hv_clothRegionCoordinateX2)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_DefectRegion = null, ho_Rectangle4 = null;
            HObject ho_GrayImage = null, ho_RectangleL = null, ho_RectangleR = null;
            HObject ho_Rectangle = null, ho_ImageR0Left = null, ho_ImageG0Left = null;
            HObject ho_ImageB0Left = null, ho_ImageLLeft = null, ho_ImageALeft = null;
            HObject ho_ImageBLeft = null, ho_ImageR0Right = null, ho_ImageG0Right = null;
            HObject ho_ImageB0Right = null, ho_ImageLRight1 = null, ho_ImageARigh = null;
            HObject ho_ImageBRight = null, ho_ClothRegionLeft = null, ho_ClothRegionRight = null;
            HObject ho_ClothRegion = null, ho_ValidClothRegion = null, ho_RegionOpening2 = null;
            HObject ho_GrayImageRight = null, ho_GrayImageLeft = null;

            // Local copy input parameter variables 
            HObject ho_Image_COPY_INP_TMP;
            ho_Image_COPY_INP_TMP = ho_Image.CopyObj(1, -1);



            // Local control variables 

            HTuple hv_magnification = null, hv_leftSide = null;
            HTuple hv_rightSide = null, hv_boxNumber = null, hv_boxWidth = null;
            HTuple hv_boxHeight = null, hv_boxBenginX = null, hv_dynThresh = null;
            HTuple hv_medianKernal = null, hv_thresh = null, hv_defectArea = null;
            HTuple hv_defectWidth = null, hv_defectHeight = null, hv_edgeRollSlope = null;
            HTuple hv_imperfectBorderWidth = null, hv_clothAberrationGrad1 = null;
            HTuple hv_clothAberrationGrad2 = null, hv_clothAberrationGrad3 = null;
            HTuple hv_clothAberrationGrad4 = null, hv_clothSideUnDetectWidth = null;
            HTuple hv_defectLoactionNumber = null, hv_tupleDefectRadius = null;
            HTuple hv_clothAberration = null, hv_detectDefectsFlag = null;
            HTuple hv_edgeRollFlag = null, hv_imperfectBorderFlag = null;
            HTuple hv_zhezhouDetectflag = null, hv_otherFlag = null;
            HTuple hv_second1 = null, hv_message = new HTuple(), hv_color = new HTuple();
            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_MeanL = new HTuple(), hv_DeviationL = new HTuple();
            HTuple hv_MeanR = new HTuple(), hv_DeviationR = new HTuple();
            HTuple hv_Mean = new HTuple(), hv_Deviation = new HTuple();
            HTuple hv_AreaLeft = new HTuple(), hv_Row = new HTuple();
            HTuple hv_Column = new HTuple(), hv_AreaRight = new HTuple();
            HTuple hv_Row1 = new HTuple(), hv_Row2 = new HTuple();
            HTuple hv_Row15 = new HTuple(), hv_Row25 = new HTuple();
            HTuple hv_colorDetect = new HTuple(), hv_Length = new HTuple();
            HTuple hv_Exception1 = new HTuple(), hv_tupleL1 = new HTuple();
            HTuple hv_tupleA1 = new HTuple(), hv_tupleB1 = new HTuple();
            HTuple hv_total = new HTuple(), hv_totalL = new HTuple();
            HTuple hv_totalA = new HTuple(), hv_totalB = new HTuple();
            HTuple hv_k = new HTuple(), hv_tupleMessagesColor1 = new HTuple();
            HTuple hv_tupleMessages1 = new HTuple(), hv_tupleMessagesColor2 = new HTuple();
            HTuple hv_tupleMessages2 = new HTuple(), hv_tupleDefectRow11 = new HTuple();
            HTuple hv_tupleDefectRow21 = new HTuple(), hv_tupleDefectColumn11 = new HTuple();
            HTuple hv_tupleDefectColumn21 = new HTuple(), hv_tupleDefectRow12 = new HTuple();
            HTuple hv_tupleDefectRow22 = new HTuple(), hv_tupleDefectColumn12 = new HTuple();
            HTuple hv_tupleDefectColumn22 = new HTuple(), hv_tupleDefectClass11 = new HTuple();
            HTuple hv_tupleDefectClass12 = new HTuple(), hv_tupleDefectClass1 = new HTuple();
            HTuple hv_Exception = null, hv_second2 = null;
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Boxs);
            HOperatorSet.GenEmptyObj(out ho_DefectRegion);
            HOperatorSet.GenEmptyObj(out ho_Rectangle4);
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_RectangleL);
            HOperatorSet.GenEmptyObj(out ho_RectangleR);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_ImageR0Left);
            HOperatorSet.GenEmptyObj(out ho_ImageG0Left);
            HOperatorSet.GenEmptyObj(out ho_ImageB0Left);
            HOperatorSet.GenEmptyObj(out ho_ImageLLeft);
            HOperatorSet.GenEmptyObj(out ho_ImageALeft);
            HOperatorSet.GenEmptyObj(out ho_ImageBLeft);
            HOperatorSet.GenEmptyObj(out ho_ImageR0Right);
            HOperatorSet.GenEmptyObj(out ho_ImageG0Right);
            HOperatorSet.GenEmptyObj(out ho_ImageB0Right);
            HOperatorSet.GenEmptyObj(out ho_ImageLRight1);
            HOperatorSet.GenEmptyObj(out ho_ImageARigh);
            HOperatorSet.GenEmptyObj(out ho_ImageBRight);
            HOperatorSet.GenEmptyObj(out ho_ClothRegionLeft);
            HOperatorSet.GenEmptyObj(out ho_ClothRegionRight);
            HOperatorSet.GenEmptyObj(out ho_ClothRegion);
            HOperatorSet.GenEmptyObj(out ho_ValidClothRegion);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening2);
            HOperatorSet.GenEmptyObj(out ho_GrayImageRight);
            HOperatorSet.GenEmptyObj(out ho_GrayImageLeft);
            hv_standardTupleL = new HTuple();
            hv_standardTupleA = new HTuple();
            hv_standardTupleB = new HTuple();
            try
            {
                //*********************输入参数
                //magnification放大率piexels/mm
                //LeftSide左边有效参数
                //RightSide右边有效参数
                //boxNumber框个数
                //boxWidth框宽度
                //boxHeight框高度
                //boxBenginX框起始X坐标
                //dynThresh缺陷阈值
                //medianKernal滤波卷积核大小
                //defectArea缺陷面积
                //edgeRollSlope判断卷边的斜率偏差
                //imperfectBorderWidth判断缺边的宽度
                //leftSide左边有效区域
                //rightSide右边有效区域
                //clothAberrationGrad1-clothAberrationGrad4色差等级分类
                //布匹边缘不检测宽度clothSideUnDetectWidth
                //*****************************
                hv_magnification = 4.8188;
                hv_leftSide = 41.5040;
                hv_rightSide = 41.5040;
                hv_boxNumber = 6;
                hv_boxWidth = 83;
                hv_boxHeight = 83;
                hv_boxBenginX = 200;
                hv_dynThresh = 15;
                hv_medianKernal = 20;
                hv_thresh = 30;
                hv_defectArea = 0.2157;
                hv_defectWidth = 1.0393;
                hv_defectHeight = 1.0393;
                hv_edgeRollSlope = 0.1;
                hv_imperfectBorderWidth = 4.15;
                hv_clothAberrationGrad1 = 0.5;
                hv_clothAberrationGrad2 = 1.5;
                hv_clothAberrationGrad3 = 3.0;
                hv_clothAberrationGrad4 = 6.0;
                hv_clothSideUnDetectWidth = 20.7;


                hv_metersCounter = 1;
                hv_leftSide = hv_leftSide * hv_magnification;
                hv_rightSide = hv_rightSide * hv_magnification;
                hv_boxWidth = hv_boxWidth * hv_magnification;
                hv_boxHeight = hv_boxHeight * hv_magnification;
                hv_boxBenginX = hv_boxBenginX * hv_magnification;
                hv_defectArea = (hv_defectArea * hv_magnification) * hv_magnification;
                hv_defectWidth = hv_defectWidth * hv_magnification;
                hv_defectHeight = hv_defectHeight * hv_magnification;
                hv_imperfectBorderWidth = hv_imperfectBorderWidth * hv_magnification;
                hv_clothSideUnDetectWidth = hv_clothSideUnDetectWidth * hv_magnification;
                hv_result = 0;



                //*********************输入参数*******************
                hv_magnification = PublicClass.mag;
                hv_leftSide = PublicClass.leftside * PublicClass.mag;
                hv_rightSide = PublicClass.rightside * hv_magnification;
                hv_boxWidth = PublicClass.Square * hv_magnification;
                hv_boxHeight = PublicClass.Square * hv_magnification;
                hv_boxBenginX = PublicClass.OriginX * hv_magnification;
                hv_defectArea = (PublicClass.defectArea * hv_magnification) * hv_magnification;
                hv_defectWidth = PublicClass.defectWidth * hv_magnification;
                hv_defectHeight = PublicClass.defectHeight * hv_magnification;
                hv_imperfectBorderWidth = PublicClass.imperfectBorderWidth * hv_magnification;
                hv_clothSideUnDetectWidth = PublicClass.clothSideUnDetectWidth * hv_magnification;
                hv_edgeRollSlope = PublicClass.EdgeRollslope;
                hv_isSeperateComputer = PublicClass.isSeperateComputer;
                 //   hv_ExpDefaultWinHandle = hv_windowHandle;
                hv_metersCounter = MetersCounter;
                //*********************输入参数*********************




                //defectLoactionNumber缺陷位置编号
                hv_defectLoactionNumber = 1;
                //***********************************
                //tupleDetectResult表示各个缺陷的个数,>0为有缺陷
                //tupleDetectResult[0]表示接缝个数，严重，亮红灯
                //tupleDetectResult[1]表示周期性缺陷个数，严重，亮红灯
                //tupleDetectResult[2]表示卷边，严重，亮红灯
                //tupleDetectResult[3]表示缺边个数，黄灯
                //tupleDetectResult[4]表示点瑕疵个数，黄灯
                //***********************************
                hv_tupleDetectResult = new HTuple();
                hv_tupleDetectResult[0] = 0;
                hv_tupleDetectResult[1] = 0;
                hv_tupleDetectResult[2] = 0;
                hv_tupleDetectResult[3] = 0;
                hv_tupleDetectResult[4] = 0;
                hv_tupleDetectResult[5] = 0;
                hv_tupleDetectResult[6] = 0;
                hv_tupleDetectResult[7] = 0;
                hv_tupleDetectResult[8] = 0;
                hv_tupleDetectResult[9] = 0;
                //瑕疵半径
                hv_tupleDefectRadius = new HTuple();
                //瑕疵X坐标
                hv_tupleDefectX = new HTuple();
                //瑕疵Y坐标
                hv_tupleDefectY = new HTuple();
                //tupleDefectClass表示瑕疵分类,0表示接，1表示周期性缺陷，2表示卷边，3表示缺边，4表示其他瑕疵
                hv_tupleDefectClass = new HTuple();

                hv_L = 0;
                hv_A = 0;
                hv_B = 0;
                //clothberration表示色差值
                hv_clothAberration = 0;
                hv_minWidth = 0;
                hv_maxWidth = 0;
                hv_meanWidth = 0;

                //leftDetectSide、rightDetectSide
                hv_leftDetectSide = 0;
                hv_rightDetectSide = 0;
                //输出结果
                hv_tupleMessages = new HTuple();
                hv_tupleMessagesColor = new HTuple();
                //检测缺陷的个数
                hv_defectNumber = 0;
                //缺陷框坐标
                hv_tupleDefectRow1 = new HTuple();
                hv_tupleDefectRow2 = new HTuple();
                hv_tupleDefectColumn1 = new HTuple();
                hv_tupleDefectColumn2 = new HTuple();


                //缺陷检测标志位
                hv_detectDefectsFlag = 1;
                //卷边检测标志位
                hv_edgeRollFlag = 1;
                //缺边检测
                hv_imperfectBorderFlag = 1;
                //其他缺陷检测
                //褶皱检测标志位
                hv_zhezhouDetectflag = 0;
                hv_otherFlag = 1;
                hv_standardL = 0;
                hv_standardA = 0;
                hv_standardB = 0;
                hv_clothRegionCoordinateX1 = 0;
                hv_clothRegionCoordinateX2 = 0;


                HOperatorSet.CountSeconds(out hv_second1);
                try
                {
                    hv_message = "图片编号：" + hv_metersCounter;
                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    hv_color = "green";
                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);

                    ho_DefectRegion.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_DefectRegion);
                    ho_Boxs.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_Boxs);
                    HOperatorSet.GetImageSize(ho_Image_COPY_INP_TMP, out hv_Width, out hv_Height);
                    //裁减无效区域
                    ho_Rectangle4.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle4, -10, 148, hv_Height + 10, 8059);
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ReduceDomain(ho_Image_COPY_INP_TMP, ho_Rectangle4, out ExpTmpOutVar_0
                            );
                        ho_Image_COPY_INP_TMP.Dispose();
                        ho_Image_COPY_INP_TMP = ExpTmpOutVar_0;
                    }
                    ho_GrayImage.Dispose();
                    HOperatorSet.Rgb1ToGray(ho_Image_COPY_INP_TMP, out ho_GrayImage);

                    //***********************************获取底部图像，计算灰度值判断图像是否过暗或者过亮
                    ho_RectangleL.Dispose();
                    HOperatorSet.GenRectangle1(out ho_RectangleL, hv_Height - 1000, (hv_Width / 2) - 1000,
                        hv_Height, hv_Width / 2);
                    ho_RectangleR.Dispose();
                    HOperatorSet.GenRectangle1(out ho_RectangleR, hv_Height - 1000, (hv_Width / 2) + 1,
                        hv_Height, (hv_Width / 2) + 1000);
                    ho_Rectangle.Dispose();
                    HOperatorSet.Union2(ho_RectangleL, ho_RectangleR, out ho_Rectangle);
                    HOperatorSet.Intensity(ho_RectangleL, ho_GrayImage, out hv_MeanL, out hv_DeviationL);
                    HOperatorSet.Intensity(ho_RectangleR, ho_GrayImage, out hv_MeanR, out hv_DeviationR);
                    HOperatorSet.Intensity(ho_Rectangle, ho_GrayImage, out hv_Mean, out hv_Deviation);
                    if ((int)(new HTuple(((((hv_MeanL - hv_MeanR)).TupleAbs())).TupleGreater(50))) != 0)
                    {
                        hv_result = 12;
                        hv_message = "左右亮度不均";
                        HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                        hv_color = "green";
                        HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                    }
                    else
                    {
                        if ((int)(new HTuple(hv_Mean.TupleGreater(235))) != 0)
                        {
                            hv_result = 10;
                            hv_message = "标准色差获取失败，图片过亮";
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "red";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                        }
                        else
                        {
                            if ((int)(new HTuple(hv_Mean.TupleLess(20))) != 0)
                            {
                                hv_result = 11;
                                hv_message = "标准色差获取失败，图片过暗";
                                HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                                hv_color = "red";
                                HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                            }
                        }
                    }

                    //***********************************************************************************
                    if ((int)(new HTuple(hv_result.TupleEqual(0))) != 0)
                    {
                        //获取布匹区域
                        ho_ImageR0Left.Dispose(); ho_ImageG0Left.Dispose(); ho_ImageB0Left.Dispose(); ho_ImageLLeft.Dispose(); ho_ImageALeft.Dispose(); ho_ImageBLeft.Dispose(); ho_ImageR0Right.Dispose(); ho_ImageG0Right.Dispose(); ho_ImageB0Right.Dispose(); ho_ImageLRight1.Dispose(); ho_ImageARigh.Dispose(); ho_ImageBRight.Dispose(); ho_ClothRegionLeft.Dispose(); ho_ClothRegionRight.Dispose();
                        get_cloth_region(ho_Image_COPY_INP_TMP, out ho_ImageR0Left, out ho_ImageG0Left,
                            out ho_ImageB0Left, out ho_ImageLLeft, out ho_ImageALeft, out ho_ImageBLeft,
                            out ho_ImageR0Right, out ho_ImageG0Right, out ho_ImageB0Right, out ho_ImageLRight1,
                            out ho_ImageARigh, out ho_ImageBRight, out ho_ClothRegionLeft, out ho_ClothRegionRight,
                            hv_isSeperateComputer);
                        HOperatorSet.AreaCenter(ho_ClothRegionLeft, out hv_AreaLeft, out hv_Row,
                            out hv_Column);
                        HOperatorSet.AreaCenter(ho_ClothRegionRight, out hv_AreaRight, out hv_Row,
                            out hv_Column);
                        ho_ClothRegion.Dispose();
                        HOperatorSet.Union2(ho_ClothRegionLeft, ho_ClothRegionRight, out ho_ClothRegion
                            );
                        if ((int)((new HTuple(hv_AreaLeft.TupleLess(1))).TupleOr(new HTuple(hv_AreaRight.TupleLess(
                            1)))) != 0)
                        {
                            //未找到布匹
                            hv_result = 1;
                            hv_message = "未找到布匹";
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "red";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                        }
                        else
                        {
                            HOperatorSet.SmallestRectangle1(ho_ClothRegion, out hv_Row1, out hv_clothRegionCoordinateX1,
                                out hv_Row2, out hv_clothRegionCoordinateX2);

                            //生成检测区域
                            ho_ValidClothRegion.Dispose();
                            HOperatorSet.ErosionRectangle1(ho_ClothRegion, out ho_ValidClothRegion,
                                hv_clothSideUnDetectWidth * 2, 1);
                            ho_RegionOpening2.Dispose();
                            HOperatorSet.OpeningRectangle1(ho_ValidClothRegion, out ho_RegionOpening2,
                                400, 1);
                            HOperatorSet.SmallestRectangle1(ho_RegionOpening2, out hv_Row15, out hv_leftDetectSide,
                                out hv_Row25, out hv_rightDetectSide);
                        }
                    }

                    if ((int)(new HTuple(hv_result.TupleEqual(0))) != 0)
                    {
                        //*****************获取标准颜色,colorDetect表示是否检测成功
                        hv_colorDetect = 0;
                        try
                        {
                            //获取标准颜色
                            ho_Boxs.Dispose();
                            get_boxes_color(ho_Image_COPY_INP_TMP, ho_ClothRegion, out ho_Boxs, hv_boxNumber,
                                hv_boxBenginX, hv_boxWidth, hv_boxHeight, hv_Width, hv_Height, hv_medianKernal,
                                out hv_standardTupleL, out hv_standardTupleA, out hv_standardTupleB);
                            //查看是否获取成功
                            HOperatorSet.TupleLength(hv_standardTupleL, out hv_Length);
                            if ((int)(new HTuple(hv_Length.TupleEqual(hv_boxNumber))) != 0)
                            {
                                hv_colorDetect = 1;
                            }
                        }
                        // catch (Exception1) 
                        catch (HalconException HDevExpDefaultException2)
                        {
                            HDevExpDefaultException2.ToHTuple(out hv_Exception1);
                            hv_colorDetect = 0;
                        }
                        if ((int)(new HTuple(hv_colorDetect.TupleEqual(1))) != 0)
                        {
                            hv_message = "标准色差获取成功";
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "green";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);



                            HOperatorSet.TupleSort(hv_standardTupleL, out hv_tupleL1);
                            HOperatorSet.TupleSort(hv_standardTupleA, out hv_tupleA1);
                            HOperatorSet.TupleSort(hv_standardTupleB, out hv_tupleB1);
                            if ((int)(new HTuple(hv_boxNumber.TupleGreater(2))) != 0)
                            {
                                hv_total = 0;
                                hv_totalL = 0;
                                hv_totalA = 0;
                                hv_totalB = 0;
                                HTuple end_val237 = hv_boxNumber - 2;
                                HTuple step_val237 = 1;
                                for (hv_k = 1; hv_k.Continue(end_val237, step_val237); hv_k = hv_k.TupleAdd(step_val237))
                                {
                                    hv_totalL = hv_totalL + (hv_tupleL1.TupleSelect(hv_k));
                                    hv_totalA = hv_totalA + (hv_tupleA1.TupleSelect(hv_k));
                                    hv_totalB = hv_totalB + (hv_tupleB1.TupleSelect(hv_k));
                                }
                                hv_clothAberration = hv_total / (hv_boxNumber - 2);
                                hv_L = hv_totalL / (hv_boxNumber - 2);
                                hv_A = hv_totalA / (hv_boxNumber - 2);
                                hv_B = hv_totalB / (hv_boxNumber - 2);
                            }
                            else
                            {
                                hv_L = hv_tupleL1.TupleSelect(0);
                                hv_A = hv_tupleA1.TupleSelect(0);
                                hv_B = hv_tupleB1.TupleSelect(0);
                            }

                            hv_message = "L:" + (hv_L.TupleString("#.2f"));
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "green";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                            hv_message = "A:" + (hv_A.TupleString("#.2f"));
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "green";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                            hv_message = "B:" + (hv_B.TupleString("#.2f"));
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "green";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);

                        }
                        else
                        {
                            hv_message = "标准色差获取失败";
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "red";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                        }
                    }

                    if ((int) (new HTuple(hv_result.TupleEqual(0))) != 0)
                    {
                        ho_GrayImageRight.Dispose();
                        HOperatorSet.ReduceDomain(ho_GrayImage, ho_ClothRegionRight, out ho_GrayImageRight
                        );
                        ho_GrayImageLeft.Dispose();
                        HOperatorSet.ReduceDomain(ho_GrayImage, ho_ClothRegionLeft, out ho_GrayImageLeft
                        );
                        hv_tupleMessagesColor1 = new HTuple();
                        hv_tupleMessages1 = new HTuple();
                        hv_tupleMessagesColor2 = new HTuple();
                        hv_tupleMessages2 = new HTuple();
                        //缺陷框坐标
                        hv_tupleDefectRow11 = new HTuple();
                        hv_tupleDefectRow21 = new HTuple();
                        hv_tupleDefectColumn11 = new HTuple();
                        hv_tupleDefectColumn21 = new HTuple();
                        //缺陷框坐标
                        hv_tupleDefectRow12 = new HTuple();
                        hv_tupleDefectRow22 = new HTuple();
                        hv_tupleDefectColumn12 = new HTuple();
                        hv_tupleDefectColumn22 = new HTuple();
                        hv_tupleDefectClass11 = new HTuple();
                        hv_tupleDefectClass12 = new HTuple();
                        detect_defects_fun(ho_GrayImage, ho_ClothRegion, ho_ValidClothRegion, hv_tupleMessagesColor,
                            hv_tupleMessages, hv_zhezhouDetectflag, hv_edgeRollFlag, hv_imperfectBorderFlag,
                            hv_otherFlag, hv_edgeRollSlope, hv_Width, hv_Height, hv_magnification,
                            hv_imperfectBorderWidth, hv_thresh, hv_defectArea, hv_defectWidth, hv_defectHeight,
                            hv_zhezhouDetectflag, out hv_minWidth, out hv_maxWidth, out hv_meanWidth,
                            out hv_tupleMessages, out hv_tupleMessagesColor, out hv_tupleDefectColumn1,
                            out hv_tupleDefectColumn2, out hv_tupleDefectRow1, out hv_tupleDefectRow2,
                            out hv_tupleDefectClass);
                    }
                    //par_start<thread1> : detect_defects_fun (GrayImageLeft, ClothRegionLeft, ValidClothRegion, tupleMessagesColor1, tupleMessages1, detectDefectsFlag, edgeRollFlag, imperfectBorderFlag, otherFlag, edgeRollSlope, Width, Height, magnification, imperfectBorderWidth, thresh, defectArea, defectWidth, defectHeight, zhezhouDetectfalg1, minWidth, maxWidth, meanWidth, tupleMessages, tupleMessagesColor1, tupleDefectColumn11, tupleDefectColumn21, tupleDefectRow11, tupleDefectRow21, tupleDefectClass11)
                    //par_start<thread2> : detect_defects_fun (GrayImageRight, ClothRegionRight, ValidClothRegion, tupleMessagesColor1, tupleMessages1, detectDefectsFlag, edgeRollFlag, imperfectBorderFlag, otherFlag, edgeRollSlope, Width, Height, magnification, imperfectBorderWidth, thresh, defectArea, defectWidth, defectHeight, zhezhouDetectfalg2, minWidth, maxWidth, meanWidth, tupleMessages, tupleMessagesColor2, tupleDefectColumn12, tupleDefectColumn22, tupleDefectRow12, tupleDefectRow22, tupleDefectClass12)
                    //par_join ([thread1,thread2])
                    //tuple_concat (tupleMessagesColor1, tupleMessagesColor2, tupleMessagesColor)
                    //tuple_concat (tupleMessages1, tupleMessages2, tupleMessages)
                    //tuple_concat (tupleDefectColumn11, tupleDefectColumn12, tupleDefectColumn1)
                    //tuple_concat (tupleDefectColumn21, tupleDefectColumn22, tupleDefectColumn2)
                    //tuple_concat (tupleDefectRow11, tupleDefectRow12, tupleDefectRow1)
                    //tuple_concat (tupleDefectRow21, tupleDefectRow22, tupleDefectRow2)
                    //tuple_concat (tupleDefectClass11, tupleDefectClass12, tupleDefectClass1)

                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    hv_result = -1;
                    HOperatorSet.WriteImage(ho_Image_COPY_INP_TMP, "jpeg", 0, hv_metersCounter + ".jpeg");

                }

                if ((int)(new HTuple(hv_result.TupleEqual(-1))) != 0)
                {
                    hv_message = "检测结果:异常";
                    hv_color = "red";
                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                }
                else
                {
                    if ((int)(new HTuple((new HTuple(hv_tupleDefectRow1.TupleLength())).TupleEqual(
                        0))) != 0)
                    {
                        hv_message = "检测结果:合格";
                        hv_color = "green";
                        HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                        HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                    }
                }

                if ((int)(new HTuple(hv_minWidth.TupleGreater(100))) != 0)
                {
                    hv_message = ("最小宽度：" + hv_minWidth) + "mm";
                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                    hv_message = ("最大宽度：" + hv_maxWidth) + "mm";
                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                    hv_message = ("平均宽度：" + hv_meanWidth) + "mm";
                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                }


                HOperatorSet.CountSeconds(out hv_second2);
                hv_message = ("算法耗时：" + (((hv_second2 - hv_second1)).TupleString("#.2f"))) + "s";
                HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                ho_Image_COPY_INP_TMP.Dispose();
                ho_DefectRegion.Dispose();
                ho_Rectangle4.Dispose();
                ho_GrayImage.Dispose();
                ho_RectangleL.Dispose();
                ho_RectangleR.Dispose();
                ho_Rectangle.Dispose();
                ho_ImageR0Left.Dispose();
                ho_ImageG0Left.Dispose();
                ho_ImageB0Left.Dispose();
                ho_ImageLLeft.Dispose();
                ho_ImageALeft.Dispose();
                ho_ImageBLeft.Dispose();
                ho_ImageR0Right.Dispose();
                ho_ImageG0Right.Dispose();
                ho_ImageB0Right.Dispose();
                ho_ImageLRight1.Dispose();
                ho_ImageARigh.Dispose();
                ho_ImageBRight.Dispose();
                ho_ClothRegionLeft.Dispose();
                ho_ClothRegionRight.Dispose();
                ho_ClothRegion.Dispose();
                ho_ValidClothRegion.Dispose();
                ho_RegionOpening2.Dispose();
                ho_GrayImageRight.Dispose();
                ho_GrayImageLeft.Dispose();

                return;



            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Image_COPY_INP_TMP.Dispose();
                ho_DefectRegion.Dispose();
                ho_Rectangle4.Dispose();
                ho_GrayImage.Dispose();
                ho_RectangleL.Dispose();
                ho_RectangleR.Dispose();
                ho_Rectangle.Dispose();
                ho_ImageR0Left.Dispose();
                ho_ImageG0Left.Dispose();
                ho_ImageB0Left.Dispose();
                ho_ImageLLeft.Dispose();
                ho_ImageALeft.Dispose();
                ho_ImageBLeft.Dispose();
                ho_ImageR0Right.Dispose();
                ho_ImageG0Right.Dispose();
                ho_ImageB0Right.Dispose();
                ho_ImageLRight1.Dispose();
                ho_ImageARigh.Dispose();
                ho_ImageBRight.Dispose();
                ho_ClothRegionLeft.Dispose();
                ho_ClothRegionRight.Dispose();
                ho_ClothRegion.Dispose();
                ho_ValidClothRegion.Dispose();
                ho_RegionOpening2.Dispose();
                ho_GrayImageRight.Dispose();
                ho_GrayImageLeft.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void get_defect_aberration_fun(HObject ho_Image, HTuple hv_WindowHandle,
            HTuple hv_inPar, out HTuple hv_result, out HTuple hv_defectNumber, out HTuple hv_tupleDefectX,
            out HTuple hv_tupleDefectY, out HTuple hv_tupleDefectRadius, out HTuple hv_L,
            out HTuple hv_A, out HTuple hv_B)
        {




            // Local iconic variables 

            // Local control variables 

            HTuple hv_boxNumber = null, hv_boxWidth = null;
            HTuple hv_boxHeight = null, hv_boxBenginX = null, hv_medianKernal = null;
            HTuple hv_dynThresh = null, hv_defectArea = null, hv_standardTupleL = null;
            HTuple hv_standardTupleA = null, hv_standardTupleB = null;
            HTuple hv_clothAberrationGrad1 = null, hv_clothAberrationGrad2 = null;
            HTuple hv_clothAberrationGrad3 = null, hv_clothAberrationGrad4 = null;
            // Initialize local and output iconic variables 
            hv_result = new HTuple();
            hv_defectNumber = new HTuple();
            hv_tupleDefectX = new HTuple();
            hv_tupleDefectY = new HTuple();
            hv_tupleDefectRadius = new HTuple();
            hv_L = new HTuple();
            hv_A = new HTuple();
            hv_B = new HTuple();
            try
            {
                hv_boxNumber = hv_inPar.TupleSelect(0);
                hv_boxWidth = hv_inPar.TupleSelect(1);
                hv_boxHeight = hv_inPar.TupleSelect(2);
                hv_boxBenginX = hv_inPar.TupleSelect(3);
                hv_medianKernal = hv_inPar.TupleSelect(4);
                hv_dynThresh = hv_inPar.TupleSelect(5);
                hv_defectArea = hv_inPar.TupleSelect(6);
                hv_standardTupleL = hv_inPar.TupleSelect(7);
                hv_standardTupleA = hv_inPar.TupleSelect(8);
                hv_standardTupleB = hv_inPar.TupleSelect(9);
                hv_clothAberrationGrad1 = hv_inPar.TupleSelect(10);
                hv_clothAberrationGrad2 = hv_inPar.TupleSelect(11);
                hv_clothAberrationGrad3 = hv_inPar.TupleSelect(12);
                hv_clothAberrationGrad4 = hv_inPar.TupleSelect(13);
                //get_defect_aberration (Image, ImageWithDefect, windowHandle, standardTupleL1, standardTupleA1, standardTupleB1, isSeperateComputer, algorithmOfAberration, clothAberration, leftRightAberration, L, A, B, result1, tupleDetectResult, defectNumber, tupleDefectClass, tupleDefectX, tupleDefectY, tupleDefectRow1, tupleDefectRow2, tupleDefectColumn1, tupleDefectColumn2, minWidth, maxWidth, meanWidth, metersCounter, tupleMessages, tupleMessagesColor, leftDetectSide, rightDetectSide, clothRegionCoordinateX1, clothRegionCoordinateX2)


                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                throw HDevExpDefaultException;
            }
        }

        public void estimate_background_illumination(HObject ho_Image, out HObject ho_IlluminationImage)
        {



            // Local iconic variables 

            HObject ho_ImageFFT, ho_ImageGauss, ho_ImageConvol;

            // Local control variables 

            HTuple hv_Width = null, hv_Height = null;
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_IlluminationImage);
            HOperatorSet.GenEmptyObj(out ho_ImageFFT);
            HOperatorSet.GenEmptyObj(out ho_ImageGauss);
            HOperatorSet.GenEmptyObj(out ho_ImageConvol);
            try
            {
                HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
                ho_ImageFFT.Dispose();
                HOperatorSet.RftGeneric(ho_Image, out ho_ImageFFT, "to_freq", "none", "complex",
                    hv_Width);
                ho_ImageGauss.Dispose();
                HOperatorSet.GenGaussFilter(out ho_ImageGauss, 50, 50, 0, "n", "rft", hv_Width,
                    hv_Height);
                ho_ImageConvol.Dispose();
                HOperatorSet.ConvolFft(ho_ImageFFT, ho_ImageGauss, out ho_ImageConvol);
                ho_IlluminationImage.Dispose();
                HOperatorSet.RftGeneric(ho_ImageConvol, out ho_IlluminationImage, "from_freq",
                    "none", "byte", hv_Width);
                ho_ImageFFT.Dispose();
                ho_ImageGauss.Dispose();
                ho_ImageConvol.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_ImageFFT.Dispose();
                ho_ImageGauss.Dispose();
                ho_ImageConvol.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void disp_detect_result(HObject ho_Image, HObject ho_Boxs, out HObject ho_ImageDump,
            HTuple hv_windowHandle, HTuple hv_tupleBackgroundColor, HTuple hv_tupleDefectRow1,
            HTuple hv_tupleDefectRow2, HTuple hv_tupleDefectColumn1, HTuple hv_tupleDefectColumn2,
            HTuple hv_minWidth, HTuple hv_maxWidth, HTuple hv_meanWidth, HTuple hv_metersCounter,
            HTuple hv_tupleMessages, HTuple hv_tupleMessagesColor, HTuple hv_LeftDetectSide,
            HTuple hv_RightDetectSide)
        {




            // Local iconic variables 

            HObject ho_Rectangle1 = null, ho_Rectangle2 = null;
            HObject ho_Rectangle = null;

            // Local control variables 

            HTuple hv_Width = null, hv_Height = null, hv_j = null;
            HTuple hv_i = null;
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageDump);
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            HOperatorSet.GenEmptyObj(out ho_Rectangle2);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            try
            {
                HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
                //DispWidth := 0.207*Width
                //gen_image_const (TempImage, 'byte', Width, Height)
                //if (|tupleBackgroundColor|>2 and tupleBackgroundColor[0]<256 and tupleBackgroundColor[1]<255 and tupleBackgroundColor[2]<255)
                //gen_image_proto (TempImage, ImageCleared1, tupleBackgroundColor[0])
                //gen_image_proto (TempImage, ImageCleared2, tupleBackgroundColor[1])
                //gen_image_proto (TempImage, ImageCleared3, tupleBackgroundColor[2])
                //compose3 (ImageCleared1, ImageCleared2, ImageCleared3, MultiChannelImage)
                //else
                //compose3 (TempImage, TempImage, TempImage, MultiChannelImage)
                //endif
                //concat_obj (Image, MultiChannelImage, ImagesConcat)
                //tile_images (ImagesConcat, TiledImage, 2, 'vertical')
                //crop_rectangle1 (TiledImage, ImagePart, 0, 0, Height, Width+DispWidth)
                //dev_set_part (0, 0, Height, Width+DispWidth)
                HOperatorSet.SetPart(hv_ExpDefaultWinHandle, 0, 0, hv_Height, hv_Width);
                HOperatorSet.ClearWindow(hv_ExpDefaultWinHandle);
                HOperatorSet.SetLineWidth(hv_ExpDefaultWinHandle, 4);
                set_display_font(hv_ExpDefaultWinHandle, 16, "mono", "true", "false");
                //dev_display (ImagePart)
                HOperatorSet.DispObj(ho_Image, hv_ExpDefaultWinHandle);
                HOperatorSet.SetColor(hv_ExpDefaultWinHandle, "green");
                HOperatorSet.SetDraw(hv_ExpDefaultWinHandle, "margin");
                HOperatorSet.DispObj(ho_Boxs, hv_ExpDefaultWinHandle);
                if ((int)((new HTuple((new HTuple(hv_RightDetectSide.TupleLength())).TupleEqual(
                    1))).TupleAnd(new HTuple((new HTuple(hv_LeftDetectSide.TupleLength())).TupleEqual(
                    1)))) != 0)
                {
                    ho_Rectangle1.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle1, 0, hv_LeftDetectSide - 10, hv_Height,
                        hv_LeftDetectSide + 10);
                    ho_Rectangle2.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle2, 0, hv_RightDetectSide - 10, hv_Height,
                        hv_RightDetectSide + 10);
                }
                HOperatorSet.SetDraw(hv_ExpDefaultWinHandle, "fill");
                HOperatorSet.SetColor(hv_ExpDefaultWinHandle, "blue");
                HOperatorSet.DispObj(ho_Rectangle1, hv_ExpDefaultWinHandle);
                HOperatorSet.DispObj(ho_Rectangle2, hv_ExpDefaultWinHandle);
                HOperatorSet.SetColor(hv_ExpDefaultWinHandle, "red");
                HOperatorSet.SetDraw(hv_ExpDefaultWinHandle, "margin");
                for (hv_j = 0; (int)hv_j <= (int)((new HTuple(hv_tupleDefectRow1.TupleLength())) - 1); hv_j = (int)hv_j + 1)
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, hv_tupleDefectRow1.TupleSelect(
                        hv_j), hv_tupleDefectColumn1.TupleSelect(hv_j), hv_tupleDefectRow2.TupleSelect(
                        hv_j), hv_tupleDefectColumn2.TupleSelect(hv_j));
                    HOperatorSet.DispObj(ho_Rectangle, hv_ExpDefaultWinHandle);
                    disp_message(hv_ExpDefaultWinHandle, hv_j + 1, "image", 0.5 * ((hv_tupleDefectRow1.TupleSelect(
                        hv_j)) + (hv_tupleDefectRow2.TupleSelect(hv_j))), hv_tupleDefectColumn2.TupleSelect(
                        hv_j), "red", "true");
                }

                for (hv_i = 0; (int)hv_i <= (int)((new HTuple(hv_tupleMessages.TupleLength())) - 1); hv_i = (int)hv_i + 1)
                {
                    disp_message(hv_ExpDefaultWinHandle, hv_tupleMessages.TupleSelect(hv_i),
                        "image", 100 + ((hv_Height / 15) * hv_i), 0, hv_tupleMessagesColor.TupleSelect(
                        hv_i), "false");
                }
                ho_ImageDump.Dispose();
                HOperatorSet.DumpWindowImage(out ho_ImageDump, hv_ExpDefaultWinHandle);
                ho_Rectangle1.Dispose();
                ho_Rectangle2.Dispose();
                ho_Rectangle.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Rectangle1.Dispose();
                ho_Rectangle2.Dispose();
                ho_Rectangle.Dispose();

               // throw HDevExpDefaultException;
            }
        }

        public void get_cloth_region1(HObject ho_Image, out HObject ho_ImageR0, out HObject ho_ImageG0,
            out HObject ho_ImageB0, out HObject ho_ImageL, out HObject ho_ImageA, out HObject ho_ImageB,
            out HObject ho_ClothRegion, HTuple hv_leftSide, HTuple hv_rightSide, HTuple hv_Width,
            HTuple hv_Height)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_MidRectangle, ho_LeftRectangle;
            HObject ho_RightRectangle, ho_Regions1 = null, ho_ConnectedRegions4 = null;
            HObject ho_SelectedRegions = null, ho_Rectangle5 = null, ho_ObjectSelected3 = null;
            HObject ho_Rectangle6 = null, ho_Region = null, ho_Rectangle1 = null;
            HObject ho_Rectangle2 = null, ho_RegionUnion1 = null, ho_RegionUnion2 = null;
            HObject ho_RegionOpening1 = null, ho_ConnectedRegions1 = null;
            HObject ho_SelectedRegions1 = null;

            // Local control variables 

            HTuple hv_Mean11 = null, hv_Deviation11 = null;
            HTuple hv_Mean12 = null, hv_Deviation12 = null, hv_Mean13 = null;
            HTuple hv_Deviation13 = null, hv_Max1 = null, hv_Mean21 = null;
            HTuple hv_Mean22 = null, hv_Mean23 = null, hv_Max2 = null;
            HTuple hv_Mean31 = null, hv_Mean32 = null, hv_Mean33 = null;
            HTuple hv_Max3 = null, hv_Number = null, hv_UsedThreshold = new HTuple();
            HTuple hv_Area = new HTuple(), hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_Mean1 = new HTuple(), hv_Deviation1 = new HTuple();
            HTuple hv_i = new HTuple(), hv_Area1 = new HTuple(), hv_Row4 = new HTuple();
            HTuple hv_Column4 = new HTuple(), hv_Mean2 = new HTuple();
            HTuple hv_Deviation2 = new HTuple(), hv_Min2 = new HTuple();
            HTuple hv_leftSide_COPY_INP_TMP = hv_leftSide.Clone();
            HTuple hv_rightSide_COPY_INP_TMP = hv_rightSide.Clone();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageR0);
            HOperatorSet.GenEmptyObj(out ho_ImageG0);
            HOperatorSet.GenEmptyObj(out ho_ImageB0);
            HOperatorSet.GenEmptyObj(out ho_ImageL);
            HOperatorSet.GenEmptyObj(out ho_ImageA);
            HOperatorSet.GenEmptyObj(out ho_ImageB);
            HOperatorSet.GenEmptyObj(out ho_ClothRegion);
            HOperatorSet.GenEmptyObj(out ho_MidRectangle);
            HOperatorSet.GenEmptyObj(out ho_LeftRectangle);
            HOperatorSet.GenEmptyObj(out ho_RightRectangle);
            HOperatorSet.GenEmptyObj(out ho_Regions1);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions4);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_Rectangle5);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelected3);
            HOperatorSet.GenEmptyObj(out ho_Rectangle6);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            HOperatorSet.GenEmptyObj(out ho_Rectangle2);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion1);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion2);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening1);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            try
            {
                if ((int)(new HTuple(hv_leftSide_COPY_INP_TMP.TupleLess(1))) != 0)
                {
                    hv_leftSide_COPY_INP_TMP = 200;
                }
                if ((int)(new HTuple(hv_rightSide_COPY_INP_TMP.TupleLess(1))) != 0)
                {
                    hv_rightSide_COPY_INP_TMP = 200;
                }

                ho_ClothRegion.Dispose();
                HOperatorSet.GenEmptyObj(out ho_ClothRegion);
                //图像分割是成RGB图
                ho_ImageR0.Dispose(); ho_ImageG0.Dispose(); ho_ImageB0.Dispose();
                HOperatorSet.Decompose3(ho_Image, out ho_ImageR0, out ho_ImageG0, out ho_ImageB0
                    );
                //将图像转换为LAB图像
                ho_ImageL.Dispose(); ho_ImageA.Dispose(); ho_ImageB.Dispose();
                HOperatorSet.TransFromRgb(ho_ImageR0, ho_ImageG0, ho_ImageB0, out ho_ImageL,
                    out ho_ImageA, out ho_ImageB, "cielab");
                //***********************************************************************************


                //***********************************************************************************
                //分别计算LAB图像布匹中间与两边的灰度差，将灰度差较大的作为布匹区域分割对象
                //***********************************************************************************
                ho_MidRectangle.Dispose();
                HOperatorSet.GenRectangle1(out ho_MidRectangle, (hv_Height / 2) - 300, (hv_Width / 2) - 300,
                    (hv_Height / 2) + 300, (hv_Width / 2) + 300);
                ho_LeftRectangle.Dispose();
                HOperatorSet.GenRectangle1(out ho_LeftRectangle, (hv_Height / 2) - 300, hv_leftSide_COPY_INP_TMP + 20,
                    (hv_Height / 2) + 300, hv_leftSide_COPY_INP_TMP + 150);
                ho_RightRectangle.Dispose();
                HOperatorSet.GenRectangle1(out ho_RightRectangle, (hv_Height / 2) - 300, (hv_Width - hv_rightSide_COPY_INP_TMP) - 150,
                    (hv_Height / 2) + 300, (hv_Width - hv_rightSide_COPY_INP_TMP) - 20);



                HOperatorSet.Intensity(ho_MidRectangle, ho_ImageL, out hv_Mean11, out hv_Deviation11);
                HOperatorSet.Intensity(ho_LeftRectangle, ho_ImageL, out hv_Mean12, out hv_Deviation12);
                HOperatorSet.Intensity(ho_RightRectangle, ho_ImageL, out hv_Mean13, out hv_Deviation13);
                HOperatorSet.TupleMax2(hv_Mean11 - hv_Mean12, hv_Mean11 - hv_Mean13, out hv_Max1);
                HOperatorSet.TupleAbs(hv_Max1, out hv_Max1);


                HOperatorSet.Intensity(ho_MidRectangle, ho_ImageA, out hv_Mean21, out hv_Deviation11);
                HOperatorSet.Intensity(ho_LeftRectangle, ho_ImageA, out hv_Mean22, out hv_Deviation12);
                HOperatorSet.Intensity(ho_RightRectangle, ho_ImageA, out hv_Mean23, out hv_Deviation13);
                HOperatorSet.TupleMax2(hv_Mean21 - hv_Mean22, hv_Mean21 - hv_Mean23, out hv_Max2);
                HOperatorSet.TupleAbs(hv_Max2, out hv_Max2);

                HOperatorSet.Intensity(ho_MidRectangle, ho_ImageB, out hv_Mean31, out hv_Deviation11);
                HOperatorSet.Intensity(ho_LeftRectangle, ho_ImageB, out hv_Mean32, out hv_Deviation12);
                HOperatorSet.Intensity(ho_RightRectangle, ho_ImageB, out hv_Mean33, out hv_Deviation13);
                HOperatorSet.TupleMax2(hv_Mean31 - hv_Mean32, hv_Mean31 - hv_Mean33, out hv_Max3);
                HOperatorSet.TupleAbs(hv_Max3, out hv_Max3);
                //***********************************************************************************

                //***********************************************************************************
                //如AB图像的灰度差<5,且L图像灰度差小于10，则未找到布匹，否则分割布匹区域，SelectedRegions1
                //***********************************************************************************
                hv_Number = 0;
                if ((int)((new HTuple((new HTuple(hv_Max2.TupleLess(5))).TupleAnd(new HTuple(hv_Max3.TupleLess(
                    5))))).TupleAnd(new HTuple(hv_Max1.TupleLess(10)))) != 0)
                {
                    ho_MidRectangle.Dispose();
                    ho_LeftRectangle.Dispose();
                    ho_RightRectangle.Dispose();
                    ho_Regions1.Dispose();
                    ho_ConnectedRegions4.Dispose();
                    ho_SelectedRegions.Dispose();
                    ho_Rectangle5.Dispose();
                    ho_ObjectSelected3.Dispose();
                    ho_Rectangle6.Dispose();
                    ho_Region.Dispose();
                    ho_Rectangle1.Dispose();
                    ho_Rectangle2.Dispose();
                    ho_RegionUnion1.Dispose();
                    ho_RegionUnion2.Dispose();
                    ho_RegionOpening1.Dispose();
                    ho_ConnectedRegions1.Dispose();
                    ho_SelectedRegions1.Dispose();

                    return;
                }
                else
                {
                    if ((int)((new HTuple(hv_Max2.TupleGreater(5))).TupleOr(new HTuple(hv_Max3.TupleGreater(
                        5)))) != 0)
                    {
                        //***********************************************************************************
                        //在AB图像中选取色差大的图，作为分割布匹区域，SelectedRegions1
                        //***********************************************************************************
                        if ((int)(new HTuple(hv_Max2.TupleGreater(hv_Max3))) != 0)
                        {

                            if ((int)(new HTuple(((hv_Mean21 - hv_Mean22)).TupleLess(0))) != 0)
                            {
                                ho_Regions1.Dispose();
                                HOperatorSet.BinaryThreshold(ho_ImageA, out ho_Regions1, "max_separability",
                                    "dark", out hv_UsedThreshold);
                            }
                            else
                            {
                                ho_Regions1.Dispose();
                                HOperatorSet.BinaryThreshold(ho_ImageA, out ho_Regions1, "max_separability",
                                    "light", out hv_UsedThreshold);
                            }

                            ho_ConnectedRegions4.Dispose();
                            HOperatorSet.Connection(ho_Regions1, out ho_ConnectedRegions4);
                            ho_Regions1.Dispose();
                            HOperatorSet.SelectShape(ho_ConnectedRegions4, out ho_Regions1, (new HTuple("height")).TupleConcat(
                                "column"), "and", ((hv_Height - 100)).TupleConcat(hv_leftSide_COPY_INP_TMP + 300),
                                ((hv_Height + 100)).TupleConcat((hv_Width - hv_rightSide_COPY_INP_TMP) - 300));
                            HOperatorSet.CountObj(ho_Regions1, out hv_Number);
                            if ((int)(new HTuple(hv_Number.TupleGreater(1))) != 0)
                            {
                                ho_SelectedRegions.Dispose();
                                HOperatorSet.SelectShapeStd(ho_Regions1, out ho_SelectedRegions, "max_area",
                                    80);
                                HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row,
                                    out hv_Column);
                                ho_Rectangle5.Dispose();
                                HOperatorSet.GenRectangle1(out ho_Rectangle5, hv_Row - 500, hv_Column - 500,
                                    hv_Row + 500, hv_Column + 500);
                                HOperatorSet.Intensity(ho_Rectangle5, ho_ImageA, out hv_Mean1, out hv_Deviation1);
                                HTuple end_val71 = hv_Number;
                                HTuple step_val71 = 1;
                                for (hv_i = 1; hv_i.Continue(end_val71, step_val71); hv_i = hv_i.TupleAdd(step_val71))
                                {
                                    ho_ObjectSelected3.Dispose();
                                    HOperatorSet.SelectObj(ho_Regions1, out ho_ObjectSelected3, hv_i);
                                    HOperatorSet.AreaCenter(ho_ObjectSelected3, out hv_Area1, out hv_Row4,
                                        out hv_Column4);
                                    if ((int)(new HTuple(hv_Area1.TupleNotEqual(hv_Area))) != 0)
                                    {
                                        ho_Rectangle6.Dispose();
                                        HOperatorSet.GenRectangle1(out ho_Rectangle6, hv_Row4 - 500, hv_Column4 - 30,
                                            hv_Row4 + 500, hv_Column4 + 30);
                                        HOperatorSet.Intensity(ho_Rectangle6, ho_ImageA, out hv_Mean2,
                                            out hv_Deviation2);
                                        if ((int)((new HTuple(((hv_Mean2 - hv_Mean1)).TupleGreater(-10))).TupleAnd(
                                            new HTuple(((hv_Mean2 - hv_Mean1)).TupleLess(10)))) != 0)
                                        {
                                            {
                                                HObject ExpTmpOutVar_0;
                                                HOperatorSet.Union2(ho_SelectedRegions, ho_ObjectSelected3, out ExpTmpOutVar_0
                                                    );
                                                ho_SelectedRegions.Dispose();
                                                ho_SelectedRegions = ExpTmpOutVar_0;
                                            }
                                        }
                                    }

                                }
                                ho_Regions1.Dispose();
                                HOperatorSet.Connection(ho_SelectedRegions, out ho_Regions1);
                            }
                        }
                        else
                        {

                            if ((int)(new HTuple(((hv_Mean31 - hv_Mean32)).TupleLess(0))) != 0)
                            {
                                ho_Regions1.Dispose();
                                HOperatorSet.BinaryThreshold(ho_ImageB, out ho_Regions1, "max_separability",
                                    "dark", out hv_UsedThreshold);
                            }
                            else
                            {
                                ho_Regions1.Dispose();
                                HOperatorSet.BinaryThreshold(ho_ImageB, out ho_Regions1, "max_separability",
                                    "light", out hv_UsedThreshold);
                            }
                            ho_ConnectedRegions4.Dispose();
                            HOperatorSet.Connection(ho_Regions1, out ho_ConnectedRegions4);
                            ho_Regions1.Dispose();
                            HOperatorSet.SelectShape(ho_ConnectedRegions4, out ho_Regions1, (new HTuple("height")).TupleConcat(
                                "column"), "and", ((hv_Height - 100)).TupleConcat(hv_leftSide_COPY_INP_TMP + 300),
                                ((hv_Height + 100)).TupleConcat((hv_Width - hv_rightSide_COPY_INP_TMP) - 300));
                            HOperatorSet.CountObj(ho_Regions1, out hv_Number);
                            if ((int)(new HTuple(hv_Number.TupleGreater(1))) != 0)
                            {
                                //***********************************************************************************
                                //如果区域个数大于1，选取与最大区域灰度接近的区域并与最大区域连到一起
                                //****************************************************************u*******************
                                ho_SelectedRegions.Dispose();
                                HOperatorSet.SelectShapeStd(ho_Regions1, out ho_SelectedRegions, "max_area",
                                    80);
                                HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row,
                                    out hv_Column);
                                ho_Rectangle5.Dispose();
                                HOperatorSet.GenRectangle1(out ho_Rectangle5, hv_Row - 500, hv_Column - 500,
                                    hv_Row + 500, hv_Column + 500);
                                HOperatorSet.Intensity(ho_Rectangle5, ho_ImageB, out hv_Mean1, out hv_Deviation1);
                                HTuple end_val103 = hv_Number;
                                HTuple step_val103 = 1;
                                for (hv_i = 1; hv_i.Continue(end_val103, step_val103); hv_i = hv_i.TupleAdd(step_val103))
                                {
                                    ho_ObjectSelected3.Dispose();
                                    HOperatorSet.SelectObj(ho_Regions1, out ho_ObjectSelected3, hv_i);
                                    HOperatorSet.AreaCenter(ho_ObjectSelected3, out hv_Area1, out hv_Row4,
                                        out hv_Column4);
                                    if ((int)(new HTuple(hv_Area1.TupleNotEqual(hv_Area))) != 0)
                                    {
                                        ho_Rectangle6.Dispose();
                                        HOperatorSet.GenRectangle1(out ho_Rectangle6, hv_Row4 - 500, hv_Column4 - 30,
                                            hv_Row4 + 500, hv_Column4 + 30);
                                        HOperatorSet.Intensity(ho_Rectangle6, ho_ImageB, out hv_Mean2,
                                            out hv_Deviation2);
                                        if ((int)((new HTuple(((hv_Mean2 - hv_Mean1)).TupleGreater(-10))).TupleAnd(
                                            new HTuple(((hv_Mean2 - hv_Mean1)).TupleLess(10)))) != 0)
                                        {
                                            {
                                                HObject ExpTmpOutVar_0;
                                                HOperatorSet.Union2(ho_SelectedRegions, ho_ObjectSelected3, out ExpTmpOutVar_0
                                                    );
                                                ho_SelectedRegions.Dispose();
                                                ho_SelectedRegions = ExpTmpOutVar_0;
                                            }
                                        }
                                    }

                                }
                                ho_Regions1.Dispose();
                                HOperatorSet.Connection(ho_SelectedRegions, out ho_Regions1);
                            }

                        }

                    }
                    else
                    {
                        HOperatorSet.TupleMin2(hv_Mean12, hv_Mean13, out hv_Min2);
                        HOperatorSet.TupleMax2(hv_Mean12, hv_Mean13, out hv_Max2);
                        if ((int)(new HTuple(hv_Mean11.TupleLess(hv_Max2))) != 0)
                        {
                            if ((int)(new HTuple(hv_Max1.TupleGreater(40))) != 0)
                            {
                                ho_Region.Dispose();
                                HOperatorSet.Threshold(ho_ImageL, out ho_Region, 10, hv_Max2 - 20);
                            }
                            else if ((int)(new HTuple(hv_Max1.TupleGreater(30))) != 0)
                            {
                                ho_Region.Dispose();
                                HOperatorSet.Threshold(ho_ImageL, out ho_Region, 10, hv_Max2 - 10);
                            }
                            else
                            {
                                ho_Region.Dispose();
                                HOperatorSet.Threshold(ho_ImageL, out ho_Region, 10, 0.5 * (hv_Max2 + hv_Mean11));
                            }
                        }
                        else
                        {
                            if ((int)(new HTuple(hv_Max1.TupleGreater(40))) != 0)
                            {
                                ho_Region.Dispose();
                                HOperatorSet.Threshold(ho_ImageL, out ho_Region, hv_Max2 + 25, 255);
                            }
                            else if ((int)(new HTuple(hv_Max1.TupleGreater(30))) != 0)
                            {
                                ho_Region.Dispose();
                                HOperatorSet.Threshold(ho_ImageL, out ho_Region, hv_Max2 + 18, 255);
                            }
                            else
                            {
                                ho_Region.Dispose();
                                HOperatorSet.Threshold(ho_ImageL, out ho_Region, 0.5 * (hv_Max2 + hv_Mean11),
                                    255);
                            }

                        }

                        ho_ConnectedRegions4.Dispose();
                        HOperatorSet.Connection(ho_Region, out ho_ConnectedRegions4);
                        ho_Regions1.Dispose();
                        HOperatorSet.SelectShape(ho_ConnectedRegions4, out ho_Regions1, (new HTuple("height")).TupleConcat(
                            "column"), "and", ((hv_Height - 100)).TupleConcat(hv_leftSide_COPY_INP_TMP + 100),
                            ((hv_Height + 100)).TupleConcat((hv_Width - hv_rightSide_COPY_INP_TMP) - 100));
                        HOperatorSet.CountObj(ho_Regions1, out hv_Number);
                        if ((int)(new HTuple(hv_Number.TupleGreater(1))) != 0)
                        {
                            ho_SelectedRegions.Dispose();
                            HOperatorSet.SelectShapeStd(ho_Regions1, out ho_SelectedRegions, "max_area",
                                80);
                            HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row,
                                out hv_Column);
                            ho_Rectangle5.Dispose();
                            HOperatorSet.GenRectangle1(out ho_Rectangle5, hv_Row - 500, hv_Column - 500,
                                hv_Row + 500, hv_Column + 500);
                            HOperatorSet.Intensity(ho_Rectangle5, ho_ImageL, out hv_Mean1, out hv_Deviation1);
                            HTuple end_val150 = hv_Number;
                            HTuple step_val150 = 1;
                            for (hv_i = 1; hv_i.Continue(end_val150, step_val150); hv_i = hv_i.TupleAdd(step_val150))
                            {
                                ho_ObjectSelected3.Dispose();
                                HOperatorSet.SelectObj(ho_Regions1, out ho_ObjectSelected3, hv_i);
                                HOperatorSet.AreaCenter(ho_ObjectSelected3, out hv_Area1, out hv_Row4,
                                    out hv_Column4);
                                if ((int)(new HTuple(hv_Area1.TupleNotEqual(hv_Area))) != 0)
                                {
                                    ho_Rectangle6.Dispose();
                                    HOperatorSet.GenRectangle1(out ho_Rectangle6, hv_Row4 - 500, hv_Column4 - 30,
                                        hv_Row4 + 500, hv_Column4 + 30);
                                    HOperatorSet.Intensity(ho_Rectangle6, ho_ImageL, out hv_Mean2, out hv_Deviation2);
                                    if ((int)((new HTuple(((hv_Mean2 - hv_Mean1)).TupleGreater(-10))).TupleAnd(
                                        new HTuple(((hv_Mean2 - hv_Mean1)).TupleLess(10)))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_SelectedRegions, ho_ObjectSelected3, out ExpTmpOutVar_0
                                                );
                                            ho_SelectedRegions.Dispose();
                                            ho_SelectedRegions = ExpTmpOutVar_0;
                                        }
                                    }
                                }
                            }
                            ho_Regions1.Dispose();
                            HOperatorSet.Connection(ho_SelectedRegions, out ho_Regions1);
                        }

                    }

                    //***********************************************************************************
                    //防止周期性缺陷
                    //***********************************************************************************
                    ho_Rectangle1.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle1, 1, 50 + hv_leftSide_COPY_INP_TMP,
                        3, (hv_Width - hv_rightSide_COPY_INP_TMP) - 50);
                    ho_Rectangle2.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle2, hv_Height - 3, 50 + hv_leftSide_COPY_INP_TMP,
                        hv_Height, (hv_Width - hv_rightSide_COPY_INP_TMP) - 50);
                    ho_RegionUnion1.Dispose();
                    HOperatorSet.Union2(ho_Rectangle1, ho_Rectangle2, out ho_RegionUnion1);
                    ho_RegionUnion2.Dispose();
                    HOperatorSet.Union2(ho_RegionUnion1, ho_Regions1, out ho_RegionUnion2);
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.FillUp(ho_RegionUnion2, out ExpTmpOutVar_0);
                        ho_RegionUnion2.Dispose();
                        ho_RegionUnion2 = ExpTmpOutVar_0;
                    }
                    ho_RegionOpening1.Dispose();
                    HOperatorSet.OpeningRectangle1(ho_RegionUnion2, out ho_RegionOpening1, 1,
                        7);
                    //***********************************************************************************
                    ho_ConnectedRegions1.Dispose();
                    HOperatorSet.Connection(ho_RegionOpening1, out ho_ConnectedRegions1);
                    ho_SelectedRegions1.Dispose();
                    HOperatorSet.SelectShape(ho_ConnectedRegions1, out ho_SelectedRegions1, (new HTuple("height")).TupleConcat(
                        "column"), "and", ((hv_Height - 100)).TupleConcat(hv_leftSide_COPY_INP_TMP + 300),
                        ((hv_Height + 100)).TupleConcat((hv_Width - hv_rightSide_COPY_INP_TMP) - 300));
                    HOperatorSet.CountObj(ho_SelectedRegions1, out hv_Number);
                }
                //***********************************************************************************
                ho_ClothRegion.Dispose();
                HOperatorSet.FillUp(ho_SelectedRegions1, out ho_ClothRegion);
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ClosingRectangle1(ho_ClothRegion, out ExpTmpOutVar_0, 10, 60);
                    ho_ClothRegion.Dispose();
                    ho_ClothRegion = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.OpeningRectangle1(ho_ClothRegion, out ExpTmpOutVar_0, 1, 200);
                    ho_ClothRegion.Dispose();
                    ho_ClothRegion = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ClosingRectangle1(ho_ClothRegion, out ExpTmpOutVar_0, 60, 20);
                    ho_ClothRegion.Dispose();
                    ho_ClothRegion = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.OpeningRectangle1(ho_ClothRegion, out ExpTmpOutVar_0, 1, 10);
                    ho_ClothRegion.Dispose();
                    ho_ClothRegion = ExpTmpOutVar_0;
                }

                ho_MidRectangle.Dispose();
                ho_LeftRectangle.Dispose();
                ho_RightRectangle.Dispose();
                ho_Regions1.Dispose();
                ho_ConnectedRegions4.Dispose();
                ho_SelectedRegions.Dispose();
                ho_Rectangle5.Dispose();
                ho_ObjectSelected3.Dispose();
                ho_Rectangle6.Dispose();
                ho_Region.Dispose();
                ho_Rectangle1.Dispose();
                ho_Rectangle2.Dispose();
                ho_RegionUnion1.Dispose();
                ho_RegionUnion2.Dispose();
                ho_RegionOpening1.Dispose();
                ho_ConnectedRegions1.Dispose();
                ho_SelectedRegions1.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_MidRectangle.Dispose();
                ho_LeftRectangle.Dispose();
                ho_RightRectangle.Dispose();
                ho_Regions1.Dispose();
                ho_ConnectedRegions4.Dispose();
                ho_SelectedRegions.Dispose();
                ho_Rectangle5.Dispose();
                ho_ObjectSelected3.Dispose();
                ho_Rectangle6.Dispose();
                ho_Region.Dispose();
                ho_Rectangle1.Dispose();
                ho_Rectangle2.Dispose();
                ho_RegionUnion1.Dispose();
                ho_RegionUnion2.Dispose();
                ho_RegionOpening1.Dispose();
                ho_ConnectedRegions1.Dispose();
                ho_SelectedRegions1.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void get_boxes_color(HObject ho_Image, HObject ho_ClothRegion, out HObject ho_Boxs,
            HTuple hv_boxNumber, HTuple hv_boxBenginX, HTuple hv_boxWidth, HTuple hv_boxHeight,
            HTuple hv_Width, HTuple hv_Height, HTuple hv_medianKernal, out HTuple hv_standardTupleL,
            out HTuple hv_standardTupleA, out HTuple hv_standardTupleB)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_Rectangle = null, ho_BoxImage = null;
            HObject ho_BoxImagedMedian = null, ho_Region1 = null, ho_BoxImagedMedianValid = null;
            HObject ho_BoxImageR0 = null, ho_BoxImageG0 = null, ho_BoxImageB0 = null;
            HObject ho_BoxImageL = null, ho_BoxImageA = null, ho_BoxImageB = null;

            // Local control variables 

            HTuple hv_Row1 = null, hv_Column1 = null, hv_Row2 = null;
            HTuple hv_Column2 = null, hv_message = new HTuple(), hv_tupleMessages = new HTuple();
            HTuple hv_color = new HTuple(), hv_tupleMessagesColor = new HTuple();
            HTuple hv_boxDistance = new HTuple(), hv_boxBenginY = new HTuple();
            HTuple hv_tupleDeviationL = new HTuple(), hv_tupleDeviationA = new HTuple();
            HTuple hv_tupleDeviationB = new HTuple(), hv_i = new HTuple();
            HTuple hv_Seconds = new HTuple(), hv_MeanL = new HTuple();
            HTuple hv_DeviationL = new HTuple(), hv_MeanA = new HTuple();
            HTuple hv_DeviationA = new HTuple(), hv_MeanB = new HTuple();
            HTuple hv_DeviationB = new HTuple(), hv_standardTupleL1 = new HTuple();
            HTuple hv_standardTupleA1 = new HTuple(), hv_standardTupleB1 = new HTuple();
            HTuple hv_totalL = new HTuple(), hv_totalA = new HTuple();
            HTuple hv_totalB = new HTuple(), hv_k = new HTuple(), hv_standardL = new HTuple();
            HTuple hv_standardA = new HTuple(), hv_standardB = new HTuple();
            HTuple hv_Width_COPY_INP_TMP = hv_Width.Clone();
            HTuple hv_boxBenginX_COPY_INP_TMP = hv_boxBenginX.Clone();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Boxs);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_BoxImage);
            HOperatorSet.GenEmptyObj(out ho_BoxImagedMedian);
            HOperatorSet.GenEmptyObj(out ho_Region1);
            HOperatorSet.GenEmptyObj(out ho_BoxImagedMedianValid);
            HOperatorSet.GenEmptyObj(out ho_BoxImageR0);
            HOperatorSet.GenEmptyObj(out ho_BoxImageG0);
            HOperatorSet.GenEmptyObj(out ho_BoxImageB0);
            HOperatorSet.GenEmptyObj(out ho_BoxImageL);
            HOperatorSet.GenEmptyObj(out ho_BoxImageA);
            HOperatorSet.GenEmptyObj(out ho_BoxImageB);
            hv_standardTupleL = new HTuple();
            hv_standardTupleA = new HTuple();
            hv_standardTupleB = new HTuple();
            try
            {

                HOperatorSet.SmallestRectangle1(ho_ClothRegion, out hv_Row1, out hv_Column1,
                    out hv_Row2, out hv_Column2);
                ho_Boxs.Dispose();
                HOperatorSet.GenEmptyObj(out ho_Boxs);
                if ((int)(new HTuple(hv_boxNumber.TupleLessEqual(0))) != 0)
                {
                    //检测框个数必须大于0
                    hv_message = "检测框个数必须大于0";
                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    hv_color = "red";
                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                }
                else
                {
                    //标准色差检测
                    //boxDistance表示两个框间距

                    hv_Width_COPY_INP_TMP = hv_Column2 - hv_Column1;
                    hv_boxDistance = ((hv_Width_COPY_INP_TMP - (hv_boxNumber * hv_boxWidth)) - (hv_boxBenginX_COPY_INP_TMP * 2)) / (hv_boxNumber - 1);
                    hv_boxBenginX_COPY_INP_TMP = hv_boxBenginX_COPY_INP_TMP + hv_Column1;
                    //，boxBenginY框起始y坐标
                    hv_boxBenginY = (hv_Height - hv_boxHeight) / 2;
                    hv_tupleDeviationL = new HTuple();
                    hv_tupleDeviationA = new HTuple();
                    hv_tupleDeviationB = new HTuple();
                    hv_standardTupleL = new HTuple();
                    hv_standardTupleA = new HTuple();
                    hv_standardTupleB = new HTuple();
                    //threadName := []
                    //tuple_gen_const (boxNumber, 0, standardTupleL)
                    //tuple_gen_const (boxNumber, 0, standardTupleA)
                    //tuple_gen_const (boxNumber, 0, standardTupleB)
                    //tuple_gen_const (boxNumber, 0, threadName)

                    //for i := 0 to boxNumber-1 by 1
                    //tuple_concat (threadName, 'thread'+i, threadName)
                    //endfor
                    HOperatorSet.CountSeconds(out hv_Seconds);
                    //定义检测框
                    ho_Boxs.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_Boxs);
                    HTuple end_val36 = hv_boxNumber - 1;
                    HTuple step_val36 = 1;
                    for (hv_i = 0; hv_i.Continue(end_val36, step_val36); hv_i = hv_i.TupleAdd(step_val36))
                    {

                        ho_Rectangle.Dispose();
                        HOperatorSet.GenRectangle1(out ho_Rectangle, hv_boxBenginY, hv_boxBenginX_COPY_INP_TMP + (hv_i * (hv_boxWidth + hv_boxDistance)),
                            hv_boxBenginY + hv_boxHeight, (hv_boxBenginX_COPY_INP_TMP + (hv_i * (hv_boxWidth + hv_boxDistance))) + hv_boxWidth);
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.Union2(ho_Boxs, ho_Rectangle, out ExpTmpOutVar_0);
                            ho_Boxs.Dispose();
                            ho_Boxs = ExpTmpOutVar_0;
                        }
                        ho_BoxImage.Dispose();
                        HOperatorSet.ReduceDomain(ho_Image, ho_Rectangle, out ho_BoxImage);

                        //中值滤波
                        ho_BoxImagedMedian.Dispose();
                        HOperatorSet.MedianImage(ho_BoxImage, out ho_BoxImagedMedian, "circle",
                            hv_medianKernal, "mirrored");
                        //median_image (BoxImage, BoxImagedMedian, 'circle', 1, 'mirrored')
                        //动态阈值，并选取区域最大
                        ho_Region1.Dispose();
                        HOperatorSet.VarThreshold(ho_BoxImagedMedian, out ho_Region1, 25, 25, 0.1,
                            1, "equal");
                        ho_BoxImagedMedianValid.Dispose();
                        HOperatorSet.ReduceDomain(ho_BoxImagedMedian, ho_Region1, out ho_BoxImagedMedianValid
                            );

                        ho_BoxImageR0.Dispose(); ho_BoxImageG0.Dispose(); ho_BoxImageB0.Dispose();
                        HOperatorSet.Decompose3(ho_BoxImagedMedianValid, out ho_BoxImageR0, out ho_BoxImageG0,
                            out ho_BoxImageB0);
                        ho_BoxImageL.Dispose(); ho_BoxImageA.Dispose(); ho_BoxImageB.Dispose();
                        HOperatorSet.TransFromRgb(ho_BoxImageR0, ho_BoxImageG0, ho_BoxImageB0,
                            out ho_BoxImageL, out ho_BoxImageA, out ho_BoxImageB, "cielab");

                        HOperatorSet.Intensity(ho_Rectangle, ho_BoxImageL, out hv_MeanL, out hv_DeviationL);
                        HOperatorSet.Intensity(ho_Rectangle, ho_BoxImageA, out hv_MeanA, out hv_DeviationA);
                        HOperatorSet.Intensity(ho_Rectangle, ho_BoxImageB, out hv_MeanB, out hv_DeviationB);
                        if (hv_standardTupleL == null)
                            hv_standardTupleL = new HTuple();
                        hv_standardTupleL[hv_i] = hv_MeanL;
                        if (hv_standardTupleA == null)
                            hv_standardTupleA = new HTuple();
                        hv_standardTupleA[hv_i] = hv_MeanA;
                        if (hv_standardTupleB == null)
                            hv_standardTupleB = new HTuple();
                        hv_standardTupleB[hv_i] = hv_MeanB;
                        if (hv_tupleDeviationL == null)
                            hv_tupleDeviationL = new HTuple();
                        hv_tupleDeviationL[hv_i] = hv_DeviationL;
                        if (hv_tupleDeviationA == null)
                            hv_tupleDeviationA = new HTuple();
                        hv_tupleDeviationA[hv_i] = hv_DeviationA;
                        if (hv_tupleDeviationB == null)
                            hv_tupleDeviationB = new HTuple();
                        hv_tupleDeviationB[hv_i] = hv_DeviationB;
                        //gen_rectangle1 (Rectangle, boxBenginY, boxBenginX+i*(boxWidth+boxDistance), boxBenginY+boxHeight, boxBenginX+i*(boxWidth+boxDistance)+boxWidth)
                        //union2 (Boxs, Rectangle, Boxs)
                        //reduce_domain (Image, Rectangle, BoxImage)



                        //par_start<ThreadID> : get_boxes_lab_use_multi_threads (Image, Rectangle, medianKernal, i, standardTupleL, standardTupleA, standardTupleB, standardTupleL, standardTupleA, standardTupleB)
                        //        threadName[i]:=theThreadname par_start<theThreadname> :
                        //par_join (ThreadID)
                    }
                    //count_seconds (Seconds1)
                    //Seconds2 := Seconds1-Seconds
                    //ThreadIDs := []
                    //for i := 0 to boxNumber-1 by 1
                    //tuple_concat (ThreadIDs, threadName[i], aaa)
                    //endfor

                    //par_join (ThreadIDs)

                    //***********************************************************************************
                    //去掉最大、最小的，求取标准色差
                    //***********************************************************************************
                    HOperatorSet.TupleSort(hv_standardTupleL, out hv_standardTupleL1);
                    HOperatorSet.TupleSort(hv_standardTupleA, out hv_standardTupleA1);
                    HOperatorSet.TupleSort(hv_standardTupleB, out hv_standardTupleB1);
                    if ((int)(new HTuple(hv_boxNumber.TupleGreater(2))) != 0)
                    {
                        hv_totalL = 0;
                        hv_totalA = 0;
                        hv_totalB = 0;
                        HTuple end_val90 = hv_boxNumber - 2;
                        HTuple step_val90 = 1;
                        for (hv_k = 1; hv_k.Continue(end_val90, step_val90); hv_k = hv_k.TupleAdd(step_val90))
                        {
                            hv_totalL = hv_totalL + (hv_standardTupleL1.TupleSelect(hv_k));
                            hv_totalA = hv_totalA + (hv_standardTupleA1.TupleSelect(hv_k));
                            hv_totalB = hv_totalB + (hv_standardTupleB1.TupleSelect(hv_k));
                        }
                        hv_standardL = hv_totalL / (hv_boxNumber - 2);
                        hv_standardA = hv_totalA / (hv_boxNumber - 2);
                        hv_standardB = hv_totalB / (hv_boxNumber - 2);
                    }
                    else
                    {
                        hv_standardL = hv_standardTupleL1.TupleSelect(0);
                        hv_standardA = hv_standardTupleA1.TupleSelect(0);
                        hv_standardB = hv_standardTupleB1.TupleSelect(0);
                    }
                }


                ho_Rectangle.Dispose();
                ho_BoxImage.Dispose();
                ho_BoxImagedMedian.Dispose();
                ho_Region1.Dispose();
                ho_BoxImagedMedianValid.Dispose();
                ho_BoxImageR0.Dispose();
                ho_BoxImageG0.Dispose();
                ho_BoxImageB0.Dispose();
                ho_BoxImageL.Dispose();
                ho_BoxImageA.Dispose();
                ho_BoxImageB.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Rectangle.Dispose();
                ho_BoxImage.Dispose();
                ho_BoxImagedMedian.Dispose();
                ho_Region1.Dispose();
                ho_BoxImagedMedianValid.Dispose();
                ho_BoxImageR0.Dispose();
                ho_BoxImageG0.Dispose();
                ho_BoxImageB0.Dispose();
                ho_BoxImageL.Dispose();
                ho_BoxImageA.Dispose();
                ho_BoxImageB.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void detect_defects_fun(HObject ho_GrayImage, HObject ho_ClothRegion, HObject ho_ValidClothRegion,
            HTuple hv_tupleMessagesColor1, HTuple hv_tupleMessages1, HTuple hv_detectDefectsFlag,
            HTuple hv_edgeRollFlag, HTuple hv_imperfectBorderFlag, HTuple hv_otherFlag,
            HTuple hv_edgeRollSlope, HTuple hv_Width, HTuple hv_Height, HTuple hv_magnification,
            HTuple hv_imperfectBorderWidth, HTuple hv_thresh, HTuple hv_defectArea, HTuple hv_defectWidth,
            HTuple hv_defectHeight, HTuple hv_zhezhouDetectflag, out HTuple hv_minWidth,
            out HTuple hv_maxWidth, out HTuple hv_meanWidth, out HTuple hv_tupleMessages,
            out HTuple hv_tupleMessagesColor, out HTuple hv_tupleDefectColumn1, out HTuple hv_tupleDefectColumn2,
            out HTuple hv_tupleDefectRow1, out HTuple hv_tupleDefectRow2, out HTuple hv_tupleDefectClass)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_Contours = null, ho_ContoursSplit = null;
            HObject ho_UnionContours = null, ho_SelectedContours1 = null;
            HObject ho_SelectedContours2 = null, ho_ObjectsConcat = null;
            HObject ho_SelectedContours = null, ho_ObjectSelected1 = null;
            HObject ho_ObjectSelected = null, ho_Rectangle1 = null, ho_RegionIntersection1 = null;
            HObject ho_RegionTrans = null, ho_RegionTrans1 = null, ho_RegionDifference = null;
            HObject ho_RegionOpening = null, ho_ConnectedRegions = null;
            HObject ho_SelectedRegions2 = null, ho_RegionUnion3 = null,ho_Regions=null;
            HObject ho_RegionDilation1 = null, ho_ImageReduced = null, ho_MidRectangle = null;
            HObject ho_ImageReduced1 = null, ho_LightRegion = null, ho_LowGrayRegion = null;
            HObject ho_RegionUnion5 = null, ho_RegionOpening3 = null, ho_EmptyRegion = null;
            HObject ho_VeryLowGrayRegion = null, ho_VeryRegion = null, ho_ImageMean2 = null;
            HObject ho_ImageMean1 = null, ho_RegionUnion6 = null, ho_RegionOpening1 = null;
            HObject ho_ConnectedRegions1 = null, ho_SelectedRegions = null;
            HObject ho_RegionUnion1 = null, ho_RegionDilation2 = null, ho_ConnectedRegions2 = null;
            HObject ho_ConnectedRegions7 = null, ho_RegionIntersection5 = null;
            HObject ho_ConnectedRegions5 = null, ho_SelectedRegions5 = null;
            HObject ho_RegionUnion = null, ho_RegionDilation = null;


            HObject ho_ImageMean = null;
            HObject ho_RegionDynThresh1 = null;
            HObject ho_SelectedRegions1 = null, ho_RegionDynThresh2 = null;
      


            // Local control variables 

            HTuple hv_tupleDetectResult = null, hv_tupleDefectRadius = null;
            HTuple hv_tupleDefectX = null, hv_tupleDefectY = null;
            HTuple hv_defectLoactionNumber = null, hv_leftDetectSide = null;
            HTuple hv_rightDetectSide = null, hv_defectNumber = null;
            HTuple hv_Row1 = new HTuple(), hv_Column1 = new HTuple();
            HTuple hv_Row2 = new HTuple(), hv_Column2 = new HTuple();
            HTuple hv_Number2 = new HTuple(), hv_result = new HTuple();
            HTuple hv_Newtuple = new HTuple(), hv_tempMessage = new HTuple();
            HTuple hv_i = new HTuple(), hv_Row14 = new HTuple(), hv_Column14 = new HTuple();
            HTuple hv_Row24 = new HTuple(), hv_Column24 = new HTuple();
            HTuple hv_AreaTemp = new HTuple(), hv_RowTemp = new HTuple();
            HTuple hv_ColumnTemp = new HTuple(), hv_message = new HTuple();
            HTuple hv_color = new HTuple(), hv_Area3 = new HTuple();
            HTuple hv_Row3 = new HTuple(), hv_Column3 = new HTuple();
            HTuple hv_in = new HTuple(), hv_Row13 = new HTuple(), hv_Column13 = new HTuple();
            HTuple hv_Row23 = new HTuple(), hv_Column23 = new HTuple();
            HTuple hv_AbsoluteHisto = new HTuple(), hv_RelativeHisto = new HTuple();
            HTuple hv_minGrayValue = new HTuple(), hv_maxgrayValue = new HTuple();
            HTuple hv_maxGrayValue = new HTuple(), hv_Energy = new HTuple();
            HTuple hv_Correlation = new HTuple(), hv_Homogeneity = new HTuple();
            HTuple hv_Contrast = new HTuple(), hv_Mean = new HTuple();
            HTuple hv_Deviation = new HTuple(), hv_Area5 = new HTuple();
            HTuple hv_Row5 = new HTuple(), hv_Column5 = new HTuple();
            HTuple hv_Indices = new HTuple(), hv_Num = new HTuple();
            HTuple hv_j = new HTuple(), hv_DefectRow = new HTuple();
            HTuple hv_DefectColumn = new HTuple(), hv_DefectRadius = new HTuple();
            // Initialize local and output iconic variables 



            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_ImageMean);

            HOperatorSet.GenEmptyObj(out ho_RegionDynThresh1);

            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);

            HOperatorSet.GenEmptyObj(out ho_RegionDynThresh2);
            HOperatorSet.GenEmptyObj(out ho_Contours);
            HOperatorSet.GenEmptyObj(out ho_ContoursSplit);
            HOperatorSet.GenEmptyObj(out ho_UnionContours);
            HOperatorSet.GenEmptyObj(out ho_SelectedContours1);
            HOperatorSet.GenEmptyObj(out ho_SelectedContours2);
            HOperatorSet.GenEmptyObj(out ho_ObjectsConcat);
            HOperatorSet.GenEmptyObj(out ho_SelectedContours);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelected1);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelected);
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            HOperatorSet.GenEmptyObj(out ho_RegionIntersection1);
            HOperatorSet.GenEmptyObj(out ho_RegionTrans);
            HOperatorSet.GenEmptyObj(out ho_RegionTrans1);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions2);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion3);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation1);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_MidRectangle);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced1);
            HOperatorSet.GenEmptyObj(out ho_LightRegion);
            HOperatorSet.GenEmptyObj(out ho_LowGrayRegion);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion5);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening3);
            HOperatorSet.GenEmptyObj(out ho_EmptyRegion);
            HOperatorSet.GenEmptyObj(out ho_VeryLowGrayRegion);
            HOperatorSet.GenEmptyObj(out ho_VeryRegion);
            HOperatorSet.GenEmptyObj(out ho_ImageMean2);
            HOperatorSet.GenEmptyObj(out ho_ImageMean1);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion6);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening1);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion1);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation2);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions2);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions7);
            HOperatorSet.GenEmptyObj(out ho_RegionIntersection5);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions5);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions5);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation);
            try
            {
                hv_tupleDetectResult = new HTuple();
                hv_tupleDetectResult[0] = 0;
                hv_tupleDetectResult[1] = 0;
                hv_tupleDetectResult[2] = 0;
                hv_tupleDetectResult[3] = 0;
                hv_tupleDetectResult[4] = 0;
                hv_tupleDetectResult[5] = 0;
                hv_tupleDetectResult[6] = 0;
                hv_tupleDetectResult[7] = 0;
                hv_tupleDetectResult[8] = 0;
                hv_tupleDetectResult[9] = 0;
                //瑕疵半径
                hv_tupleDefectRadius = new HTuple();
                //瑕疵X坐标
                hv_tupleDefectX = new HTuple();
                //瑕疵Y坐标
                hv_tupleDefectY = new HTuple();
                //tupleDefectClass表示瑕疵分类,0表示接，1表示周期性缺陷，2表示卷边，3表示缺边，4表示其他瑕疵
                hv_tupleDefectClass = new HTuple();
                //defectLoactionNumber缺陷位置编号
                hv_defectLoactionNumber = 1;
                hv_minWidth = 0;
                hv_maxWidth = 0;
                hv_meanWidth = 0;

                //leftDetectSide、rightDetectSide
                hv_leftDetectSide = 0;
                hv_rightDetectSide = 0;
                //输出结果
                hv_tupleMessages = new HTuple();
                hv_tupleMessagesColor = new HTuple();
                //检测缺陷的个数
                hv_defectNumber = 0;
                //缺陷框坐标
                hv_tupleDefectRow1 = new HTuple();
                hv_tupleDefectRow2 = new HTuple();
                hv_tupleDefectColumn1 = new HTuple();
                hv_tupleDefectColumn2 = new HTuple();
                hv_tupleMessagesColor = hv_tupleMessagesColor1.Clone();
                hv_tupleMessages = hv_tupleMessages1.Clone();

                //**************卷边检测
                if ((int)((new HTuple(hv_detectDefectsFlag.TupleGreater(0))).TupleAnd(new HTuple(hv_edgeRollFlag.TupleGreater(
                    0)))) != 0)
                {
                    //检测布匹两边的斜率，判断其是否卷边，如卷边，则不铺宽度不求取
                    //***********************************************************************************
                    HOperatorSet.SmallestRectangle1(ho_ClothRegion, out hv_Row1, out hv_Column1,
                        out hv_Row2, out hv_Column2);
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_ClothRegion, out ho_Contours, "border");
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_Contours, out ho_ContoursSplit, "lines_circles",
                        40, 20, 1);
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionCollinearContoursXld(ho_ContoursSplit, out ho_UnionContours,
                        20, 10, 10, 0.2, "attr_keep");
                    ho_SelectedContours1.Dispose();
                    HOperatorSet.SelectContoursXld(ho_UnionContours, out ho_SelectedContours1,
                        "direction", 1.17, 1.57 - hv_edgeRollSlope, -0.5, 0.5);
                    ho_SelectedContours2.Dispose();
                    HOperatorSet.SelectContoursXld(ho_UnionContours, out ho_SelectedContours2,
                        "direction", 1.57 + hv_edgeRollSlope, 1.97, -0.5, 0.5);
                    ho_ObjectsConcat.Dispose();
                    HOperatorSet.ConcatObj(ho_SelectedContours1, ho_SelectedContours2, out ho_ObjectsConcat
                        );
                    ho_SelectedContours.Dispose();
                    HOperatorSet.SelectContoursXld(ho_ObjectsConcat, out ho_SelectedContours,
                        "contour_length", 100, 20000, -0.5, 0.5);
                    HOperatorSet.CountObj(ho_SelectedContours, out hv_Number2);
                    //**********************************************************************************
                    hv_tempMessage = hv_defectLoactionNumber.Clone();

                    if ((int)(new HTuple(hv_Number2.TupleGreater(0))) != 0)
                    {
                        hv_result = 4;
                        //卷边或者布匹偏移++
                        if (hv_tupleDetectResult == null)
                            hv_tupleDetectResult = new HTuple();
                        hv_tupleDetectResult[2] = (hv_tupleDetectResult.TupleSelect(2)) + hv_Number2;
                        HOperatorSet.TupleGenConst(hv_Number2, 2, out hv_Newtuple);
                        HOperatorSet.TupleConcat(hv_tupleDefectClass, hv_Newtuple, out hv_tupleDefectClass);
                        hv_defectNumber = hv_defectNumber + hv_Number2;
                      
                        HTuple end_val53 = hv_Number2;
                        HTuple step_val53 = 1;
                        for (hv_i = 1; hv_i.Continue(end_val53, step_val53); hv_i = hv_i.TupleAdd(step_val53))
                        {
                            ho_ObjectSelected1.Dispose();
                            HOperatorSet.SelectObj(ho_SelectedContours, out ho_ObjectSelected1, hv_i);
                            ho_ObjectSelected.Dispose();
                            HOperatorSet.GenRegionContourXld(ho_ObjectSelected1, out ho_ObjectSelected,
                                "filled");
                            HOperatorSet.SmallestRectangle1(ho_ObjectSelected, out hv_Row14, out hv_Column14,
                                out hv_Row24, out hv_Column24);
                            //画缺陷框
                            HOperatorSet.AreaCenter(ho_ObjectSelected, out hv_AreaTemp, out hv_RowTemp,
                                out hv_ColumnTemp);
                            HOperatorSet.TupleConcat(hv_tupleDefectX, hv_ColumnTemp, out hv_tupleDefectX);
                            HOperatorSet.TupleConcat(hv_tupleDefectY, hv_RowTemp, out hv_tupleDefectY);
                            HOperatorSet.TupleConcat(hv_tupleDefectRow1, ((hv_Row14 - 100)).TupleMax2(
                                0), out hv_tupleDefectRow1);
                            HOperatorSet.TupleConcat(hv_tupleDefectRow2, ((hv_Row24 + 100)).TupleMin2(
                                hv_Height), out hv_tupleDefectRow2);
                            HOperatorSet.TupleConcat(hv_tupleDefectColumn1, hv_Column14 - 100, out hv_tupleDefectColumn1);
                            HOperatorSet.TupleConcat(hv_tupleDefectColumn2, hv_Column24 + 100, out hv_tupleDefectColumn2);
                            hv_tupleDefectX = hv_ColumnTemp.Clone();
                            hv_tupleDefectY = hv_RowTemp.Clone();
                            hv_defectLoactionNumber = hv_defectLoactionNumber + 1;
                            //在缺陷框右边标注编号
                        }
                        hv_message = "卷边缺陷，编号：" + hv_tempMessage;
                        if ((int)(new HTuple(hv_Number2.TupleEqual(1))) != 0)
                        {
                            hv_message = "卷边缺陷，编号：" + hv_tempMessage;
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "red";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                        }
                        else
                        {
                            hv_message = (("卷边缺陷，编号：" + hv_tempMessage) + "-") + (hv_defectLoactionNumber - 1);
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "red";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                        }
                    }
                    else
                    {
                        //**没有卷边计算平均宽度
                        //通过布匹区域的面积以及高度，算出平均宽度
                        HOperatorSet.AreaCenter(ho_ClothRegion, out hv_Area3, out hv_Row3, out hv_Column3);
                        hv_meanWidth = hv_Area3 / (hv_Row2 - hv_Row1);

                        //***********************************************************************************
                        //将布匹分成10份，并求取最小宽度，最大宽度，平均宽度
                        //***********************************************************************************
                        for (hv_in = 0; (int)hv_in <= 19; hv_in = (int)hv_in + 4)
                        {
                            ho_Rectangle1.Dispose();
                            HOperatorSet.GenRectangle1(out ho_Rectangle1, (hv_in * (hv_Row2 - hv_Row1)) / 20,
                                1, ((hv_in + 1) * (hv_Row2 - hv_Row1)) / 20, hv_Width);
                            ho_RegionIntersection1.Dispose();
                            HOperatorSet.Intersection(ho_Rectangle1, ho_ClothRegion, out ho_RegionIntersection1
                                );
                            HOperatorSet.SmallestRectangle1(ho_RegionIntersection1, out hv_Row13,
                                out hv_Column13, out hv_Row23, out hv_Column23);
                            if ((int)(new HTuple(hv_minWidth.TupleEqual(0))) != 0)
                            {
                                hv_minWidth = hv_Column23 - hv_Column13;
                                hv_maxWidth = hv_Column23 - hv_Column13;
                            }
                            else
                            {
                                if ((int)(new HTuple(((hv_Column23 - hv_Column13)).TupleGreater(hv_maxWidth))) != 0)
                                {
                                    hv_maxWidth = hv_Column23 - hv_Column13;
                                }
                                if ((int)(new HTuple(((hv_Column23 - hv_Column13)).TupleLess(hv_minWidth))) != 0)
                                {
                                    hv_minWidth = hv_Column23 - hv_Column13;
                                }
                            }

                        }
                        //***********************************************************************************
                        //如果平均宽度不在最小宽度与最大宽度之间，则将平均宽度置为0.5*(minWidth+maxWidth)
                        if ((int)((new HTuple(hv_meanWidth.TupleLess(hv_minWidth))).TupleOr(new HTuple(hv_meanWidth.TupleGreater(
                            hv_maxWidth)))) != 0)
                        {
                            hv_meanWidth = 0.5 * (hv_minWidth + hv_maxWidth);
                        }
                        HOperatorSet.TupleFloor(hv_minWidth / hv_magnification, out hv_minWidth);
                        HOperatorSet.TupleFloor(hv_maxWidth / hv_magnification, out hv_maxWidth);
                        HOperatorSet.TupleFloor(hv_meanWidth / hv_magnification, out hv_meanWidth);
                    }
                }

                hv_tempMessage = hv_defectLoactionNumber.Clone();
                //**************缺边检测
                if ((int)((new HTuple(hv_detectDefectsFlag.TupleGreater(0))).TupleAnd(new HTuple(hv_imperfectBorderFlag.TupleGreater(
                    0)))) != 0)
                {
                    ho_RegionTrans.Dispose();
                    HOperatorSet.ClosingRectangle1(ho_ClothRegion, out ho_RegionTrans, 100, 400);
                    ho_RegionTrans1.Dispose();
                    HOperatorSet.ShapeTrans(ho_RegionTrans, out ho_RegionTrans1, "convex");
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.Union2(ho_RegionTrans, ho_RegionTrans1, out ExpTmpOutVar_0);
                        ho_RegionTrans.Dispose();
                        ho_RegionTrans = ExpTmpOutVar_0;
                    }
                    ho_RegionDifference.Dispose();
                    HOperatorSet.Difference(ho_RegionTrans, ho_ClothRegion, out ho_RegionDifference
                        );
                    ho_RegionOpening.Dispose();
                    HOperatorSet.OpeningRectangle1(ho_RegionDifference, out ho_RegionOpening,
                        hv_imperfectBorderWidth * 2, 2);
                    ho_ConnectedRegions.Dispose();
                    HOperatorSet.Connection(ho_RegionOpening, out ho_ConnectedRegions);
                    ho_SelectedRegions2.Dispose();
                    HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions2, (new HTuple("row")).TupleConcat(
                        "height"), "and", ((hv_Row1 + 10)).TupleConcat(hv_imperfectBorderWidth),
                        ((hv_Row2 - 10)).TupleConcat(20000));
                    ho_RegionUnion3.Dispose();
                    HOperatorSet.Union1(ho_SelectedRegions2, out ho_RegionUnion3);
                    ho_RegionDilation1.Dispose();
                    HOperatorSet.DilationRectangle1(ho_RegionUnion3, out ho_RegionDilation1,
                        10, 200);
                    ho_SelectedRegions2.Dispose();
                    HOperatorSet.Connection(ho_RegionDilation1, out ho_SelectedRegions2);
                    HOperatorSet.CountObj(ho_SelectedRegions2, out hv_Number2);
                    if ((int)(new HTuple(((hv_defectNumber + hv_Number2)).TupleGreater(10))) != 0)
                    {
                        hv_result = 4;
                        hv_message = "警告，缺边缺陷个数大于10个";
                        HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                        hv_color = "red";
                        HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                    }
                    else
                    {
                        if ((int)(new HTuple(hv_Number2.TupleGreater(0))) != 0)
                        {
                            hv_result = 4;
                            //缺边缺陷
                            HOperatorSet.TupleGenConst(hv_Number2, 3, out hv_Newtuple);
                            HOperatorSet.TupleConcat(hv_tupleDefectClass, hv_Newtuple, out hv_tupleDefectClass);
                            if (hv_tupleDetectResult == null)
                                hv_tupleDetectResult = new HTuple();
                            hv_tupleDetectResult[2] = (hv_tupleDetectResult.TupleSelect(2)) + hv_Number2;
                            hv_tempMessage = hv_defectLoactionNumber.Clone();
                            hv_defectNumber = hv_defectNumber + hv_Number2;
                            HTuple end_val148 = hv_Number2;
                            HTuple step_val148 = 1;
                            for (hv_i = 1; hv_i.Continue(end_val148, step_val148); hv_i = hv_i.TupleAdd(step_val148))
                            {
                                ho_ObjectSelected.Dispose();
                                HOperatorSet.SelectObj(ho_SelectedRegions2, out ho_ObjectSelected,
                                    hv_i);
                                HOperatorSet.SmallestRectangle1(ho_ObjectSelected, out hv_Row14, out hv_Column14,
                                    out hv_Row24, out hv_Column24);
                                //画缺陷框
                                HOperatorSet.AreaCenter(ho_ObjectSelected, out hv_AreaTemp, out hv_RowTemp,
                                    out hv_ColumnTemp);
                                HOperatorSet.TupleConcat(hv_tupleDefectRow1, ((hv_Row14 - 100)).TupleMax2(
                                    0), out hv_tupleDefectRow1);
                                HOperatorSet.TupleConcat(hv_tupleDefectRow2, ((hv_Row24 + 100)).TupleMin2(
                                    hv_Height), out hv_tupleDefectRow2);
                                HOperatorSet.TupleConcat(hv_tupleDefectColumn1, hv_Column14 - 100, out hv_tupleDefectColumn1);
                                HOperatorSet.TupleConcat(hv_tupleDefectColumn2, hv_Column24 + 100, out hv_tupleDefectColumn2);
                                HOperatorSet.TupleConcat(hv_tupleDefectX, hv_ColumnTemp, out hv_tupleDefectX);
                                HOperatorSet.TupleConcat(hv_tupleDefectY, hv_RowTemp, out hv_tupleDefectY);
                                hv_defectLoactionNumber = hv_defectLoactionNumber + 1;
                            }
                            if ((int)(new HTuple(hv_Number2.TupleEqual(1))) != 0)
                            {
                                hv_message = "缺边缺陷，编号：" + hv_tempMessage;
                                HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                                hv_color = "red";
                                HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                            }
                            else
                            {
                                hv_message = (("缺边缺陷，编号：" + hv_tempMessage) + "-") + (hv_defectLoactionNumber - 1);
                                HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                                hv_color = "red";
                                HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);

                            }
                        }
                    }
                }

                hv_tempMessage = hv_defectLoactionNumber.Clone();

                //***********其他缺陷检测
                if ((int)((new HTuple(hv_detectDefectsFlag.TupleGreater(0))).TupleAnd(new HTuple(hv_otherFlag.TupleGreater(
                    0)))) != 0)
                {
                    ho_ImageReduced.Dispose();
                    HOperatorSet.ReduceDomain(ho_GrayImage, ho_ClothRegion, out ho_ImageReduced
                        );
                    //mean_image (ImageReduced, ImageMean, 500, 500)
                    //sub_image (ImageReduced, ImageMean, ImageSub, 1, 128)
                    //ho_MidRectangle.Dispose();
                    //HOperatorSet.GenRectangle1(out ho_MidRectangle, (hv_Height / 2) - 1000, (hv_Width / 2) - 1000,
                    //    (hv_Height / 2) + 1000, (hv_Width / 2) + 1000);
                    //ho_ImageReduced1.Dispose();
                    //HOperatorSet.ReduceDomain(ho_GrayImage, ho_MidRectangle, out ho_ImageReduced1
                    //    );
                    //HOperatorSet.GrayHisto(ho_MidRectangle, ho_GrayImage, out hv_AbsoluteHisto,
                    //    out hv_RelativeHisto);
                    //hv_minGrayValue = 0;
                    //hv_maxgrayValue = 255;
                    //for (hv_i = 0; (int)hv_i <= 255; hv_i = (int)hv_i + 1)
                    //{
                    //    if ((int)(new HTuple(((hv_AbsoluteHisto.TupleSelect(hv_i))).TupleGreater(
                    //        50))) != 0)
                    //    {
                    //        hv_minGrayValue = hv_i.Clone();
                    //        break;
                    //    }
                    //}

                    //for (hv_i = 255; (int)hv_i >= 0; hv_i = (int)hv_i + -1)
                    //{
                    //    if ((int)(new HTuple(((hv_AbsoluteHisto.TupleSelect(hv_i))).TupleGreater(
                    //        50))) != 0)
                    //    {
                    //        hv_maxGrayValue = hv_i.Clone();
                    //        break;
                    //    }
                    //}


                    ////histo_to_thresh (AbsoluteHisto, 8, MinThresh, MaxThresh)

                    //HOperatorSet.CoocFeatureImage(ho_MidRectangle, ho_ImageReduced, 6, 0, out hv_Energy,
                    //    out hv_Correlation, out hv_Homogeneity, out hv_Contrast);
                    //if ((int)(new HTuple(hv_Contrast.TupleLess(1))) != 0)
                    //{
                    //    ho_LightRegion.Dispose();
                    //    HOperatorSet.Threshold(ho_ImageReduced, out ho_LightRegion, (new HTuple(240)).TupleMin2(
                    //        hv_maxGrayValue + hv_thresh), 255);
                    //    ho_LowGrayRegion.Dispose();
                    //    HOperatorSet.Threshold(ho_ImageReduced, out ho_LowGrayRegion, 0, (new HTuple(10)).TupleMax2(
                    //        hv_minGrayValue - hv_thresh));

                    //}
                    //else
                    //{
                    //    ho_LightRegion.Dispose();
                    //    HOperatorSet.Threshold(ho_ImageReduced, out ho_LightRegion, (new HTuple(230)).TupleMin2(
                    //        (hv_maxGrayValue + hv_thresh) + (2 * hv_Contrast)), 255);
                    //    ho_LowGrayRegion.Dispose();
                    //    HOperatorSet.Threshold(ho_ImageReduced, out ho_LowGrayRegion, 0, (new HTuple(20)).TupleMax2(
                    //        (hv_minGrayValue - hv_thresh) - (2 * hv_Contrast)));
                    //}
                    //ho_RegionUnion5.Dispose();
                    //HOperatorSet.Union2(ho_LowGrayRegion, ho_LightRegion, out ho_RegionUnion5
                    //    );
                    //ho_RegionOpening3.Dispose();
                    //HOperatorSet.OpeningCircle(ho_RegionUnion5, out ho_RegionOpening3, 1.5);

                    //HOperatorSet.Intensity(ho_MidRectangle, ho_GrayImage, out hv_Mean, out hv_Deviation);
                    //ho_EmptyRegion.Dispose();
                    //HOperatorSet.GenEmptyRegion(out ho_EmptyRegion);
                    //ho_VeryLowGrayRegion.Dispose();
                    //HOperatorSet.GenEmptyObj(out ho_VeryLowGrayRegion);
                    //ho_VeryRegion.Dispose();
                    //HOperatorSet.Threshold(ho_GrayImage, out ho_VeryRegion, (new HTuple(235)).TupleMin2(
                    //    hv_maxGrayValue + 80), 255);
                    ////*     if (minGrayValue>20)
                    ////threshold (GrayImage, VeryLowGrayRegion, 0, 50)
                    ////union2 (GrayImage, VeryLowGrayRegion, VeryRegion)
                    ////*     else
                    ////union1 (GrayImage, VeryRegion)

                    //if ((int)(new HTuple(hv_zhezhouDetectflag.TupleGreater(0))) != 0)
                    //{
                    //    //**动态阈值缺陷分割
                    //    ho_ImageMean2.Dispose();
                    //    HOperatorSet.MeanImage(ho_ImageReduced, out ho_ImageMean2, 30, 30);
                    //    ho_ImageMean1.Dispose();
                    //    HOperatorSet.MeanImage(ho_ImageReduced, out ho_ImageMean1, 160, 160);
                    //    ho_RegionUnion6.Dispose();
                    //    HOperatorSet.DynThreshold(ho_ImageMean2, ho_ImageMean1, out ho_RegionUnion6,
                    //        2, "dark");
                    //    ho_RegionOpening1.Dispose();
                    //    HOperatorSet.OpeningCircle(ho_RegionUnion6, out ho_RegionOpening1, 5);
                    //    ho_ConnectedRegions1.Dispose();

                    //    HOperatorSet.Connection(ho_RegionOpening1, out ho_ConnectedRegions1);
                    //    ho_SelectedRegions.Dispose();
                    //    HOperatorSet.SelectShape(ho_ConnectedRegions1, out ho_SelectedRegions,
                    //        "ra", "and", 50, 5000);
                    //    ho_RegionUnion1.Dispose();
                    //    HOperatorSet.Union1(ho_SelectedRegions, out ho_RegionUnion1);
                    //    ho_RegionDilation2.Dispose();
                    //    HOperatorSet.DilationRectangle1(ho_RegionUnion1, out ho_RegionDilation2,
                    //        1, 45);

                    //    ho_RegionIntersection5.Dispose();
                    //    HOperatorSet.Intersection(ho_RegionDilation2, ho_ValidClothRegion, out ho_RegionIntersection5
                    //    );



                    //    ho_ConnectedRegions2.Dispose();
                    //    HOperatorSet.Connection(ho_RegionIntersection5, out ho_ConnectedRegions2);
                    //    ho_ConnectedRegions7.Dispose();
                    //    HOperatorSet.SelectShape(ho_ConnectedRegions2, out ho_ConnectedRegions7,
                    //        "ra", "and", 200, 5000);

                    //    HOperatorSet.CountObj(ho_ConnectedRegions7, out hv_Number2);
                    //    if ((int)(new HTuple(hv_Number2.TupleGreater(10))) != 0)
                    //    {
                    //        hv_result = 4;
                    //        hv_message = "警告，褶皱位置个数大于10个";
                    //        HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    //        hv_color = "red";
                    //        HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                    //        HOperatorSet.TupleGenConst(10, 4, out hv_Newtuple);
                    //        HOperatorSet.TupleConcat(hv_tupleDefectClass, hv_Newtuple, out hv_tupleDefectClass);
                    //        hv_defectNumber = hv_defectNumber + 10;
                    //        if (hv_tupleDetectResult == null)
                    //            hv_tupleDetectResult = new HTuple();
                    //        hv_tupleDetectResult[4] = (hv_tupleDetectResult.TupleSelect(4)) + 10;
                    //        HOperatorSet.AreaCenter(ho_ConnectedRegions7, out hv_Area5, out hv_Row5,
                    //            out hv_Column5);
                    //        HOperatorSet.TupleSortIndex(hv_Area5, out hv_Indices);
                    //        HOperatorSet.TupleInverse(hv_Indices, out hv_Indices);
                    //        hv_Num = new HTuple(hv_Indices.TupleLength());
                    //        for (hv_j = 1; (int)hv_j <= 10; hv_j = (int)hv_j + 1)
                    //        {
                    //            ho_ObjectSelected.Dispose();
                    //            HOperatorSet.SelectObj(ho_ConnectedRegions7, out ho_ObjectSelected,
                    //                (hv_Indices.TupleSelect(hv_Num - hv_j)) + 1);
                    //            HOperatorSet.SmallestRectangle1(ho_ObjectSelected, out hv_Row14, out hv_Column14,
                    //                out hv_Row24, out hv_Column24);
                    //            HOperatorSet.AreaCenter(ho_ObjectSelected, out hv_AreaTemp, out hv_RowTemp,
                    //                out hv_ColumnTemp);
                    //            HOperatorSet.TupleConcat(hv_tupleDefectX, hv_ColumnTemp, out hv_tupleDefectX);
                    //            HOperatorSet.TupleConcat(hv_tupleDefectY, hv_RowTemp, out hv_tupleDefectY);

                    //            HOperatorSet.TupleConcat(hv_tupleDefectRow1, ((hv_Row14 - 100)).TupleMax2(
                    //                0), out hv_tupleDefectRow1);
                    //            HOperatorSet.TupleConcat(hv_tupleDefectRow2, ((hv_Row24 + 100)).TupleMin2(
                    //                hv_Height), out hv_tupleDefectRow2);
                    //            HOperatorSet.TupleConcat(hv_tupleDefectColumn1, hv_Column14 - 100, out hv_tupleDefectColumn1);
                    //            HOperatorSet.TupleConcat(hv_tupleDefectColumn2, hv_Column24 + 100, out hv_tupleDefectColumn2);
                    //            hv_tupleDefectX = hv_ColumnTemp.Clone();
                    //            hv_tupleDefectY = hv_RowTemp.Clone();
                    //            hv_defectLoactionNumber = hv_defectLoactionNumber + 1;
                    //        }
                    //        hv_message = (("褶皱位置，编号：" + hv_tempMessage) + "-") + (hv_defectLoactionNumber - 1);
                    //        HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    //        hv_color = "red";
                    //        HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                    //    }
                    //    else
                    //    {
                    //        if ((int)(new HTuple(hv_Number2.TupleGreater(0))) != 0)
                    //        {
                    //            HOperatorSet.TupleGenConst(hv_Number2, 4, out hv_Newtuple);
                    //            HOperatorSet.TupleConcat(hv_tupleDefectClass, hv_Newtuple, out hv_tupleDefectClass);
                    //            if (hv_tupleDetectResult == null)
                    //                hv_tupleDetectResult = new HTuple();
                    //            hv_tupleDetectResult[4] = (hv_tupleDetectResult.TupleSelect(4)) + hv_Number2;
                    //            hv_result = 4;
                    //            HOperatorSet.SmallestCircle(ho_ConnectedRegions7, out hv_DefectRow,
                    //                out hv_DefectColumn, out hv_DefectRadius);
                    //            HOperatorSet.TupleConcat(hv_tupleDefectRadius, hv_DefectRadius, out hv_tupleDefectRadius);
                    //            HOperatorSet.TupleConcat(hv_tupleDefectX, hv_DefectColumn, out hv_tupleDefectX);
                    //            HOperatorSet.TupleConcat(hv_tupleDefectY, hv_DefectRow, out hv_tupleDefectY);
                    //            if (hv_tupleDetectResult == null)
                    //                hv_tupleDetectResult = new HTuple();
                    //            hv_tupleDetectResult[0] = (hv_tupleDetectResult.TupleSelect(0)) + hv_Number2;
                    //            HOperatorSet.TupleGenConst(hv_Number2, 4, out hv_Newtuple);
                    //            HOperatorSet.TupleConcat(hv_tupleDefectClass, hv_Newtuple, out hv_tupleDefectClass);
                    //            if (hv_tupleDetectResult == null)
                    //                hv_tupleDetectResult = new HTuple();
                    //            hv_tupleDetectResult[3] = (hv_tupleDetectResult.TupleSelect(3)) + hv_Number2;
                    //            hv_tempMessage = hv_defectLoactionNumber.Clone();
                    //            hv_defectNumber = hv_defectNumber + hv_Number2;
                    //            HTuple end_val290 = hv_Number2;
                    //            HTuple step_val290 = 1;
                    //            for (hv_i = 1; hv_i.Continue(end_val290, step_val290); hv_i = hv_i.TupleAdd(step_val290))
                    //            {
                    //                ho_ObjectSelected.Dispose();
                    //                HOperatorSet.SelectObj(ho_ConnectedRegions7, out ho_ObjectSelected,
                    //                    hv_i);
                    //                HOperatorSet.SmallestRectangle1(ho_ObjectSelected, out hv_Row14,
                    //                    out hv_Column14, out hv_Row24, out hv_Column24);
                    //                HOperatorSet.AreaCenter(ho_ObjectSelected, out hv_AreaTemp, out hv_RowTemp,
                    //                    out hv_ColumnTemp);
                    //                HOperatorSet.TupleConcat(hv_tupleDefectX, hv_ColumnTemp, out hv_tupleDefectX);
                    //                HOperatorSet.TupleConcat(hv_tupleDefectY, hv_RowTemp, out hv_tupleDefectY);
                    //                HOperatorSet.TupleConcat(hv_tupleDefectRow1, ((hv_Row14 - 100)).TupleMax2(
                    //                    0), out hv_tupleDefectRow1);
                    //                HOperatorSet.TupleConcat(hv_tupleDefectRow2, ((hv_Row24 + 100)).TupleMin2(
                    //                    hv_Height), out hv_tupleDefectRow2);
                    //                HOperatorSet.TupleConcat(hv_tupleDefectColumn1, hv_Column14 - 100,
                    //                    out hv_tupleDefectColumn1);
                    //                HOperatorSet.TupleConcat(hv_tupleDefectColumn2, hv_Column24 + 100,
                    //                    out hv_tupleDefectColumn2);
                    //                hv_tupleDefectX = hv_ColumnTemp.Clone();
                    //                hv_tupleDefectY = hv_RowTemp.Clone();
                    //                hv_defectLoactionNumber = hv_defectLoactionNumber + 1;
                    //            }
                    //            if ((int)(new HTuple(hv_Number2.TupleEqual(1))) != 0)
                    //            {
                    //                hv_message = "褶皱位置，编号：" + hv_tempMessage;
                    //                HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    //                hv_color = "red";
                    //                HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                    //            }
                    //            else
                    //            {
                    //                hv_message = (("褶皱位置，编号：" + hv_tempMessage) + "-") + (hv_defectLoactionNumber - 1);
                    //                HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    //                hv_color = "red";
                    //                HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);

                    //            }
                    //        }


                    //    }


                    //    //dilation_circle (RegionUnion6, RegionDilation6, 20)
                    //    //connection (RegionDilation6, ConnectedRegions6)
                    //    //select_shape (ConnectedRegions6, SelectedRegions6, 'ra', 'and', 50, 3000000)


                    //    //union2 (VeryRegion, SelectedRegions1, VeryRegion)

                    //    //筛选普通缺陷
                   


                    //    //筛选周期性缺陷
                    //    //union1 (RegionUnion5, RegionUnion7)
                    //    //union2 (RegionUnion6, RegionUnion5, RegionUnion7)
                    //    //intersection (RegionUnion7, ValidClothRegion, RegionIntersection)
                    //    //connection (RegionIntersection, ConnectedRegions3)
                    //    //select_shape (ConnectedRegions3, SelectedRegions3, 'height', 'and', Height-100, Height)
                    //    //count_obj (SelectedRegions3, Number2)
                    //    //周期性缺陷画图
                    //    //tempMessage := defectLoactionNumber
                    //    //if (Number2>10)
                    //    //result := 4
                    //    //message := '警告，周期性缺陷个数大于10个'
                    //    //tuple_concat (tupleMessages, message, tupleMessages)
                    //    //color := 'red'
                    //    //tuple_concat (tupleMessagesColor, color, tupleMessagesColor)
                    //    //tuple_gen_const (10, 4, Newtuple)
                    //    //tuple_concat (tupleDefectClass, Newtuple, tupleDefectClass)
                    //    //defectNumber := defectNumber+10
                    //    //tupleDetectResult[4] := tupleDetectResult[4]+10
                    //    //area_center (SelectedRegions3, Area5, Row5, Column5)
                    //    //tuple_sort_index (Area5, Indices)
                    //    //Num := |Indices|
                    //    //for j := 1 to 10 by 1
                    //    //select_obj (SelectedRegions3, ObjectSelected, Indices[Num-j]+1)
                    //    //smallest_rectangle1 (ObjectSelected, Row14, Column14, Row24, Column24)
                    //    //area_center (ObjectSelected, AreaTemp, RowTemp, ColumnTemp)
                    //    //tuple_concat (tupleDefectX, ColumnTemp, tupleDefectX)
                    //    //tuple_concat (tupleDefectY, RowTemp, tupleDefectY)
                    //    //tuple_concat (tupleDefectRow1, max2(Row14-100,0), tupleDefectRow1)
                    //    //tuple_concat (tupleDefectRow2, min2(Row24+100,Height), tupleDefectRow2)
                    //    //tuple_concat (tupleDefectColumn1, Column14-100, tupleDefectColumn1)
                    //    //tuple_concat (tupleDefectColumn2, Column24+100, tupleDefectColumn2)
                    //    //tupleDefectX := ColumnTemp
                    //    //tupleDefectY := RowTemp
                    //    //defectLoactionNumber := defectLoactionNumber+1
                    //    //endfor
                    //    //message := '瑕疵点，编号：'+tempMessage+'-'+(defectLoactionNumber-1)
                    //    //tuple_concat (tupleMessages, message, tupleMessages)
                    //    //color := 'red'
                    //    //tuple_concat (tupleMessagesColor, color, tupleMessagesColor)
                    //    //else
                    //    //if (Number2>0)
                    //    //tuple_gen_const (Number2, 2, Newtuple)
                    //    //tuple_concat (tupleDefectClass, Newtuple, tupleDefectClass)
                    //    //tupleDetectResult[1] := tupleDetectResult[1]+Number2
                    //    //result := 4
                    //    //smallest_circle (SelectedRegions3, DefectRow, DefectColumn, DefectRadius)
                    //    //tuple_concat (tupleDefectRadius, DefectRadius, tupleDefectRadius)
                    //    //tuple_concat (tupleDefectX, DefectColumn, tupleDefectX)
                    //    //tuple_concat (tupleDefectY, DefectRow, tupleDefectY)
                    //    //tupleDetectResult[1] := tupleDetectResult[1]+Number2
                    //    //tuple_gen_const (Number2, 1, Newtuple)
                    //    //tuple_concat (tupleDefectClass, Newtuple, tupleDefectClass)
                    //    //tupleDetectResult[0] := tupleDetectResult[0]+Number2
                    //    //tempMessage := defectLoactionNumber
                    //    //defectNumber := defectNumber+Number2
                    //    //for i := 1 to Number2 by 1
                    //    //select_obj (SelectedRegions3, ObjectSelected, i)
                    //    //smallest_rectangle1 (ObjectSelected, Row14, Column14, Row24, Column24)
                    //    //area_center (ObjectSelected, AreaTemp, RowTemp, ColumnTemp)
                    //    //tuple_concat (tupleDefectX, ColumnTemp, tupleDefectX)
                    //    //tuple_concat (tupleDefectY, RowTemp, tupleDefectY)
                    //    //tuple_concat (tupleDefectRow1, max2(Row14-100,0), tupleDefectRow1)
                    //    //tuple_concat (tupleDefectRow2, min2(Row24+100,Height), tupleDefectRow2)
                    //    //tuple_concat (tupleDefectColumn1, Column14-100, tupleDefectColumn1)
                    //    //tuple_concat (tupleDefectColumn2, Column24+100, tupleDefectColumn2)
                    //    //tupleDefectX := ColumnTemp
                    //    //tupleDefectY := RowTemp
                    //    //defectLoactionNumber := defectLoactionNumber+1
                    //    //endfor
                    //    //if (Number2=1)
                    //    //message := '周期性缺陷，编号：'+tempMessage
                    //tuple_concat (tupleMessages, message, tupleMessages)
                    //color := 'red'
                    //tuple_concat (tupleMessagesColor, color, tupleMessagesColor)
                    //else
                    //message := '周期性缺陷，编号：'+tempMessage+'-'+(defectLoactionNumber-1)
                    //tuple_concat (tupleMessages, message, tupleMessages)
                    //color := 'red'
                    //tuple_concat (tupleMessagesColor, color, tupleMessagesColor)

                    //endif
                    //endif
                    //endif


                    //筛选普通缺陷
                    //intersection (VeryRegion, ValidClothRegion, RegionIntersection5)
                    //connection (RegionIntersection5, ConnectedRegions5)
                    //select_shape (ConnectedRegions5, SelectedRegions51, 'height', 'and', 10, Height-100)
                    //select_shape (SelectedRegions51, SelectedRegions71, ['area','rb','ra'], 'and', [defectArea,defectWidth/2,defectHeight/2], [500000,200000,200000])
                    //intersection (RegionUnion5, ValidClothRegion, RegionIntersection6)
                    //connection (RegionIntersection6, ConnectedRegions6)
                    //select_shape (ConnectedRegions6, SelectedRegions61, 'height', 'and', 10, Height-100)
                    //select_shape (SelectedRegions61, SelectedRegions72, ['area','ra'], 'and', [defectArea*2,defectHeight/2*3], [500000,200000])
                    //union2 (SelectedRegions71, SelectedRegions72, RegionUnion)
                    //union1 (SelectedRegions71, RegionUnion)
                    //dilation_rectangle1 (RegionUnion, RegionDilation, 60, 200)
                    //union1 (RegionDilation, RegionDilation)
                    //connection (RegionDilation, ConnectedRegions7)
                    //count_obj (ConnectedRegions7, Number2)



                    ho_ImageMean.Dispose();
                    HOperatorSet.MeanImage(ho_ImageReduced, out ho_ImageMean, 10, 10);
                    ho_ImageMean1.Dispose();
                    HOperatorSet.MeanImage(ho_ImageReduced, out ho_ImageMean1, 60, 60);
                    ho_RegionDynThresh1.Dispose();
                    HOperatorSet.DynThreshold(ho_ImageMean, ho_ImageMean1, out ho_RegionDynThresh1,
                        5, "dark");
                    ho_ConnectedRegions1.Dispose();
                    HOperatorSet.Connection(ho_RegionDynThresh1, out ho_ConnectedRegions1);
                    ho_SelectedRegions1.Dispose();
                    HOperatorSet.SelectShape(ho_ConnectedRegions1, out ho_SelectedRegions1, "area",
                        "and", 300, 1853890);
                    ho_ImageMean.Dispose();
                    HOperatorSet.MeanImage(ho_ImageReduced, out ho_ImageMean, 3, 3);
                    ho_RegionDynThresh2.Dispose();
                    HOperatorSet.DynThreshold(ho_ImageMean, ho_ImageMean1, out ho_RegionDynThresh2,
                        25, "dark");
                    ho_ConnectedRegions2.Dispose();
                    HOperatorSet.Connection(ho_RegionDynThresh2, out ho_ConnectedRegions2);
                    ho_SelectedRegions2.Dispose();
                    HOperatorSet.SelectShape(ho_ConnectedRegions2, out ho_SelectedRegions2, "area",
                        "and", 5, 1853890);

                    ho_RegionUnion1.Dispose();
                    HOperatorSet.Union2(ho_SelectedRegions1, ho_SelectedRegions2, out ho_RegionUnion1
                        );
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_GrayImage, out ho_Regions, 180, 255);
                    ho_VeryRegion.Dispose();
                    HOperatorSet.Union2(ho_RegionUnion1, ho_Regions, out ho_VeryRegion);




                    ho_RegionIntersection5.Dispose();
                    HOperatorSet.Intersection(ho_VeryRegion, ho_ValidClothRegion, out ho_RegionIntersection5
                        );
                    ho_ConnectedRegions5.Dispose();
                    HOperatorSet.Connection(ho_RegionIntersection5, out ho_ConnectedRegions5
                        );
                    ho_SelectedRegions5.Dispose();
                    HOperatorSet.SelectShape(ho_ConnectedRegions5, out ho_SelectedRegions5,
                        ((new HTuple("area")).TupleConcat("rb")).TupleConcat("ra"), "and",
                        ((hv_defectArea.TupleConcat(hv_defectWidth / 2))).TupleConcat(hv_defectHeight / 2),
                        ((new HTuple(5000000)).TupleConcat(200000)).TupleConcat(200000));
                    //select_shape (ConnectedRegions5, SelectedRegions51, 'height', 'and', 10, Height)
                    //select_shape (SelectedRegions51, SelectedRegions71, ['area','rb','ra'], 'or', [defectArea,defectWidth/2,defectHeight/2], [500000,200000,200000])
                    //intersection (RegionUnion5, ValidClothRegion, RegionIntersection6)
                    //connection (RegionIntersection6, ConnectedRegions6)
                    //select_shape (ConnectedRegions6, SelectedRegions61, 'height', 'and', 10, Height/2)
                    //select_shape (SelectedRegions61, SelectedRegions72, ['area','ra'], 'and', [defectArea*2,defectHeight/2*3], [500000,200000])
                    //union2 (SelectedRegions71, SelectedRegions72, RegionUnion)
                    //union1 (SelectedRegions71, RegionUnion)
                    ho_RegionUnion.Dispose();
                    HOperatorSet.Union1(ho_SelectedRegions5, out ho_RegionUnion);
                    ho_RegionDilation.Dispose();
                    HOperatorSet.DilationRectangle1(ho_RegionUnion, out ho_RegionDilation,
                        60, 200);
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.Union1(ho_RegionDilation, out ExpTmpOutVar_0);
                        ho_RegionDilation.Dispose();
                        ho_RegionDilation = ExpTmpOutVar_0;
                    }
                    ho_ConnectedRegions7.Dispose();
                    HOperatorSet.Connection(ho_RegionDilation, out ho_ConnectedRegions7);
                    HOperatorSet.CountObj(ho_ConnectedRegions7, out hv_Number2);


                    hv_tempMessage = hv_defectLoactionNumber.Clone();
                        if ((int)(new HTuple(hv_Number2.TupleGreater(10))) != 0)
                        {
                            hv_result = 4;
                            hv_message = "警告，疵点个数大于10个";
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "red";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                            HOperatorSet.TupleGenConst(10, 4, out hv_Newtuple);
                            HOperatorSet.TupleConcat(hv_tupleDefectClass, hv_Newtuple, out hv_tupleDefectClass);
                            hv_defectNumber = hv_defectNumber + 10;
                            if (hv_tupleDetectResult == null)
                                hv_tupleDetectResult = new HTuple();
                            hv_tupleDetectResult[4] = (hv_tupleDetectResult.TupleSelect(4)) + 10;
                            HOperatorSet.AreaCenter(ho_ConnectedRegions7, out hv_Area5, out hv_Row5,
                                out hv_Column5);
                            HOperatorSet.TupleSortIndex(hv_Area5, out hv_Indices);
                            HOperatorSet.TupleInverse(hv_Indices, out hv_Indices);
                            hv_Num = new HTuple(hv_Indices.TupleLength());
                            for (hv_j = 1; (int)hv_j <= 10; hv_j = (int)hv_j + 1)
                            {
                                ho_ObjectSelected.Dispose();
                                HOperatorSet.SelectObj(ho_ConnectedRegions7, out ho_ObjectSelected,
                                    (hv_Indices.TupleSelect(hv_Num - hv_j)) + 1);
                                HOperatorSet.SmallestRectangle1(ho_ObjectSelected, out hv_Row14, out hv_Column14,
                                    out hv_Row24, out hv_Column24);
                                HOperatorSet.AreaCenter(ho_ObjectSelected, out hv_AreaTemp, out hv_RowTemp,
                                    out hv_ColumnTemp);
                                HOperatorSet.TupleConcat(hv_tupleDefectX, hv_ColumnTemp, out hv_tupleDefectX);
                                HOperatorSet.TupleConcat(hv_tupleDefectY, hv_RowTemp, out hv_tupleDefectY);

                                HOperatorSet.TupleConcat(hv_tupleDefectRow1, ((hv_Row14 - 100)).TupleMax2(
                                    0), out hv_tupleDefectRow1);
                                HOperatorSet.TupleConcat(hv_tupleDefectRow2, ((hv_Row24 + 100)).TupleMin2(
                                    hv_Height), out hv_tupleDefectRow2);
                                HOperatorSet.TupleConcat(hv_tupleDefectColumn1, hv_Column14 - 100, out hv_tupleDefectColumn1);
                                HOperatorSet.TupleConcat(hv_tupleDefectColumn2, hv_Column24 + 100, out hv_tupleDefectColumn2);
                                hv_tupleDefectX = hv_ColumnTemp.Clone();
                                hv_tupleDefectY = hv_RowTemp.Clone();
                                hv_defectLoactionNumber = hv_defectLoactionNumber + 1;
                            }
                            hv_message = (("瑕疵点，编号：" + hv_tempMessage) + "-") + (hv_defectLoactionNumber - 1);
                            HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                            hv_color = "red";
                            HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                        }
                        else
                        {
                            if ((int)(new HTuple(hv_Number2.TupleGreater(0))) != 0)
                            {
                                HOperatorSet.TupleGenConst(hv_Number2, 4, out hv_Newtuple);
                                HOperatorSet.TupleConcat(hv_tupleDefectClass, hv_Newtuple, out hv_tupleDefectClass);
                                if (hv_tupleDetectResult == null)
                                    hv_tupleDetectResult = new HTuple();
                                hv_tupleDetectResult[4] = (hv_tupleDetectResult.TupleSelect(4)) + hv_Number2;
                                hv_result = 4;
                                HOperatorSet.SmallestCircle(ho_ConnectedRegions7, out hv_DefectRow,
                                    out hv_DefectColumn, out hv_DefectRadius);
                                HOperatorSet.TupleConcat(hv_tupleDefectRadius, hv_DefectRadius, out hv_tupleDefectRadius);
                                HOperatorSet.TupleConcat(hv_tupleDefectX, hv_DefectColumn, out hv_tupleDefectX);
                                HOperatorSet.TupleConcat(hv_tupleDefectY, hv_DefectRow, out hv_tupleDefectY);
                                if (hv_tupleDetectResult == null)
                                    hv_tupleDetectResult = new HTuple();
                                hv_tupleDetectResult[0] = (hv_tupleDetectResult.TupleSelect(0)) + hv_Number2;
                                HOperatorSet.TupleGenConst(hv_Number2, 4, out hv_Newtuple);
                                HOperatorSet.TupleConcat(hv_tupleDefectClass, hv_Newtuple, out hv_tupleDefectClass);
                                if (hv_tupleDetectResult == null)
                                    hv_tupleDetectResult = new HTuple();
                                hv_tupleDetectResult[3] = (hv_tupleDetectResult.TupleSelect(3)) + hv_Number2;
                                hv_tempMessage = hv_defectLoactionNumber.Clone();
                                hv_defectNumber = hv_defectNumber + hv_Number2;
                                HTuple end_val498 = hv_Number2;
                                HTuple step_val498 = 1;
                                for (hv_i = 1; hv_i.Continue(end_val498, step_val498); hv_i = hv_i.TupleAdd(step_val498))
                                {
                                    ho_ObjectSelected.Dispose();
                                    HOperatorSet.SelectObj(ho_ConnectedRegions7, out ho_ObjectSelected,
                                        hv_i);
                                    HOperatorSet.SmallestRectangle1(ho_ObjectSelected, out hv_Row14,
                                        out hv_Column14, out hv_Row24, out hv_Column24);
                                    HOperatorSet.AreaCenter(ho_ObjectSelected, out hv_AreaTemp, out hv_RowTemp,
                                        out hv_ColumnTemp);
                                    HOperatorSet.TupleConcat(hv_tupleDefectX, hv_ColumnTemp, out hv_tupleDefectX);
                                    HOperatorSet.TupleConcat(hv_tupleDefectY, hv_RowTemp, out hv_tupleDefectY);
                                    HOperatorSet.TupleConcat(hv_tupleDefectRow1, ((hv_Row14 - 100)).TupleMax2(
                                        0), out hv_tupleDefectRow1);
                                    HOperatorSet.TupleConcat(hv_tupleDefectRow2, ((hv_Row24 + 100)).TupleMin2(
                                        hv_Height), out hv_tupleDefectRow2);
                                    HOperatorSet.TupleConcat(hv_tupleDefectColumn1, hv_Column14 - 100,
                                        out hv_tupleDefectColumn1);
                                    HOperatorSet.TupleConcat(hv_tupleDefectColumn2, hv_Column24 + 100,
                                        out hv_tupleDefectColumn2);
                                    hv_tupleDefectX = hv_ColumnTemp.Clone();
                                    hv_tupleDefectY = hv_RowTemp.Clone();
                                    hv_defectLoactionNumber = hv_defectLoactionNumber + 1;
                                }
                                if ((int)(new HTuple(hv_Number2.TupleEqual(1))) != 0)
                                {
                                    hv_message = "瑕疵点，编号：" + hv_tempMessage;
                                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                                    hv_color = "red";
                                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                                }
                                else
                                {
                                    hv_message = (("瑕疵点，编号：" + hv_tempMessage) + "-") + (hv_defectLoactionNumber - 1);
                                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                                    hv_color = "red";
                                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);

                                }
                            }
                        }
                    
                }
                //HOperatorSet.GenEmptyObj(out ho_Regions);
                //HOperatorSet.GenEmptyObj(out ho_ImageMean);

                //HOperatorSet.GenEmptyObj(out ho_RegionDynThresh1);

                //HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);

                //HOperatorSet.GenEmptyObj(out ho_RegionDynThresh2);

            ho_Regions.Dispose();
            ho_ImageMean.Dispose();
            ho_RegionDynThresh1.Dispose();
            ho_SelectedRegions1.Dispose();
            ho_RegionDynThresh2.Dispose();
            ho_Contours.Dispose();
                ho_ContoursSplit.Dispose();
                ho_UnionContours.Dispose();
                ho_SelectedContours1.Dispose();
                ho_SelectedContours2.Dispose();
                ho_ObjectsConcat.Dispose();
                ho_SelectedContours.Dispose();
                ho_ObjectSelected1.Dispose();
                ho_ObjectSelected.Dispose();
                ho_Rectangle1.Dispose();
                ho_RegionIntersection1.Dispose();
                ho_RegionTrans.Dispose();
                ho_RegionTrans1.Dispose();
                ho_RegionDifference.Dispose();
                ho_RegionOpening.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_SelectedRegions2.Dispose();
                ho_RegionUnion3.Dispose();
                ho_RegionDilation1.Dispose();
                ho_ImageReduced.Dispose();
                ho_MidRectangle.Dispose();
                ho_ImageReduced1.Dispose();
                ho_LightRegion.Dispose();
                ho_LowGrayRegion.Dispose();
                ho_RegionUnion5.Dispose();
                ho_RegionOpening3.Dispose();
                ho_EmptyRegion.Dispose();
                ho_VeryLowGrayRegion.Dispose();
                ho_VeryRegion.Dispose();
                ho_ImageMean2.Dispose();
                ho_ImageMean1.Dispose();
                ho_RegionUnion6.Dispose();
                ho_RegionOpening1.Dispose();
                ho_ConnectedRegions1.Dispose();
                ho_SelectedRegions.Dispose();
                ho_RegionUnion1.Dispose();
                ho_RegionDilation2.Dispose();
                ho_ConnectedRegions2.Dispose();
                ho_ConnectedRegions7.Dispose();
                ho_RegionIntersection5.Dispose();
                ho_ConnectedRegions5.Dispose();
                ho_SelectedRegions5.Dispose();
                ho_RegionUnion.Dispose();
                ho_RegionDilation.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

              ho_Regions.Dispose();
            ho_ImageMean.Dispose();
            ho_RegionDynThresh1.Dispose();
            ho_SelectedRegions1.Dispose();
            ho_RegionDynThresh2.Dispose();
                ho_Contours.Dispose();
                ho_ContoursSplit.Dispose();
                ho_UnionContours.Dispose();
                ho_SelectedContours1.Dispose();
                ho_SelectedContours2.Dispose();
                ho_ObjectsConcat.Dispose();
                ho_SelectedContours.Dispose();
                ho_ObjectSelected1.Dispose();
                ho_ObjectSelected.Dispose();
                ho_Rectangle1.Dispose();
                ho_RegionIntersection1.Dispose();
                ho_RegionTrans.Dispose();
                ho_RegionTrans1.Dispose();
                ho_RegionDifference.Dispose();
                ho_RegionOpening.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_SelectedRegions2.Dispose();
                ho_RegionUnion3.Dispose();
                ho_RegionDilation1.Dispose();
                ho_ImageReduced.Dispose();
                ho_MidRectangle.Dispose();
                ho_ImageReduced1.Dispose();
                ho_LightRegion.Dispose();
                ho_LowGrayRegion.Dispose();
                ho_RegionUnion5.Dispose();
                ho_RegionOpening3.Dispose();
                ho_EmptyRegion.Dispose();
                ho_VeryLowGrayRegion.Dispose();
                ho_VeryRegion.Dispose();
                ho_ImageMean2.Dispose();
                ho_ImageMean1.Dispose();
                ho_RegionUnion6.Dispose();
                ho_RegionOpening1.Dispose();
                ho_ConnectedRegions1.Dispose();
                ho_SelectedRegions.Dispose();
                ho_RegionUnion1.Dispose();
                ho_RegionDilation2.Dispose();
                ho_ConnectedRegions2.Dispose();
                ho_ConnectedRegions7.Dispose();
                ho_RegionIntersection5.Dispose();
                ho_ConnectedRegions5.Dispose();
                ho_SelectedRegions5.Dispose();
                ho_RegionUnion.Dispose();
                ho_RegionDilation.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void get_cloth_region(HObject ho_Image, out HObject ho_ImageR0Left, out HObject ho_ImageG0Left,
            out HObject ho_ImageB0Left, out HObject ho_ImageLLeft, out HObject ho_ImageALeft,
            out HObject ho_ImageBLeft, out HObject ho_ImageR0Right, out HObject ho_ImageG0Right,
            out HObject ho_ImageB0Right, out HObject ho_ImageLRight, out HObject ho_ImageARight,
            out HObject ho_ImageBRight, out HObject ho_ClothRegionLeft, out HObject ho_ClothRegionRight,
            HTuple hv_isSeparateComputer)
        {


            using (HDevThreadContext context = new HDevThreadContext())
            {
                // +++ Threading variables 
                HDevThread devThread;


                // Stack for temporary objects 
                HObject[] OTemp = new HObject[20];

                // Local iconic variables 

                HObject ho_RectangleLeft, ho_RectangleRight;
                HObject ho_ImageLeft, ho_ImageRight, ho_MidRectangLeft;
                HObject ho_MidRectangRight, ho_LeftRectangle, ho_RightRectangle;

                // Local control variables 

                HTuple hv_Width = null, hv_Height = null, hv_thread1 = new HTuple();
                HTuple hv_thread2 = new HTuple(), hv_Mean11 = null, hv_Deviation11 = null;
                HTuple hv_Mean11_1 = null, hv_Mean12 = null, hv_Deviation12 = null;
                HTuple hv_Mean13 = null, hv_Deviation13 = null, hv_max1Left = null;
                HTuple hv_max1Right = null, hv_Mean21 = null, hv_Mean21_1 = null;
                HTuple hv_Mean22 = null, hv_Mean23 = null, hv_max2Left = null;
                HTuple hv_max2Right = null, hv_Mean31 = null, hv_Mean31_1 = null;
                HTuple hv_Mean32 = null, hv_Mean33 = null, hv_max3Left = null;
                HTuple hv_max3Right = null, hv_thread3 = new HTuple();
                HTuple hv_thread4 = new HTuple();
                // Initialize local and output iconic variables 
                HOperatorSet.GenEmptyObj(out ho_ImageR0Left);
                HOperatorSet.GenEmptyObj(out ho_ImageG0Left);
                HOperatorSet.GenEmptyObj(out ho_ImageB0Left);
                HOperatorSet.GenEmptyObj(out ho_ImageLLeft);
                HOperatorSet.GenEmptyObj(out ho_ImageALeft);
                HOperatorSet.GenEmptyObj(out ho_ImageBLeft);
                HOperatorSet.GenEmptyObj(out ho_ImageR0Right);
                HOperatorSet.GenEmptyObj(out ho_ImageG0Right);
                HOperatorSet.GenEmptyObj(out ho_ImageB0Right);
                HOperatorSet.GenEmptyObj(out ho_ImageLRight);
                HOperatorSet.GenEmptyObj(out ho_ImageARight);
                HOperatorSet.GenEmptyObj(out ho_ImageBRight);
                HOperatorSet.GenEmptyObj(out ho_ClothRegionLeft);
                HOperatorSet.GenEmptyObj(out ho_ClothRegionRight);
                HOperatorSet.GenEmptyObj(out ho_RectangleLeft);
                HOperatorSet.GenEmptyObj(out ho_RectangleRight);
                HOperatorSet.GenEmptyObj(out ho_ImageLeft);
                HOperatorSet.GenEmptyObj(out ho_ImageRight);
                HOperatorSet.GenEmptyObj(out ho_MidRectangLeft);
                HOperatorSet.GenEmptyObj(out ho_MidRectangRight);
                HOperatorSet.GenEmptyObj(out ho_LeftRectangle);
                HOperatorSet.GenEmptyObj(out ho_RightRectangle);
                try
                {
                    ho_ImageR0Left.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_ImageR0Left);
                    ho_ImageG0Left.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_ImageG0Left);
                    ho_ImageB0Left.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_ImageB0Left);
                    ho_ImageLLeft.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_ImageLLeft);
                    ho_ImageALeft.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_ImageALeft);
                    ho_ImageBLeft.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_ImageBLeft);
                    ho_ClothRegionLeft.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_ClothRegionLeft);
                    //
                    ho_ImageR0Right.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_ImageR0Right);
                    ho_ImageG0Right.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_ImageG0Right);
                    ho_ImageB0Right.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_ImageB0Right);
                    ho_ImageLRight.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_ImageLRight);
                    ho_ImageARight.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_ImageARight);
                    ho_ImageBRight.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_ImageBRight);
                    ho_ClothRegionRight.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_ClothRegionRight);

                    HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
                    ho_RectangleLeft.Dispose();
                    HOperatorSet.GenRectangle1(out ho_RectangleLeft, 0, 0, hv_Height, hv_Width / 2);
                    ho_RectangleRight.Dispose();
                    HOperatorSet.GenRectangle1(out ho_RectangleRight, 0, (hv_Width / 2) + 1, hv_Height,
                        hv_Width);
                    ho_ImageLeft.Dispose();
                    HOperatorSet.ReduceDomain(ho_Image, ho_RectangleLeft, out ho_ImageLeft);
                    ho_ImageRight.Dispose();
                    HOperatorSet.ReduceDomain(ho_Image, ho_RectangleRight, out ho_ImageRight);
                    //crop_rectangle1 (Image, ImageLeft, 0, 0, Height, Width/2)
                    //crop_rectangle1 (Image, ImageRight, 0, Width/2+1, Height, Width)

                    //图像分割是成RGB图
                    ho_ImageR0Left.Dispose(); ho_ImageG0Left.Dispose(); ho_ImageB0Left.Dispose();
                    HOperatorSet.Decompose3(ho_ImageLeft, out ho_ImageR0Left, out ho_ImageG0Left,
                        out ho_ImageB0Left);
                    ho_ImageR0Right.Dispose(); ho_ImageG0Right.Dispose(); ho_ImageB0Right.Dispose();
                    HOperatorSet.Decompose3(ho_ImageRight, out ho_ImageR0Right, out ho_ImageG0Right,
                        out ho_ImageB0Right);
                    devThread = new HDevThread(context,
                      (HDevThread.ProcCallback)delegate (HDevThread devThreadCB)
                      {
                          try
                          {
                // Input parameters
                HObject cbho_ImageRed = devThreadCB.GetInputIconicParamObject(0);
                              HObject cbho_ImageGreen = devThreadCB.GetInputIconicParamObject(1);
                              HObject cbho_ImageBlue = devThreadCB.GetInputIconicParamObject(2);
                              HTuple cbhv_ColorSpace = devThreadCB.GetInputCtrlParamTuple(3);

                // Output parameters
                HObject cbho_ImageResult1;
                              HObject cbho_ImageResult2;
                              HObject cbho_ImageResult3;

                // Call trans_from_rgb
                HOperatorSet.TransFromRgb(cbho_ImageRed, cbho_ImageGreen, cbho_ImageBlue,
                          out cbho_ImageResult1, out cbho_ImageResult2, out cbho_ImageResult3,
                          cbhv_ColorSpace);

                // Store output parameters in thread object
                devThreadCB.StoreOutputIconicParamObject(0, cbho_ImageResult1);
                              devThreadCB.StoreOutputIconicParamObject(1, cbho_ImageResult2);
                              devThreadCB.StoreOutputIconicParamObject(2, cbho_ImageResult3);

                // Reduce reference counter of thread object
                devThreadCB.Exit();
                              devThreadCB.Dispose();

                          }
                          catch (HalconException exc)
                          {
                // No exceptions may be raised from stub in parallel case,
                // so we need to store this information prior to cleanup
                bool is_direct_call = devThreadCB.IsDirectCall();
                // Attempt to clean up in error case, too
                devThreadCB.Exit();
                              devThreadCB.Dispose();
                // Propagate exception if called directly
                if (is_direct_call)
                                  throw exc;
                          }
                      }, 4, 3);
                    // Set thread procedure call arguments 
                    devThread.SetInputIconicParamObject(0, ho_ImageR0Left);
                    devThread.SetInputIconicParamObject(1, ho_ImageG0Left);
                    devThread.SetInputIconicParamObject(2, ho_ImageB0Left);
                    devThread.SetInputCtrlParamTuple(3, "cielab");
                    devThread.BindOutputIconicParamObject(0, false, ho_ImageLLeft);
                    devThread.BindOutputIconicParamObject(1, false, ho_ImageALeft);
                    devThread.BindOutputIconicParamObject(2, false, ho_ImageBLeft);

                    // Start proc line in thread
                    devThread.ParStart(out hv_thread1);

                    devThread = new HDevThread(context,
                      (HDevThread.ProcCallback)delegate (HDevThread devThreadCB)
                      {
                          try
                          {
                // Input parameters
                HObject cbho_ImageRed = devThreadCB.GetInputIconicParamObject(0);
                              HObject cbho_ImageGreen = devThreadCB.GetInputIconicParamObject(1);
                              HObject cbho_ImageBlue = devThreadCB.GetInputIconicParamObject(2);
                              HTuple cbhv_ColorSpace = devThreadCB.GetInputCtrlParamTuple(3);

                // Output parameters
                HObject cbho_ImageResult1;
                              HObject cbho_ImageResult2;
                              HObject cbho_ImageResult3;

                // Call trans_from_rgb
                HOperatorSet.TransFromRgb(cbho_ImageRed, cbho_ImageGreen, cbho_ImageBlue,
                          out cbho_ImageResult1, out cbho_ImageResult2, out cbho_ImageResult3,
                          cbhv_ColorSpace);

                // Store output parameters in thread object
                devThreadCB.StoreOutputIconicParamObject(0, cbho_ImageResult1);
                              devThreadCB.StoreOutputIconicParamObject(1, cbho_ImageResult2);
                              devThreadCB.StoreOutputIconicParamObject(2, cbho_ImageResult3);

                // Reduce reference counter of thread object
                devThreadCB.Exit();
                              devThreadCB.Dispose();

                          }
                          catch (HalconException exc)
                          {
                // No exceptions may be raised from stub in parallel case,
                // so we need to store this information prior to cleanup
                bool is_direct_call = devThreadCB.IsDirectCall();
                // Attempt to clean up in error case, too
                devThreadCB.Exit();
                              devThreadCB.Dispose();
                // Propagate exception if called directly
                if (is_direct_call)
                                  throw exc;
                          }
                      }, 4, 3);
                    // Set thread procedure call arguments 
                    devThread.SetInputIconicParamObject(0, ho_ImageR0Right);
                    devThread.SetInputIconicParamObject(1, ho_ImageG0Right);
                    devThread.SetInputIconicParamObject(2, ho_ImageB0Right);
                    devThread.SetInputCtrlParamTuple(3, "cielab");
                    devThread.BindOutputIconicParamObject(0, false, ho_ImageLRight);
                    devThread.BindOutputIconicParamObject(1, false, ho_ImageARight);
                    devThread.BindOutputIconicParamObject(2, false, ho_ImageBRight);

                    // Start proc line in thread
                    devThread.ParStart(out hv_thread2);

                    HDevThread.ParJoin(hv_thread1.TupleConcat(hv_thread2));

                    //***********************************************************************************
                    //分别计算LAB图像布匹中间与两边的灰度差，将灰度差较大的作为布匹区域分割对象
                    //***********************************************************************************
                    ho_MidRectangLeft.Dispose();
                    HOperatorSet.GenRectangle1(out ho_MidRectangLeft, (hv_Height / 2) - 300, (hv_Width / 2) - 300,
                        (hv_Height / 2) + 300, hv_Width / 2);
                    ho_MidRectangRight.Dispose();
                    HOperatorSet.GenRectangle1(out ho_MidRectangRight, (hv_Height / 2) - 300, hv_Width / 2,
                        (hv_Height / 2) + 300, (hv_Width / 2) + 300);
                    ho_LeftRectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_LeftRectangle, (hv_Height / 2) - 300, 188, (hv_Height / 2) + 300,
                        328);
                    ho_RightRectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_RightRectangle, (hv_Height / 2) - 300, hv_Width - 300,
                        (hv_Height / 2) + 300, hv_Width - 200);

                    HOperatorSet.Intensity(ho_MidRectangLeft, ho_ImageLLeft, out hv_Mean11, out hv_Deviation11);
                    HOperatorSet.Intensity(ho_MidRectangRight, ho_ImageLRight, out hv_Mean11_1,
                        out hv_Deviation11);
                    HOperatorSet.Intensity(ho_LeftRectangle, ho_ImageLLeft, out hv_Mean12, out hv_Deviation12);
                    HOperatorSet.Intensity(ho_RightRectangle, ho_ImageLRight, out hv_Mean13, out hv_Deviation13);
                    hv_max1Left = ((hv_Mean12 - hv_Mean11)).TupleAbs();
                    hv_max1Right = ((hv_Mean13 - hv_Mean11_1)).TupleAbs();

                    HOperatorSet.Intensity(ho_MidRectangLeft, ho_ImageALeft, out hv_Mean21, out hv_Deviation11);
                    HOperatorSet.Intensity(ho_MidRectangRight, ho_ImageARight, out hv_Mean21_1,
                        out hv_Deviation11);
                    HOperatorSet.Intensity(ho_LeftRectangle, ho_ImageALeft, out hv_Mean22, out hv_Deviation12);
                    HOperatorSet.Intensity(ho_RightRectangle, ho_ImageARight, out hv_Mean23, out hv_Deviation13);
                    hv_max2Left = ((hv_Mean22 - hv_Mean21)).TupleAbs();
                    hv_max2Right = ((hv_Mean23 - hv_Mean21_1)).TupleAbs();

                    HOperatorSet.Intensity(ho_MidRectangLeft, ho_ImageBLeft, out hv_Mean31, out hv_Deviation11);
                    HOperatorSet.Intensity(ho_MidRectangRight, ho_ImageBRight, out hv_Mean31_1,
                        out hv_Deviation11);
                    HOperatorSet.Intensity(ho_LeftRectangle, ho_ImageBLeft, out hv_Mean32, out hv_Deviation12);
                    HOperatorSet.Intensity(ho_RightRectangle, ho_ImageBRight, out hv_Mean33, out hv_Deviation13);
                    hv_max3Left = ((hv_Mean32 - hv_Mean31)).TupleAbs();
                    hv_max3Right = ((hv_Mean33 - hv_Mean31_1)).TupleAbs();

                    devThread = new HDevThread(context,
                      (HDevThread.ProcCallback)delegate (HDevThread devThreadCB)
                      {
                          try
                          {
                // Input parameters
                HObject cbho_Image = devThreadCB.GetInputIconicParamObject(0);
                              HObject cbho_ImageL = devThreadCB.GetInputIconicParamObject(1);
                              HObject cbho_ImageA = devThreadCB.GetInputIconicParamObject(2);
                              HObject cbho_ImageB = devThreadCB.GetInputIconicParamObject(3);
                              HTuple cbhv_Max1 = devThreadCB.GetInputCtrlParamTuple(4);
                              HTuple cbhv_Max2 = devThreadCB.GetInputCtrlParamTuple(5);
                              HTuple cbhv_Max3 = devThreadCB.GetInputCtrlParamTuple(6);
                              HTuple cbhv_Mean11 = devThreadCB.GetInputCtrlParamTuple(7);
                              HTuple cbhv_Mean12 = devThreadCB.GetInputCtrlParamTuple(8);
                              HTuple cbhv_Mean21 = devThreadCB.GetInputCtrlParamTuple(9);
                              HTuple cbhv_Mean22 = devThreadCB.GetInputCtrlParamTuple(10);
                              HTuple cbhv_Mean31 = devThreadCB.GetInputCtrlParamTuple(11);
                              HTuple cbhv_Mean32 = devThreadCB.GetInputCtrlParamTuple(12);
                              HTuple cbhv_Width = devThreadCB.GetInputCtrlParamTuple(13);
                              HTuple cbhv_Height = devThreadCB.GetInputCtrlParamTuple(14);

                // Output parameters
                HObject cbho_ClothRegion;

                // Call cloth_region_auto_find
                cloth_region_auto_find(cbho_Image, cbho_ImageL, cbho_ImageA, cbho_ImageB,
                          out cbho_ClothRegion, cbhv_Max1, cbhv_Max2, cbhv_Max3, cbhv_Mean11,
                          cbhv_Mean12, cbhv_Mean21, cbhv_Mean22, cbhv_Mean31, cbhv_Mean32,
                          cbhv_Width, cbhv_Height);

                // Store output parameters in thread object
                devThreadCB.StoreOutputIconicParamObject(0, cbho_ClothRegion);

                // Reduce reference counter of thread object
                devThreadCB.Exit();
                              devThreadCB.Dispose();

                          }
                          catch (HalconException exc)
                          {
                // No exceptions may be raised from stub in parallel case,
                // so we need to store this information prior to cleanup
                bool is_direct_call = devThreadCB.IsDirectCall();
                // Attempt to clean up in error case, too
                devThreadCB.Exit();
                              devThreadCB.Dispose();
                // Propagate exception if called directly
                if (is_direct_call)
                                  throw exc;
                          }
                      }, 15, 1);
                    // Set thread procedure call arguments 
                    devThread.SetInputIconicParamObject(0, ho_ImageLeft);
                    devThread.SetInputIconicParamObject(1, ho_ImageLLeft);
                    devThread.SetInputIconicParamObject(2, ho_ImageALeft);
                    devThread.SetInputIconicParamObject(3, ho_ImageBLeft);
                    devThread.SetInputCtrlParamTuple(4, hv_max1Left);
                    devThread.SetInputCtrlParamTuple(5, hv_max2Left);
                    devThread.SetInputCtrlParamTuple(6, hv_max3Left);
                    devThread.SetInputCtrlParamTuple(7, hv_Mean11);
                    devThread.SetInputCtrlParamTuple(8, hv_Mean12);
                    devThread.SetInputCtrlParamTuple(9, hv_Mean21);
                    devThread.SetInputCtrlParamTuple(10, hv_Mean22);
                    devThread.SetInputCtrlParamTuple(11, hv_Mean31);
                    devThread.SetInputCtrlParamTuple(12, hv_Mean32);
                    devThread.SetInputCtrlParamTuple(13, hv_Width);
                    devThread.SetInputCtrlParamTuple(14, hv_Height);
                    devThread.BindOutputIconicParamObject(0, false, ho_ClothRegionLeft);

                    // Start proc line in thread
                    devThread.ParStart(out hv_thread3);

                    devThread = new HDevThread(context,
                      (HDevThread.ProcCallback)delegate (HDevThread devThreadCB)
                      {
                          try
                          {
                // Input parameters
                HObject cbho_Image = devThreadCB.GetInputIconicParamObject(0);
                              HObject cbho_ImageL = devThreadCB.GetInputIconicParamObject(1);
                              HObject cbho_ImageA = devThreadCB.GetInputIconicParamObject(2);
                              HObject cbho_ImageB = devThreadCB.GetInputIconicParamObject(3);
                              HTuple cbhv_Max1 = devThreadCB.GetInputCtrlParamTuple(4);
                              HTuple cbhv_Max2 = devThreadCB.GetInputCtrlParamTuple(5);
                              HTuple cbhv_Max3 = devThreadCB.GetInputCtrlParamTuple(6);
                              HTuple cbhv_Mean11 = devThreadCB.GetInputCtrlParamTuple(7);
                              HTuple cbhv_Mean12 = devThreadCB.GetInputCtrlParamTuple(8);
                              HTuple cbhv_Mean21 = devThreadCB.GetInputCtrlParamTuple(9);
                              HTuple cbhv_Mean22 = devThreadCB.GetInputCtrlParamTuple(10);
                              HTuple cbhv_Mean31 = devThreadCB.GetInputCtrlParamTuple(11);
                              HTuple cbhv_Mean32 = devThreadCB.GetInputCtrlParamTuple(12);
                              HTuple cbhv_Width = devThreadCB.GetInputCtrlParamTuple(13);
                              HTuple cbhv_Height = devThreadCB.GetInputCtrlParamTuple(14);

                // Output parameters
                HObject cbho_ClothRegion;

                // Call cloth_region_auto_find
                cloth_region_auto_find(cbho_Image, cbho_ImageL, cbho_ImageA, cbho_ImageB,
                          out cbho_ClothRegion, cbhv_Max1, cbhv_Max2, cbhv_Max3, cbhv_Mean11,
                          cbhv_Mean12, cbhv_Mean21, cbhv_Mean22, cbhv_Mean31, cbhv_Mean32,
                          cbhv_Width, cbhv_Height);

                // Store output parameters in thread object
                devThreadCB.StoreOutputIconicParamObject(0, cbho_ClothRegion);

                // Reduce reference counter of thread object
                devThreadCB.Exit();
                              devThreadCB.Dispose();

                          }
                          catch (HalconException exc)
                          {
                // No exceptions may be raised from stub in parallel case,
                // so we need to store this information prior to cleanup
                bool is_direct_call = devThreadCB.IsDirectCall();
                // Attempt to clean up in error case, too
                devThreadCB.Exit();
                              devThreadCB.Dispose();
                // Propagate exception if called directly
                if (is_direct_call)
                                  throw exc;
                          }
                      }, 15, 1);
                    // Set thread procedure call arguments 
                    devThread.SetInputIconicParamObject(0, ho_ImageRight);
                    devThread.SetInputIconicParamObject(1, ho_ImageLRight);
                    devThread.SetInputIconicParamObject(2, ho_ImageARight);
                    devThread.SetInputIconicParamObject(3, ho_ImageBRight);
                    devThread.SetInputCtrlParamTuple(4, hv_max1Right);
                    devThread.SetInputCtrlParamTuple(5, hv_max2Right);
                    devThread.SetInputCtrlParamTuple(6, hv_max3Right);
                    devThread.SetInputCtrlParamTuple(7, hv_Mean11_1);
                    devThread.SetInputCtrlParamTuple(8, hv_Mean13);
                    devThread.SetInputCtrlParamTuple(9, hv_Mean21_1);
                    devThread.SetInputCtrlParamTuple(10, hv_Mean23);
                    devThread.SetInputCtrlParamTuple(11, hv_Mean31_1);
                    devThread.SetInputCtrlParamTuple(12, hv_Mean33);
                    devThread.SetInputCtrlParamTuple(13, hv_Width);
                    devThread.SetInputCtrlParamTuple(14, hv_Height);
                    devThread.BindOutputIconicParamObject(0, false, ho_ClothRegionRight);

                    // Start proc line in thread
                    devThread.ParStart(out hv_thread4);

                    HDevThread.ParJoin(hv_thread3.TupleConcat(hv_thread4));
                    if ((int)(new HTuple(hv_isSeparateComputer.TupleEqual(1))) != 0)
                    {

                    }
                    else
                    {
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.Union2(ho_ClothRegionLeft, ho_ClothRegionRight, out ExpTmpOutVar_0
                                );
                            ho_ClothRegionLeft.Dispose();
                            ho_ClothRegionLeft = ExpTmpOutVar_0;
                        }
                    }



                    ho_RectangleLeft.Dispose();
                    ho_RectangleRight.Dispose();
                    ho_ImageLeft.Dispose();
                    ho_ImageRight.Dispose();
                    ho_MidRectangLeft.Dispose();
                    ho_MidRectangRight.Dispose();
                    ho_LeftRectangle.Dispose();
                    ho_RightRectangle.Dispose();

                    return;
                }
                catch (HalconException HDevExpDefaultException)
                {
                    ho_RectangleLeft.Dispose();
                    ho_RectangleRight.Dispose();
                    ho_ImageLeft.Dispose();
                    ho_ImageRight.Dispose();
                    ho_MidRectangLeft.Dispose();
                    ho_MidRectangRight.Dispose();
                    ho_LeftRectangle.Dispose();
                    ho_RightRectangle.Dispose();

                    throw HDevExpDefaultException;
                }
            }
        }

        public void cloth_region_auto_find(HObject ho_Image, HObject ho_ImageL, HObject ho_ImageA,
            HObject ho_ImageB, out HObject ho_ClothRegion, HTuple hv_Max1, HTuple hv_Max2,
            HTuple hv_Max3, HTuple hv_Mean11, HTuple hv_Mean12, HTuple hv_Mean21, HTuple hv_Mean22,
            HTuple hv_Mean31, HTuple hv_Mean32, HTuple hv_Width, HTuple hv_Height)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_Regions1 = null, ho_ConnectedRegions4 = null;
            HObject ho_SelectedRegions = null, ho_Rectangle5 = null, ho_ObjectSelected3 = null;
            HObject ho_Rectangle6 = null, ho_Regions = null, ho_Rectangle1 = null;
            HObject ho_Rectangle2 = null, ho_RegionUnion1 = null, ho_RegionUnion2 = null;
            HObject ho_RegionOpening1 = null, ho_ConnectedRegions1 = null;
            HObject ho_SelectedRegions1 = null;

            // Local control variables 

            HTuple hv_Number = null, hv_UsedThreshold = new HTuple();
            HTuple hv_Area = new HTuple(), hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_Mean1 = new HTuple(), hv_Deviation1 = new HTuple();
            HTuple hv_i = new HTuple(), hv_Area1 = new HTuple(), hv_Row4 = new HTuple();
            HTuple hv_Column4 = new HTuple(), hv_Mean2 = new HTuple();
            HTuple hv_Deviation2 = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ClothRegion);
            HOperatorSet.GenEmptyObj(out ho_Regions1);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions4);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_Rectangle5);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelected3);
            HOperatorSet.GenEmptyObj(out ho_Rectangle6);
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            HOperatorSet.GenEmptyObj(out ho_Rectangle2);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion1);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion2);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening1);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            try
            {
                //***********************************************************************************
                //如AB图像的灰度差<5,且L图像灰度差小于10，则未找到布匹，否则分割布匹区域，SelectedRegions1
                //***********************************************************************************
                hv_Number = 0;
                if ((int)((new HTuple((new HTuple(hv_Max2.TupleLess(5))).TupleAnd(new HTuple(hv_Max3.TupleLess(
                    5))))).TupleAnd(new HTuple(hv_Max1.TupleLess(10)))) != 0)
                {
                    ho_Regions1.Dispose();
                    ho_ConnectedRegions4.Dispose();
                    ho_SelectedRegions.Dispose();
                    ho_Rectangle5.Dispose();
                    ho_ObjectSelected3.Dispose();
                    ho_Rectangle6.Dispose();
                    ho_Regions.Dispose();
                    ho_Rectangle1.Dispose();
                    ho_Rectangle2.Dispose();
                    ho_RegionUnion1.Dispose();
                    ho_RegionUnion2.Dispose();
                    ho_RegionOpening1.Dispose();
                    ho_ConnectedRegions1.Dispose();
                    ho_SelectedRegions1.Dispose();

                    return;
                }
                else
                {
                    if ((int)((new HTuple(hv_Max2.TupleGreater(5))).TupleOr(new HTuple(hv_Max3.TupleGreater(
                        5)))) != 0)
                    {
                        //***********************************************************************************
                        //在AB图像中选取色差大的图，作为分割布匹区域，SelectedRegions1
                        //***********************************************************************************
                        if ((int)(new HTuple(hv_Max2.TupleGreater(hv_Max3))) != 0)
                        {
                            //
                            if ((int)(new HTuple(((hv_Mean21 - hv_Mean22)).TupleLess(0))) != 0)
                            {
                                ho_Regions1.Dispose();
                                HOperatorSet.BinaryThreshold(ho_ImageA, out ho_Regions1, "max_separability",
                                    "dark", out hv_UsedThreshold);
                            }
                            else
                            {
                                ho_Regions1.Dispose();
                                HOperatorSet.BinaryThreshold(ho_ImageA, out ho_Regions1, "max_separability",
                                    "light", out hv_UsedThreshold);
                            }
                            //
                            ho_ConnectedRegions4.Dispose();
                            HOperatorSet.Connection(ho_Regions1, out ho_ConnectedRegions4);
                            ho_Regions1.Dispose();
                            HOperatorSet.SelectShape(ho_ConnectedRegions4, out ho_Regions1, (new HTuple("height")).TupleConcat(
                                "area"), "and", ((hv_Height - 100)).TupleConcat(hv_Height * 1000), ((hv_Height + 100)).TupleConcat(
                                10000000000));
                            HOperatorSet.CountObj(ho_Regions1, out hv_Number);
                            if ((int)(new HTuple(hv_Number.TupleGreater(1))) != 0)
                            {
                                ho_SelectedRegions.Dispose();
                                HOperatorSet.SelectShapeStd(ho_Regions1, out ho_SelectedRegions, "max_area",
                                    80);
                                HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row,
                                    out hv_Column);
                                ho_Rectangle5.Dispose();
                                HOperatorSet.GenRectangle1(out ho_Rectangle5, hv_Row - 500, hv_Column - 500,
                                    hv_Row + 500, hv_Column + 500);
                                HOperatorSet.Intensity(ho_Rectangle5, ho_ImageA, out hv_Mean1, out hv_Deviation1);
                                HTuple end_val27 = hv_Number;
                                HTuple step_val27 = 1;
                                for (hv_i = 1; hv_i.Continue(end_val27, step_val27); hv_i = hv_i.TupleAdd(step_val27))
                                {
                                    ho_ObjectSelected3.Dispose();
                                    HOperatorSet.SelectObj(ho_Regions1, out ho_ObjectSelected3, hv_i);
                                    HOperatorSet.AreaCenter(ho_ObjectSelected3, out hv_Area1, out hv_Row4,
                                        out hv_Column4);
                                    if ((int)(new HTuple(hv_Area1.TupleNotEqual(hv_Area))) != 0)
                                    {
                                        ho_Rectangle6.Dispose();
                                        HOperatorSet.GenRectangle1(out ho_Rectangle6, hv_Row4 - 500, hv_Column4 - 30,
                                            hv_Row4 + 500, hv_Column4 + 30);
                                        HOperatorSet.Intensity(ho_Rectangle6, ho_ImageA, out hv_Mean2,
                                            out hv_Deviation2);
                                        if ((int)((new HTuple(((hv_Mean2 - hv_Mean1)).TupleGreater(-10))).TupleAnd(
                                            new HTuple(((hv_Mean2 - hv_Mean1)).TupleLess(10)))) != 0)
                                        {
                                            {
                                                HObject ExpTmpOutVar_0;
                                                HOperatorSet.Union2(ho_SelectedRegions, ho_ObjectSelected3, out ExpTmpOutVar_0
                                                    );
                                                ho_SelectedRegions.Dispose();
                                                ho_SelectedRegions = ExpTmpOutVar_0;
                                            }
                                        }
                                    }
                                    //
                                }
                                ho_Regions1.Dispose();
                                HOperatorSet.Connection(ho_SelectedRegions, out ho_Regions1);
                            }
                        }
                        else
                        {
                            //
                            if ((int)(new HTuple(((hv_Mean31 - hv_Mean32)).TupleLess(0))) != 0)
                            {
                                ho_Regions1.Dispose();
                                HOperatorSet.BinaryThreshold(ho_ImageB, out ho_Regions1, "max_separability",
                                    "dark", out hv_UsedThreshold);
                            }
                            else
                            {
                                ho_Regions1.Dispose();
                                HOperatorSet.BinaryThreshold(ho_ImageB, out ho_Regions1, "max_separability",
                                    "light", out hv_UsedThreshold);
                            }
                            ho_ConnectedRegions4.Dispose();
                            HOperatorSet.Connection(ho_Regions1, out ho_ConnectedRegions4);
                            ho_Regions1.Dispose();
                            HOperatorSet.SelectShape(ho_ConnectedRegions4, out ho_Regions1, (new HTuple("height")).TupleConcat(
                                "area"), "and", ((hv_Height - 100)).TupleConcat(hv_Height * 1000), ((hv_Height + 100)).TupleConcat(
                                10000000000));
                            HOperatorSet.CountObj(ho_Regions1, out hv_Number);
                            if ((int)(new HTuple(hv_Number.TupleGreater(1))) != 0)
                            {
                                //***********************************************************************************
                                //如果区域个数大于1，选取与最大区域灰度接近的区域并与最大区域连到一起
                                //****************************************************************u*******************
                                ho_SelectedRegions.Dispose();
                                HOperatorSet.SelectShapeStd(ho_Regions1, out ho_SelectedRegions, "max_area",
                                    80);
                                HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row,
                                    out hv_Column);
                                ho_Rectangle5.Dispose();
                                HOperatorSet.GenRectangle1(out ho_Rectangle5, hv_Row - 500, hv_Column - 500,
                                    hv_Row + 500, hv_Column + 500);
                                HOperatorSet.Intensity(ho_Rectangle5, ho_ImageB, out hv_Mean1, out hv_Deviation1);
                                HTuple end_val59 = hv_Number;
                                HTuple step_val59 = 1;
                                for (hv_i = 1; hv_i.Continue(end_val59, step_val59); hv_i = hv_i.TupleAdd(step_val59))
                                {
                                    ho_ObjectSelected3.Dispose();
                                    HOperatorSet.SelectObj(ho_Regions1, out ho_ObjectSelected3, hv_i);
                                    HOperatorSet.AreaCenter(ho_ObjectSelected3, out hv_Area1, out hv_Row4,
                                        out hv_Column4);
                                    if ((int)(new HTuple(hv_Area1.TupleNotEqual(hv_Area))) != 0)
                                    {
                                        ho_Rectangle6.Dispose();
                                        HOperatorSet.GenRectangle1(out ho_Rectangle6, hv_Row4 - 500, hv_Column4 - 30,
                                            hv_Row4 + 500, hv_Column4 + 30);
                                        HOperatorSet.Intensity(ho_Rectangle6, ho_ImageB, out hv_Mean2,
                                            out hv_Deviation2);
                                        if ((int)((new HTuple(((hv_Mean2 - hv_Mean1)).TupleGreater(-10))).TupleAnd(
                                            new HTuple(((hv_Mean2 - hv_Mean1)).TupleLess(10)))) != 0)
                                        {
                                            {
                                                HObject ExpTmpOutVar_0;
                                                HOperatorSet.Union2(ho_SelectedRegions, ho_ObjectSelected3, out ExpTmpOutVar_0
                                                    );
                                                ho_SelectedRegions.Dispose();
                                                ho_SelectedRegions = ExpTmpOutVar_0;
                                            }
                                        }
                                    }
                                    //
                                }
                                ho_Regions1.Dispose();
                                HOperatorSet.Connection(ho_SelectedRegions, out ho_Regions1);
                            }
                            //
                        }
                        //
                    }
                    else
                    {

                        ho_Regions.Dispose();
                        HOperatorSet.AutoThreshold(ho_ImageL, out ho_Regions, 2);
                        ho_ConnectedRegions4.Dispose();
                        HOperatorSet.Connection(ho_Regions, out ho_ConnectedRegions4);
                        ho_Regions1.Dispose();
                        HOperatorSet.SelectShape(ho_ConnectedRegions4, out ho_Regions1, (new HTuple("height")).TupleConcat(
                            "area"), "and", ((hv_Height - 100)).TupleConcat(hv_Height * 1000), ((hv_Height + 100)).TupleConcat(
                            10000000000));
                        HOperatorSet.CountObj(ho_Regions1, out hv_Number);
                        if ((int)(new HTuple(hv_Number.TupleGreater(1))) != 0)
                        {
                            ho_SelectedRegions.Dispose();
                            HOperatorSet.SelectShapeStd(ho_Regions1, out ho_SelectedRegions, "max_area",
                                80);
                            HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row,
                                out hv_Column);
                            ho_Rectangle5.Dispose();
                            HOperatorSet.GenRectangle1(out ho_Rectangle5, hv_Row - 500, hv_Column - 500,
                                hv_Row + 500, hv_Column + 500);
                            HOperatorSet.Intensity(ho_Rectangle5, ho_ImageL, out hv_Mean1, out hv_Deviation1);
                            HTuple end_val87 = hv_Number;
                            HTuple step_val87 = 1;
                            for (hv_i = 1; hv_i.Continue(end_val87, step_val87); hv_i = hv_i.TupleAdd(step_val87))
                            {
                                ho_ObjectSelected3.Dispose();
                                HOperatorSet.SelectObj(ho_Regions1, out ho_ObjectSelected3, hv_i);
                                HOperatorSet.AreaCenter(ho_ObjectSelected3, out hv_Area1, out hv_Row4,
                                    out hv_Column4);
                                if ((int)(new HTuple(hv_Area1.TupleNotEqual(hv_Area))) != 0)
                                {
                                    ho_Rectangle6.Dispose();
                                    HOperatorSet.GenRectangle1(out ho_Rectangle6, hv_Row4 - 500, hv_Column4 - 30,
                                        hv_Row4 + 500, hv_Column4 + 30);
                                    HOperatorSet.Intensity(ho_Rectangle6, ho_ImageL, out hv_Mean2, out hv_Deviation2);
                                    if ((int)((new HTuple(((hv_Mean2 - hv_Mean1)).TupleGreater(-10))).TupleAnd(
                                        new HTuple(((hv_Mean2 - hv_Mean1)).TupleLess(10)))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_SelectedRegions, ho_ObjectSelected3, out ExpTmpOutVar_0
                                                );
                                            ho_SelectedRegions.Dispose();
                                            ho_SelectedRegions = ExpTmpOutVar_0;
                                        }
                                    }
                                }
                            }
                            ho_Regions1.Dispose();
                            HOperatorSet.Connection(ho_SelectedRegions, out ho_Regions1);
                        }
                        //
                    }
                    //
                    //***********************************************************************************
                    //防止周期性缺陷
                    //***********************************************************************************
                    ho_Rectangle1.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle1, 1, 200, 3, hv_Width - 250);
                    ho_Rectangle2.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle2, hv_Height - 3, 200, hv_Height,
                        hv_Width - 250);
                    ho_RegionUnion1.Dispose();
                    HOperatorSet.Union2(ho_Rectangle1, ho_Rectangle2, out ho_RegionUnion1);
                    ho_RegionUnion2.Dispose();
                    HOperatorSet.Union2(ho_RegionUnion1, ho_Regions1, out ho_RegionUnion2);
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.FillUp(ho_RegionUnion2, out ExpTmpOutVar_0);
                        ho_RegionUnion2.Dispose();
                        ho_RegionUnion2 = ExpTmpOutVar_0;
                    }
                    ho_RegionOpening1.Dispose();
                    HOperatorSet.OpeningRectangle1(ho_RegionUnion2, out ho_RegionOpening1, 1,
                        7);
                    //***********************************************************************************
                    ho_ConnectedRegions1.Dispose();
                    HOperatorSet.Connection(ho_RegionOpening1, out ho_ConnectedRegions1);
                    ho_SelectedRegions1.Dispose();
                    HOperatorSet.SelectShape(ho_ConnectedRegions1, out ho_SelectedRegions1, (new HTuple("height")).TupleConcat(
                        "area"), "and", ((hv_Height - 100)).TupleConcat(hv_Height * 1000), ((hv_Height + 100)).TupleConcat(
                        10000000000));
                    HOperatorSet.CountObj(ho_SelectedRegions1, out hv_Number);
                }
                //***********************************************************************************
                ho_ClothRegion.Dispose();
                HOperatorSet.FillUp(ho_SelectedRegions1, out ho_ClothRegion);
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ClosingRectangle1(ho_ClothRegion, out ExpTmpOutVar_0, 10, 60);
                    ho_ClothRegion.Dispose();
                    ho_ClothRegion = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.OpeningRectangle1(ho_ClothRegion, out ExpTmpOutVar_0, 1, 200);
                    ho_ClothRegion.Dispose();
                    ho_ClothRegion = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ClosingRectangle1(ho_ClothRegion, out ExpTmpOutVar_0, 60, 20);
                    ho_ClothRegion.Dispose();
                    ho_ClothRegion = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.OpeningRectangle1(ho_ClothRegion, out ExpTmpOutVar_0, 1, 10);
                    ho_ClothRegion.Dispose();
                    ho_ClothRegion = ExpTmpOutVar_0;
                }
                //

                ho_Regions1.Dispose();
                ho_ConnectedRegions4.Dispose();
                ho_SelectedRegions.Dispose();
                ho_Rectangle5.Dispose();
                ho_ObjectSelected3.Dispose();
                ho_Rectangle6.Dispose();
                ho_Regions.Dispose();
                ho_Rectangle1.Dispose();
                ho_Rectangle2.Dispose();
                ho_RegionUnion1.Dispose();
                ho_RegionUnion2.Dispose();
                ho_RegionOpening1.Dispose();
                ho_ConnectedRegions1.Dispose();
                ho_SelectedRegions1.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Regions1.Dispose();
                ho_ConnectedRegions4.Dispose();
                ho_SelectedRegions.Dispose();
                ho_Rectangle5.Dispose();
                ho_ObjectSelected3.Dispose();
                ho_Rectangle6.Dispose();
                ho_Regions.Dispose();
                ho_Rectangle1.Dispose();
                ho_Rectangle2.Dispose();
                ho_RegionUnion1.Dispose();
                ho_RegionUnion2.Dispose();
                ho_RegionOpening1.Dispose();
                ho_ConnectedRegions1.Dispose();
                ho_SelectedRegions1.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void get_boxes_lab_use_multi_threads(HObject ho_BoxImage, HObject ho_Rectangle,
            HTuple hv_medianKernal, HTuple hv_index, HTuple hv_standardTupleL1, HTuple hv_standardTupleA1,
            HTuple hv_standardTupleB1, out HTuple hv_standardTupleL, out HTuple hv_standardTupleA,
            out HTuple hv_standardTupleB)
        {




            // Local iconic variables 

            HObject ho_BoxImagedMedian, ho_Region1, ho_BoxImagedMedianValid;
            HObject ho_BoxImageR0, ho_BoxImageG0, ho_BoxImageB0, ho_BoxImageL;
            HObject ho_BoxImageA, ho_BoxImageB;

            // Local control variables 

            HTuple hv_MeanL = null, hv_DeviationL = null;
            HTuple hv_MeanA = null, hv_DeviationA = null, hv_MeanB = null;
            HTuple hv_DeviationB = null;
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_BoxImagedMedian);
            HOperatorSet.GenEmptyObj(out ho_Region1);
            HOperatorSet.GenEmptyObj(out ho_BoxImagedMedianValid);
            HOperatorSet.GenEmptyObj(out ho_BoxImageR0);
            HOperatorSet.GenEmptyObj(out ho_BoxImageG0);
            HOperatorSet.GenEmptyObj(out ho_BoxImageB0);
            HOperatorSet.GenEmptyObj(out ho_BoxImageL);
            HOperatorSet.GenEmptyObj(out ho_BoxImageA);
            HOperatorSet.GenEmptyObj(out ho_BoxImageB);
            try
            {
                //standardTupleL := standardTupleL1
                //standardTupleA := standardTupleA1
                //standardTupleB := standardTupleB1

                //中值滤波
                //median_image (BoxImage, BoxImagedMedian, 'circle', medianKernal*0.5, 'mirrored')
                ho_BoxImagedMedian.Dispose();
                HOperatorSet.MedianImage(ho_BoxImage, out ho_BoxImagedMedian, "circle", hv_medianKernal,
                    "mirrored");
                //动态阈值，并选取区域最大
                ho_Region1.Dispose();
                HOperatorSet.VarThreshold(ho_BoxImagedMedian, out ho_Region1, 25, 25, 0.1,
                    1, "equal");
                ho_BoxImagedMedianValid.Dispose();
                HOperatorSet.ReduceDomain(ho_BoxImagedMedian, ho_Region1, out ho_BoxImagedMedianValid
                    );

                ho_BoxImageR0.Dispose(); ho_BoxImageG0.Dispose(); ho_BoxImageB0.Dispose();
                HOperatorSet.Decompose3(ho_BoxImagedMedianValid, out ho_BoxImageR0, out ho_BoxImageG0,
                    out ho_BoxImageB0);
                ho_BoxImageL.Dispose(); ho_BoxImageA.Dispose(); ho_BoxImageB.Dispose();
                HOperatorSet.TransFromRgb(ho_BoxImageR0, ho_BoxImageG0, ho_BoxImageB0, out ho_BoxImageL,
                    out ho_BoxImageA, out ho_BoxImageB, "cielab");
                HOperatorSet.Intensity(ho_Rectangle, ho_BoxImageL, out hv_MeanL, out hv_DeviationL);
                HOperatorSet.Intensity(ho_Rectangle, ho_BoxImageA, out hv_MeanA, out hv_DeviationA);
                HOperatorSet.Intensity(ho_Rectangle, ho_BoxImageB, out hv_MeanB, out hv_DeviationB);

                HOperatorSet.TupleConcat(hv_standardTupleL1, hv_MeanL + (hv_index * 1000), out hv_standardTupleL);
                HOperatorSet.TupleConcat(hv_standardTupleA1, hv_MeanA + (hv_index * 1000), out hv_standardTupleA);
                HOperatorSet.TupleConcat(hv_standardTupleB1, hv_MeanA + (hv_index * 1000), out hv_standardTupleB);

                ho_BoxImagedMedian.Dispose();
                ho_Region1.Dispose();
                ho_BoxImagedMedianValid.Dispose();
                ho_BoxImageR0.Dispose();
                ho_BoxImageG0.Dispose();
                ho_BoxImageB0.Dispose();
                ho_BoxImageL.Dispose();
                ho_BoxImageA.Dispose();
                ho_BoxImageB.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_BoxImagedMedian.Dispose();
                ho_Region1.Dispose();
                ho_BoxImagedMedianValid.Dispose();
                ho_BoxImageR0.Dispose();
                ho_BoxImageG0.Dispose();
                ho_BoxImageB0.Dispose();
                ho_BoxImageL.Dispose();
                ho_BoxImageA.Dispose();
                ho_BoxImageB.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void get_algorithm(HTuple hv_l, HTuple hv_a, HTuple hv_b, HTuple hv_l1,
            HTuple hv_a1, HTuple hv_b1, HTuple hv_algorithm, out HTuple hv_theAberration)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_S_L = new HTuple(), hv_C_Std = new HTuple();
            HTuple hv_S_C = new HTuple(), hv_f = new HTuple(), hv_ATan = new HTuple();
            HTuple hv_H_Std = new HTuple(), hv_Cos = new HTuple();
            HTuple hv_Abs = new HTuple(), hv_T = new HTuple(), hv_S_H = new HTuple();
            HTuple hv_L_Diff = new HTuple(), hv_C = new HTuple(), hv_C_Diff = new HTuple();
            HTuple hv_H = new HTuple(), hv_H_Diff = new HTuple();
            // Initialize local and output iconic variables 
            hv_theAberration = new HTuple();


            if ((int)(new HTuple(hv_algorithm.TupleEqual(1))) != 0)
            {
                HOperatorSet.TupleSqrt((((hv_l1 - hv_l) * (hv_l1 - hv_l)) + ((hv_a1 - hv_a) * (hv_a1 - hv_a))) + ((hv_b1 - hv_b) * (hv_b1 - hv_b)),
                    out hv_theAberration);
            }
            else
            {
                if ((int)(new HTuple(hv_algorithm.TupleEqual(2))) != 0)
                {
                    if ((int)(new HTuple(hv_l.TupleGreater(16))) != 0)
                    {
                        hv_S_L = (0.040975 * hv_l) / (1 + (0.01765 * hv_l));
                    }
                    else
                    {
                        hv_S_L = 0.511;
                    }
                    HOperatorSet.TupleSqrt((hv_a * hv_a) + (hv_b * hv_b), out hv_C_Std);
                    hv_S_C = ((0.0638 * hv_C_Std) / (1 + (0.0131 * hv_C_Std))) + 0.638;
                    HOperatorSet.TupleSqrt((((hv_C_Std * hv_C_Std) * hv_C_Std) * hv_C_Std) / (1900 + (((hv_C_Std * hv_C_Std) * hv_C_Std) * hv_C_Std)),
                        out hv_f);
                    HOperatorSet.TupleAtan2(hv_b, hv_a, out hv_ATan);
                    hv_H_Std = (hv_ATan / 3.141659) * 180;
                    if ((int)((new HTuple(hv_H_Std.TupleGreater(345))).TupleOr(new HTuple(hv_H_Std.TupleLess(
                        164)))) != 0)
                    {
                        HOperatorSet.TupleCos(hv_H_Std + 35, out hv_Cos);
                        HOperatorSet.TupleAbs(0.4 * hv_Cos, out hv_Abs);
                        hv_T = 0.36 + hv_Abs;
                    }
                    else
                    {
                        HOperatorSet.TupleCos(hv_H_Std + 168, out hv_Cos);
                        HOperatorSet.TupleAbs(0.2 * hv_Cos, out hv_Abs);
                        hv_T = 0.56 + hv_Abs;
                    }
                    hv_S_H = hv_S_C * (((hv_T * hv_f) + 1) - hv_f);

                    hv_L_Diff = hv_l1 - hv_l;
                    HOperatorSet.TupleSqrt((hv_a1 * hv_a1) + (hv_b1 * hv_b1), out hv_C);
                    hv_C_Diff = hv_C - hv_C_Std;
                    HOperatorSet.TupleAtan2(hv_b1, hv_a1, out hv_ATan);
                    hv_H = (hv_ATan / 3.141659) * 180;
                    hv_H_Diff = hv_H - hv_H_Std;
                    HOperatorSet.TupleSqrt((((hv_L_Diff / hv_S_L) * (hv_L_Diff / hv_S_L)) + ((hv_C_Diff / hv_S_C) * (hv_C_Diff / hv_S_C))) + ((hv_H_Diff / hv_S_H) * (hv_H_Diff / hv_S_H)),
                        out hv_theAberration);
                }
            }






            return;
        }

        public void detection(HObject ho_Image, HObject ho_ImageWithDefect, HObject ho_windowHandle,
            HTuple hv_getStandardFlag, HTuple hv_standardTupleL, HTuple hv_standardTupleA,
            HTuple hv_standardTupleB, HTuple hv_algorithmOfAberration, HTuple hv_isSeperateComputer,
            out HTuple hv_result, out HTuple hv_tupleL, out HTuple hv_tupleA, out HTuple hv_tupleB,
            out HTuple hv_standardTupleLGeted, out HTuple hv_standardTupleAGeted, out HTuple hv_standardTupleBGeted,
            out HTuple hv_clothAberration, out HTuple hv_leftRightAberration, out HTuple hv_defectNumber,
            out HTuple hv_tupleDefectX, out HTuple hv_tupleDefectY, out HTuple hv_tupleDefectRadius,
            out HTuple hv_minWidth, out HTuple hv_maxWidth, out HTuple hv_meanWidth, out HTuple hv_metersCounter,
            out HTuple hv_tupleMessages, out HTuple hv_tupleMessagesColor, out HTuple hv_leftDetectSide,
            out HTuple hv_rightDetectSide)
        {



            // Initialize local and output iconic variables 
            hv_result = new HTuple();
            hv_tupleL = new HTuple();
            hv_tupleA = new HTuple();
            hv_tupleB = new HTuple();
            hv_standardTupleLGeted = new HTuple();
            hv_standardTupleAGeted = new HTuple();
            hv_standardTupleBGeted = new HTuple();
            hv_clothAberration = new HTuple();
            hv_leftRightAberration = new HTuple();
            hv_defectNumber = new HTuple();
            hv_tupleDefectX = new HTuple();
            hv_tupleDefectY = new HTuple();
            hv_tupleDefectRadius = new HTuple();
            hv_minWidth = new HTuple();
            hv_maxWidth = new HTuple();
            hv_meanWidth = new HTuple();
            hv_metersCounter = new HTuple();
            hv_tupleMessages = new HTuple();
            hv_tupleMessagesColor = new HTuple();
            hv_leftDetectSide = new HTuple();
            hv_rightDetectSide = new HTuple();

            return;
        }

        public void get_cloth_region_COPY_1(HObject ho_Image, out HObject ho_ImageR0,
            out HObject ho_ImageG0, out HObject ho_ImageB0, out HObject ho_ImageL, out HObject ho_ImageA,
            out HObject ho_ImageB, out HObject ho_ClothRegion, HTuple hv_leftSide, HTuple hv_rightSide,
            HTuple hv_Width, HTuple hv_Height)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_MidRectangle, ho_LeftRectangle;
            HObject ho_RightRectangle, ho_Regions1 = null, ho_ConnectedRegions4 = null;
            HObject ho_SelectedRegions = null, ho_Rectangle5 = null, ho_ObjectSelected3 = null;
            HObject ho_Rectangle6 = null, ho_Region = null, ho_Rectangle1 = null;
            HObject ho_Rectangle2 = null, ho_RegionUnion1 = null, ho_RegionUnion2 = null;
            HObject ho_RegionOpening1 = null, ho_ConnectedRegions1 = null;
            HObject ho_SelectedRegions1 = null;

            // Local control variables 

            HTuple hv_Mean11 = null, hv_Deviation11 = null;
            HTuple hv_Mean12 = null, hv_Deviation12 = null, hv_Mean13 = null;
            HTuple hv_Deviation13 = null, hv_Max1 = null, hv_Mean21 = null;
            HTuple hv_Mean22 = null, hv_Mean23 = null, hv_Max2 = null;
            HTuple hv_Mean31 = null, hv_Mean32 = null, hv_Mean33 = null;
            HTuple hv_Max3 = null, hv_Number = null, hv_UsedThreshold = new HTuple();
            HTuple hv_Area = new HTuple(), hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_Mean1 = new HTuple(), hv_Deviation1 = new HTuple();
            HTuple hv_i = new HTuple(), hv_Area1 = new HTuple(), hv_Row4 = new HTuple();
            HTuple hv_Column4 = new HTuple(), hv_Mean2 = new HTuple();
            HTuple hv_Deviation2 = new HTuple(), hv_Min2 = new HTuple();
            HTuple hv_leftSide_COPY_INP_TMP = hv_leftSide.Clone();
            HTuple hv_rightSide_COPY_INP_TMP = hv_rightSide.Clone();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageR0);
            HOperatorSet.GenEmptyObj(out ho_ImageG0);
            HOperatorSet.GenEmptyObj(out ho_ImageB0);
            HOperatorSet.GenEmptyObj(out ho_ImageL);
            HOperatorSet.GenEmptyObj(out ho_ImageA);
            HOperatorSet.GenEmptyObj(out ho_ImageB);
            HOperatorSet.GenEmptyObj(out ho_ClothRegion);
            HOperatorSet.GenEmptyObj(out ho_MidRectangle);
            HOperatorSet.GenEmptyObj(out ho_LeftRectangle);
            HOperatorSet.GenEmptyObj(out ho_RightRectangle);
            HOperatorSet.GenEmptyObj(out ho_Regions1);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions4);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_Rectangle5);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelected3);
            HOperatorSet.GenEmptyObj(out ho_Rectangle6);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            HOperatorSet.GenEmptyObj(out ho_Rectangle2);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion1);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion2);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening1);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            try
            {
                if ((int)(new HTuple(hv_leftSide_COPY_INP_TMP.TupleLess(1))) != 0)
                {
                    hv_leftSide_COPY_INP_TMP = 200;
                }
                if ((int)(new HTuple(hv_rightSide_COPY_INP_TMP.TupleLess(1))) != 0)
                {
                    hv_rightSide_COPY_INP_TMP = 200;
                }
                //
                ho_ClothRegion.Dispose();
                HOperatorSet.GenEmptyObj(out ho_ClothRegion);
                //图像分割是成RGB图
                ho_ImageR0.Dispose(); ho_ImageG0.Dispose(); ho_ImageB0.Dispose();
                HOperatorSet.Decompose3(ho_Image, out ho_ImageR0, out ho_ImageG0, out ho_ImageB0
                    );
                //将图像转换为LAB图像
                ho_ImageL.Dispose(); ho_ImageA.Dispose(); ho_ImageB.Dispose();
                HOperatorSet.TransFromRgb(ho_ImageR0, ho_ImageG0, ho_ImageB0, out ho_ImageL,
                    out ho_ImageA, out ho_ImageB, "cielab");
                //***********************************************************************************
                //
                //
                //***********************************************************************************
                //分别计算LAB图像布匹中间与两边的灰度差，将灰度差较大的作为布匹区域分割对象
                //***********************************************************************************
                ho_MidRectangle.Dispose();
                HOperatorSet.GenRectangle1(out ho_MidRectangle, (hv_Height / 2) - 300, (hv_Width / 2) - 300,
                    (hv_Height / 2) + 300, (hv_Width / 2) + 300);
                ho_LeftRectangle.Dispose();
                HOperatorSet.GenRectangle1(out ho_LeftRectangle, (hv_Height / 2) - 300, hv_leftSide_COPY_INP_TMP + 20,
                    (hv_Height / 2) + 300, hv_leftSide_COPY_INP_TMP + 150);
                ho_RightRectangle.Dispose();
                HOperatorSet.GenRectangle1(out ho_RightRectangle, (hv_Height / 2) - 300, (hv_Width - hv_rightSide_COPY_INP_TMP) - 150,
                    (hv_Height / 2) + 300, (hv_Width - hv_rightSide_COPY_INP_TMP) - 20);
                //
                //
                //
                HOperatorSet.Intensity(ho_MidRectangle, ho_ImageL, out hv_Mean11, out hv_Deviation11);
                HOperatorSet.Intensity(ho_LeftRectangle, ho_ImageL, out hv_Mean12, out hv_Deviation12);
                HOperatorSet.Intensity(ho_RightRectangle, ho_ImageL, out hv_Mean13, out hv_Deviation13);
                HOperatorSet.TupleMax2(hv_Mean11 - hv_Mean12, hv_Mean11 - hv_Mean13, out hv_Max1);
                HOperatorSet.TupleAbs(hv_Max1, out hv_Max1);
                //
                //
                HOperatorSet.Intensity(ho_MidRectangle, ho_ImageA, out hv_Mean21, out hv_Deviation11);
                HOperatorSet.Intensity(ho_LeftRectangle, ho_ImageA, out hv_Mean22, out hv_Deviation12);
                HOperatorSet.Intensity(ho_RightRectangle, ho_ImageA, out hv_Mean23, out hv_Deviation13);
                HOperatorSet.TupleMax2(hv_Mean21 - hv_Mean22, hv_Mean21 - hv_Mean23, out hv_Max2);
                HOperatorSet.TupleAbs(hv_Max2, out hv_Max2);
                //
                HOperatorSet.Intensity(ho_MidRectangle, ho_ImageB, out hv_Mean31, out hv_Deviation11);
                HOperatorSet.Intensity(ho_LeftRectangle, ho_ImageB, out hv_Mean32, out hv_Deviation12);
                HOperatorSet.Intensity(ho_RightRectangle, ho_ImageB, out hv_Mean33, out hv_Deviation13);
                HOperatorSet.TupleMax2(hv_Mean31 - hv_Mean32, hv_Mean31 - hv_Mean33, out hv_Max3);
                HOperatorSet.TupleAbs(hv_Max3, out hv_Max3);
                //***********************************************************************************
                //
                //***********************************************************************************
                //如AB图像的灰度差<5,且L图像灰度差小于10，则未找到布匹，否则分割布匹区域，SelectedRegions1
                //***********************************************************************************
                hv_Number = 0;
                if ((int)((new HTuple((new HTuple(hv_Max2.TupleLess(5))).TupleAnd(new HTuple(hv_Max3.TupleLess(
                    5))))).TupleAnd(new HTuple(hv_Max1.TupleLess(10)))) != 0)
                {
                    ho_MidRectangle.Dispose();
                    ho_LeftRectangle.Dispose();
                    ho_RightRectangle.Dispose();
                    ho_Regions1.Dispose();
                    ho_ConnectedRegions4.Dispose();
                    ho_SelectedRegions.Dispose();
                    ho_Rectangle5.Dispose();
                    ho_ObjectSelected3.Dispose();
                    ho_Rectangle6.Dispose();
                    ho_Region.Dispose();
                    ho_Rectangle1.Dispose();
                    ho_Rectangle2.Dispose();
                    ho_RegionUnion1.Dispose();
                    ho_RegionUnion2.Dispose();
                    ho_RegionOpening1.Dispose();
                    ho_ConnectedRegions1.Dispose();
                    ho_SelectedRegions1.Dispose();

                    return;
                }
                else
                {
                    if ((int)((new HTuple(hv_Max2.TupleGreater(5))).TupleOr(new HTuple(hv_Max3.TupleGreater(
                        5)))) != 0)
                    {
                        //***********************************************************************************
                        //在AB图像中选取色差大的图，作为分割布匹区域，SelectedRegions1
                        //***********************************************************************************
                        if ((int)(new HTuple(hv_Max2.TupleGreater(hv_Max3))) != 0)
                        {
                            //
                            if ((int)(new HTuple(((hv_Mean21 - hv_Mean22)).TupleLess(0))) != 0)
                            {
                                ho_Regions1.Dispose();
                                HOperatorSet.BinaryThreshold(ho_ImageA, out ho_Regions1, "max_separability",
                                    "dark", out hv_UsedThreshold);
                            }
                            else
                            {
                                ho_Regions1.Dispose();
                                HOperatorSet.BinaryThreshold(ho_ImageA, out ho_Regions1, "max_separability",
                                    "light", out hv_UsedThreshold);
                            }
                            //
                            ho_ConnectedRegions4.Dispose();
                            HOperatorSet.Connection(ho_Regions1, out ho_ConnectedRegions4);
                            ho_Regions1.Dispose();
                            HOperatorSet.SelectShape(ho_ConnectedRegions4, out ho_Regions1, (new HTuple("height")).TupleConcat(
                                "column"), "and", ((hv_Height - 100)).TupleConcat(hv_leftSide_COPY_INP_TMP + 300),
                                ((hv_Height + 100)).TupleConcat((hv_Width - hv_rightSide_COPY_INP_TMP) - 300));
                            HOperatorSet.CountObj(ho_Regions1, out hv_Number);
                            if ((int)(new HTuple(hv_Number.TupleGreater(1))) != 0)
                            {
                                ho_SelectedRegions.Dispose();
                                HOperatorSet.SelectShapeStd(ho_Regions1, out ho_SelectedRegions, "max_area",
                                    80);
                                HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row,
                                    out hv_Column);
                                ho_Rectangle5.Dispose();
                                HOperatorSet.GenRectangle1(out ho_Rectangle5, hv_Row - 500, hv_Column - 500,
                                    hv_Row + 500, hv_Column + 500);
                                HOperatorSet.Intensity(ho_Rectangle5, ho_ImageA, out hv_Mean1, out hv_Deviation1);
                                HTuple end_val71 = hv_Number;
                                HTuple step_val71 = 1;
                                for (hv_i = 1; hv_i.Continue(end_val71, step_val71); hv_i = hv_i.TupleAdd(step_val71))
                                {
                                    ho_ObjectSelected3.Dispose();
                                    HOperatorSet.SelectObj(ho_Regions1, out ho_ObjectSelected3, hv_i);
                                    HOperatorSet.AreaCenter(ho_ObjectSelected3, out hv_Area1, out hv_Row4,
                                        out hv_Column4);
                                    if ((int)(new HTuple(hv_Area1.TupleNotEqual(hv_Area))) != 0)
                                    {
                                        ho_Rectangle6.Dispose();
                                        HOperatorSet.GenRectangle1(out ho_Rectangle6, hv_Row4 - 500, hv_Column4 - 30,
                                            hv_Row4 + 500, hv_Column4 + 30);
                                        HOperatorSet.Intensity(ho_Rectangle6, ho_ImageA, out hv_Mean2,
                                            out hv_Deviation2);
                                        if ((int)((new HTuple(((hv_Mean2 - hv_Mean1)).TupleGreater(-10))).TupleAnd(
                                            new HTuple(((hv_Mean2 - hv_Mean1)).TupleLess(10)))) != 0)
                                        {
                                            {
                                                HObject ExpTmpOutVar_0;
                                                HOperatorSet.Union2(ho_SelectedRegions, ho_ObjectSelected3, out ExpTmpOutVar_0
                                                    );
                                                ho_SelectedRegions.Dispose();
                                                ho_SelectedRegions = ExpTmpOutVar_0;
                                            }
                                        }
                                    }
                                    //
                                }
                                ho_Regions1.Dispose();
                                HOperatorSet.Connection(ho_SelectedRegions, out ho_Regions1);
                            }
                        }
                        else
                        {
                            //
                            if ((int)(new HTuple(((hv_Mean31 - hv_Mean32)).TupleLess(0))) != 0)
                            {
                                ho_Regions1.Dispose();
                                HOperatorSet.BinaryThreshold(ho_ImageB, out ho_Regions1, "max_separability",
                                    "dark", out hv_UsedThreshold);
                            }
                            else
                            {
                                ho_Regions1.Dispose();
                                HOperatorSet.BinaryThreshold(ho_ImageB, out ho_Regions1, "max_separability",
                                    "light", out hv_UsedThreshold);
                            }
                            ho_ConnectedRegions4.Dispose();
                            HOperatorSet.Connection(ho_Regions1, out ho_ConnectedRegions4);
                            ho_Regions1.Dispose();
                            HOperatorSet.SelectShape(ho_ConnectedRegions4, out ho_Regions1, (new HTuple("height")).TupleConcat(
                                "column"), "and", ((hv_Height - 100)).TupleConcat(hv_leftSide_COPY_INP_TMP + 300),
                                ((hv_Height + 100)).TupleConcat((hv_Width - hv_rightSide_COPY_INP_TMP) - 300));
                            HOperatorSet.CountObj(ho_Regions1, out hv_Number);
                            if ((int)(new HTuple(hv_Number.TupleGreater(1))) != 0)
                            {
                                //***********************************************************************************
                                //如果区域个数大于1，选取与最大区域灰度接近的区域并与最大区域连到一起
                                //****************************************************************u*******************
                                ho_SelectedRegions.Dispose();
                                HOperatorSet.SelectShapeStd(ho_Regions1, out ho_SelectedRegions, "max_area",
                                    80);
                                HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row,
                                    out hv_Column);
                                ho_Rectangle5.Dispose();
                                HOperatorSet.GenRectangle1(out ho_Rectangle5, hv_Row - 500, hv_Column - 500,
                                    hv_Row + 500, hv_Column + 500);
                                HOperatorSet.Intensity(ho_Rectangle5, ho_ImageB, out hv_Mean1, out hv_Deviation1);
                                HTuple end_val103 = hv_Number;
                                HTuple step_val103 = 1;
                                for (hv_i = 1; hv_i.Continue(end_val103, step_val103); hv_i = hv_i.TupleAdd(step_val103))
                                {
                                    ho_ObjectSelected3.Dispose();
                                    HOperatorSet.SelectObj(ho_Regions1, out ho_ObjectSelected3, hv_i);
                                    HOperatorSet.AreaCenter(ho_ObjectSelected3, out hv_Area1, out hv_Row4,
                                        out hv_Column4);
                                    if ((int)(new HTuple(hv_Area1.TupleNotEqual(hv_Area))) != 0)
                                    {
                                        ho_Rectangle6.Dispose();
                                        HOperatorSet.GenRectangle1(out ho_Rectangle6, hv_Row4 - 500, hv_Column4 - 30,
                                            hv_Row4 + 500, hv_Column4 + 30);
                                        HOperatorSet.Intensity(ho_Rectangle6, ho_ImageB, out hv_Mean2,
                                            out hv_Deviation2);
                                        if ((int)((new HTuple(((hv_Mean2 - hv_Mean1)).TupleGreater(-10))).TupleAnd(
                                            new HTuple(((hv_Mean2 - hv_Mean1)).TupleLess(10)))) != 0)
                                        {
                                            {
                                                HObject ExpTmpOutVar_0;
                                                HOperatorSet.Union2(ho_SelectedRegions, ho_ObjectSelected3, out ExpTmpOutVar_0
                                                    );
                                                ho_SelectedRegions.Dispose();
                                                ho_SelectedRegions = ExpTmpOutVar_0;
                                            }
                                        }
                                    }
                                    //
                                }
                                ho_Regions1.Dispose();
                                HOperatorSet.Connection(ho_SelectedRegions, out ho_Regions1);
                            }
                            //
                        }
                        //
                    }
                    else
                    {
                        HOperatorSet.TupleMin2(hv_Mean12, hv_Mean13, out hv_Min2);
                        HOperatorSet.TupleMax2(hv_Mean12, hv_Mean13, out hv_Max2);
                        if ((int)(new HTuple(hv_Mean11.TupleLess(hv_Max2))) != 0)
                        {
                            if ((int)(new HTuple(hv_Max1.TupleGreater(40))) != 0)
                            {
                                ho_Region.Dispose();
                                HOperatorSet.Threshold(ho_ImageL, out ho_Region, 10, hv_Max2 - 20);
                            }
                            else if ((int)(new HTuple(hv_Max1.TupleGreater(30))) != 0)
                            {
                                ho_Region.Dispose();
                                HOperatorSet.Threshold(ho_ImageL, out ho_Region, 10, hv_Max2 - 10);
                            }
                            else
                            {
                                ho_Region.Dispose();
                                HOperatorSet.Threshold(ho_ImageL, out ho_Region, 10, 0.5 * (hv_Max2 + hv_Mean11));
                            }
                        }
                        else
                        {
                            if ((int)(new HTuple(hv_Max1.TupleGreater(40))) != 0)
                            {
                                ho_Region.Dispose();
                                HOperatorSet.Threshold(ho_ImageL, out ho_Region, hv_Max2 + 25, 255);
                            }
                            else if ((int)(new HTuple(hv_Max1.TupleGreater(30))) != 0)
                            {
                                ho_Region.Dispose();
                                HOperatorSet.Threshold(ho_ImageL, out ho_Region, hv_Max2 + 18, 255);
                            }
                            else
                            {
                                ho_Region.Dispose();
                                HOperatorSet.Threshold(ho_ImageL, out ho_Region, 0.5 * (hv_Max2 + hv_Mean11),
                                    255);
                            }
                            //
                        }
                        //
                        ho_ConnectedRegions4.Dispose();
                        HOperatorSet.Connection(ho_Region, out ho_ConnectedRegions4);
                        ho_Regions1.Dispose();
                        HOperatorSet.SelectShape(ho_ConnectedRegions4, out ho_Regions1, (new HTuple("height")).TupleConcat(
                            "column"), "and", ((hv_Height - 100)).TupleConcat(hv_leftSide_COPY_INP_TMP + 100),
                            ((hv_Height + 100)).TupleConcat((hv_Width - hv_rightSide_COPY_INP_TMP) - 100));
                        HOperatorSet.CountObj(ho_Regions1, out hv_Number);
                        if ((int)(new HTuple(hv_Number.TupleGreater(1))) != 0)
                        {
                            ho_SelectedRegions.Dispose();
                            HOperatorSet.SelectShapeStd(ho_Regions1, out ho_SelectedRegions, "max_area",
                                80);
                            HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row,
                                out hv_Column);
                            ho_Rectangle5.Dispose();
                            HOperatorSet.GenRectangle1(out ho_Rectangle5, hv_Row - 500, hv_Column - 500,
                                hv_Row + 500, hv_Column + 500);
                            HOperatorSet.Intensity(ho_Rectangle5, ho_ImageL, out hv_Mean1, out hv_Deviation1);
                            HTuple end_val150 = hv_Number;
                            HTuple step_val150 = 1;
                            for (hv_i = 1; hv_i.Continue(end_val150, step_val150); hv_i = hv_i.TupleAdd(step_val150))
                            {
                                ho_ObjectSelected3.Dispose();
                                HOperatorSet.SelectObj(ho_Regions1, out ho_ObjectSelected3, hv_i);
                                HOperatorSet.AreaCenter(ho_ObjectSelected3, out hv_Area1, out hv_Row4,
                                    out hv_Column4);
                                if ((int)(new HTuple(hv_Area1.TupleNotEqual(hv_Area))) != 0)
                                {
                                    ho_Rectangle6.Dispose();
                                    HOperatorSet.GenRectangle1(out ho_Rectangle6, hv_Row4 - 500, hv_Column4 - 30,
                                        hv_Row4 + 500, hv_Column4 + 30);
                                    HOperatorSet.Intensity(ho_Rectangle6, ho_ImageL, out hv_Mean2, out hv_Deviation2);
                                    if ((int)((new HTuple(((hv_Mean2 - hv_Mean1)).TupleGreater(-10))).TupleAnd(
                                        new HTuple(((hv_Mean2 - hv_Mean1)).TupleLess(10)))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_SelectedRegions, ho_ObjectSelected3, out ExpTmpOutVar_0
                                                );
                                            ho_SelectedRegions.Dispose();
                                            ho_SelectedRegions = ExpTmpOutVar_0;
                                        }
                                    }
                                }
                            }
                            ho_Regions1.Dispose();
                            HOperatorSet.Connection(ho_SelectedRegions, out ho_Regions1);
                        }
                        //
                    }
                    //
                    //***********************************************************************************
                    //防止周期性缺陷
                    //***********************************************************************************
                    ho_Rectangle1.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle1, 1, 50 + hv_leftSide_COPY_INP_TMP,
                        3, (hv_Width - hv_rightSide_COPY_INP_TMP) - 50);
                    ho_Rectangle2.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle2, hv_Height - 3, 50 + hv_leftSide_COPY_INP_TMP,
                        hv_Height, (hv_Width - hv_rightSide_COPY_INP_TMP) - 50);
                    ho_RegionUnion1.Dispose();
                    HOperatorSet.Union2(ho_Rectangle1, ho_Rectangle2, out ho_RegionUnion1);
                    ho_RegionUnion2.Dispose();
                    HOperatorSet.Union2(ho_RegionUnion1, ho_Regions1, out ho_RegionUnion2);
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.FillUp(ho_RegionUnion2, out ExpTmpOutVar_0);
                        ho_RegionUnion2.Dispose();
                        ho_RegionUnion2 = ExpTmpOutVar_0;
                    }
                    ho_RegionOpening1.Dispose();
                    HOperatorSet.OpeningRectangle1(ho_RegionUnion2, out ho_RegionOpening1, 1,
                        7);
                    //***********************************************************************************
                    ho_ConnectedRegions1.Dispose();
                    HOperatorSet.Connection(ho_RegionOpening1, out ho_ConnectedRegions1);
                    ho_SelectedRegions1.Dispose();
                    HOperatorSet.SelectShape(ho_ConnectedRegions1, out ho_SelectedRegions1, (new HTuple("height")).TupleConcat(
                        "column"), "and", ((hv_Height - 100)).TupleConcat(hv_leftSide_COPY_INP_TMP + 300),
                        ((hv_Height + 100)).TupleConcat((hv_Width - hv_rightSide_COPY_INP_TMP) - 300));
                    HOperatorSet.CountObj(ho_SelectedRegions1, out hv_Number);
                }
                //***********************************************************************************
                ho_ClothRegion.Dispose();
                HOperatorSet.FillUp(ho_SelectedRegions1, out ho_ClothRegion);
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ClosingRectangle1(ho_ClothRegion, out ExpTmpOutVar_0, 10, 60);
                    ho_ClothRegion.Dispose();
                    ho_ClothRegion = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.OpeningRectangle1(ho_ClothRegion, out ExpTmpOutVar_0, 1, 200);
                    ho_ClothRegion.Dispose();
                    ho_ClothRegion = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ClosingRectangle1(ho_ClothRegion, out ExpTmpOutVar_0, 60, 20);
                    ho_ClothRegion.Dispose();
                    ho_ClothRegion = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.OpeningRectangle1(ho_ClothRegion, out ExpTmpOutVar_0, 1, 10);
                    ho_ClothRegion.Dispose();
                    ho_ClothRegion = ExpTmpOutVar_0;
                }
                //
                ho_MidRectangle.Dispose();
                ho_LeftRectangle.Dispose();
                ho_RightRectangle.Dispose();
                ho_Regions1.Dispose();
                ho_ConnectedRegions4.Dispose();
                ho_SelectedRegions.Dispose();
                ho_Rectangle5.Dispose();
                ho_ObjectSelected3.Dispose();
                ho_Rectangle6.Dispose();
                ho_Region.Dispose();
                ho_Rectangle1.Dispose();
                ho_Rectangle2.Dispose();
                ho_RegionUnion1.Dispose();
                ho_RegionUnion2.Dispose();
                ho_RegionOpening1.Dispose();
                ho_ConnectedRegions1.Dispose();
                ho_SelectedRegions1.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_MidRectangle.Dispose();
                ho_LeftRectangle.Dispose();
                ho_RightRectangle.Dispose();
                ho_Regions1.Dispose();
                ho_ConnectedRegions4.Dispose();
                ho_SelectedRegions.Dispose();
                ho_Rectangle5.Dispose();
                ho_ObjectSelected3.Dispose();
                ho_Rectangle6.Dispose();
                ho_Region.Dispose();
                ho_Rectangle1.Dispose();
                ho_Rectangle2.Dispose();
                ho_RegionUnion1.Dispose();
                ho_RegionUnion2.Dispose();
                ho_RegionOpening1.Dispose();
                ho_ConnectedRegions1.Dispose();
                ho_SelectedRegions1.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void get_boxes_color_COPY_1(HObject ho_Image, out HObject ho_Boxs, HTuple hv_boxNumber,
            HTuple hv_boxBenginX, HTuple hv_boxWidth, HTuple hv_boxHeight, HTuple hv_Width,
            HTuple hv_Height, HTuple hv_medianKernal, out HTuple hv_standardTupleL, out HTuple hv_standardTupleA,
            out HTuple hv_standardTupleB)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_Rectangle = null, ho_BoxImage = null;
            HObject ho_BoxImagedMedian = null, ho_Region1 = null, ho_BoxImagedMedianValid = null;
            HObject ho_BoxImageR0 = null, ho_BoxImageG0 = null, ho_BoxImageB0 = null;
            HObject ho_BoxImageL = null, ho_BoxImageA = null, ho_BoxImageB = null;

            // Local control variables 

            HTuple hv_message = new HTuple(), hv_tupleMessages = new HTuple();
            HTuple hv_color = new HTuple(), hv_tupleMessagesColor = new HTuple();
            HTuple hv_boxDistance = new HTuple(), hv_boxBenginY = new HTuple();
            HTuple hv_tupleDeviationL = new HTuple(), hv_tupleDeviationA = new HTuple();
            HTuple hv_tupleDeviationB = new HTuple(), hv_i = new HTuple();
            HTuple hv_MeanL = new HTuple(), hv_DeviationL = new HTuple();
            HTuple hv_MeanA = new HTuple(), hv_DeviationA = new HTuple();
            HTuple hv_MeanB = new HTuple(), hv_DeviationB = new HTuple();
            HTuple hv_standardTupleL1 = new HTuple(), hv_standardTupleA1 = new HTuple();
            HTuple hv_standardTupleB1 = new HTuple(), hv_totalL = new HTuple();
            HTuple hv_totalA = new HTuple(), hv_totalB = new HTuple();
            HTuple hv_k = new HTuple(), hv_standardL = new HTuple();
            HTuple hv_standardA = new HTuple(), hv_standardB = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Boxs);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_BoxImage);
            HOperatorSet.GenEmptyObj(out ho_BoxImagedMedian);
            HOperatorSet.GenEmptyObj(out ho_Region1);
            HOperatorSet.GenEmptyObj(out ho_BoxImagedMedianValid);
            HOperatorSet.GenEmptyObj(out ho_BoxImageR0);
            HOperatorSet.GenEmptyObj(out ho_BoxImageG0);
            HOperatorSet.GenEmptyObj(out ho_BoxImageB0);
            HOperatorSet.GenEmptyObj(out ho_BoxImageL);
            HOperatorSet.GenEmptyObj(out ho_BoxImageA);
            HOperatorSet.GenEmptyObj(out ho_BoxImageB);
            hv_standardTupleL = new HTuple();
            hv_standardTupleA = new HTuple();
            hv_standardTupleB = new HTuple();
            try
            {
                ho_Boxs.Dispose();
                HOperatorSet.GenEmptyObj(out ho_Boxs);
                if ((int)(new HTuple(hv_boxNumber.TupleLessEqual(0))) != 0)
                {
                    //检测框个数必须大于0
                    hv_message = "检测框个数必须大于0";
                    HOperatorSet.TupleConcat(hv_tupleMessages, hv_message, out hv_tupleMessages);
                    hv_color = "red";
                    HOperatorSet.TupleConcat(hv_tupleMessagesColor, hv_color, out hv_tupleMessagesColor);
                }
                else
                {
                    //标准色差检测
                    //boxDistance表示两个框间距
                    hv_boxDistance = ((hv_Width - (hv_boxNumber * hv_boxWidth)) - (hv_boxBenginX * 2)) / (hv_boxNumber - 1);
                    //，boxBenginY框起始y坐标
                    hv_boxBenginY = (hv_Height - hv_boxHeight) / 2;
                    hv_tupleDeviationL = new HTuple();
                    hv_tupleDeviationA = new HTuple();
                    hv_tupleDeviationB = new HTuple();
                    //定义检测框
                    ho_Boxs.Dispose();
                    HOperatorSet.GenEmptyObj(out ho_Boxs);
                    HTuple end_val18 = hv_boxNumber - 1;
                    HTuple step_val18 = 1;
                    for (hv_i = 0; hv_i.Continue(end_val18, step_val18); hv_i = hv_i.TupleAdd(step_val18))
                    {
                        ho_Rectangle.Dispose();
                        HOperatorSet.GenRectangle1(out ho_Rectangle, hv_boxBenginY, hv_boxBenginX + (hv_i * (hv_boxWidth + hv_boxDistance)),
                            hv_boxBenginY + hv_boxHeight, (hv_boxBenginX + (hv_i * (hv_boxWidth + hv_boxDistance))) + hv_boxWidth);
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.Union2(ho_Boxs, ho_Rectangle, out ExpTmpOutVar_0);
                            ho_Boxs.Dispose();
                            ho_Boxs = ExpTmpOutVar_0;
                        }
                        ho_BoxImage.Dispose();
                        HOperatorSet.ReduceDomain(ho_Image, ho_Rectangle, out ho_BoxImage);
                        //
                        //中值滤波
                        //median_image (BoxImage, BoxImagedMedian, 'circle', medianKernal*0.5, 'mirrored')
                        ho_BoxImagedMedian.Dispose();
                        HOperatorSet.MedianImage(ho_BoxImage, out ho_BoxImagedMedian, "circle",
                            1, "mirrored");
                        //动态阈值，并选取区域最大
                        ho_Region1.Dispose();
                        HOperatorSet.VarThreshold(ho_BoxImagedMedian, out ho_Region1, 25, 25, 0.1,
                            1, "equal");
                        ho_BoxImagedMedianValid.Dispose();
                        HOperatorSet.ReduceDomain(ho_BoxImagedMedian, ho_Region1, out ho_BoxImagedMedianValid
                            );
                        //
                        ho_BoxImageR0.Dispose(); ho_BoxImageG0.Dispose(); ho_BoxImageB0.Dispose();
                        HOperatorSet.Decompose3(ho_BoxImagedMedianValid, out ho_BoxImageR0, out ho_BoxImageG0,
                            out ho_BoxImageB0);
                        ho_BoxImageL.Dispose(); ho_BoxImageA.Dispose(); ho_BoxImageB.Dispose();
                        HOperatorSet.TransFromRgb(ho_BoxImageR0, ho_BoxImageG0, ho_BoxImageB0,
                            out ho_BoxImageL, out ho_BoxImageA, out ho_BoxImageB, "cielab");
                        //
                        HOperatorSet.Intensity(ho_Rectangle, ho_BoxImageL, out hv_MeanL, out hv_DeviationL);
                        HOperatorSet.Intensity(ho_Rectangle, ho_BoxImageA, out hv_MeanA, out hv_DeviationA);
                        HOperatorSet.Intensity(ho_Rectangle, ho_BoxImageB, out hv_MeanB, out hv_DeviationB);
                        if (hv_standardTupleL == null)
                            hv_standardTupleL = new HTuple();
                        hv_standardTupleL[hv_i] = hv_MeanL;
                        if (hv_standardTupleA == null)
                            hv_standardTupleA = new HTuple();
                        hv_standardTupleA[hv_i] = hv_MeanA;
                        if (hv_standardTupleB == null)
                            hv_standardTupleB = new HTuple();
                        hv_standardTupleB[hv_i] = hv_MeanB;
                        if (hv_tupleDeviationL == null)
                            hv_tupleDeviationL = new HTuple();
                        hv_tupleDeviationL[hv_i] = hv_DeviationL;
                        if (hv_tupleDeviationA == null)
                            hv_tupleDeviationA = new HTuple();
                        hv_tupleDeviationA[hv_i] = hv_DeviationA;
                        if (hv_tupleDeviationB == null)
                            hv_tupleDeviationB = new HTuple();
                        hv_tupleDeviationB[hv_i] = hv_DeviationB;
                    }
                    //
                    //***********************************************************************************
                    //去掉最大、最小的，求取标准色差
                    //***********************************************************************************
                    HOperatorSet.TupleSort(hv_standardTupleL, out hv_standardTupleL1);
                    HOperatorSet.TupleSort(hv_standardTupleA, out hv_standardTupleA1);
                    HOperatorSet.TupleSort(hv_standardTupleB, out hv_standardTupleB1);
                    if ((int)(new HTuple(hv_boxNumber.TupleGreater(2))) != 0)
                    {
                        hv_totalL = 0;
                        hv_totalA = 0;
                        hv_totalB = 0;
                        HTuple end_val54 = hv_boxNumber - 2;
                        HTuple step_val54 = 1;
                        for (hv_k = 1; hv_k.Continue(end_val54, step_val54); hv_k = hv_k.TupleAdd(step_val54))
                        {
                            hv_totalL = hv_totalL + (hv_standardTupleL1.TupleSelect(hv_k));
                            hv_totalA = hv_totalA + (hv_standardTupleA1.TupleSelect(hv_k));
                            hv_totalB = hv_totalB + (hv_standardTupleB1.TupleSelect(hv_k));
                        }
                        hv_standardL = hv_totalL / (hv_boxNumber - 2);
                        hv_standardA = hv_totalA / (hv_boxNumber - 2);
                        hv_standardB = hv_totalB / (hv_boxNumber - 2);
                    }
                    else
                    {
                        hv_standardL = hv_standardTupleL1.TupleSelect(0);
                        hv_standardA = hv_standardTupleA1.TupleSelect(0);
                        hv_standardB = hv_standardTupleB1.TupleSelect(0);
                    }
                }
                //
                //
                ho_Rectangle.Dispose();
                ho_BoxImage.Dispose();
                ho_BoxImagedMedian.Dispose();
                ho_Region1.Dispose();
                ho_BoxImagedMedianValid.Dispose();
                ho_BoxImageR0.Dispose();
                ho_BoxImageG0.Dispose();
                ho_BoxImageB0.Dispose();
                ho_BoxImageL.Dispose();
                ho_BoxImageA.Dispose();
                ho_BoxImageB.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Rectangle.Dispose();
                ho_BoxImage.Dispose();
                ho_BoxImagedMedian.Dispose();
                ho_Region1.Dispose();
                ho_BoxImagedMedianValid.Dispose();
                ho_BoxImageR0.Dispose();
                ho_BoxImageG0.Dispose();
                ho_BoxImageB0.Dispose();
                ho_BoxImageL.Dispose();
                ho_BoxImageA.Dispose();
                ho_BoxImageB.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Main procedure 
        private void action()
        {


            // Local iconic variables 

            HObject ho_DefectRegion, ho_Image = null, ho_Boxs = null;
            HObject ho_ImageDump = null, ho_ImageWithDefect = null;

            // Local control variables 

            HTuple hv_magnification = null, hv_leftSide = null;
            HTuple hv_rightSide = null, hv_boxNumber = null, hv_boxWidth = null;
            HTuple hv_boxHeight = null, hv_boxBenginX = null, hv_dynThresh = null;
            HTuple hv_medianKernal = null, hv_thresh = null, hv_defectArea = null;
            HTuple hv_defectWidth = null, hv_defectHeight = null, hv_edgeRollSlope = null;
            HTuple hv_imperfectBorderWidth = null, hv_clothAberrationGrad1 = null;
            HTuple hv_clothAberrationGrad2 = null, hv_clothAberrationGrad3 = null;
            HTuple hv_clothAberrationGrad4 = null, hv_result = null;
            HTuple hv_tupleL = null, hv_tupleA = null, hv_tupleB = null;
            HTuple hv_standardTupleL = null, hv_standardTupleA = null;
            HTuple hv_standardTupleB = null, hv_defectNumber = null;
            HTuple hv_tupleDefectX = null, hv_tupleDefectY = null;
            HTuple hv_tupleDefectRadius = null, hv_minWidth = null;
            HTuple hv_maxWidth = null, hv_meanWidth = null, hv_flag = null;
            HTuple hv_ins = null, hv_windowHandle = new HTuple(), hv_tupleDetectResult = new HTuple();
            HTuple hv_tupleDefectClass = new HTuple(), hv_L = new HTuple();
            HTuple hv_A = new HTuple(), hv_B = new HTuple(), hv_clothAberration = new HTuple();
            HTuple hv_tupleMessages = new HTuple(), hv_tupleMessagesColor = new HTuple();
            HTuple hv_tupleDefectRow1 = new HTuple(), hv_tupleDefectRow2 = new HTuple();
            HTuple hv_tupleDefectColumn1 = new HTuple(), hv_tupleDefectColumn2 = new HTuple();
            HTuple hv_clothSideUnDetectWidth = new HTuple(), hv_isSeperateComputer = new HTuple();
            HTuple hv_metersCounter = new HTuple(), hv_algorithmOfAberration = new HTuple();
            HTuple hv_standardL = new HTuple(), hv_standardA = new HTuple();
            HTuple hv_standardB = new HTuple(), hv_leftDetectSide = new HTuple();
            HTuple hv_rightDetectSide = new HTuple(), hv_ClothRegionCoordinateX1 = new HTuple();
            HTuple hv_ClothRegionCoordinateX2 = new HTuple(), hv_tupleBackgroundColor = new HTuple();
            HTuple hv_leftRightAberration2 = new HTuple(), hv_clothRegionCoordinateX1 = new HTuple();
            HTuple hv_clothRegionCoordinateX2 = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_DefectRegion);
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_Boxs);
            HOperatorSet.GenEmptyObj(out ho_ImageDump);
            HOperatorSet.GenEmptyObj(out ho_ImageWithDefect);
            try
            {
                //*********************输入参数
                //magnification放大率piexels/mm
                //LeftSide左边有效参数
                //RightSide右边有效参数
                //boxNumber框个数
                //boxWidth框宽度
                //boxHeight框高度
                //boxBenginX框起始X坐标
                //dynThresh缺陷阈值
                //medianKernal滤波卷积核大小
                //defectArea缺陷面积
                //edgeRollSlope判断卷边的斜率偏差
                //imperfectBorderWidth判断缺边的宽度
                //leftSide左边有效区域
                //rightSide右边有效区域
                //clothAberrationGrad1-clothAberrationGrad4色差等级分类
                //*****************************
                hv_magnification = 4.8188;
                hv_leftSide = 41.5040;
                hv_rightSide = 41.5040;
                hv_boxNumber = 6;
                hv_boxWidth = 83;
                hv_boxHeight = 83;
                hv_boxBenginX = 200;
                hv_dynThresh = 15;
                hv_medianKernal = 5;
                hv_thresh = 30;
                hv_defectArea = 0.2157;
                hv_defectWidth = 1.0393;
                hv_defectHeight = 1.0393;
                hv_edgeRollSlope = 0.1;
                hv_imperfectBorderWidth = 4.15;
                hv_clothAberrationGrad1 = 0.5;
                hv_clothAberrationGrad2 = 1.5;
                hv_clothAberrationGrad3 = 3.0;
                hv_clothAberrationGrad4 = 6.0;
                //**********************输出参数
                //result检测结果，0表示没有缺陷，1表示未找到布匹，2表示布匹接缝，3表示检测到缺陷
                //tupleL/A/B六个框的LAB值
                //standardTupleL/A/B标准LAB值
                //defectNumber检测到的缺陷个数
                //tupleDefectX缺陷中心X坐标合集
                //tupleDefectY缺陷中心Y坐标合集
                //tupleDefectRadius缺陷半径合集
                //minWidth最小宽度
                //maxWidth最大宽度
                //meanWidth平均宽度
                //*****************************
                hv_result = 0;
                hv_tupleL = new HTuple();
                hv_tupleA = new HTuple();
                hv_tupleB = new HTuple();
                hv_standardTupleL = new HTuple();
                hv_standardTupleA = new HTuple();
                hv_standardTupleB = new HTuple();
                hv_defectNumber = 0;
                hv_tupleDefectX = new HTuple();
                hv_tupleDefectY = new HTuple();
                hv_tupleDefectRadius = new HTuple();
                hv_minWidth = 0;
                hv_maxWidth = 0;
                hv_meanWidth = 0;
                ho_DefectRegion.Dispose();
                HOperatorSet.GenEmptyObj(out ho_DefectRegion);
                //read_image (Image1, 'C:/Users/Administrator/Desktop/b9a07ccb082531c2e9b6733e783e6cd.jpg')


                hv_flag = 0;

                for (hv_ins = 0; (int)hv_ins <= 20; hv_ins = (int)hv_ins + 1)
                {

                    ho_Image.Dispose();
                    HOperatorSet.ReadImage(out ho_Image, hv_ins + ".jpg");
                    //read_image (Image, 'E:/项目/布匹色差检测-缺陷识别/检测图片存档/2019年3月28日 星期四/3005/'+ins+'.jpg')
                    //dev_open_window(...);

                    //tupleDetectResult表示各个缺陷的个数,>0为有缺陷
                    //tupleDetectResult[0]表示接缝个数，严重，亮红灯
                    //tupleDetectResult[1]表示周期性缺陷个数，严重，亮红灯
                    //tupleDetectResult[2]表示卷边，严重，亮红灯
                    //tupleDetectResult[3]表示缺边个数，黄灯
                    //tupleDetectResult[4]表示点瑕疵个数，黄灯
                    //***********************************
                    hv_tupleDetectResult = new HTuple();
                    hv_tupleDetectResult[0] = 0;
                    hv_tupleDetectResult[1] = 0;
                    hv_tupleDetectResult[2] = 0;
                    hv_tupleDetectResult[3] = 0;
                    hv_tupleDetectResult[4] = 0;
                    hv_tupleDetectResult[5] = 0;
                    hv_tupleDetectResult[6] = 0;
                    hv_tupleDetectResult[7] = 0;
                    hv_tupleDetectResult[8] = 0;
                    hv_tupleDetectResult[9] = 0;
                    //瑕疵X坐标
                    hv_tupleDefectX = new HTuple();
                    //瑕疵Y坐标
                    hv_tupleDefectY = new HTuple();
                    //tupleDefectClass表示瑕疵分类,0表示接，1表示周期性缺陷，2表示卷边，3表示缺边，4表示其他瑕疵
                    hv_tupleDefectClass = new HTuple();
                    hv_L = 0;
                    hv_A = 0;
                    hv_B = 0;
                    //clothberration表示色差值
                    hv_clothAberration = 0;
                    hv_minWidth = 0;
                    hv_maxWidth = 0;
                    hv_meanWidth = 0;
                    //输出结果
                    hv_tupleMessages = new HTuple();
                    hv_tupleMessagesColor = new HTuple();
                    //检测缺陷的个数
                    hv_defectNumber = 0;
                    //缺陷框坐标
                    hv_tupleDefectRow1 = new HTuple();
                    hv_tupleDefectRow2 = new HTuple();
                    hv_tupleDefectColumn1 = new HTuple();
                    hv_tupleDefectColumn2 = new HTuple();

                    //*********************输入参数
                    //magnification放大率piexels/mm
                    //LeftSide左边有效参数
                    //RightSide右边有效参数
                    //boxNumber框个数
                    //boxWidth框宽度
                    //boxHeight框高度
                    //boxBenginX框起始X坐标
                    //dynThresh缺陷阈值
                    //medianKernal滤波卷积核大小
                    //defectArea缺陷面积
                    //edgeRollSlope判断卷边的斜率偏差
                    //imperfectBorderWidth判断缺边的宽度
                    //leftSide左边有效区域
                    //rightSide右边有效区域
                    //clothAberrationGrad1-clothAberrationGrad4色差等级分类
                    //布匹边缘不检测宽度clothSideUnDetectWidth
                    //*****************************
                    hv_magnification = 4.8188;
                    hv_leftSide = 41.5040;
                    hv_rightSide = 41.5040;
                    hv_boxNumber = 6;
                    hv_boxWidth = 83;
                    hv_boxHeight = 83;
                    hv_boxBenginX = 200;
                    hv_dynThresh = 15;
                    hv_medianKernal = 20;
                    hv_thresh = 30;
                    hv_defectArea = 0.2157;
                    hv_defectWidth = 1.0393;
                    hv_defectHeight = 1.0393;
                    hv_edgeRollSlope = 0.1;
                    hv_imperfectBorderWidth = 4.15;
                    hv_clothAberrationGrad1 = 0.5;
                    hv_clothAberrationGrad2 = 1.5;
                    hv_clothAberrationGrad3 = 3.0;
                    hv_clothAberrationGrad4 = 6.0;
                    hv_clothSideUnDetectWidth = 20.7;
                    hv_isSeperateComputer = 1;

                    hv_metersCounter = hv_ins.Clone();

                    hv_leftSide = hv_leftSide * hv_magnification;
                    hv_rightSide = hv_rightSide * hv_magnification;
                    hv_boxWidth = hv_boxWidth * hv_magnification;
                    hv_boxHeight = hv_boxHeight * hv_magnification;
                    hv_boxBenginX = hv_boxBenginX * hv_magnification;
                    hv_defectArea = (hv_defectArea * hv_magnification) * hv_magnification;
                    hv_defectWidth = hv_defectWidth * hv_magnification;
                    hv_defectHeight = hv_defectHeight * hv_magnification;
                    hv_imperfectBorderWidth = hv_imperfectBorderWidth * hv_magnification;
                    hv_clothSideUnDetectWidth = hv_clothSideUnDetectWidth * hv_magnification;
                    hv_result = 0;
                    hv_algorithmOfAberration = 2;






                    if ((int)(new HTuple(hv_flag.TupleEqual(0))) != 0)
                    {
                        hv_flag = 1;

                        ho_Boxs.Dispose();
                        get_standard_lab(ho_Image, out ho_Boxs, hv_isSeperateComputer, out hv_standardTupleL,
                            out hv_standardTupleA, out hv_standardTupleB, out hv_standardL, out hv_standardA,
                            out hv_standardB, out hv_result, out hv_tupleDetectResult, out hv_defectNumber,
                            out hv_tupleDefectClass, out hv_tupleDefectX, out hv_tupleDefectY,
                            out hv_tupleDefectRow1, out hv_tupleDefectRow2, out hv_tupleDefectColumn1,
                            out hv_tupleDefectColumn2, out hv_minWidth, out hv_maxWidth, out hv_meanWidth,
                            out hv_metersCounter, out hv_tupleMessages, out hv_tupleMessagesColor,
                            out hv_leftDetectSide, out hv_rightDetectSide, out hv_L, out hv_A,
                            out hv_B, out hv_ClothRegionCoordinateX1, out hv_ClothRegionCoordinateX2);
                        hv_tupleBackgroundColor = new HTuple();
                        hv_tupleBackgroundColor[0] = 0;
                        hv_tupleBackgroundColor[1] = 0;
                        hv_tupleBackgroundColor[2] = 0;
                        ho_ImageDump.Dispose();
                        disp_detect_result(ho_Image, ho_Boxs, out ho_ImageDump, hv_windowHandle,
                            hv_tupleBackgroundColor, hv_tupleDefectRow1, hv_tupleDefectRow2, hv_tupleDefectColumn1,
                            hv_tupleDefectColumn2, hv_minWidth, hv_maxWidth, hv_meanWidth, hv_metersCounter,
                            hv_tupleMessages, hv_tupleMessagesColor, hv_leftDetectSide, hv_rightDetectSide);

                    }
                    else
                    {
                        ho_ImageWithDefect.Dispose();
                        get_defect_aberration(ho_Image, out ho_ImageWithDefect, hv_windowHandle,
                            hv_standardTupleL, hv_standardTupleA, hv_standardTupleB, hv_isSeperateComputer,
                            hv_algorithmOfAberration, out hv_clothAberration, out hv_leftRightAberration2,
                            out hv_L, out hv_A, out hv_B, out hv_result, out hv_tupleDetectResult,
                            out hv_defectNumber, out hv_tupleDefectClass, out hv_tupleDefectX,
                            out hv_tupleDefectY, out hv_tupleDefectRow1, out hv_tupleDefectRow2,
                            out hv_tupleDefectColumn1, out hv_tupleDefectColumn2, out hv_minWidth,
                            out hv_maxWidth, out hv_meanWidth, out hv_metersCounter, out hv_tupleMessages,
                            out hv_tupleMessagesColor, out hv_leftDetectSide, out hv_rightDetectSide,
                            out hv_clothRegionCoordinateX1, out hv_clothRegionCoordinateX2);

                        hv_tupleBackgroundColor = new HTuple();
                        hv_tupleBackgroundColor[0] = 80;
                        hv_tupleBackgroundColor[1] = 80;
                        hv_tupleBackgroundColor[2] = 80;
                        ho_ImageDump.Dispose();
                        disp_detect_result(ho_Image, ho_Boxs, out ho_ImageDump, hv_windowHandle,
                            hv_tupleBackgroundColor, hv_tupleDefectRow1, hv_tupleDefectRow2, hv_tupleDefectColumn1,
                            hv_tupleDefectColumn2, hv_minWidth, hv_maxWidth, hv_meanWidth, hv_metersCounter,
                            hv_tupleMessages, hv_tupleMessagesColor, hv_leftDetectSide, hv_rightDetectSide);


                    }
                    HOperatorSet.WaitSeconds(2);


                }



            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_DefectRegion.Dispose();
                ho_Image.Dispose();
                ho_Boxs.Dispose();
                ho_ImageDump.Dispose();
                ho_ImageWithDefect.Dispose();

                throw HDevExpDefaultException;
            }
            ho_DefectRegion.Dispose();
            ho_Image.Dispose();
            ho_Boxs.Dispose();
            ho_ImageDump.Dispose();
            ho_ImageWithDefect.Dispose();

        }

        public void InitHalcon()
        {
            // Default settings used in HDevelop 
            HOperatorSet.SetSystem("width", 512);
            HOperatorSet.SetSystem("height", 512);
        }

        public void RunHalcon(HTuple Window)
        {
            hv_ExpDefaultWinHandle = Window;
            action();
        }







    }
}

