namespace Parking.Operator.WinForms
{
    partial class LoginForm
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
            tbPass = new TextBox();
            label4 = new Label();
            label3 = new Label();
            cbRole = new ComboBox();
            btnOk = new Button();
            SuspendLayout();
            // 
            // tbPass
            // 
            tbPass.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tbPass.Location = new Point(91, 66);
            tbPass.Name = "tbPass";
            tbPass.Size = new Size(131, 33);
            tbPass.TabIndex = 20;
            tbPass.TextAlign = HorizontalAlignment.Center;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label4.ForeColor = Color.Brown;
            label4.Location = new Point(12, 73);
            label4.Name = "label4";
            label4.Size = new Size(73, 20);
            label4.TabIndex = 18;
            label4.Text = "Пароль";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label3.ForeColor = Color.Brown;
            label3.Location = new Point(46, 35);
            label3.Name = "label3";
            label3.Size = new Size(39, 20);
            label3.TabIndex = 17;
            label3.Text = "Тип";
            // 
            // cbRole
            // 
            cbRole.Anchor = AnchorStyles.Left;
            cbRole.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            cbRole.FormattingEnabled = true;
            cbRole.Location = new Point(91, 28);
            cbRole.Name = "cbRole";
            cbRole.Size = new Size(131, 33);
            cbRole.TabIndex = 28;
            // 
            // btnOk
            // 
            btnOk.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            btnOk.ForeColor = Color.Black;
            btnOk.Location = new Point(75, 116);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(99, 33);
            btnOk.TabIndex = 29;
            btnOk.Text = "Ок";
            btnOk.UseVisualStyleBackColor = true;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(247, 171);
            Controls.Add(btnOk);
            Controls.Add(cbRole);
            Controls.Add(tbPass);
            Controls.Add(label4);
            Controls.Add(label3);
            Name = "LoginForm";
            Text = "Login";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox tbPass;
        private Label label4;
        private Label label3;
        private ComboBox cbRole;
        private Button btnOk;
    }
}