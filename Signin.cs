using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.Diagnostics;
using MySql.Data.MySqlClient;

namespace DALSA.SaperaLT.Demos.NET.CSharp.MultiBoardSyncGrabDemo
{
    public partial class Form1 : Form
    {
        private  String mysqlcon = "database=mysql;Password=;User ID=root;server=localhost";//定义数据库连接账号密码
        Process kbpr;
        string sqlcmd;
        MySqlCommand mysqlcmd;//数据库执行命令
        MySqlDataReader mysqldr;//数据库查询结果
        MessageBoxForm messageboxForm;

        public Form1()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBoxID_MouseClick(object sender, MouseEventArgs e)
        {           
            try
            {
                kbpr.Kill();

            }
            catch
            {
            }
            //******************************************
            //如果软键盘标志位打开，则打开软键盘
            //******************************************
            if (checkBox1.Checked)
                kbpr = System.Diagnostics.Process.Start("osk.exe"); // 打开系统键盘
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            //******************************************
            //关闭软键盘
            //******************************************
            try
            {
                kbpr.Kill();                
            }
            catch
            {
            }

            if (textBoxID.Text == "")
            {
                PublicClass.message = "姓名或者工号不能为空！";
                messageboxForm = new MessageBoxForm(1);
                messageboxForm.Owner = this;
                messageboxForm.ShowDialog();
                return;
            }
            if (textBoxPassword.Text == "")
            {
                PublicClass.message = "密码不能为空！";
                messageboxForm = new MessageBoxForm(1);
                messageboxForm.Owner = this;
                messageboxForm.ShowDialog();
                return;
            }
            //******************************************
            //查询数据库，验证登录账户，密码，及用户权限
            //******************************************
            sqlcmd = "select * from clothemployeedetails where employeeID='"+ textBoxID.Text+"'";
            mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
            mysqldr = mysqlcmd.ExecuteReader();
            if (mysqldr.HasRows)
            {
                mysqldr.Read();
                {
                    if (mysqldr["Password"].ToString() == textBoxPassword.Text)
                    {
                        //登录成功
                        if (Convert.ToInt32(mysqldr["Authority"].ToString()) == 1)
                        {
                            //管理员模式
                            AdminForm adminForm = new AdminForm();
                            adminForm.ShowDialog();
                        }
                        else
                        {
                            if (Convert.ToInt32(mysqldr["Department"].ToString()) == 1)
                            {
                                //检测部门
                             //   DetectForm detectForm = new DetectForm();
                                

                                MultiBoardSyncGrabDemoDlg form = new MultiBoardSyncGrabDemoDlg();
                                form.ShowDialog();
                            }
                            else
                            {
                                //销售部门
                                PublicClass.message = "非检测部门员工不能操作设备！";
                                messageboxForm = new MessageBoxForm(1);
                                messageboxForm.Owner = this;
                                messageboxForm.ShowDialog();
                                return;

                            }
                        }
                    }
                    else
                    {
                        PublicClass.message = "密码错误！";
                        messageboxForm = new MessageBoxForm(1);
                        messageboxForm.Owner = this;
                        messageboxForm.ShowDialog();
                    }                 
                }
                mysqldr.Close();
            }
            else
            {
                mysqldr.Close();
                sqlcmd = "select * from clothemployeedetails where employeename='" + textBoxID.Text + "'";
                mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
                mysqldr = mysqlcmd.ExecuteReader();
                if (mysqldr.HasRows)
                {
                    mysqldr.Read();
                    {
                        if (mysqldr["Password"].ToString() == textBoxPassword.Text)
                        {
                            //登录成功
                            if (Convert.ToInt32(mysqldr["Authority"].ToString()) == 1)
                            {
                                mysqldr.Close();
                                //管理员模式
                                AdminForm adminForm = new AdminForm();
                                adminForm.ShowDialog();
                            }
                            else
                            {
                                if (Convert.ToInt32(mysqldr["Department"].ToString()) == 1)
                                {
                                    //检测部门
                                    MultiBoardSyncGrabDemoDlg form = new MultiBoardSyncGrabDemoDlg();
                                    form.Show();
                                }
                                else
                                {
                                    //销售部门
                                    PublicClass.message = "非检测部门员工不能操作设备！";
                                    messageboxForm = new MessageBoxForm(1);
                                    messageboxForm.Owner = this;
                                    messageboxForm.ShowDialog();
                                    return;

                                }

                            }
                        }

                        else
                        {
                            PublicClass.message = "密码错误！";
                            messageboxForm = new MessageBoxForm(1);
                            messageboxForm.Owner = this;
                            messageboxForm.ShowDialog();
                        }

                    }
                }
                else
                {
                    PublicClass.message = "工号或者姓名不存在！";
                    messageboxForm = new MessageBoxForm(1);
                    messageboxForm.Owner = this;
                    messageboxForm.ShowDialog();
                }
                mysqldr.Close();
            }


        }

        public static MySqlCommand getSqlCommand(String sql, MySqlConnection mysql)
        {
            MySqlCommand mySqlCommand = new MySqlCommand(sql, mysql);
            return mySqlCommand;
        }


        private void Form1_Shown(object sender, EventArgs e)
        {
            try
            {//连接数据库
                PublicClass.conn = new MySqlConnection(mysqlcon);
                PublicClass.conn.Open();
                PublicClass.message = "数据库连接成功！";       
                messageboxForm = new MessageBoxForm(1);
                messageboxForm.Owner = this;
                messageboxForm.ShowDialog();
            }
            catch
            {
                PublicClass.message = "数据库连接失败！";
               
                messageboxForm = new MessageBoxForm(1);
                messageboxForm.Owner = this;
                messageboxForm.ShowDialog();
                return;
            }

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                kbpr = System.Diagnostics.Process.Start("osk.exe"); // 打开系统键盘 
            }
            else
            {
                try
                {
                    kbpr.Kill();
                }
                catch
                { }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            PasswordForm passWordForm = new PasswordForm();
            passWordForm.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(this.buttonChangePassword, "登录成功后方可修改密码！");
        }

        private void buttonLogout_Click(object sender, EventArgs e)
        {
        //    DialogResult result = MessageBox.Show("确定退出程序？", "温馨提示", MessageBoxButtons.YesNo);
        //    {
        //        if (result == DialogResult.Yes)
        //        try
        //        {
        //            PublicClass.conn.Close();

        //        }
        //        catch
        //        {
                        
        //        }
        //        try
        //        {
        //            kbpr.Kill();
        //        }
        //        catch
        //        {
        //        }
        //        Application.Exit();
        //    }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                PublicClass.conn.Close();

            }
            catch
            {

            }
            try
            {
                kbpr.Kill();
            }
            catch
            {
            }
        }
    }
}
