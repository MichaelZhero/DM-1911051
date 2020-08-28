namespace DALSA.SaperaLT.Demos.NET.CSharp.MultiBoardSyncGrabDemo
{
    partial class ParForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxThresh = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxDynThresh = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxArea = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxRa = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxSideWidth = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxMinExposueTime = new System.Windows.Forms.TextBox();
            this.textBoxMaxExposueTime = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxThresh
            // 
            this.textBoxThresh.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxThresh.Location = new System.Drawing.Point(126, 33);
            this.textBoxThresh.Name = "textBoxThresh";
            this.textBoxThresh.Size = new System.Drawing.Size(100, 29);
            this.textBoxThresh.TabIndex = 0;
            this.textBoxThresh.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxThresh_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(30, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 21);
            this.label1.TabIndex = 1;
            this.label1.Text = "检测阈值：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(279, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 21);
            this.label2.TabIndex = 3;
            this.label2.Text = "动态阈值：";
            // 
            // textBoxDynThresh
            // 
            this.textBoxDynThresh.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxDynThresh.Location = new System.Drawing.Point(375, 33);
            this.textBoxDynThresh.Name = "textBoxDynThresh";
            this.textBoxDynThresh.Size = new System.Drawing.Size(100, 29);
            this.textBoxDynThresh.TabIndex = 2;
            this.textBoxDynThresh.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxThresh_KeyPress);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(279, 98);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 21);
            this.label3.TabIndex = 7;
            this.label3.Text = "缺陷面积：";
            // 
            // textBoxArea
            // 
            this.textBoxArea.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxArea.Location = new System.Drawing.Point(375, 98);
            this.textBoxArea.Name = "textBoxArea";
            this.textBoxArea.Size = new System.Drawing.Size(100, 29);
            this.textBoxArea.TabIndex = 6;
            this.textBoxArea.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxThresh_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(30, 98);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 21);
            this.label4.TabIndex = 5;
            this.label4.Text = "缺陷长度：";
            // 
            // textBoxRa
            // 
            this.textBoxRa.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxRa.Location = new System.Drawing.Point(126, 98);
            this.textBoxRa.Name = "textBoxRa";
            this.textBoxRa.Size = new System.Drawing.Size(100, 29);
            this.textBoxRa.TabIndex = 4;
            this.textBoxRa.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxThresh_KeyPress);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(30, 160);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(90, 21);
            this.label6.TabIndex = 9;
            this.label6.Text = "边缘宽度：";
            // 
            // textBoxSideWidth
            // 
            this.textBoxSideWidth.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxSideWidth.Location = new System.Drawing.Point(126, 160);
            this.textBoxSideWidth.Name = "textBoxSideWidth";
            this.textBoxSideWidth.Size = new System.Drawing.Size(100, 29);
            this.textBoxSideWidth.TabIndex = 8;
            this.textBoxSideWidth.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxThresh_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(30, 222);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(154, 21);
            this.label5.TabIndex = 10;
            this.label5.Text = "曝光时间限定范围：";
            // 
            // textBoxMinExposueTime
            // 
            this.textBoxMinExposueTime.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxMinExposueTime.Location = new System.Drawing.Point(209, 222);
            this.textBoxMinExposueTime.Name = "textBoxMinExposueTime";
            this.textBoxMinExposueTime.Size = new System.Drawing.Size(100, 29);
            this.textBoxMinExposueTime.TabIndex = 11;
            this.textBoxMinExposueTime.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxThresh_KeyPress);
            // 
            // textBoxMaxExposueTime
            // 
            this.textBoxMaxExposueTime.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxMaxExposueTime.Location = new System.Drawing.Point(375, 222);
            this.textBoxMaxExposueTime.Name = "textBoxMaxExposueTime";
            this.textBoxMaxExposueTime.Size = new System.Drawing.Size(100, 29);
            this.textBoxMaxExposueTime.TabIndex = 12;
            this.textBoxMaxExposueTime.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxThresh_KeyPress);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(332, 222);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(22, 21);
            this.label7.TabIndex = 13;
            this.label7.Text = "~";
            // 
            // comboBox1
            // 
            this.comboBox1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(375, 160);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(100, 29);
            this.comboBox1.TabIndex = 17;
            this.comboBox1.Text = "选择串口";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(279, 160);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(90, 21);
            this.label8.TabIndex = 16;
            this.label8.Text = "串口设置：";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.Location = new System.Drawing.Point(199, 279);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(118, 51);
            this.button1.TabIndex = 18;
            this.button1.Text = "保存设置";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ParForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 354);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBoxMaxExposueTime);
            this.Controls.Add(this.textBoxMinExposueTime);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBoxSideWidth);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxArea);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxRa);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxDynThresh);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxThresh);
            this.Name = "ParForm";
            this.Text = "参数设置";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ParForm_FormClosed);
            this.Load += new System.EventHandler(this.parForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxThresh;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxDynThresh;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxArea;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxRa;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxSideWidth;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxMinExposueTime;
        private System.Windows.Forms.TextBox textBoxMaxExposueTime;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button1;
    }
}