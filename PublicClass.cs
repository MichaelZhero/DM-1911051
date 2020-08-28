using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using MySql.Data.MySqlClient;

namespace DALSA.SaperaLT.Demos.NET.CSharp.MultiBoardSyncGrabDemo
{
    class PublicClass
    {
        Dictionary<string,HTuple> DispResultDict=new Dictionary<string, HTuple>();
        public static string message;//消息显示窗口
        public static MySqlConnection conn;//定义连接数据库
        public static String mysqlcon = "server=localhost;database=mysql;uid=root;pwd=";// "database=mysql;Password=;User ID=root;server=localhost";
        public static String SavedImgPath = "";
        public static String SqlImgPath = "";
        public static String originpath = "";
        public static String detectedpath = "";
        public static String reportpath = "";
        public static String sqloriginpath = "";
        public static String sqldetectedpath = "";
        //检测框及区域参数
        public static  int OriginY=2404;
        public static int OriginX=200;
        public static int Period=83;
        public static int Square=83;
       
        public static int Cap_NUM=6;

        public static int Width ;
        public static int Height ;

        public static Double mag = 4.82;
        public static Double leftside = 41.50;
        public static Double rightside = 41.50;
        public static Double EdgeRollslope = 0.1;
        public static Double imperfectBorderWidth = 4.15;
        public static Double clothSideUnDetectWidth = 20.7;

        public static int mediankernal=20;
        public static int dynthresh = 20;
        public static int defectarea = 5;
        public static int Thresh = 30;

        public static Double defectArea = 0.216;
        public static Double defectWidth = 1.039;
        public static Double defectHeight = 1.039;

        public static int hv_defectNumber = 0;
        public static int sideWidth = 0;
        public static int ra = 0;
        public static bool parChanged = false;



        public static int detectDefectsFlag=1;
        public static int edgeRollFlag = 1;
        public static int imperfectBorderFlag = 1;
        public static int otherFlag = 1;

        public static int isSeperateComputer = 1;
        public static int algorithmOfAberration = 2;
        public static int maxExposureTime = 10000;//最小曝光时间
        public static int minExposureTime =4000;//最大曝光时间
        public static System.IO.Ports.SerialPort serialPort1 = new System.IO.Ports.SerialPort();
        public static System.IO.Ports.SerialPort serialPort2 = new System.IO.Ports.SerialPort();
        public static System.IO.Ports.SerialPort serialPort3 = new System.IO.Ports.SerialPort();
        public static string serialPort1Name = "COM3";//控制电机串口号
        public static string serialPort2Name = "Xtium-CL_MX4_1_Serial_0";//相机1串口号
        public static string serialPort3Name = "Xtium-CL_MX4_1_Serial_1";//相机2串口号
        //public static string serialPort2Name = "COM3";//相机1串口号
        //public static string serialPort3Name = "COM4";//相机2串口号
        public static string theDeviceNumber = "e0001";
        public static bool genStandardColorFlag = false;//更换布匹标志位,用于获取标准色差
        public static bool clothChangedFlag = false;//更换布匹标志位，用于生成布匹详情
        public static bool plcHandShaking;//与PLC握手
        public static int CameraCrtl0 = -1;
        public static int CameraCrtl1 = -1;
        public static int speed = 0;



        public static string clothNumber = null;//检测编号
        public static string batchNumber = null;//不批批号
        public static string cylinderNumber = null;//缸号
        public static string clothClassNumber = null;//布匹类号
        public static string markedMeters = null;//米数
        public static string volumnNumber = null;//卷数
        public static string totalVolumn = null;//总卷数
        public static string sourceName = null;//货号来源名字
        public static string sourceID = null; //货号来源ID
        public static string detecterID = null;//检测人员ID
        public static string detecterName = null;//检测人员姓名


        //*********************************
        //上一次的的记录
        //**********************************
        
        public static string tempClothNumber = null;
        public static string tempBatchNumber = null;
        public static string tempCylinderNumber = null;
        public static string tempClothClassNumber = null;
        public static string tempMarkedMeters = null;
        public static string tempVolumnNumber = null;
        public static string tempTotalVolumn = null;
        public static string tempSourceName = null;
        public static string tempDetecterId = null;
        public static string tempDetecterName = null;
        public static int    tempMeters=0;


    }
}
