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
using System.Management;
using System.Diagnostics;


namespace DALSA.SaperaLT.Demos.NET.CSharp.MultiBoardSyncGrabDemo
{
    public partial class PasswordForm : Form
    {
        Process kbpr;
        MySqlCommand mysqlcmd;//数据库执行命令
        MySqlDataReader mysqldr;//数据库查询结果
        string sqlcmd;
        MessageBoxForm messageboxForm;
        bool logSucces = false;
        string ID;
        public PasswordForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
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

            if (textBoxOldPassword.Text == "" || textBoxNewPassword.Text == "" || textBoxNewPassword1.Text == "")
            {
                PublicClass.message = "密码不能为空！";
                messageboxForm = new MessageBoxForm(1);
                messageboxForm.Owner = this;
                messageboxForm.ShowDialog();
                return;
            }

            if (textBoxNewPassword.Text != textBoxNewPassword1.Text)
            {
                PublicClass.message = "两次密码不一致！";
                messageboxForm = new MessageBoxForm(1);
                messageboxForm.Owner = this;
                messageboxForm.ShowDialog();
                return;
            }
            sqlcmd = "select * from clothemployeedetails where employeeID='" + textBoxID.Text + "'";
            mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
            mysqldr = mysqlcmd.ExecuteReader();
            if (mysqldr.HasRows)
            {
                mysqldr.Read();                                  
                if (mysqldr["Password"].ToString() == textBoxOldPassword.Text)
                {
                    ID = mysqldr["employeeID"].ToString();
                    //登录成功
                    logSucces = true;

                }
                else
                {
                    PublicClass.message = "旧密码错误！";
                    messageboxForm = new MessageBoxForm(1);
                    messageboxForm.Owner = this;
                    messageboxForm.ShowDialog();
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
                    if (mysqldr["Password"].ToString() == textBoxOldPassword.Text)
                    {
                        ID = mysqldr["employeeID"].ToString();
                        //登录成功
                        logSucces = true;
                    }
                    else
                    {
                        PublicClass.message = "旧密码错误！";
                        messageboxForm = new MessageBoxForm(1);
                        messageboxForm.Owner = this;
                        messageboxForm.ShowDialog();
                    }
                }
                else {
                    PublicClass.message = "工号或者姓名不存在！";
                    messageboxForm = new MessageBoxForm(1);
                    messageboxForm.Owner = this;
                    messageboxForm.ShowDialog();
                }
                mysqldr.Close();
            }
            if(logSucces)
            {
                DialogResult result = MessageBox.Show("确定修改密码？", "温馨提示", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        sqlcmd = "update clothemployeedetails set password='" + textBoxNewPassword.Text + "' where employeeID = "+ ID;
                        mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
                        mysqlcmd.ExecuteNonQuery();
                        PublicClass.message = "密码修改成功！";
                        messageboxForm = new MessageBoxForm(1);
                        messageboxForm.Owner = this;
                        messageboxForm.ShowDialog();

                    }
                    catch
                    {
                        PublicClass.message = "密码修改失败！";
                        messageboxForm = new MessageBoxForm(1);
                        messageboxForm.Owner = this;
                        messageboxForm.ShowDialog();
                    }

                }
            }
            logSucces = false;


        }

        public static MySqlCommand getSqlCommand(String sql, MySqlConnection mysql)
        {
            MySqlCommand mySqlCommand = new MySqlCommand(sql, mysql);
            return mySqlCommand;
        }

        private void PasswordForm_Load(object sender, EventArgs e)
        {

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

        private void PasswordForm_FormClosed(object sender, FormClosedEventArgs e)
        {
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
