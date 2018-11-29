using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalPair : MonoBehaviour {

    private static List<PortalPair> _portalPairs = new List<PortalPair>();
    public static List<PortalPair> portalPairs
    {
        get
        {
            return _portalPairs;
        }
    }

    public bool isOpen;

    [SerializeField]
    private Portal prePortalA;
    [SerializeField]
    private Portal prePortalB;

    private Portal _portalA = null;
    public Portal portalA
    {
        set
        {
            if (value == _portalA)
                return;
            if(_portalA != null)
            {
                _portalA.motherPair = null;
                _portalA.OnDisconnected(this);
            }
            _portalA = value;
            if(_portalA != null)
            {
                if(_portalA.motherPair != null)
                {
                    if(_portalA.motherPair.portalA == _portalA)
                    {
                        portalA.motherPair.portalA = null;
                    }
                    else if(_portalA.motherPair.portalB == _portalA)
                    {
                        portalA.motherPair.portalB = null;
                    }
                }
                _portalA.motherPair = this;
                _portalA.OnConnected(this);
            }
        }
        get
        {
            return _portalA;
        }
    }
    private Portal _portalB = null;
    public Portal portalB
    {
        set
        {
            if (value == _portalB)
                return;
            if (_portalB != null)
            {
                _portalB.motherPair = null;
                _portalB.OnDisconnected(this);
            }
            _portalB = value;
            if (_portalB != null)
            {
                if (_portalB.motherPair != null)
                {
                    if (_portalB.motherPair.portalA == _portalB)
                    {
                        portalB.motherPair.portalA = null;
                    }
                    else if (_portalB.motherPair.portalB == _portalB)
                    {
                        portalB.motherPair.portalB = null;
                    }
                }
                _portalB.motherPair = this;
                _portalB.OnConnected(this);
            }
        }
        get
        {
            return _portalB;
        }
    }

    protected virtual void Awake()
    {
        _portalPairs.Add(this);
        if(prePortalA != null)
        {
            portalA = prePortalA;
        }
        if(prePortalB != null)
        {
            portalB = prePortalB;
        }
    }
    protected virtual void OnDestroy()
    {
        _portalPairs.Remove(this);
    }
}
