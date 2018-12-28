using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public static class UtilHelper
{
    public static void AddAsSet<T>(this List<T> me, T other)
    {
        if(!me.Contains(other))
        {
            me.Add(other);
        }
    }

    public static List<T> Intersect<T>(this List<T> me, List<T> other)
    {
        var ret = new List<T>();
        for(int i = 0; i < me.Count; ++i)
        {
            if(other.Contains(me[i]))
            {
                ret.Add(me[i]);
            }
        }
        return ret;
    }
    public static List<T> Diff<T>(this List<T> me, List<T> other)
    {
        var ret = new List<T>();
        for(int i = 0; i < me.Count; ++i)
        {
            if(!other.Contains(me[i]))
            {
                ret.Add(me[i]);
            }
        }
        return ret;
    }
    public static List<T> Union<T>(this List<T> me, List<T> other)
    {
        var ret = new List<T>();
        ret.AddRange(me);
        for(int i = 0; i < other.Count; ++i)
        {
            ret.AddAsSet(other[i]);
        }
        return ret;
    }
}

