namespace Editor
{
    partial class ZoomForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.rbCustom = new System.Windows.Forms.RadioButton();
            this.rb25 = new System.Windows.Forms.RadioButton();
            this.rb50 = new System.Windows.Forms.RadioButton();
            this.rb75 = new System.Windows.Forms.RadioButton();
            this.rb100 = new System.Windows.Forms.RadioButton();
            this.rb125 = new System.Windows.Forms.RadioButton();
            this.rb150 = new System.Windows.Forms.RadioButton();
            this.rb200 = new System.Windows.Forms.RadioButton();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.rbCustom);
            this.groupBox1.Controls.Add(this.rb25);
            this.groupBox1.Controls.Add(this.rb50);
            this.groupBox1.Controls.Add(this.rb75);
            this.groupBox1.Controls.Add(this.rb100);
            this.groupBox1.Controls.Add(this.rb125);
            this.groupBox1.Controls.Add(this.rb150);
            this.groupBox1.Controls.Add(this.rb200);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(167, 212);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Magnification";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(138, 180);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(15, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "%";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(81, 177);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(51, 20);
            this.textBox1.TabIndex = 8;
            this.textBox1.Enter += new System.EventHandler(this.textBox1_Enter);
            // 
            // rbCustom
            // 
            this.rbCustom.AutoSize = true;
            this.rbCustom.Location = new System.Drawing.Point(17, 180);
            this.rbCustom.Name = "rbCustom";
            this.rbCustom.Size = new System.Drawing.Size(60, 17);
            this.rbCustom.TabIndex = 7;
            this.rbCustom.TabStop = true;
            this.rbCustom.Text = "Custom";
            this.rbCustom.UseVisualStyleBackColor = true;
            this.rbCustom.Click += new System.EventHandler(this.rbCustom_Click);
            // 
            // rb25
            // 
            this.rb25.AutoSize = true;
            this.rb25.Location = new System.Drawing.Point(17, 157);
            this.rb25.Name = "rb25";
            this.rb25.Size = new System.Drawing.Size(48, 17);
            this.rb25.TabIndex = 6;
            this.rb25.TabStop = true;
            this.rb25.Text = "25 %";
            this.rb25.UseVisualStyleBackColor = true;
            this.rb25.Click += new System.EventHandler(this.rbXXX_Click);
            // 
            // rb50
            // 
            this.rb50.AutoSize = true;
            this.rb50.Location = new System.Drawing.Point(17, 134);
            this.rb50.Name = "rb50";
            this.rb50.Size = new System.Drawing.Size(48, 17);
            this.rb50.TabIndex = 5;
            this.rb50.TabStop = true;
            this.rb50.Text = "50 %";
            this.rb50.UseVisualStyleBackColor = true;
            this.rb50.Click += new System.EventHandler(this.rbXXX_Click);
            // 
            // rb75
            // 
            this.rb75.AutoSize = true;
            this.rb75.Location = new System.Drawing.Point(17, 111);
            this.rb75.Name = "rb75";
            this.rb75.Size = new System.Drawing.Size(48, 17);
            this.rb75.TabIndex = 4;
            this.rb75.TabStop = true;
            this.rb75.Text = "75 %";
            this.rb75.UseVisualStyleBackColor = true;
            this.rb75.Click += new System.EventHandler(this.rbXXX_Click);
            // 
            // rb100
            // 
            this.rb100.AutoSize = true;
            this.rb100.Location = new System.Drawing.Point(17, 88);
            this.rb100.Name = "rb100";
            this.rb100.Size = new System.Drawing.Size(54, 17);
            this.rb100.TabIndex = 3;
            this.rb100.TabStop = true;
            this.rb100.Text = "100 %";
            this.rb100.UseVisualStyleBackColor = true;
            this.rb100.Click += new System.EventHandler(this.rbXXX_Click);
            // 
            // rb125
            // 
            this.rb125.AutoSize = true;
            this.rb125.Location = new System.Drawing.Point(17, 65);
            this.rb125.Name = "rb125";
            this.rb125.Size = new System.Drawing.Size(54, 17);
            this.rb125.TabIndex = 2;
            this.rb125.TabStop = true;
            this.rb125.Text = "125 %";
            this.rb125.UseVisualStyleBackColor = true;
            this.rb125.Click += new System.EventHandler(this.rbXXX_Click);
            // 
            // rb150
            // 
            this.rb150.AutoSize = true;
            this.rb150.Location = new System.Drawing.Point(17, 42);
            this.rb150.Name = "rb150";
            this.rb150.Size = new System.Drawing.Size(54, 17);
            this.rb150.TabIndex = 1;
            this.rb150.TabStop = true;
            this.rb150.Text = "150 %";
            this.rb150.UseVisualStyleBackColor = true;
            this.rb150.Click += new System.EventHandler(this.rbXXX_Click);
            // 
            // rb200
            // 
            this.rb200.AutoSize = true;
            this.rb200.Location = new System.Drawing.Point(17, 19);
            this.rb200.Name = "rb200";
            this.rb200.Size = new System.Drawing.Size(54, 17);
            this.rb200.TabIndex = 0;
            this.rb200.TabStop = true;
            this.rb200.Text = "200 %";
            this.rb200.UseVisualStyleBackColor = true;
            this.rb200.Click += new System.EventHandler(this.rbXXX_Click);
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(12, 242);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(104, 242);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // ZoomForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(195, 275);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.groupBox1);
            this.Name = "ZoomForm";
            this.Text = "ZoomForm";
            this.Load += new System.EventHandler(this.ZoomForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.RadioButton rbCustom;
        private System.Windows.Forms.RadioButton rb25;
        private System.Windows.Forms.RadioButton rb50;
        private System.Windows.Forms.RadioButton rb75;
        private System.Windows.Forms.RadioButton rb100;
        private System.Windows.Forms.RadioButton rb125;
        private System.Windows.Forms.RadioButton rb150;
        private System.Windows.Forms.RadioButton rb200;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
    }
}