namespace Parking.Operator.WinForms
{
    partial class AddStatus
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
            label3 = new Label();
            label4 = new Label();
            label6 = new Label();
            chbUnrestrict = new CheckBox();
            chbUnlimit = new CheckBox();
            tbStatus = new TextBox();
            tbCode = new TextBox();
            tbText = new TextBox();
            cdColor = new ColorDialog();
            btnColor = new Button();
            btnOk = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label3.ForeColor = Color.Brown;
            label3.Location = new Point(27, 48);
            label3.Name = "label3";
            label3.Size = new Size(98, 20);
            label3.TabIndex = 6;
            label3.Text = "Категория";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label4.ForeColor = Color.Brown;
            label4.Location = new Point(277, 48);
            label4.Name = "label4";
            label4.Size = new Size(121, 20);
            label4.TabIndex = 9;
            label4.Text = "Обозначение";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label6.ForeColor = Color.Brown;
            label6.Location = new Point(27, 138);
            label6.Name = "label6";
            label6.Size = new Size(57, 20);
            label6.TabIndex = 11;
            label6.Text = "Текст";
            // 
            // chbUnrestrict
            // 
            chbUnrestrict.AutoSize = true;
            chbUnrestrict.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            chbUnrestrict.ForeColor = Color.Brown;
            chbUnrestrict.Location = new Point(27, 90);
            chbUnrestrict.Name = "chbUnrestrict";
            chbUnrestrict.Size = new Size(131, 24);
            chbUnrestrict.TabIndex = 12;
            chbUnrestrict.Text = "Автопроезд";
            chbUnrestrict.UseVisualStyleBackColor = true;
            // 
            // chbUnlimit
            // 
            chbUnlimit.AutoSize = true;
            chbUnlimit.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            chbUnlimit.ForeColor = Color.Brown;
            chbUnlimit.Location = new Point(229, 92);
            chbUnlimit.Name = "chbUnlimit";
            chbUnlimit.Size = new Size(112, 24);
            chbUnlimit.TabIndex = 13;
            chbUnlimit.Text = "Безлимит";
            chbUnlimit.UseVisualStyleBackColor = true;
            // 
            // tbStatus
            // 
            tbStatus.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tbStatus.Location = new Point(131, 41);
            tbStatus.Name = "tbStatus";
            tbStatus.Size = new Size(126, 33);
            tbStatus.TabIndex = 15;
            tbStatus.Text = " ";
            tbStatus.TextAlign = HorizontalAlignment.Center;
            // 
            // tbCode
            // 
            tbCode.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tbCode.Location = new Point(404, 41);
            tbCode.Name = "tbCode";
            tbCode.Size = new Size(97, 33);
            tbCode.TabIndex = 16;
            tbCode.Text = " ";
            tbCode.TextAlign = HorizontalAlignment.Center;
            // 
            // tbText
            // 
            tbText.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tbText.Location = new Point(90, 131);
            tbText.Name = "tbText";
            tbText.Size = new Size(411, 33);
            tbText.TabIndex = 17;
            tbText.Text = " ";
            tbText.TextAlign = HorizontalAlignment.Center;
            // 
            // btnColor
            // 
            btnColor.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            btnColor.ForeColor = Color.Brown;
            btnColor.Location = new Point(422, 90);
            btnColor.Name = "btnColor";
            btnColor.Size = new Size(75, 26);
            btnColor.TabIndex = 18;
            btnColor.Text = "Цвет";
            btnColor.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            btnOk.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            btnOk.ForeColor = Color.Black;
            btnOk.Location = new Point(329, 190);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(99, 33);
            btnOk.TabIndex = 19;
            btnOk.Text = "Ок";
            btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            btnCancel.ForeColor = Color.Black;
            btnCancel.Location = new Point(103, 184);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(101, 39);
            btnCancel.TabIndex = 20;
            btnCancel.Text = "Отмена";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // AddStatus
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(537, 241);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(btnColor);
            Controls.Add(tbText);
            Controls.Add(tbCode);
            Controls.Add(tbStatus);
            Controls.Add(chbUnlimit);
            Controls.Add(chbUnrestrict);
            Controls.Add(label6);
            Controls.Add(label4);
            Controls.Add(label3);
            Name = "AddStatus";
            Text = "AddStatus";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label3;
        private Label label4;
        private Label label6;
        private CheckBox chbUnrestrict;
        private CheckBox chbUnlimit;
        private TextBox tbStatus;
        private TextBox tbCode;
        private TextBox tbText;
        private ColorDialog cdColor;
        private Button btnColor;
        private Button btnOk;
        private Button btnCancel;
    }
}