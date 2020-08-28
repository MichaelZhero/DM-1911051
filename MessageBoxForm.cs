using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DALSA.SaperaLT.Demos.NET.CSharp.MultiBoardSyncGrabDemo
{
    public partial class MessageBoxForm : Form
    {
        int locationX;
        int locationY;
        System.Drawing.Size mSize = SystemInformation.WorkingArea.Size;
        public MessageBoxForm()
        {
            InitializeComponent();
            label1.Text = PublicClass.message;
            locationX = (int)(0.4 * mSize.Width);
            locationY = (int)(0.5 * mSize.Height);
            //if (PublicClass.messageflag)
            //    AutoCloseTimer.Enabled = true;
        }
        public MessageBoxForm(int a)//自动关闭
        {
            InitializeComponent();
            label1.Text = PublicClass.message;
            locationX = (int)(0.4 * mSize.Width);
            locationY = (int)(0.5 * mSize.Height);
            //if (PublicClass.messageflag)
            AutoCloseTimer.Enabled = true;
        }

        public MessageBoxForm(int a,int lasttime)//自动关闭
        {
            InitializeComponent();
            label1.Text = PublicClass.message;
            locationX = (int)(0.4 * mSize.Width);
            locationY = (int)(0.5 * mSize.Height);
            //if (PublicClass.messageflag)
            AutoCloseTimer.Interval = lasttime;
            AutoCloseTimer.Enabled = true;
        }

        //public MessageBoxForm(MainForm mainForm)
        //{
        //    InitializeComponent();
        //    label1.Text = mainForm.message;
        //    locationX = (int)(mainForm.Location.X + mainForm.Width * 0.3);
        //    locationY = (int)(mainForm.Location.Y + mainForm.Height * 0.3);
        //}

        //public MessageBoxForm(MainForm mainForm,int flag)
        //{
        //    InitializeComponent();
        //    label1.Text = mainForm.message;
        //    locationX = (int)(mainForm.Location.X + mainForm.Width * 0.3);
        //    locationY = (int)(mainForm.Location.Y + mainForm.Height * 0.3);
        //    if (flag == 1)
        //    AutoCloseTimer.Enabled = true;
        //}

        //public MessageBoxForm(InitForm initForm, int flag)
        //{
        //    InitializeComponent();
        //    label1.Text = initForm.message;
        //    locationX = (int)(initForm.Location.X + initForm.Width * 0.4);
        //    locationY = (int)(initForm.Location.Y + initForm.Height * 0.2);
        //    if (flag == 1)
        //        AutoCloseTimer.Enabled = true;
        //}


        //public MessageBoxForm(MarkForm markForm, int flag)
        //{
        //    InitializeComponent();
        //    label1.Text = markForm.message;
        //    locationX = (int)(markForm.Location.X + markForm.Width * 0.4);
        //    locationY = (int)(markForm.Location.Y + markForm.Height * 0.2);
        //    if (flag == 1)
        //        AutoCloseTimer.Enabled = true;
        //}

        //public MessageBoxForm(BrightnessForm brightnessForm, int flag)
        //{
        //    InitializeComponent();
        //    label1.Text = brightnessForm.message;
        //    locationX = (int)(brightnessForm.Location.X + brightnessForm.Width * 0.45);
        //    locationY = (int)(brightnessForm.Location.Y + brightnessForm.Height * 0.2);
        //    if (flag == 1)
        //        AutoCloseTimer.Enabled = true;
        //}

        //public MessageBoxForm(PasswordForm passwordForm, int flag)
        //{
        //    InitializeComponent();
        //    label1.Text = passwordForm.message;
        //    locationX = (int)(passwordForm.Location.X + passwordForm.Width * 0.5);
        //    locationY = (int)(passwordForm.Location.Y + passwordForm.Height * 0.3);
        //    if (flag == 1)
        //        AutoCloseTimer.Enabled = true;

        //}

        //public MessageBoxForm(AutoDetectForm autoDetectForm, int flag)
        //{
        //    InitializeComponent();
        //    label1.Text = autoDetectForm.message;
        //    locationX = (int)(autoDetectForm.Location.X + autoDetectForm.Width * 0.5);
        //    locationY = (int)(autoDetectForm.Location.Y + autoDetectForm.Height * 0.5);
        //    if (flag == 1)
        //        AutoCloseTimer.Enabled = true;
        //}

        //public MessageBoxForm(ManualDetectForm manualDetectForm, int flag)
        //{
        //    InitializeComponent();
        //    label1.Text = manualDetectForm.message;
        //    locationX = (int)(manualDetectForm.Location.X + manualDetectForm.Width * 0.5);
        //    locationY = (int)(manualDetectForm.Location.Y + manualDetectForm.Height * 0.5);
        //    if (flag == 1)
        //        AutoCloseTimer.Enabled = true;
        //}

        //public MessageBoxForm(ParameterForm Form1, int flag)
        //{
        //    InitializeComponent();
        //    label1.Text = Form1.message;
        //    locationX = (int)(Form1.Location.X + Form1.Width * 0.5);
        //    locationY = (int) (Form1.Location.Y + Form1.Height * 0.5);
        //    if (flag == 1)
        //        AutoCloseTimer.Enabled = true;
        //}


        private void MessageBoxForm_Load(object sender, EventArgs e)
        {
            this.Width= label1.Width + 50;
            this.Location = new Point(locationX, locationY);

        }

        private void AutoCloseTimer_Tick(object sender, EventArgs e)
        {
            AutoCloseTimer.Enabled = false;
            this.Close();
        }
    }
}
