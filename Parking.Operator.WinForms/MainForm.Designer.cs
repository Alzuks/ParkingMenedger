namespace Parking.Operator.WinForms
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            MainTableLayer = new TableLayoutPanel();
            HeaderTableLayout = new TableLayoutPanel();
            lblShift = new Label();
            pnlCapacity = new Panel();
            lblCapacity = new Label();
            progressCapacity = new ProgressBar();
            DateShiftTableLayout = new TableLayoutPanel();
            lblData = new Label();
            lblTime = new Label();
            lblOperatorName = new Label();
            btnRefresh = new Button();
            txtSearch = new TextBox();
            pbLogo = new PictureBox();
            pbOperatorPhoto = new PictureBox();
            tlpLive = new TableLayoutPanel();
            carCardMain = new CarCardControl();
            tlpRight = new TableLayoutPanel();
            carCard1 = new CarCardControl();
            carCard2 = new CarCardControl();
            carCard3 = new CarCardControl();
            carCard4 = new CarCardControl();
            btnPublishClientPost = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            gridHistory = new DataGridView();
            carGrid = new PictureBox();
            timerRefresh = new System.Windows.Forms.Timer(components);
            ssStatus = new StatusStrip();
            ssServer = new ToolStripStatusLabel();
            ssLastUpdate = new ToolStripStatusLabel();
            MainTableLayer.SuspendLayout();
            HeaderTableLayout.SuspendLayout();
            pnlCapacity.SuspendLayout();
            DateShiftTableLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbLogo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbOperatorPhoto).BeginInit();
            tlpLive.SuspendLayout();
            tlpRight.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridHistory).BeginInit();
            ((System.ComponentModel.ISupportInitialize)carGrid).BeginInit();
            ssStatus.SuspendLayout();
            SuspendLayout();
            // 
            // MainTableLayer
            // 
            MainTableLayer.ColumnCount = 1;
            MainTableLayer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            MainTableLayer.Controls.Add(HeaderTableLayout, 0, 0);
            MainTableLayer.Controls.Add(tlpLive, 0, 1);
            MainTableLayer.Controls.Add(tableLayoutPanel1, 0, 2);
            MainTableLayer.Dock = DockStyle.Fill;
            MainTableLayer.Location = new Point(0, 0);
            MainTableLayer.Name = "MainTableLayer";
            MainTableLayer.RowCount = 3;
            MainTableLayer.RowStyles.Add(new RowStyle(SizeType.Absolute, 121F));
            MainTableLayer.RowStyles.Add(new RowStyle(SizeType.Percent, 63.8198776F));
            MainTableLayer.RowStyles.Add(new RowStyle(SizeType.Percent, 36.1801224F));
            MainTableLayer.Size = new Size(1525, 765);
            MainTableLayer.TabIndex = 0;
            // 
            // HeaderTableLayout
            // 
            HeaderTableLayout.ColumnCount = 8;
            HeaderTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
            HeaderTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 185F));
            HeaderTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            HeaderTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260F));
            HeaderTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 137F));
            HeaderTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 206F));
            HeaderTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
            HeaderTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 201F));
            HeaderTableLayout.Controls.Add(lblShift, 4, 0);
            HeaderTableLayout.Controls.Add(pnlCapacity, 3, 0);
            HeaderTableLayout.Controls.Add(DateShiftTableLayout, 5, 0);
            HeaderTableLayout.Controls.Add(lblOperatorName, 6, 0);
            HeaderTableLayout.Controls.Add(btnRefresh, 2, 0);
            HeaderTableLayout.Controls.Add(txtSearch, 1, 0);
            HeaderTableLayout.Controls.Add(pbLogo, 0, 0);
            HeaderTableLayout.Controls.Add(pbOperatorPhoto, 7, 0);
            HeaderTableLayout.Dock = DockStyle.Fill;
            HeaderTableLayout.Location = new Point(3, 3);
            HeaderTableLayout.Name = "HeaderTableLayout";
            HeaderTableLayout.RowCount = 1;
            HeaderTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            HeaderTableLayout.Size = new Size(1519, 115);
            HeaderTableLayout.TabIndex = 0;
            // 
            // lblShift
            // 
            lblShift.Anchor = AnchorStyles.None;
            lblShift.AutoSize = true;
            lblShift.Font = new Font("Tahoma", 36F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lblShift.Location = new Point(864, 28);
            lblShift.Name = "lblShift";
            lblShift.Size = new Size(39, 58);
            lblShift.TabIndex = 0;
            lblShift.Text = " ";
            lblShift.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlCapacity
            // 
            pnlCapacity.BackColor = Color.Transparent;
            pnlCapacity.Controls.Add(lblCapacity);
            pnlCapacity.Controls.Add(progressCapacity);
            pnlCapacity.Dock = DockStyle.Fill;
            pnlCapacity.Location = new Point(558, 3);
            pnlCapacity.Name = "pnlCapacity";
            pnlCapacity.Size = new Size(254, 109);
            pnlCapacity.TabIndex = 7;
            // 
            // lblCapacity
            // 
            lblCapacity.BackColor = SystemColors.ControlLight;
            lblCapacity.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lblCapacity.Location = new Point(58, 30);
            lblCapacity.Name = "lblCapacity";
            lblCapacity.Size = new Size(133, 38);
            lblCapacity.TabIndex = 2;
            lblCapacity.Text = "90/150";
            lblCapacity.TextAlign = ContentAlignment.MiddleCenter;
            lblCapacity.Click += lblCapacity_Click;
            // 
            // progressCapacity
            // 
            progressCapacity.Anchor = AnchorStyles.None;
            progressCapacity.Location = new Point(26, 20);
            progressCapacity.Name = "progressCapacity";
            progressCapacity.Size = new Size(196, 59);
            progressCapacity.TabIndex = 1;
            // 
            // DateShiftTableLayout
            // 
            DateShiftTableLayout.ColumnCount = 1;
            DateShiftTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            DateShiftTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            DateShiftTableLayout.Controls.Add(lblData, 0, 0);
            DateShiftTableLayout.Controls.Add(lblTime, 0, 1);
            DateShiftTableLayout.Dock = DockStyle.Fill;
            DateShiftTableLayout.Location = new Point(955, 3);
            DateShiftTableLayout.Name = "DateShiftTableLayout";
            DateShiftTableLayout.RowCount = 2;
            DateShiftTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            DateShiftTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            DateShiftTableLayout.Size = new Size(200, 109);
            DateShiftTableLayout.TabIndex = 2;
            // 
            // lblData
            // 
            lblData.Anchor = AnchorStyles.None;
            lblData.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lblData.Location = new Point(17, 4);
            lblData.Name = "lblData";
            lblData.Size = new Size(166, 34);
            lblData.TabIndex = 1;
            lblData.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTime
            // 
            lblTime.Anchor = AnchorStyles.None;
            lblTime.Font = new Font("Segoe UI", 26.2956524F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lblTime.Location = new Point(11, 51);
            lblTime.Name = "lblTime";
            lblTime.Size = new Size(177, 49);
            lblTime.TabIndex = 2;
            lblTime.Text = " ";
            lblTime.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblOperatorName
            // 
            lblOperatorName.Anchor = AnchorStyles.None;
            lblOperatorName.AutoSize = true;
            lblOperatorName.Font = new Font("Segoe UI", 18.1565228F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lblOperatorName.Location = new Point(1257, 40);
            lblOperatorName.Name = "lblOperatorName";
            lblOperatorName.Size = new Size(22, 35);
            lblOperatorName.TabIndex = 4;
            lblOperatorName.Text = " ";
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Left;
            btnRefresh.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            btnRefresh.Location = new Point(428, 32);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(81, 50);
            btnRefresh.TabIndex = 6;
            btnRefresh.Text = "Сброс";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.None;
            txtSearch.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 204);
            txtSearch.Location = new Point(246, 32);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(173, 50);
            txtSearch.TabIndex = 5;
            txtSearch.TextAlign = HorizontalAlignment.Right;
            // 
            // pbLogo
            // 
            pbLogo.Anchor = AnchorStyles.None;
            pbLogo.BackColor = Color.Transparent;
            pbLogo.BackgroundImageLayout = ImageLayout.Zoom;
            pbLogo.ErrorImage = null;
            pbLogo.Image = Properties.Resources.Logo2;
            pbLogo.Location = new Point(7, 12);
            pbLogo.Name = "pbLogo";
            pbLogo.Size = new Size(226, 91);
            pbLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pbLogo.TabIndex = 0;
            pbLogo.TabStop = false;
            // 
            // pbOperatorPhoto
            // 
            pbOperatorPhoto.Anchor = AnchorStyles.None;
            pbOperatorPhoto.Location = new Point(1420, 12);
            pbOperatorPhoto.Name = "pbOperatorPhoto";
            pbOperatorPhoto.Size = new Size(116, 91);
            pbOperatorPhoto.TabIndex = 3;
            pbOperatorPhoto.TabStop = false;
            // 
            // tlpLive
            // 
            tlpLive.ColumnCount = 3;
            tlpLive.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 719F));
            tlpLive.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 719F));
            tlpLive.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 236F));
            tlpLive.Controls.Add(carCardMain, 0, 0);
            tlpLive.Controls.Add(tlpRight, 1, 0);
            tlpLive.Controls.Add(btnPublishClientPost, 2, 0);
            tlpLive.Dock = DockStyle.Fill;
            tlpLive.Location = new Point(3, 124);
            tlpLive.Name = "tlpLive";
            tlpLive.RowCount = 1;
            tlpLive.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpLive.Size = new Size(1519, 405);
            tlpLive.TabIndex = 1;
            // 
            // carCardMain
            // 
            carCardMain.Dock = DockStyle.Fill;
            carCardMain.Location = new Point(3, 4);
            carCardMain.Margin = new Padding(3, 4, 3, 4);
            carCardMain.Name = "carCardMain";
            carCardMain.Size = new Size(713, 397);
            carCardMain.TabIndex = 0;
            // 
            // tlpRight
            // 
            tlpRight.ColumnCount = 2;
            tlpRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpRight.Controls.Add(carCard1, 0, 0);
            tlpRight.Controls.Add(carCard2, 1, 0);
            tlpRight.Controls.Add(carCard3, 0, 1);
            tlpRight.Controls.Add(carCard4, 1, 1);
            tlpRight.Dock = DockStyle.Fill;
            tlpRight.Location = new Point(722, 3);
            tlpRight.Name = "tlpRight";
            tlpRight.RowCount = 2;
            tlpRight.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpRight.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpRight.Size = new Size(713, 399);
            tlpRight.TabIndex = 1;
            // 
            // carCard1
            // 
            carCard1.Dock = DockStyle.Fill;
            carCard1.Location = new Point(3, 4);
            carCard1.Margin = new Padding(3, 4, 3, 4);
            carCard1.Name = "carCard1";
            carCard1.Size = new Size(350, 191);
            carCard1.TabIndex = 0;
            // 
            // carCard2
            // 
            carCard2.Dock = DockStyle.Fill;
            carCard2.Location = new Point(359, 4);
            carCard2.Margin = new Padding(3, 4, 3, 4);
            carCard2.Name = "carCard2";
            carCard2.Size = new Size(351, 191);
            carCard2.TabIndex = 1;
            // 
            // carCard3
            // 
            carCard3.Dock = DockStyle.Fill;
            carCard3.Location = new Point(3, 203);
            carCard3.Margin = new Padding(3, 4, 3, 4);
            carCard3.Name = "carCard3";
            carCard3.Size = new Size(350, 192);
            carCard3.TabIndex = 2;
            // 
            // carCard4
            // 
            carCard4.Dock = DockStyle.Fill;
            carCard4.Location = new Point(359, 203);
            carCard4.Margin = new Padding(3, 4, 3, 4);
            carCard4.Name = "carCard4";
            carCard4.Size = new Size(351, 192);
            carCard4.TabIndex = 3;
            // 
            // btnPublishClientPost
            // 
            btnPublishClientPost.Location = new Point(1441, 3);
            btnPublishClientPost.Name = "btnPublishClientPost";
            btnPublishClientPost.Size = new Size(75, 36);
            btnPublishClientPost.TabIndex = 2;
            btnPublishClientPost.Text = "Telega";
            btnPublishClientPost.UseVisualStyleBackColor = true;
            btnPublishClientPost.Visible = false;
            btnPublishClientPost.Click += btnPublishClientPost_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75.1152039F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24.8847923F));
            tableLayoutPanel1.Controls.Add(gridHistory, 0, 0);
            tableLayoutPanel1.Controls.Add(carGrid, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(3, 535);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(1519, 227);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // gridHistory
            // 
            gridHistory.AllowUserToAddRows = false;
            gridHistory.AllowUserToDeleteRows = false;
            gridHistory.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridHistory.Dock = DockStyle.Fill;
            gridHistory.Location = new Point(3, 3);
            gridHistory.Name = "gridHistory";
            gridHistory.ReadOnly = true;
            gridHistory.RowHeadersWidth = 49;
            gridHistory.ShowCellToolTips = false;
            gridHistory.Size = new Size(1134, 221);
            gridHistory.TabIndex = 2;
            // 
            // carGrid
            // 
            carGrid.BackColor = SystemColors.AppWorkspace;
            carGrid.Dock = DockStyle.Fill;
            carGrid.Location = new Point(1143, 3);
            carGrid.Name = "carGrid";
            carGrid.Size = new Size(373, 221);
            carGrid.TabIndex = 3;
            carGrid.TabStop = false;
            // 
            // timerRefresh
            // 
            timerRefresh.Interval = 5000;
            // 
            // ssStatus
            // 
            ssStatus.ImageScalingSize = new Size(19, 19);
            ssStatus.Items.AddRange(new ToolStripItem[] { ssServer, ssLastUpdate });
            ssStatus.Location = new Point(0, 743);
            ssStatus.Name = "ssStatus";
            ssStatus.Size = new Size(1525, 22);
            ssStatus.SizingGrip = false;
            ssStatus.TabIndex = 1;
            ssStatus.Text = "Сервер...";
            // 
            // ssServer
            // 
            ssServer.Name = "ssServer";
            ssServer.Size = new Size(118, 17);
            ssServer.Text = "toolStripStatusLabel1";
            // 
            // ssLastUpdate
            // 
            ssLastUpdate.Name = "ssLastUpdate";
            ssLastUpdate.Size = new Size(118, 17);
            ssLastUpdate.Text = "toolStripStatusLabel2";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1525, 765);
            Controls.Add(ssStatus);
            Controls.Add(MainTableLayer);
            Name = "MainForm";
            Text = "Form1";
            MainTableLayer.ResumeLayout(false);
            HeaderTableLayout.ResumeLayout(false);
            HeaderTableLayout.PerformLayout();
            pnlCapacity.ResumeLayout(false);
            DateShiftTableLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pbLogo).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbOperatorPhoto).EndInit();
            tlpLive.ResumeLayout(false);
            tlpRight.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridHistory).EndInit();
            ((System.ComponentModel.ISupportInitialize)carGrid).EndInit();
            ssStatus.ResumeLayout(false);
            ssStatus.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel MainTableLayer;
        private TableLayoutPanel HeaderTableLayout;
        private PictureBox pbLogo;
        private ProgressBar progressCapacity;
        private TableLayoutPanel DateShiftTableLayout;
        private Label lblShift;
        private Label lblData;
        private PictureBox pbOperatorPhoto;
        private Label lblOperatorName;
        private TableLayoutPanel tlpLive;
        private CarCardControl carCardMain;
        private TableLayoutPanel tlpRight;
        private CarCardControl carCard1;
        private CarCardControl carCard2;
        private CarCardControl carCard3;
        private CarCardControl carCard4;
        private TextBox txtSearch;
        private Button btnRefresh;
        private DataGridView gridHistory;
        private System.Windows.Forms.Timer timerRefresh;
        private StatusStrip ssStatus;
        private Panel pnlCapacity;
        private Label lblCapacity;
        private TableLayoutPanel tableLayoutPanel1;
        private PictureBox carGrid;
        private Label lblTime;
        private ToolStripStatusLabel ssServer;
        private ToolStripStatusLabel ssLastUpdate;
        private Button btnPublishClientPost;
    }
}
