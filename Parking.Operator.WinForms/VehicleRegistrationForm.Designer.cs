namespace Parking.Operator.WinForms
{
    partial class VehicleRegistrationForm
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
            pbPhoto = new PictureBox();
            dataGridView1 = new DataGridView();
            lblState = new Label();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            cbPlate = new ComboBox();
            lbDate = new Label();
            lbConfidence = new Label();
            comboBox1 = new ComboBox();
            label6 = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            btnPlay = new Button();
            ((System.ComponentModel.ISupportInitialize)pbPhoto).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // pbPhoto
            // 
            pbPhoto.BorderStyle = BorderStyle.FixedSingle;
            pbPhoto.Location = new Point(1, 2);
            pbPhoto.Name = "pbPhoto";
            pbPhoto.Size = new Size(1100, 660);
            pbPhoto.SizeMode = PictureBoxSizeMode.Zoom;
            pbPhoto.TabIndex = 0;
            pbPhoto.TabStop = false;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(1107, 2);
            dataGridView1.MultiSelect = false;
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(315, 336);
            dataGridView1.TabIndex = 1;
            // 
            // lblState
            // 
            lblState.BackColor = Color.FromArgb(0, 192, 0);
            lblState.Location = new Point(1107, 353);
            lblState.Name = "lblState";
            lblState.Size = new Size(315, 43);
            lblState.TabIndex = 2;
            lblState.Text = "label1";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold);
            label1.Location = new Point(104, 13);
            label1.Name = "label1";
            label1.Size = new Size(56, 25);
            label1.TabIndex = 3;
            label1.Text = "Дата";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label2.Location = new Point(132, 683);
            label2.Name = "label2";
            label2.Size = new Size(47, 21);
            label2.TabIndex = 4;
            label2.Text = "Дата";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label3.Location = new Point(12, 683);
            label3.Name = "label3";
            label3.Size = new Size(59, 21);
            label3.TabIndex = 5;
            label3.Text = "Место";
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Right;
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold);
            label4.Location = new Point(63, 169);
            label4.Name = "label4";
            label4.Size = new Size(97, 25);
            label4.TabIndex = 6;
            label4.Text = "Точность";
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Right;
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold);
            label5.Location = new Point(84, 65);
            label5.Name = "label5";
            label5.Size = new Size(76, 25);
            label5.TabIndex = 7;
            label5.Text = "Номер";
            // 
            // cbPlate
            // 
            cbPlate.Anchor = AnchorStyles.Left;
            cbPlate.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            cbPlate.FormattingEnabled = true;
            cbPlate.Location = new Point(166, 61);
            cbPlate.Name = "cbPlate";
            cbPlate.Size = new Size(129, 33);
            cbPlate.TabIndex = 8;
            // 
            // lbDate
            // 
            lbDate.Anchor = AnchorStyles.Left;
            lbDate.AutoSize = true;
            lbDate.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lbDate.Location = new Point(166, 13);
            lbDate.Name = "lbDate";
            lbDate.Size = new Size(65, 25);
            lbDate.TabIndex = 9;
            lbDate.Text = "label6";
            // 
            // lbConfidence
            // 
            lbConfidence.Anchor = AnchorStyles.Left;
            lbConfidence.AutoSize = true;
            lbConfidence.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lbConfidence.Location = new Point(166, 169);
            lbConfidence.Name = "lbConfidence";
            lbConfidence.Size = new Size(65, 25);
            lbConfidence.TabIndex = 10;
            lbConfidence.Text = "label7";
            // 
            // comboBox1
            // 
            comboBox1.Anchor = AnchorStyles.Left;
            comboBox1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(166, 113);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(129, 33);
            comboBox1.TabIndex = 12;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Right;
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold);
            label6.Location = new Point(23, 117);
            label6.Name = "label6";
            label6.Size = new Size(137, 25);
            label6.TabIndex = 11;
            label6.Text = "Направление";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.None;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label4, 0, 3);
            tableLayoutPanel1.Controls.Add(lbConfidence, 1, 3);
            tableLayoutPanel1.Controls.Add(label6, 0, 2);
            tableLayoutPanel1.Controls.Add(comboBox1, 1, 2);
            tableLayoutPanel1.Controls.Add(lbDate, 1, 0);
            tableLayoutPanel1.Controls.Add(cbPlate, 1, 1);
            tableLayoutPanel1.Controls.Add(label5, 0, 1);
            tableLayoutPanel1.Controls.Add(btnPlay, 1, 4);
            tableLayoutPanel1.Location = new Point(1107, 399);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.Size = new Size(327, 263);
            tableLayoutPanel1.TabIndex = 13;
            // 
            // btnPlay
            // 
            btnPlay.Anchor = AnchorStyles.None;
            btnPlay.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            btnPlay.Location = new Point(188, 214);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(114, 42);
            btnPlay.TabIndex = 13;
            btnPlay.Text = "Оплата";
            btnPlay.UseVisualStyleBackColor = true;
            // 
            // VehicleRegistrationForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1434, 861);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(lblState);
            Controls.Add(dataGridView1);
            Controls.Add(pbPhoto);
            KeyPreview = true;
            Name = "VehicleRegistrationForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Регистрация авто";
            ((System.ComponentModel.ISupportInitialize)pbPhoto).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pbPhoto;
        private DataGridView dataGridView1;
        private Label lblState;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private ComboBox cbPlate;
        private Label lbDate;
        private Label lbConfidence;
        private ComboBox comboBox1;
        private Label label6;
        private TableLayoutPanel tableLayoutPanel1;
        private Button btnPlay;
    }
}