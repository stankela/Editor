using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Editor
{
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
        }

        public Color BackgroundColor
        {
            get { return pnlBackground.BackColor; }
            set { pnlBackground.BackColor = value; }
        }

        public Color CircuitColor
        {
            get { return pnlCircuit.BackColor; }
            set { pnlCircuit.BackColor = value; }
        }

        public Color LinkColor
        {
            get { return pnlLink.BackColor; }
            set { pnlLink.BackColor = value; }
        }

        public Color PinColor
        {
            get { return pnlPin.BackColor; }
            set { pnlPin.BackColor = value; }
        }

        public Color NodeColor
        {
            get { return pnlNode.BackColor; }
            set { pnlNode.BackColor = value; }
        }

        public Color SelectColor
        {
            get { return pnlSelect.BackColor; }
            set { pnlSelect.BackColor = value; }
        }

        public Color PinEndColor
        {
            get { return pnlPinEnd.BackColor; }
            set { pnlPinEnd.BackColor = value; }
        }

        public Color GridDotColor
        {
            get { return pnlGridDot.BackColor; }
            set { pnlGridDot.BackColor = value; }
        }

        public SemaSize SheetSize
        {
            get
            {
                if (rbtnA4.Checked)
                    return SemaSize.A4;
                else if (rbtnA3.Checked)
                    return SemaSize.A3;
                else if (rbtnA2.Checked)
                    return SemaSize.A2;
                else if (rbtnA1.Checked)
                    return SemaSize.A1;
                else if (rbtnA0.Checked)
                    return SemaSize.A0;
                else
                    return SemaSize.A4;
            }
            set
            {
                if (value == SemaSize.A4)
                    rbtnA4.Checked = true;
                else if (value == SemaSize.A3)
                    rbtnA3.Checked = true;
                else if (value == SemaSize.A2)
                    rbtnA2.Checked = true;
                else if (value == SemaSize.A1)
                    rbtnA1.Checked = true;
                else if (value == SemaSize.A0)
                    rbtnA0.Checked = true;
                else
                    rbtnA4.Checked = true;
            }
        }

        private void panel_MouseDown(object sender, MouseEventArgs e)
        {
            (sender as Panel).BorderStyle = BorderStyle.Fixed3D;
        }

        private void panel_MouseMove(object sender, MouseEventArgs e)
        {
            int W = (sender as Panel).Width;
            int H = (sender as Panel).Height;
            if (e.X < 0 || e.X > W || e.Y < 0 || e.Y > H)
                (sender as Panel).BorderStyle = BorderStyle.None;
        }

        private void panel_MouseUp(object sender, MouseEventArgs e)
        {
            (sender as Panel).BorderStyle = BorderStyle.None;
        }

        private void panel_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = (sender as Panel).BackColor;
            if (dlg.ShowDialog() == DialogResult.OK)
                (sender as Panel).BackColor = dlg.Color;
        }
    }
}