// EditorForm.cs
// Written by Sasa Stankovic

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Drawing.Printing;

namespace Editor
{
    public enum RezimRada { None, Selektovanje, StavljanjeKola, StavljanjeVeze, StavljanjeKopije }

    public partial class EditorForm : Form
    {
        public Opcije opcije;
        public int[] brojUlaza;
        public Sema sema;

        public bool saved;
        public Point delta;  // pokazuje za koliko je sema odskrolovana u stranu
        public Bitmap bitmap; // bitmapa po kojoj se crta, i koja se kopira na PictureBox
        public Graphics bitmapGraphics;

        private bool leftButtonPressed = false;
        private int pnlSemaX, pnlSemaY; // non-snap polozaj misa

        public TipKola tipKola;     // tip kola koje se stavlja

        public FileStream fileStream;
        public string fileName;

        public float zoomFaktor;
        public bool zumirano; // pokazuje da li je sema zumirana u trenutku kada
                       // ComboBox gubi ulazni fokus;

        public Brush bojaPozadineBrush;

        EditorPresenter presenter;
    
        public EditorForm()
        {
            InitializeComponent();

            // podesi panel i klizace
            // prvo podesi polozaj panela
            int w = vScrollBar.Size.Width;
            panelSema.Location = new Point(0, toolStripComponent.Bottom);
            panelSema.Size = new Size(this.ClientSize.Width - w, this.ClientSize.Height - 
                toolStripComponent.Bottom - statusStrip1.Height - w);
            // zatim podesi polozaj klizaca na krajevima panela (polozaj zavisi samo 
            // od polozaja panela2)
            vScrollBar.Location = new Point(panelSema.Right, panelSema.Top);
            vScrollBar.Size = new Size(w, panelSema.Height);
            hScrollBar.Location = new Point(panelSema.Left, panelSema.Bottom);
            hScrollBar.Size = new Size(panelSema.Width, w);
            createBitmap();
            this.KeyPreview = true;
            presenter = new EditorPresenter(this);
        }

        private void initPensAndBrushes()
        {
            bojaPozadineBrush = new SolidBrush(opcije.BojaPozadine);
        }

        private void EditorForm_Load(object sender, EventArgs e)
        {
            opcije = new Opcije(10, 3, Color.Blue, Color.Red, Color.Silver, Color.Maroon,
                Color.Fuchsia, Color.Fuchsia, Color.White, Color.Silver, 4, mnGridDots.Checked);
            initPensAndBrushes();

            zoomFaktor = 100f;
            // temporary disable event handler
            this.zoomComboBox.SelectedIndexChanged -= new System.EventHandler(this.ZoomComboBox_SelectedIndexChanged);
            zoomComboBox.SelectedIndex = zoomComboBox.Items.IndexOf((int)zoomFaktor + "%");
            this.zoomComboBox.SelectedIndexChanged += new System.EventHandler(this.ZoomComboBox_SelectedIndexChanged);

            statusStrip1.Visible = mnStatusBar.Checked;
            toolStripStandard.Visible = mnStandardToolbar.Checked;
            toolStripComponent.Visible = mnComponentToolbar.Checked;

            brojUlaza = new int[(int)TipKola.ExIli + 1];
            for (int i = 0; i <= (int)TipKola.ExIli; i++)
            {
                brojUlaza[i] = 2;
            }
            updateToolButtonKoloText();
            setToolButtonTag();
            presenter.cmdNew();
        }

        public void updateToolButtonKoloText()
        {
            tbNe.Text = "Ne";
            tbI.Text = "I - " + brojUlaza[(int)TipKola.I];
            tbIli.Text = "Ili - " + brojUlaza[(int)TipKola.Ili];
            tbNi.Text = "Ni - " + brojUlaza[(int)TipKola.Ni];
            tbNili.Text = "Nli - " + brojUlaza[(int)TipKola.Nili];
            tbExili.Text = "Exili - " + brojUlaza[(int)TipKola.ExIli];
        }

        private void setToolButtonTag()
        {
            tbNe.Tag = TipKola.Ne;
            tbI.Tag = TipKola.I;
            tbIli.Tag = TipKola.Ili;
            tbNi.Tag = TipKola.Ni;
            tbNili.Tag = TipKola.Nili;
            tbExili.Tag = TipKola.ExIli;
        }

        public void resetujKlizace()
        {
            delta = new Point(0, 0);
            if (sema.dajSirinu() > panelSema.Width)
            {
                hScrollBar.Enabled = true;
                setScrollBarParams(hScrollBar, 0, sema.dajSirinu() - 1, 0, panelSema.Width);
            }
            else
            {
                hScrollBar.Enabled = false;
            }
            if (sema.dajVisinu() > panelSema.Height)
            {
                vScrollBar.Enabled = true;
                setScrollBarParams(vScrollBar, 0, sema.dajVisinu() - 1, 0, panelSema.Height);
            }
            else
            {
                vScrollBar.Enabled = false;
            }
        }

        public void updateInterface()
        {
            int brojSel = 0;
            if (sema != null)
                brojSel = sema.dajBrojSelektovanih();

            if (sema == null)
                mnSave.Enabled = false;
            else
                mnSave.Enabled = sema.modified || !saved;
            tbSave.Enabled = mnSave.Enabled;

            if (sema != null)
                mnUndo.Enabled = sema.canUndo();
            else
                mnUndo.Enabled = false;
            tbUndo.Enabled = mnUndo.Enabled;
            if (sema != null)
                mnRedo.Enabled = sema.canRedo();
            else
                mnRedo.Enabled = false;
            tbRedo.Enabled = mnRedo.Enabled;
            mnCut.Enabled = brojSel > 0;
            tbCut.Enabled = mnCut.Enabled;
            mnCopy.Enabled = brojSel > 0;
            tbCopy.Enabled = mnCopy.Enabled;
            if (sema != null)
                mnPaste.Enabled = sema.canPaste();
            else
                mnPaste.Enabled = false;
            tbPaste.Enabled = mnPaste.Enabled;
            mnDelete.Enabled = brojSel > 0;
            tbDelete.Enabled = mnDelete.Enabled;

            mnRotate.Enabled = sema != null && sema.rotateApplicable();
            tbRotate.Enabled = mnRotate.Enabled; 
        }

        public void nacrtajSemu()
        {
            Rectangle vidljiviDeo = dajVidljiviDeoSeme();
            if (vidljiviDeo.Width < panelSema.Width || vidljiviDeo.Height < panelSema.Height)
            {
                // crtaj pozadinu
                bitmapGraphics.Clear(SystemColors.AppWorkspace);
            }

            sema.nacrtaj(bitmapGraphics, bitmap, vidljiviDeo);
        }

        public Rectangle getRectangle(int left, int top, int right, int bottom)
        {
            int tmp;
            if (left > right)
            {
                tmp = left;
                left = right;
                right = tmp;
            }
            if (top > bottom)
            {
                tmp = top;
                top = bottom;
                bottom = tmp;
            }
            return new Rectangle(left, top, right - left, bottom - top);
        }

        private void panelSema_Paint(object sender, PaintEventArgs e)
        {
            if (e.ClipRectangle == Rectangle.Empty)
                return;
            if (sema != null)
                nacrtajSemu();
        }

        private Rectangle dajVidljiviDeoSeme()
        {
            int Sirina = panelSema.Width;
            if (sema.dajSirinu() < panelSema.Width)
                Sirina = sema.dajSirinu();
            int Visina = panelSema.Height;
            if (sema.dajVisinu() < panelSema.Height)
                Visina = sema.dajVisinu();
            return new Rectangle(delta.X, delta.Y, Sirina, Visina);
        }

        private void panelSema_MouseDown(object sender, MouseEventArgs e)
        {
            panelSema.Focus(); // sklanja fokus sa ZoomComboBoxa (ako je Combo bio u 
                            // fokusu) i generise Leave event za ZoomComboBox
            if (e.Button == MouseButtons.Left)
                leftButtonPressed = true;
            presenter.panelSemaMouseDown(e);
        }

        public void prekini()
        {
            leftButtonPressed = false;
            presenter.prekini();
        }

        private void panelSema_MouseMove(object sender, MouseEventArgs e)
        {
            pnlSemaX = e.X;
            pnlSemaY = e.Y;
            presenter.panelSemaMouseMove(e);
            if (leftButtonPressed)
                tryStartDragScroll();
        }

        public void tryStartDragScroll()
        {
            bool enableTimer = hDragScroll() || vDragScroll();
            if (enableTimer != scrollTimer.Enabled)
                scrollTimer.Enabled = enableTimer;
        }

        private bool hDragScroll()
        {
            // Za levu ivicu je bitno da se proverava da li je vrednost manja
            // ILI JEDNAKA nuli zbog situacije kada je prozor maksimizovan. Tada
            // koordinata ne moze da bude negativna, a mora da se dozvoli skrolovanje
            // kada se objekat dovuce do leve ivice prozora.
            return leftButtonPressed && hScrollBar.Enabled
                && (pnlSemaX <= 0 || pnlSemaX >= panelSema.Width - 1);
        }

        private bool vDragScroll()
        {
            return leftButtonPressed && vScrollBar.Enabled 
                && (pnlSemaY <= 0 || pnlSemaY >= panelSema.Height - 1);
        }

        private Smer hDragScrollSmer()
        {
            if (!hDragScroll())
                return Smer.None;
            if (pnlSemaX <= 0)
                return Smer.Zap;
            else
                return Smer.Ist;
        }

        private Smer vDragScrollSmer()
        {
            if (!vDragScroll())
                return Smer.None;
            if (pnlSemaY <= 0)
                return Smer.Sev;
            else
                return Smer.Jug;
        }

        private void scrollTimer_Tick(object sender, EventArgs e)
        {
            if (!hDragScroll() && !vDragScroll())
            {
                scrollTimer.Enabled = false;
                return;
            }
            onDragScroll();
        }

        private void onDragScroll()
        {
            Point deltaScroll = new Point();

            Smer hSmerSkrol = hDragScrollSmer();
            if (hSmerSkrol != Smer.None)
                deltaScroll.X = getDragScroll(hSmerSkrol);
            Smer vSmerSkrol = vDragScrollSmer();
            if (vSmerSkrol != Smer.None)
                deltaScroll.Y = getDragScroll(vSmerSkrol);

            presenter.dragScroll(deltaScroll);
        }

        private int getDragScroll(Smer smerSkrol)
        {
            int smer, max;
            ScrollBar scrollBar;

            if (smerSkrol == Smer.Zap || smerSkrol == Smer.Ist)
                scrollBar = hScrollBar;
            else
                scrollBar = vScrollBar;
            if (smerSkrol == Smer.Zap || smerSkrol == Smer.Sev)
            {
                smer = -1;
                max = 0;
            }
            else
            {
                smer = 1;
                // Value kod ScrollBar kontrole moze da bude u opsegu [Minimum,
                // Maximum + 1 - LargeChange]
                max = scrollBar.Maximum - scrollBar.LargeChange + 1;
            }
            int skrol = sema.dajPinRazmak();
            if (smer * (max - scrollBar.Value) < skrol)
            {
                // skrol je uvek pozitivan ili nula (kada je smer -1 tada je max 0 sto 
                // daje pozitivan skrol)
                skrol = smer * (max - scrollBar.Value);
            }
            return smer * skrol;
        }

        public void updateStatusBarCoordinates(int x, int y)
        {
            statusStrip1.Items[0].Text = " X=" + (x + delta.X) + "   " +
                "Y=" + (y + delta.Y);
        }

        private void panelSema_MouseUp(object sender, MouseEventArgs e)
        {
            // ovde se ne proverava da li otpusteno levo dugme misa zato sto mis
            // gubi Capture kada se otpusti bilo koje dugme misa
            leftButtonPressed = false;

            presenter.panelSemaMouseUp(e);
        }

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == 533)     // WM_CAPTURECHANGED
            {
                // mis je izgubion Capture
                onLostCapture();
            }
            base.WndProc(ref message);
        }

        public void onLostCapture()
        {
            if (leftButtonPressed)
            {
                leftButtonPressed = false;
                prekini();
            }
        }

        private void tbSelektovanje_Click(object sender, EventArgs e)
        {
            presenter.cmdRezimSelektovanje();
        }

        public void selectToolStripComponentSelektovanjeButton()
        {
            deselectToolStripComponentButton();
            tbSelektovanje.Checked = true;
        }

        private void tbVeza_Click(object sender, EventArgs e)
        {
            presenter.cmdRezimStavljanjeVeze();
        }

        public void selectToolStripComponentStavljanjeVezeButton()
        {
            deselectToolStripComponentButton();
            tbVeza.Checked = true;
        }

        // event handler za dogadjaj OnClick dugmeta tbNe, tbI, tbIli, tnNi, tbNili
        // tbExili
        private void tbKolo_Click(object sender, EventArgs e)
        {
            presenter.cmdRezimStavljanjeKola((TipKola)(sender as ToolStripButton).Tag);
        }

        public void selectToolStripComponentKoloButton(TipKola tipKola)
        {
            deselectToolStripComponentButton();
            getTbKolo(tipKola).Checked = true;
        }

        private ToolStripButton getTbKolo(TipKola tipKola)
        {
            switch (tipKola)
            {
                case TipKola.Ne:
                    return tbNe;
                case TipKola.I:
                    return tbI;
                case TipKola.Ili:
                    return tbIli;
                case TipKola.Ni:
                    return tbNi;
                case TipKola.Nili:
                    return tbNili;
                case TipKola.ExIli:
                    return tbExili;
                default:
                    return null;
            }
        }

        private void deselectToolStripComponentButton()
        {
            foreach (ToolStripButton tsb in toolStripComponent.Items)
            {
                tsb.Checked = false;
            }
        }

        // event handler za dogadjaj OnClick stavke menija New
        private void mnNew_Click(object sender, EventArgs e)
        {
            presenter.cmdNew();
        }

        // event handler za dogadjaj FormClosing(dogadjaj koji se desava uvek kada
        // se pokusa da se zatvori prozor, bilo pritiskom na dugme 'X', bilo pozi-
        // vom metoda prozora Close)
        private void EditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !presenter.canClose();
        }

        // event handler za dogadjaj Click stavke menija mnSave
        private void mnSave_Click(object sender, EventArgs e)
        {
            presenter.cmdSave();
        }

        // event handler za dogadjaj Click stavke menija mnSaveAs
        private void mnSaveAs_Click(object sender, EventArgs e)
        {
            presenter.cmdSaveAs();
        }

        // event handler za dogadjaj Click stavke menija mnOpen
        private void mnOpen_Click(object sender, EventArgs e)
        {
            presenter.cmdOpen();
        }

        public void showZoomInCombo()
        {
            zoomComboBox.Text = (int)zoomFaktor + "%";
        }

        private void setScrollBarParams(ScrollBar scrollBar, int min, int max, int val,
            int largeChange)
        {
            disableScrollBarHandlers();
            scrollBar.Minimum = min;
            scrollBar.Maximum = max;
            scrollBar.LargeChange = largeChange;

            if (val < min)
                val = min;
            if (val > max - largeChange + 1)
                val = max - largeChange + 1;
            scrollBar.Value = val;

            if (scrollBar.GetType() == typeof(HScrollBar))
                delta.X = scrollBar.Value;
            else
                delta.Y = scrollBar.Value;

            enableScrollBarHandlers();
        }

        // event handler za dogadjaj Click stavke menija mnExit
        private void mnExit_Click(object sender, EventArgs e)
        {
            prekini();
            Close();
        }

        // event handler za dogadjaj Click stavke menija mnUndo
        private void mnUndo_Click(object sender, EventArgs e)
        {
            presenter.cmdUndo();
        }

        // event handler za dogadjaj Click stavke menija mnRedo
        private void mnRedo_Click(object sender, EventArgs e)
        {
            presenter.cmdRedo();
        }

        // event handler za dogadjaj Click stavke menija mnCut
        private void mnCut_Click(object sender, EventArgs e)
        {
            presenter.cmdCut();
        }

        // event handler za dogadjaj Click stavke menija mnCopy
        private void mnCopy_Click(object sender, EventArgs e)
        {
            presenter.cmdCopy();
        }

        // event handler za dogadjaj Click stavke menija mnPaste
        private void mnPaste_Click(object sender, EventArgs e)
        {
            presenter.cmdPaste();
        }

        // event handler za dogadjaj Click stavke menija mnDelete
        private void mnDelete_Click(object sender, EventArgs e)
        {
            presenter.cmdDelete();
        }

        // event handler za dogadjaj Click stavke menija mnRotate
        private void mnRotate_Click(object sender, EventArgs e)
        {
            presenter.cmdRotate();
        }

        private void panelSema_Resize(object sender, EventArgs e)
        {
            if (sema == null || this.WindowState == FormWindowState.Minimized)
                return;

            createBitmap();

            if (sema.dajSirinu() > panelSema.Width)
            {
                if (hScrollBar.Enabled)
                {
                    setScrollBarParams(hScrollBar, hScrollBar.Minimum, hScrollBar.Maximum,
                        hScrollBar.Value, panelSema.Width);
                }
                else
                {
                    hScrollBar.Enabled = true;
                    setScrollBarParams(hScrollBar, 0, sema.dajSirinu() - 1, 0,
                        panelSema.Width);
                }
            }
            else
            {
                hScrollBar.Enabled = false;
                delta.X = 0;
            }

            if (sema.dajVisinu() > panelSema.Height)
            {
                if (vScrollBar.Enabled)
                {
                    setScrollBarParams(vScrollBar, vScrollBar.Minimum, vScrollBar.Maximum,
                        vScrollBar.Value, panelSema.Height);
                }
                else
                {
                    vScrollBar.Enabled = true;
                    setScrollBarParams(vScrollBar, 0, sema.dajVisinu() - 1, 0,
                        panelSema.Height);
                }
            }
            else
            {
                vScrollBar.Enabled = false;
                delta.Y = 0;
            }

            // ne treba da se crta jer se crta u Paint eventu za panelSema (promenom 
            // velicine panela invalidira se deo panela sto generise Paint event)
            
            // nacrtajSemu();          
        }

        private void createBitmap()
        {
            if (bitmap != null)
            {
                bitmapGraphics.Dispose();
                bitmap.Dispose();
            }
            bitmap = new Bitmap(panelSema.Width, panelSema.Height);
            bitmapGraphics = Graphics.FromImage(bitmap);
        }

        private void EditorForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    if (leftButtonPressed)
                        Capture = false;
                    prekini();
                    nacrtajSemu();
                    break;

                case Keys.F1:
                    // ShowHelp;
                    break;

                default:
                    break;
            }
        }

        private void EditorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (fileStream != null)
                fileStream.Dispose();
            bitmap.Dispose();
        }

        private void hScrollBar_ValueChanged(object sender, EventArgs e)
        {
            skroluj(hScrollBar.Value - delta.X, 0);
        }

        private void vScrollBar_ValueChanged(object sender, EventArgs e)
        {
            skroluj(0, vScrollBar.Value - delta.Y);
        }

        public void skroluj(int dx, int dy)
        {
            delta.X += dx;
            delta.Y += dy;

            disableScrollBarHandlers();
            hScrollBar.Value = delta.X;
            vScrollBar.Value = delta.Y;
            enableScrollBarHandlers();

            nacrtajSemu();
        }

        private void disableScrollBarHandlers()
        {
            hScrollBar.ValueChanged -= hScrollBar_ValueChanged;
            vScrollBar.ValueChanged -= vScrollBar_ValueChanged;
        }

        private void enableScrollBarHandlers()
        {
            hScrollBar.ValueChanged += hScrollBar_ValueChanged;
            vScrollBar.ValueChanged += vScrollBar_ValueChanged;
        }

        // event handler za dogadjaj Click stavke menija mnGridDots
        private void mnGridDots_Click(object sender, EventArgs e)
        {
            mnGridDots.Checked = !mnGridDots.Checked;
            opcije.GridDots = mnGridDots.Checked;
            nacrtajSemu();
        }

        // event handler za dogadjaj Click dugmeta tbNew
        private void tbNew_Click(object sender, EventArgs e)
        {
            presenter.cmdNew();
        }

        // event handler za dogadjaj Click dugmeta tbOpen
        private void tbOpen_Click(object sender, EventArgs e)
        {
            presenter.cmdOpen();
        }

        // event handler za dogadjaj Click dugmeta tbSave
        private void tbSave_Click(object sender, EventArgs e)
        {
            presenter.cmdSave();
        }

        // event handler za dogadjaj Click stavke dugmeta tbCut
        private void tbCut_Click(object sender, EventArgs e)
        {
            presenter.cmdCut();
        }

        // event handler za dogadjaj Click stavke dugmeta tbCopy
        private void tbCopy_Click(object sender, EventArgs e)
        {
            presenter.cmdCopy();
        }

        // event handler za dogadjaj Click stavke dugmeta tbPaste
        private void tbPaste_Click(object sender, EventArgs e)
        {
            presenter.cmdPaste();
        }

        // event handler za dogadjaj Click stavke dugmeta tbDelete
        private void tbDelete_Click(object sender, EventArgs e)
        {
            presenter.cmdDelete();
        }

        // event handler za dogadjaj Click stavke dugmeta tbUndo
        private void tbUndo_Click(object sender, EventArgs e)
        {
            presenter.cmdUndo();
        }

        // event handler za dogadjaj OnClick stavke dugmeta tbRedo
        private void tbRedo_Click(object sender, EventArgs e)
        {
            presenter.cmdRedo();
        }

        // event handler za dogadjaj Click dugmeta tbRotate
        private void tbRotate_Click(object sender, EventArgs e)
        {
            presenter.cmdRotate();
        }

        // event handler za dogadjaj Click stavke menija mnZoom
        private void mnZoom_Click(object sender, EventArgs e)
        {
            presenter.cmdZoom();
        }

        // event handler za dogadjaj Enter ComboBoxa ZoomComboBox
        private void ZoomComboBox_Enter(object sender, EventArgs e)
        {
            zumirano = false;
        }

        // event handler za dogadjaj Leave ComboBoxa ZoomComboBox
        private void ZoomComboBox_Leave(object sender, EventArgs e)
        {
            if (!zumirano)
                showZoomInCombo();
        }

        private void zoomComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                // '\r' je 13, 'n' je 10
                prekini();
                float zoom;
                if (float.TryParse(zoomComboBox.Text.Replace("%", ""), out zoom)
                && zoom >= 10 && zoom <= 800)
                {
                    presenter.cmdZoom(zoom);
                    toolStripComponent.Focus();
                }
                else
                {
                    MessageBox.Show("Incorrect zoom value.", "Greska");
                    showZoomInCombo();
                }
            }
        }

        // event handler za dogadjaj SelectedIndexChanged ComboBoxa ZoomComboBox
        private void ZoomComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (zumirano || zoomComboBox.Text == "")
            {
                toolStripComponent.Focus();
                return;
            }
            presenter.cmdZoom(float.Parse(zoomComboBox.Text.Replace("%", "")));
            toolStripComponent.Focus(); // sklanja fokus sa ComboBoxa
        }

        private void mnPrint_Click(object sender, EventArgs e)
        {
            presenter.cmdPrint();
        }

        private void tbPrint_Click(object sender, EventArgs e)
        {
            presenter.cmdPrint();
        }

        // event handler za dogadjaj Click stavke menija mnOptions
        private void mnOptions_Click(object sender, EventArgs e)
        {
            prekini();
            OptionsForm dlg = new OptionsForm();
            // potvrdi odgovarajuce radio dugme
            dlg.SheetSize = sema.getSize();
            // postavi boje u prozoru Options u skladu sa trenutnim vrednostima Opcija
            dlg.BackgroundColor = opcije.BojaPozadine;
            dlg.CircuitColor = opcije.BojaKola;
            dlg.LinkColor = opcije.BojaVeze;
            dlg.PinColor = opcije.BojaPina;
            dlg.NodeColor = opcije.BojaCvora;
            dlg.SelectColor = opcije.BojaSelekcije;
            dlg.PinEndColor = opcije.BojaKrajaPina;
            dlg.GridDotColor = opcije.GridDotColor;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // postavi Opcije u skladu sa izabranim vrednostima u prozoru Options
                opcije.BojaPozadine = dlg.BackgroundColor;
                opcije.BojaKola = dlg.CircuitColor;
                opcije.BojaVeze = dlg.LinkColor;
                opcije.BojaPina = dlg.PinColor;
                opcije.BojaCvora = dlg.NodeColor;
                opcije.BojaSelekcije = dlg.SelectColor;
                opcije.BojaKrajaPina = dlg.PinEndColor;
                opcije.GridDotColor = dlg.GridDotColor;
                // nova velicina izabrana u prozoru Options
                SemaSize NewSize = dlg.SheetSize;
                if (NewSize != sema.getSize())
                {
                    sema.changeSize(NewSize);
                    panelSema_Resize(this, null);
                }
            }
            nacrtajSemu();
        }

        // event handler za dogadjaj Click stavke menija mnPrintPreview
        private void mnPrintPreview_Click(object sender, EventArgs e)
        {
            presenter.cmdPrintPreview();
        }

        private void tbPrintPreview_Click(object sender, EventArgs e)
        {
            presenter.cmdPrintPreview();
        }

        // event handler za dogadjaj Opening context menija za panel1
        private void panelSemaContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (sema == null)
                return;
            if (!sema.manipulating())
            {
                if (sema.canSelektujIstiPotencijal())
                {
                    pmnPotencijal.Enabled = true;
                    pmnEnd.Enabled = false;
                }
                else
                    e.Cancel = true;
                return;
            }
            Stanje stanje = sema.dajStanje();
            switch (stanje)
            {
                case Stanje.Veza2:
                    pmnEnd.Enabled = true;
                    pmnPotencijal.Enabled = false;
                    break;

                default:
                    e.Cancel = true; // parametar Cancel ima podrazumevanu vrednost
                                   // false; ako se postavi na true tada se popup
                                   // meni ne pojavljuje
                    break;
            }
        }

        // event handler za dogadjaj Click stavke pop-up menija pmnPotencijal
        private void pmnPotencijal_Click(object sender, EventArgs e)
        {
            if (sema.canSelektujIstiPotencijal()) // selektovan je samo jedan element
            //  i taj element je veza
            {
                sema.selektujIstiPotencijal();
                nacrtajSemu();
            }
        }

        // event handler za dogadjaj Click stavke pop-up menija pmnEnd
        private void pmnEnd_Click(object sender, EventArgs e)
        {
            prekini();
        }

        // event handler za dogadjaj Opening(pre pojavljivanja Popup menija)
        // za dugmad tbI, tbIli, tbNi, tbNili, tbExili
        private void KoloContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (presenter.rezimRada == RezimRada.StavljanjeKola)
            {
                // deselektuj sve
                pmnInput2.Checked = false;
                pmnInput3.Checked = false;
                pmnInput4.Checked = false;
                // potvrdjuje odgovarajucu stavku, u zavisnosti od broja ulaza priti-
                // snutog kola
                switch (brojUlaza[(int)tipKola])
                {
                    case 2:
                        pmnInput2.Checked = true;
                        break;
                    case 3:
                        pmnInput3.Checked = true;
                        break;
                    case 4:
                        pmnInput4.Checked = true;
                        break;
                    default:
                        break;
                }
            }
            else
                e.Cancel = true;
        }

        //event handler za dogadjaj Click za sve stavke context menija KoloContextMenu
        // (context meni pridruzen dugmadima tbI, tbIli, tbNi, tbNili, tbExili)
        private void pmnInput_Click(object sender, EventArgs e)
        {
            prekini();
            // prvo se inicijalizuje promenljiva BrUlaza, u zavisnosti od stavke koja
            // je kliknuta
            int BrUlaza = 2;
            if ((sender as ToolStripMenuItem).Text.IndexOf('2') >= 0)
                BrUlaza = 2;
            else if ((sender as ToolStripMenuItem).Text.IndexOf('3') >= 0)
                BrUlaza = 3;
            else if ((sender as ToolStripMenuItem).Text.IndexOf('4') >= 0)
                BrUlaza = 4;
            brojUlaza[(int)tipKola] = BrUlaza; // menja se odgovarajuci clan niza
                                                  // BrojUlaza, u zavisnosti od tipa
                                                  // kola cije je dugme pritisnuto(Pri-
                                                  // tisnutoKolo), i u zavisnosti od
                                                  // stavke koja je izabrana sa popup
                                                  // menija(BrUlaza)
            updateToolButtonKoloText();
        }

        // event handler za dogadjaj Click stavke menija mnContents
        private void mnContents_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Nije implementirano.");
        }

        // event handler za dogadjaj Click stavke menija mnAbout
        private void mnAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Editor\n" + "Version 1.0\n" + "by Sasa Stankovic");
        }

        // event handler za dogadjaj Click stavke menija mnStatusBar(kojom se
        // prikazuje i sakriva StatusBar)
        private void mnStatusBar_Click(object sender, EventArgs e)
        {
            statusStrip1.Visible = (sender as ToolStripMenuItem).Checked;
        }

        private void mnStandardToolbar_Click(object sender, EventArgs e)
        {
            toolStripStandard.Visible = (sender as ToolStripMenuItem).Checked;
        }

        private void mnComponentToolbar_Click(object sender, EventArgs e)
        {
            toolStripComponent.Visible = (sender as ToolStripMenuItem).Checked;
        }

        public void print()
        {
            presenter.cmdPrint();
        }

        public void updateScrollBarsOnZoom(float staraSkala)
        {
            int sredina;
            if (!hScrollBar.Enabled)
            {
                // sirina seme pre zumiranja bila je manja od sirine panela
                if (sema.dajSirinu() > panelSema.Width)
                {
                    // sirina seme nakon zumiranja je veca od panela
                    hScrollBar.Enabled = true;
                    setScrollBarParams(hScrollBar, 0, sema.dajSirinu() - 1, 0,
                        panelSema.Width);
                }
            }
            else
            {
                // sirina seme pre zumiranja je bila veca od panela
                if (sema.dajSirinu() <= panelSema.Width)
                {
                    //sirina nakon zumiranja je manja od panela
                    hScrollBar.Enabled = false;
                    delta.X = 0;
                }
                else
                {
                    // sirina seme nakon zumiranja je veca od panela
                    // dovedi zumiranu sredinu ponovo u sredinu panela

                    // sredina pre zumiranja
                    sredina = hScrollBar.Value + panelSema.Width / 2;
                    // sredina nakon zumiranja
                    sredina = (int)Math.Round(sredina * sema.dajSkalu() / staraSkala);

                    setScrollBarParams(hScrollBar, hScrollBar.Minimum,
                        sema.dajSirinu() - 1, sredina - panelSema.Width / 2,
                        panelSema.Width);
                }
            }
            // ponovi isto za y koordinatu
            if (!vScrollBar.Enabled)
            {
                if (sema.dajVisinu() > panelSema.Height)
                {
                    vScrollBar.Enabled = true;
                    setScrollBarParams(vScrollBar, 0, sema.dajVisinu() - 1, 0,
                        panelSema.Height);
                }
            }
            else
            {
                if (sema.dajVisinu() <= panelSema.Height)
                {
                    vScrollBar.Enabled = false;
                    delta.Y = 0;
                }
                else
                {
                    sredina = vScrollBar.Value + panelSema.Height / 2;
                    sredina = (int)Math.Round(sredina * sema.dajSkalu() / staraSkala);

                    setScrollBarParams(vScrollBar, vScrollBar.Minimum,
                        sema.dajVisinu() - 1, sredina - panelSema.Height / 2,
                        panelSema.Height);

                }
            }
        }

        private void panelSema_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show(sema.brojevi());
        }
    }
}
