// Prostor.cs
// Written by Sasa Stankovic

using System.Drawing;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public enum SemaSize { A4, A3, A2, A1, A0 }

public enum Pravac { Hor, Vert, None }

public enum Smer { Ist, Sev, Zap, Jug, None }

public struct StepeniSlobode
{
    private bool hor;
    private bool vert;

    public StepeniSlobode(Pravac[] arr)
    {
        hor = false;
        vert = false;
        foreach (Pravac p in arr)
        {
            if (p == Pravac.Hor)
                hor = true;
            else
                vert = true;
        }
    }

    public bool contains(Pravac p)
    {
        if (p == Pravac.Hor)
            return hor;
        else
            return vert;
    }

    public void Remove(Pravac p)
    {
        if (p == Pravac.Hor)
            hor = false;
        else
            vert = false;
    }

    public StepeniSlobode(BinaryReader br)
    {
        hor = br.ReadBoolean();
        vert = br.ReadBoolean();
    }

    public void SaveToStream(BinaryWriter bw)
    {
        bw.Write(hor);
        bw.Write(vert);
    }
}

public class Prostor
{
    public static int sizeToWidth(SemaSize Size)
    { 
        if (Size == SemaSize.A4)
            return 116;
        else
            return 2 * sizeToHeight((SemaSize)((int)Size - 1)); 
    }

    public static int sizeToHeight(SemaSize Size)
    {
        if (Size == SemaSize.A4)
            return 82;
        else
            return sizeToWidth((SemaSize)((int)Size - 1));
    }

    public static Pravac getPravac(Smer Smer)
    {
        switch (Smer)
        {
            case Smer.Ist:
            case Smer.Zap:
                return Pravac.Hor;

            case Smer.Sev:
            case Smer.Jug:
                return Pravac.Vert;

            default:
                return 0;
        }
    }

    public static Pravac getPravac(Point pocetak, Point kraj)
    {
        if (pocetak.Y == kraj.Y)
            return Pravac.Hor;
        else if (pocetak.X == kraj.X)
            return Pravac.Vert;
        else
            return Pravac.None;
    }

    public static Pravac suprotanPravac(Pravac Pravac)
    {
        if (Pravac == Pravac.Hor)
            return Pravac.Vert;
        else
            return Pravac.Hor;
    }

    public static Smer suprotanSmer(Smer Smer)
    {
        switch(Smer)
        {
            case Smer.Ist:
                return Smer.Zap;
            case Smer.Zap:
                return Smer.Ist;
            case Smer.Sev:
                return Smer.Jug;
            case Smer.Jug:
                return Smer.Sev;
            default:
                return 0;
        }
    }

    public static void orijentisi(Pravac Pravac, ref Smer Preth, ref Smer Sled)
    {
        if (Pravac == Pravac.Hor)
        {
            Preth = Smer.Zap;
            Sled = Smer.Ist;
        }
        else
        {
            Preth = Smer.Sev;
            Sled = Smer.Jug;
        }
    }

    public static void orijentisi(Pravac Pravac, ref Smer PrethHor, ref Smer SledHor, 
        ref Smer PrethVert, ref Smer SledVert)
    {
        orijentisi(Pravac, ref PrethHor, ref SledHor);
        orijentisi(suprotanPravac(Pravac), ref PrethVert, ref SledVert);
    }

    public static void orijentisi(Smer Smer, ref Smer SuprSmer, ref Smer PrethVert, ref Smer SledVert)
    {
        SuprSmer = suprotanSmer(Smer);
        orijentisi(suprotanPravac(getPravac(Smer)), ref PrethVert, ref SledVert);
    }

    public static Smer rotirajSmer(Smer Smer, int SmerRot)
    {
        if (SmerRot == 1)
        {
            switch (Smer)
            {
                case Smer.Ist:
                    return Smer.Sev;
                case Smer.Sev:
                    return Smer.Zap;
                case Smer.Zap:
                    return Smer.Jug;
                case Smer.Jug:
                    return Smer.Ist;
            }
        }
        else
        {
            switch (Smer)
            {
                case Smer.Ist:
                    return Smer.Jug;
                case Smer.Sev:
                    return Smer.Ist;
                case Smer.Zap:
                    return Smer.Sev;
                case Smer.Jug:
                    return Smer.Zap;
            }
        }
        return 0;
    }

    public static void rotirajTacku(ref Point Tacka, Point CentarRot, int SmerRot)
    {
        Point Tmp = Tacka;
        Tacka.X = CentarRot.X + SmerRot * (Tacka.Y - CentarRot.Y);
        Tacka.Y = CentarRot.Y - SmerRot * (Tmp.X - CentarRot.X);
    }

    public static void order(ref Point pocetak, ref Point kraj)
    {
        Pravac pravac = Prostor.getPravac(pocetak, kraj);
        if (pravac == Pravac.None)
            return;

        bool swap = false;
        if (pravac == Pravac.Hor)
        {
            if (pocetak.X > kraj.X)
                swap = true;
        }
        else
        {
            if (pocetak.Y > kraj.Y)
                swap = true;
        }
        if (swap)
        {
            Point tmp = pocetak;
            pocetak = kraj;
            kraj = tmp;
        }
    }
}