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
            cbDirection = new ComboBox();
            label6 = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            btnEdit = new Button();
            btnSave = new Button();
            btnPlay = new Button();
            tbSpot = new TextBox();
            label7 = new Label();
            dgvPayments = new DataGridView();
            label8 = new Label();
            label9 = new Label();
            cbStatus = new ComboBox();
            lbDebt = new Label();
            label10 = new Label();
            cbTariff = new ComboBox();
            btnAddStatus = new Button();
            btnAddTariff = new Button();
            cbOwnerSurname = new ComboBox();
            btnAddOwner = new Button();
            label14 = new Label();
            label15 = new Label();
            label16 = new Label();
            label17 = new Label();
            tbYear = new TextBox();
            tbColor = new TextBox();
            tbModel = new TextBox();
            tbBrand = new TextBox();
            label18 = new Label();
            tbPhone = new TextBox();
            label13 = new Label();
            ((System.ComponentModel.ISupportInitialize)pbPhoto).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPayments).BeginInit();
            SuspendLayout();
            // 
            // pbPhoto
            // 
            pbPhoto.BorderStyle = BorderStyle.FixedSingle;
            pbPhoto.Location = new Point(10, 12);
            pbPhoto.Name = "pbPhoto";
            pbPhoto.Size = new Size(1002, 560);
            pbPhoto.SizeMode = PictureBoxSizeMode.Zoom;
            pbPhoto.TabIndex = 0;
            pbPhoto.TabStop = false;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(1018, 12);
            dataGridView1.MultiSelect = false;
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(367, 319);
            dataGridView1.TabIndex = 1;
            // 
            // lblState
            // 
            lblState.BackColor = Color.FromArgb(0, 192, 0);
            lblState.Location = new Point(1018, 334);
            lblState.Name = "lblState";
            lblState.Size = new Size(367, 43);
            lblState.TabIndex = 2;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label1.ForeColor = Color.Brown;
            label1.Location = new Point(128, 9);
            label1.Name = "label1";
            label1.Size = new Size(52, 20);
            label1.TabIndex = 3;
            label1.Text = "Дата";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label2.ForeColor = Color.Brown;
            label2.Location = new Point(189, 604);
            label2.Name = "label2";
            label2.Size = new Size(136, 20);
            label2.TabIndex = 4;
            label2.Text = "Время стоянки";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label3.ForeColor = Color.Brown;
            label3.Location = new Point(12, 604);
            label3.Name = "label3";
            label3.Size = new Size(62, 20);
            label3.TabIndex = 5;
            label3.Text = "Место";
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Right;
            label4.AutoSize = true;
            label4.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label4.ForeColor = Color.Brown;
            label4.Location = new Point(92, 123);
            label4.Name = "label4";
            label4.Size = new Size(88, 20);
            label4.TabIndex = 6;
            label4.Text = "Точность";
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Right;
            label5.AutoSize = true;
            label5.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label5.ForeColor = Color.Brown;
            label5.Location = new Point(116, 47);
            label5.Name = "label5";
            label5.Size = new Size(64, 20);
            label5.TabIndex = 7;
            label5.Text = "Номер";
            // 
            // cbPlate
            // 
            cbPlate.Anchor = AnchorStyles.Left;
            cbPlate.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            cbPlate.FormattingEnabled = true;
            cbPlate.Location = new Point(186, 41);
            cbPlate.Name = "cbPlate";
            cbPlate.Size = new Size(129, 33);
            cbPlate.TabIndex = 8;
            // 
            // lbDate
            // 
            lbDate.Anchor = AnchorStyles.Left;
            lbDate.AutoSize = true;
            lbDate.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lbDate.Location = new Point(186, 6);
            lbDate.Name = "lbDate";
            lbDate.Size = new Size(17, 25);
            lbDate.TabIndex = 9;
            lbDate.Text = " ";
            // 
            // lbConfidence
            // 
            lbConfidence.Anchor = AnchorStyles.Left;
            lbConfidence.AutoSize = true;
            lbConfidence.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lbConfidence.Location = new Point(186, 120);
            lbConfidence.Name = "lbConfidence";
            lbConfidence.Size = new Size(17, 25);
            lbConfidence.TabIndex = 10;
            lbConfidence.Text = " ";
            // 
            // cbDirection
            // 
            cbDirection.Anchor = AnchorStyles.Left;
            cbDirection.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            cbDirection.FormattingEnabled = true;
            cbDirection.Location = new Point(186, 79);
            cbDirection.Name = "cbDirection";
            cbDirection.Size = new Size(129, 33);
            cbDirection.TabIndex = 12;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Right;
            label6.AutoSize = true;
            label6.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label6.ForeColor = Color.Brown;
            label6.Location = new Point(57, 85);
            label6.Name = "label6";
            label6.Size = new Size(123, 20);
            label6.TabIndex = 11;
            label6.Text = "Направление";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.None;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(btnEdit, 0, 4);
            tableLayoutPanel1.Controls.Add(btnSave, 1, 4);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label4, 0, 3);
            tableLayoutPanel1.Controls.Add(lbConfidence, 1, 3);
            tableLayoutPanel1.Controls.Add(label6, 0, 2);
            tableLayoutPanel1.Controls.Add(cbDirection, 1, 2);
            tableLayoutPanel1.Controls.Add(lbDate, 1, 0);
            tableLayoutPanel1.Controls.Add(label5, 0, 1);
            tableLayoutPanel1.Controls.Add(cbPlate, 1, 1);
            tableLayoutPanel1.Location = new Point(1018, 381);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(367, 191);
            tableLayoutPanel1.TabIndex = 13;
            // 
            // btnEdit
            // 
            btnEdit.Anchor = AnchorStyles.None;
            btnEdit.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            btnEdit.Location = new Point(26, 155);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(131, 33);
            btnEdit.TabIndex = 16;
            btnEdit.Text = "Изменить";
            btnEdit.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.None;
            btnSave.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            btnSave.Location = new Point(213, 155);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(124, 33);
            btnSave.TabIndex = 17;
            btnSave.Text = "Сохранить";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // btnPlay
            // 
            btnPlay.Anchor = AnchorStyles.None;
            btnPlay.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            btnPlay.Location = new Point(888, 702);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(114, 42);
            btnPlay.TabIndex = 13;
            btnPlay.Text = "Оплата";
            btnPlay.UseVisualStyleBackColor = true;
            // 
            // tbSpot
            // 
            tbSpot.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tbSpot.Location = new Point(80, 597);
            tbSpot.Name = "tbSpot";
            tbSpot.ReadOnly = true;
            tbSpot.Size = new Size(52, 33);
            tbSpot.TabIndex = 14;
            tbSpot.Text = " ";
            tbSpot.TextAlign = HorizontalAlignment.Center;
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Left;
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label7.Location = new Point(331, 604);
            label7.Name = "label7";
            label7.Size = new Size(17, 25);
            label7.TabIndex = 14;
            label7.Text = " ";
            // 
            // dgvPayments
            // 
            dgvPayments.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPayments.Location = new Point(1018, 575);
            dgvPayments.Name = "dgvPayments";
            dgvPayments.Size = new Size(367, 169);
            dgvPayments.TabIndex = 15;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label8.ForeColor = Color.Brown;
            label8.Location = new Point(755, 603);
            label8.Name = "label8";
            label8.Size = new Size(68, 20);
            label8.TabIndex = 16;
            label8.Text = "Статус";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label9.ForeColor = Color.Brown;
            label9.Location = new Point(532, 604);
            label9.Name = "label9";
            label9.Size = new Size(51, 20);
            label9.TabIndex = 17;
            label9.Text = "Долг";
            // 
            // cbStatus
            // 
            cbStatus.Anchor = AnchorStyles.Left;
            cbStatus.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            cbStatus.FormattingEnabled = true;
            cbStatus.Location = new Point(830, 595);
            cbStatus.Name = "cbStatus";
            cbStatus.Size = new Size(143, 33);
            cbStatus.TabIndex = 18;
            // 
            // lbDebt
            // 
            lbDebt.Anchor = AnchorStyles.Left;
            lbDebt.AutoSize = true;
            lbDebt.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lbDebt.Location = new Point(589, 600);
            lbDebt.Name = "lbDebt";
            lbDebt.Size = new Size(23, 25);
            lbDebt.TabIndex = 19;
            lbDebt.Text = "0";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label10.ForeColor = Color.Brown;
            label10.Location = new Point(758, 659);
            label10.Name = "label10";
            label10.Size = new Size(65, 20);
            label10.TabIndex = 20;
            label10.Text = "Тариф";
            // 
            // cbTariff
            // 
            cbTariff.Anchor = AnchorStyles.Left;
            cbTariff.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            cbTariff.FormattingEnabled = true;
            cbTariff.Location = new Point(830, 652);
            cbTariff.Name = "cbTariff";
            cbTariff.Size = new Size(143, 33);
            cbTariff.TabIndex = 24;
            // 
            // btnAddStatus
            // 
            btnAddStatus.Anchor = AnchorStyles.None;
            btnAddStatus.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            btnAddStatus.Location = new Point(979, 596);
            btnAddStatus.Name = "btnAddStatus";
            btnAddStatus.Size = new Size(33, 33);
            btnAddStatus.TabIndex = 18;
            btnAddStatus.Text = "+";
            btnAddStatus.UseVisualStyleBackColor = true;
            btnAddStatus.Visible = false;
            // 
            // btnAddTariff
            // 
            btnAddTariff.Anchor = AnchorStyles.None;
            btnAddTariff.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            btnAddTariff.Location = new Point(979, 652);
            btnAddTariff.Name = "btnAddTariff";
            btnAddTariff.Size = new Size(33, 33);
            btnAddTariff.TabIndex = 28;
            btnAddTariff.Text = "+";
            btnAddTariff.UseVisualStyleBackColor = true;
            btnAddTariff.Visible = false;
            // 
            // cbOwnerSurname
            // 
            cbOwnerSurname.Anchor = AnchorStyles.Left;
            cbOwnerSurname.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            cbOwnerSurname.FormattingEnabled = true;
            cbOwnerSurname.Location = new Point(106, 703);
            cbOwnerSurname.Name = "cbOwnerSurname";
            cbOwnerSurname.Size = new Size(450, 32);
            cbOwnerSurname.TabIndex = 29;
            // 
            // btnAddOwner
            // 
            btnAddOwner.Anchor = AnchorStyles.None;
            btnAddOwner.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            btnAddOwner.Location = new Point(561, 703);
            btnAddOwner.Name = "btnAddOwner";
            btnAddOwner.Size = new Size(33, 33);
            btnAddOwner.TabIndex = 30;
            btnAddOwner.Text = "+";
            btnAddOwner.UseVisualStyleBackColor = true;
            btnAddOwner.Visible = false;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label14.ForeColor = Color.Brown;
            label14.Location = new Point(624, 660);
            label14.Name = "label14";
            label14.Size = new Size(41, 20);
            label14.TabIndex = 31;
            label14.Text = "Год";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label15.ForeColor = Color.Brown;
            label15.Location = new Point(447, 659);
            label15.Name = "label15";
            label15.Size = new Size(52, 20);
            label15.TabIndex = 32;
            label15.Text = "Цвет";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label16.ForeColor = Color.Brown;
            label16.Location = new Point(231, 660);
            label16.Name = "label16";
            label16.Size = new Size(76, 20);
            label16.TabIndex = 33;
            label16.Text = "Модель";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label17.ForeColor = Color.Brown;
            label17.Location = new Point(11, 660);
            label17.Name = "label17";
            label17.Size = new Size(62, 20);
            label17.TabIndex = 34;
            label17.Text = "Марка";
            // 
            // tbYear
            // 
            tbYear.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tbYear.Location = new Point(668, 652);
            tbYear.Name = "tbYear";
            tbYear.ReadOnly = true;
            tbYear.Size = new Size(59, 33);
            tbYear.TabIndex = 35;
            // 
            // tbColor
            // 
            tbColor.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tbColor.Location = new Point(501, 652);
            tbColor.Name = "tbColor";
            tbColor.ReadOnly = true;
            tbColor.Size = new Size(113, 33);
            tbColor.TabIndex = 36;
            // 
            // tbModel
            // 
            tbModel.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tbModel.Location = new Point(307, 652);
            tbModel.Name = "tbModel";
            tbModel.ReadOnly = true;
            tbModel.Size = new Size(134, 33);
            tbModel.TabIndex = 37;
            // 
            // tbBrand
            // 
            tbBrand.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tbBrand.Location = new Point(74, 653);
            tbBrand.Name = "tbBrand";
            tbBrand.ReadOnly = true;
            tbBrand.Size = new Size(151, 33);
            tbBrand.TabIndex = 38;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label18.ForeColor = Color.Brown;
            label18.Location = new Point(613, 711);
            label18.Name = "label18";
            label18.Size = new Size(86, 20);
            label18.TabIndex = 39;
            label18.Text = "Телефон";
            // 
            // tbPhone
            // 
            tbPhone.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tbPhone.Location = new Point(705, 704);
            tbPhone.Name = "tbPhone";
            tbPhone.ReadOnly = true;
            tbPhone.Size = new Size(151, 33);
            tbPhone.TabIndex = 40;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label13.ForeColor = Color.Brown;
            label13.Location = new Point(12, 711);
            label13.Name = "label13";
            label13.Size = new Size(88, 20);
            label13.TabIndex = 23;
            label13.Text = "Фамилия";
            // 
            // VehicleRegistrationForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1390, 777);
            Controls.Add(tbPhone);
            Controls.Add(label18);
            Controls.Add(tbBrand);
            Controls.Add(tbModel);
            Controls.Add(tbColor);
            Controls.Add(tbYear);
            Controls.Add(label17);
            Controls.Add(label16);
            Controls.Add(label15);
            Controls.Add(label14);
            Controls.Add(btnAddOwner);
            Controls.Add(cbOwnerSurname);
            Controls.Add(btnAddTariff);
            Controls.Add(btnAddStatus);
            Controls.Add(cbTariff);
            Controls.Add(label13);
            Controls.Add(label10);
            Controls.Add(lbDebt);
            Controls.Add(cbStatus);
            Controls.Add(label9);
            Controls.Add(label8);
            Controls.Add(dgvPayments);
            Controls.Add(label7);
            Controls.Add(tbSpot);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(lblState);
            Controls.Add(dataGridView1);
            Controls.Add(btnPlay);
            Controls.Add(pbPhoto);
            KeyPreview = true;
            Name = "VehicleRegistrationForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Регистрация авто";
            ((System.ComponentModel.ISupportInitialize)pbPhoto).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPayments).EndInit();
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
        private ComboBox cbDirection;
        private Label label6;
        private TableLayoutPanel tableLayoutPanel1;
        private Button btnPlay;
        private TextBox tbSpot;
        private Label label7;
        private Button btnEdit;
        private Button btnSave;
        private DataGridView dgvPayments;
        private Label label8;
        private Label label9;
        private ComboBox cbStatus;
        private Label lbDebt;
        private Label label10;
        private ComboBox cbTariff;
        private Button btnAddStatus;
        private Button btnAddTariff;
        private ComboBox cbOwnerSurname;
        private Button btnAddOwner;
        private Label label14;
        private Label label15;
        private Label label16;
        private Label label17;
        private TextBox tbYear;
        private TextBox tbColor;
        private TextBox tbModel;
        private TextBox tbBrand;
        private Label label18;
        private TextBox tbPhone;
        private Label label13;
    }
}