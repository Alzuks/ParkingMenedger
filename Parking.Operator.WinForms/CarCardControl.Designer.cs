namespace Parking.Operator.WinForms
{
    partial class CarCardControl
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            pnlBorder = new Panel();
            pnlContent = new Panel();
            lblPlate = new Label();
            lblDebt = new Label();
            pbPhoto = new PictureBox();
            pnlBorder.SuspendLayout();
            pnlContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbPhoto).BeginInit();
            SuspendLayout();
            // 
            // pnlBorder
            // 
            pnlBorder.BackColor = Color.FromArgb(224, 224, 224);
            pnlBorder.Controls.Add(pnlContent);
            pnlBorder.Dock = DockStyle.Fill;
            pnlBorder.Location = new Point(0, 0);
            pnlBorder.Name = "pnlBorder";
            pnlBorder.Padding = new Padding(4);
            pnlBorder.Size = new Size(640, 380);
            pnlBorder.TabIndex = 0;
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.Black;
            pnlContent.Controls.Add(lblPlate);
            pnlContent.Controls.Add(lblDebt);
            pnlContent.Controls.Add(pbPhoto);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(4, 4);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(632, 372);
            pnlContent.TabIndex = 0;
            // 
            // lblPlate
            // 
            lblPlate.BackColor = Color.LightGray;
            lblPlate.Dock = DockStyle.Top;
            lblPlate.Font = new Font("Segoe UI", 26.25F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lblPlate.Location = new Point(0, 0);
            lblPlate.Name = "lblPlate";
            lblPlate.Size = new Size(632, 48);
            lblPlate.TabIndex = 1;
            lblPlate.Text = "label1";
            lblPlate.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblDebt
            // 
            lblDebt.Dock = DockStyle.Bottom;
            lblDebt.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lblDebt.ForeColor = Color.White;
            lblDebt.Location = new Point(0, 344);
            lblDebt.Name = "lblDebt";
            lblDebt.Size = new Size(632, 28);
            lblDebt.TabIndex = 2;
            lblDebt.Text = "label1";
            lblDebt.TextAlign = ContentAlignment.MiddleCenter;
            lblDebt.Visible = false;
            // 
            // pbPhoto
            // 
            pbPhoto.BackColor = Color.DimGray;
            pbPhoto.Dock = DockStyle.Fill;
            pbPhoto.Location = new Point(0, 0);
            pbPhoto.Name = "pbPhoto";
            pbPhoto.Size = new Size(632, 372);
            pbPhoto.SizeMode = PictureBoxSizeMode.Zoom;
            pbPhoto.TabIndex = 0;
            pbPhoto.TabStop = false;
            // 
            // CarCardControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlBorder);
            Name = "CarCardControl";
            Size = new Size(640, 380);
            pnlBorder.ResumeLayout(false);
            pnlContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pbPhoto).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlBorder;
        private Panel pnlContent;
        private PictureBox pbPhoto;
        private Label lblPlate;
        private Label lblDebt;
    }
}
