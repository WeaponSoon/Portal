using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRockerEvent : MonoBehaviour {
    public ChaC chac;
	public void OnRocker(RockerEventParam rep)
    {
        switch(rep.rockerAction)
        {
            case RockerAction.Dragging:
            case RockerAction.Hold:
                chac.Move(rep.nowPosition.y, rep.nowPosition.x);
                break;
            default:
                break;
        }
    }
}
