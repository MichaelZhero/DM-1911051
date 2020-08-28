using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Web;
using MySql.Data.MySqlClient;
using System.IO;

namespace DALSA.SaperaLT.Demos.NET.CSharp.MultiBoardSyncGrabDemo
{
    public partial class SQLForm : Form
    {

        string sqlcmd;
        MySqlCommand mysqlcmd;//数据库执行命令
        MySqlDataReader mysqldr;//数据库查询结果
        private MySqlDataAdapter adastAdapter;
        float qualifiedMeters = 0;
        float actualMeters = 0;
        float aberrationGrad5Meters = 0;
        float aberrationGrad4Meters = 0;
        float aberrationGrad3Meters = 0;
        float aberrationGrad2Meters = 0;
        float aberrationGrad1Meters = 0;
        string ImagePath;
        MessageBoxForm messageboxForm;
        int boxNumber = 6;//框的数量
        float intervalMeter = 0.5F;//间隔米数
        List<string> ImageFiles = new List<string>();
        List<double> MaxDiffList = new List<double>(); //定义色差偏差最大值  
        bool flag = false;//正在输入日历


        float xPain;//画图色框x坐标
        float yPain; //画图色框y坐标
        float widthPain; //画图色框宽度
        float heightPain;//画图色框高度
        Brush bush;//定义画图刷颜色

        double chromaticAberration4 = 0.5;//色差等级4、5分界值
        double chromaticAberration3 = 3;//色差等级3、4分界值
        double chromaticAberration2 = 6;//色差等级2、3分界值
        double chromaticAberration1 = 12;//色差等级1、2分界值

          

        public SQLForm()
        {
            InitializeComponent();

            button2.Visible = true;
            button3.Visible = true;
            button4.Visible = true;
            button5.Visible = true;
            button6.Visible = true;
            label24.Visible = true;
            label26.Visible = true;
            label28.Visible = true;
            label35.Visible = true;
            label38.Visible = true;
            label40.Visible = true;

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        public static MySqlCommand getSqlCommand(String sql, MySqlConnection mysql)
        {
            MySqlCommand mySqlCommand = new MySqlCommand(sql, mysql);
            return mySqlCommand;
        }


        private void SQLForm_Load(object sender, EventArgs e)
        {
            /*************************************
           读取色差等级
       *************************************/

            PublicClass.conn = new MySqlConnection(PublicClass.mysqlcon);
            PublicClass.conn.Open();
            PublicClass.message = "数据库连接成功！";

            
            sqlcmd = "select * from clothaberrationgrad";
          //  sqlcmd = "select * from clothdetectresult where ClothNumber ='" + tempClothNumber + "'";
            mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
            //mysqlcmd.CommandType = CommandType.Text;

            //adastAdapter = new MySqlDataAdapter(mysqlcmd);
            //DataTable pdt = new DataTable();
            //adastAdapter.Fill(pdt);
            //this.dataGridView1.DataSource = pdt;



            mysqldr = mysqlcmd.ExecuteReader();
            if (mysqldr.HasRows)
            {
                while (mysqldr.Read())
                {
                    chromaticAberration1 = Convert.ToDouble(mysqldr["clothaberrationgrad1"].ToString());
                    chromaticAberration2 = Convert.ToDouble(mysqldr["clothaberrationgrad2"].ToString());
                    chromaticAberration3 = Convert.ToDouble(mysqldr["clothaberrationgrad3"].ToString());
                    chromaticAberration4 = Convert.ToDouble(mysqldr["clothaberrationgrad4"].ToString());

                    break;
                }
            }
            mysqldr.Close();

            button6.Text = "0.0-" + chromaticAberration4.ToString();
            button5.Text = chromaticAberration4.ToString() + "-" + chromaticAberration3.ToString();
            button4.Text = chromaticAberration3.ToString() + "-" + chromaticAberration2.ToString();
            button3.Text = chromaticAberration2.ToString() + "-" + chromaticAberration1.ToString();
            button2.Text = ">" + chromaticAberration1.ToString();

            string tempClothNumber = PublicClass.clothNumber;//"201911032321";d
            sqlcmd = "select * from clothdetectreport where ClothNumber ='" + tempClothNumber + "'";
            mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
            mysqlcmd.CommandType = CommandType.Text;

            adastAdapter = new MySqlDataAdapter(mysqlcmd);
            DataTable pdt = new DataTable();

            adastAdapter.Fill(pdt);
            this.dataGridView1.DataSource = pdt;

            DataTable savepdt = new DataTable();
            savepdt = GetDgvToTable(dataGridView1);
            
            

            ExportToExcel(savepdt, PublicClass.reportpath+ "色差检测详细表.xls");
           // ExportToExcel(pdt, PublicClass.reportpath + "text1.xls");

           





            tabControl1.Enabled = true;
             tempClothNumber = PublicClass.clothNumber;//查询布匹编号string
            float standardColor = 0;//定义标准色差

            /*************************************
               查询销售员工名字
           *************************************/
            string theSourceName = "";
            sqlcmd = "select * from clothemployeedetails where employeeID in (select SourceID from clothdetectdetail where ClothNumber =" + tempClothNumber + ")";
            mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
            mysqldr = mysqlcmd.ExecuteReader();
            if (mysqldr.HasRows)
            {
                while (mysqldr.Read())
                {
                    theSourceName = mysqldr["employeeName"].ToString();
                    break;
                }
            }
            mysqldr.Close();


            /*************************************
                查询操作员工名字
            *************************************/
            string theOperatorName = "";
            sqlcmd = "select * from clothemployeedetails where employeeID in (select OperatorID from clothdetectdetail where ClothNumber =" + tempClothNumber + ")";
            mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
            mysqldr = mysqlcmd.ExecuteReader();
            if (mysqldr.HasRows)
            {
                while (mysqldr.Read())
                {
                    theOperatorName = mysqldr["employeeName"].ToString();
                    break;
                }
            }
            mysqldr.Close();

            /*************************************
                查询布匹检测详情，并加载到tagPage2里面
            *************************************/
            tabPage2.Invalidate();
            sqlcmd = "select * from clothdetectdetail where ClothNumber =" + tempClothNumber;
            mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
            mysqldr = mysqlcmd.ExecuteReader();
            if (mysqldr.HasRows)
            {
                while (mysqldr.Read())
                {
                    try
                    {
                        standardColor = Convert.ToSingle(mysqldr["StandardColorLAB"].ToString());
                    }
                    catch
                    {

                    }
                    labelClothNumber.Text = mysqldr["ClothNumber"].ToString();
                    labelBatchNumber.Text = mysqldr["BatchNumber"].ToString();
                    labelClassNumber.Text = mysqldr["ClothClassNumber"].ToString();
                    labelEquipmentNumber.Text = mysqldr["EquipmentNumber"].ToString();
                    labelVolumn.Text = mysqldr["TotalVolumn"].ToString() + "-" + mysqldr["VolumnNumber"].ToString();
                    labelCylinderNumber.Text = mysqldr["CylinderNumber"].ToString();
                    labelSource.Text = theSourceName;
                    labelOperator.Text = theOperatorName;
                    labelMeters.Text = mysqldr["LabelMeters"].ToString();

                    labelActualMeters.Text = mysqldr["ActualMeters"].ToString();
                    labelActualM1.Text = mysqldr["ActualMeters"].ToString();
                    labelActualM.Text = mysqldr["ActualMeters"].ToString();

                    labelMaxWidth.Text = mysqldr["MaxWidth"].ToString();
                    labelMinWidth.Text = mysqldr["MinWidth"].ToString();
                    labelMeanWidth.Text = mysqldr["MeanWidth"].ToString();
                    labelDate.Text = mysqldr["Date"].ToString().Split(' ')[0];
                    labelStartTime.Text = mysqldr["StartTime"].ToString();
                    labelEndTime.Text = mysqldr["EndTime"].ToString();

                    labelStandardL.Text = mysqldr["StandardColorL"].ToString();
                    labelStandardA.Text = mysqldr["StandardColorA"].ToString();
                    labelStandardB.Text = mysqldr["StandardColorB"].ToString();
                    labelStandardLAB.Text = mysqldr["StandardColorLAB"].ToString();
                    labelMaxDiffL.Text = mysqldr["MaxLDiff"].ToString();
                    labelMaxDiffA.Text = mysqldr["MaxADiff"].ToString();
                    labelMaxDiffB.Text = mysqldr["MaxBDiff"].ToString();
                    labelMaxDiffLAB.Text = mysqldr["MaxLABDiff"].ToString();

                    labelStandardL1.Text = mysqldr["StandardColorL"].ToString();
                    labelStandardA1.Text = mysqldr["StandardColorA"].ToString();
                    labelStandardB1.Text = mysqldr["StandardColorB"].ToString();
                    labelStandardLAB1.Text = mysqldr["StandardColorLAB"].ToString();
                    labelMaxDiffL1.Text = mysqldr["MaxLDiff"].ToString();
                    labelMaxDiffA1.Text = mysqldr["MaxADiff"].ToString();
                    labelMaxDiffB1.Text = mysqldr["MaxBDiff"].ToString();
                    labelMaxDiffLAB1.Text = mysqldr["MaxLABDiff"].ToString();

                    labelDefectNumber.Text = mysqldr["DefectNumber"].ToString();
                    richTextBox1.Clear();
                    richTextBox1.AppendText(mysqldr["AberrationDetails"].ToString() + "\r\n");
                    richTextBox1.AppendText(mysqldr["DefectDetails"].ToString() + "\r\n");
                    label11.Text ="色差详情："+ mysqldr["AberrationDetails"].ToString() + "\r\n";
                    label1.Text = "缺陷详情："+ mysqldr["DefectDetails"].ToString() + "\r\n";

                    try
                    {
                        actualMeters = Convert.ToSingle(mysqldr["ActualMeters"].ToString());
                        qualifiedMeters = Convert.ToSingle(mysqldr["QualifiedMeters"].ToString());
                        aberrationGrad1Meters = Convert.ToSingle(mysqldr["AberrationGrad1Meters"].ToString());
                        aberrationGrad2Meters = Convert.ToSingle(mysqldr["AberrationGrad2Meters"].ToString());
                        aberrationGrad3Meters = Convert.ToSingle(mysqldr["AberrationGrad3Meters"].ToString());
                        aberrationGrad4Meters = Convert.ToSingle(mysqldr["AberrationGrad4Meters"].ToString());
                        aberrationGrad5Meters = Convert.ToSingle(mysqldr["AberrationGrad5Meters"].ToString());

                    }
                    catch
                    {

                    }

                    ImagePath = mysqldr["ImagePath"].ToString();
                    break;
                }
                mysqldr.Close();
            }
            else
            {
                PublicClass.message = "没有检测结果";
                messageboxForm = new MessageBoxForm(1);
                messageboxForm.Owner = this;
                messageboxForm.ShowDialog();
                mysqldr.Close();
                return;
            }

           

            /*************************************
               遍历布匹检测详情数据库
           *************************************/

            sqlcmd = "select * from clothdetectresult where ClothNumber ='" + tempClothNumber + "'";
            mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
            mysqldr = mysqlcmd.ExecuteReader();

            int j = 0;
            while (mysqldr.Read())
            {
               // intervalMeter = Convert.ToSingle(mysqldr["IntervalMeter"].ToString());//获取间隔米数
                label40.Text = "每格米数:" + mysqldr["IntervalMeter"].ToString();
               // boxNumber = mysqldr.GetInt32("BoxNumber");

                ImageFiles.Add(mysqldr["ImagePath"].ToString());
                //double []boxColors = new double[10];
                 double MaxDiff = 0;

                 for (int i = 1; i <= 1; i++)
                {
                    double tempColor = mysqldr["Box" + i.ToString() + "Color"] == DBNull.Value ? 0 : Convert.ToSingle(mysqldr["Box" + i.ToString() + "Color"]);

                    double tempDiff = tempColor;
                        //Math.Abs(tempColor - standardColor);
                   if (tempDiff > MaxDiff)
                       MaxDiff = tempDiff;
                }




                MaxDiffList.Add(MaxDiff);
                j++;
            }
            mysqldr.Close();
            tabPage1.Invalidate();

            /***************************************
               查询布匹检测详情，将结果放到图片浏览界面
             *************************************/
            //int ImageNumber =Convert.ToInt32( Math.Ceiling(actualMeters / intervalMeter));

            int ImageNumber = ImageFiles.Count;


            trackBar1.Value = 0;
            if (ImageNumber > 0 && ImageFiles[0] != null)
            {
                trackBar1.Minimum = 0;
                trackBar1.Maximum = ImageNumber - 1;
                try
                {
                    //  pictureBox1.Image = Image.FromFile(ImageFiles[0]);
                    string tempstr = System.IO.Path.GetDirectoryName(ImagePath);
                    string imageFile = ImageFiles[0];
                    //   int Index1 = imageFile.IndexOf(".jpg");
                    //  int Index2 = imageFile.IndexOf("-");
                    //   string currentMeter = imageFile.Substring(Index2, Index1 - Index2);
                    if (checkBox1.Checked == false)
                        pictureBox1.Image = Image.FromFile(tempstr + "\\检测图\\" + (trackBar1.Value + 1).ToString() + ".jpg");
                    else
                        pictureBox1.Image = Image.FromFile(tempstr + "\\原图\\" + (trackBar1.Value + 1).ToString() + ".jpg");

                    //   labelCurrentM.Text = labelActualMeters.Text + currentMeter;
                }
                catch (Exception ed)
                {

                }
            }
            else
            {
                trackBar1.Enabled = false;
            }

            tabPage5.Invalidate();
            /*************************************
              查询布匹检测详情，将结果放到图表里面
            *************************************/
            float qualifiedPercents = (qualifiedMeters / actualMeters);
            float unQualifiedPercents = (actualMeters - qualifiedMeters) / actualMeters;

            List<string> xData = new List<string>() { "合格率:" + (qualifiedPercents * 100).ToString("0.00") + "%", "折损率:" + (unQualifiedPercents * 100).ToString("0.00") + "%" };
            List<float> yData = new List<float>() { float.Parse(qualifiedPercents.ToString("0.00")), float.Parse(unQualifiedPercents.ToString("0.00")) };
            //chart1.seri
            // chart1.Series[0].Points[0].Color  = Color.Green;

            chart1.Series[0]["PieLabelStyle"] = "Outside";
            chart1.Series[0]["PieLabelStyle"] = "Outside";//将文字移到外侧
            chart1.Series[0]["PieLineColor"] = "Black";//绘制黑色的连线。
            chart1.Series[0].Points.DataBindXY(xData, yData);
            labelActualM.Text = actualMeters.ToString("0.00") + "米";
            labelQulifiedM.Text = qualifiedMeters.ToString("0.00") + "米";
            labelLoseM.Text = (actualMeters - qualifiedMeters).ToString("0.00") + "米";

            chart2.Series[0].Points.AddXY("色差等级1", aberrationGrad1Meters);
            chart2.Series[0].Points.AddXY("色差等级2", aberrationGrad2Meters);
            chart2.Series[0].Points.AddXY("色差等级3", aberrationGrad3Meters);
            chart2.Series[0].Points.AddXY("色差等级4", aberrationGrad4Meters);
            chart2.Series[0].Points.AddXY("色差等级5", aberrationGrad5Meters);
            chart2.Series[0].Points[0].Color = Color.Green;
            chart2.Series[0].Points[1].Color = Color.LimeGreen;
            chart2.Series[0].Points[2].Color = Color.YellowGreen;
            chart2.Series[0].Points[3].Color = Color.Orange;
            chart2.Series[0].Points[4].Color = Color.Red;

            chart2.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            chart2.ChartAreas[0].AxisY.Title = "单位：米数";
            chart2.Series[0].IsValueShownAsLabel = true;
            chart2.Series[0].Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            tabPage4.Invalidate();
        }

        private void tabPage1_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics gra = e.Graphics;
            int boxNumbers = 30;//每行画30个框
            int ZhiShiQuYu = tabPage1.Width / 10;//边上标注的区域为宽度的1/10,剩下画图区域为(tabPage1.Width / 10) * 9)
            float IntervalNumbers = (float)(boxNumbers * 3 + boxNumbers + 2);
            float IntervalDistance = ((tabPage1.Width / 10) * 9) / IntervalNumbers;//框间隔是框宽度的1/3
            float SideDistance = (((tabPage1.Width / 10) * 9) - IntervalDistance * (boxNumbers * 3 + boxNumbers - 1)) / 2;//框离边缘宽度
            widthPain = IntervalDistance * 3;//框的宽度
            heightPain = IntervalDistance * 6;//框的高度
            int boxRowsNumber = MaxDiffList.Count / boxNumbers + 1;//框的行数
            int tabPage1Height = Convert.ToInt32(SideDistance + IntervalDistance * 7 * boxRowsNumber);

            this.tabPage1.AutoScrollMinSize = new Size(this.tabPage1.Width, tabPage1Height);//增加滚动条     
            int j = 0;

            gra.TranslateTransform(this.tabPage1.AutoScrollPosition.X, this.tabPage1.AutoScrollPosition.Y);//自动跟随滚动条
            foreach (var MaxDiff in MaxDiffList)
            {
                if (MaxDiff < chromaticAberration4)
                {
                    bush = new SolidBrush(Color.Green);
                }
                else
                {
                    if (MaxDiff < chromaticAberration3)
                    {
                        bush = new SolidBrush(Color.GreenYellow);
                    }
                    else
                    {
                        if (MaxDiff < chromaticAberration2)
                        {
                            bush = new SolidBrush(Color.Orange);
                        }
                        else
                        {
                            if (MaxDiff < chromaticAberration1)
                            {
                                bush = new SolidBrush(Color.Red);
                            }
                            else
                            {
                                bush = new SolidBrush(Color.DarkRed);
                            }
                        }
                    }
                }

                int kkx = j % boxNumbers;
                int kky = j / boxNumbers;
                xPain = SideDistance + IntervalDistance * 4 * kkx;
                yPain = SideDistance + IntervalDistance * 7 * kky;
                gra.FillRectangle(bush, xPain, yPain, widthPain, heightPain);//画方框
                j++;
            }

            //int BiaoZhuSide = tabPage1.Width / 60;
            //int BiaoZhuWidth = tabPage1.Width / 60 * 4;
            //int BiaoZhuheight = tabPage1.Width / 60 * 2;
            //bush = new SolidBrush(Color.Green);
            //gra.FillRectangle(bush, tabPage1.Width- tabPage1.Width / 60*5, BiaoZhuSide, BiaoZhuWidth, BiaoZhuheight);//画方框
            //bush = new SolidBrush(Color.LimeGreen);
            //gra.FillRectangle(bush, tabPage1.Width - tabPage1.Width / 60 * 5, BiaoZhuSide+ BiaoZhuSide*5, BiaoZhuWidth, BiaoZhuheight);//画方框
            //bush = new SolidBrush(Color.YellowGreen);
            //gra.FillRectangle(bush, tabPage1.Width - tabPage1.Width / 60 * 5, BiaoZhuSide + BiaoZhuSide * 5*2, BiaoZhuWidth, BiaoZhuheight);//画方框
            //bush = new SolidBrush(Color.Orange);
            //gra.FillRectangle(bush, tabPage1.Width - tabPage1.Width / 60 * 5, BiaoZhuSide + BiaoZhuSide * 5 * 3, BiaoZhuWidth, BiaoZhuheight);//画方框
            //bush = new SolidBrush(Color.Red);
            //gra.FillRectangle(bush, tabPage1.Width - tabPage1.Width / 60 * 5, BiaoZhuSide + BiaoZhuSide * 5 * 4, BiaoZhuWidth, BiaoZhuheight);//画方框

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            try
            {

                string imageFile = ImageFiles[trackBar1.Value];
                string tempstr = System.IO.Path.GetDirectoryName(ImagePath);
                int Index1 = imageFile.IndexOf(".jpg");
                int Index2 = imageFile.IndexOf("-");
                // string currentMeter = imageFile.Substring(Index2, Index1 - Index2);
                //pictureBox1.Image = Image.FromFile(ImageFiles[trackBar1.Value]);
                try
                {

                    if (checkBox1.Checked == false)
                        pictureBox1.Image = Image.FromFile(tempstr + "\\检测图\\" + (trackBar1.Value + 1).ToString() + ".jpg");
                    else
                        pictureBox1.Image = Image.FromFile(tempstr + "\\原图\\" + (trackBar1.Value + 1).ToString() + ".jpg");

                }
                catch (Exception)
                {

                    throw;
                }

                labelCurrentM.Text = labelActualMeters.Text + "-" + trackBar1.Value.ToString();
            }
            catch
            {

                labelCurrentM.Text = labelActualMeters.Text + "-" + trackBar1.Value.ToString();
                pictureBox1.Image = null;
                //PublicClass.message = "无图！";
                //messageboxForm = new MessageBoxForm(1);
                //messageboxForm.Owner = this;
                //messageboxForm.ShowDialog();
            }


        }

        private void SQLForm_Shown(object sender, EventArgs e)
        {
           
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string tempstr = System.IO.Path.GetDirectoryName(ImagePath);
                if (checkBox1.Checked == false)
                    pictureBox1.Image = Image.FromFile(tempstr + "\\检测图\\" + (trackBar1.Value + 1).ToString() + ".jpg");
                else
                    pictureBox1.Image = Image.FromFile(tempstr + "\\原图\\" + (trackBar1.Value + 1).ToString() + ".jpg");

            }
            catch (Exception)
            {

               
            }
        }



        /// <summary>
        /// 导出文件，使用文件流。该方法使用的数据源为DataTable,导出的Excel文件没有具体的样式。
        /// </summary>
        /// <param name="dt"></param>
        public static string ExportToExcel(System.Data.DataTable dt, string path)
        {
            KillSpecialExcel();
            string result = string.Empty;
            try
            {
                // 实例化流对象，以特定的编码向流中写入字符。
                StreamWriter sw = new StreamWriter(path, false, Encoding.GetEncoding("gb2312"));

                StringBuilder sb = new StringBuilder();
                for (int k = 0; k < dt.Columns.Count; k++)
                {
                    // 添加列名称
                    sb.Append(dt.Columns[k].ColumnName.ToString() + "\t");
                }
                sb.Append(Environment.NewLine);
                // 添加行数据
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        // 根据列数追加行数据
                        sb.Append(row[j].ToString() + "\t");
                    }
                    sb.Append(Environment.NewLine);
                }
                sw.Write(sb.ToString());
                sw.Flush();
                sw.Close();
                sw.Dispose();

                // 导出成功后打开
                //System.Diagnostics.Process.Start(path);
            }
            catch (Exception)
            {
                result = "请保存或关闭可能已打开的Excel文件";
            }
            finally
            {
                dt.Dispose();
            }
            return result;
        }
        /// <summary>
        /// 结束进程
        /// </summary>
        private static void KillSpecialExcel()
        {
            foreach (System.Diagnostics.Process theProc in System.Diagnostics.Process.GetProcessesByName("EXCEL"))
            {
                if (!theProc.HasExited)
                {
                    bool b = theProc.CloseMainWindow();
                    if (b == false)
                    {
                        theProc.Kill();
                    }
                    theProc.Close();
                }
            }
        }


        public DataTable GetDgvToTable(DataGridView dgv)
        {
            DataTable dt = new DataTable();
            for (int count = 0; count < dgv.Columns.Count; count++)
            {
                DataColumn dc = new DataColumn(dgv.Columns[count].Name.ToString());
                dt.Columns.Add(dc);
            }
            for (int count = 0; count < dgv.Rows.Count; count++)
            {
                DataRow dr = dt.NewRow();
                for (int countsub = 0; countsub < dgv.Columns.Count; countsub++)
                {
                    dr[countsub] = Convert.ToString(dgv.Rows[count].Cells[countsub].Value);
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }



        private void button7_Click(object sender, EventArgs e)
        {
          
            Bitmap tmBitmap1 = new Bitmap(tabPage1.Width, tabPage1.Height);
            this.tabPage1.DrawToBitmap(tmBitmap1, new Rectangle(0, 0, tabPage1.Width, tabPage1.Height));
            string tempath = PublicClass.reportpath + "每米色差等级.bmp";
            tmBitmap1.Save(tempath);
            Bitmap tmBitmap2 = new Bitmap(tabPage2.Width, tabPage2.Height);
            this.tabPage2.DrawToBitmap(tmBitmap2, new Rectangle(0, 0, tabPage2.Width, tabPage2.Height));
            tempath = PublicClass.reportpath + "检测详情.bmp";
            tmBitmap2.Save(@tempath);
            Bitmap tmBitmap3 = new Bitmap(tabPage3.Width, tabPage3.Height);
            this.tabPage3.DrawToBitmap(tmBitmap3, new Rectangle(0, 0, tabPage3.Width, tabPage3.Height));
            tempath = PublicClass.reportpath + "合格比例图.bmp";
            tmBitmap3.Save(tempath);
            Bitmap tmBitmap4 = new Bitmap(tabPage4.Width, tabPage4.Height);
            this.tabPage4.DrawToBitmap(tmBitmap4, new Rectangle(0, 0, tabPage4.Width, tabPage4.Height));
            tempath = PublicClass.reportpath + "色差等级柱状图.bmp";
            tmBitmap4.Save(tempath);
            Bitmap tmBitmap5 = new Bitmap(tabPage5.Width, tabPage5.Height);
            this.tabPage5.DrawToBitmap(tmBitmap5, new Rectangle(0, 0, tabPage5.Width, tabPage5.Height));
            tempath = PublicClass.reportpath + "图片浏览截图.bmp";
            tmBitmap5.Save(tempath);
            Bitmap tmBitmap6 = new Bitmap(tabPage6.Width, tabPage6.Height);
            this.tabPage6.DrawToBitmap(tmBitmap6, new Rectangle(0, 0, tabPage6.Width, tabPage6.Height));
            tempath = PublicClass.reportpath + "测查检测表截图.bmp";
            tmBitmap6.Save(tempath);

        }

        private void printPreviewDialog1_Load(object sender, EventArgs e)
        {

        }
        private int currentPageIndex = 0;
        private int rowCount = 0;
        private int pageCount = 0;
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {

            Bitmap tmBitmap1 = new Bitmap(tabPage1.Width, tabPage1.Height);
            this.tabPage1.DrawToBitmap(tmBitmap1, new Rectangle(0, 0, tabPage1.Width, tabPage1.Height));
            string tempath = PublicClass.reportpath + "test11.bmp";
            tmBitmap1.Save(tempath);
            Bitmap tmBitmap2 = new Bitmap(tabPage2.Width, tabPage2.Height);
            this.tabPage2.DrawToBitmap(tmBitmap2, new Rectangle(0, 0, tabPage2.Width, tabPage2.Height));
            tempath = PublicClass.reportpath + "test22.bmp";
            tmBitmap2.Save(@tempath);
            Bitmap tmBitmap3 = new Bitmap(tabPage3.Width, tabPage3.Height);
            this.tabPage3.DrawToBitmap(tmBitmap3, new Rectangle(0, 0, tabPage3.Width, tabPage3.Height));
            tempath = PublicClass.reportpath + "test33.bmp";
            tmBitmap3.Save(tempath);
            Bitmap tmBitmap4 = new Bitmap(tabPage4.Width, tabPage4.Height);
            this.tabPage4.DrawToBitmap(tmBitmap4, new Rectangle(0, 0, tabPage4.Width, tabPage4.Height));
            tempath = PublicClass.reportpath + "test44.bmp";
            tmBitmap4.Save(tempath);
            Bitmap tmBitmap5 = new Bitmap(tabPage5.Width, tabPage5.Height);
            this.tabPage2.DrawToBitmap(tmBitmap5, new Rectangle(0, 0, tabPage5.Width, tabPage5.Height));
            tempath = PublicClass.reportpath + "test55.bmp";
            tmBitmap5.Save(tempath);

            Font fntTxt = new Font("宋体", 11, FontStyle.Regular);    //正文文字
            Brush brush = new SolidBrush(Color.Black);//画刷 
            Pen pen = new Pen(Color.Black);           //线条颜色


            pageCount = 5;     //定义页数


            if (currentPageIndex == 0)   //当为第一页时
            {

                e.Graphics.DrawString("111111111111111111111111111", fntTxt, brush, new Point(0, 0));
                e.Graphics.DrawImage(tmBitmap1, 30, 30, 200, 200);
            }
            else if (currentPageIndex == 1)   //当为第二页时
            {
                // e.Graphics.DrawString("222222222222222222222222222", fntTxt, brush, new Point(0, 0));
                e.Graphics.DrawImage(tmBitmap2, 30, 30, 200, 200);
            }
            else if (currentPageIndex == 2)   //当为第三页时
            {
                // e.Graphics.DrawString("333333333333333333333333333", fntTxt, brush, new Point(0, 0));
                e.Graphics.DrawImage(tmBitmap3, 30, 30, 200, 200);
            }
            else if (currentPageIndex == 3)   //当为第三页时
            {
                //  e.Graphics.DrawString("444444444444444444", fntTxt, brush, new Point(0, 0));
                e.Graphics.DrawImage(tmBitmap4, 30, 30, 400, 200);
            }
            else if (currentPageIndex == 4)   //当为第三页时
            {
                //  e.Graphics.DrawString("55555555555555555555", fntTxt, brush, new Point(0, 0));
                e.Graphics.DrawImage(tmBitmap5, 30, 30, 400, 200);
            }


            currentPageIndex++;      //加新页
            if (currentPageIndex < pageCount)
            {
                e.HasMorePages = true;  //如果小于定义页 那么增加新的页数

            }
            else
            {
                e.HasMorePages = false; //停止增加新的页数
                currentPageIndex = 0;
            }



            e.Graphics.DrawImage(tmBitmap1, 0, 0, 200, 200);

            e.Graphics.DrawImage(tmBitmap3, 0, 400, 200, 200);
            e.Graphics.DrawImage(tmBitmap4, 0, 600, 200, 200);
            e.Graphics.DrawImage(tmBitmap5, 0, 800, 200, 200);


            e.Graphics.DrawImage(tmBitmap2, tmBitmap1.Width, tmBitmap1.Height, tmBitmap2.Width, tmBitmap2.Height);
            e.Graphics.DrawImage(tmBitmap3, tmBitmap2.Width, tmBitmap2.Height, tmBitmap3.Width, tmBitmap3.Height);
            e.Graphics.DrawImage(tmBitmap4, tmBitmap3.Width, tmBitmap3.Height, tmBitmap4.Width, tmBitmap4.Height);
            e.Graphics.DrawImage(tmBitmap5, tmBitmap4.Width, tmBitmap4.Height, tmBitmap5.Width, tmBitmap5.Height);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            printDialog1.ShowDialog();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void tabPage6_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
