using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Editor
{
    public partial class PreviewForm : Form
    {
        public PreviewForm()
        {
            InitializeComponent();

            // podesi panel2 i klizace
            // prvo podesi polozaj panela
            int w = ScrollBarV.Size.Width;
            panel2.Location = new Point(0, panel1.Bottom);
            panel2.Size = new Size(this.ClientSize.Width - w,
                this.ClientSize.Height - panel1.Bottom - statusStrip1.Height - w);
            // zatim podesi polozaj klizaca na krajevima panela (polozaj zavisi samo 
            // od polozaja panela2)
            ScrollBarV.Location = new Point(panel2.Right, panel2.Top);
            ScrollBarV.Size = new Size(w, panel2.Height);
            ScrollBarH.Location = new Point(panel2.Left, panel2.Bottom);
            ScrollBarH.Size = new Size(panel2.Width, w);
        }

        public static SemaSize ScaleSheetTo = SemaSize.A4;
        public Sema Sema;
        public EditorForm EditorForm;

        private int margin;
        private int LeftMargin;
        private int TopMargin;
        public Bitmap BmpSema;
        private int BrojVrsta;
        private int BrojKolona;
        private int PreviewWidth;
        private int PreviewHeight;
        private PrevItem[,] Items = new PrevItem[5, 5]; // index 0 se ne koristi
                                                    // (posledica prevoda sa Delphija)
        private bool PritisnutMis;
        private bool Zumirano;
        private string StaraVrednost;
        private int PrethX;
        private int PrethY;



        private void PreviewForm_Load(object sender, EventArgs e)
        {
            margin = 10;
            ScaleSheetTo = SemaSize.A4;
            PaperComboBox.SelectedIndex = 0;

            OdrediDimenzije();
            Podeli();
            ResetujKlizace();
            Zumiraj(0, true);
            ScaleComboBox.SelectedIndex = (int)ScaleSheetTo;
        }

        private void OdrediDimenzije()
        {
            switch (ScaleSheetTo)
            {
                case SemaSize.A4:
                    BrojVrsta = 1;
                    BrojKolona = 1;
                    break;

                case SemaSize.A3:
                    BrojVrsta = 1;
                    BrojKolona = 2;
                    break;

                case SemaSize.A2:
                    BrojVrsta = 2;
                    BrojKolona = 2;
                    break;
                    
                case SemaSize.A1:
                    BrojVrsta = 2;
                    BrojKolona = 4;
                    break;

                case SemaSize.A0:
                    BrojVrsta = 4;
                    BrojKolona = 4;
                    break;

                default:
                    break;
            }
        }

        private void Podeli()
        {
            Rectangle[,] Delovi;
            int BmpWidth = (int)Math.Round((float)Sema.dajSirinu() / BrojKolona);
            int BmpHeight = (int)Math.Round((float)Sema.dajVisinu() / BrojVrsta);
            Delovi = new Rectangle[5, 5];//index 0 se ne koristi(posledica prevoda sa Delphija)
            for (int I = 1; I <= BrojVrsta - 1; I++)
            {
                for (int J = 1; J <= BrojKolona - 1; J++)
                    Delovi[I, J] = Rectangle.FromLTRB((J - 1) * BmpWidth, (I - 1) * BmpHeight,
                                      J * BmpWidth, I * BmpHeight);
                Delovi[I, BrojKolona] = Rectangle.FromLTRB((BrojKolona - 1) * BmpWidth, 
                                 (I - 1) * BmpHeight, Sema.dajSirinu(), I * BmpHeight);
            }
            for (int J = 1; J <= BrojKolona - 1; J++)
                Delovi[BrojVrsta, J] = Rectangle.FromLTRB((J - 1) * BmpWidth, 
                    (BrojVrsta - 1) * BmpHeight, J * BmpWidth, Sema.dajVisinu());
            Delovi[BrojVrsta, BrojKolona] = Rectangle.FromLTRB((BrojKolona - 1) * BmpWidth,
                      (BrojVrsta - 1) * BmpHeight, Sema.dajSirinu(), Sema.dajVisinu());

            PreviewWidth = Sema.dajSirinu() + (BrojKolona + 1) * margin;
            PreviewHeight = Sema.dajVisinu() + (BrojVrsta + 1) * margin;

            for (int I = 1; I <= BrojVrsta; I++)
            {
                for (int J = 1; J <= BrojKolona; J++)
                {
                    Rectangle Deo = Delovi[I, J];
                    Rectangle PomerenDeo = Rectangle.FromLTRB(Deo.Left + J * margin, 
                        Deo.Top + I * margin, Deo.Right + J * margin, 
                        Deo.Bottom + I * margin);
                    Items[I, J] = new PrevItem(PomerenDeo, Deo, this);
                }
            }
        }

        private void ResetujKlizace()
        {
            if (PreviewWidth <= panel2.Width)
            {
                ScrollBarH.Enabled = false;
                LeftMargin = (panel2.Width - PreviewWidth) / 2;
            }
            else
            {
                ScrollBarH.Enabled = true;
                ScrollBarH.LargeChange = panel2.Width;
                SetScrollBarParams(ScrollBarH, 0, PreviewWidth - 1, 0);
            }
            if (PreviewHeight <= panel2.Height)
            {
                ScrollBarV.Enabled = false;
                TopMargin = (panel2.Height - PreviewHeight) / 2;
            }
            else
            {
                ScrollBarV.Enabled = true;
                ScrollBarV.LargeChange = panel2.Height;
                SetScrollBarParams(ScrollBarV, 0, PreviewHeight - 1, 0);
            }
        }

        private void SetScrollBarParams(ScrollBar scrollBar, int min, int max, int val)
        {
            scrollBar.Minimum = min;
            scrollBar.Maximum = max;
            scrollBar.Value = val;
        }

        private void Zumiraj(int Procenat, bool FitInWindow)
        {
            Point Sredina = new Point();
            float StaraSkala = Sema.dajSkalu();
            if (!FitInWindow)
                Sema.skaliraj(Procenat);
            else
                Sema.skaliraj(new Rectangle(0, 0, panel2.Width - (BrojKolona + 1) * margin,
                    panel2.Height - (BrojVrsta + 1) * margin));
            ZoomComboBox.Text = (int)Math.Round(Sema.dajSkalu()) + "%";
            Podeli();

            if (!ScrollBarH.Enabled)
            {
                if (PreviewWidth <= panel2.Width)
                    LeftMargin = (panel2.Width - PreviewWidth) / 2;
                else
                {
                    ScrollBarH.Enabled = true;
                    ScrollBarH.LargeChange = panel2.Width;
                    ScrollBarH.Maximum = PreviewWidth - 1;
                    ScrollBarH.Value = 
                        (ScrollBarH.Maximum - ScrollBarH.LargeChange + 1) / 2;
                }
            }
            else
            {
                if (PreviewWidth <= panel2.Width)
                {
                    ScrollBarH.Enabled = false;
                    LeftMargin = (panel2.Width - PreviewWidth) / 2;
                }
                else
                {
                    ScrollBarH.Maximum = PreviewWidth - 1;
                    Sredina.X = ScrollBarH.Value + panel2.Width / 2;
                    Sredina.X = (int)Math.Round(Sredina.X * Sema.dajSkalu() / StaraSkala);
                    int newValue = Sredina.X - panel2.Width / 2;
                    if (newValue < 0)
                        newValue = 0;
                    if (newValue > ScrollBarH.Maximum - ScrollBarH.LargeChange + 1)
                        newValue = ScrollBarH.Maximum - ScrollBarH.LargeChange + 1;
                    ScrollBarH.Value = newValue;
                }
            }
            if (!ScrollBarV.Enabled)
            {
                if (PreviewHeight <= panel2.Height)
                    TopMargin = (panel2.Height - PreviewHeight) / 2;
                else
                {
                    ScrollBarV.Enabled = true;
                    ScrollBarV.LargeChange = panel2.Height;
                    ScrollBarV.Maximum = PreviewHeight - 1;
                    ScrollBarV.Value = 
                        (ScrollBarV.Maximum - ScrollBarV.LargeChange + 1) / 2;
                }
            }
            else
            {
                if (PreviewHeight <= panel2.Height)
                {
                    ScrollBarV.Enabled = false;
                    TopMargin = (panel2.Height - PreviewHeight) / 2;
                }
                else
                {
                    ScrollBarV.Maximum = PreviewHeight - 1;
                    Sredina.Y = ScrollBarV.Value + panel2.Height / 2;
                    Sredina.Y = (int)Math.Round(Sredina.Y * Sema.dajSkalu() / StaraSkala);
                    int newValue = Sredina.Y - panel2.Height / 2;
                    if (newValue < 0)
                        newValue = 0;
                    if (newValue > ScrollBarV.Maximum - ScrollBarV.LargeChange + 1)
                        newValue = ScrollBarV.Maximum - ScrollBarV.LargeChange + 1;
                    ScrollBarV.Value = newValue;
                }
            }
            nacrtaj();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            nacrtaj();
        }

        private void nacrtaj()
        {
            Rectangle VidljiviDeoSeme = DajVidljiviDeoSeme();
            BmpSema = new Bitmap(VidljiviDeoSeme.Width, VidljiviDeoSeme.Height);
            Graphics grSema = Graphics.FromImage(BmpSema);
            Sema.nacrtaj(grSema, BmpSema, VidljiviDeoSeme);
            grSema.Dispose();

            Bitmap BmpPanel = new Bitmap(panel2.Width, panel2.Height);
            Graphics grPanel = Graphics.FromImage(BmpPanel);
            grPanel.Clear(SystemColors.AppWorkspace);

            for (int I = 1; I <= BrojVrsta; I++)
            {
                for (int J = 1; J <= BrojKolona; J++)
                {
                    Items[I, J].Nacrtaj(grPanel);
                }
            }
            grPanel.Dispose();
            // Graphics g = e.Graphics;  // ne radi (vidi objasnjenje kod EditorForm)
            Graphics g = panel2.CreateGraphics();
            g.DrawImage(BmpPanel, 0, 0, panel2.Width, panel2.Height);
            g.Dispose();
        }

        public Rectangle DajVidljiviDeoSeme()
        {
            Rectangle Presek;
            int left = 0, top = 0, right = 0, bottom = 0;
            Rectangle VidljiviDeo = DajVidljiviDeo();
            bool Nadjen = false;
            for (int I = 1; I <= BrojVrsta; I++)
            {
                for (int J = 1; J <= BrojKolona; J++)
                {
                    if (Items[I, J].Unutar(VidljiviDeo, out Presek))
                    {
                        left = Presek.Left - J * margin;
                        top = Presek.Top - I * margin;
                        Nadjen = true;
                        break;
                    }
                }
                if (Nadjen) 
                    break;
            }
            Nadjen = false;
            for (int I = BrojVrsta; I >= 1; I--)
            {
                for (int J = BrojKolona; J >= 1; J--)
                {
                    if (Items[I, J].Unutar(VidljiviDeo, out Presek))
                    {
                        right = Presek.Right - J * margin;
                        bottom = Presek.Bottom - I * margin;
                        Nadjen = true;
                        break;
                    }
                }
                if (Nadjen)
                    break;
            }
            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        public Rectangle DajVidljiviDeo()
        {
            int left;
            if (ScrollBarH.Enabled)
                left = ScrollBarH.Value;
            else
                left = - LeftMargin;
            int top;
            if (ScrollBarV.Enabled)
                top = ScrollBarV.Value;
            else
                top = - TopMargin;
            return new Rectangle(left, top, panel2.Width, panel2.Height);
        }

        private void panel2_Resize(object sender, EventArgs e)
        {
            if (PreviewWidth <= panel2.Width)
            {
                ScrollBarH.Enabled = false;
                LeftMargin = (panel2.Width - PreviewWidth) / 2;
            }
            else
            {
                if (ScrollBarH.Enabled)
                {
                    ScrollBarH.Maximum = PreviewWidth - 1;
                    ScrollBarH.LargeChange = panel2.Width;
                }
                else
                {
                    ScrollBarH.Enabled = true;
                    ScrollBarH.LargeChange = panel2.Width;
                    SetScrollBarParams(ScrollBarH, 0, PreviewWidth - 1, 0);
                }
            }
            if (PreviewHeight <= panel2.Height)
            {
                ScrollBarV.Enabled = false;
                TopMargin = (panel2.Height - PreviewHeight) / 2;
            }
            else
            {
                if (ScrollBarV.Enabled)
                {
                    ScrollBarV.Maximum = PreviewHeight - 1;
                    ScrollBarV.LargeChange = panel2.Height;
                }
                else
                {
                    ScrollBarV.Enabled = true;
                    ScrollBarV.LargeChange = panel2.Height;
                    SetScrollBarParams(ScrollBarV, 0, PreviewHeight - 1, 0);
                }
            }
        }

        private void ScrollBarV_ValueChanged(object sender, EventArgs e)
        {
          if (!PritisnutMis)
              nacrtaj();
      }

        private void ScrollBarH_ValueChanged(object sender, EventArgs e)
        {
            if (!PritisnutMis)
                nacrtaj();
        }

        private void ZoomComboBox_Enter(object sender, EventArgs e)
        {
            Zumirano = false;
            StaraVrednost = ZoomComboBox.Text;
        }

        private void ZoomComboBox_Leave(object sender, EventArgs e)
        {
            if (!Zumirano)
                ZoomComboBox.Text = StaraVrednost;
        }

        private void ZoomComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;
            try
            {
                int ZoomFaktor = Int32.Parse(ZoomComboBox.Text.Replace("%", ""));
                if (ZoomFaktor <= 0)
                    throw new FormatException();
                if (ZoomFaktor < 10)
                    ZoomFaktor = 10;
                if (ZoomFaktor > 800)
                    ZoomFaktor = 800;
                Zumiraj(ZoomFaktor, false);
                Zumirano = true;
                nacrtaj();
                panel1.Focus();
            }
            catch (FormatException)
            {
                MessageBox.Show("Positive value is required.");
                ZoomComboBox.Text = StaraVrednost;
                ZoomComboBox.SelectAll();
            }
        }

        private void ZoomComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Zumirano || ZoomComboBox.Text == "")
            {
                panel1.Focus();
                return;
            }
            if (ZoomComboBox.Text != "Fit in Window")
                Zumiraj(Int32.Parse(ZoomComboBox.Text.Replace("%", "")), false);
            else
            {
                Zumiraj(0, true);
            }
            Zumirano = true;
            nacrtaj();
            panel1.Focus(); // sklanja fokus sa ComboBoxa
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            panel1.Focus();
            PritisnutMis = true;
            PrethX = e.X;
            PrethY = e.Y;
        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (PritisnutMis)
            {
                int newValue;
                if (ScrollBarH.Enabled)
                {
                    newValue = ScrollBarH.Value - (e.X - PrethX);
                    if (newValue < 0)
                        newValue = 0;
                    if (newValue > ScrollBarH.Maximum - ScrollBarH.LargeChange + 1)
                        newValue = ScrollBarH.Maximum - ScrollBarH.LargeChange + 1;
                    ScrollBarH.Value = newValue;
                }
                if (ScrollBarV.Enabled)
                {
                    newValue = ScrollBarV.Value - (e.Y - PrethY);
                    if (newValue < 0)
                        newValue = 0;
                    if (newValue > ScrollBarV.Maximum - ScrollBarV.LargeChange + 1)
                        newValue = ScrollBarV.Maximum - ScrollBarV.LargeChange + 1;
                    ScrollBarV.Value = newValue;
                }
                nacrtaj();
                PrethX = e.X;
                PrethY = e.Y;
            }
        }

        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            PritisnutMis = false;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            panel1.Focus();
        }

        private void ScaleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ScaleSheetTo = (SemaSize)ScaleComboBox.SelectedIndex;
            OdrediDimenzije();
            Podeli();
            ResetujKlizace();
            Zumiraj(0, true);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            EditorForm.print();
         }
    }

    public class PrevItem
    {
        private Rectangle Pozicija; // pozicija na Previewu (racunaju se i margine izmedju
                             // kao i margine na pocetku
        private Rectangle DeoSeme;  // deo seme koji prikazuje PrevItem
        PreviewForm Preview;

        public PrevItem(Rectangle APozicija, Rectangle ADeoSeme, PreviewForm APreview)
        {
            Pozicija = APozicija;
            DeoSeme = ADeoSeme;
            Preview = APreview;
        }

        public void Nacrtaj(Graphics g)
        {
            Rectangle VidljiviDeo = Preview.DajVidljiviDeo();
            Rectangle VDS = Preview.DajVidljiviDeoSeme();
            Rectangle rectDest = new Rectangle(Pozicija.Left - VidljiviDeo.Left, 
                Pozicija.Top - VidljiviDeo.Top, Pozicija.Right - Pozicija.Left, 
                Pozicija.Bottom - Pozicija.Top);
            Rectangle rectSrc = new Rectangle(DeoSeme.Left - VDS.Left, 
                DeoSeme.Top - VDS.Top, DeoSeme.Right - DeoSeme.Left, 
                DeoSeme.Bottom - DeoSeme.Top);
            g.DrawImage(Preview.BmpSema, rectDest.X, rectDest.Y, rectSrc, 
                GraphicsUnit.Pixel);
            Rectangle rectBorder = rectDest;
            rectBorder.Inflate(1, 1);
            using (Pen p = new Pen(Color.Black))
            {
                g.DrawRectangle(p, rectBorder); 
            }
            int d = 3;
            Rectangle rightShadow = new Rectangle(rectDest.Right, 
                rectDest.Top + d, d, rectDest.Height);
            Rectangle bottomShadow = new Rectangle(rectDest.Left + d, rectDest.Bottom, 
                rectDest.Width, d);
            using (Brush b = new SolidBrush(Color.Black))
            {
                g.FillRectangle(b, rightShadow);
                g.FillRectangle(b, bottomShadow);
            }
        }

        public bool Unutar(Rectangle Rect, out Rectangle Presek)
        {
            Presek = new Rectangle();
            bool Result = (Pozicija.Top < Rect.Bottom) 
                && (Pozicija.Bottom > Rect.Top) 
                && (Pozicija.Left < Rect.Right) 
                && (Pozicija.Right > Rect.Left);
            if (Result)
            {
                int top = Pozicija.Top;
                if (Rect.Top > Pozicija.Top)
                    top = Rect.Top;
                int left = Pozicija.Left;
                if (Rect.Left > Pozicija.Left)
                    left = Rect.Left;
                int bottom = Pozicija.Bottom;
                if (Rect.Bottom < Pozicija.Bottom)
                    bottom = Rect.Bottom;
                int right = Pozicija.Right;
                if (Rect.Right < Pozicija.Right)
                    right = Rect.Right;

                Presek = Rectangle.FromLTRB(left, top, right, bottom);
            }
            return Result;
        }

    }
}