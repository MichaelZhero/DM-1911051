using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace DALSA.SaperaLT.Demos.NET.CSharp.MultiBoardSyncGrabDemo
{
    public partial class ParForm : Form
    {
        public ParForm()
        {
            InitializeComponent();
            textBoxThresh.Text = PublicClass.Thresh.ToString();
            textBoxDynThresh.Text = PublicClass.dynthresh.ToString();
            textBoxRa.Text = PublicClass.ra.ToString();
            textBoxArea.Text = PublicClass.defectArea.ToString();
            textBoxSideWidth.Text = PublicClass.sideWidth.ToString();
            textBoxMinExposueTime.Text = PublicClass.minExposureTime.ToString();
            textBoxMaxExposueTime.Text = PublicClass.maxExposureTime.ToString();
            string[] ports = SerialPort.GetPortNames();
            comboBox1.Items.AddRange(ports);
        }

        private void parForm_Load(object sender, EventArgs e)
        {
            try
            {
                comboBox1.Text = PublicClass.serialPort1Name;
            }
            catch
            {

            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
          
            if (Convert.ToInt32(textBoxMaxExposueTime.Text) > Convert.ToInt32(textBoxMaxExposueTime.Text))
            {
                MessageBox.Show("曝光时间限定范围设置错误！");
                return;
            }
            if(textBoxMaxExposueTime.Text==""|| textBoxMaxExposueTime.Text==""|| textBoxThresh.Text==""|| textBoxDynThresh.Text==""|| textBoxRa.Text==""|| textBoxArea.Text==""|| textBoxSideWidth.Text=="")
            {
                MessageBox.Show("参数输入不能为空！");
                return;
            }

            DialogResult result = MessageBox.Show("确定保存么？", "请输入选择", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                //if (comboBox1.Text != PublicClass.portName && comboBox1.Text!=""&& comboBox1.Text != "请选择串口")
                //{
                //    PublicClass.portName = comboBox1.Text;
                //   // PublicClass.portChanged = true;
                //}
                try
                {
                    PublicClass.minExposureTime = Convert.ToInt32(textBoxMinExposueTime.Text);
                    PublicClass.maxExposureTime = Convert.ToInt32(textBoxMaxExposueTime.Text);
                    PublicClass.Thresh = Convert.ToInt32(textBoxThresh.Text);
                    PublicClass.dynthresh = Convert.ToInt32(textBoxDynThresh.Text);
                    PublicClass.ra = Convert.ToInt32(textBoxRa.Text);
                    PublicClass.defectArea = Convert.ToInt32(textBoxArea.Text);
                    PublicClass.sideWidth = Convert.ToInt32(textBoxSideWidth.Text);

                    PublicClass.parChanged = true;
                    this.Close();

                }
                catch
                {
                    MessageBox.Show("参数有误！");
                }
               

                //SaveParmeterToDisk();
                //确定后的操作
            }

        }

        private void textBoxThresh_KeyPress(object sender, KeyPressEventArgs e)
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

        private void ParForm_FormClosed(object sender, FormClosedEventArgs e)
        {
           //ublicClass.portChanged= true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (comboBox1.Text != PublicClass.portName && comboBox1.Text !="选择串口")
            //{
            //    PublicClass.portName = comboBox1.Text;
            //    PublicClass.portChanged = true;
            //}
        }
    }
}
