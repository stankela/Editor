using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Printing;
using System.Drawing;

namespace Editor
{
    public class EditorPresenter
    {
        private EditorForm form;

        private Sema sema;
        public Sema Sema
        {
            set { sema = value; }
        }

        // za stampanje
        private Sema kopija; // kopija seme koja se stampa
        private int k; // koliko puta ce se "proci" kroz sve strane 
        private int l; // broji do k
        private int m; // koliko puta se stampa trenutna strana
        private int n; // broji do m
        private int I; // broji do brojVrsta
        private int J; // broji do brojKolona

        public RezimRada rezimRada;

        public EditorPresenter(EditorForm form)
        {
            this.form = form;
        }

        public void cmdUndo()
        {
            // TODO: Pokusaj da dovedes objekt u vidno polje
            if (sema.canUndo())
            {
                if (!sema.manipulating())
                {
                    sema.deselektuj();
                    sema.undo();
                    sema.modified = true;
                    form.nacrtajSemu();
                    form.updateInterface();
                }
            }
        }

        public void cmdRedo()
        {
            if (sema.canRedo())
            {
                if (!sema.manipulating())
                {
                    sema.deselektuj();
                    sema.redo();
                    sema.modified = true;
                    form.nacrtajSemu();
                    form.updateInterface();
                }
            }
        }

        public void cmdCut()
        {
            if (sema.dajBrojSelektovanih() > 0)
            {
                if (!sema.manipulating())
                {
                    sema.cut();
                    sema.modified = true;
                    form.nacrtajSemu();
                    form.updateInterface();
                }
            }
        }

        public void cmdCopy()
        {
            if (sema.dajBrojSelektovanih() > 0)
            {
                if (!sema.manipulating())
                {
                    sema.copy();
                    form.updateInterface();
                }
            }
        }

        public void cmdPaste()
        {
            sema.paste();
        }

        public void cmdDelete()
        {
            if (sema.dajBrojSelektovanih() > 0)
            {
                if (!sema.manipulating())
                {
                    sema.brisi();
                    sema.modified = true;
                    form.nacrtajSemu();
                    form.updateInterface();
                }
            }
        }

        public void cmdRotate()
        {
            sema.rotate();
        }

        public void cmdRezimSelektovanje()
        {
            form.prekini();
            form.selectToolStripComponentSelektovanjeButton();
            rezimRada = RezimRada.Selektovanje;
        }

        public void cmdRezimStavljanjeVeze()
        {
            form.prekini();
            form.selectToolStripComponentStavljanjeVezeButton();
            rezimRada = RezimRada.StavljanjeVeze;
        }

        public void cmdRezimStavljanjeKola(TipKola tip)
        {
            form.prekini();
            form.selectToolStripComponentKoloButton(tip);
            TipKola PrethTipKola = form.tipKola;
            form.tipKola = tip;
            if (rezimRada == RezimRada.StavljanjeKola && form.tipKola == PrethTipKola
            && form.tipKola != TipKola.Ne)
            {
                // uslov RezimRada = StavljanjeKola znaci da je prethodno bilo pritis-
                // nuto jedno od dugmadi za kola, a TipKola = PrethTipKola znaci da je
                // ponovo kliknuto isto dugme; u tom slucaju treba povecati broj ulaza
                // tog kola
                form.brojUlaza[(int)form.tipKola] = form.brojUlaza[(int)form.tipKola] + 1;
                if (form.brojUlaza[(int)form.tipKola] == 5)
                    form.brojUlaza[(int)form.tipKola] = 2;      //max vrednost je 4;
                form.updateToolButtonKoloText();
            }
            rezimRada = RezimRada.StavljanjeKola;
        }

        public void cmdNew()
        {
            form.prekini();
            if (canClose())
            {
                if (form.fileStream != null)
                    form.fileStream.Dispose(); // oslobadja strim na koji je sema snimana
                form.fileStream = null;
                novaSema();
                form.nacrtajSemu();  // prikazuje novo stanje (praznu semu)
            }
        }

        private void novaSema()
        {
            // procedura se poziva na pocetku programa, ili kada se izabere New sa
            // menija(ili toolbara)
            sema = new Sema(form.opcije, SemaSize.A4, form, this);
            form.sema = sema;
            sema.skaliraj(form.zoomFaktor);    // skalira semu na trenutni zoomFaktor
            form.resetujKlizace();   // resetuje klizace, tj. skroluje semu na pocetak
            rezimRada = RezimRada.Selektovanje;
            form.selectToolStripComponentSelektovanjeButton();
            sema.modified = false;
            form.saved = false;
            form.Text = "Editor - Untitled";  // naziv u glavnom prozoru
            form.nacrtajSemu();      // prikazuje novo stanje(praznu semu)
            form.updateInterface();
        }

        public void cmdOpen()
        {
            form.prekini();
            if (canClose())
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.DefaultExt = "edc";
                dlg.Filter = "Editor Document (*.edc)|*.edc";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    form.fileName = dlg.FileName;
                    if (form.fileStream != null)
                        form.fileStream.Dispose(); // oslobadja strim na koji je prethodna sema snimana

                    // I zatim kreira novi strim koji odgovara imenu datoteke izabranom
                    // u dijalogu Open. fmOpenRead znaci da se datoteka otvara samo za
                    // citanje, a fmShareExclusive znaci da program ima iskljucivo pravo
                    // da koristi datoteku, odnosno da drugi programi nemaju pravo da
                    // otvaraju tu datoteku (pogledati Help za metod TFileStream.Create)
                    form.fileStream = new FileStream(form.fileName, FileMode.Open, FileAccess.Read,
                        FileShare.None);
                    // ucitava semu sa strima
                    sema = new Sema(new BinaryReader(form.fileStream), form.opcije, form, this);
                    form.sema = sema;
                    zumiraj(sema.dajSkalu()); // zumira na onaj faktor pod kojim je
                    // sema snimljena na strim
                    form.resetujKlizace();     // dovodi klizace (tj.semu) u pocetan polozaj
                    sema.modified = false;
                    form.saved = true;
                    rezimRada = RezimRada.Selektovanje;
                    form.selectToolStripComponentSelektovanjeButton();
                    form.Text = "Editor - " + form.fileName;  // naziv u glavnom prozoru
                    form.nacrtajSemu();   // crta semu
                    form.updateInterface();
                }
            }
        }

        public void cmdSave()
        {
            if (sema.modified || !form.saved)
                Save();
        }

        // funkcija koja snima semu i vraca vrednost True ako je snimanje uspesno
        // (snimanje moze da bude neuspesno jedino u slucaju da sema nije prethodno
        // bila snimljena i da korisnik u dijalogu SaveAs(koji se poziva iz pro-
        // cedure SaveAs) pritisne Cancel)
        private bool Save()
        {
            if (!form.saved)
                return saveAs();
            else
            {
                if (form.fileStream != null)
                    form.fileStream.Dispose(); //strim se oslobadja i ponovo kreira(u sledecoj liniji
                //koda) jer se procedura Save poziva i iz procedure
                // SaveAs, a u njoj se atributu FileName dodeljuje
                // nova vrednost, tako da se ovim oslobadja stari
                // strim(sa starom vrednosti FileName) i kreira novi
                // strim(sa novom vrednosti FileName); inace, ako se
                // procedura ne poziva iz procedure SaveAs, tada,nakon
                // sto se strim oslobodi, u sledecoj liniji koda se
                // ponovo kreira isti taj strim(tj. sa istom vrednosti
                // FileName)

                // fmCreate znaci da se kreira datoteka sa specificiranim imenom, a ako
                // datoteka sa datim imenom vec postoji, otvara se samo za pisanje;
                // fmShareExclusive znaci da program ima iskljucivo pravo da koristi
                // datoteku, odnosno da drugi programi nemaju pravo da otvaraju tu
                // datoteku (pogledati Help za metod TFileStream.Create)
                form.fileStream = new FileStream(form.fileName, FileMode.Create,
                    FileAccess.Write, FileShare.None);
                sema.saveToStream(new BinaryWriter(form.fileStream));
                sema.modified = false;
                form.updateInterface();
                return true;
            }
        }

        // funkcija koja snima semu pod novim imenom i vraca vrednost True ako je
        // snimanje uspesno (snimanje moze da bude neuspesno jedino u slucaju da
        // korisnik u dijalogu SaveAs pritisne Cancel)
        private bool saveAs()
        {
            form.prekini();
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "";  // pocetni sadrzaj okvira za text FileName
            // u dijalogu SaveAs
            dlg.DefaultExt = "edc";
            dlg.Filter = "Editor Document (*.edc)|*.edc";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                form.saved = true; // obezbedjuje da ne dodje do beskonacnog pozivanja
                // funkcije SaveAs iz funkcije Save
                form.fileName = dlg.FileName;
                Save();  // snimi semu pod novim imenom
                form.Text = "Editor - " + form.fileName;  // nov naziv u glavnom prozoru
                return true;
            }
            else // pritisnuto je Cancel
                return false;
        }

        public bool canClose()
        {
            return sema == null || !sema.modified || saveChanges();
            // prozor ne sme da se zatvori
            // jedino u slucaju kada je sema modifikovana
            // (Modified = True) i korisnik na pitanje
            // da li zeli da snimi promene pritisne
            // Cancel(jedino u tom slucaju funkcija
            // SaveChanges vraca False)
        }

        // funkcija koja se poziva kada korisnik zavrsava rad sa trenutnom semom
        // (bilo da zavrsava rad sa programom, bilo da otvara neku drugu semu ili
        // pocinje rad na novoj semi) a postoje izmene koje nisu snimljene; u tom
        // slucaju funkcija vraca True ako korisnik izabere da snimi(Yes) ili da ne
        // snimi(No) promene, a vraca vrednost False jedino ako korisnik pritisne
        // Cancel(cime korisnik signalizira da odustaje od zapocete operacije i da
        // zeli da i dalje radi u postojecoj semi)
        private bool saveChanges()
        {
            switch (MessageBox.Show("Do you want to save the changes you made to the document?",
                "Save changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
            {
                case DialogResult.Yes:
                    return Save(); // iako je korisnik kliknuo Yes, rezultat zavisi od
                // funkcije Save jer i tu korisnik moze da izabere
                // Cancel(to je situacija kada sema nije snimljena
                // i kada u dijalogu SaveAs korisnik klikne Cancel)
                case DialogResult.No:
                    return true;
                default: // idCancel
                    return false;
            }
        }

        public void cmdSaveAs()
        {
            saveAs();
        }

        public void cmdPrint()
        {
            form.prekini();
            PrintDocument doc = new PrintDocument();
            PrintDialog dlg = new PrintDialog();
            dlg.Document = doc;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                doc.DocumentName = form.Text;
                doc.QueryPageSettings += new QueryPageSettingsEventHandler(OnQueryPageSettings);
                doc.PrintPage += new PrintPageEventHandler(OnPrintPage);

                kopija = sema.dajKopiju(); //stampa se kopija seme jer se prilikom
                //stampanja sema skalira na mnogo vecu vrednost
                //od trenutne velicine(i do 1000%),pa je time
                //sto se stampa kopija obezbedjeno da se
                //prikaz u glavnom prozoru aplikacije u toku
                //stampanja ne menja
                kopija.pocniPrintPreview(); //stavlja kopiju u rezim PrintPreview; u
                //rezmu PrintPreview sema se crta bez grid tackica,
                //bez kvadratica na krajevima pinova i moze da
                //skalira u pravougaonik proizvoljne velicine, a
                //to je upravo ono sto je potrebno prilikom sta-
                //mpanja

                k = 1; // K pokazuje koliko puta ce se "proci" kroz sve strane 
                if (dlg.PrinterSettings.Collate)
                    k = dlg.PrinterSettings.Copies;
                m = 1; // M pokazuje koliko puta se stampa trenutna strana
                if (!dlg.PrinterSettings.Collate)
                    m = dlg.PrinterSettings.Copies;
                l = 1; // broji do K
                n = 1; // broji do M
                I = 1;
                J = 1;

                doc.Print();
            }
        }
        void OnQueryPageSettings(object sender, QueryPageSettingsEventArgs e)
        {
            switch (PreviewForm.ScaleSheetTo)
            {
                case SemaSize.A4:
                    e.PageSettings.Landscape = true;
                    break;

                case SemaSize.A3:
                    e.PageSettings.Landscape = false;
                    break;

                case SemaSize.A2:
                    e.PageSettings.Landscape = true;
                    break;

                case SemaSize.A1:
                    e.PageSettings.Landscape = false;
                    break;

                case SemaSize.A0:
                    e.PageSettings.Landscape = true;
                    break;

                default:
                    break;
            }
        }

        void OnPrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            g.PageUnit = GraphicsUnit.Pixel;
            g.PageScale = 1;
            //pretpostavlja se da je printer formata A4
            int PW_A4 = (int)g.VisibleClipBounds.Width;
            int PH_A4 = (int)g.VisibleClipBounds.Height;
            int BrojVrsta = 1, BrojKolona = 1;
            switch (PreviewForm.ScaleSheetTo)
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
            kopija.skaliraj(new Rectangle(0, 0, BrojKolona * PW_A4, BrojVrsta * PH_A4));
            kopija.nacrtaj(g, form.bitmap, new Rectangle((J - 1) * PW_A4,
                (I - 1) * PH_A4, PW_A4, PH_A4));
            e.HasMorePages = (n != m || I != BrojVrsta || J != BrojKolona || l != k);
            if (++n > m)
            {
                n = 1;
                if (++J > BrojKolona)
                {
                    J = 1;
                    if (++I > BrojVrsta)
                    {
                        I = 1;
                        ++l;
                    }
                }
            }
        }

        public void cmdPrintPreview()
        {
            form.prekini();
            Sema Kopija = sema.dajKopiju(); // kopija seme koja ce se prikazivati u 
            // prozoru Preview
            Kopija.pocniPrintPreview(); // kopija seme ulazi u rezim PrintPreviewa
            PreviewForm dlg = new PreviewForm();
            dlg.EditorForm = form;
            dlg.Sema = Kopija; // inicijalizuje atribut Sema prozora Preview
            form.Hide();  // sakriva glavni prozor aplikacije
            dlg.ShowDialog();  // prikazuje uslovljeno prozor Preview; povratak iz
            // funkcije ShowModal je tek kada se prozor zatvori,
            // tako da se sledeca linija koda (Kopija.Free) iz-
            // vrsava tek kada se prozor Preview zatvori
            form.Show();  // ponovo prikazuje glavni prozor aplikacije
        }

        public void cmdZoom(float zoom)
        {
            form.prekini();
            zumiraj(zoom);
            form.zumirano = true;
            form.nacrtajSemu();
        }

        public void zumiraj(float procenat)
        {
            float staraSkala = sema.dajSkalu();
            sema.skaliraj(procenat);
            form.zoomFaktor = sema.dajSkalu();
            form.showZoomInCombo();

            form.updateScrollBarsOnZoom(staraSkala);
            form.nacrtajSemu();
        }


        public void cmdZoom()
        {
            form.prekini();
            ZoomForm dlg = new ZoomForm();
            dlg.ZoomFaktor = (int)form.zoomFaktor;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                zumiraj(dlg.ZoomFaktor);
                form.nacrtajSemu();
            }
        }

        public void panelSemaMouseDown(MouseEventArgs e)
        {
            sema.mouseDown(e);
        }

        public void panelSemaMouseMove(MouseEventArgs e)
        {
            sema.mouseMove(e);
        }

        public void snapToGrid(ref int pnlSemaX, ref int pnlSemaY)
        {
            if (pnlSemaX + form.delta.X > sema.dajSirinu() - 1)
                pnlSemaX = sema.dajSirinu() - 1 - form.delta.X;
            if (pnlSemaY + form.delta.Y > sema.dajVisinu() - 1)
                pnlSemaY = sema.dajVisinu() - 1 - form.delta.Y;

            if (pnlSemaX < 0) pnlSemaX = 0;
            if (pnlSemaX > form.panelSema.Width - 1) pnlSemaX = form.panelSema.Width - 1;
            if (pnlSemaY < 0) pnlSemaY = 0;
            if (pnlSemaY > form.panelSema.Height - 1) pnlSemaY = form.panelSema.Height - 1;

            int semaX = pnlSemaX + form.delta.X;
            int semaY = pnlSemaY + form.delta.Y;
            sema.snapToGrid(ref semaX, ref semaY);
            pnlSemaX = semaX - form.delta.X;
            pnlSemaY = semaY - form.delta.Y;
            if (pnlSemaX < 0)
                pnlSemaX = pnlSemaX + sema.dajPinRazmak();
            else if (pnlSemaX > form.panelSema.Width - 1)
                pnlSemaX = pnlSemaX - sema.dajPinRazmak();
            if (pnlSemaY < 0)
                pnlSemaY = pnlSemaY + sema.dajPinRazmak();
            else if (pnlSemaY > form.panelSema.Height - 1)
                pnlSemaY = pnlSemaY - sema.dajPinRazmak();
        }

        public void panelSemaMouseUp(MouseEventArgs e)
        {
            sema.mouseUp(e);
        }

        public void dragScroll(Point deltaScroll)
        {
            Point pomerajObjekta = getPomerajObjektaOnDragScroll(deltaScroll);

            if (sema.moveObjectOnDragScroll(pomerajObjekta))
                form.skroluj(deltaScroll.X, deltaScroll.Y);
        }

        private Point getPomerajObjektaOnDragScroll(Point deltaScroll)
        {
            Point result = new Point();
            if (deltaScroll.X != 0)
                result.X = getXPomerajObjektaOnDragScroll(deltaScroll.X);
            if (deltaScroll.Y != 0)
                result.Y = getYPomerajObjektaOnDragScroll(deltaScroll.Y);
            return result;
        }

        private int getXPomerajObjektaOnDragScroll(int xDragScroll)
        {
            int margin =(xDragScroll > 0) ? getRightMargin() : getLeftMargin();
            int pomeraj = (margin + Math.Abs(xDragScroll) < sema.dajPinRazmak()) ?
                0 : sema.dajPinRazmak();
            return (xDragScroll > 0) ? pomeraj : -pomeraj;
        }

        private int getYPomerajObjektaOnDragScroll(int yDragScroll)
        {
            int margin = (yDragScroll > 0) ? getBottomMargin() : getTopMargin();
            int pomeraj = (margin + Math.Abs(yDragScroll) < sema.dajPinRazmak()) ? 
                0 : sema.dajPinRazmak();
            return (yDragScroll > 0) ? pomeraj : -pomeraj;
        }

        // Vraca marginu izmedju leve ivice panela seme i najblize linije grida
        private int getLeftMargin()
        {
            int pinRazmak = sema.dajPinRazmak();
            return (pinRazmak - form.delta.X % pinRazmak) % pinRazmak;
        }

        private int getRightMargin()
        {
            return (form.delta.X + form.panelSema.Width - 1) % sema.dajPinRazmak();
        }

        private int getTopMargin()
        {
            int pinRazmak = sema.dajPinRazmak();
            return (pinRazmak - form.delta.Y % pinRazmak) % pinRazmak;
        }

        private int getBottomMargin()
        {
            // TODO: Kreiraj metode za transformaciju panel u sema koordinate i obratno
            return (form.delta.Y + form.panelSema.Height - 1) % sema.dajPinRazmak();
        }

        public void prekini()
        {
            if (sema != null)
                sema.prekini();
            form.updateInterface();
        }
    }
}
