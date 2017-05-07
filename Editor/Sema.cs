// Sema.cs
// Written by Sasa Stankovic

using System;
using System.Drawing;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Editor;
using System.Windows.Forms;

public enum TipKola { Ne, I, Ili, Ni, Nili, ExIli }
public enum StanjeKomande { Kreirana, Izvrsena, Vracena }

public enum Stanje
{
    None, Pomeranje1, Pomeranje2, Laso1, Laso2, Kolo1, Kolo2, Veza1, Veza2, Kopija1, Kopija2
}


public abstract class GrafObjekt
{  
    protected Point pocetak;
    protected Point kraj;
    protected bool objektJeSelektovan;
    protected bool objektSePomera;
    protected bool objektSePrikazuje;
    protected float skala;
    protected Sema sema;
    protected int ID;
    protected bool nonGridSkaliranje;

    private PointF realPocetak;
    private PointF realKraj;

    private List<Prikljucak> prikljucci = new List<Prikljucak>();

    public List<Prikljucak> getPrikljucci()
    {
        return prikljucci;
    }

    protected GrafObjekt()
    {

    }

    protected GrafObjekt(Point pocetak, Point kraj, Sema sema, int AID)
    {
        init(pocetak, kraj, sema, AID);
    }

    protected void init(Point pocetak, Point kraj, Sema sema, int ID)
    {
        this.pocetak = pocetak;
        this.kraj = kraj;
        this.sema = sema;
        if (sema != null)
            skala = sema.dajSkalu();
        this.ID = ID;
        objektSePrikazuje = true;
    }

    public abstract void nacrtaj(Graphics g, Point delta);
    public abstract void pomeri(int dx, int dy);
    public abstract bool pogodjen(int x, int y);
    public abstract Point krajnjeGoreLevo();
    public abstract Point krajnjeDoleDesno();
    public abstract Point dajTeziste();
    public abstract GrafObjekt dajKopiju();

    public void selektuj()
    {
        objektJeSelektovan = true;
    }

    public void deselektuj()
    {
        objektJeSelektovan = false;
    }

    public bool selektovan()
    {
        return objektJeSelektovan;
    }

    public void pocniPomeranje()
    {
        objektSePomera = true;
    }

    public void zavrsiPomeranje()
    {
        objektSePomera = false;
    }

    public bool pomeraSe()
    {
        return objektSePomera;
    }

    public void prikazi()
    {
        objektSePrikazuje = true;
    }

    public void sakrij()
    {
        objektSePrikazuje = false;
    }

    public bool prikazujeSe()
    {
        return objektSePrikazuje;
    }

    public Point dajPocetak()
    {
        return pocetak;
    }

    public Point dajKraj()
    {
        return kraj;
    }

    public Sema dajSemu()
    {
        return sema;
    }

    public void postaviSemu(Sema ASema)
    {
        sema = ASema;
    }

    public int dajID()
    {
        return ID;
    }

    public virtual bool unutar(Rectangle rect, bool skroz)
    {
        Point poc, kr;
        poc = krajnjeGoreLevo();
        kr = krajnjeDoleDesno();
        if (skroz)
            return (poc.X >= rect.Left) && (poc.Y >= rect.Top) &&
                (kr.X <= rect.Right) && (kr.Y <= rect.Bottom);
        else
            return (poc.X < rect.Right) && (poc.Y < rect.Bottom) &&
                (kr.X > rect.Left) && (kr.Y > rect.Top);
    }

    public virtual void rotiraj(Point centarRot, int smerRot)
    {
        Prostor.rotirajTacku(ref pocetak, centarRot, smerRot);
        Prostor.rotirajTacku(ref kraj, centarRot, smerRot);
    }

    public virtual void pocniNonGridSkaliranje()
    {
        nonGridSkaliranje = true;
        realPocetak.X = pocetak.X;
        realPocetak.Y = pocetak.Y;
        realKraj.X = kraj.X;
        realKraj.Y = kraj.Y;
    }

    public virtual void zavrsiNonGridSkaliranje()
    {
        nonGridSkaliranje = false;
    }

    public virtual void skaliraj(float novaSkala)
    {
        if (nonGridSkaliranje)
        {
            realPocetak.X = realPocetak.X * novaSkala/skala;
            realPocetak.Y = realPocetak.Y * novaSkala/skala;
            realKraj.X = realKraj.X * novaSkala/skala;
            realKraj.Y = realKraj.Y * novaSkala/skala;
            pocetak.X = (int)Math.Round(realPocetak.X);
            pocetak.Y = (int)Math.Round(realPocetak.Y);
            kraj.X = (int)Math.Round(realKraj.X);
            kraj.Y = (int)Math.Round(realKraj.Y);
        }
        else
        {
            pocetak.X = (int)Math.Round(pocetak.X * novaSkala / skala);
            pocetak.Y = (int)Math.Round(pocetak.Y * novaSkala / skala);
            kraj.X = (int)Math.Round(kraj.X * novaSkala / skala);
            kraj.Y = (int)Math.Round(kraj.Y * novaSkala / skala);
        }
        skala = novaSkala;
    }

    protected GrafObjekt(BinaryReader br)
    {
        int x, y;
        x = br.ReadInt32();
        y = br.ReadInt32();
        pocetak = new Point(x, y);
        x = br.ReadInt32();
        y = br.ReadInt32();
        kraj = new Point(x, y);
        skala = br.ReadSingle();
        ID = br.ReadInt32();
        objektSePrikazuje = true;
        // Atributi koji nisu inicijalizovani podrazumevano se inicijalizuju na
        // vrednosti 0 ili False, zavisno od tipa (zapravo, svaki konstruktor
        // najpre sve atribute inicijalizuje na vrednosti 0 ili False, pa tek onda
        // pocne da izvrsava naredbe koje cine telo konstruktora (i koje inicija-
        // lizuju atribute na konkretne vrednosti)).
    }

    public virtual void saveToStream(BinaryWriter bw)
    {
        bw.Write(pocetak.X);
        bw.Write(pocetak.Y);
        bw.Write(kraj.X);
        bw.Write(kraj.Y);
        bw.Write(skala);
        bw.Write(ID);
        // Atributi ObjektJeSelektovan, ObjektSePomera, ObjektSePrikazuje,
        // NonGridSkaliranje se ne smestaju na strim, jer se prilikom ucitavanja
        // objekta sa strima podrazumevano inicijalizuju na vrednosti False,
        // False, True, False, respektivno.
        // Atribut Sema se ne smesta na strim, jer ga prilikom ucitavanja objekta
        // sa strima inicijalizuje sema koja ucitava objekt sa strima
        // (postavlja ga na svoju vrednost).
        // RealPocetak i RealKraj se takodje ne smestaju na strim, jer se oni
        // i ne inicijalizuju prilikom ucitavanja sa strima, vec se inicijalizuju
        // tek kada Sema (a samim tim i objekt) udje u rezim NonGridSkaliranja.

        bw.Write(prikljucci.Count);
        foreach (Prikljucak p in prikljucci)
            p.saveToStream(bw);
    }
}

public class Opcije
{
    private int pinRazmak;
    private int duzinaPina;
    private Color bojaKola;
    private Color bojaPina;
    private Color bojaKrajaPina;
    private Color bojaVeze;
    private Color bojaCvora;
    private Color bojaSelekcije;
    private Color bojaPozadine;
    private Color gridDotColor;
    private int osetljivost;
    private bool gridDots;

    public int PinRazmak
    {
        get { return pinRazmak; }
        set { pinRazmak = value; }
    }

    public int DuzinaPina
    {
        get { return duzinaPina; }
        set { duzinaPina = value; }
    }

    public Color BojaKola
    {
        get { return bojaKola; }
        set { bojaKola = value; }
    }
        
    public Color BojaPina
    {
        get { return bojaPina; }
        set { bojaPina = value; }
    }

    public Color BojaKrajaPina
    {
        get { return bojaKrajaPina; }
        set { bojaKrajaPina = value; }
    }
        
    public Color BojaVeze
    {
        get { return bojaVeze; }
        set { bojaVeze = value; }
    }
       
    public Color BojaCvora
    {
        get { return bojaCvora; }
        set { bojaCvora = value; }
    }
        
    public Color BojaSelekcije
    {
        get { return bojaSelekcije; }
        set { bojaSelekcije = value; }
    }
        
    public Color BojaPozadine
    {
        get { return bojaPozadine; }
        set { bojaPozadine = value; }
    }
        
    public Color GridDotColor
    {
        get { return gridDotColor; }
        set { gridDotColor = value; }
    }
        
    public int Osetljivost
    {
        get { return osetljivost; }
        set { osetljivost = value; }
    }

    public bool GridDots
    {
        get { return gridDots; }
        set { gridDots = value; }
    }

    public Opcije(int pinRazmak, int duzinaPina, Color bojaKola, Color bojaPina, 
        Color bojaKrajaPina, Color bojaVeze, Color bojaCvora, Color bojaSelekcije,
        Color bojaPozadine, Color gridDotColor, int osetljivost, bool gridDots)
    {
        this.pinRazmak = pinRazmak;
        this.duzinaPina = duzinaPina;
        this.bojaKola = bojaKola;
        this.bojaPina = bojaPina;
        this.bojaKrajaPina = bojaKrajaPina;
        this.bojaVeze = bojaVeze;
        this.bojaCvora = bojaCvora;
        this.bojaSelekcije = bojaSelekcije;
        this.bojaPozadine = bojaPozadine;
        this.gridDotColor = gridDotColor;
        this.osetljivost = osetljivost;
        this.gridDots = gridDots;
    }

    public Opcije(BinaryReader br)
    {
        pinRazmak = br.ReadInt32();
        duzinaPina = br.ReadInt32();
        bojaKola = Color.FromArgb(br.ReadInt32());
        bojaPina = Color.FromArgb(br.ReadInt32());
        bojaKrajaPina = Color.FromArgb(br.ReadInt32());
        bojaVeze = Color.FromArgb(br.ReadInt32());
        bojaCvora = Color.FromArgb(br.ReadInt32());
        bojaSelekcije = Color.FromArgb(br.ReadInt32());
        bojaPozadine = Color.FromArgb(br.ReadInt32());
        gridDotColor = Color.FromArgb(br.ReadInt32());
        osetljivost = br.ReadInt32();
        gridDots = br.ReadBoolean();
    }

    public void SaveToStream(BinaryWriter bw)
    {
        bw.Write(pinRazmak);
        bw.Write(duzinaPina);
        bw.Write(bojaKola.ToArgb());
        bw.Write(bojaPina.ToArgb());
        bw.Write(bojaKrajaPina.ToArgb());
        bw.Write(bojaVeze.ToArgb());
        bw.Write(bojaCvora.ToArgb());
        bw.Write(bojaSelekcije.ToArgb());
        bw.Write(bojaPozadine.ToArgb());
        bw.Write(gridDotColor.ToArgb());
        bw.Write(osetljivost);
        bw.Write(gridDots);
    }
}

public class Segment : GrafObjekt
{
    protected Pravac pravac;

    public Segment()
    {

    }

    public Segment(Point pocetak, Point kraj, Sema sema, int ID)
    {
        init(pocetak, kraj, sema, ID);
    }

    protected new void init(Point pocetak, Point kraj, Sema sema, int AID)
    {
        base.init(pocetak, kraj, sema, AID);
        if (this.pocetak.Y == this.kraj.Y)
            pravac = Pravac.Hor;
        else
            pravac = Pravac.Vert;
    }

    public override void pomeri(int dx, int dy)
    {
        pocetak.Offset(dx, dy);
        kraj.Offset(dx, dy);
    }

    public override bool pogodjen(int x, int y)
    {
        int o = sema.dajOpcije().Osetljivost;
        if (pravac == Pravac.Hor)
        {
            if (pocetak.X < kraj.X)
                // Pocetak je ispred Kraja
                return (x >= pocetak.X - o) && (x <= kraj.X + o) &&
                        (y >= pocetak.Y - o) && (y <= pocetak.Y + o);
            else
                // Kraj je ispred Pocetka
                return (x >= kraj.X - o) && (x <= pocetak.X + o) &&
                        (y >= pocetak.Y - o) && (y <= pocetak.Y + o);
        }
        else  // Pravac = Vert
        {
            if (pocetak.Y < kraj.Y)
                return (y >= pocetak.Y - o) && (y <= kraj.Y + o) &&
                        (x >= pocetak.X - o) && (x <= pocetak.X + o);
            else
                return (y >= kraj.Y - o) && (y <= pocetak.Y + o) &&
                        (x >= pocetak.X - o) && (x <= pocetak.X + o);
        }
    }

    public override void nacrtaj(Graphics g, Point delta)
    {
        if (!prikazujeSe())
            return;

        g.DrawLine(sema.BojaVezePen, pocetak.X - delta.X, pocetak.Y - delta.Y,
            kraj.X - delta.X, kraj.Y - delta.Y);
    }

    public override Point krajnjeGoreLevo()
    {
        Point result = pocetak;
        if ((pravac == Pravac.Hor) && (pocetak.X > kraj.X) 
        || (pravac == Pravac.Vert) && (pocetak.Y > kraj.Y))
            // Kraj je ispred Pocetka
            result = kraj;
        return result;
    }

    public override Point krajnjeDoleDesno()
    {
        Point result = kraj;
        if ((pravac == Pravac.Hor) && (pocetak.X > kraj.X)
        || (pravac == Pravac.Vert) && (pocetak.Y > kraj.Y))
            result = pocetak;
        return result;
    }

    public override Point dajTeziste()
    {
        Point teziste = new Point();
        teziste.X = (pocetak.X + kraj.X) / 2;
        teziste.X = teziste.X - teziste.X % sema.dajPinRazmak();
        teziste.Y = (pocetak.Y + kraj.Y) / 2;
        teziste.Y = teziste.Y - teziste.Y % sema.dajPinRazmak();
        return teziste;
    }

    public override GrafObjekt dajKopiju()
    {
        return new Segment(pocetak, kraj, sema, ID);
    }

    public int getDuzina()
    {
        return Math.Abs(pocetak.X - kraj.X) + Math.Abs(pocetak.Y - kraj.Y);
    }

    public Pravac dajPravac()
    {
        return pravac;
    }

    public void postaviPravac(Pravac pravac)
    {
        if (getDuzina() == 0)
            this.pravac = pravac;
    }

    public override void rotiraj(Point centarRot, int smerRot)
    {
        base.rotiraj(centarRot, smerRot);
        pravac = Prostor.suprotanPravac(pravac);
    }

    public override void skaliraj(float novaSkala)
    {
        base.skaliraj(novaSkala);
    }

    public override void pocniNonGridSkaliranje()
    {
        base.pocniNonGridSkaliranje();
    }

    public Segment(BinaryReader br) : base(br)
    {
        pravac = (Pravac)br.ReadInt32();
    }

    public override void saveToStream(BinaryWriter bw)
    {
        base.saveToStream(bw);  // smesta na strim atribute nasledjene od GrafObjekt
        bw.Write((int)pravac);
    }
}

public class RastegljivSegment : Segment
{
    protected StepeniSlobode stepSlobodePoc;
    protected StepeniSlobode stepSlobodeKraj;
    protected Point poslednjiPomeraj;

    public RastegljivSegment()
    { 
    
    }

    public RastegljivSegment(Point pocetak, Point kraj, Sema sema, int ID,
        StepeniSlobode stepSlobodePoc, StepeniSlobode stepSlobodeKraj)
    {
        init(pocetak, kraj, sema, ID, stepSlobodePoc, stepSlobodeKraj);
    }

    protected void init(Point pocetak, Point kraj, Sema sema, int AID,
        StepeniSlobode stepSlobodePoc, StepeniSlobode stepSlobodeKraj)
    {
        base.init(pocetak, kraj, sema, AID);
        this.stepSlobodePoc = stepSlobodePoc;
        this.stepSlobodeKraj = stepSlobodeKraj;
    }

    // pomera krajeve segmenta relativno za (dx, dy) u onim pravcima u kojima
    // imaju slobodu da se krecu(sto odredjuju atributi StepSlobodePoc i
    // StepSlobodeKraj),i pazi da krajevi segmenta ne izadju izvan granica seme.
    // Dodatne provere su posledica cinjenice da kod segmenta, za razliku od
    // veze, nije uvek Pocetak ispred Kraja.
    public override void pomeri(int dx, int dy)
    {
        int dxPoc, dyPoc, dxKraj, dyKraj;
        dxPoc = dx;
        dyPoc = dy;
        dxKraj = dx;
        dyKraj = dy;
        if (!stepSlobodePoc.contains(Pravac.Hor))
            dxPoc = 0;
        if (!stepSlobodePoc.contains(Pravac.Vert))
            dyPoc = 0;
        if (!stepSlobodeKraj.contains(Pravac.Hor))
            dxKraj = 0;
        if (!stepSlobodeKraj.contains(Pravac.Vert))
            dyKraj = 0;
        if (pocetak.X + dxPoc < 0)
        {
            dxPoc = -pocetak.X;
            if (stepSlobodeKraj.contains(Pravac.Hor))
                dxKraj = dxPoc;
        }
        else if (pocetak.X + dxPoc > sema.dajSirinu() - 1)
        {
            dxPoc = sema.dajSirinu() - 1 - pocetak.X;
            if (stepSlobodeKraj.contains(Pravac.Hor))
                dxKraj = dxPoc;
        }
        if (pocetak.Y + dyPoc < 0)
        {
            dyPoc = -pocetak.Y;
            if (stepSlobodeKraj.contains(Pravac.Vert))
                dyKraj = dyPoc;
        }
        else if (pocetak.Y + dyPoc > sema.dajVisinu() - 1)
        {
            dyPoc = sema.dajVisinu() - 1 - pocetak.Y;
            if (stepSlobodeKraj.contains(Pravac.Vert))
                dyKraj = dyPoc;
        }
        if (kraj.X + dxKraj > sema.dajSirinu() - 1)
        {
            dxKraj = sema.dajSirinu() - 1 - kraj.X;
            if (stepSlobodePoc.contains(Pravac.Hor))
                dxPoc = dxKraj;
        }
        else if (kraj.X + dxKraj < 0)
        {
            dxKraj = -kraj.X;
            if (stepSlobodePoc.contains(Pravac.Hor))
                dxPoc = dxKraj;
        }
        if (kraj.Y + dyKraj > sema.dajVisinu() - 1)
        {
            dyKraj = sema.dajVisinu() - 1 - kraj.Y;
            if (stepSlobodePoc.contains(Pravac.Vert))
                dyPoc = dyKraj;
        }
        else if (kraj.Y + dyKraj < 0)
        {
            dyKraj = -kraj.Y;
            if (stepSlobodePoc.contains(Pravac.Vert))
                dyPoc = dyKraj;
        }
        pocetak.X = pocetak.X + dxPoc;
        pocetak.Y = pocetak.Y + dyPoc;
        kraj.X = kraj.X + dxKraj;
        kraj.Y = kraj.Y + dyKraj;
        if (dxPoc >= dxKraj)
            poslednjiPomeraj.X = dxPoc;
        else
            poslednjiPomeraj.X = dxKraj;
        if (dyPoc >= dyKraj)
            poslednjiPomeraj.Y = dyPoc;
        else
            poslednjiPomeraj.Y = dyKraj;
    }

    public override GrafObjekt dajKopiju()
    {
        return new RastegljivSegment(pocetak, kraj, sema, ID, stepSlobodePoc, stepSlobodeKraj);
    }

    public Point dajPoslednjiPomeraj()
    {
        return poslednjiPomeraj;
    }

    public StepeniSlobode dajStepSlobodePoc()
    {
        return stepSlobodePoc;
    }

    public StepeniSlobode dajStepSlobodeKraj()
    {
        return stepSlobodeKraj;
    }

    public void postaviStepSlobodePoc(StepeniSlobode stepSlobodePoc)
    {
        this.stepSlobodePoc = stepSlobodePoc;
    }

    public void postaviStepSlobodeKraj(StepeniSlobode stepSlobodeKraj)
    {
        this.stepSlobodeKraj = stepSlobodeKraj;
    }

    // postavlja stepene slobode za krajeve segmenta, i to tako da kraj
    // segmenta sa koordinatama pocetak dobija stepene slobode stepSlobodePoc
    // a drugi kraj stepene slobode stepSlobodeKraj
    public void postaviStepeneSlobode(StepeniSlobode stepSlobodePoc,
        StepeniSlobode stepSlobodeKraj, Point pocetak)
    {
        if (pocetak == this.pocetak)
        {
            this.stepSlobodePoc = stepSlobodePoc;
            this.stepSlobodeKraj = stepSlobodeKraj;
        }
        else
        {
            this.stepSlobodePoc = stepSlobodeKraj;
            this.stepSlobodeKraj = stepSlobodePoc;
        }
    }

    public RastegljivSegment(BinaryReader br)
        : base(br)
    {
        stepSlobodePoc = new StepeniSlobode(br);
        stepSlobodeKraj = new StepeniSlobode(br);
    }

    public override void saveToStream(BinaryWriter bw)
    {
        base.saveToStream(bw);
        stepSlobodePoc.SaveToStream(bw);
        stepSlobodeKraj.SaveToStream(bw);
    }
}

public abstract class DeoKola : Segment
{
    protected Kolo kolo;

    public DeoKola()
    {

    }

    public DeoKola(Point pocetak, Point kraj, Kolo kolo, int ID)
    {
        init(pocetak, kraj, kolo, ID);
    }

    protected void init(Point pocetak, Point kraj, Kolo kolo, int ID)
    {
        base.init(pocetak, kraj, kolo.dajSemu(), ID);
        this.kolo = kolo;
    }

    public override void pomeri(int dx, int dy)
    {
        pocetak.X = pocetak.X + dx;
        pocetak.Y = pocetak.Y + dy;
        kraj.X = kraj.X + dx;
        kraj.Y = kraj.Y + dy;
    }

    public Kolo dajKolo()
    {
        return kolo;
    }

    public void postaviKolo(Kolo kolo)
    {
        this.kolo = kolo;
    }

    public DeoKola (BinaryReader br) : base(br)
    {

    }

    public abstract string getClassId();
}

public class Prikljucak : GrafObjekt
{ 
    private Cvor cvor;
    private GrafObjekt graphObjekt;

    public Prikljucak(Point polozaj, Sema sema, int id)
        : base(polozaj, polozaj, sema, id)
    { 
    
    }

    public Cvor getCvor()
    {
        return cvor;
    }

    public void setCvor(Cvor cvor)
    {
        if (this.cvor != null)
            this.cvor.getPovezaniPrikljucci().Remove(this);
        this.cvor = cvor;
        if (this.cvor != null)
            this.cvor.getPovezaniPrikljucci().Add(this);
    }

    public GrafObjekt getGraphObject()
    {
        return graphObjekt;
    }

    public void setGraphObject(GrafObjekt graphObjekt)
    {
        if (this.graphObjekt != null)
            this.graphObjekt.getPrikljucci().Remove(this);
        this.graphObjekt = graphObjekt;
        if (this.graphObjekt != null)
            this.graphObjekt.getPrikljucci().Add(this);
    }

    public override void nacrtaj(Graphics g, Point delta)
    {
        throw new Exception("Method not implemented");
    }
    
    public override void pomeri(int dx, int dy)
    {
        pocetak.Offset(dx, dy);
        kraj.Offset(dx, dy);
    }
    
    public override bool pogodjen(int x, int y)
    {
        throw new Exception("Method not implemented");
    }
    
    public override Point krajnjeGoreLevo()
    {
        throw new Exception("Method not implemented");
    }
    
    public override Point krajnjeDoleDesno()
    {
        throw new Exception("Method not implemented");
    }
    
    public override Point dajTeziste()
    {
        throw new Exception("Method not implemented");
    }

    public override GrafObjekt dajKopiju()
    {
        throw new Exception("Method not implemented");
    }

    public override void saveToStream(BinaryWriter bw)
    {
        throw new Exception("Method not implemented");
    }

    public Prikljucak(BinaryReader br)
        : base(br)
    {
        throw new Exception("Method not implemented");
    }
}

public class Pin : Segment
{ 
    protected Smer smer;
    protected Kolo kolo;

    // koristi se samo u ProveriTopologiju    
    private List<Veza> vezeZaPomeranje;
    private List<Veza> vezeZaOdrzavanjeKontakta;


    // kreira pin na osnovu tacke pocetak koja predstavlja
    // kraj pina koji se nalazi uz kolo;
    public Pin(Point pocetak, Kolo kolo, int ID)
    {
        init(pocetak, kolo, ID);
    }

    protected void init(Point pocetak, Kolo kolo, int ID)
    { 
        Point kraj = new Point();
        int duzinaPina = kolo.dajSemu().dajDuzinuPina();
        int pinRazmak = kolo.dajSemu().dajPinRazmak();
        if (pocetak.X == kolo.dajPocetak().X)
        {
            // pin se nalazi sa leve strane
            kraj.X = pocetak.X - duzinaPina * pinRazmak;
            kraj.Y = pocetak.Y;
            smer = Smer.Zap;
        }
        else if (pocetak.X == kolo.dajKraj().X)
        {
            // sa desne strane
            kraj.X = pocetak.X + duzinaPina * pinRazmak;
            kraj.Y = pocetak.Y;
            smer = Smer.Ist;
        }
        else if (pocetak.Y == kolo.dajPocetak().Y)
        {
            // gore
            kraj.X = pocetak.X;
            kraj.Y = pocetak.Y - duzinaPina * pinRazmak;
            smer = Smer.Sev;
        }
        else 
        {
            // dole
            kraj.X = pocetak.X;
            kraj.Y = pocetak.Y + duzinaPina * pinRazmak;
            smer = Smer.Jug;
        }
        base.init(pocetak, kraj, kolo.dajSemu(), ID);
        this.kolo = kolo;

        Prikljucak p = new Prikljucak(kraj, kolo.dajSemu(), kolo.dajSemu().dajNoviID());
        p.setGraphObject(this);
    }
  
    public Kolo dajKolo()
    {
        return kolo;
    }

    public void postaviKolo(Kolo kolo)
    {
        this.kolo = kolo;
    }

    public Cvor dajCvor()
    {
        return getPrikljucci()[0].getCvor();
    }

    public Smer dajSmer()
    {
        return smer;
    }

    // spaja se na cvor
    public void spoji(Cvor cvor)
    {
        cvor.dodajPrikljucak(getPrikljucci()[0]);
    }

    // odspaja se sa cvora za koji je bio prikljucen;
    public void odspoji()
    {
        Cvor cvor = dajCvor();
        if (cvor != null)
            cvor.ukloniPrikljucak(getPrikljucci()[0]);
    }

    // vraca True ako je pin povezan sa jednom ili vise veza (ili pinova);
    public bool povezan()
    {
        bool result = false;
        Cvor cvor = dajCvor();
        if (cvor != null)
            result = cvor.dajBrojVeza() > 0 || cvor.dajBrojPinova() > 1;
        return result;
    }

    public override void nacrtaj(Graphics g, Point delta)
    {
        if (kolo.prikazujeSe())
        {
            if (!povezan() && !sema.isPrintPreview())
            {
                // crtaj kvadratic
                int duzina = (int)Math.Round(sema.dajPinRazmak() / 5.0);
                int levo = kraj.X - duzina - delta.X;
                int gore = kraj.Y - duzina - delta.Y;
                int desno = kraj.X + duzina - delta.X;
                int dole = kraj.Y + duzina - delta.Y;
                g.DrawLine(sema.BojaKrajaPinaPen, levo, gore, desno, gore);
                g.DrawLine(sema.BojaKrajaPinaPen, desno, gore, desno, dole);
                g.DrawLine(sema.BojaKrajaPinaPen, desno, dole, levo, dole);
                g.DrawLine(sema.BojaKrajaPinaPen, levo, dole, levo, gore);
            }
            if (kolo.selektovan())
            {
                g.DrawLine(sema.BojaSelekcijePen, pocetak.X - delta.X, pocetak.Y - delta.Y,
                    kraj.X - delta.X, kraj.Y - delta.Y);
            }
            else
            {
                g.DrawLine(sema.BojaPinaPen, pocetak.X - delta.X, pocetak.Y - delta.Y,
                    kraj.X - delta.X, kraj.Y - delta.Y);
            }
        }
    }

    public override void pomeri(int dx, int dy)
    {
        base.pomeri(dx, dy);
        getPrikljucci()[0].pomeri(dx, dy);
    }

    public override void rotiraj(Point centarRot, int smerRot)
    {
        base.rotiraj(centarRot, smerRot);
        smer = Prostor.rotirajSmer(smer, smerRot);
    }

    public void azurirajPokazivace()
    {

    }

    public void proveriTopologiju(List<Veza> vezeZaPomeranje, List<Veza> vezeZaOdrzavanjeKontakta)
    {
        this.vezeZaPomeranje = vezeZaPomeranje;
        this.vezeZaOdrzavanjeKontakta = vezeZaOdrzavanjeKontakta;
        Pin susPin;
        int i, j;
        Cvor susCvor, susSusCvor, susSusSusCvor;
        Veza veza, susVeza, susSusVeza;
        List<Veza> veze, vezeIzmedjuPinova;
        List<Pin> pinovi;
        bool naPinuIstogKola;

        veze = null;
        vezeIzmedjuPinova = null;
        pinovi = null;
        Cvor cvor = dajCvor();
        if (povezan() && cvor.dajBrojPinova() == 1)
        {
            veze = cvor.dajVeze();
            vezeIzmedjuPinova = new List<Veza>();
            for (i = 0; i < veze.Count; i++)
            {
                veza = veze[i];
                susCvor = veza.dajSuprotanCvor(cvor);
                if (susCvor.dajBrojPinova() > 0)
                {
                    pinovi = susCvor.dajPinove();
                    for (j = 0; j < pinovi.Count; j++)
                    {
                        susPin = pinovi[j];
                        if (susPin.dajKolo() == this.kolo)
                        {
                            vezeIzmedjuPinova.Add(veza);
                            topologija1(vezeIzmedjuPinova);
                            break;
                        }
                    }
                }
            }
            for (i = 0; i < vezeIzmedjuPinova.Count; i++)
                veze.Remove(vezeIzmedjuPinova[i]);
            if (veze.Count > 1)
                topologija2();
            else if (veze.Count == 1)
            {
                veza = veze[0];
                susCvor = veza.dajSuprotanCvor(cvor);
                if (susCvor.dajBrojPinova() > 0)
                {
                    if (veza.dajPravac() == this.pravac)
                        topologija3(veza, cvor.dajPocetak());
                    else
                        topologija4(veza, cvor.dajPocetak());
                }
                else
                {
                    if (susCvor.dajBrojVeza() == 1)
                    {
                        if (veza.dajPravac() == this.pravac)
                            topologija3(veza, cvor.dajPocetak());
                        else
                            topologija4(veza, cvor.dajPocetak());
                    }
                    else if (susCvor.dajBrojVeza() > 2)
                    {
                        if (veza.dajPravac() == this.pravac)
                            topologija5(veza, cvor);
                        else
                            topologija4(veza, cvor.dajPocetak());
                    }
                    else // dve veze u SusCvor;
                    {
                        veze = susCvor.dajVeze();
                        susVeza = veze[0];
                        if (susVeza == veza)
                            susVeza = veze[1];
                        susSusCvor = susVeza.dajSuprotanCvor(susCvor);
                        if (susSusCvor.dajBrojPinova() > 0)
                        {
                            naPinuIstogKola = false;
                            pinovi = susSusCvor.dajPinove();
                            for (i = 0; i < pinovi.Count; i++)
                            {
                                susPin = pinovi[i];
                                if (susPin.dajKolo() == this.kolo)
                                {
                                    topologija1(veze);
                                    naPinuIstogKola = true;
                                    break;
                                }
                            }
                            if (!naPinuIstogKola)
                            {
                                if (veza.dajPravac() == this.pravac)
                                    topologija6(veza, susVeza, cvor);
                                else
                                    topologija4(veza, cvor.dajPocetak());
                            }
                        }
                        else if (susSusCvor.dajBrojVeza() != 2)
                        {
                            if (veza.dajPravac() == this.pravac)
                                topologija6(veza, susVeza, cvor);
                            else
                                topologija4(veza, cvor.dajPocetak());
                        }
                        else  // dve veze u SusSusCvor
                        {
                            veze = susSusCvor.dajVeze();
                            susSusVeza = veze[0];
                            if (susSusVeza == susVeza)
                                susSusVeza = veze[1];
                            susSusSusCvor = susSusVeza.dajSuprotanCvor(susSusCvor);
                            naPinuIstogKola = false;
                            if (susSusSusCvor.dajBrojPinova() > 0)
                            {
                                pinovi = susSusSusCvor.dajPinove();
                                for (i = 0; i < pinovi.Count; i++)
                                {
                                    susPin = pinovi[i];
                                    if (susPin.dajKolo() == this.kolo)
                                    {
                                        veze.Add(veza);
                                        topologija1(veze);
                                        naPinuIstogKola = true;
                                        break;
                                    }
                                }
                            }
                            if (!naPinuIstogKola)
                            {
                                if (veza.dajPravac() == this.pravac)
                                    topologija6(veza, susVeza, cvor);
                                else
                                    topologija4(veza, cvor.dajPocetak());
                            }
                        }
                    }
                }
            }
        }

        i = 0;
        while (i < vezeZaPomeranje.Count)
        {
            for (j = vezeZaPomeranje.Count - 1; j > i; j--)
                if (vezeZaPomeranje[j] == vezeZaPomeranje[i])
                  vezeZaPomeranje.RemoveAt(j);
            i = i + 1;
        }
    }
    
    private void topologija1(List<Veza> veze)
    {
        for (int i = 0; i < veze.Count; i++)
        {
             Veza veza = veze[i];
             veza.postaviStepeneSlobode(
                 new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }), 
                 new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }),
                 veza.dajPrviCvor().dajPocetak());
             vezeZaPomeranje.Add(veza);
        }
    }

    private void topologija2()
    {
        Veza nulaVeza = new Veza(this.kraj, this.kraj, sema, 0,
           new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }),
           new StepeniSlobode(new Pravac[] { Prostor.suprotanPravac(this.dajPravac()) }));
        nulaVeza.postaviPravac(this.dajPravac());
        vezeZaOdrzavanjeKontakta.Add(nulaVeza);
        nulaVeza = new Veza(this.kraj, this.kraj, sema, 0,
            new StepeniSlobode(new Pravac[] { Prostor.suprotanPravac(this.dajPravac()) }),
            new StepeniSlobode(new Pravac[] { }));
        nulaVeza.postaviPravac(Prostor.suprotanPravac(this.dajPravac()));
        vezeZaOdrzavanjeKontakta.Add(nulaVeza);
    }

    private void topologija3(Veza veza, Point pocetak)
    {
        veza.postaviStepeneSlobode(new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }),
            new StepeniSlobode(new Pravac[] { Prostor.suprotanPravac(this.dajPravac()) }),
            pocetak);
        vezeZaPomeranje.Add(veza);
    }

    private void topologija4(Veza veza, Point pocetak)
    {
        veza.postaviStepeneSlobode(new StepeniSlobode(new Pravac[] { veza.dajPravac() }), 
            new StepeniSlobode(new Pravac[] { }), pocetak);
        vezeZaPomeranje.Add(veza);
        Veza nulaVeza = new Veza(pocetak, pocetak, sema, 0, 
            new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }),
            new StepeniSlobode(new Pravac[] { veza.dajPravac() }));
        nulaVeza.postaviPravac(Prostor.suprotanPravac(veza.dajPravac()));
        vezeZaOdrzavanjeKontakta.Add(nulaVeza);
    }

    private void topologija5(Veza veza, Cvor cvor)
    {
        veza.postaviStepeneSlobode(new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }), 
            new StepeniSlobode(new Pravac[] { Prostor.suprotanPravac(veza.dajPravac()) }),
            cvor.dajPocetak());
        vezeZaPomeranje.Add(veza);
        Cvor susCvor = veza.dajSuprotanCvor(cvor);
        Veza nulaVeza = new Veza(susCvor.dajPocetak(), susCvor.dajPocetak(), sema, 0, 
            new StepeniSlobode(new Pravac[] { Prostor.suprotanPravac(veza.dajPravac()) }),
            new StepeniSlobode(new Pravac[] { }));
        nulaVeza.postaviPravac(Prostor.suprotanPravac(veza.dajPravac()));
        vezeZaOdrzavanjeKontakta.Add(nulaVeza);
    }

    private void topologija6(Veza veza, Veza susVeza, Cvor cvor)
    {
        veza.postaviStepeneSlobode(new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }),
            new StepeniSlobode(new Pravac[] { Prostor.suprotanPravac(veza.dajPravac()) }),
            cvor.dajPocetak());
        vezeZaPomeranje.Add(veza);
        Cvor susCvor = veza.dajSuprotanCvor(cvor);
        susVeza.postaviStepeneSlobode(new StepeniSlobode(new Pravac[] { susVeza.dajPravac() }),
            new StepeniSlobode(new Pravac[] { }), susCvor.dajPocetak());
        vezeZaPomeranje.Add(susVeza);
    }

    public Pin(BinaryReader br) : base(br)
    {
        smer = (Smer)br.ReadByte();
    }

    public override void saveToStream(BinaryWriter bw)
    {
        base.saveToStream(bw);  // smesta nasledjene atribute na strim
        bw.Write((Byte)smer);
        // Atribut Kolo(pokazivac na kolo kojem pin pripada) se ne smesta na
        // strim, jer ga prilikom ucitavanja sa strima inicijalizuje kolo koje
        // ucitava pin sa strima
    }

    public virtual string getClassId()
    {
        return "Pin";
    }
}

public class Cvor : GrafObjekt
{ 
    private bool crtaSe;
    private int potencijal;
    private List<Prikljucak> povezaniPrikljucci = new List<Prikljucak>();

    public List<Prikljucak> getPovezaniPrikljucci()
    {
        return povezaniPrikljucci;
    }

    public void dodajPrikljucak(Prikljucak p)
    {
        if (povezaniPrikljucci.IndexOf(p) == -1)
        {
            p.setCvor(this);

            // azuriraj atribut crtaSe ako je potrebno
            if (p.getGraphObject() is Pin)
            {
                Pin pin = p.getGraphObject() as Pin;
                Smer smer = Prostor.suprotanSmer(pin.dajSmer());
                if (dajBrojVeza() + dajBrojPinova() > 2 || dajPin(pin.dajSmer()) != null
                || dajVezu(smer) != null)
                    // dajPin(pin.dajSmer()) != null je slucaj kada su dva pina jedan nasuprot
                    // drugom(crta se cvor da bi se raspoznavalo mesto kontakta)
                    // dajVezu(smer) != null je slucaj kada je veza poklopljena sa pinom
                    crtaSe = true;
            }
            else if (p.getGraphObject() is Veza)
            {
                Veza veza = p.getGraphObject() as Veza;
                if (dajBrojVeza() + dajBrojPinova() > 2 || dajPin(dajSmer(veza)) != null)
                    crtaSe = true;
                // dajPin(dajSmer(veza)) != null obradjuje slucaj kada su
                // veza i pin poklopljeni(tada se pin ne vidi
                // od veze pa se cvor crta da oznaci mesto kontakta)
            }

        }
    }

    public void ukloniPrikljucak(Prikljucak p)
    {
        if (povezaniPrikljucci.IndexOf(p) != -1)
        {
            p.setCvor(null);

            // azuriraj atribut crtaSe ako je potrebno
            if (p.getGraphObject() is Pin)
            {
                Pin pin = p.getGraphObject() as Pin;
                Smer suprSmer = new Smer();
                Smer prethVert = new Smer();
                Smer sledVert = new Smer();
                Smer s;
                if (dajBrojVeza() + dajBrojPinova() < 3)
                {
                    crtaSe = false;
                    switch (dajBrojPinova())
                    {
                        case 0:
                            if (dajBrojVeza() == 0)
                                sema.ukloniCvor(this); // nema ni veza ni pinova
                            else if (dajBrojVeza() == 2)
                            {
                                Smer smer = pin.dajSmer();
                                Prostor.orijentisi(smer, ref suprSmer, ref prethVert, ref sledVert);
                                if (dajVezu(prethVert) != null && dajVezu(sledVert) != null)
                                    // ostale su dve veze koje se nadovezuju jedna na drugu
                                    sema.spojiVeze(dajVezu(prethVert), dajVezu(sledVert));
                                else if (dajVezu(smer) != null && dajVezu(suprSmer) != null)
                                    // isti slucaj
                                    sema.spojiVeze(dajVezu(smer), dajVezu(suprSmer));
                            }
                            break;

                        case 1:
                            if (dajBrojVeza() == 1)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    s = (Smer)i;
                                    if (dajPin(s) != null && dajVezu(s) != null)
                                        // ostali su veza i pin, medjusobno poklopljeni
                                        crtaSe = true;
                                }
                            }
                            break;

                        case 2:
                            for (int i = 0; i < 4; i++)
                            {
                                s = (Smer)i;
                                if (dajPin(s) != null && dajPin(Prostor.suprotanSmer(s)) != null)
                                    // dva pina su jedan naspram drugog
                                    crtaSe = true;
                            }
                            break;
                    }
                }
            }
            else if (p.getGraphObject() is Veza)
            {
                Veza veza = p.getGraphObject() as Veza;
                Smer suprSmer = new Smer();
                Smer prethVert = new Smer();
                Smer sledVert = new Smer();
                Smer s;
                if (dajBrojVeza() + dajBrojPinova() < 3)
                {
                    crtaSe = false;
                    switch (dajBrojPinova())
                    {
                        case 0:
                            if (dajBrojVeza() == 0) // u cvoru nema ni veza ni pinova
                                sema.ukloniCvor(this);
                            else if (dajBrojVeza() == 2)
                            {
                                Prostor.orijentisi(dajSmer(veza), ref suprSmer, ref prethVert, ref sledVert);
                                if (dajVezu(prethVert) != null && dajVezu(sledVert) != null)
                                    // ostale su dve veze koje se nadovezuju jedna na drugu
                                    sema.spojiVeze(dajVezu(prethVert), dajVezu(sledVert));
                            }
                            break;

                        case 1:
                            if (dajBrojVeza() == 1)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    s = (Smer)i;
                                    if (dajPin(s) != null && dajVezu(s) != null)
                                        // veza i pin su poklopljeni
                                        crtaSe = true;
                                }
                            }
                            break;

                        case 2:
                            for (int i = 0; i < 4; i++)
                            {
                                s = (Smer)i;
                                if (dajPin(s) != null && dajPin(Prostor.suprotanSmer(s)) != null)
                                    // dva pina su jedan naspram drugog
                                    crtaSe = true;
                            }
                            break;
                    }
                }
            }
        }
    }
 
    private List<int> pr = new List<int>();  // identifikatori prikljucaka pre
                                             //  stavljanja cvora na strim

    public Cvor (Point polozaj, Sema sema, int ID)
        : base(polozaj, polozaj, sema, ID)
    {
        // VAZNO!
        // Cvor je svako mesto gde se zavrsavaju veza ili pin;
        // cvor ima pokazivace na sve veze i pinove koji se zavrsavaju u njemu
        // (maksimalno 4 veze i 4 pina); glavna uloga cvora je da povezuje veze
        // i pinove, i to na sledeci nacin: svaka veza ima pokazivace na dva
        // cvora(koji odgovaraju njenim krajevima); ako veza hoce da sazna sa
        // kojim je vezama i pinovima povezana, sve sto treba da uradi je da od
        // jednog ili drugog (ili oba) cvora zatrazi listu svih veza ili pinova
        // koji se u njima zavrsavaju; analogno vazi i za pin: svaki pin ima
        // pokazivac na jedan cvor(koji odgovara njegovom kraju); ako pin hoce da
        // sazna sa kojom vezom (vezama) je povezan, treba da od cvora zatrazi
        // sve veze koje se u njemu zavrsavaju.
        // Sema ne drzi cvorove u obicnoj listi, vec su, za potrebe brzeg
        // pronalazenja cvorova, cvorovi organizovani u mrezu, gde svaki cvor ima
        // pokazivace na 4 susedna cvora(atribut Sused).
        // Takodje, cvor ima i dodatnu ulogu da se, u zavisnosti od broja veza i
        // pinova koji se u njemu zavrsavaju, crta na ekranu u obliku kruzica
        // (cvor se crta ako se u njemu zavrsavaju vise od dve veze ili pina).
    }

    public override void nacrtaj(Graphics g, Point delta)
    {
        if (prikazujeSe() && crtaSe)
        {
            g.FillEllipse(sema.BojaCvoraBrush, pocetak.X - 2 - delta.X, 
                pocetak.Y - 2 - delta.Y, 4, 4);
        }
    }

    public override bool pogodjen(int x, int y)
    {
        int o = sema.dajOpcije().Osetljivost;
        return (Math.Abs(x - pocetak.X) <= o) && (Math.Abs(y - pocetak.Y) <= o);
    }

    public override void pomeri(int dx, int dy)
    { 
    
    }

    public override Point krajnjeGoreLevo()
    {
        return pocetak;
    }

    public override Point krajnjeDoleDesno()
    {
        return pocetak;
    }

    public override Point dajTeziste()
    {
        return pocetak;
    }

    public override GrafObjekt dajKopiju()
    {
        return null;
    }

    public int dajBrojVeza()
    {
        return dajVeze().Count;
    }

    public int dajBrojPinova()
    {
        return dajPinove().Count;
    }

    public List<Veza> dajVeze()
    {
        List<Veza> result = new List<Veza>();
        foreach (Prikljucak p in povezaniPrikljucci)
        {
            Veza veza = p.getGraphObject() as Veza;
            if (veza != null)
                result.Add(veza);
        }
        return result;
    }

    public List<Pin> dajPinove()
    {
        List<Pin> result = new List<Pin>();
        foreach (Prikljucak p in povezaniPrikljucci)
        {
            Pin pin = p.getGraphObject() as Pin;
            if (pin != null)
                result.Add(pin);
        }
        return result;
    }

    public Veza dajVezu(Smer smer)
    {
        List<Veza> veze = dajVeze();
        foreach (Veza v in veze)
        {
            if (dajSmer(v) == smer)
                return v;
        }
        return null;
    }

    private Smer dajSmer(Veza veza)
    {
        Point thisPoint = dajPocetak();
        Point other = veza.dajPocetak();
        if (other == thisPoint)
            other = veza.dajKraj();
        if (veza.dajPravac() == Pravac.Hor)
        {
            if (thisPoint.X < other.X)
                return Smer.Ist;
            else
                return Smer.Zap;
        }
        else
        {
            if (thisPoint.Y < other.Y)
                return Smer.Jug;
            else
                return Smer.Sev;
        }
    }

    // ako postoji jedna selektovana veza, vraca tu vezu; inace vraca vezu
    // koja je najbliza tacki (x, y).
    public Veza dajVezu(int x, int y)
    {
        Smer horSmer, vertSmer;
        Smer smerVeze = new Smer();
        if (dajBrojVeza() == 0)
            return null;
        int brojSelVeza = 0;
        int index = -1;
        // trazi selektovane veze
        List<Veza> veze = dajVeze();
        for (int i = 0; i < veze.Count; i++)
        {
            if (veze[i].selektovan())
            {
                brojSelVeza = brojSelVeza + 1;
                index = i;
            }
        }
        if (brojSelVeza == 1)
            return veze[index];
        else
        {
            // HorSmer i VertSmer definisu kvadrant u kome se nalazi tacka (x, y)
            // (u odnosu na polozaj cvora); ako postoje veze(veza) koje dodiruju dati
            // kvadrant, vraca se najbliza od njih(nje); inace,(ako postoje samo veze
            // koje dodiruju suprotni kvadrant), vraca se prva od njih na koju se
            // naidje(sto nije sasvim korektno)
            horSmer = Smer.Ist;
            if (x < dajKoordinatu(Pravac.Hor))
                horSmer = Smer.Zap;
            vertSmer = Smer.Jug;
            if (y < dajKoordinatu(Pravac.Vert))
                vertSmer = Smer.Sev;
            if (dajVezu(horSmer) != null && dajVezu(vertSmer) != null)
            {
                // daj najblizu vezu
                smerVeze = horSmer;
                if (Math.Abs(x - dajKoordinatu(Pravac.Hor)) < Math.Abs(y - dajKoordinatu(Pravac.Vert)))
                    smerVeze = vertSmer;
                return dajVezu(smerVeze);
            }
            else if (dajVezu(horSmer) != null)
                return dajVezu(horSmer);
            else if (dajVezu(vertSmer) != null)
                return dajVezu(vertSmer);
            else
            {
                // daj prvu na koju naidjes
                if (veze.Count > 0)
                    return veze[0];
                else
                    return null;
            }
        }
    }

    public Pin dajPin(Smer smer)
    {
        Smer smerPina = Prostor.suprotanSmer(smer);
        List<Pin> pinovi = dajPinove();
        foreach (Pin p in pinovi)
        {
            if (p.dajSmer() == smerPina)
                return p;
        }
        return null;
    }

    // proverava na koji je nacin veza koja se u datom cvoru nalazi u smeru
    // SmerVeze povezana sa ostatkom seme(preko tog cvora).
    // OdrziKontakt je True ako u cvoru postoji jos neka veza ili pin;
    // ako je OdrziKontakt True, JednaSusVeza je True ako postoji samo jedna
    // susedna veza i nijedan pin, i u tom slucaju SmerSusVeze daje smer te veze
    public void proveriTopologiju(Smer smerVeze, ref bool odrziKontakt, 
        ref bool jednaSusVeza, ref Smer smerSusVeze)
    {
        Smer suprSmer = new Smer(), prethVert = new Smer(), sledVert = new Smer();
        if (dajBrojPinova() > 0)
        {
            odrziKontakt = true;
            jednaSusVeza = false;
        }
        else
        {
            Prostor.orijentisi(smerVeze, ref suprSmer, ref prethVert, ref sledVert);
            if (dajBrojVeza() == 1) // || ((BrojVeza == 2) && PostojiVeza(SuprSmer))
                odrziKontakt = false;
            else
            {
                odrziKontakt = true;
                jednaSusVeza = false;
                if (dajBrojVeza() == 2)
                {
                    jednaSusVeza = true;
                    if (dajVezu(prethVert) != null)
                        smerSusVeze = prethVert;
                    else
                        smerSusVeze = sledVert;
                }
            }
        }
    }

    public void azurirajPokazivace()
    {
        // TODO
    }

    public override void skaliraj(float novaSkala)
    {
        base.skaliraj(novaSkala);
    }

    public int dajPotencijal()
    {
        return potencijal;
    }

    public void setPotencijal(int noviPotencijal)
    {
        potencijal = noviPotencijal;
    }

    public void sprovediPotencijal()
    {
        Smer s;
        for (int i = 0; i < 4; i++)
        {
            s = (Smer)i;
            if (dajVezu(s) != null)
            {
                Cvor susedanCvor = dajVezu(s).dajSuprotanCvor(this);
                if (susedanCvor.dajPotencijal() == 0)
                {
                    susedanCvor.setPotencijal(this.potencijal);
                    susedanCvor.sprovediPotencijal();
                }
            }
        }
    }

    public Cvor (BinaryReader br) : base(br)
    {
        crtaSe = br.ReadBoolean();

        int brojPrikljucaka = br.ReadInt32();
        for (int i = 0; i < brojPrikljucaka; i++)
        {
            pr.Add(br.ReadInt32());
        }
    }

    public override void saveToStream(BinaryWriter bw)
    {
        base.saveToStream(bw); // smesta nasledjene atribute na strim
        bw.Write(crtaSe);

        bw.Write(povezaniPrikljucci.Count);
        foreach (Prikljucak p in povezaniPrikljucci)
            bw.Write(p.dajID());
    }

    public int dajKoordinatu(Pravac pravac)
    {
        if (pravac == Pravac.Hor)
            return pocetak.X;
        else
            return pocetak.Y;
    }
}

public class Veza : RastegljivSegment
{
    // kreira vezu tako da joj je pocetak ispred kraja(posmatrano u pravcu veze),
    // i zatim je spaja za odgovarajuce cvorove
    public Veza(Cvor prviCvor, Cvor drugiCvor, int ID)
    {
        init(prviCvor, drugiCvor, ID);
    }

    protected void init(Cvor prviCvor, Cvor drugiCvor, int ID)
    {
        if (prviCvor.dajPocetak().Y == drugiCvor.dajPocetak().Y)
            pravac = Pravac.Hor;
        else
            pravac = Pravac.Vert;

        if (prviCvor.dajKoordinatu(pravac) > drugiCvor.dajKoordinatu(pravac))
        {
            Cvor tmp = prviCvor;
            prviCvor = drugiCvor;
            drugiCvor = tmp;
        }
        
        base.init(prviCvor.dajPocetak(), drugiCvor.dajPocetak(), prviCvor.dajSemu(), ID,
            new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }),
            new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }));

        Prikljucak p1 = new Prikljucak(prviCvor.dajPocetak(), sema, sema.dajNoviID());
        p1.setGraphObject(this);
        Prikljucak p2 = new Prikljucak(drugiCvor.dajPocetak(), sema, sema.dajNoviID());
        p2.setGraphObject(this);

        // prikljuci se na semu
        Smer prethSmer = new Smer(), sledSmer = new Smer();
        Prostor.orijentisi(pravac, ref prethSmer, ref sledSmer);
        prviCvor.dodajPrikljucak(getPrikljucci()[0]);
        drugiCvor.dodajPrikljucak(getPrikljucci()[1]);
    }

    public Veza(Point pocetak, Point kraj, Sema sema, int ID,
        StepeniSlobode stepSlobodePoc, StepeniSlobode stepSlobodeKraj)
    {
        Prostor.order(ref pocetak, ref kraj);
        base.init(pocetak, kraj, sema, ID, stepSlobodePoc, stepSlobodeKraj);

        Prikljucak p1 = new Prikljucak(pocetak, sema, sema.dajNoviID());
        p1.setGraphObject(this);
        Prikljucak p2 = new Prikljucak(kraj, sema, sema.dajNoviID());
        p2.setGraphObject(this);
    }

    public Veza(Point pocetak, Point kraj, Sema sema, int ID)
    {
        Prostor.order(ref pocetak, ref kraj);
        base.init(pocetak, kraj, sema, ID, 
            new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }),
            new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }));

        Prikljucak p1 = new Prikljucak(pocetak, sema, sema.dajNoviID());
        p1.setGraphObject(this);
        Prikljucak p2 = new Prikljucak(kraj, sema, sema.dajNoviID());
        p2.setGraphObject(this);
    }

    public Cvor dajPrviCvor()
    {
        return getPrikljucci()[0].getCvor();
    }

    public Cvor dajDrugiCvor()
    {
        return getPrikljucci()[1].getCvor();
    }

    public Cvor dajSuprotanCvor(Cvor cvor)
    {
        if (cvor == dajPrviCvor())
            return dajDrugiCvor();
        else if (cvor == dajDrugiCvor())
            return dajPrviCvor();
        else
            return null;
    }

    public override void nacrtaj(Graphics g, Point delta)
    {
        if (!prikazujeSe())
            return;
        if (selektovan())
        {
            g.DrawLine(sema.BojaSelekcijePen, pocetak.X - delta.X, pocetak.Y - delta.Y,
                kraj.X - delta.X, kraj.Y - delta.Y);

            // nacrtaj kvadrate na krajevima
            int r = (int)Math.Round(sema.dajPinRazmak() * 3.0 / 10);
            if (r > 3)
                r = 3;
            g.FillRectangle(sema.BojaSelekcijeBrush, pocetak.X - r - delta.X, pocetak.Y - r - delta.Y,
                2 * r + 1, 2 * r + 1);
            g.FillRectangle(sema.BojaSelekcijeBrush, kraj.X - r - delta.X, kraj.Y - r - delta.Y,
                2 * r + 1, 2 * r + 1);
        }
        else
        {
            //if PomeraSe then
            //    Pen.Mode := pmNot;
            g.DrawLine(sema.BojaVezePen, pocetak.X - delta.X, pocetak.Y - delta.Y,
                kraj.X - delta.X, kraj.Y - delta.Y);
        }
    }

    public override void pomeri(int dx, int dy)
    {
        base.pomeri(dx, dy);
        Point pomeraj = dajPoslednjiPomeraj();
        getPrikljucci()[0].pomeri(pomeraj.X, pomeraj.Y);
        getPrikljucci()[1].pomeri(pomeraj.X, pomeraj.Y);
    }

    public bool pogodjena(int x, int y)
    {
        Point pocetak = this.pocetak;
        Point kraj = this.kraj;
        Prostor.order(ref pocetak, ref kraj);


        int O = sema.dajOpcije().Osetljivost;
        if (pravac == Pravac.Hor)
            return (x > pocetak.X + O) && (x < kraj.X - O) &&
                    (y >= pocetak.Y - O) && (y <= pocetak.Y + O);
        else
            return (y > pocetak.Y + O) && (y < kraj.Y - O) &&
                    (x >= pocetak.X - O) && (x <= pocetak.X + O);
    }

    public void analizirajPovlacenje(List<Veza> vezeZaPomeranje, List<Veza> vezeZaOdrzavanjeKontakta)
    {
        Smer smerVezePr = new Smer(), smerVezeDr = new Smer(),
            smerSusVezePr = new Smer(), smerSusVezeDr = new Smer();
        bool odrziKontaktPr = false, odrziKontaktDr = false, 
            jednaSusVezaPr = false, jednaSusVezaDr = false;
        Veza susVeza;
        Prostor.orijentisi(pravac, ref smerVezeDr, ref smerVezePr);
        dajPrviCvor().proveriTopologiju(smerVezePr, ref odrziKontaktPr, ref jednaSusVezaPr,
            ref smerSusVezePr);
        dajDrugiCvor().proveriTopologiju(smerVezeDr, ref odrziKontaktDr, ref jednaSusVezaDr,
            ref smerSusVezeDr);
        StepeniSlobode stepeniSlobode = new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert});
        if (odrziKontaktPr || odrziKontaktDr)
            stepeniSlobode.Remove(pravac);
        postaviStepeneSlobode(stepeniSlobode, stepeniSlobode, pocetak);
        if (odrziKontaktPr)
        {
            if (jednaSusVezaPr)
            {
                susVeza = dajPrviCvor().dajVezu(smerSusVezePr);
                susVeza.postaviStepeneSlobode(stepeniSlobode, 
                    new StepeniSlobode(new Pravac[]{ }), pocetak);
                vezeZaPomeranje.Add(susVeza);
            }
            else
                vezeZaOdrzavanjeKontakta.Add(new Veza(pocetak, pocetak, sema,
                    0, new StepeniSlobode(new Pravac[]{ }), stepeniSlobode));
        }
        if (odrziKontaktDr)
        {
            if (jednaSusVezaDr)
            {
                susVeza = dajDrugiCvor().dajVezu(smerSusVezeDr);
                susVeza.postaviStepeneSlobode(stepeniSlobode, 
                    new StepeniSlobode(new Pravac[]{ }), kraj);
                vezeZaPomeranje.Add(susVeza);
            }
            else
                vezeZaOdrzavanjeKontakta.Add(new Veza(kraj, kraj, sema, 0,
                    new StepeniSlobode(new Pravac[] { }), stepeniSlobode));
        }
    }

    public void azurirajPokazivace()
    {

    }

    public override GrafObjekt dajKopiju()
    {
        return new Veza(pocetak, kraj, sema, 0, stepSlobodePoc,
            stepSlobodeKraj);
    }

    public Veza(BinaryReader br) : base(br)
    {

    }

    public override void saveToStream(BinaryWriter bw)
    {
        base.saveToStream(bw);  // smesta nasledjene atribute na strim
    }

    public bool containsExclusive(Point tacka)
    {
        if (pravac == Pravac.Hor)
        {
            if (tacka.Y != pocetak.Y)
                return false;
            return tacka.X > pocetak.X && tacka.X < kraj.X
                || tacka.X > kraj.X && tacka.X < pocetak.X;
        }
        else
        {
            if (tacka.X != pocetak.X)
                return false;
            return tacka.Y > pocetak.Y && tacka.Y < kraj.Y
                || tacka.Y > kraj.Y && tacka.Y < pocetak.Y;
        }
    }

    public bool overlaps(Point pocetak, Point kraj)
    {
        Pravac pravac = Prostor.getPravac(pocetak, kraj);
        if (this.pravac != pravac || pravac == Pravac.None)
            return false;

        Prostor.order(ref pocetak, ref kraj);
        Point thisPocetak = this.pocetak, thisKraj = this.kraj;
        Prostor.order(ref thisPocetak, ref thisKraj);

        if (pravac == Pravac.Hor)
        {
            return thisPocetak.Y == pocetak.Y
                && thisKraj.X > pocetak.X
                && thisPocetak.X < kraj.X;
        }
        else
        {
            return thisPocetak.X == pocetak.X
                && thisKraj.Y > pocetak.Y
                && thisPocetak.Y < kraj.Y;
        }
    }

    public void spoji(Cvor prviCvor, Cvor drugiCvor)
    {
        Smer prethSmer = new Smer(), sledSmer = new Smer();
        Prostor.orijentisi(pravac, ref prethSmer, ref sledSmer);
        if (prviCvor.dajKoordinatu(pravac) > drugiCvor.dajKoordinatu(pravac))
        {
            Smer tmp = prethSmer;
            prethSmer = sledSmer;
            sledSmer = tmp;
        }

        prviCvor.dodajPrikljucak(getPrikljucci()[0]);
        drugiCvor.dodajPrikljucak(getPrikljucci()[1]);
    }

    public void odspoji()
    {
        Cvor prviCvor = dajPrviCvor();
        if (prviCvor != null)
            prviCvor.ukloniPrikljucak(getPrikljucci()[0]);
        Cvor drugiCvor = dajDrugiCvor();
        if (drugiCvor != null)
            drugiCvor.ukloniPrikljucak(getPrikljucci()[1]);
    }
}

public abstract class Kolo : GrafObjekt
{
    protected List<DeoKola> delovi;
    protected List<Pin> pinovi;
    protected Smer smer;
    protected Point poslednjiPomeraj;

    public Kolo()
    {

    }

    public Kolo(Point goreLevo, Sema sema, int ID)
    {
        init(goreLevo, sema, ID);
    }

    protected void init(Point goreLevo, Sema sema, int ID)
    {
        base.init(goreLevo, goreLevo, sema, ID);
        smer = Smer.Ist;
        delovi = new List<DeoKola>();
        pinovi = new List<Pin>();
    }

    public override void pomeri(int dx, int dy)
    {
        if (krajnjeGoreLevo().X + dx < 0)
            dx = - krajnjeGoreLevo().X;
        if (krajnjeGoreLevo().Y + dy < 0)
            dy = -krajnjeGoreLevo().Y;
        if (krajnjeDoleDesno().X + dx > sema.dajSirinu() - 1)
            dx = sema.dajSirinu() - 1 - krajnjeDoleDesno().X;
        if (krajnjeDoleDesno().Y + dy > sema.dajVisinu() - 1)
            dy = sema.dajVisinu() - 1 - krajnjeDoleDesno().Y;
        pocetak.X = pocetak.X + dx;
        pocetak.Y = pocetak.Y + dy;
        kraj.X = kraj.X + dx;
        kraj.Y = kraj.Y + dy;
        for (int i = 0; i < delovi.Count; i++)
            delovi[i].pomeri(dx, dy);
        for (int i = 0; i < pinovi.Count; i++)
            pinovi[i].pomeri(dx, dy);
        poslednjiPomeraj = new Point(dx, dy);
    }

    public override bool pogodjen(int x, int y)
    {
        return (x >= pocetak.X) && (x <= kraj.X) &&
            (y >= pocetak.Y) && (y <= kraj.Y);
    }

    public override void nacrtaj(Graphics g, Point delta)
    {
        if (prikazujeSe())
        {
            for (int i = 0; i < pinovi.Count; i++)
                pinovi[i].nacrtaj(g, delta);
            for (int i = 0; i < delovi.Count; i++)
                delovi[i].nacrtaj(g, delta);
        }
    }

    public override Point dajTeziste()
    {
        Point teziste = new Point();
        teziste.X = (pocetak.X + kraj.X) / 2;
        teziste.X = teziste.X - teziste.X % sema.dajPinRazmak();
        teziste.Y = (pocetak.Y + kraj.Y) / 2;
        teziste.Y = teziste.Y - teziste.Y % sema.dajPinRazmak();
        return teziste;
    }

    // vraca pin kola sa identifikatorom Id; ako pin ne postoji, vraca null.
    public Pin dajPin(int id)
    {
        Pin pin = null;
        for (int I = 0; I < pinovi.Count; I++)
        {
            if (pinovi[I].dajID() == id)
            {
                pin = pinovi[I];
                break;
            }
        }
        return pin;
    }

    public List<DeoKola> dajDelove()
    {
        return delovi;
    }

    public List<Pin> dajPinove()
    {
        return pinovi;
    }

    public Smer dajSmer()
    {
        return smer;
    }

    public override void rotiraj(Point centarRot, int smerRot)
    {
        base.rotiraj(centarRot, smerRot);
        smer = Prostor.rotirajSmer(smer, smerRot);
        Point tmp = pocetak;
        if (smerRot == 1)
        {
            pocetak.Y = kraj.Y;
            kraj.Y = tmp.Y;
        }
        else
        {
            pocetak.X = kraj.X;
            kraj.X = tmp.X;
        }
        for (int i = 0; i < delovi.Count; i++)
            delovi[i].rotiraj(centarRot, smerRot);
        for (int i = 0; i < pinovi.Count; i++ )
            pinovi[i].rotiraj(centarRot, smerRot);
    }

    public void rotiraj(Smer smer)
    {
        Point centarRot = dajTeziste();
        while (this.smer != smer)
            rotiraj(centarRot, 1);
    }

    public void analizirajPovlacenje(List<Veza> vezeZaPomeranje, List<Veza> vezeZaOdrzavanjeKontakta)
    {
        int i;
        for (i = 0; i < pinovi.Count; i++)
        {
            pinovi[i].proveriTopologiju(vezeZaPomeranje, vezeZaOdrzavanjeKontakta);
        }
        i = 0;
        while (i < vezeZaPomeranje.Count)
        {
            for (int J = vezeZaPomeranje.Count - 1; J > i; J--)
            {
                if (vezeZaPomeranje[J] == vezeZaPomeranje[i])
                    vezeZaPomeranje.RemoveAt(J);
            }
            i = i + 1;
        }
    }

    public Point dajPoslednjiPomeraj()
    {
        return poslednjiPomeraj;
    }

    public void azurirajPokazivace()
    {
        for (int i = 0; i < pinovi.Count; i++)
            pinovi[i].azurirajPokazivace();
    }

    public new void postaviSemu(Sema sema)
    {
        this.sema = sema;
        for (int i = 0; i < delovi.Count; i++)
            delovi[i].postaviSemu(sema);
        for (int i = 0; i < pinovi.Count; i++)
            pinovi[i].postaviSemu(sema);
    }

    public override void skaliraj(float novaSkala)
    {
        base.skaliraj(novaSkala);
        for (int i = 0; i < delovi.Count; i++)
            delovi[i].skaliraj(novaSkala);
        for (int i = 0; i < pinovi.Count; i++)
            pinovi[i].skaliraj(novaSkala);
        skala = novaSkala;
    }

    public override void pocniNonGridSkaliranje()
    {
        base.pocniNonGridSkaliranje();
        for (int i = 0; i < delovi.Count; i++)
            delovi[i].pocniNonGridSkaliranje();
        for (int i = 0; i < pinovi.Count; i ++)
            pinovi[i].pocniNonGridSkaliranje();
    }

    public override void zavrsiNonGridSkaliranje()
    {
        base.zavrsiNonGridSkaliranje();
        for (int i = 0; i < delovi.Count; i++)
            delovi[i].zavrsiNonGridSkaliranje();
        for (int i = 0; i < pinovi.Count; i++)
            pinovi[i].zavrsiNonGridSkaliranje();
    }

    public abstract TipKola dajTip();
    public abstract int dajBrojUlaza();

    public Kolo(BinaryReader br) : base(br)
    {
        int id;
        DeoKola deoKola = null;
        Pin pin = null;
        smer = (Smer)br.ReadInt32();
        delovi = new List<DeoKola>();
        int brojDelova = br.ReadInt32();
        for (int i = 0; i < brojDelova; i++)  // ucitava delove sa strima
        {
            id = br.ReadInt32(); //prvo ucitava sifru dela kola
            if (id == 1)  // a zatim i odgovarajuci deo
                deoKola = new Linija(br);
            else if (id == 2)
                deoKola = new Luk(br);
            deoKola.postaviKolo(this);
            delovi.Add(deoKola);;
        }
        pinovi = new List<Pin>();
        int brojPinova = br.ReadInt32();
        for (int i = 0; i < brojPinova; i++)   // ucitava pinove sa strima
        {
            id = br.ReadInt32();  // prvo ucitava sifru pina
            if (id == 1)                  // a zatim i odgovarajuci pin
                pin = new Pin(br);
            else if (id == 2)
                pin = new InvertPin(br);
            pin.postaviKolo(this);
            pinovi.Add(pin);;
        }
    }

    public override void saveToStream(BinaryWriter bw)
    {
        int id = 0;
        base.saveToStream(bw);  // smesta nasledjene atribute na strim
        bw.Write((int)smer);
        int brojDelova = delovi.Count;
        bw.Write(brojDelova);
        for (int i = 0; i < delovi.Count; i++)  // smesta delove na strim
        {
            string imeKlase = delovi[i].getClassId();
            if (imeKlase == "Linija")
                id = 1;
            else if (imeKlase == "Luk")
                id = 2;
            bw.Write(id); //prvo smesta sifru dela kola na strim
            delovi[i].saveToStream(bw); // a zatim i deo kola
        }
        int brojPinova = pinovi.Count;
        bw.Write(brojPinova);
        for (int i = 0; i < pinovi.Count; i++)  // smesta pinove na strim
        {
            string imeKlase = pinovi[i].getClassId();
            if (imeKlase == "Pin")
                id = 1;
            else if (imeKlase == "InvertPin")
                id = 2;
            bw.Write(id); // prvo smesta sifru pina na strim
            pinovi[i].saveToStream(bw);  // a zatim i pin
        }
    }
}

public class Linija : DeoKola
{
    public Linija(Point pocetak, Point kraj, Kolo kolo, int ID)
        : base(pocetak, kraj, kolo, ID)
    {

    }

    public override void nacrtaj(Graphics g, Point delta)
    {
        if (kolo.prikazujeSe())
        {
            if (kolo.selektovan())
            {
                g.DrawLine(sema.BojaSelekcijePen, pocetak.X - delta.X, pocetak.Y - delta.Y,
                    kraj.X - delta.X, kraj.Y - delta.Y);
            }
            else
            {
                g.DrawLine(sema.BojaKolaPen, pocetak.X - delta.X, pocetak.Y - delta.Y,
                    kraj.X - delta.X, kraj.Y - delta.Y);
            }
        }
    }

    public Linija(BinaryReader br)
        : base(br)
    {

    }

    public override string getClassId()
    {
        return "Linija";
    }
}


public class Luk : DeoKola
{
    protected Smer smer;

    // kreira luk(odnosno poluelipsu) okrenut nadesno; GoreLevo i DoleDesno
    // su koordinate pravougaonika koji obuhvata luk; atributi luka Pocetak,
    // Kraj(nasledjeni od TGrafObjekt) predstavljaju koordinate pravougaonika koji 
    // obuhvata celu elipsu (ne samo luk)
    public Luk(Point goreLevo, Point doleDesno, Kolo kolo, int ID)
        : base(new Point(goreLevo.X - (doleDesno.X - goreLevo.X), goreLevo.Y), 
            doleDesno, kolo, ID)
    {
        smer = Smer.Ist;
    }

    public override void nacrtaj(Graphics g, Point delta)
    {
        if (!kolo.prikazujeSe())
            return;
        int startAngle = 0;
        switch (smer)
        {
            case Smer.Ist:
                startAngle = -90;
                break;

            case Smer.Jug:
                startAngle = 0;
                break;

            case Smer.Sev:
                startAngle = 180;
                break;

            case Smer.Zap:
                startAngle = 90;
                break;
        }
        if (kolo.selektovan())
        {
            g.DrawArc(sema.BojaSelekcijePen, pocetak.X - delta.X, pocetak.Y - delta.Y,
               Math.Abs(kraj.X - pocetak.X), Math.Abs(kraj.Y - pocetak.Y),
               startAngle, 180);
        }
        else
        {
            g.DrawArc(sema.BojaKolaPen, pocetak.X - delta.X, pocetak.Y - delta.Y,
               kraj.X - pocetak.X, kraj.Y - pocetak.Y,
               startAngle, 180);
        }
    }

    public override void rotiraj(Point centarRot, int smerRot)
    {
        base.rotiraj(centarRot, smerRot);
        smer = Prostor.rotirajSmer(smer, smerRot);
        Point tmp = pocetak;
        if (smerRot == 1)
        {
            pocetak.Y = kraj.Y;
            kraj.Y = tmp.Y;
        }
        else
        {
            pocetak.X = kraj.X;
            kraj.X = tmp.X;
        }
    }

    public Luk(BinaryReader br) : base(br)
    {
        smer = (Smer)br.ReadInt32();
    }

    public override void saveToStream(BinaryWriter bw)
    {
        base.saveToStream(bw); // smesta nasledjene atribute na strim
        bw.Write((int)smer);
    }

    public override string getClassId()
    {
        return "Luk";
    }
}

public class InvertPin : Pin
{
    public InvertPin(Point pocetak, Kolo kolo, int ID)
        : base(pocetak, kolo, ID)
    {

    }

    public InvertPin(BinaryReader br) : base(br)
    {

    }

    public override string getClassId()
    {
        return "InvertPin";
    }

    public override void nacrtaj(Graphics g, Point delta)
    {
        if (!prikazujeSe())
            return;

        if (!povezan() && !sema.isPrintPreview())
        {
            // crtaj kvadratic
            int duzina = (int)Math.Round(sema.dajPinRazmak() / 5.0);
            int levo = kraj.X - duzina - delta.X;
            int gore = kraj.Y - duzina - delta.Y;
            int desno = kraj.X + duzina - delta.X;
            int dole = kraj.Y + duzina - delta.Y;
            g.DrawLine(sema.BojaKrajaPinaPen, levo, gore, desno, gore);
            g.DrawLine(sema.BojaKrajaPinaPen, desno, gore, desno, dole);
            g.DrawLine(sema.BojaKrajaPinaPen, desno, dole, levo, dole);
            g.DrawLine(sema.BojaKrajaPinaPen, levo, dole, levo, gore);
        }
        Pen p;
        if (kolo.selektovan())
        {
            p = sema.BojaSelekcijePen;
        }
        else
        {
            p = sema.BojaPinaPen;
        }
        // crtaj pin i invertorski kruzic
        int R = (int)Math.Round(sema.dajPinRazmak() * 4 / 5.0);  // precnik
        int r = (int)Math.Round(R / 2.0);  // poluprecnik
        switch (smer)
        {
            case Smer.Ist:
                // r+1 centrira krug po osi pina
                g.DrawEllipse(p, pocetak.X - delta.X, pocetak.Y - r - delta.Y, R, 
                    2 * r + 1);
                g.DrawLine(p, pocetak.X + R - delta.X, pocetak.Y - delta.Y,
                    kraj.X - delta.X, kraj.Y - delta.Y);
                break;

            case Smer.Zap:
                g.DrawEllipse(p, pocetak.X - R - delta.X, pocetak.Y - r - delta.Y,
                    R, 2 * r + 1);
                g.DrawLine(p, pocetak.X - R - delta.X, pocetak.Y - delta.Y,
                    kraj.X - delta.X, kraj.Y - delta.Y);
                break;

            case Smer.Sev:
                g.DrawEllipse(p, pocetak.X - r - delta.X, pocetak.Y - R - delta.Y,
                    2 * r + 1, R);
                g.DrawLine(p, pocetak.X - delta.X, pocetak.Y - R - delta.Y,
                    kraj.X - delta.X, kraj.Y - delta.Y);
                break;

            case Smer.Jug:
                g.DrawEllipse(p, pocetak.X - r - delta.X, pocetak.Y - delta.Y,
                    2 * r + 1, R);
                g.DrawLine(p, pocetak.X - delta.X, pocetak.Y + R - delta.Y,
                    kraj.X - delta.X, kraj.Y - delta.Y);
                break;
        }
    }
}

public abstract class LogickoKolo : Kolo
{
    public LogickoKolo()
    {

    }

    public LogickoKolo(Point goreLevo, Sema sema, int ID)
    {
        init(goreLevo, sema, ID);
    }

    protected new void init (Point goreLevo, Sema sema, int ID)
    {
        base.init(goreLevo, sema, ID);
    }

    public override Point krajnjeGoreLevo()
    { 
        Point result = new Point();
        int duzinaPina = pinovi[0].getDuzina();
        if (smer == Smer.Ist || smer == Smer.Zap)
        {
            result.X = pocetak.X - duzinaPina;
            result.Y = pocetak.Y;
        }
        else
        {
           result.X = pocetak.X;
           result.Y = pocetak.Y - duzinaPina;
        }
        return result;
    }

    public override Point krajnjeDoleDesno()
    {
        Point result = new Point();
        int duzinaPina = pinovi[0].getDuzina();
        if (smer == Smer.Ist || smer == Smer.Zap)
        {
            result.X = kraj.X + duzinaPina;
            result.Y = kraj.Y;
        }
        else
        {
            result.X = kraj.X;
            result.Y = kraj.Y + duzinaPina;
        }
        return result;
    }

    public override int dajBrojUlaza()
    {
        return pinovi.Count - 1;
    }

    public LogickoKolo(BinaryReader br) : base(br)
    {

    }
}

public class Ne : LogickoKolo
{
    public Ne(Point goreLevo, Sema sema, int ID)
    {
        init(goreLevo, sema, ID);
    }

    protected new void init(Point goreLevo, Sema sema, int ID)
    {
        base.init(goreLevo, sema, ID);
        int pinRazmak = sema.dajPinRazmak();
        kraj.X = pocetak.X + 4 * pinRazmak;
        kraj.Y = pocetak.Y + 4 * pinRazmak;
        Point p1 = pocetak;
        Point p2 = new Point(pocetak.X, kraj.Y);
        Point p3 = new Point(kraj.X, kraj.Y - 2 * pinRazmak);
        delovi.Add(new Linija(p1, p2, this, 0));
        delovi.Add(new Linija(p3, p2, this, 0));
        delovi.Add(new Linija(p3, p1, this, 0));
        Point pinPocetak = new Point();
        pinPocetak.X = pocetak.X;
        pinPocetak.Y = pocetak.Y + 2 * pinRazmak;
        pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        pinPocetak.X = kraj.X;
        pinPocetak.Y = pocetak.Y + 2 * pinRazmak;
        pinovi.Add(new InvertPin(pinPocetak, this, sema.dajNoviID()));
        pomeri(0, 0);
    }

    public Ne(Point goreLevo, Sema sema, Smer smer, int ID)
    {
        init(goreLevo, sema, ID);
        rotiraj(smer);
        pomeri(goreLevo.X - pocetak.X, goreLevo.Y - pocetak.Y);
        pomeri(0, 0);
    }

    public override GrafObjekt dajKopiju()
    {
        return new Ne(pocetak, sema, smer, sema.dajNoviID());
    }

    public override TipKola dajTip()
    {
        return TipKola.Ne;
    }

    public Ne(BinaryReader br) : base(br)
    {

    }
}

public class I : LogickoKolo
{
    public I(Point goreLevo, int brojUlaza, Sema sema, int ID)
    {
        init(goreLevo, brojUlaza, sema, ID);
    }

    protected void init (Point goreLevo, int brojUlaza, Sema sema, int ID)
    {
        base.init(goreLevo, sema, ID);
        int pinRazmak = sema.dajPinRazmak();
        kraj.X = pocetak.X + 6 * pinRazmak;
        kraj.Y = pocetak.Y + 4 * pinRazmak;
        delovi.Add(new Linija(pocetak, new Point(pocetak.X, kraj.Y), this, 0));
        delovi.Add(new Luk(pocetak, kraj, this, 0));
        Point pinPocetak = new Point();
        pinPocetak.X = pocetak.X;
        pinPocetak.Y = pocetak.Y + pinRazmak;
        pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        pinPocetak.Y = pocetak.Y + 3 * pinRazmak;
        pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        pinPocetak.X = kraj.X;
        pinPocetak.Y = pocetak.Y + 2 * pinRazmak;
        pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        if (brojUlaza == 3)
        {
            pinPocetak.X = pocetak.X;
            pinPocetak.Y = pocetak.Y + 2 * pinRazmak;
            pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        }
        else if (brojUlaza == 4)
        {
            pinPocetak.X = pocetak.X;
            pinPocetak.Y = pocetak.Y;
            pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
            pinPocetak.Y = pocetak.Y + 4 * pinRazmak;
            pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        }
        pomeri(0, 0);
    }

    public I(Point goreLevo, int brojUlaza, Sema sema, Smer smer, int ID)
    {
        init(goreLevo, brojUlaza, sema, ID);
        rotiraj(smer);
        pomeri(goreLevo.X - pocetak.X, goreLevo.Y - pocetak.Y);
        pomeri(0, 0);
    }

    public override GrafObjekt dajKopiju()
    {
        return new I(pocetak, pinovi.Count - 1, sema, smer, sema.dajNoviID());
    }

    public override TipKola dajTip()
    {
      return TipKola.I;
    }

    public I(BinaryReader br) : base(br)
    {

    }
}

public class Ili : LogickoKolo
{
    public Ili(Point goreLevo, int brojUlaza, Sema sema, int ID)
    {
        init(goreLevo, brojUlaza, sema, ID);
    }

    protected void init(Point goreLevo, int brojUlaza, Sema sema, int ID)
    {
        base.init(goreLevo, sema, ID);
        int pinRazmak = sema.dajPinRazmak();
        // odredi donji desni kraj
        kraj.X = pocetak.X + 6 * pinRazmak;
        kraj.Y = pocetak.Y + 4 * pinRazmak;
        delovi.Add(new Luk(pocetak, kraj, this, 0)); // spoljasnji luk
        int zakrivljenost = 1 * pinRazmak; // zakrivljenost unutrasnjeg luka
        delovi.Add(new Luk(pocetak, new Point(pocetak.X + zakrivljenost, kraj.Y),
            this, 0)); // unutrasnji luk
        // parametri elipse unutrasnjeg luka
        int x1 = pocetak.X;
        int y1 = pocetak.Y + 2 * pinRazmak;
        int a = zakrivljenost;
        int b = 2 * pinRazmak;
        int y = pocetak.Y + pinRazmak;
        // presek elipse sa pravcem gornjeg levog pina
        int x = (int)Math.Round(x1 + a * Math.Sqrt(1 - Math.Pow((y - y1)/b, 2)));
        // linija koja spaja kraj gornjeg ulaznog pina sa unutrasnjim lukom
        delovi.Add(new Linija(new Point(pocetak.X, pocetak.Y + pinRazmak),
            new Point(x, pocetak.Y + pinRazmak), this, 0));
        // linija koja spaja kraj donjeg ulaznog pina sa unutrasnjim lukom
        delovi.Add(new Linija(new Point(pocetak.X, pocetak.Y + 3 * pinRazmak),
            new Point(x, pocetak.Y + 3 * pinRazmak), this, 0));
        // pocetak gornjeg ulaznog pina
        Point pinPocetak = new Point();
        pinPocetak.X = pocetak.X;
        pinPocetak.Y = pocetak.Y + pinRazmak;
        pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        // pocetak donjeg ulaznog pina
        pinPocetak.Y = pocetak.Y + 3 * pinRazmak;
        pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        // pocetak izlaznog pina
        pinPocetak.X = kraj.X;
        pinPocetak.Y = pocetak.Y + 2 * pinRazmak;
        pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        if (brojUlaza == 3)
        {
            delovi.Add(new Linija(new Point(pocetak.X, pocetak.Y + 2 * pinRazmak),
                new Point(pocetak.X + zakrivljenost, pocetak.Y + 2 * pinRazmak), this, 0));
            pinPocetak.X = pocetak.X;
            pinPocetak.Y = pocetak.Y + 2 * pinRazmak;
            pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        }
        else if (brojUlaza == 4)
        {
            pinPocetak.X = pocetak.X;
            pinPocetak.Y = pocetak.Y;
            pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
            pinPocetak.Y = pocetak.Y + 4 * pinRazmak;
            pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        }
        pomeri(0, 0); // ako je kolo unutar granica seme, poziv nema efekta;
                      // inace, pomera kolo unutar granica seme
    }

    public Ili(Point goreLevo, int brojUlaza, Sema sema, Smer smer, int ID)
    {
        init(goreLevo, brojUlaza, sema, ID);
        rotiraj(smer);
        pomeri(goreLevo.X - pocetak.X, goreLevo.Y - pocetak.Y);
        pomeri(0, 0);
    }

    public override GrafObjekt dajKopiju()
    {
        return new Ili(pocetak, pinovi.Count - 1, sema, smer, sema.dajNoviID());
    }

    public override TipKola dajTip()
    {
        return TipKola.Ili;
    }

    public Ili(BinaryReader br) : base(br)
    {

    }
}

public class Ni : LogickoKolo
{
    public Ni(Point goreLevo, int brojUlaza, Sema sema, int ID)
    {
        init(goreLevo, brojUlaza, sema, ID);
    }

    protected void init(Point goreLevo, int brojUlaza, Sema sema, int ID)
    {
        base.init(goreLevo, sema, ID);
        int pinRazmak = sema.dajPinRazmak();
        kraj.X = pocetak.X + 6 * pinRazmak;
        kraj.Y = pocetak.Y + 4 * pinRazmak;
        delovi.Add(new Linija(pocetak, new Point(pocetak.X, kraj.Y), this, 0));
        delovi.Add(new Luk(pocetak, kraj, this, 0));
        Point pinPocetak = new Point();
        pinPocetak.X = pocetak.X;
        pinPocetak.Y = pocetak.Y + pinRazmak;
        pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        pinPocetak.Y = pocetak.Y + 3 * pinRazmak;
        pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        pinPocetak.X = kraj.X;
        pinPocetak.Y = pocetak.Y + 2 * pinRazmak;
        pinovi.Add(new InvertPin(pinPocetak, this, sema.dajNoviID()));
        if (brojUlaza == 3)
        {
            pinPocetak.X = pocetak.X;
            pinPocetak.Y = pocetak.Y + 2 * pinRazmak;
            pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        }
        else if (brojUlaza == 4)
        {
            pinPocetak.X = pocetak.X;
            pinPocetak.Y = pocetak.Y;
            pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
            pinPocetak.Y = pocetak.Y + 4 * pinRazmak;
            pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        }
        pomeri(0, 0);
    }

    public Ni(Point goreLevo, int brojUlaza, Sema sema, Smer smer, int ID)
    { 
        init(goreLevo, brojUlaza, sema, ID);
        rotiraj(smer);
        pomeri(goreLevo.X - pocetak.X, goreLevo.Y  - pocetak.Y);
        pomeri(0, 0);
    }

    public override GrafObjekt dajKopiju()
    {
        return new Ni(pocetak, pinovi.Count - 1, sema, smer, sema.dajNoviID());
    }

    public override TipKola dajTip()
    {
        return TipKola.Ni;
    }

    public Ni(BinaryReader br) : base(br)
    {

    }
}

public class NIli : LogickoKolo
{
    public NIli(Point goreLevo, int brojUlaza, Sema sema, int ID)
    {
        init(goreLevo, brojUlaza, sema, ID);
    }

    protected void init(Point goreLevo, int brojUlaza, Sema sema, int ID)
    {
        base.init(goreLevo, sema, ID);
        int pinRazmak = sema.dajPinRazmak();
        kraj.X = pocetak.X + 6 * pinRazmak;
        kraj.Y = pocetak.Y + 4 * pinRazmak;
        delovi.Add(new Luk(pocetak, kraj, this, 0));
        int zakrivljenost = 1 * pinRazmak;
        delovi.Add(new Luk(pocetak, new Point(pocetak.X + zakrivljenost, kraj.Y),
            this, 0));
        int x1 = pocetak.X;
        int y1 = pocetak.Y + 2 * pinRazmak;
        int a = zakrivljenost;
        int b = 2 * pinRazmak;
        int y = pocetak.Y + pinRazmak;
        int x = (int)Math.Round(x1 + a * Math.Sqrt(1 - Math.Pow((y - y1)/b, 2)));
        delovi.Add(new Linija(new Point(pocetak.X, pocetak.Y + pinRazmak),
            new Point(x, pocetak.Y + pinRazmak), this, 0));
        delovi.Add(new Linija(new Point(pocetak.X, pocetak.Y + 3 * pinRazmak),
            new Point(x, pocetak.Y + 3 * pinRazmak), this, 0));
        Point pinPocetak = new Point();
        pinPocetak.X = pocetak.X;
        pinPocetak.Y = pocetak.Y + pinRazmak;
        pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        pinPocetak.Y = pocetak.Y + 3 * pinRazmak;
        pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        pinPocetak.X = kraj.X;
        pinPocetak.Y = pocetak.Y + 2 * pinRazmak;
        pinovi.Add(new InvertPin(pinPocetak, this, sema.dajNoviID()));
        if (brojUlaza == 3)
        {
            delovi.Add(new Linija(new Point(pocetak.X, pocetak.Y + 2 * pinRazmak),
                new Point(pocetak.X + zakrivljenost, pocetak.Y + 2 * pinRazmak), this, 0));
            pinPocetak.X = pocetak.X;
            pinPocetak.Y = pocetak.Y + 2 * pinRazmak;
            pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        }
        else if (brojUlaza == 4)
        {
            pinPocetak.X = pocetak.X;
            pinPocetak.Y = pocetak.Y;
            pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
            pinPocetak.Y = pocetak.Y + 4 * pinRazmak;
            pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        }
        pomeri(0, 0);
    }

    public NIli(Point goreLevo, int brojUlaza, Sema sema, Smer smer, int ID)
    {
        init(goreLevo, brojUlaza, sema, ID);
        rotiraj(smer);
        pomeri(goreLevo.X - pocetak.X, goreLevo.Y - pocetak.Y);
        pomeri(0, 0);
    }

    public override GrafObjekt dajKopiju()
    {
        return new NIli(pocetak, pinovi.Count - 1, sema, smer, sema.dajNoviID());
    }

    public override TipKola dajTip()
    {
        return  TipKola.Nili;
    }

    public NIli(BinaryReader br) : base(br)
    {

    }
}

public class ExIli : LogickoKolo
{
    public ExIli(Point goreLevo, int brojUlaza, Sema sema, int ID)
    {
        init(goreLevo, brojUlaza, sema, ID);
    }

    protected void init(Point goreLevo, int brojUlaza, Sema sema, int ID)
    {
        base.init(goreLevo, sema, ID);
        int pinRazmak = sema.dajPinRazmak();
        // odredi donji desni kraj
        kraj.X = pocetak.X + 6 * pinRazmak;
        kraj.Y = pocetak.Y + 4 * pinRazmak;
        Point poc = new Point(pocetak.X + pinRazmak / 2, pocetak.Y);// pocetak(gore levo)
                                                             // spoljasnjeg luka
        delovi.Add(new Luk(poc, kraj, this, 0)); // spoljasnji luk
        int zakrivljenost = 1 * pinRazmak; // zakrivljenost unutrasnjeg luka
        delovi.Add(new Luk(poc, new Point(poc.X + zakrivljenost, kraj.Y),
            this, 0)); // prvi(unutrasnji) unutrasnji luk
        delovi.Add(new Luk(pocetak, new Point(pocetak.X + zakrivljenost, kraj.Y),
            this, 0)); // drugi(spoljasnji) unutrasnji luk
        // parametri elipse drugog(spoljasnjeg) unutrasnjeg luka
        int x1 = pocetak.X;
        int y1 = pocetak.Y + 2 * pinRazmak;
        int a = zakrivljenost;
        int b = 2 * pinRazmak;
        int y = pocetak.Y + pinRazmak;
        // presek elipse sa pravcem gornjeg levog pina
        int x = (int)Math.Round(x1 + a * Math.Sqrt(1 - Math.Pow((y - y1)/b, 2)));
        // linija koja spaja kraj gornjeg ulaznog pina sa unutrasnjim lukom
        delovi.Add(new Linija(new Point(pocetak.X, pocetak.Y + pinRazmak),
            new Point(x, pocetak.Y + pinRazmak), this, 0));
        // linija koja spaja kraj donjeg ulaznog pina sa unutrasnjim lukom
        delovi.Add(new Linija(new Point(pocetak.X, pocetak.Y + 3 * pinRazmak),
            new Point(x, pocetak.Y + 3 * pinRazmak), this, 0));
        // pocetak gornjeg ulaznog pina
        Point pinPocetak = new Point();
        pinPocetak.X = pocetak.X;
        pinPocetak.Y = pocetak.Y + pinRazmak;
        pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        // pocetak donjeg ulaznog pina
        pinPocetak.Y = pocetak.Y + 3 * pinRazmak;
        pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        // pocetak izlaznog pina
        pinPocetak.X = kraj.X;
        pinPocetak.Y = pocetak.Y + 2 * pinRazmak;
        pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        if (brojUlaza == 3)
        {
            delovi.Add(new Linija(new Point(pocetak.X, pocetak.Y + 2 * pinRazmak),
                new Point(pocetak.X + zakrivljenost, pocetak.Y + 2 * pinRazmak), this, 0));
            pinPocetak.X = pocetak.X;
            pinPocetak.Y = pocetak.Y + 2 * pinRazmak;
            pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        }
        else if (brojUlaza == 4)
        {
            pinPocetak.X = pocetak.X;
            pinPocetak.Y = pocetak.Y;
            pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
            pinPocetak.Y = pocetak.Y + 4 * pinRazmak;
            pinovi.Add(new Pin(pinPocetak, this, sema.dajNoviID()));
        }
        pomeri(0, 0); // ako je kolo unutar granica seme, poziv nema efekta;
                      // inace, pomera kolo unutar granica seme
    }

    public ExIli(Point goreLevo, int brojUlaza, Sema sema, Smer smer, int ID)
    {
        init(goreLevo, brojUlaza, sema, ID);
        rotiraj(smer);
        pomeri(goreLevo.X - pocetak.X, goreLevo.Y - pocetak.Y);
        pomeri(0, 0);
    }

    public override GrafObjekt dajKopiju()
    {
        return new ExIli(pocetak, pinovi.Count - 1, sema, smer, sema.dajNoviID());
    }

    public override TipKola dajTip()
    {
        return TipKola.ExIli;
    }

    public ExIli(BinaryReader br) : base(br)
    {

    }
}

public class Blok : GrafObjekt
{
    protected List<GrafObjekt> objekti;
    protected Point poslednjiPomeraj;

    public Blok(Sema sema, int ID) : base(new Point(0, 0), new Point(0, 0), 
        sema, ID)
    {
        objekti = new List<GrafObjekt>();
        prikazi();
    }

    public override void nacrtaj(Graphics g, Point delta)
    {
        for (int i = 0; i < objekti.Count; i++)
            objekti[i].nacrtaj(g, delta);
    }

    // ne radi ispravno ako blok ne moze ceo da stane u semu (voditi o ovome
    // racuna kod rotiranja, gde ova situacija moze da se desi ako se rotira
    // velik blok)
    public override void pomeri(int dx, int dy)
    {
        if (pocetak.X + dx < 0)
            dx = - pocetak.X;
        if (pocetak.Y + dy < 0)
            dy = - pocetak.Y;
        if (kraj.X + dx > sema.dajSirinu() - 1)
            dx = sema.dajSirinu() - 1 - kraj.X;
        if (kraj.Y + dy > sema.dajVisinu() - 1)
            dy = sema.dajVisinu() - 1 - kraj.Y;
        pocetak.X = pocetak.X + dx;
        pocetak.Y = pocetak.Y + dy;
        kraj.X = kraj.X + dx;
        kraj.Y = kraj.Y + dy;
        for (int i = 0; i < objekti.Count; i++)
             objekti[i].pomeri(dx, dy);
        poslednjiPomeraj = new Point(dx, dy);
    }

    public override bool pogodjen(int x, int y)
    {
        return (x >= pocetak.X) && (x <= kraj.X) && (y >= pocetak.Y) && (y <= kraj.Y);
    }

    public override Point dajTeziste()
    {
        Point teziste = new Point();
        teziste.X = (pocetak.X + kraj.X) / 2;
        teziste.X = teziste.X - teziste.X % sema.dajPinRazmak();
        teziste.Y = (pocetak.Y + kraj.Y) / 2;
        teziste.Y = teziste.Y - teziste.Y % sema.dajPinRazmak();
        return teziste;
    }

    public override void rotiraj(Point centarRot, int smerRot)
    {
        base.rotiraj(centarRot, smerRot);
        Point tmp = pocetak;
        if (smerRot == 1)
        {
            pocetak.Y = kraj.Y;
            kraj.Y = tmp.Y;
        }
        else
        {
            pocetak.X = kraj.X;
            kraj.X = tmp.X;
        }
        for (int i = 0; i < objekti.Count; i++)
            objekti[i].rotiraj(centarRot, smerRot);
    }

    public override void skaliraj(float novaSkala)
    {
        base.skaliraj(novaSkala);
        for (int i = 0; i < objekti.Count; i++)
            objekti[i].skaliraj(novaSkala);
    }

    public override GrafObjekt dajKopiju()
    {
        Blok blok = new Blok(sema, 0);
        for (int i = 0; i < objekti.Count; i++)
            blok.dodajObjekt(objekti[i].dajKopiju());
        return blok;
    }

    public override Point krajnjeGoreLevo()
    {
        return pocetak;
    }

    public override Point krajnjeDoleDesno()
    {
        return kraj;
    }

    // dodaje Objekt u Blok, i azurira Pocetak i Kraj(tj. GoreLevo i DoleDesno)
    // ako je Objekt izvan granica
    public void dodajObjekt(GrafObjekt objekt)
    {
        Point pocetakObj = objekt.krajnjeGoreLevo();
        Point krajObj = objekt.krajnjeDoleDesno();
        if (pocetakObj.X > krajObj.X || pocetakObj.Y > krajObj.Y)
        {
            Point tmp = pocetakObj;
            pocetakObj = krajObj;
            krajObj = tmp;
        }
        if (objekti.Count == 0)
        {
            // inicijalizuj pocetak i kraj
            pocetak = pocetakObj;
            kraj = krajObj;
        }
        objekti.Add(objekt);
        if (pocetakObj.X < pocetak.X)
            pocetak.X = pocetakObj.X;
        if (pocetakObj.Y < pocetak.Y)
            pocetak.Y = pocetakObj.Y;
        if (krajObj.X > kraj.X)
            kraj.X = krajObj.X;
        if (krajObj.Y > kraj.Y)
            kraj.Y = krajObj.Y;
    }

    public void ukloniObjekt(GrafObjekt objekt)
    {
        objekti.Remove(objekt);
    }

    public new void pocniPomeranje()
    {
      base.pocniPomeranje();
      for (int i = 0; i < objekti.Count; i++)
          objekti[i].pocniPomeranje();
    }

    public new void zavrsiPomeranje()
    {
        base.zavrsiPomeranje();
        for (int i = 0; i < objekti.Count; i++)
            objekti[i].zavrsiPomeranje();
    }

    public bool canRotate()
    {
        Blok kopija = (Blok)dajKopiju();
        kopija.rotiraj(dajTeziste(), 1);
        kopija.pomeri(0, 0);
        if (kopija.unutar(new Rectangle(0, 0, sema.dajSirinu(), sema.dajVisinu()), true))
            return true;
        else
            return false;
    }

    public Point dajPoslednjiPomeraj()
    {
        return poslednjiPomeraj;
    }

    public List<GrafObjekt> dajObjekte()
    {
        return objekti;
    }
}

public abstract class Komanda
{
    protected StanjeKomande stanje;
    protected Sema sema;
    protected float skala;

    public Komanda(Sema sema)
    {
        this.sema = sema;
        skala = sema.dajSkalu();
        stanje = StanjeKomande.Kreirana;
    }

    public Komanda()
    {

    }

    public abstract void izvrsi();
    public abstract void undo();
    public abstract void skaliraj();

    public virtual void selektuj()
    { 
    
    }
}

public class KmdStaviVezu : Komanda
{
    private Veza veza;
    private bool pocCvoriste, krajCvoriste;

    public KmdStaviVezu(Point pocetak, Point kraj, Sema sema) : base(sema)
    {
        veza = new Veza(pocetak, kraj, sema, 0);
    }

    public KmdStaviVezu(Veza veza) : base(veza.dajSemu())
    {
        this.veza = veza;
    }

    public override void izvrsi()
    {
        if (veza.dajPocetak() != veza.dajKraj())
        {
            skaliraj();
            pocCvoriste = sema.ukrstanjeVeza(veza.dajPocetak());
            krajCvoriste = sema.ukrstanjeVeza(veza.dajKraj());
            sema.dodajVezu(veza);

            // TODO: Trebalo bi spojiti vezu sa susednim vezama ako se u pocetnom ili
            // krajnjem cvoru veza nadovezuje na susedne veze. Spajanje bi trebalo
            // realizovati u vidu nekog post-procesinga seme nakon sto se doda veza
            // (analiziralo bi se sta treba da se uradi). Videti u starom metodu
            // SpojiTacke kako se to radilo.
        }

        stanje = StanjeKomande.Izvrsena;
    }

    public override void undo()
    {
        if (veza.dajPocetak() != veza.dajKraj())
        {
            skaliraj();
            sema.ukloniVezu(sema.dajVezu(veza.dajPocetak(), veza.dajKraj()));
            if (krajCvoriste)
                sema.ukloniCvoriste(sema.dajCvor(veza.dajKraj()));
            if (pocCvoriste)
                sema.ukloniCvoriste(sema.dajCvor(veza.dajPocetak()));
        }
        stanje = StanjeKomande.Vracena;
    }

    public override void selektuj()
    {
        veza.selektuj();
    }

    public override void skaliraj()
    {
        float novaSkala = sema.dajSkalu();
        veza.skaliraj(novaSkala);
        skala = novaSkala;
    }
}

public class KmdUkloniVezu : Komanda
{
    private Veza veza;
    private Point pocetakVeze, krajVeze;

    public KmdUkloniVezu(Veza veza) : base(veza.dajSemu())
    {
        pocetakVeze = veza.dajPrviCvor().dajPocetak();
        krajVeze = veza.dajDrugiCvor().dajPocetak();
    }

    public override void izvrsi()
    {
        skaliraj();
        veza = sema.dajVezu(pocetakVeze, krajVeze);
        sema.ukloniVezu(veza);
        stanje = StanjeKomande.Izvrsena;
    }

    public override void undo()
    {
        skaliraj();
        sema.dodajVezu(veza);
        stanje = StanjeKomande.Vracena;
    }

    public override void skaliraj()
    {
        float novaSkala = sema.dajSkalu();
        if (stanje == StanjeKomande.Izvrsena)
            veza.skaliraj(novaSkala);
        pocetakVeze.X = (int)Math.Round(pocetakVeze.X * novaSkala/skala);
        pocetakVeze.Y = (int)Math.Round(pocetakVeze.Y * novaSkala/skala);
        krajVeze.X = (int)Math.Round(krajVeze.X * novaSkala/skala);
        krajVeze.Y = (int)Math.Round(krajVeze.Y * novaSkala/skala);
        skala = novaSkala;
    }

    public override void selektuj()
    {
        veza.selektuj();
    }
}

public abstract class KmdPomeri : Komanda
{
    public KmdPomeri(Sema sema) : base(sema)
    {

    }

    public abstract void pomeri(int dx, int dy);
    public abstract void pocniPomeranje();
    public abstract void zavrsiPomeranje();
    public abstract void ukloni();
    public abstract void stavi();
    public abstract void nacrtaj(Graphics g, Point delta);
}

public class KmdPomeriVezu : KmdPomeri
{
    private List<Veza> veze;
    private List<Komanda> ukloniVeze, staviVeze;

    public KmdPomeriVezu(Veza veza, bool povuci) : base(veza.dajSemu())
    {
        veza.postaviStepeneSlobode(
            new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }), 
            new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }),
            veza.dajPrviCvor().dajPocetak());
        List<Veza> vezeZaPomeranje = new List<Veza>();
        List<Veza> vezeZaOdrzavanjeKontakta = new List<Veza>();
        if (povuci)
            veza.analizirajPovlacenje(vezeZaPomeranje, vezeZaOdrzavanjeKontakta);
        ukloniVeze = new List<Komanda>();
        ukloniVeze.Add(new KmdUkloniVezu(veza));
        for (int i = 0; i < vezeZaPomeranje.Count; i++)
            ukloniVeze.Add(new KmdUkloniVezu(vezeZaPomeranje[i]));
        veze = new List<Veza>();
        veze.Add(veza.dajKopiju() as Veza);
        for (int i = 0; i < vezeZaPomeranje.Count; i++)
            veze.Add(vezeZaPomeranje[i].dajKopiju() as Veza);
        for (int i = 0; i < vezeZaOdrzavanjeKontakta.Count; i++)
            veze.Add(vezeZaOdrzavanjeKontakta[i].dajKopiju() as Veza);
        staviVeze = new List<Komanda>();
        for (int i = 0; i < veze.Count; i++)
            staviVeze.Add(new KmdStaviVezu(veze[i]));
    }

    public KmdPomeriVezu(Veza veza, Cvor cvor) : base(veza.dajSemu())
    {
        veze = new List<Veza>();
        ukloniVeze = new List<Komanda>();
        staviVeze = new List<Komanda>();
        Cvor susCvor = veza.dajSuprotanCvor(cvor);
        if (susCvor.dajBrojPinova() > 0 || susCvor.dajBrojVeza() != 2)
        {
            ukloniVeze.Add(new KmdUkloniVezu(veza));
            veza.postaviStepeneSlobode(
                new StepeniSlobode(new Pravac[] {veza.dajPravac()}), 
                new StepeniSlobode(new Pravac[] { }), cvor.dajPocetak());
            veze.Add(veza.dajKopiju() as Veza);
            veze.Insert(0, new Veza(cvor.dajPocetak(), cvor.dajPocetak(), sema, 0,
                new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }), 
                new StepeniSlobode(new Pravac[] {veza.dajPravac()})));
            staviVeze.Add(new KmdStaviVezu(veze[0]));
            staviVeze.Add(new KmdStaviVezu(veze[1]));
        }
        else
        {
            List<Veza> v = susCvor.dajVeze();
            Veza susVeza = v[0];
            if (susVeza == veza)
                susVeza = v[1];
            ukloniVeze.Add(new KmdUkloniVezu(veza));
            ukloniVeze.Add(new KmdUkloniVezu(susVeza));
            veza.postaviStepeneSlobode(
                new StepeniSlobode(new Pravac[] {Pravac.Hor, Pravac.Vert}), 
                new StepeniSlobode(new Pravac[] {susVeza.dajPravac()}),
                cvor.dajPocetak());
            susVeza.postaviStepeneSlobode(
                new StepeniSlobode(new Pravac[] {susVeza.dajPravac()}),
                new StepeniSlobode(new Pravac[] { }), 
                susCvor.dajPocetak());
            veze.Add(veza.dajKopiju() as Veza);
            veze.Add(susVeza.dajKopiju() as Veza);
            staviVeze.Add(new KmdStaviVezu(veze[0]));
            staviVeze.Add(new KmdStaviVezu(veze[1]));
        }
    }

    public override void pomeri(int dx, int dy)
    {
        for (int i = 0; i < veze.Count; i++)
            veze[i].pomeri(dx, dy);
    }

    public override void pocniPomeranje()
    {
        for (int i = 0; i < veze.Count; i++)
            veze[i].pocniPomeranje();
    }

    public override void zavrsiPomeranje()
    {
        for (int i = 0; i < veze.Count; i++)
            veze[i].zavrsiPomeranje();
    }

    public override void ukloni()
    {
        for (int i = 0; i < ukloniVeze.Count; i++)
            ukloniVeze[i].izvrsi();
    }

    public override void stavi()
    {
        for (int i = staviVeze.Count - 1; i >= 0; i--)
            staviVeze[i].izvrsi();
        if (veze[0].getDuzina() != 0)
            staviVeze[0].selektuj();
        else // samo za slucaj povlacenja veze za kraj
            staviVeze[1].selektuj();
        stanje = StanjeKomande.Izvrsena;
    }

    public override void izvrsi()
    {
        skaliraj();
        ukloni();
        stavi();
        stanje = StanjeKomande.Izvrsena;
    }

    public override void undo()
    {
        skaliraj();
        for (int i = 0; i < staviVeze.Count; i++)
            staviVeze[i].undo();
        for (int i = ukloniVeze.Count - 1; i >= 0; i--)
            ukloniVeze[i].undo();
        sema.deselektuj();
        stanje = StanjeKomande.Vracena;
    }

    public override void nacrtaj(Graphics g, Point delta)
    {
        for (int i = 0; i < veze.Count; i++)
            veze[i].nacrtaj(g, delta);
    }

    public override void skaliraj()
    {
        float novaSkala = sema.dajSkalu();
        for (int i = 0; i < ukloniVeze.Count; i++)
            ukloniVeze[i].skaliraj();
        for (int i = 0; i < staviVeze.Count; i++)
            staviVeze[i].skaliraj();
        skala = novaSkala;
    }
}

public class KmdStaviKolo : Komanda
{
    private Kolo kolo;
    private List<Point> cvorista;

    public KmdStaviKolo(Kolo kolo) : base(kolo.dajSemu())
    {
        this.kolo = kolo;
        cvorista = new List<Point>();
    }

    public override void izvrsi()
    {
        skaliraj();
        sema.dodajKolo(kolo, cvorista);
        stanje = StanjeKomande.Izvrsena;
    }

    public override void undo()
    {
        skaliraj();
        sema.ukloniKolo(kolo);
        for (int i = 0; i < cvorista.Count; i++)
        {
            sema.ukloniCvoriste(sema.dajCvor(cvorista[i]));
        }
        cvorista.Clear();
        stanje = StanjeKomande.Vracena;
    }

    public override void skaliraj()
    {
        float novaSkala = sema.dajSkalu();
        kolo.skaliraj(novaSkala);
        for (int i = 0; i < cvorista.Count; i++)
        {
            Point polozaj = cvorista[i];
            int x = (int)Math.Round(polozaj.X * novaSkala/skala);
            int y = (int)Math.Round(polozaj.Y * novaSkala / skala);
            cvorista[i] = new Point(x, y);
        }
        skala = novaSkala;
    }

    public override void selektuj()
    {
        kolo.selektuj();
    }
}

public class KmdUkloniKolo : Komanda
{
    private Kolo kolo;
    private bool oslobodiKolo;
    private List<Point> neTreba;

    public KmdUkloniKolo(Kolo kolo, bool oslobodiKolo) : base(kolo.dajSemu())
    {
        this.kolo = kolo;
        this.oslobodiKolo = oslobodiKolo;
        neTreba = new List<Point>();
    }

    public override void izvrsi()
    {
        skaliraj();
        sema.ukloniKolo(kolo);
        stanje = StanjeKomande.Izvrsena;
    }

    public override void undo()
    {
        skaliraj();
        sema.dodajKolo(kolo, neTreba);
        stanje = StanjeKomande.Vracena;
    }

    public override void skaliraj()
    {
        float novaSkala = sema.dajSkalu();
        kolo.skaliraj(novaSkala);
        skala = novaSkala;
    }

    public override void selektuj()
    {
        kolo.selektuj();
    }
}

public class KmdPomeriKolo : KmdPomeri
{
    private Kolo kolo;
    private List<Veza> veze;
    private List<Komanda> kmdUkloni;
    private List<Komanda> kmdStavi;
    int dx, dy;

    public KmdPomeriKolo(Kolo kolo, bool povuci) : base(kolo.dajSemu())
    {
        this.kolo = kolo;
        kmdUkloni = new List<Komanda>();
        kmdStavi = new List<Komanda>();
        kmdUkloni.Add(new KmdUkloniKolo(kolo, false));
        kmdStavi.Add(new KmdStaviKolo(kolo));
        List<Veza> vezeZaPomeranje = new List<Veza>();
        List<Veza> vezeZaOdrzavanjeKontakta = new List<Veza>();
        if (povuci)
            kolo.analizirajPovlacenje(vezeZaPomeranje, vezeZaOdrzavanjeKontakta);
        for (int i = 0; i < vezeZaPomeranje.Count; i++)
            kmdUkloni.Add(new KmdUkloniVezu(vezeZaPomeranje[i]));
        veze = new List<Veza>();
        for (int i = 0; i < vezeZaPomeranje.Count; i++)
            veze.Add(vezeZaPomeranje[i].dajKopiju() as Veza);
        for (int i = 0; i < vezeZaOdrzavanjeKontakta.Count; i++)
            veze.Add(vezeZaOdrzavanjeKontakta[i].dajKopiju() as Veza);
        for (int i = 0; i < veze.Count; i++ )
            kmdStavi.Add(new KmdStaviVezu(veze[i]));
    }

    public override void pomeri(int adx, int ady)
    {
        kolo.pomeri(adx, ady);
        Point pomeraj = kolo.dajPoslednjiPomeraj();
        for (int i = 0; i < veze.Count; i++)
            veze[i].pomeri(pomeraj.X, pomeraj.Y);
        dx = dx + pomeraj.X;
        dy = dy + pomeraj.Y;
    }

    public override void pocniPomeranje()
    {
      kolo.pocniPomeranje();
      for (int i = 0; i < veze.Count; i++)
          veze[i].pocniPomeranje();
    }

    public override void zavrsiPomeranje()
    {
        kolo.zavrsiPomeranje();
        for (int i = 0; i < veze.Count; i++ )
            veze[i].zavrsiPomeranje();
    }

    public override void ukloni()
    {
        for (int i = 0; i < kmdUkloni.Count; i++)
            kmdUkloni[i].izvrsi();
    }

    public override void stavi()
    {
        for (int i = 0; i < kmdStavi.Count; i++)
            kmdStavi[i].izvrsi();
        kolo.selektuj();
        stanje = StanjeKomande.Izvrsena;
    }

    public override void izvrsi()
    {
        skaliraj();
        ukloni();
        kolo.pomeri(dx, dy);
        stavi();
        stanje = StanjeKomande.Izvrsena;
    }

    public override void undo()
    {
        skaliraj();
        for (int i = kmdStavi.Count - 1; i >=  0; i--)
            kmdStavi[i].undo();
        kolo.pomeri(-dx, -dy);
        for (int i = kmdUkloni.Count - 1; i >= 0; i--)
            kmdUkloni[i].undo();
        kolo.selektuj();
        stanje = StanjeKomande.Vracena;
    }

    public override void skaliraj()
    {
        float novaSkala = sema.dajSkalu();
        kolo.skaliraj(novaSkala);
        dx = (int)Math.Round(dx * novaSkala/skala);
        dy = (int)Math.Round(dy * novaSkala/skala);
        for (int i = 0; i < kmdUkloni.Count; i++)
            kmdUkloni[i].skaliraj();
        for (int i = 0; i < kmdStavi.Count; i++)
            kmdStavi[i].skaliraj();
        skala = novaSkala;
    }

    public override void nacrtaj(Graphics g, Point delta)
    {
        kolo.nacrtaj(g, delta);
        for (int i = 0; i < veze.Count; i++)
            veze[i].nacrtaj(g, delta);
    }
}

public class KmdPomeriGrupu : KmdPomeri
{
    private Blok blok;
    private List<Komanda> kmdUkloni, kmdStavi;
    private int dx, dy;

    public KmdPomeriGrupu(List<GrafObjekt> grupa) : base(grupa[0].dajSemu())
    {
        blok = new Blok(sema, 0);
        kmdUkloni = new List<Komanda>();
        kmdStavi = new List<Komanda>();
        for (int i = 0; i < grupa.Count; i++)
        {
            GrafObjekt grafObjekt = grupa[i];
            if (grafObjekt is Kolo)
            {
                Kolo kolo = grafObjekt as Kolo;
                blok.dodajObjekt(kolo);
                kmdUkloni.Add(new KmdUkloniKolo(kolo, false));
                kmdStavi.Add(new KmdStaviKolo(kolo));
            }
            else if (grafObjekt is Veza)
            {
                Veza veza = grafObjekt as Veza;
                veza.postaviStepeneSlobode(
                    new StepeniSlobode(new Pravac[]{ Pravac.Hor, Pravac.Vert }), 
                    new StepeniSlobode(new Pravac[]{ Pravac.Hor, Pravac.Vert }),
                    veza.dajPrviCvor().dajPocetak());
                Veza kopija = veza.dajKopiju() as Veza;
                blok.dodajObjekt(kopija);
                kmdUkloni.Add(new KmdUkloniVezu(veza));
                kmdStavi.Add(new KmdStaviVezu(kopija));
            }
        }
    }

    public override void pomeri(int adx, int ady)
    {
        blok.pomeri(adx, ady);
        Point pomeraj = blok.dajPoslednjiPomeraj();
        dx = dx + pomeraj.X;
        dy = dy + pomeraj.Y;
    }

    public override void pocniPomeranje()
    {
        blok.pocniPomeranje();
    }

    public override void zavrsiPomeranje()
    {
        blok.zavrsiPomeranje();
    }

    public override void ukloni()
    {
        for (int i = 0; i < kmdUkloni.Count; i++)
            kmdUkloni[i].izvrsi();
    }

    public override void stavi()
    {
        for (int i = 0; i < kmdStavi.Count; i++)
        {
            Komanda Komanda = kmdStavi[i];
            Komanda.izvrsi();
            Komanda.selektuj();
        }
        stanje = StanjeKomande.Izvrsena;
    }

    public override void izvrsi()
    {
        skaliraj();
        ukloni();
        blok.pomeri(dx, dy);
        stavi();
        stanje = StanjeKomande.Izvrsena;
    }

    public override void undo()
    {
        skaliraj();
        for (int i = kmdStavi.Count - 1; i >= 0; i--)
            kmdStavi[i].undo();
        blok.pomeri(-dx, -dy);
        for (int i = kmdUkloni.Count - 1; i >= 0; i--)
            kmdUkloni[i].undo();
        stanje = StanjeKomande.Vracena;
    }

    public override void nacrtaj(Graphics g, Point delta)
    {
        blok.nacrtaj(g, delta);
    }

    public override void skaliraj()
    {
        float novaSkala = sema.dajSkalu();
        blok.skaliraj(novaSkala);
        dx = (int)Math.Round(dx * novaSkala/skala);
        dy = (int)Math.Round(dy * novaSkala/skala);
        for (int i = 0; i < kmdUkloni.Count; i++)
            kmdUkloni[i].skaliraj();
        for (int i = 0; i < kmdStavi.Count; i++)
            kmdStavi[i].skaliraj();
        skala = novaSkala;
    }
}

public class KmdRotiraj : Komanda
{
    private Blok blok;
    private List<Komanda> kmdUkloni, kmdStavi;
    private Point centarRot;
    private int smerRot;
    private Point pomeraj;

    public KmdRotiraj(List<GrafObjekt> objekti, int smerRot) 
        : base(objekti[0].dajSemu())
    {
        blok = new Blok(sema, 0);
        kmdUkloni = new List<Komanda>();
        kmdStavi = new List<Komanda>();
        for (int i = 0; i < objekti.Count; i++)
        {
            GrafObjekt grafObjekt = objekti[i];
            if (grafObjekt is Kolo)
            {
                Kolo kolo = grafObjekt as Kolo;
                blok.dodajObjekt(kolo);
                kmdUkloni.Add(new KmdUkloniKolo(kolo, false));
                kmdStavi.Add(new KmdStaviKolo(kolo));
            }
            else if (grafObjekt is Veza)
            {
                Veza veza = grafObjekt as Veza;
                Veza kopija = (Veza)veza.dajKopiju();
                blok.dodajObjekt(kopija);
                kmdUkloni.Add(new KmdUkloniVezu(veza));
                kmdStavi.Add(new KmdStaviVezu(kopija));
            }
        }
        centarRot = blok.dajTeziste();
        this.smerRot = smerRot;
    }

    public override void izvrsi()
    {
        skaliraj();
        for (int i = 0; i < kmdUkloni.Count; i++)
            kmdUkloni[i].izvrsi();
        blok.rotiraj(centarRot, smerRot);
        blok.pomeri(0, 0);
        pomeraj = blok.dajPoslednjiPomeraj();
        for (int i = 0; i < kmdStavi.Count; i++)
        {
            Komanda komanda = kmdStavi[i];
            komanda.izvrsi();
            komanda.selektuj();
        }
        stanje = StanjeKomande.Izvrsena;
    }

    public override void undo()
    {
        skaliraj();
        for (int i = kmdStavi.Count - 1; i >= 0; i--)
            kmdStavi[i].undo();
        centarRot.X = centarRot.X + pomeraj.X;
        centarRot.Y = centarRot.Y + pomeraj.Y;
        blok.rotiraj(centarRot, - smerRot);
        blok.pomeri(- pomeraj.X, - pomeraj.Y);
        centarRot.X = centarRot.X - pomeraj.X;
        centarRot.Y = centarRot.Y - pomeraj.Y;
        for (int i = kmdUkloni.Count - 1; i >= 0; i--)
            kmdUkloni[i].undo();
        stanje = StanjeKomande.Vracena;
    }

    public override void skaliraj()
    {
        float novaSkala = sema.dajSkalu();
        blok.skaliraj(novaSkala);
        for (int i = 0; i < kmdUkloni.Count; i++)
            kmdUkloni[i].skaliraj();
        for (int i = 0; i < kmdStavi.Count; i++)
            kmdStavi[i].skaliraj();
        centarRot.X = (int)Math.Round(centarRot.X * novaSkala/skala);
        centarRot.Y = (int)Math.Round(centarRot.Y * novaSkala/skala);
        pomeraj.X = (int)Math.Round(pomeraj.X * novaSkala / skala);
        pomeraj.Y = (int)Math.Round(pomeraj.Y * novaSkala / skala);
        skala = novaSkala;
    }
}

public class SlozenaKomanda : Komanda
{
    private List<Komanda> komande;

    public SlozenaKomanda(Sema sema) : base(sema)
    {
        komande = new List<Komanda>();
    }

    public void dodajKomandu(Komanda komanda)
    {
        komande.Add(komanda);
    }

    public override void izvrsi()
    {
        for (int i = 0; i < komande.Count; i++)
        {
            Komanda komanda = komande[i];
            komanda.izvrsi();
            komanda.selektuj();
        }
        stanje = StanjeKomande.Izvrsena;
    }

    public override void undo()
    {
        for (int i = komande.Count - 1; i >= 0; i--)
            komande[i].undo();
        stanje = StanjeKomande.Vracena;
    }

    public override void skaliraj()
    {

    }
}

public class KmdBrisi : SlozenaKomanda
{
    public KmdBrisi(Sema sema) : base(sema)
    {

}
}

public class KmdStavi : SlozenaKomanda
{
    public KmdStavi(Sema sema) : base(sema)
    {

    }
}

public class Sema
{
    protected Opcije opcije;
    protected int visina;
    protected int sirina;
    protected int pinRazmak;
    protected float skala;
    protected int duzinaPina;
    protected int IDBrojac;
    protected bool printPrev;
    private List<Cvor> listaCvorova;
    private List<Cvor> cvorovi = new List<Cvor>();
    private List<Kolo> kola;
    protected SemaSize size;
    protected List<Komanda> listaKomandi;
    private List<Veza> listaVeza;
    private List<Veza> veze = new List<Veza>();
    private int brojKomande;
    protected Blok clipBoard;
    protected bool nonGridSkaliranje;
    private float realPinRazmak;
    private float realVisina;
    private float realSirina;
    private float staraSkala;

    Pen bojaKrajaPinaPen;
    public Pen BojaKrajaPinaPen
    {
        get { return bojaKrajaPinaPen; }
    }

    Pen bojaSelekcijePen;
    public Pen BojaSelekcijePen
    {
        get { return bojaSelekcijePen; }
    }

    Pen bojaPinaPen;
    public Pen BojaPinaPen
    {
        get { return bojaPinaPen; }
    }

    Pen bojaVezePen;
    public Pen BojaVezePen
    {
        get { return bojaVezePen; }
    }

    Brush bojaCvoraBrush;
    public Brush BojaCvoraBrush
    {
        get { return bojaCvoraBrush; }
    }

    Brush bojaSelekcijeBrush;
    public Brush BojaSelekcijeBrush
    {
        get { return bojaSelekcijeBrush; }
    }

    Pen bojaKolaPen;
    public Pen BojaKolaPen
    {
        get { return bojaKolaPen; }
    }

    private EditorForm form;
    private EditorPresenter presenter;

    private bool pritisnutMis;  // pokazuje da li je pritisnut levi taster misa
    private Point preth;   // prethodni polozaj misa
    private Point poc;    // pocetni polozaj misa
    private RastegljivSegment segment1, segment2; // dva segmenta koja se crtaju u rezimu
                                        // stavljanja veze
    private Blok blok;
    private GrafObjekt selObjekt;
    private Cvor pogodjenCvor;
    private bool prethodnoSelektovan;

    private KmdPomeri kmdPomeri; // komanda za pomeranje
    private Kolo stavljanoKolo;  // kolo koje se stavlja

    public bool modified;
    private Stanje stanje = Stanje.None;

    public Sema(Opcije opcije, SemaSize size, EditorForm form, EditorPresenter presenter)
    {
        this.form = form;
        this.presenter = presenter;
        kola = new List<Kolo>();
        this.opcije = opcije;
        pinRazmak = opcije.PinRazmak;
        this.size = size;
        sirina = Prostor.sizeToWidth(size) * pinRazmak + 1;
        visina = Prostor.sizeToHeight(size) * pinRazmak + 1;
        duzinaPina = opcije.DuzinaPina;
        listaKomandi = new List<Komanda>();
        skala = 100;

        initPensAndBrushes();
    }

    private void initPensAndBrushes()
    {
        bojaKrajaPinaPen = new Pen(opcije.BojaKrajaPina);
        bojaSelekcijePen = new Pen(opcije.BojaSelekcije);
        bojaPinaPen = new Pen(opcije.BojaPina);
        bojaVezePen = new Pen(opcije.BojaVeze);
        bojaCvoraBrush = new SolidBrush(opcije.BojaCvora);
        bojaSelekcijeBrush = new SolidBrush(opcije.BojaSelekcije);
        bojaKolaPen = new Pen(opcije.BojaKola);
    }

    public float dajSkalu()
    {
        return skala;
    }

    public int dajVisinu()
    {
        return visina;
    }

    public int dajSirinu()
    { 
        return sirina;
    }

    public Opcije dajOpcije()
    {
        return opcije;
    }

    public int dajPinRazmak()
    { 
       return pinRazmak;
    }

    public int dajDuzinuPina()
    {
        return duzinaPina;
    }

    // spaja veze koje se nadovezuju jedna na drugu, i vraca novostvorenu vezu
    public Veza spojiVeze(Veza veza1, Veza veza2)
    {
        Cvor pocetak = veza1.dajPrviCvor();
        Cvor kraj = veza2.dajDrugiCvor();
        Cvor sredina = veza1.dajDrugiCvor();
        if (pocetak == kraj)
        {
            // Veza2 je ispred Veze1
            pocetak = veza2.dajPrviCvor();
            kraj = veza1.dajDrugiCvor();
            sredina = veza2.dajDrugiCvor();
        }
        ukloniVezu(veza1);
        ukloniVezu(veza2);
        
        // kreiraj novu vezu
        Veza result = new Veza(pocetak, kraj, dajNoviID());
        veze.Add(result);
        return result;
    }

    // uklanja cvor
    public void ukloniCvor(Cvor cvor)
    {
        cvorovi.Remove(cvor);
    }

    public int dajNoviID()
    {
        IDBrojac = IDBrojac + 1;
        return IDBrojac;
    }

    public bool isPrintPreview()
    {
        return printPrev;
    }

    public Cvor dajCvor(int Id)
    {
        // Koristi se samo prilikom ucitavanja seme sa strima. Iz liste Cvorovi
        // (lista u kojoj su privremeno smesteni cvorovi ucitani sa strima) vraca
        // cvor sa identifikatorom Id.
        Cvor cvor = null;
        for (int i = 0; i < listaCvorova.Count; i++)
        {
            if (listaCvorova[i].dajID() == Id)
            {
                cvor = listaCvorova[i];
                break;
            }
        }
        return cvor;
    }

    public Veza dajVezu(int Id)
    {
        // Koristi se samo prilikom ucitavanja seme sa strima. Iz liste Veze
        // (lista u kojoj su privremeno smestene veze ucitane sa strima) vraca
        // vezu sa identifikatorom Id.
        Veza veza = null;
        for (int I = 0; I < listaVeza.Count; I++)
        {
            if (listaVeza[I].dajID() == Id)
            {
                veza = listaVeza[I];
                break;
            }
        }
        return veza;
    }

    public Pin dajPin(int Id)
    {
        // koristi se samo prilikom ucitavanja seme sa strima. Vraca pin sa 
        // identifikatorom Id.
        Pin pin = null;
        for (int i = 0; i < kola.Count; i++)
        {
            pin = kola[i].dajPin(Id);
            if (pin != null) 
                break;
        }
        return pin;
    }

    // vraca cvor sa koordinatama TacanPolozaj; ako ne postoji, vraca null
    public Cvor dajCvor(Point tacanPolozaj)
    {
        foreach (Cvor c in cvorovi)
        {
            if (c.dajPocetak() == tacanPolozaj)
                return c;
        }
        return null;
    }

    // kreira cvor na mestu Polozaj i vraca ga kao rezultat; podrazumeva se
    // da je prethodno provereno da na mestu Polozaj ne postoji cvor, a u
    // funkciji se dodatno proverava da li se Polozaj nalazi unutar neke veze
    public Cvor noviCvor(Point polozaj)
    {
        Cvor result;
        Veza horVeza = dajVezu(polozaj, Pravac.Hor);
        Veza vertVeza = dajVezu(polozaj, Pravac.Vert);
        if (horVeza == null && vertVeza == null)
        {
            result = new Cvor(polozaj, this, dajNoviID());
            cvorovi.Add(result);
        }
        else if (horVeza == null && vertVeza != null)
            // Polozaj se nalazi na vertikalnoj vezi
            result = podeliVezu(vertVeza, polozaj);
        else if (horVeza != null && vertVeza == null)
            // Polozaj se nalazi na horizontalnoj vezi
            result = podeliVezu(horVeza, polozaj);
        else
            // Polozaj se nalazi na ukrstanju horizontalne i vertikalne veze
            result = napraviCvoriste(horVeza, vertVeza, polozaj);
        return result;
    }

    public bool ukrstanjeVeza(Point polozaj)
    {
        return dajVezu(polozaj, Pravac.Hor) != null &&
            dajVezu(polozaj, Pravac.Vert) != null;
    }

    // vraca vezu koja prolazi kroz tacku tacanPolozaj u pravcu pravac
    // (pri trazenju veze ne uzima u obzir krajeve veze); ako veza ne postoji,
    // vraca null
    protected Veza dajVezu(Point tacanPolozaj, Pravac pravac)
    {
        foreach (Veza v in veze)
        {
            if (v.dajPravac() == pravac && v.containsExclusive(tacanPolozaj))
                return v;
        }
        return null;
    }

    // deli vezu Veza na dva dela(dve veze) kreiranjem cvora na mestu Polozaj,
    // i vraca cvor kao rezultat
    protected Cvor podeliVezu(Veza veza, Point polozaj)
    {
        Cvor cvor = new Cvor(polozaj, veza.dajSemu(), dajNoviID());
        Cvor pocCvor = veza.dajPrviCvor();
        Cvor krajCvor = veza.dajDrugiCvor();
        bool selektovana = veza.selektovan();

        // napravi dve nove veze
        Veza novaVeza = new Veza(pocCvor, cvor, dajNoviID());
        if (selektovana)
            novaVeza.selektuj();
        veze.Add(novaVeza);
        novaVeza = new Veza(cvor, krajCvor, dajNoviID());
        if (selektovana) 
            novaVeza.selektuj();
        veze.Add(novaVeza);
        // veza sa uklanja tek nakon sto se postave dve nove veze zato sto prilikom
        // uklanjanja veza moze doci do uklanjanja cvora na krajevima veze (ako u
        // cvoru postoji samo ta veza). Ovako, kada se prvo dodaju dve nove veze pa tek
        // nakon toga ukloni stara veza sigurno je da ce cvorovi na kraju veze ostati
        // tu gde jesu
        ukloniVezu(veza);

        cvorovi.Add(cvor);
        return cvor;
    }

    // pravi cvor na mestu ukrstanja dve veze, i vraca ga kao rezultat;
    // (od dve veze pravi cetiri)
    protected Cvor napraviCvoriste(Veza horVeza, Veza vertVeza, Point polozaj)
    {
        Cvor cvor = new Cvor(polozaj, horVeza.dajSemu(), dajNoviID());
        cvorovi.Add(cvor);

        Cvor pocHor = horVeza.dajPrviCvor();
        Cvor krajHor = horVeza.dajDrugiCvor();
        bool selektovana = horVeza.selektovan();
        // kreiraj dve nove veze
        Veza novaVeza = new Veza(pocHor, cvor, dajNoviID());
        veze.Add(novaVeza);
        if (selektovana)
            novaVeza.selektuj();
        novaVeza = new Veza(cvor, krajHor, dajNoviID());
        veze.Add(novaVeza);
        if (selektovana)
            novaVeza.selektuj();
        ukloniVezu(horVeza);

        Cvor pocVert = vertVeza.dajPrviCvor();
        Cvor krajVert = vertVeza.dajDrugiCvor();
        selektovana = vertVeza.selektovan();
        // isto i sa vertikalnom
        novaVeza = new Veza(pocVert, cvor, dajNoviID());
        veze.Add(novaVeza);
        if (selektovana)
            novaVeza.selektuj();
        novaVeza = new Veza(cvor, krajVert, dajNoviID());
        veze.Add(novaVeza);
        if (selektovana)
            novaVeza.selektuj();
        ukloniVezu(vertVeza);

        return cvor;
    }

    // Stavlja vezu izmedju tacka Pocetak i Kraj i vraca stavljene veze.
    // Dodaje nove veze samo na onim mestima
    // na kojima one ne postoje(eventualne veze koje postoje izmedju Pocetak i Kraj
    // se preskacu). Na taj nacin, broj stavljenih veza moze da bude nula (ako
    // izmedju Pocetak i Kraj vec postoji veza), jedan (ako ne postoje ni veza ni
    // cvor), ili vise od jedan (ako postoji jedan ili vise cvorova, ili ako na
    // nekim delovima izmedju Pocetak i Kraj postoje veze a na nekim ne).
    public void spojiTacke(Point pocetak, Point kraj, List<Veza> veze)
    {
        if (pocetak == kraj)
            // pocetak i kraj se poklapaju
            return;
        Cvor pocCvor = dajCvor(pocetak);
        if (pocCvor == null)
            pocCvor = noviCvor(pocetak);
        Cvor krajCvor = dajCvor(kraj);
        if (krajCvor == null)
            krajCvor = noviCvor(kraj);

        Pravac pravac;
        Smer prethHor = new Smer(), sledHor = new Smer(), prethVert = new Smer(),
            sledVert = new Smer(), tmp = new Smer();
        // odredi pravac u kom se postavlja veza
        if (pocCvor.dajPocetak().Y == krajCvor.dajPocetak().Y)
            pravac = Pravac.Hor;
        else
            pravac = Pravac.Vert;
        Prostor.orijentisi(pravac, ref prethHor, ref sledHor, ref prethVert, ref sledVert);
        if (pocCvor.dajKoordinatu(pravac) > krajCvor.dajKoordinatu(pravac))
        {
            // obrni prethodni i sledeci smer
            tmp = prethHor;
            prethHor = sledHor;
            sledHor = tmp;
        }

        List<Cvor> cvorList = dajCvorove(pocCvor, krajCvor);
        int i = 0;
        while (i < cvorList.Count - 1)
        {
            Cvor cvor = cvorList[i];
            if (cvor.dajVezu(sledHor) == null)
            {
                // kreiraj novu vezu
                Veza veza = new Veza(cvor, cvorList[i + 1], dajNoviID());
                this.veze.Add(veza);
                veze.Add(veza);
            }

            if (cvor.dajVezu(prethHor) != null && cvor.dajVezu(prethVert) == null
            && cvor.dajVezu(sledVert) == null && cvor.dajBrojPinova() == 0)
            {
                // dve veze se nadovezuju jedna na drugu, pa ih treba spojiti
                spojiVeze(cvor.dajVezu(prethHor), cvor.dajVezu(sledHor));
            }
            i++;
        }

        if (krajCvor.dajVezu(sledHor) != null && krajCvor.dajVezu(prethVert) == null
        && krajCvor.dajVezu(sledVert) == null && krajCvor.dajBrojPinova() == 0)
        {
            // u cvoru Kraj se dve veze nadovezuju jedna na drugu
            spojiVeze(krajCvor.dajVezu(prethHor), krajCvor.dajVezu(sledHor));
        }
    }

    public List<Veza> dajVeze(Point pocetak, Point kraj)
    {
        List<Veza> result = new List<Veza>();
        foreach (Veza v in veze)
        {
            if (v.overlaps(pocetak, kraj))
                result.Add(v);
        }
        return result;
    }

    // Vraca sve cvorove izmedju cvorova poc i kraj (ukljucujuci poc i kraj)
    private List<Cvor> dajCvorove(Cvor poc, Cvor kraj)
    {
        List<Cvor> result = new List<Cvor>();
        Pravac pravac = Prostor.getPravac(poc.dajPocetak(), kraj.dajPocetak());
        if (pravac == Pravac.None)
            throw new Exception("Cvorovi nisu na ortogonalnom pravcu.");
        Pravac suprPravac = Prostor.suprotanPravac(pravac);

        bool reverse = false;
        if (poc.dajKoordinatu(pravac) > kraj.dajKoordinatu(pravac))
        {
            reverse = true;
            Cvor tmp = poc;
            poc = kraj;
            kraj = tmp;
        }

        foreach (Cvor c in cvorovi)
        {
            if (c.dajKoordinatu(suprPravac) == poc.dajKoordinatu(suprPravac)
            && c.dajKoordinatu(pravac) >= poc.dajKoordinatu(pravac)
            && c.dajKoordinatu(pravac) <= kraj.dajKoordinatu(pravac))
                result.Add(c);
        }
        result.Sort(new CvorComparer(pravac));
        if (reverse)
            result.Reverse();
        return result;
    }

    public void ukloniVeze(List<Veza> veze)
    {
        for (int i = 0; i < veze.Count; i++)
        {
            Veza v = veze[i];
            // mora da se ponovo zatrazi veza zato sto prilikom stavljanja veze
            // moze da dodje do merdzovanja, tako da ona veza koja je stavljena
            // vise nije na semi
            Veza veza = dajVezu(v.dajPocetak(), v.dajKraj());
            ukloniVezu(veza);
        }
    }

    public Veza dajVezu(Point pocetak, Point kraj)
    {
        Cvor pocCvor = dajCvor(pocetak);
        if (pocCvor == null)
            pocCvor = noviCvor(pocetak);
        Cvor krajCvor = dajCvor(kraj);
        if (krajCvor == null)
            krajCvor = noviCvor(kraj);
        foreach (Veza v in veze)
        {
            if (v.dajPocetak() == pocetak && v.dajKraj() == kraj
            || v.dajPocetak() == kraj && v.dajKraj() == pocetak)
                return v;
        }
        return null;
    }

    public void ukloniCvoriste(Cvor cvor)
    {
        Cvor levo = cvor.dajVezu(Smer.Zap).dajPrviCvor();
        Cvor desno = cvor.dajVezu(Smer.Ist).dajDrugiCvor();
        Cvor gore = cvor.dajVezu(Smer.Sev).dajPrviCvor();
        Cvor dole = cvor.dajVezu(Smer.Jug).dajDrugiCvor();

        veze.Remove(cvor.dajVezu(Smer.Zap));
        veze.Remove(cvor.dajVezu(Smer.Ist));
        veze.Remove(cvor.dajVezu(Smer.Sev));
        veze.Remove(cvor.dajVezu(Smer.Jug));

        veze.Add(new Veza(levo, desno, dajNoviID()));
        veze.Add(new Veza(gore, dole, dajNoviID()));
        cvorovi.Remove(cvor);
    }

    public void deselektuj()
    {
        List<GrafObjekt> selektovani = dajSelektovane();
        for (int i = 0; i < selektovani.Count; i++)
            selektovani[i].deselektuj();
    }

    public List<GrafObjekt> dajSelektovane()
    {
        List<GrafObjekt> selektovani = new List<GrafObjekt>();
        for (int i = 0; i < kola.Count; i ++)
        {
            if (kola[i].selektovan())
                selektovani.Add(kola[i]);
        }
        for (int i = 0; i < veze.Count; i++)
        {
            if (veze[i].selektovan())
                selektovani.Add(veze[i]);
        }
        return selektovani;
    }

    public void dodajKolo(Kolo kolo, List<Point> cvorista)
    {
        kola.Add(kolo);

        // spoji kolo na semu, spajanjem pinova na cvorove
        List<Pin> pinovi = kolo.dajPinove();
        for (int i = 0; i < pinovi.Count; i++)
        {
            Pin pin = pinovi[i];
            // da li postoji cvor na mestu kraja pina
            Cvor cvor = dajCvor(pin.dajKraj());
            if (cvor == null)
            {
                // napravi novi cvor
                cvor = noviCvor(pin.dajKraj());
                if (cvor.dajBrojVeza() == 4)
                    cvorista.Add(pin.dajKraj());
            }
            // spoji pin na cvor
            pin.spoji(cvor);
        }
    }

    public void ukloniKolo(Kolo kolo)
    {
        // odspaja kolo sa seme, odspajanjem pinova sa cvorova
        List<Pin> pinovi = kolo.dajPinove();
        for (int i = 0; i < pinovi.Count; i++)
            pinovi[i].odspoji();

        kola.Remove(kolo);
    }

    public void staviVezu(Point pocetak, Point kraj)
    {
        deselektuj();
        KmdStaviVezu kmdStaviVezu = new KmdStaviVezu(pocetak, kraj, this);
        kmdStaviVezu.izvrsi();
        kmdStaviVezu.selektuj();
        dodajKomandu(kmdStaviVezu);
    }

    public void dodajKomandu(Komanda komanda)
    {
        for (int i = listaKomandi.Count - 1; i >= brojKomande; i--)
            listaKomandi.RemoveAt(i);
        listaKomandi.Add(komanda);
        brojKomande = brojKomande + 1;
    }

    public void staviKolo(int x, int y, TipKola tip, int brojUlaza)
    {
        Kolo kolo = null;
        switch (tip)
        {
            case TipKola.Ne: 
                kolo = new Ne(new Point(x, y), this, dajNoviID());
                break;
            case TipKola.I: 
                kolo = new I(new Point(x, y), brojUlaza, this, dajNoviID());
                break;
            case TipKola.Ili: 
                kolo = new Ili(new Point(x, y), brojUlaza, this, dajNoviID());
                break;
            case TipKola.Ni: 
                kolo = new Ni(new Point(x, y), brojUlaza, this, dajNoviID());
                break;
            case TipKola.Nili: 
                kolo = new NIli(new Point(x, y), brojUlaza, this, dajNoviID());
                break;
            case TipKola.ExIli: 
                kolo = new ExIli(new Point(x, y), brojUlaza, this, dajNoviID());
                break;
        }
        kolo.pomeri(0, 0);
        deselektuj();
        KmdStaviKolo kmdStaviKolo = new KmdStaviKolo(kolo);
        kmdStaviKolo.izvrsi();
        kolo.selektuj();
        dodajKomandu(kmdStaviKolo);
    }

    public void staviKolo(Kolo kolo)
    {
        kolo.pomeri(0, 0);
        deselektuj();
        KmdStaviKolo kmdStaviKolo = new KmdStaviKolo(kolo);
        kmdStaviKolo.izvrsi();
        kolo.selektuj();
        dodajKomandu(kmdStaviKolo);
    }

    public void selektuj(Rectangle rect)
    {
        for (int i = 0; i < kola.Count; i++)
        {
            Kolo kolo = kola[i];
            if (kolo.unutar(rect, false))
                kolo.selektuj();
        }
        for (int i = 0; i < veze.Count; i++)
        {
            Veza veza = veze[i];
            if (veza.unutar(rect, false))
                veza.selektuj();
        }
    }

    // brise selektovane elemente
    public void brisi()
    {
        List<GrafObjekt> selektovani = dajSelektovane();
        if (selektovani.Count > 0)
        {
            KmdBrisi kmdBrisi = new KmdBrisi(this);
            for (int i = 0; i < selektovani.Count; i++)
            {
                GrafObjekt grafObj = selektovani[i];
                if (grafObj is Kolo)
                    kmdBrisi.dodajKomandu(new KmdUkloniKolo(grafObj as Kolo, true));
                else if (grafObj is Veza)
                    kmdBrisi.dodajKomandu(new KmdUkloniVezu(grafObj as Veza));
            }
            kmdBrisi.izvrsi();
            dodajKomandu(kmdBrisi);
        }
    }

    // rotira selektovane elemente
    public void rotiraj()
    {
        List<GrafObjekt> selektovani = dajSelektovane();
        if (selektovani.Count > 0)
        {
            Blok blok = new Blok(this, 0);
            for (int i = 0; i < selektovani.Count; i++)
                blok.dodajObjekt(selektovani[i].dajKopiju());
            if (blok.canRotate())
            {
                KmdRotiraj kmdRotiraj = new KmdRotiraj(selektovani, 1);
                kmdRotiraj.izvrsi();
                dodajKomandu(kmdRotiraj);
            }
        }
    }

    public Blok dajClipBoard()
    {
        if (clipBoard != null)
            return clipBoard.dajKopiju() as Blok;
        else
            return null;
    }

    public void cut()
    {
        List<GrafObjekt> selektovani = dajSelektovane();
        if (selektovani.Count > 0)
        {
            clipBoard = new Blok(this, 0);
            KmdBrisi kmdBrisi = new KmdBrisi(this);
            for (int i = 0; i < selektovani.Count; i++)
            {
                GrafObjekt grafObj = selektovani[i];
                clipBoard.dodajObjekt(grafObj.dajKopiju());
                if (grafObj is Kolo)
                    kmdBrisi.dodajKomandu(new KmdUkloniKolo(grafObj as Kolo, true));
                else if (grafObj is Veza)
                    kmdBrisi.dodajKomandu(new KmdUkloniVezu(grafObj as Veza));
            }
            kmdBrisi.izvrsi();
            dodajKomandu(kmdBrisi);
        }
    }

    public void copy()
    {
        List<GrafObjekt> selektovani = dajSelektovane();
        if (selektovani.Count > 0)
        {
            clipBoard = new Blok(this, 0);
            for (int i = 0; i < selektovani.Count; i++)
            {
                GrafObjekt grafObj = selektovani[i];
                clipBoard.dodajObjekt(grafObj.dajKopiju());
            }
        }
    }

    public void paste(Blok blok)
    {
        KmdStavi kmdStavi = new KmdStavi(this);
        List<GrafObjekt> objekti = blok.dajObjekte();
        for (int i = objekti.Count - 1; i >= 0; i--)
        {
            GrafObjekt grafObj = (GrafObjekt)objekti[i];
            if (grafObj is Kolo)
                kmdStavi.dodajKomandu(new KmdStaviKolo(grafObj as Kolo));
            else if (grafObj is Veza)
                kmdStavi.dodajKomandu(new KmdStaviVezu(grafObj as Veza));
            blok.ukloniObjekt(grafObj);
        }
        kmdStavi.izvrsi();
        dodajKomandu(kmdStavi);
    }

    public void undo()
    {   
        if (canUndo())
        {
            brojKomande = brojKomande - 1;
            listaKomandi[brojKomande].undo();
        }
    }

    public bool canUndo()
    {
        return brojKomande > 0;
    }

    public bool canPaste()
    {
        return clipBoard != null;
    }

    public void redo()
    {
        if (canRedo())
        {
            listaKomandi[brojKomande].izvrsi();
            brojKomande = brojKomande + 1;
        }
    }

    public bool canRedo()
    {
        return brojKomande < listaKomandi.Count;
    }

    public void skaliraj(float procenat)
    {
        if (procenat < 10)
           procenat = 10;
        if (nonGridSkaliranje)
        {
            realPinRazmak = realPinRazmak * procenat/skala;
            realSirina = realSirina * procenat/skala;
            realVisina = realVisina * procenat/skala;
            pinRazmak = (int)Math.Round(realPinRazmak);
            sirina = (int)Math.Round(realSirina);
            visina = (int)Math.Round(realVisina);
            skala = procenat;
        }
        else
        {
            int noviPinRazmak = (int)Math.Round(pinRazmak * procenat/skala);
            float novaSkala = skala * (float)noviPinRazmak / pinRazmak;
            sirina = (int)Math.Round((sirina - 1) * novaSkala/skala) + 1;
            visina = (int)Math.Round((visina - 1) * novaSkala/skala) + 1;
            pinRazmak = noviPinRazmak;
            skala = novaSkala;
        }
        for (int i = 0; i < kola.Count; i++)
           kola[i].skaliraj(skala);
        for (int i = 0; i < veze.Count; i++)
            veze[i].skaliraj(skala);
        for (int i = 0; i < cvorovi.Count; i++)
            cvorovi[i].skaliraj(skala);
        if (clipBoard != null && !nonGridSkaliranje)
            clipBoard.skaliraj(skala);
    }

    public void skaliraj(Rectangle rect)
    {
        float novaSkala;
        if (nonGridSkaliranje)
        {
            if ((float)sirina / visina > (float)rect.Right / rect.Bottom)
                novaSkala = skala * (float)rect.Right / sirina;
            else
                novaSkala = skala * (float)rect.Bottom / visina;
            skaliraj(novaSkala);
        }
    }

    public void nacrtaj(Graphics g, Bitmap bitmap, Rectangle vidljiviDeo)
    {
        g.FillRectangle(form.bojaPozadineBrush,
            new Rectangle(0, 0, vidljiviDeo.Width, vidljiviDeo.Height));
        if (opcije.GridDots && (dajPinRazmak() >= 5))
        {
            int pinRazmak = dajPinRazmak();
            int i = pinRazmak - form.delta.X % pinRazmak;
            int pocJ = pinRazmak - form.delta.Y % pinRazmak;
            int iMax = vidljiviDeo.Width;
            int jMax = vidljiviDeo.Height;
            while (i < iMax)
            {
                int j = pocJ;
                while (j < jMax)
                {
                    bitmap.SetPixel(i, j, opcije.GridDotColor);
                    j = j + pinRazmak;
                }
                i = i + pinRazmak;
            }
        }

        Point delta = vidljiviDeo.Location;
        for (int i = 0; i < kola.Count; i++)
            kola[i].nacrtaj(g, delta);
        List<Veza> selektovaneVeze = new List<Veza>();
        for (int i = 0; i < veze.Count; i++)
        {
          Veza veza = veze[i];
          if (!veza.selektovan())
              veza.nacrtaj(g, delta);
          else
              selektovaneVeze.Add(veza);
        }
        for (int i = 0; i < cvorovi.Count; i++)
            cvorovi[i].nacrtaj(g, delta);
        for (int i = 0; i < selektovaneVeze.Count; i++)
            selektovaneVeze[i].nacrtaj(g, delta);

        switch (stanje)
        {
            case Stanje.Laso2:
                crtajLaso(form.bitmapGraphics, form.getRectangle(
                    poc.X, poc.Y, preth.X, preth.Y), delta);
                break;

            case Stanje.Pomeranje2:
                kmdPomeri.nacrtaj(form.bitmapGraphics, delta);
                break;

            case Stanje.Kolo1:
            case Stanje.Kolo2:
                stavljanoKolo.nacrtaj(form.bitmapGraphics, delta);
                break;

            case Stanje.Veza2:
                segment1.nacrtaj(form.bitmapGraphics, delta);
                segment2.nacrtaj(form.bitmapGraphics, delta);
                break;

            case Stanje.Kopija2:
                blok.nacrtaj(form.bitmapGraphics, delta);
                break;
        }
        Graphics g2 = form.panelSema.CreateGraphics();
        g2.DrawImage(form.bitmap, 0, 0, form.panelSema.Width, form.panelSema.Height);
        g2.Dispose();

    }

    private void crtajLaso(Graphics g, Rectangle rect, Point delta)
    {
        using (Pen p = new Pen(Color.Black))
        {
            rect.Offset(-delta.X, -delta.Y);
            g.DrawRectangle(p, rect);
        }
    }

    // vraca cvor u blizini tacke (x, y); ako ne postoji, vraca null
    public Cvor traziCvor(int x, int y)
    {
        Cvor cvor = null;
        for (int i = 0; i < cvorovi.Count; i++)
        {
            if (cvorovi[i].pogodjen(x, y))
            {
                cvor = cvorovi[i];
                break;
            }
        }
        return cvor;
    }

    // vraca vezu u blizini tacke (x, y)(ako postoji), inace vraca null;
    // pri trazenju se ne uzimaju u obzir krajevi veze(cvorovi)
    public Veza traziVezu(int x, int y)
    {
        Veza veza = null;
        for (int i = 0; i < veze.Count; i++)
        {
            if (veze[i].pogodjena(x, y))
            {
                veza = (Veza)veze[i];
                break;
            }
        }
        return veza;
    }

    public Kolo traziKolo(int x, int y)
    {
        Kolo kolo = null;
        for (int i = kola.Count - 1; i >= 0; i--)
        {
            if (kola[i].pogodjen(x, y))
            {
                kolo = kola[i];
                break;
            }
        }
        return kolo;
    }

    public int dajBrojSelektovanih()
    {
        return dajSelektovane().Count;
    }

    public Sema dajKopiju()
    {
        Sema kopija = new Sema(opcije, size, form, presenter);
        kopija.skaliraj(skala);
        for (int i = 0; i < kola.Count; i++)
        {
            Kolo kolo = kola[i];
            Kolo kolo2 = null;
            switch (kolo.dajTip())
            {
                case TipKola.Ne: 
                    kolo2 = new Ne(kolo.dajPocetak(), kopija, kolo.dajSmer(),
                        kopija.dajNoviID());
                    break;
                case TipKola.I: 
                    kolo2 = new I(kolo.dajPocetak(), kolo.dajBrojUlaza(), kopija,
                        kolo.dajSmer(), kopija.dajNoviID());
                    break;
                case TipKola.Ili: 
                    kolo2 = new Ili(kolo.dajPocetak(), kolo.dajBrojUlaza(), kopija,
                        kolo.dajSmer(), kopija.dajNoviID());
                    break;
                case TipKola.Ni: 
                    kolo2 = new Ni(kolo.dajPocetak(), kolo.dajBrojUlaza(), kopija,
                        kolo.dajSmer(), kopija.dajNoviID());
                    break;
                case TipKola.Nili: 
                    kolo2 = new NIli(kolo.dajPocetak(), kolo.dajBrojUlaza(), kopija,
                        kolo.dajSmer(), kopija.dajNoviID());
                    break;
                case TipKola.ExIli: 
                    kolo2 = new ExIli(kolo.dajPocetak(), kolo.dajBrojUlaza(), kopija,
                        kolo.dajSmer(), kopija.dajNoviID());
                    break;
            }
            kopija.staviKolo(kolo2);
        }
        for (int i = 0; i < veze.Count; i++)
        {
            Veza veza = veze[i];
            kopija.staviVezu(veza.dajPocetak(), veza.dajKraj());
        }
        kopija.deselektuj();
        return kopija;
    }

    public SemaSize getSize()
    {
        return size;
    }

    public void changeSize(SemaSize newSize)
    {
        if (!nonGridSkaliranje)
        {
            if ((SemaSize)newSize >= (SemaSize)size)
            {
                size = newSize;
                sirina = Prostor.sizeToWidth(size) * pinRazmak + 1;
                visina = Prostor.sizeToHeight(size) * pinRazmak + 1;
            }
        }
    }

    public void snapToGrid(ref int x, ref int y)
    {
        if (x < 0) x = 0;
        if (x > sirina - 1) x = sirina - 1;
        if (y < 0) y = 0;
        if (y > visina - 1) y = visina - 1;
        x = snapToGrid(x);
        y = snapToGrid(y);
    }

    protected int snapToGrid(int x)
    {
        if (x % pinRazmak <= pinRazmak / 2)
            return snapToGridBottom(x);
        else
            return snapToGridUp(x);
    }

    protected int snapToGridBottom(int x)
    {
        return x - x % pinRazmak;
    }

    protected int snapToGridUp(int x)
    {
        return x + (pinRazmak - x % pinRazmak) % pinRazmak;
    }

    public void pocniNonGridSkaliranje()
    {
        if (!nonGridSkaliranje)
        {
            nonGridSkaliranje = true;
            staraSkala = skala;
            realPinRazmak = pinRazmak;
            realSirina = sirina;
            realVisina = visina;
            for (int i = 0; i < kola.Count; i++)
                kola[i].pocniNonGridSkaliranje();
            for (int i = 0; i < veze.Count; i++)
                veze[i].pocniNonGridSkaliranje();
            for (int i = 0; i < cvorovi.Count; i++)
                cvorovi[i].pocniNonGridSkaliranje();
        }
    }

    public void zavrsiNonGridSkaliranje()
    {
        if (nonGridSkaliranje)
        {
            skaliraj(staraSkala);
            for (int i = 0; i < kola.Count; i++)
                kola[i].zavrsiNonGridSkaliranje();
            for (int i = 0; i < veze.Count; i++)
                veze[i].zavrsiNonGridSkaliranje();
            for (int i = 0; i < cvorovi.Count; i++)
                cvorovi[i].zavrsiNonGridSkaliranje();
            nonGridSkaliranje = false;
        }
    }

    public void pocniPrintPreview()
    {
        printPrev = true;
        pocniNonGridSkaliranje();
    }

    public void zavrsiPrintPreview()
    {
        printPrev = false;
        zavrsiNonGridSkaliranje();
    }

    public bool canSelektujIstiPotencijal()
    {
        List<GrafObjekt> selektovani = dajSelektovane();
        bool result = false;
        if (selektovani.Count == 1)
        {
            GrafObjekt selObjekt = selektovani[0];
            if (selObjekt is Veza)
                result = true;
        }
        return result;
    }

    public void selektujIstiPotencijal()
    {
        if (canSelektujIstiPotencijal())
        {
            List<GrafObjekt> selektovani = dajSelektovane();
            Veza selVeza = (Veza)selektovani[0];
            resetujPotencijale();
            Cvor cvor = selVeza.dajPrviCvor();
            cvor.setPotencijal(1);
            cvor.sprovediPotencijal();
            for (int i = 0; i < veze.Count; i++)
            {
                Veza veza = veze[i];
                if (veza.dajPrviCvor().dajPotencijal() == 1)
                    veza.selektuj();
            }
        }
    }

    public void resetujPotencijale()
    {
        for (int i = 0; i < cvorovi.Count; i++)
        {
            cvorovi[i].setPotencijal(0);
        }
    }

    public Sema(BinaryReader br, Opcije opcije, EditorForm form, EditorPresenter presenter)
    {
        this.form = form;
        this.presenter = presenter;
        this.opcije = opcije;  // inicijalizuje atribut opcije
        listaKomandi = new List<Komanda>();  // kreira listu komandi(praznu)
        size = (SemaSize)br.ReadInt32();
        visina = br.ReadInt32();
        sirina = br.ReadInt32();
        pinRazmak = br.ReadInt32();
        duzinaPina = br.ReadInt32();
        skala = br.ReadSingle();

        initPensAndBrushes();

        kola = new List<Kolo>();
        int brojKola = br.ReadInt32();
        for (int j = 0; j < brojKola; j++)   // ucitava kola sa strima
        {
            Kolo kolo = null;
            TipKola tipKola = (TipKola)br.ReadInt32();  // prvo ucitava tip kola
            if (tipKola == TipKola.Ne)                // a zatim i odgovarajuce kolo
                kolo = new Ne(br);
            else if (tipKola == TipKola.I)
                kolo = new I(br);
            else if (tipKola == TipKola.Ili)
                kolo = new Ili(br);
            else if (tipKola == TipKola.Ni)
                kolo = new Ni(br);
            else if (tipKola == TipKola.Nili)
                kolo = new NIli(br);
            else if (tipKola == TipKola.ExIli)
                kolo = new ExIli(br);
            kolo.postaviSemu(this);
            kola.Add(kolo);
        }
        listaVeza = new List<Veza>();  // Veze i Cvorovi su pomocni atributi koji sluze da
                               // prihvate veze i cvorove veze i cvorove koji se
                               // ucitavaju sa strima. Kada se za sve veze i cvo-
                               // rove ponovo azuriraju pokazivaci, i kada se po-
                               // novo naprave liste PrviCvorovi[Hor] i
                               // PrviCvorovi[Vert], liste Veze i Cvorovi se vise
                               // ne koriste.
        int brojVeza = br.ReadInt32();
        for (int j = 0; j < brojVeza; j++)  // ucitava veze sa strima i smesta
        {                                     // ih u listu Veze
            Veza veza = new Veza(br);
            veza.postaviSemu(this);
            listaVeza.Add(veza);
            veze.Add(veza);
        }
        listaCvorova = new List<Cvor>();
        int brojCvorova = br.ReadInt32();
        Cvor cvor;
        for (int j = 0; j < brojCvorova; j++)  // ucitava cvorove sa strima i
        {                                        // smesta ih u listu listaCvorova
            cvor = new Cvor(br);
            cvorovi.Add(cvor);
            cvor.postaviSemu(this);
            listaCvorova.Add(cvor);
        }
        IDBrojac = br.ReadInt32();
        // azurira pokazivace za kola, veze i cvorove
        for (int j = 0; j < kola.Count; j++)
            kola[j].azurirajPokazivace();
        for (int j = 0; j < listaVeza.Count; j++)
            listaVeza[j].azurirajPokazivace();
        for (int j = 0; j < listaCvorova.Count; j++ )
            listaCvorova[j].azurirajPokazivace();
    }

    public void saveToStream(BinaryWriter bw)
    {
        // komentar za atribute koji se ne smestaju na strim:
        // Opcije se ne smestaju na strim jer se dobijaju kao parametar prilikom
        // ucitavanja sa strima
        // ClipBoard se ne smesta na strim(nakon ucitavanja seme sa strima,
        // ClipBoard je prazan)
        // ListaKomandi se ne smesta na strim(nakon ucitavanja seme sa strima,
        // kreira se prazna lista komandi)
        // BrojKomande(indeks tekuce komande) se ne smesta na strim jer se nakon
        // ucitavanja sa strima postavlja na nulu
        // NonGridSkaliranje i PrintPrev(atributi koji pokazuju da li je sema u
        // rezimu ne-grid skaliranja i u rezimu prikaza za stampu) se ne smestaju
        // na strim jer se nakon ucitavanja sa strima postavljaju na False
        // Cvorovi i Veze su pomocni atributi koji se koriste samo prilikom
        // ucitavanja sa strima(prihvataju cvorove i veze ucitane sa strima) i
        // zato se ne smestaju na strim
        // RealPinRazmak, RealVisina, RealSirina, StaraSkala se ne smestaju na
        // strim jer se koriste samo u rezimu ne-grid skaliranja, a sema nakon
        // ucitavanja sa strima nije u ovom rezimu

        bw.Write((int)size);
        bw.Write(visina);
        bw.Write(sirina);
        bw.Write(pinRazmak);
        bw.Write(duzinaPina);
        bw.Write(skala);
        int brojKola = kola.Count;
        bw.Write(brojKola);
        for (int i = 0; i < brojKola; i++)  // smesta kola na strim
        {
            Kolo kolo = kola[i];
            TipKola tipKola = kolo.dajTip();
            bw.Write((int)tipKola); // prvo smesta tip kola
            kolo.saveToStream(bw);     // a zatim i kolo
        }
        int brojVeza = veze.Count;
        bw.Write(brojVeza);
        for (int i = 0; i < brojVeza; i++)   // smesta veze na strim
        {
            veze[i].saveToStream(bw);
        }
        int brojCvorova = cvorovi.Count;
        bw.Write(brojCvorova);
        Cvor cvor;
        for (int i = 0; i < brojCvorova; i++)    // smesta cvorove na strim
        {
            cvor = cvorovi[i];
            cvor.saveToStream(bw);
        }
        bw.Write(IDBrojac);
    }

    public bool unutar(int x, int y)
    {
        return x >= 0 && x < sirina && y >= 0 && y < visina;
    }

    public void prekini()
    {
        switch (stanje)
        {
            case Stanje.Pomeranje1:
            case Stanje.Pomeranje2:
            case Stanje.Laso1:
            case Stanje.Laso2:
                if (stanje == Stanje.Laso1 || stanje == Stanje.Laso2)
                    prekiniCrtanjeLasa();
                else if (stanje == Stanje.Pomeranje2)  // poceto pomeranje
                    prekiniPomeranje();
                else if (stanje == Stanje.Pomeranje1) // nije poceto pomeranje
                    pritisnutMis = false;
                break;

            case Stanje.Kolo1:
            case Stanje.Kolo2:
                prekiniStavljanjeKola();
                break;

            case Stanje.Veza1:
            case Stanje.Veza2:
                prekiniCrtanjeVeze();
                break;

            case Stanje.Kopija1:
            case Stanje.Kopija2:
                prekiniStavljanjeKopije();
                break;
        }
        stanje = Stanje.None;
    }

    private void prekiniCrtanjeLasa()
    {
        pritisnutMis = false;
        form.nacrtajSemu();
        stanje = Stanje.None;
    }

    private void prekiniPomeranje()
    {
        pritisnutMis = false;
        if (stanje == Stanje.Pomeranje2)
        {
            kmdPomeri.zavrsiPomeranje();
            kmdPomeri.stavi();
            kmdPomeri.undo();
        }
        stanje = Stanje.None;
        form.nacrtajSemu();
    }

    private void prekiniStavljanjeKola()
    {
        if (pritisnutMis)
        {
            pritisnutMis = false;
            form.nacrtajSemu();
        }
    }

    private void prekiniCrtanjeVeze()
    {
        stanje = Stanje.None;
        form.nacrtajSemu();
    }

    private void prekiniStavljanjeKopije()
    {
        stanje = Stanje.None;
        pritisnutMis = false;
        form.tbSelektovanje.Checked = true;
        form.nacrtajSemu();
    }

    public void mouseDown(MouseEventArgs e)
    {
        switch (stanje)
        {
            case Stanje.None:
                switch (presenter.rezimRada)
                {
                    case RezimRada.Selektovanje:
                        selektovanjeMouseDown(e);
                        break;
                    case RezimRada.StavljanjeKola:
                        stavljanjeKolaMouseDown(e);
                        break;
                    case RezimRada.StavljanjeVeze:
                        stavljanjeVezeMouseDown(e);
                        break;
                }
                break;

            case Stanje.Pomeranje1:
            case Stanje.Pomeranje2:
            case Stanje.Laso1:
            case Stanje.Laso2:
                selektovanjeMouseDown(e);
                break;

            case Stanje.Kolo1:
            case Stanje.Kolo2:
                stavljanjeKolaMouseDown(e);
                break;

            case Stanje.Veza1:
            case Stanje.Veza2:
                stavljanjeVezeMouseDown(e);
                break;

            case Stanje.Kopija1:
            case Stanje.Kopija2:
                stavljanjeKopijeMouseDown(e);
                break;
        }
    }

    public void selektovanjeMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;

        int semaX = e.X + form.delta.X;
        int semaY = e.Y + form.delta.Y;
        if (unutar(semaX, semaY))
        {
            pritisnutMis = true;
            selObjekt = nadjiObjekt(semaX, semaY);
            if (selObjekt != null)
            {
                stanje = Stanje.Pomeranje1;
                if (Control.ModifierKeys == Keys.Control)
                    prethodnoSelektovan = selObjekt.selektovan();
                else if (!selObjekt.selektovan())
                    deselektuj();
                selObjekt.selektuj();
            }
            else  // nije selektovana ni veza ni kolo
            {
                stanje = Stanje.Laso1;  // pocni crtanje lasa
                deselektuj();
            }

            snapToGrid(ref semaX, ref semaY);
            // TODO: Mozda ovde ne bi trebalo kreirati novu tacku vec dodeljivati
            // vrednosti za poc.X i poc.Y da se ne bi kreirao novi objekat na heapu
            // (ispitati da li se ovde kreira novi objekat na heapu).
            // Izmeniti ovo i na ostalim mestima)
            poc = new Point(semaX, semaY);
            preth = poc;
            form.nacrtajSemu();
        }
    }

    private GrafObjekt nadjiObjekt(int semaX, int semaY)
    {
        GrafObjekt result = null;
        pogodjenCvor = traziCvor(semaX, semaY);
        if (pogodjenCvor != null)
        {
            if (pogodjenCvor.dajBrojVeza() > 0)  // pronadjen je cvor
                // od veza u cvoru, daj onu koja je najbliza
                result = pogodjenCvor.dajVezu(semaX, semaY);
            else
                pogodjenCvor = null;
        }
        if (result == null)
            // nije pronadjen cvor, pa proveri vezu
            result = traziVezu(semaX, semaY);
        if (result == null)
            // nije pronadjena veza, pa proveri kolo
            result = traziKolo(semaX, semaY);
        return result;
    }

    public void stavljanjeKolaMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;

        int semaX = e.X + form.delta.X;
        int semaY = e.Y + form.delta.Y;
        if (unutar(semaX, semaY))
        {
            stanje = Stanje.Kolo1;
            pritisnutMis = true;
            snapToGrid(ref semaX, ref semaY);
            stavljanoKolo = kreirajKolo(form.tipKola, form.brojUlaza[(int)form.tipKola],
                new Point(semaX, semaY));
            stavljanoKolo.pocniPomeranje();
            preth = new Point(semaX, semaY);
            form.nacrtajSemu();
        }
    }

    private Kolo kreirajKolo(TipKola tipKola, int brojUlaza, Point goreLevo)
    {
        switch (tipKola)
        {
            case TipKola.Ne:
                return new Ne(goreLevo, this, dajNoviID());

            case TipKola.I:
                return new I(goreLevo, brojUlaza, this, dajNoviID());

            case TipKola.Ili:
                return new Ili(goreLevo, brojUlaza, this, dajNoviID());

            case TipKola.Ni:
                return new Ni(goreLevo, brojUlaza, this, dajNoviID());

            case TipKola.Nili:
                return new NIli(goreLevo, brojUlaza, this, dajNoviID());

            case TipKola.ExIli:
                return new ExIli(goreLevo, brojUlaza, this, dajNoviID());

            default:
                return null;
        }
    }

    public void stavljanjeVezeMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;

        int semaX = e.X + form.delta.X;
        int semaY = e.Y + form.delta.Y;
        if (unutar(semaX, semaY))
        {
            snapToGrid(ref semaX, ref semaY);
            Point semaPt = new Point(semaX, semaY);
            if (stanje == Stanje.None)
            {
                stanje = Stanje.Veza1;
                pocniCrtanjeVeze(semaPt);
            }
            else   //  pritisnut je mis i izdata komanda za stavljanje veze
            {
                if (semaPt != preth)
                {
                    // da bi algoritam korektno radio, potrebno je da koordinate 
                    // na kojima je mis pritisnut budu jednaki koordinatama koje 
                    // je mis imao kada su segmenti zadnji put pomereni  
                    return;
                }
                if (segment1.getDuzina() == 0 && segment2.getDuzina() == 0)
                {
                    // prekida se sa crtanjem kada se mis pritisne u pocetnoj tacki
                    form.prekini();
                    return;
                }

                Segment segmentVeza = segment1;
                Point krajVeze = getUgao(segment1, segment2);
                if (segment1.getDuzina() == 0)
                {
                    segmentVeza = segment2;
                    krajVeze = semaPt;
                }
                staviVezu(segmentVeza, krajVeze);
                modified = true;
                form.updateInterface();

                // prekini ili nastavi sa crtanjem veze
                Cvor c = dajCvor(krajVeze);
                if (c == null || c.dajBrojVeza() > 1 || c.dajBrojPinova() > 0)
                {
                    // c == null je situacija kada je kraj nove veze na postojecoj
                    // vezi i pritom se nova veza merdzuje sa susednim vezama. Tada
                    // na mestu kraja veze nema cvora
                    form.prekini();
                    return;
                }
                // azuriraj segmente za nastavak crtanja veze
                if (segment1.getDuzina() != 0)
                {
                    // drugi segment(koji nije pretvoren u vezu) postaje prvi segment
                    // (koji ce sledeci biti pretvoren u vezu)
                    promeniSegmente(semaPt);
                }
                else
                {
                    // kreiraj oba segmenta duzine nula
                    initSegments(semaPt);
                }
                form.nacrtajSemu(); // mora da se poziva NAKON sto se azuriraju
                                    // segmenti, da bi se segmenti korektno prikazali
                                    // na semi
            }
            preth = semaPt;
        }
    }

    private void pocniCrtanjeVeze(Point begin)
    {
        initSegments(begin);
    }

    private Point getUgao(Segment segment1, Segment segment2)
    {
        if (segment1.dajKraj() == segment2.dajPocetak()
        || segment1.dajKraj() == segment2.dajKraj())
            return segment1.dajKraj();
        else
            return segment1.dajPocetak();
    }

    private void staviVezu(Segment segment, Point kraj)
    {
        Point pocetakVeze = segment.dajPocetak();
        Point krajVeze = segment.dajKraj();
        if (segment.dajPocetak() == kraj)
        {
            pocetakVeze = segment.dajKraj();
            krajVeze = segment.dajPocetak();
        }
        staviVezu(pocetakVeze, krajVeze);
    }

    // metod koji pretvara segment2 u segment1, i kreira novi segment2 duzine nula na
    // kraju novog segment1 (sto je odredjeno parametrom kraj)
    private void promeniSegmente(Point kraj)
    {
        // Segmenti 1 i 2 menjaju mesta
        segment1 = segment2;
        segment1.postaviStepeneSlobode(
            new StepeniSlobode(new Pravac[] { segment1.dajPravac() }),
            new StepeniSlobode(new Pravac[] { }), kraj);
        segment2 = new RastegljivSegment(kraj, kraj, this, 0,
            new StepeniSlobode(new Pravac[] { segment1.dajPravac() }),
            new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }));
        segment2.postaviPravac(Prostor.suprotanPravac(segment1.dajPravac()));
        segment2.pocniPomeranje();
    }

    private void initSegments(Point begin)
    {
        // kreiraj prvi segment(duzine nula)
        segment1 = new RastegljivSegment(begin, begin, this, 0,
            new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }),
            new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }));
        // kreiraj drugi segment(duzine nula)
        segment2 = new RastegljivSegment(begin, begin, this, 0,
            new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }),
            new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }));
        segment1.pocniPomeranje();
        segment2.pocniPomeranje();
    }

    public void stavljanjeKopijeMouseDown(MouseEventArgs e)
    {
        // TODO: Kada se u rezimu stavljanja kola ili veze pritisne dugme Copy pa zatim dugme
        // Paste, postaju selektovana i dugme za Selektovanje i dugme za Kolo.
        // Takodje sredi situaciju kada se pojavljuje Timer20 (tada mora da se prekine
        // sve)
        // Dodaj skrolovanje seme tockicem
        // Preimenuj crtaSeLaso u crtaSePravouugaonik (i ostala mesta gde se pominje
        // laso preimenuj u pravougaonik)


        if (e.Button != MouseButtons.Left)
            return;

        int semaX = e.X + form.delta.X;
        int semaY = e.Y + form.delta.Y;
        if (unutar(semaX, semaY))
            pritisnutMis = true;
    }

    public void mouseMove(MouseEventArgs e)
    {
        switch (stanje)
        {
            case Stanje.Pomeranje1:
            case Stanje.Pomeranje2:
            case Stanje.Laso1:
            case Stanje.Laso2:
                selektovanjeMouseMove(e);
                break;

            case Stanje.Kolo1:
            case Stanje.Kolo2:
                stavljanjeKolaMouseMove(e);
                break;

            case Stanje.Veza1:
            case Stanje.Veza2:
                stavljanjeVezeMouseMove(e);
                break;

            case Stanje.Kopija1:
            case Stanje.Kopija2:
                stavljanjeKopijeMouseMove(e);
                break;
        }
    }

    public void selektovanjeMouseMove(MouseEventArgs e)
    {
        int x = e.X;
        int y = e.Y;
        presenter.snapToGrid(ref x, ref y);
        form.updateStatusBarCoordinates(x, y);
        if (!pritisnutMis)
            return;
        
        Point pomeraj = new Point(x + form.delta.X - preth.X, y + form.delta.Y - preth.Y);
        if (pomeraj == Point.Empty)
            return;

        if (stanje == Stanje.Laso1 || stanje == Stanje.Laso2)
        {
            stanje = Stanje.Laso2;
            // samo se azurira preth i crta sema (a to se obavlja na kraju if naredbe)
        }
        else
        {
            if (stanje == Stanje.Pomeranje1) // selektovani elementi jos nisu pomerani
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                    stanje = Stanje.Kopija2;
                    // TODO: StavljanjeKopije ne bi trebao da bude poseban rezim rada
                    // vec verovatno podrezim rezima selektovanja, koji bi bio
                    // odredjivan bool promenljivom stavljanjeKopije
                    pocniStavljanjeKopijeSelektovanih(pomeraj);
                }
                else
                {
                    stanje = Stanje.Pomeranje2;
                    pocniPomeranjeSelektovanih(pomeraj);
                }
            }
            else // selektovani elemente su pomerani
            {
                kmdPomeri.pomeri(pomeraj.X, pomeraj.Y);
            }
        }
        preth.Offset(pomeraj);
        form.nacrtajSemu();
    }

    private void pocniStavljanjeKopijeSelektovanih(Point pomeraj)
    {
        stanje = Stanje.Kopija2;
        List<GrafObjekt> selektovani = dajSelektovane();
        deselektuj();
        blok = new Blok(this, 0);
        for (int i = 0; i < selektovani.Count; i++)
        {
            GrafObjekt grafObjekt = selektovani[i];
            if (grafObjekt is Kolo)
                blok.dodajObjekt(grafObjekt.dajKopiju());
            else if (grafObjekt is Veza)
            {
                Veza veza = grafObjekt as Veza;
                veza.postaviStepeneSlobode(
                    new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }),
                    new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }),
                    veza.dajPrviCvor().dajPocetak());
                blok.dodajObjekt(veza.dajKopiju());
            }
        }
        blok.pocniPomeranje();
        blok.pomeri(pomeraj.X, pomeraj.Y);
    }

    private void pocniPomeranjeSelektovanih(Point pomeraj)
    {
        stanje = Stanje.Pomeranje2;
        List<GrafObjekt> selektovani = dajSelektovane();
        deselektuj();
        bool povuci = true;
        if (Control.ModifierKeys == Keys.Alt)
            povuci = false;
        if (selektovani.Count > 1)
            kmdPomeri = new KmdPomeriGrupu(selektovani);
        else
        {
            if (selObjekt is Kolo)
                kmdPomeri = new KmdPomeriKolo(selObjekt as Kolo, povuci);
            else if (selObjekt is Veza)
            {
                if (pogodjenCvor != null)
                    kmdPomeri = new KmdPomeriVezu(selObjekt as Veza, pogodjenCvor);
                else
                    kmdPomeri = new KmdPomeriVezu(selObjekt as Veza, povuci);
            }
        }
        kmdPomeri.ukloni();
        kmdPomeri.pocniPomeranje();
        kmdPomeri.pomeri(pomeraj.X, pomeraj.Y);
    }

    public void stavljanjeKolaMouseMove(MouseEventArgs e)
    {
        int x = e.X;
        int y = e.Y;
        presenter.snapToGrid(ref x, ref y);
        form.updateStatusBarCoordinates(x, y);

        Point pomeraj = new Point(x + form.delta.X - preth.X, y + form.delta.Y - preth.Y);
        if (pomeraj == Point.Empty)
            return;
        if (pritisnutMis)
        {
            stanje = Stanje.Kolo2;
            stavljanoKolo.pomeri(pomeraj.X, pomeraj.Y);
            preth.Offset(pomeraj);
            form.nacrtajSemu();
        }
    }

    public void stavljanjeVezeMouseMove(MouseEventArgs e)
    {
        int x = e.X;
        int y = e.Y;
        presenter.snapToGrid(ref x, ref y);
        form.updateStatusBarCoordinates(x, y);
        if (stanje == Stanje.None)
            return;

        Point pomeraj = new Point(x + form.delta.X - preth.X, y + form.delta.Y - preth.Y);
        if (pomeraj == Point.Empty)
            return;

        stanje = Stanje.Veza2;
        if (segment1.getDuzina() == 0 && segment2.getDuzina() == 0)
        {
            // i prvi i drugi segment imaju duzinu nula(sto moze da se desi u 
            // trenutku kada je poceto crtanje veze(kada je pritisnut mis) ili kada
            // je korisnik ponovo doveo misa u tacku iz koje je poceto crtanje);
            // u toj situaciji pravac veze (tj. pravac segmenta1) ce biti u onom pravcu
            // u kome je mis pomeren
            initPravceIStepeneSlobodeSegmenata(pomeraj);
        }
        segment1.pomeri(pomeraj.X, pomeraj.Y);
        segment2.pomeri(pomeraj.X, pomeraj.Y);

        if (segment1.getDuzina() == 0 && segment2.getDuzina() != 0
        && Control.ModifierKeys == Keys.Shift)
        {
            promeniSegmente(preth + (Size)pomeraj);
        }
        preth.Offset(pomeraj);
        form.nacrtajSemu();
    }

    private void initPravceIStepeneSlobodeSegmenata(Point pomeraj)
    {
        if (segment1.getDuzina() != 0 || segment2.getDuzina() != 0)
            return;
        
        Pravac pravacVeze = Pravac.Vert;
        if (Math.Abs(pomeraj.X) >= Math.Abs(pomeraj.Y))
            pravacVeze = Pravac.Hor;
        segment1.postaviPravac(pravacVeze);
        segment2.postaviPravac(Prostor.suprotanPravac(pravacVeze));
        segment1.postaviStepSlobodePoc(new StepeniSlobode(new Pravac[] { }));
        segment1.postaviStepSlobodeKraj(new StepeniSlobode(new Pravac[] { pravacVeze }));
        segment2.postaviStepSlobodePoc(new StepeniSlobode(new Pravac[] { pravacVeze }));
        segment2.postaviStepSlobodeKraj(new StepeniSlobode(new Pravac[] { Pravac.Hor, Pravac.Vert }));
    }

    public void stavljanjeKopijeMouseMove(MouseEventArgs e)
    {
        int x = e.X;
        int y = e.Y;
        presenter.snapToGrid(ref x, ref y);
        form.updateStatusBarCoordinates(x, y);

        Point pomeraj = new Point(x + form.delta.X - preth.X, y + form.delta.Y - preth.Y);
        if (pomeraj == Point.Empty)
            return;

        if (stanje == Stanje.Kopija1)
            pocniPomeranjeKopije(preth + (Size)pomeraj);
        else if (pomeraj != Point.Empty)
            blok.pomeri(pomeraj.X, pomeraj.Y);
        stanje = Stanje.Kopija2;

        preth.Offset(pomeraj);
        form.nacrtajSemu();
    }

    private void pocniPomeranjeKopije(Point semaPt)
    {
        stanje = Stanje.Kopija2;
        blok = dajClipBoard();
        blok.pocniPomeranje();
        Point pomeraj = semaPt - (Size)blok.dajPocetak();
        blok.pomeri(pomeraj.X, pomeraj.Y);
    }

    public void mouseUp(MouseEventArgs e)
    {
        switch (stanje)
        {
            case Stanje.Pomeranje1:
            case Stanje.Pomeranje2:
            case Stanje.Laso1:
            case Stanje.Laso2:
                selektovanjeMouseUp(e);
                break;

            case Stanje.Kolo1:
            case Stanje.Kolo2:
                stavljanjeKolaMouseUp(e);
                break;

            case Stanje.Kopija1:
            case Stanje.Kopija2:
                stavljanjeKopijeMouseUp(e);
                break;
        }
    }

    public void selektovanjeMouseUp(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;
        if (!pritisnutMis)
            return;

        if (stanje == Stanje.Laso1 || stanje == Stanje.Laso2)
        {
            zavrsiCrtanjeLasa();
        }
        else if (stanje == Stanje.Pomeranje2)
        {
            zavrsiPomeranje();
        }
        else     // element je bio selektovan ali nije bio pomeran
        {
            pritisnutMis = false;
            if (Control.ModifierKeys == Keys.Control && prethodnoSelektovan)
                selObjekt.deselektuj();
        }
        stanje = Stanje.None;
        form.nacrtajSemu();
        form.updateInterface();
    }

    private void zavrsiCrtanjeLasa()
    {
        pritisnutMis = false;
        selektuj(form.getRectangle(poc.X, poc.Y, preth.X, preth.Y));
        stanje = Stanje.None;
    }

    private void zavrsiPomeranje()
    {
        pritisnutMis = false;
        stanje = Stanje.None;
        kmdPomeri.zavrsiPomeranje();
        kmdPomeri.stavi();
        dodajKomandu(kmdPomeri);
        modified = true;
    }

    public void stavljanjeKolaMouseUp(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && pritisnutMis)
        {
            pritisnutMis = false;
            stavljanoKolo.zavrsiPomeranje();
            staviKolo(stavljanoKolo);
            stavljanoKolo = null;
            modified = true;
            stanje = Stanje.None;
            form.nacrtajSemu();
            form.updateInterface();
        }
    }

    public void stavljanjeKopijeMouseUp(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            pritisnutMis = false;
            deselektuj();
            blok.zavrsiPomeranje();
            paste(blok);
            form.nacrtajSemu();
            modified = true;
            stanje = Stanje.None;
            form.updateInterface();
            form.tbSelektovanje.Checked = true;
        }
    }

    public bool moveObjectOnDragScroll(Point pomerajObjekta)
    {
        if (pomerajObjekta == Point.Empty)
            return false;

        bool moved = false;
        switch (stanje)
        {
            case Stanje.Pomeranje1:
            case Stanje.Pomeranje2:
            case Stanje.Laso1:
            case Stanje.Laso2:
                if (stanje == Stanje.Laso2)
                {
                    preth.Offset(pomerajObjekta);
                    moved = true;
                }
                else if (stanje == Stanje.Pomeranje2)
                {
                    kmdPomeri.pomeri(pomerajObjekta.X, pomerajObjekta.Y);
                    preth.Offset(pomerajObjekta);
                    moved = true;
                }
                break;

            case Stanje.Kolo1:
            case Stanje.Kolo2:
                if (pritisnutMis)
                {
                    stavljanoKolo.pomeri(pomerajObjekta.X, pomerajObjekta.Y);
                    preth.Offset(pomerajObjekta);
                    moved = true;
                }
                break;

            case Stanje.Kopija1:
            case Stanje.Kopija2:
                if (pritisnutMis)
                {
                    blok.pomeri(pomerajObjekta.X, pomerajObjekta.Y);
                    preth.Offset(pomerajObjekta);
                    moved = true;
                }
                break;

            default:
                break;
        }
        return moved;
    }

    public Stanje dajStanje()
    {
        return stanje;
    }

    public void rotate()
    {
        Stanje stanje = dajStanje();
        if (dajBrojSelektovanih() > 0
        || stanje == Stanje.Kolo2
        || stanje == Stanje.Kopija2)
        {
            if (!manipulating())
            {
                rotiraj();
                modified = true;
                form.nacrtajSemu();
                form.updateInterface();
                return;
            }

            switch (stanje)
            {
                case Stanje.Kolo2:
                    stavljanoKolo.rotiraj(stavljanoKolo.dajTeziste(), 1);
                    stavljanoKolo.pomeri(0, 0);
                    form.nacrtajSemu();
                    break;

                case Stanje.Kopija2:
                    if (blok.canRotate())
                    {
                        blok.rotiraj(blok.dajTeziste(), 1);
                        blok.pomeri(0, 0);
                        form.nacrtajSemu();
                    }
                    break;

                default:
                    break;
            }
        }
    }

    public void paste()
    {
        if (canPaste())
        {
            if (!manipulating())
            {
                // TODO: Paste bi trebalo da bude dostupno i preko context menija, a
                // tada bi trebalo odmah nacrtati blok kada se izda naredba paste.
                stanje = Stanje.Kopija1;
            }
        }
    }

    public bool rotateApplicable()
    {
        Stanje stanje = dajStanje();
        return (dajBrojSelektovanih() > 0) || (stanje == Stanje.Kolo2)
            || (stanje == Stanje.Kopija2);
    }

    public bool manipulating()
    {
        return stanje != Stanje.None;
    }

    public void ukloniVezu(Veza veza)
    {
        veza.odspoji();
        veze.Remove(veza);
    }

    public void dodajVezu(Veza veza)
    {
        veze.Add(veza);

        // spoji vezu na semu
        Cvor prviCvor = dajCvor(veza.dajPocetak());
        if (prviCvor == null)
            prviCvor = createCvor(veza.dajPocetak());
        Cvor drugiCvor = dajCvor(veza.dajKraj());
        if (drugiCvor == null)
            drugiCvor = createCvor(veza.dajKraj());
        veza.spoji(prviCvor, drugiCvor);
    }

    // kreira cvor na mestu Polozaj i vraca ga kao rezultat; podrazumeva se
    // da je prethodno provereno da na mestu Polozaj ne postoji cvor
    private Cvor createCvor(Point polozaj)
    {
        Cvor result = new Cvor(polozaj, this, dajNoviID());
        cvorovi.Add(result);
        return result;
    }

    public string brojevi()
    {
        return String.Format("Veze: {0}\nCvorovi: {1}", veze.Count, cvorovi.Count);
    }
}

public class CvorComparer : IComparer<Cvor>
{
    private Pravac pravac;

    public CvorComparer(Pravac pravac)
    {
        this.pravac = pravac;
    }

    public int Compare(Cvor x, Cvor y)
    {
        if (x.dajKoordinatu(pravac) > y.dajKoordinatu(pravac))
            return 1;
        else if (x.dajKoordinatu(pravac) < y.dajKoordinatu(pravac))
            return -1;
        else
            return 0;
    }
}
