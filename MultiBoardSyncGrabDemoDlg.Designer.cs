
using DALSA.SaperaLT.SapClassBasic;
using DALSA.SaperaLT.SapClassGui;

namespace DALSA.SaperaLT.Demos.NET.CSharp.MultiBoardSyncGrabDemo
{
    partial class MultiBoardSyncGrabDemoDlg
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MultiBoardSyncGrabDemoDlg));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.StatusLabelInfo = new System.Windows.Forms.ToolStripLabel();
            this.StatusLabelInfoTrash = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel6 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.StatusLabelInfo1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel7 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.PixelDataValue = new System.Windows.Forms.ToolStripLabel();
            this.btn_HistoricalReport = new System.Windows.Forms.Button();
            this.btn_Exit = new System.Windows.Forms.Button();
            this.btn_Freeze = new System.Windows.Forms.Button();
            this.btn_Grab = new System.Windows.Forms.Button();
            this.button_Snap = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.检测米数 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.检测时间 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.缺陷结果 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.色差均值 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.btn_outlineclick = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.btn_Setting = new System.Windows.Forms.Button();
            this.btn_writetodb = new System.Windows.Forms.Button();
            this.chk_saveimg = new System.Windows.Forms.CheckBox();
            this.chk_standlab = new System.Windows.Forms.CheckBox();
            this.Chk_Algorithms = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.chk_writetodb = new System.Windows.Forms.CheckBox();
            this.checkBoxExplosueControl = new System.Windows.Forms.CheckBox();
            this.chk_executecmd = new System.Windows.Forms.CheckBox();
            this.button4 = new System.Windows.Forms.Button();
            this.StatusTimer = new System.Windows.Forms.Timer(this.components);
            this.label5 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.label7 = new System.Windows.Forms.Label();
            this.btn_changeCloth = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelSourceName = new System.Windows.Forms.Label();
            this.labelTotalVolumnNumber = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.labelDetectName = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.labelActualMeters = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.labelCylinderNumber = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.labelVolumnNumber = new System.Windows.Forms.Label();
            this.labelClassNumber = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.labelBatchNumber = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.labelClothNumber = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btn_CurrentReport = new System.Windows.Forms.Button();
            this.label18 = new System.Windows.Forms.Label();
            this.btn_help = new System.Windows.Forms.Button();
            this.hWindowControl2 = new HalconDotNet.HWindowControl();
            this.chk_contiouswhenoutline = new System.Windows.Forms.CheckBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.chk_outlinebythread = new System.Windows.Forms.CheckBox();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabelInfo,
            this.StatusLabelInfoTrash,
            this.toolStripSeparator6,
            this.toolStripLabel6,
            this.toolStripSeparator8,
            this.StatusLabelInfo1,
            this.toolStripSeparator7,
            this.toolStripLabel5,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.toolStripSeparator3,
            this.toolStripLabel2,
            this.toolStripSeparator5,
            this.toolStripLabel3,
            this.toolStripSeparator4,
            this.toolStripLabel4,
            this.toolStripLabel7,
            this.toolStripSeparator2,
            this.PixelDataValue});
            this.toolStrip1.Location = new System.Drawing.Point(0, 722);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(1346, 25);
            this.toolStrip1.TabIndex = 11;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // StatusLabelInfo
            // 
            this.StatusLabelInfo.Name = "StatusLabelInfo";
            this.StatusLabelInfo.Size = new System.Drawing.Size(80, 22);
            this.StatusLabelInfo.Text = "相机状态：无";
            // 
            // StatusLabelInfoTrash
            // 
            this.StatusLabelInfoTrash.Name = "StatusLabelInfoTrash";
            this.StatusLabelInfoTrash.Size = new System.Drawing.Size(0, 22);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel6
            // 
            this.toolStripLabel6.Name = "toolStripLabel6";
            this.toolStripLabel6.Size = new System.Drawing.Size(80, 22);
            this.toolStripLabel6.Text = "左相机采集：";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
            // 
            // StatusLabelInfo1
            // 
            this.StatusLabelInfo1.Name = "StatusLabelInfo1";
            this.StatusLabelInfo1.Size = new System.Drawing.Size(80, 22);
            this.StatusLabelInfo1.Text = "右相机采集：";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel5
            // 
            this.toolStripLabel5.Name = "toolStripLabel5";
            this.toolStripLabel5.Size = new System.Drawing.Size(104, 22);
            this.toolStripLabel5.Text = "数据库状态：连接";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(92, 22);
            this.toolStripLabel1.Text = "电机串口：打开";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(99, 22);
            this.toolStripLabel2.Text = "相机1串口：打开";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(99, 22);
            this.toolStripLabel3.Text = "相机2串口：打开";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel4
            // 
            this.toolStripLabel4.Name = "toolStripLabel4";
            this.toolStripLabel4.Size = new System.Drawing.Size(0, 22);
            // 
            // toolStripLabel7
            // 
            this.toolStripLabel7.Name = "toolStripLabel7";
            this.toolStripLabel7.Size = new System.Drawing.Size(68, 22);
            this.toolStripLabel7.Text = "开始时间：";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // PixelDataValue
            // 
            this.PixelDataValue.Name = "PixelDataValue";
            this.PixelDataValue.Size = new System.Drawing.Size(80, 22);
            this.PixelDataValue.Text = "像素：无数据";
            this.PixelDataValue.Visible = false;
            // 
            // btn_HistoricalReport
            // 
            this.btn_HistoricalReport.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.btn_HistoricalReport.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_HistoricalReport.Location = new System.Drawing.Point(1071, 599);
            this.btn_HistoricalReport.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_HistoricalReport.Name = "btn_HistoricalReport";
            this.btn_HistoricalReport.Size = new System.Drawing.Size(125, 55);
            this.btn_HistoricalReport.TabIndex = 13;
            this.btn_HistoricalReport.Text = "历史报告";
            this.btn_HistoricalReport.UseVisualStyleBackColor = false;
            this.btn_HistoricalReport.Click += new System.EventHandler(this.button2_Click);
            // 
            // btn_Exit
            // 
            this.btn_Exit.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.btn_Exit.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_Exit.Location = new System.Drawing.Point(1213, 663);
            this.btn_Exit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_Exit.Name = "btn_Exit";
            this.btn_Exit.Size = new System.Drawing.Size(125, 55);
            this.btn_Exit.TabIndex = 10;
            this.btn_Exit.Text = "退出系统";
            this.btn_Exit.UseVisualStyleBackColor = false;
            this.btn_Exit.Click += new System.EventHandler(this.button_Exit_Click);
            // 
            // btn_Freeze
            // 
            this.btn_Freeze.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.btn_Freeze.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_Freeze.Location = new System.Drawing.Point(1071, 536);
            this.btn_Freeze.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_Freeze.Name = "btn_Freeze";
            this.btn_Freeze.Size = new System.Drawing.Size(125, 55);
            this.btn_Freeze.TabIndex = 2;
            this.btn_Freeze.Text = "停止检测";
            this.btn_Freeze.UseVisualStyleBackColor = false;
            this.btn_Freeze.Click += new System.EventHandler(this.button_Freeze_Click);
            // 
            // btn_Grab
            // 
            this.btn_Grab.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.btn_Grab.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_Grab.Location = new System.Drawing.Point(929, 536);
            this.btn_Grab.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_Grab.Name = "btn_Grab";
            this.btn_Grab.Size = new System.Drawing.Size(125, 55);
            this.btn_Grab.TabIndex = 1;
            this.btn_Grab.Text = "在线检测";
            this.btn_Grab.UseVisualStyleBackColor = false;
            this.btn_Grab.Click += new System.EventHandler(this.button_Grab_Click);
            // 
            // button_Snap
            // 
            this.button_Snap.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.button_Snap.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button_Snap.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_Snap.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button_Snap.Location = new System.Drawing.Point(1213, 602);
            this.button_Snap.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_Snap.Name = "button_Snap";
            this.button_Snap.Size = new System.Drawing.Size(125, 55);
            this.button_Snap.TabIndex = 0;
            this.button_Snap.Text = "采集图像";
            this.button_Snap.UseVisualStyleBackColor = false;
            this.button_Snap.Visible = false;
            this.button_Snap.Click += new System.EventHandler(this.button_Snap_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedHeaders;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.检测米数,
            this.检测时间,
            this.缺陷结果,
            this.色差均值});
            this.dataGridView1.Location = new System.Drawing.Point(933, 13);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dataGridView1.Name = "dataGridView1";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(405, 135);
            this.dataGridView1.TabIndex = 15;
            // 
            // 检测米数
            // 
            this.检测米数.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.检测米数.HeaderText = "检测米数";
            this.检测米数.Name = "检测米数";
            this.检测米数.Width = 81;
            // 
            // 检测时间
            // 
            this.检测时间.HeaderText = "检测时间";
            this.检测时间.Name = "检测时间";
            this.检测时间.Width = 81;
            // 
            // 缺陷结果
            // 
            this.缺陷结果.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.缺陷结果.HeaderText = "检测结果";
            this.缺陷结果.Name = "缺陷结果";
            this.缺陷结果.Width = 81;
            // 
            // 色差均值
            // 
            this.色差均值.HeaderText = "色差均值";
            this.色差均值.Name = "色差均值";
            this.色差均值.Width = 81;
            // 
            // chart1
            // 
            this.chart1.BackColor = System.Drawing.Color.Gray;
            chartArea1.AxisX.Interval = 5D;
            chartArea1.AxisX.Maximum = 100D;
            chartArea1.AxisX.Title = "米数(m)";
            chartArea1.AxisX.TitleFont = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            chartArea1.AxisY.Interval = 2D;
            chartArea1.AxisY.Maximum = 8D;
            chartArea1.AxisY.Title = "色差(nbs)";
            chartArea1.AxisY.TitleAlignment = System.Drawing.StringAlignment.Far;
            chartArea1.AxisY.TitleFont = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            chartArea1.BackColor = System.Drawing.Color.Silver;
            chartArea1.InnerPlotPosition.Auto = false;
            chartArea1.InnerPlotPosition.Height = 65F;
            chartArea1.InnerPlotPosition.Width = 95F;
            chartArea1.InnerPlotPosition.X = 5F;
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Location = new System.Drawing.Point(12, 536);
            this.chart1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Font = new System.Drawing.Font("微软雅黑", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            series1.LabelBackColor = System.Drawing.Color.WhiteSmoke;
            series1.LegendText = "1-左右色差";
            series1.Name = "1-左中色差";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Font = new System.Drawing.Font("微软雅黑", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            series2.LegendText = "1-中右色差";
            series2.Name = "1-中右色差";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series3.Font = new System.Drawing.Font("微软雅黑", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            series3.LegendText = "1-左右色差";
            series3.Name = "1-左右色差";
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series4.Font = new System.Drawing.Font("微软雅黑", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            series4.LegendText = "2-左中色差";
            series4.Name = "2-左中色差";
            series5.ChartArea = "ChartArea1";
            series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series5.Font = new System.Drawing.Font("微软雅黑", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            series5.LegendText = "2-中右色差";
            series5.Name = "2-中右色差";
            series6.ChartArea = "ChartArea1";
            series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series6.Font = new System.Drawing.Font("微软雅黑", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            series6.LegendText = "2-左右色差";
            series6.Name = "2-左右色差";
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Series.Add(series3);
            this.chart1.Series.Add(series4);
            this.chart1.Series.Add(series5);
            this.chart1.Series.Add(series6);
            this.chart1.Size = new System.Drawing.Size(900, 182);
            this.chart1.TabIndex = 16;
            this.chart1.Text = "chart1";
            title1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            title1.Name = "色差曲线图";
            title1.Text = "色差曲线图";
            this.chart1.Titles.Add(title1);
            // 
            // btn_outlineclick
            // 
            this.btn_outlineclick.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.btn_outlineclick.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_outlineclick.Location = new System.Drawing.Point(1213, 536);
            this.btn_outlineclick.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_outlineclick.Name = "btn_outlineclick";
            this.btn_outlineclick.Size = new System.Drawing.Size(125, 55);
            this.btn_outlineclick.TabIndex = 18;
            this.btn_outlineclick.Text = "离线检测";
            this.btn_outlineclick.UseVisualStyleBackColor = false;
            this.btn_outlineclick.Click += new System.EventHandler(this.halcon_Click);
            // 
            // listBox1
            // 
            this.listBox1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.HorizontalScrollbar = true;
            this.listBox1.ItemHeight = 17;
            this.listBox1.Location = new System.Drawing.Point(933, 156);
            this.listBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listBox1.Name = "listBox1";
            this.listBox1.ScrollAlwaysVisible = true;
            this.listBox1.Size = new System.Drawing.Size(405, 106);
            this.listBox1.TabIndex = 18;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // btn_Setting
            // 
            this.btn_Setting.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.btn_Setting.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_Setting.Location = new System.Drawing.Point(929, 663);
            this.btn_Setting.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_Setting.Name = "btn_Setting";
            this.btn_Setting.Size = new System.Drawing.Size(125, 55);
            this.btn_Setting.TabIndex = 22;
            this.btn_Setting.Text = "参数设置";
            this.btn_Setting.UseVisualStyleBackColor = false;
            this.btn_Setting.Click += new System.EventHandler(this.Setting_Click);
            // 
            // btn_writetodb
            // 
            this.btn_writetodb.Location = new System.Drawing.Point(1253, 722);
            this.btn_writetodb.Name = "btn_writetodb";
            this.btn_writetodb.Size = new System.Drawing.Size(81, 27);
            this.btn_writetodb.TabIndex = 28;
            this.btn_writetodb.Text = "写入数据库";
            this.btn_writetodb.UseVisualStyleBackColor = true;
            this.btn_writetodb.Visible = false;
            this.btn_writetodb.Click += new System.EventHandler(this.btn_writetodb_Click);
            // 
            // chk_saveimg
            // 
            this.chk_saveimg.AutoSize = true;
            this.chk_saveimg.Checked = true;
            this.chk_saveimg.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_saveimg.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chk_saveimg.Location = new System.Drawing.Point(13, 30);
            this.chk_saveimg.Name = "chk_saveimg";
            this.chk_saveimg.Size = new System.Drawing.Size(93, 25);
            this.chk_saveimg.TabIndex = 24;
            this.chk_saveimg.Text = "保存图像";
            this.chk_saveimg.UseVisualStyleBackColor = true;
            // 
            // chk_standlab
            // 
            this.chk_standlab.AutoSize = true;
            this.chk_standlab.Checked = true;
            this.chk_standlab.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_standlab.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chk_standlab.Location = new System.Drawing.Point(142, 3);
            this.chk_standlab.Name = "chk_standlab";
            this.chk_standlab.Size = new System.Drawing.Size(93, 25);
            this.chk_standlab.TabIndex = 25;
            this.chk_standlab.Text = "标准布匹";
            this.chk_standlab.UseVisualStyleBackColor = true;
            this.chk_standlab.CheckedChanged += new System.EventHandler(this.chk_standlab_CheckedChanged);
            // 
            // Chk_Algorithms
            // 
            this.Chk_Algorithms.AutoSize = true;
            this.Chk_Algorithms.Checked = true;
            this.Chk_Algorithms.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Chk_Algorithms.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Chk_Algorithms.Location = new System.Drawing.Point(13, 3);
            this.Chk_Algorithms.Name = "Chk_Algorithms";
            this.Chk_Algorithms.Size = new System.Drawing.Size(93, 25);
            this.Chk_Algorithms.TabIndex = 26;
            this.Chk_Algorithms.Text = "算法检测";
            this.Chk_Algorithms.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.button1);
            this.panel2.Controls.Add(this.chk_writetodb);
            this.panel2.Controls.Add(this.checkBoxExplosueControl);
            this.panel2.Controls.Add(this.chk_executecmd);
            this.panel2.Controls.Add(this.Chk_Algorithms);
            this.panel2.Controls.Add(this.chk_saveimg);
            this.panel2.Controls.Add(this.chk_standlab);
            this.panel2.Location = new System.Drawing.Point(933, 269);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(405, 64);
            this.panel2.TabIndex = 27;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(233, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 55;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // chk_writetodb
            // 
            this.chk_writetodb.AutoSize = true;
            this.chk_writetodb.Checked = true;
            this.chk_writetodb.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_writetodb.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chk_writetodb.Location = new System.Drawing.Point(284, 30);
            this.chk_writetodb.Name = "chk_writetodb";
            this.chk_writetodb.Size = new System.Drawing.Size(109, 25);
            this.chk_writetodb.TabIndex = 29;
            this.chk_writetodb.Text = "写入数据库";
            this.chk_writetodb.UseVisualStyleBackColor = true;
            // 
            // checkBoxExplosueControl
            // 
            this.checkBoxExplosueControl.AutoSize = true;
            this.checkBoxExplosueControl.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.checkBoxExplosueControl.Location = new System.Drawing.Point(284, 3);
            this.checkBoxExplosueControl.Name = "checkBoxExplosueControl";
            this.checkBoxExplosueControl.Size = new System.Drawing.Size(93, 25);
            this.checkBoxExplosueControl.TabIndex = 28;
            this.checkBoxExplosueControl.Text = "曝光控制";
            this.checkBoxExplosueControl.UseVisualStyleBackColor = true;
            this.checkBoxExplosueControl.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // chk_executecmd
            // 
            this.chk_executecmd.AutoSize = true;
            this.chk_executecmd.Checked = true;
            this.chk_executecmd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_executecmd.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chk_executecmd.Location = new System.Drawing.Point(142, 30);
            this.chk_executecmd.Name = "chk_executecmd";
            this.chk_executecmd.Size = new System.Drawing.Size(93, 25);
            this.chk_executecmd.TabIndex = 27;
            this.chk_executecmd.Text = "动作控制";
            this.chk_executecmd.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(1166, 720);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(81, 27);
            this.button4.TabIndex = 31;
            this.button4.Text = "统计数据库";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Visible = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // StatusTimer
            // 
            this.StatusTimer.Interval = 1000;
            this.StatusTimer.Tick += new System.EventHandler(this.StatusTimer_Tick);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(1312, 347);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(26, 21);
            this.label5.TabIndex = 40;
            this.label5.Text = "亮";
            // 
            // trackBar1
            // 
            this.trackBar1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.trackBar1.Enabled = false;
            this.trackBar1.LargeChange = 4;
            this.trackBar1.Location = new System.Drawing.Point(1032, 337);
            this.trackBar1.Maximum = 3;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.trackBar1.Size = new System.Drawing.Size(281, 45);
            this.trackBar1.TabIndex = 42;
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll_1);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(929, 347);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(106, 21);
            this.label7.TabIndex = 43;
            this.label7.Text = "亮度控制：暗";
            // 
            // btn_changeCloth
            // 
            this.btn_changeCloth.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.btn_changeCloth.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_changeCloth.Location = new System.Drawing.Point(929, 599);
            this.btn_changeCloth.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_changeCloth.Name = "btn_changeCloth";
            this.btn_changeCloth.Size = new System.Drawing.Size(125, 55);
            this.btn_changeCloth.TabIndex = 45;
            this.btn_changeCloth.Text = "更换布匹";
            this.btn_changeCloth.UseVisualStyleBackColor = false;
            this.btn_changeCloth.Click += new System.EventHandler(this.button7_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelSourceName);
            this.groupBox1.Controls.Add(this.labelTotalVolumnNumber);
            this.groupBox1.Controls.Add(this.label22);
            this.groupBox1.Controls.Add(this.labelDetectName);
            this.groupBox1.Controls.Add(this.label20);
            this.groupBox1.Controls.Add(this.labelActualMeters);
            this.groupBox1.Controls.Add(this.label24);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.labelCylinderNumber);
            this.groupBox1.Controls.Add(this.label17);
            this.groupBox1.Controls.Add(this.labelVolumnNumber);
            this.groupBox1.Controls.Add(this.labelClassNumber);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.labelBatchNumber);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.labelClothNumber);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(933, 388);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(405, 141);
            this.groupBox1.TabIndex = 46;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "布匹详情";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // labelSourceName
            // 
            this.labelSourceName.AutoSize = true;
            this.labelSourceName.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelSourceName.Location = new System.Drawing.Point(218, 104);
            this.labelSourceName.Name = "labelSourceName";
            this.labelSourceName.Size = new System.Drawing.Size(23, 20);
            this.labelSourceName.TabIndex = 35;
            this.labelSourceName.Text = "空";
            // 
            // labelTotalVolumnNumber
            // 
            this.labelTotalVolumnNumber.AutoSize = true;
            this.labelTotalVolumnNumber.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelTotalVolumnNumber.Location = new System.Drawing.Point(347, 71);
            this.labelTotalVolumnNumber.Name = "labelTotalVolumnNumber";
            this.labelTotalVolumnNumber.Size = new System.Drawing.Size(23, 20);
            this.labelTotalVolumnNumber.TabIndex = 34;
            this.labelTotalVolumnNumber.Text = "空";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label22.Location = new System.Drawing.Point(286, 71);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(65, 20);
            this.label22.TabIndex = 33;
            this.label22.Text = "总卷数：";
            // 
            // labelDetectName
            // 
            this.labelDetectName.AutoSize = true;
            this.labelDetectName.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelDetectName.Location = new System.Drawing.Point(347, 104);
            this.labelDetectName.Name = "labelDetectName";
            this.labelDetectName.Size = new System.Drawing.Size(23, 20);
            this.labelDetectName.TabIndex = 32;
            this.labelDetectName.Text = "空";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label20.Location = new System.Drawing.Point(286, 104);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(65, 20);
            this.label20.TabIndex = 31;
            this.label20.Text = "检测人：";
            // 
            // labelActualMeters
            // 
            this.labelActualMeters.AutoSize = true;
            this.labelActualMeters.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelActualMeters.Location = new System.Drawing.Point(77, 104);
            this.labelActualMeters.Name = "labelActualMeters";
            this.labelActualMeters.Size = new System.Drawing.Size(23, 20);
            this.labelActualMeters.TabIndex = 28;
            this.labelActualMeters.Text = "空";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label24.Location = new System.Drawing.Point(148, 104);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(79, 20);
            this.label24.TabIndex = 27;
            this.label24.Text = "货号来源：";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label13.Location = new System.Drawing.Point(5, 104);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(79, 20);
            this.label13.TabIndex = 25;
            this.label13.Text = "标注米数：";
            // 
            // labelCylinderNumber
            // 
            this.labelCylinderNumber.AutoSize = true;
            this.labelCylinderNumber.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelCylinderNumber.Location = new System.Drawing.Point(347, 35);
            this.labelCylinderNumber.Name = "labelCylinderNumber";
            this.labelCylinderNumber.Size = new System.Drawing.Size(23, 20);
            this.labelCylinderNumber.TabIndex = 24;
            this.labelCylinderNumber.Text = "空";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label17.Location = new System.Drawing.Point(287, 35);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(65, 20);
            this.label17.TabIndex = 23;
            this.label17.Text = "染缸号：";
            // 
            // labelVolumnNumber
            // 
            this.labelVolumnNumber.AutoSize = true;
            this.labelVolumnNumber.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelVolumnNumber.Location = new System.Drawing.Point(218, 69);
            this.labelVolumnNumber.Name = "labelVolumnNumber";
            this.labelVolumnNumber.Size = new System.Drawing.Size(23, 20);
            this.labelVolumnNumber.TabIndex = 22;
            this.labelVolumnNumber.Text = "空";
            // 
            // labelClassNumber
            // 
            this.labelClassNumber.AutoSize = true;
            this.labelClassNumber.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelClassNumber.Location = new System.Drawing.Point(218, 35);
            this.labelClassNumber.Name = "labelClassNumber";
            this.labelClassNumber.Size = new System.Drawing.Size(23, 20);
            this.labelClassNumber.TabIndex = 20;
            this.labelClassNumber.Text = "空";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label14.Location = new System.Drawing.Point(147, 69);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(79, 20);
            this.label14.TabIndex = 18;
            this.label14.Text = "当前卷数：";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label15.Location = new System.Drawing.Point(147, 35);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(79, 20);
            this.label15.TabIndex = 16;
            this.label15.Text = "布匹类别：";
            // 
            // labelBatchNumber
            // 
            this.labelBatchNumber.AutoSize = true;
            this.labelBatchNumber.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelBatchNumber.Location = new System.Drawing.Point(77, 69);
            this.labelBatchNumber.Name = "labelBatchNumber";
            this.labelBatchNumber.Size = new System.Drawing.Size(23, 20);
            this.labelBatchNumber.TabIndex = 3;
            this.labelBatchNumber.Text = "空";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.Location = new System.Drawing.Point(6, 69);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(79, 20);
            this.label9.TabIndex = 2;
            this.label9.Text = "订单批号：";
            // 
            // labelClothNumber
            // 
            this.labelClothNumber.AutoSize = true;
            this.labelClothNumber.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelClothNumber.Location = new System.Drawing.Point(77, 35);
            this.labelClothNumber.Name = "labelClothNumber";
            this.labelClothNumber.Size = new System.Drawing.Size(23, 20);
            this.labelClothNumber.TabIndex = 1;
            this.labelClothNumber.Text = "空";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(5, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 20);
            this.label4.TabIndex = 0;
            this.label4.Text = "检测编号：";
            // 
            // btn_CurrentReport
            // 
            this.btn_CurrentReport.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.btn_CurrentReport.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_CurrentReport.Location = new System.Drawing.Point(1071, 662);
            this.btn_CurrentReport.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_CurrentReport.Name = "btn_CurrentReport";
            this.btn_CurrentReport.Size = new System.Drawing.Size(125, 55);
            this.btn_CurrentReport.TabIndex = 47;
            this.btn_CurrentReport.Text = "查看报告";
            this.btn_CurrentReport.UseVisualStyleBackColor = false;
            this.btn_CurrentReport.Click += new System.EventHandler(this.button8_Click);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.BackColor = System.Drawing.Color.Gray;
            this.label18.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label18.ForeColor = System.Drawing.Color.Lime;
            this.label18.Location = new System.Drawing.Point(323, 225);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(240, 25);
            this.label18.TabIndex = 48;
            this.label18.Text = "相机正在初始化，请稍候！";
            // 
            // btn_help
            // 
            this.btn_help.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.btn_help.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_help.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_help.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_help.Location = new System.Drawing.Point(1213, 602);
            this.btn_help.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_help.Name = "btn_help";
            this.btn_help.Size = new System.Drawing.Size(125, 58);
            this.btn_help.TabIndex = 50;
            this.btn_help.Text = "帮助文档";
            this.btn_help.UseVisualStyleBackColor = false;
            this.btn_help.Click += new System.EventHandler(this.button6_Click_2);
            // 
            // hWindowControl2
            // 
            this.hWindowControl2.BackColor = System.Drawing.Color.Black;
            this.hWindowControl2.BorderColor = System.Drawing.Color.Black;
            this.hWindowControl2.ImagePart = new System.Drawing.Rectangle(0, 0, 8192, 4908);
            this.hWindowControl2.Location = new System.Drawing.Point(12, 12);
            this.hWindowControl2.Name = "hWindowControl2";
            this.hWindowControl2.Size = new System.Drawing.Size(900, 516);
            this.hWindowControl2.TabIndex = 52;
            this.hWindowControl2.WindowSize = new System.Drawing.Size(900, 516);
            this.hWindowControl2.HMouseMove += new HalconDotNet.HMouseEventHandler(this.hWindowControl2_HMouseMove);
            // 
            // chk_contiouswhenoutline
            // 
            this.chk_contiouswhenoutline.AutoSize = true;
            this.chk_contiouswhenoutline.Location = new System.Drawing.Point(1213, 536);
            this.chk_contiouswhenoutline.Name = "chk_contiouswhenoutline";
            this.chk_contiouswhenoutline.Size = new System.Drawing.Size(51, 21);
            this.chk_contiouswhenoutline.TabIndex = 53;
            this.chk_contiouswhenoutline.Text = "连续";
            this.chk_contiouswhenoutline.UseVisualStyleBackColor = true;
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.folderBrowserDialog1.SelectedPath = "D:\\#RIZT#";
            // 
            // chk_outlinebythread
            // 
            this.chk_outlinebythread.AutoSize = true;
            this.chk_outlinebythread.Location = new System.Drawing.Point(1284, 536);
            this.chk_outlinebythread.Name = "chk_outlinebythread";
            this.chk_outlinebythread.Size = new System.Drawing.Size(51, 21);
            this.chk_outlinebythread.TabIndex = 54;
            this.chk_outlinebythread.Text = "线程";
            this.chk_outlinebythread.UseVisualStyleBackColor = true;
            // 
            // MultiBoardSyncGrabDemoDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Silver;
            this.ClientSize = new System.Drawing.Size(1346, 747);
            this.ControlBox = false;
            this.Controls.Add(this.chk_outlinebythread);
            this.Controls.Add(this.chk_contiouswhenoutline);
            this.Controls.Add(this.hWindowControl2);
            this.Controls.Add(this.btn_help);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.btn_CurrentReport);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btn_changeCloth);
            this.Controls.Add(this.btn_writetodb);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.btn_Setting);
            this.Controls.Add(this.button_Snap);
            this.Controls.Add(this.btn_outlineclick);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.btn_Freeze);
            this.Controls.Add(this.btn_HistoricalReport);
            this.Controls.Add(this.btn_Grab);
            this.Controls.Add(this.btn_Exit);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.toolStrip1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1920, 1080);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "MultiBoardSyncGrabDemoDlg";
            this.Text = "浙江大学台州研究院";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MultiBoardSyncGrabDemoDlg_FormClosed);
            this.Load += new System.EventHandler(this.MultiBoardSyncGrabDemoDlg_Load);
            this.Shown += new System.EventHandler(this.MultiBoardSyncGrabDemoDlg_Shown);
            this.SizeChanged += new System.EventHandler(this.MultiBoardSyncGrabDemoDlg_SizeChanged);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel StatusLabelInfo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel PixelDataValue;
        private System.Windows.Forms.ToolStripLabel StatusLabelInfoTrash;
        private System.Windows.Forms.Button btn_Exit;
        private System.Windows.Forms.Button btn_Freeze;
        private System.Windows.Forms.Button btn_Grab;
        private System.Windows.Forms.Button button_Snap;

        private SapAcquisition[]        m_Acquisition;
        private SapBufferRoi[]          m_Buffers;
        private  SapBuffer               m_Buffer;
        private SapAcqDevice              m_AcqD;
        private SapAcqToBuf[]           m_Xfer;
      //  private SapView                 m_View;
        private bool m_IsSignalDetected;
        private bool m_online;
        private SapLocation m_ServerLocation;
        private string m_ConfigFileName;
        private System.Drawing.Bitmap TestImage;


       //index for "about this.." item im system menu
        private const int m_AboutID = 0x100;
       private System.Windows.Forms.Button btn_HistoricalReport;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Button btn_outlineclick;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button btn_Setting;
        private System.Windows.Forms.CheckBox chk_saveimg;
        private System.Windows.Forms.CheckBox chk_standlab;
        private System.Windows.Forms.CheckBox Chk_Algorithms;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridViewTextBoxColumn 检测米数;
        private System.Windows.Forms.DataGridViewTextBoxColumn 检测时间;
        private System.Windows.Forms.DataGridViewTextBoxColumn 缺陷结果;
        private System.Windows.Forms.DataGridViewTextBoxColumn 色差均值;
        private System.Windows.Forms.Button btn_writetodb;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.CheckBox chk_executecmd;
       // private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.CheckBox checkBoxExplosueControl;
        private System.Windows.Forms.CheckBox chk_writetodb;
       // private System.IO.Ports.SerialPort serialPort2;
        //private System.IO.Ports.SerialPort serialPort3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.Timer StatusTimer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripLabel toolStripLabel4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btn_changeCloth;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelBatchNumber;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label labelClothNumber;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelVolumnNumber;
        private System.Windows.Forms.Label labelClassNumber;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label labelCylinderNumber;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.ToolStripLabel toolStripLabel5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.Button btn_CurrentReport;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label labelTotalVolumnNumber;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label labelDetectName;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label labelActualMeters;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label labelSourceName;
        private System.Windows.Forms.Button btn_help;
        private System.Windows.Forms.ToolStripLabel StatusLabelInfo1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripLabel toolStripLabel6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private HalconDotNet.HWindowControl hWindowControl2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel7;
        private System.Windows.Forms.CheckBox chk_contiouswhenoutline;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.CheckBox chk_outlinebythread;
        private System.Windows.Forms.Button button1;
    }
}

