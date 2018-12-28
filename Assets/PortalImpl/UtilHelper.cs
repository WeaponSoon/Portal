using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class UtilHelper
{
    public static GameObject InstOnlyRenderer(GameObject origin, Portal portal)
    {
        if (origin == null)
            return null;
        var ret = new GameObject(origin.name);
        if(origin.GetComponent<MeshRenderer>() != null && origin.GetComponent<MeshFilter>() != null)
        {
            ret.AddComponent<MeshFilter>().mesh = origin.GetComponent<MeshFilter>().mesh;
            ret.AddComponent<MeshRenderer>();
            if(portal != null)
            {
                var shadow = ret.AddComponent<OnlyRenderObjFollow>();
                shadow.originTransform = origin.transform;
                shadow.portal = portal;
            }
        }
        for(int i = 0; i < origin.transform.childCount; ++i)
        {
            InstOnlyRenderer(origin.transform.GetChild(i).gameObject, portal).transform.parent = ret.transform;
        }
        return ret;
    }
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

