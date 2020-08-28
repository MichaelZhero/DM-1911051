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
    public partial class AdminForm : Form
    {
        MySqlCommand mysqlcmd;//数据库执行命令
        MySqlDataReader mysqldr;//数据库查询结果
        string sqlcmd;
        MessageBoxForm messageboxForm;
        public AdminForm()
        {
            InitializeComponent();
        }

        private void AdminForm_Shown(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            


        }

        private void AdminForm_Load(object sender, EventArgs e)
        {
            bindListView();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxID.Enabled = true;

            sqlcmd = "select * from clothemployeedetails where department="+ (comboBox1.SelectedIndex+1).ToString()+ " order by employeeID desc";
            mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
            mysqldr = mysqlcmd.ExecuteReader();
            if (mysqldr.HasRows)
            {
                while (mysqldr.Read())
                {
                    textBoxID.Text = (Convert.ToInt64(mysqldr["employeeID"].ToString()) + 1).ToString();                 
                    break;
                }
            }
            mysqldr.Close();
            textBoxID.Enabled = false;

        }

        public static MySqlCommand getSqlCommand(String sql, MySqlConnection mysql)
        {
            MySqlCommand mySqlCommand = new MySqlCommand(sql, mysql);
            return mySqlCommand;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textBoxName.Text == "")
            {
                PublicClass.message = "姓名不能为空！";             
                messageboxForm = new MessageBoxForm(1);
                messageboxForm.Owner = this;
                messageboxForm.ShowDialog();
                return;
            }
            try
            {
                sqlcmd = "insert into clothemployeedetails (employeeID,employeeName,Department) values ('" + textBoxID.Text + "','" + textBoxName.Text + "','" + (comboBox1.SelectedIndex + 1).ToString() + "')";
                mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
                mysqlcmd.ExecuteNonQuery();
                PublicClass.message = "增加用户成功！";
                messageboxForm = new MessageBoxForm(1);
                messageboxForm.Owner = this;
                messageboxForm.ShowDialog();
                textBoxID.Text = (Convert.ToInt64(textBoxID.Text) + 1).ToString();
            }
            catch
            {
                sqlcmd = "insert into clothemployeedetails (employeeID,employeeName,Department) values ('" + textBoxID.Text + "','" + textBoxName.Text + "','" + (comboBox1.SelectedIndex + 1).ToString() + "')";
                mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
                mysqlcmd.ExecuteNonQuery();
                PublicClass.message = "增加用户失败！";
                messageboxForm = new MessageBoxForm(1);
                messageboxForm.Owner = this;
                messageboxForm.ShowDialog();
            }
           
        }

        private void bindListView()//定义ListView1的格式
        {
            listView1.GridLines = true;//表格是否显示网格线
            listView1.View = View.Details;//设置显示方式
            listView1.Scrollable = true;//是否自动显示滚动条
            //listView1.MultiSelect = true;//多选

            this.listView1.Columns.Add("序号");
            this.listView1.Columns.Add("工号");
            this.listView1.Columns.Add("姓名");
            //this.listView1.Columns.Add("生产日期");

            // this.listView1.View = System.Windows.Forms.View.Details;

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {
            listView1.Items.Clear();
            sqlcmd = "select * from  clothemployeedetails where Authority= '2'";
            mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
            mysqldr = mysqlcmd.ExecuteReader();
            while (mysqldr.Read())
            {
                //构建一个ListView的数据，存入数据库数据，以便添加到listView1的行数据中
                ListViewItem lt = new ListViewItem();
                lt.SubItems.Add(mysqldr["employeeID"].ToString());
                lt.SubItems.Add(mysqldr["employeeName"].ToString());
                listView1.Items.Add(lt);
            }
            mysqldr.Close();
            foreach (ColumnHeader ch in listView1.Columns)
            {
                ch.Width = -2;
            }
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                listView1.Items[i].SubItems[0].Text = (i + 1).ToString();
            }
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                PublicClass.message = "请选择删除项！";
                messageboxForm = new MessageBoxForm(1);
                messageboxForm.Owner = this;
                messageboxForm.ShowDialog();
                return;
            }
            DialogResult result = MessageBox.Show("确定删除选择项？", "温馨提示", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                try
                {
                    //删除
                    ListView.SelectedIndexCollection c = listView1.SelectedIndices;//当前选中行数
                    string tempEmployeeID = listView1.Items[c[0]].SubItems[1].Text.ToString();//查询员工号
                    sqlcmd = "delete  from  clothemployeedetails where employeeID=" + tempEmployeeID;
                    mysqlcmd = getSqlCommand(sqlcmd, PublicClass.conn);
                    mysqldr = mysqlcmd.ExecuteReader();
                    mysqldr.Close();
                    listView1.Items[listView1.SelectedItems[0].Index].Remove();
                }
                catch
                {
                    PublicClass.message = "选择的员工与其他数据库关联，无法删除！";
                    messageboxForm = new MessageBoxForm(1);
                    messageboxForm.Owner = this;
                    messageboxForm.ShowDialog();
                }
            }
            else 
            {
                
            }
        }
    }
}
