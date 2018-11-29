using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotRockerEvent : MonoBehaviour {

    public ChaC chac;
    public void OnRocker(RockerEventParam rep)
    {
        switch (rep.rockerAction)
        {
            case RockerAction.Dragging:
            case RockerAction.Hold:
                chac.Rot(rep.nowPosition.x, rep.nowPosition.y);
                break;
            default:
                break;
        }
    }
}
