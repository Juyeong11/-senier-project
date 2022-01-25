using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beat
{
    int bar;
    public int addBeat = 0;
    int add24 = 0;
    int add16 = 0;
    int addedMS = 0;

    public Beat()
    {
        bar = 0;
        addBeat = 0;
        add24 = 0;
        add16 = 0;
        addedMS = 0;
    }

    public Beat(int bars, int beats = 0, int quater = 0, int triplet = 0)
    {
        bar = bars;
        addBeat = beats;
        add16 = quater;
        add24 = triplet;
    }

    public void Set(int bars, int beats = 0, int quater = 0, int triplet = 0)
    {
        bar = bars;
        addBeat = beats;
        add16 = quater;
        add24 = triplet;
    }


    public int GetBeatTime()
    {
        if (addedMS == 0)
        {
            return bar * GameManager.data.timeByBar
                + addBeat * GameManager.data.timeByBeat
                + add16 * GameManager.data.timeBy16Beat
                + add24 * GameManager.data.timeBy24Beat;
            }
        else
        {
            return bar * GameManager.data.timeByBar
               + addBeat * GameManager.data.timeByBeat
               + addedMS;
        }
    }
    public void SetBeatTime(int nowTime)
    {
        bar = nowTime / GameManager.data.timeByBar;
        addBeat = nowTime / GameManager.data.timeByBeat % GameManager.data.barCounts;
        addedMS = nowTime - (bar * GameManager.data.timeByBar + addBeat * GameManager.data.timeByBeat);
    }

    //public static bool operator ==(Beat a,Beat b)
    //{
    //    if (a.bar == b.bar &&
    //         a.addBeat == b.addBeat &&
    //         a.add24 == b.add24 &&
    //         a.add16 == b.add16)
    //    {
    //        return true;
    //    }
    //    else return false;
    //}

    //public static bool operator !=(Beat a, Beat b)
    //{
    //    if (a == b)
    //        return false;
    //    else
    //        return true;
    //}

    //public static bool operator ==(Beat a, int time)
    //{
    //    if (a.GetBeatTime() == time)
    //    {
    //        return true;
    //    }
    //    else return false;
    //}

    //public static bool operator !=(Beat a, int time)
    //{
    //    if (a == time)
    //        return false;
    //    else
    //        return true;
    //}

    public static Beat operator -(Beat a, Beat b)
    {

        Beat tmp = (Beat)a.MemberwiseClone();
        tmp.bar -= b.bar;
        tmp.addBeat -= b.addBeat;
        tmp.add16 -= b.add16;
        tmp.add24 -= b.add24;
        return tmp;
    }
    public static Beat operator +(Beat a, Beat b)
    {

        Beat tmp = (Beat)a.MemberwiseClone();
        tmp.bar += b.bar;
        tmp.addBeat += b.addBeat;
        tmp.add16 += b.add16;
        tmp.add24 += b.add24;
        return tmp;
    }

    public static bool operator <=(Beat a, Beat b)
    {
        return a.GetBeatTime() <= b.GetBeatTime();
    }

    public static bool operator >=(Beat a, Beat b)
    {
        return a.GetBeatTime() >= b.GetBeatTime();
    }

    public override bool Equals(object op1)
    {
        Debug.LogError("Please override Equals (of class 'Beat'), now value Ignored by false");
        return false;
    }

    //GetHashCode �� ��ü�� ���¿� ����� ������ int ���� ��µ� ��� �Ѵ�.
    public override int GetHashCode()
    {
        Debug.LogError("Please override GetHashCode (of class 'Beat'), now value Ignored by 0");
        return 0;
    }

    public override string ToString()
    {
        return "(" + bar.ToString() + ", " + addBeat.ToString() + ", " + add16.ToString() + ", " + add24.ToString() + ") : " + GetBeatTime();
    }

}
