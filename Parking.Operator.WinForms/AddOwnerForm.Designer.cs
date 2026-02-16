namespace Parking.Operator.WinForms
{
    partial class AddOwnerForm
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
            label13 = new Label();
            label12 = new Label();
            label11 = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            tbPhone = new TextBox();
            label2 = new Label();
            tbLastName = new TextBox();
            tbFirstName = new TextBox();
            tbSurname = new TextBox();
            btnOk = new Button();
            btnCancel = new Button();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label1.ForeColor = Color.Brown;
            label1.Location = new Point(107, 12);
            label1.Name = "label1";
            label1.Size = new Size(88, 20);
            label1.TabIndex = 23;
            label1.Text = "Фамилия";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label13.ForeColor = Color.Brown;
            label13.Location = new Point(-293, 83);
            label13.Name = "label13";
            label13.Size = new Size(88, 20);
            label13.TabIndex = 26;
            label13.Text = "Фамилия";
            // 
            // label12
            // 
            label12.Anchor = AnchorStyles.Right;
            label12.AutoSize = true;
            label12.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            label12.ForeColor = Color.Brown;
            label12.Location = new Point(152, 57);
            label12.Name = "label12";
            label12.Size = new Size(43, 20);
            label12.TabIndex = 25;
            label12.Text = "Имя";
            // 
            // label11
            // 
            label11.Anchor = AnchorStyles.Right;
            label11.AutoSize = true;
            label11.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label11.ForeColor = Color.Brown;
            label11.Location = new Point(104, 102);
            label11.Name = "label11";
            label11.Size = new Size(91, 20);
            label11.TabIndex = 24;
            label11.Text = "Отчество";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48.0582542F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 51.9417458F));
            tableLayoutPanel1.Controls.Add(btnCancel, 0, 4);
            tableLayoutPanel1.Controls.Add(tbPhone, 1, 3);
            tableLayoutPanel1.Controls.Add(label2, 0, 3);
            tableLayoutPanel1.Controls.Add(tbLastName, 1, 2);
            tableLayoutPanel1.Controls.Add(tbFirstName, 1, 1);
            tableLayoutPanel1.Controls.Add(tbSurname, 1, 0);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label12, 0, 1);
            tableLayoutPanel1.Controls.Add(label11, 0, 2);
            tableLayoutPanel1.Controls.Add(btnOk, 1, 4);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.Size = new Size(412, 228);
            tableLayoutPanel1.TabIndex = 27;
            // 
            // tbPhone
            // 
            tbPhone.Anchor = AnchorStyles.Left;
            tbPhone.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tbPhone.Location = new Point(201, 141);
            tbPhone.Name = "tbPhone";
            tbPhone.ReadOnly = true;
            tbPhone.Size = new Size(151, 33);
            tbPhone.TabIndex = 43;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label2.ForeColor = Color.Brown;
            label2.Location = new Point(109, 147);
            label2.Name = "label2";
            label2.Size = new Size(86, 20);
            label2.TabIndex = 42;
            label2.Text = "Телефон";
            // 
            // tbLastName
            // 
            tbLastName.Anchor = AnchorStyles.Left;
            tbLastName.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tbLastName.Location = new Point(201, 96);
            tbLastName.Name = "tbLastName";
            tbLastName.ReadOnly = true;
            tbLastName.Size = new Size(171, 33);
            tbLastName.TabIndex = 41;
            // 
            // tbFirstName
            // 
            tbFirstName.Anchor = AnchorStyles.Left;
            tbFirstName.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tbFirstName.Location = new Point(201, 51);
            tbFirstName.Name = "tbFirstName";
            tbFirstName.ReadOnly = true;
            tbFirstName.Size = new Size(171, 33);
            tbFirstName.TabIndex = 40;
            // 
            // tbSurname
            // 
            tbSurname.Anchor = AnchorStyles.Left;
            tbSurname.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tbSurname.Location = new Point(201, 6);
            tbSurname.Name = "tbSurname";
            tbSurname.ReadOnly = true;
            tbSurname.Size = new Size(171, 33);
            tbSurname.TabIndex = 39;
            // 
            // btnOk
            // 
            btnOk.Anchor = AnchorStyles.None;
            btnOk.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            btnOk.Location = new Point(259, 186);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(92, 35);
            btnOk.TabIndex = 44;
            btnOk.Text = "Ok";
            btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.None;
            btnCancel.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            btnCancel.Location = new Point(53, 186);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(92, 35);
            btnCancel.TabIndex = 45;
            btnCancel.Text = "Отмена";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // AddOwnerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(412, 228);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(label13);
            Name = "AddOwnerForm";
            Text = "AddOwnerForm";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label13;
        private Label label12;
        private Label label11;
        private TableLayoutPanel tableLayoutPanel1;
        private TextBox tbLastName;
        private TextBox tbFirstName;
        private TextBox tbSurname;
        private TextBox tbPhone;
        private Label label2;
        private Button btnOk;
        private Button btnCancel;
    }
}