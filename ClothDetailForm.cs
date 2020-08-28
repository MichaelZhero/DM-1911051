using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;


namespace DALSA.SaperaLT.Demos.NET.CSharp.MultiBoardSyncGrabDemo
{
    public partial class ClothDetailForm : Form
    {

        string sqlcmd;
        MySqlCommand mysqlcmd;//数据库执行命令
        MySqlDataReader mysqldr;//数据库查询结果
        IniFiles cinifile = new IniFiles(Application.StartupPath + @"\MyConfig.INI");
        public ClothDetailForm()
        {
            InitializeComponent();
            textBoxBachNumer.Text= DateTime.Now.Date.ToString("yyyyMMdd");
            textBoxClothNumber.Text= DateTime.Now.Date.ToString("yyyyMMdd") ;
            textBoxnum.Text= DateTime.Now.ToString("mmss");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBoxClothNumber.Text == "")
            {
                MessageBox.Show("检测编号不能为空！", "提示");
                return;
            }
            if (PublicClass.conn.State == ConnectionState.Open)
            {
                //sqlcmd = "select "+textBoxClothNumber.Text+ " from clothdetectdetail";
                sqlcmd = " SELECT * FROM clothdetectdetail WHERE ClothNumber = " + textBoxClothNumber.Text+ textBoxnum.Text;
                mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
                mysqldr = mysqlcmd.ExecuteReader();
                if (mysqldr.HasRows)
                {
                    MessageBox.Show("检测编号已经存在！", "提示");
                    mysqldr.Close();
                    return;

                }
                mysqldr.Close();

              //  return;
            }

            DialogResult result = MessageBox.Show("请确认输入信息是否正确，单击确认新建布匹信？！", "是否新建布匹", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {               
                PublicClass.clothNumber = textBoxClothNumber.Text+textBoxnum.Text;
                PublicClass.batchNumber = textBoxBachNumer.Text;
                PublicClass.clothClassNumber = comboBoxClassNumber.Text;
                PublicClass.cylinderNumber = comboBoxGangHao.Text;
                PublicClass.markedMeters = textBoxMarkedMeters.Text;
                PublicClass.volumnNumber = textBoxVolumNumber.Text;
                PublicClass.totalVolumn = textBoxTotalVolumn.Text;

                cinifile.IniWriteValue("布匹信息", "布匹编号", PublicClass.clothNumber);
                cinifile.IniWriteValue("布匹信息", "布匹批号", textBoxBachNumer.Text.Trim());
               // cinifile.IniWriteValue("布匹信息", "布匹颜色", ClothColor.Text.Trim());
                cinifile.IniWriteValue("布匹信息", "布匹类号", comboBoxClassNumber.Text.Trim());

                cinifile.IniWriteValue("布匹信息", "布匹缸号", comboBoxGangHao.Text.Trim());
             //   cinifile.IniWriteValue("布匹信息", "布匹设备号", EquipmentNumber.Text.Trim());
                cinifile.IniWriteValue("布匹信息", "布匹卷数", textBoxTotalVolumn.Text.Trim());
                cinifile.IniWriteValue("布匹信息", "布匹米数", textBoxMarkedMeters.Text.Trim());
                cinifile.IniWriteValue("布匹信息", "货物来源", comboBoxSource.Text.Trim());
                // cinifile.IniWriteValue("布匹信息", "检测日期", textBox8.Text.Trim());


                if (comboBoxSource.Text.Contains("(")&&comboBoxSource.Text.Contains(")"))
                {
                    //如果是数据库里面的名字则提取名字跟ID
                    string [] sArray=comboBoxSource.Text.Split('(');
                    PublicClass.sourceName = sArray[0];
                    string[] sArray1 = sArray[1].Split(')');
                    PublicClass.sourceID = sArray1[0];
                }
                else
                {
                   PublicClass.sourceName = comboBoxSource.Text;
                }
                PublicClass.clothChangedFlag = true;
               
                //保存至ini文件；以布匹的批号为文件保存


                //确定后的操作
                this.Close();
            }
            else {

            }
        }

        private void textBoxBachNumer_TextChanged(object sender, EventArgs e)
        {
            
        }

        public static MySqlCommand getSqlCommand(String sql, MySqlConnection mysql)
        {
            MySqlCommand mySqlCommand = new MySqlCommand(sql, mysql);
            return mySqlCommand;
        }


        private void ClothDetailForm_Load(object sender, EventArgs e)
        {
            textBoxDevice.Text = PublicClass.theDeviceNumber;
            textBoxDevice.Enabled = false;

            /*************************************
               列出布匹类号
               *************************************/
            if (PublicClass.conn.State == ConnectionState.Open)
            {
                sqlcmd = "select ClothClassNumber from clothclass group by ClothClassNumber";
                mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
                mysqldr = mysqlcmd.ExecuteReader();
                while (mysqldr.Read())
                {
                    //构建一个ListView的数据，存入数据库数据，以便添加到listView1的行数据中
                    // ListViewItem lt = new ListViewItem();
                    //将数据库数据转变成ListView类型的一行数据
                    comboBoxClassNumber.Items.Add(mysqldr["ClothClassNumber"].ToString());

                }
                comboBoxClassNumber.Items.Add("");
                mysqldr.Close();

                /*************************************
                   列出缸号
                 *************************************/
                sqlcmd = "select CylinderNumber from clothcylinder group by CylinderNumber";
                mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
                mysqldr = mysqlcmd.ExecuteReader();
                while (mysqldr.Read())
                {
                    //构建一个ListView的数据，存入数据库数据，以便添加到listView1的行数据中
                    //ListViewItem lt = new ListViewItem();
                    //将数据库数据转变成ListView类型的一行数据
                    comboBoxGangHao.Items.Add(mysqldr["CylinderNumber"].ToString());

                }
                comboBoxClassNumber.Items.Add("");
                mysqldr.Close();


                /*************************************
               列出布匹来源
               *************************************/
                sqlcmd = "select * from  clothemployeedetails where employeeID";
                mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
                mysqldr = mysqlcmd.ExecuteReader();
                while (mysqldr.Read())
                {
                    //构建一个ListView的数据，存入数据库数据，以便添加到listView1的行数据中
                    //  ListViewItem lt = new ListViewItem();
                    //将数据库数据转变成ListView类型的一行数据
                    comboBoxSource.Items.Add(mysqldr["employeeName"].ToString() + "(" + mysqldr["employeeID"].ToString() + ")" + "-销售部");

                }
                comboBoxSource.Items.Add("");
                mysqldr.Close();

            }
                
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBoxClothNumber_TextChanged(object sender, EventArgs e)
        {



        }

        private void textBoxDevice_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxClothNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 0x20) e.KeyChar = (char)0;  //禁止空格键  

            if ((e.KeyChar >= 'a' && e.KeyChar <= 'z') || (e.KeyChar >= 'A' && e.KeyChar <= 'Z')
                 || (e.KeyChar >= '0' && e.KeyChar <= '9') || (e.KeyChar == 8))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }

          


            //if ((e.KeyChar == 0x2D) && (((TextBox)sender).Text.Length == 0)) return;   //处理负数  
            //if (e.KeyChar > 0x20)
            //{
            //    try
            //    {
            //        double.Parse(((TextBox)sender).Text + e.KeyChar.ToString());
            //    }
            //    catch
            //    {
            //        e.KeyChar = (char)0;   //处理非法字符  
            //    }
            //}
        }

        private void textBoxActualMeters_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)

                e.Handled = true;


            //小数点的处理。

            if ((int)e.KeyChar == 46)                           //小数点

            {

                if (textBoxMarkedMeters.Text.Length <= 0)

                    e.Handled = true;   //小数点不能在第一位

                else

                {

                    float f;

                    float oldf;

                    bool b1 = false, b2 = false;

                    b1 = float.TryParse(textBoxMarkedMeters.Text, out oldf);

                    b2 = float.TryParse(textBoxMarkedMeters.Text + e.KeyChar.ToString(), out f);

                    if (b2 == false)

                    {

                        if (b1 == true)

                            e.Handled = true;

                        else

                            e.Handled = false;

                    }

                }

            }
        }

        private void textBoxClothNumber_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 0x20) e.KeyChar = (char)0;  //禁止空格键  

            if ((e.KeyChar >= 'a' && e.KeyChar <= 'z') || (e.KeyChar >= 'A' && e.KeyChar <= 'Z')
                 || (e.KeyChar >= '0' && e.KeyChar <= '9') || (e.KeyChar == 8))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void textBoxVolumNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 0x20) e.KeyChar = (char)0;  //禁止空格键  

            if ( (e.KeyChar >= '0' && e.KeyChar <= '9') || (e.KeyChar == 8))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }

        }
    }
}
