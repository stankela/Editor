using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Editor
{
    public partial class ZoomForm : Form
    {
        public ZoomForm()
        {
            InitializeComponent();
        }

        public int ZoomFaktor;

        private void ZoomForm_Load(object sender, EventArgs e)
        {
            string StringZoom = ZoomFaktor.ToString();
            textBox1.Text = StringZoom;
            rbCustom.Checked = true;
            StringZoom = StringZoom + " %";
            for (int I = 0; I < groupBox1.Controls.Count; I++)
            {
                RadioButton RadioBtn = groupBox1.Controls[I] as RadioButton;
                if (RadioBtn != null)
                {
                    if (RadioBtn.Text == StringZoom)
                    {
                        RadioBtn.Checked = true;
                        RadioBtn.Focus();
                    }
                }
            }
        }

        private void rbXXX_Click(object sender, EventArgs e)
        {
            string s = (sender as RadioButton).Text;
            s = s.Substring(0, s.Length - 1).Trim(); // brise znak za procenat;
            textBox1.Text = s;
        }

        private void rbCustom_Click(object sender, EventArgs e)
        {
            textBox1.Focus();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            rbCustom.Checked = true;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                ZoomFaktor = Int32.Parse(textBox1.Text);
                if (ZoomFaktor <= 0)
                    throw new FormatException();
                if (ZoomFaktor < 10) 
                    ZoomFaktor = 10;
                if (ZoomFaktor > 800)
                    ZoomFaktor = 800;
            }
            catch (FormatException)
            {
                MessageBox.Show("Positive value is required.");
                DialogResult = DialogResult.None;
            }
        }

    }
}