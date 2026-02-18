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
            label4 = new Label();
            payDate = new TextBox();
            payTime = new TextBox();
            payNumber = new NumericUpDown();
            payEmployees = new ComboBox();
            btnPay = new Button();
            label5 = new Label();
            placeNo = new TextBox();
            ((System.ComponentModel.ISupportInitialize)payNumber).BeginInit();
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
            label2.Location = new Point(127, 32);
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
            label3.Location = new Point(217, 32);
            label3.Name = "label3";
            label3.Size = new Size(113, 25);
            label3.TabIndex = 2;
            label3.Text = "Сотрудник";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label4.ForeColor = Color.Brown;
            label4.Location = new Point(341, 9);
            label4.Name = "label4";
            label4.Size = new Size(144, 50);
            label4.TabIndex = 3;
            label4.Text = "Количество\r\nдней/месяцев\r\n";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // payDate
            // 
            payDate.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            payDate.Location = new Point(25, 69);
            payDate.Name = "payDate";
            payDate.Size = new Size(80, 33);
            payDate.TabIndex = 4;
            // 
            // payTime
            // 
            payTime.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            payTime.Location = new Point(127, 69);
            payTime.Name = "payTime";
            payTime.Size = new Size(80, 33);
            payTime.TabIndex = 5;
            // 
            // payNumber
            // 
            payNumber.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            payNumber.Location = new Point(397, 69);
            payNumber.Name = "payNumber";
            payNumber.Size = new Size(37, 33);
            payNumber.TabIndex = 6;
            // 
            // payEmployees
            // 
            payEmployees.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            payEmployees.FormattingEnabled = true;
            payEmployees.Location = new Point(217, 69);
            payEmployees.Name = "payEmployees";
            payEmployees.Size = new Size(121, 33);
            payEmployees.TabIndex = 7;
            // 
            // btnPay
            // 
            btnPay.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            btnPay.Location = new Point(331, 135);
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
            label5.Location = new Point(480, 32);
            label5.Name = "label5";
            label5.Size = new Size(70, 25);
            label5.TabIndex = 9;
            label5.Text = "Место";
            // 
            // placeNo
            // 
            placeNo.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            placeNo.Location = new Point(489, 69);
            placeNo.Name = "placeNo";
            placeNo.Size = new Size(61, 33);
            placeNo.TabIndex = 10;
            // 
            // paymentForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(605, 185);
            Controls.Add(placeNo);
            Controls.Add(label5);
            Controls.Add(btnPay);
            Controls.Add(payEmployees);
            Controls.Add(payNumber);
            Controls.Add(payTime);
            Controls.Add(payDate);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "paymentForm";
            Text = "paymentForm";
            ((System.ComponentModel.ISupportInitialize)payNumber).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private TextBox payDate;
        private TextBox payTime;
        private NumericUpDown payNumber;
        private ComboBox payEmployees;
        private Button btnPay;
        private Label label5;
        private TextBox placeNo;
    }
}