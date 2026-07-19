namespace Parking.Operator.WinForms
{
    partial class paymentForm
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
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            lbDayMonth = new Label();
            payDate = new TextBox();
            payTime = new TextBox();
            payQuantity = new NumericUpDown();
            payEmployee = new ComboBox();
            btnPay = new Button();
            label5 = new Label();
            cbPlace = new ComboBox();
            label6 = new Label();
            tbSum = new TextBox();
            ((System.ComponentModel.ISupportInitialize)payQuantity).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label1.ForeColor = Color.Brown;
            label1.Location = new Point(37, 32);
            label1.Name = "label1";
            label1.Size = new Size(56, 25);
            label1.TabIndex = 0;
            label1.Text = "Дата";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label2.ForeColor = Color.Brown;
            label2.Location = new Point(148, 32);
            label2.Name = "label2";
            label2.Size = new Size(72, 25);
            label2.TabIndex = 1;
            label2.Text = "Время";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label3.ForeColor = Color.Brown;
            label3.Location = new Point(236, 32);
            label3.Name = "label3";
            label3.Size = new Size(113, 25);
            label3.TabIndex = 2;
            label3.Text = "Сотрудник";
            // 
            // lbDayMonth
            // 
            lbDayMonth.AutoSize = true;
            lbDayMonth.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lbDayMonth.ForeColor = Color.Brown;
            lbDayMonth.Location = new Point(439, 32);
            lbDayMonth.Name = "lbDayMonth";
            lbDayMonth.Size = new Size(17, 25);
            lbDayMonth.TabIndex = 3;
            lbDayMonth.Text = " ";
            lbDayMonth.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // payDate
            // 
            payDate.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            payDate.Location = new Point(25, 69);
            payDate.Name = "payDate";
            payDate.Size = new Size(117, 33);
            payDate.TabIndex = 4;
            // 
            // payTime
            // 
            payTime.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            payTime.Location = new Point(148, 69);
            payTime.Name = "payTime";
            payTime.Size = new Size(72, 33);
            payTime.TabIndex = 5;
            // 
            // payQuantity
            // 
            payQuantity.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            payQuantity.Location = new Point(439, 69);
            payQuantity.Name = "payQuantity";
            payQuantity.Size = new Size(48, 33);
            payQuantity.TabIndex = 6;
            // 
            // payEmployee
            // 
            payEmployee.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            payEmployee.FormattingEnabled = true;
            payEmployee.Location = new Point(226, 69);
            payEmployee.Name = "payEmployee";
            payEmployee.Size = new Size(207, 33);
            payEmployee.TabIndex = 7;
            // 
            // btnPay
            // 
            btnPay.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            btnPay.Location = new Point(366, 124);
            btnPay.Name = "btnPay";
            btnPay.Size = new Size(118, 38);
            btnPay.TabIndex = 8;
            btnPay.Text = "Оплатить";
            btnPay.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label5.ForeColor = Color.Brown;
            label5.Location = new Point(493, 32);
            label5.Name = "label5";
            label5.Size = new Size(70, 25);
            label5.TabIndex = 9;
            label5.Text = "Место";
            // 
            // cbPlace
            // 
            cbPlace.Anchor = AnchorStyles.Left;
            cbPlace.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            cbPlace.FormattingEnabled = true;
            cbPlace.Location = new Point(493, 69);
            cbPlace.Name = "cbPlace";
            cbPlace.Size = new Size(58, 33);
            cbPlace.TabIndex = 19;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label6.ForeColor = Color.Brown;
            label6.Location = new Point(112, 131);
            label6.Name = "label6";
            label6.Size = new Size(79, 25);
            label6.TabIndex = 20;
            label6.Text = "Сумма:";
            // 
            // tbSum
            // 
            tbSum.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tbSum.ForeColor = Color.Red;
            tbSum.Location = new Point(190, 128);
            tbSum.Name = "tbSum";
            tbSum.Size = new Size(88, 33);
            tbSum.TabIndex = 21;
            // 
            // paymentForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(580, 185);
            Controls.Add(tbSum);
            Controls.Add(label6);
            Controls.Add(cbPlace);
            Controls.Add(label5);
            Controls.Add(btnPay);
            Controls.Add(payEmployee);
            Controls.Add(payQuantity);
            Controls.Add(payTime);
            Controls.Add(payDate);
            Controls.Add(lbDayMonth);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "paymentForm";
            Text = "Оплата";
            ((System.ComponentModel.ISupportInitialize)payQuantity).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label lbDayMonth;
        private TextBox payDate;
        private TextBox payTime;
        private NumericUpDown payQuantity;
        private ComboBox payEmployee;
        private Button btnPay;
        private Label label5;
        private ComboBox cbPlace;
        private Label label6;
        private TextBox tbSum;
    }
}