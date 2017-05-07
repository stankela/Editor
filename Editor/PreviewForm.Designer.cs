namespace Editor
{
    partial class PreviewForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblScale = new System.Windows.Forms.Label();
            this.lblSize = new System.Windows.Forms.Label();
            this.lblZoom = new System.Windows.Forms.Label();
            this.ZoomComboBox = new System.Windows.Forms.ComboBox();
            this.ScaleComboBox = new System.Windows.Forms.ComboBox();
            this.PaperComboBox = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.ScrollBarV = new System.Windows.Forms.VScrollBar();
            this.ScrollBarH = new System.Windows.Forms.HScrollBar();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnPrint);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Controls.Add(this.lblScale);
            this.panel1.Controls.Add(this.lblSize);
            this.panel1.Controls.Add(this.lblZoom);
            this.panel1.Controls.Add(this.ZoomComboBox);
            this.panel1.Controls.Add(this.ScaleComboBox);
            this.panel1.Controls.Add(this.PaperComboBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(697, 46);
            this.panel1.TabIndex = 0;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(503, 8);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(75, 23);
            this.btnPrint.TabIndex = 7;
            this.btnPrint.Text = "Print";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(594, 8);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblScale
            // 
            this.lblScale.AutoSize = true;
            this.lblScale.Location = new System.Drawing.Point(310, 13);
            this.lblScale.Name = "lblScale";
            this.lblScale.Size = new System.Drawing.Size(75, 13);
            this.lblScale.TabIndex = 5;
            this.lblScale.Text = "Scale sheet to";
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Location = new System.Drawing.Point(159, 13);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(56, 13);
            this.lblSize.TabIndex = 4;
            this.lblSize.Text = "Paper size";
            // 
            // lblZoom
            // 
            this.lblZoom.AutoSize = true;
            this.lblZoom.Location = new System.Drawing.Point(12, 13);
            this.lblZoom.Name = "lblZoom";
            this.lblZoom.Size = new System.Drawing.Size(34, 13);
            this.lblZoom.TabIndex = 3;
            this.lblZoom.Text = "Zoom";
            // 
            // ZoomComboBox
            // 
            this.ZoomComboBox.FormattingEnabled = true;
            this.ZoomComboBox.Items.AddRange(new object[] {
            "200%",
            "150%",
            "125%",
            "100%",
            "75%",
            "50%",
            "25%",
            "Fit in Window"});
            this.ZoomComboBox.Location = new System.Drawing.Point(52, 10);
            this.ZoomComboBox.Name = "ZoomComboBox";
            this.ZoomComboBox.Size = new System.Drawing.Size(86, 21);
            this.ZoomComboBox.TabIndex = 2;
            this.ZoomComboBox.SelectedIndexChanged += new System.EventHandler(this.ZoomComboBox_SelectedIndexChanged);
            this.ZoomComboBox.Leave += new System.EventHandler(this.ZoomComboBox_Leave);
            this.ZoomComboBox.Enter += new System.EventHandler(this.ZoomComboBox_Enter);
            this.ZoomComboBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ZoomComboBox_KeyDown);
            // 
            // ScaleComboBox
            // 
            this.ScaleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ScaleComboBox.FormattingEnabled = true;
            this.ScaleComboBox.Items.AddRange(new object[] {
            "A4",
            "A3",
            "A2"});
            this.ScaleComboBox.Location = new System.Drawing.Point(391, 10);
            this.ScaleComboBox.Name = "ScaleComboBox";
            this.ScaleComboBox.Size = new System.Drawing.Size(66, 21);
            this.ScaleComboBox.TabIndex = 1;
            this.ScaleComboBox.SelectedIndexChanged += new System.EventHandler(this.ScaleComboBox_SelectedIndexChanged);
            // 
            // PaperComboBox
            // 
            this.PaperComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PaperComboBox.FormattingEnabled = true;
            this.PaperComboBox.Items.AddRange(new object[] {
            "A4"});
            this.PaperComboBox.Location = new System.Drawing.Point(221, 10);
            this.PaperComboBox.Name = "PaperComboBox";
            this.PaperComboBox.Size = new System.Drawing.Size(68, 21);
            this.PaperComboBox.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Location = new System.Drawing.Point(0, 46);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(630, 138);
            this.panel2.TabIndex = 1;
            this.panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.panel2_Paint);
            this.panel2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseMove);
            this.panel2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseDown);
            this.panel2.Resize += new System.EventHandler(this.panel2_Resize);
            this.panel2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseUp);
            // 
            // ScrollBarV
            // 
            this.ScrollBarV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ScrollBarV.Location = new System.Drawing.Point(652, 46);
            this.ScrollBarV.Name = "ScrollBarV";
            this.ScrollBarV.Size = new System.Drawing.Size(17, 138);
            this.ScrollBarV.TabIndex = 9;
            this.ScrollBarV.ValueChanged += new System.EventHandler(this.ScrollBarV_ValueChanged);
            // 
            // ScrollBarH
            // 
            this.ScrollBarH.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ScrollBarH.Location = new System.Drawing.Point(0, 199);
            this.ScrollBarH.Name = "ScrollBarH";
            this.ScrollBarH.Size = new System.Drawing.Size(630, 17);
            this.ScrollBarH.TabIndex = 10;
            this.ScrollBarH.ValueChanged += new System.EventHandler(this.ScrollBarH_ValueChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 244);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(697, 22);
            this.statusStrip1.TabIndex = 11;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // PreviewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(697, 266);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.ScrollBarH);
            this.Controls.Add(this.ScrollBarV);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "PreviewForm";
            this.Text = "PreviewForm";
            this.Load += new System.EventHandler(this.PreviewForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox PaperComboBox;
        private System.Windows.Forms.ComboBox ScaleComboBox;
        private System.Windows.Forms.ComboBox ZoomComboBox;
        private System.Windows.Forms.Label lblZoom;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.Label lblScale;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.VScrollBar ScrollBarV;
        private System.Windows.Forms.HScrollBar ScrollBarH;
        private System.Windows.Forms.StatusStrip statusStrip1;
    }
}