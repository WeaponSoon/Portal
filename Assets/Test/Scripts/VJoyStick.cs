using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public enum RockerAction
{
    Down,
    Up,
    Hold,
    Dragging
}

public class RockerEventParam
{
    public RockerAction rockerAction;
    public Vector3 nowPosition;
}

[System.Serializable]
public class RockerEvent : UnityEvent<RockerEventParam>
{

}

public class VJoyStick : MonoBehaviour
{

    public byte rockId;

    public Transform joystickBottom;
    public Transform joystickHead;
    //public Function function;
    public RockerEvent rockerEvent;

    //public UnityEvent RockerEvent;

    public float deadRange = 0.1f;
    private float deadErea;

    public Vector2 originBottomPosition;
    public Vector2 originHeadPosition;

    private Vector2 obp;
    private Vector2 ohp;

    public float moveRange;
    private float maxDistance;
    private bool active;
    private int trackFingureID;
    private int id;
    private Vector2 mPos;
    public bool autoZeroOnReleased;

    public Vector2 bl;
    public Vector2 tr;
    private Vector2 activeMin;
    private Vector2 activeMax;
    private Vector3 lastPos = Vector3.zero;
    // Use this for initialization
    void Awake()
    {
        //bottomImage = joystickBottom.GetComponent<Image>();
        //headImage = joystickHead.GetComponent<Image>();
        //maxDistance = joystickBottom.localScale.sqrMagnitude*10;
        activeMin = new Vector2(bl.x * Screen.width, bl.y * Screen.height);
        activeMax = new Vector2(tr.x * Screen.width, tr.y * Screen.height);
        obp = new Vector2(originBottomPosition.x * Screen.width, originBottomPosition.y * Screen.height);
        ohp = new Vector2(originHeadPosition.x * Screen.width, originHeadPosition.y * Screen.height);
        maxDistance = moveRange * Screen.width;
        deadErea = deadRange * maxDistance;
        
    }


    private void OnEnable()
    {
        active = false;
        ResetAxis();
    }
    /// <summary>
    /// 归零轴
    /// </summary>
    public void ResetAxis()
    {
        joystickHead.localPosition = Vector3.zero;
    }
    // Update is called once per frame
    void Update()
    {
        if (Screen.width != Config.screenWidth || Screen.height != Config.screenHeight)
        {
            Config.screenWidth = Screen.width;
            Config.screenHeight = Screen.height;
            activeMin = new Vector2(bl.x * Screen.width, bl.y * Screen.height);
            activeMax = new Vector2(tr.x * Screen.width, tr.y * Screen.height);
            obp = new Vector2(originBottomPosition.x * Screen.width, originBottomPosition.y * Screen.height);
            ohp = new Vector2(originHeadPosition.x * Screen.width, originHeadPosition.y * Screen.height);
            maxDistance = moveRange * Screen.width;
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        if (active)
            TouchedPC();
        else
            UnTouchedPC();
        return;
#else
        if (active)
            Touched();
        else
            UnTouched();
#endif

    }
    void UnTouched()
    {
        int count = Input.touchCount;
        if (count <= 0)
            return;

        for (int i = 0; i < count; i++)
        {
            mPos = Input.GetTouch(i).position;
            if (mPos.x < activeMin.x || mPos.y < activeMin.y)
                continue;
            if (mPos.x > activeMax.x || mPos.y > activeMax.y)
                continue;
            trackFingureID = Input.GetTouch(i).fingerId;
            active = true;
            //bottomImage.enabled = true;
            //headImage.enabled = true;
            active = true;
            joystickBottom.position = mPos;
            VRockerManager.SetRockers(rockId, true, Vector2.zero);
            if(rockerEvent != null)
                rockerEvent.Invoke(new RockerEventParam { rockerAction = RockerAction.Down, nowPosition = Vector3.zero });
            //LGameObject myPlayer = PlayerManager.FindPlayer(PlayerManager.mySeatId);
            //if (myPlayer != null && myPlayer.GetComponent<Player>() != null && myPlayer.GetComponent<Player>().vPlayer != null)
            //{
            //    myPlayer.GetComponent<Player>().vPlayer.preShow(rockId, Vector3.zero);
            //}
            break;
        }
    }
    void Touched()
    {
        int count = Input.touchCount;
        for (int i = 0; i < count; i++)
        {
            try
            {
                if (Input.GetTouch(i).phase == TouchPhase.Stationary)
                {

                }
            }
            catch
            {
                Debug.LogError("wwwwwwwwwwwwwwwwww");
            }


            if (Input.GetTouch(i).fingerId == trackFingureID)
            {
                id = i;

                try
                {
                    if (Input.GetTouch(id).position != null)
                    {
                        mPos = Input.GetTouch(id).position;
                        break;
                    }
                }
                catch
                {
                    Debug.LogWarning("mmmmmmmmmmmmmmmmmmmmmmmmmmmm");
                }


            }
        }
        Vector3 targetDir = new Vector3(mPos.x, mPos.y, 0) - joystickBottom.position;
        if (targetDir.magnitude < deadErea)
            targetDir = Vector3.zero;
        else if (targetDir.magnitude > maxDistance)
            targetDir = targetDir.normalized * maxDistance;

        joystickHead.position = joystickBottom.position + targetDir;
        VRockerManager.SetRockers(rockId, true, targetDir / maxDistance);
        if (rockerEvent != null)
        {
            Vector3 pos = joystickHead.localPosition / maxDistance;
            if (lastPos != joystickHead.localPosition)
                rockerEvent.Invoke(new RockerEventParam { rockerAction = RockerAction.Dragging, nowPosition = pos });
            else
                rockerEvent.Invoke(new RockerEventParam { rockerAction = RockerAction.Hold, nowPosition = pos });
            lastPos = joystickHead.localPosition;
        }
        //LGameObject myPlayer = PlayerManager.FindPlayer(PlayerManager.mySeatId);
        //if (myPlayer != null && myPlayer.GetComponent<Player>() != null && myPlayer.GetComponent<Player>().vPlayer != null)
        //{
        //    myPlayer.GetComponent<Player>().vPlayer.preShow(rockId, new Vector3(targetDir.x, 0, targetDir.y) / maxDistance);
        //}

        if (Input.GetTouch(id).phase == TouchPhase.Ended || Input.GetTouch(id).phase == TouchPhase.Canceled)
        {
            //bottomImage.enabled = false;
            //headImage.enabled = false;
            if (rockerEvent != null)
            {
                Vector3 pos = joystickHead.localPosition / maxDistance;
                rockerEvent.Invoke(new RockerEventParam { rockerAction = RockerAction.Up, nowPosition = pos });
            }
            active = false;
            if (autoZeroOnReleased)
                joystickHead.localPosition = Vector3.zero;
            joystickBottom.position = obp;
            VRockerManager.SetRockers(rockId, false, targetDir / maxDistance);
            //myPlayer = PlayerManager.FindPlayer(PlayerManager.mySeatId);
            //if (myPlayer != null && myPlayer.GetComponent<Player>() != null && myPlayer.GetComponent<Player>().vPlayer != null)
            //{
            //    myPlayer.GetComponent<Player>().vPlayer.preShow(rockId, new Vector3(targetDir.x, 0, targetDir.y) / maxDistance, false);
            //}

        }
    }

    void UnTouchedPC()
    {
        if (Input.GetMouseButtonDown(0))
        {
            
            mPos = Input.mousePosition;
            if (mPos.x < activeMin.x || mPos.y < activeMin.y)
                return;
            if (mPos.x > activeMax.x || mPos.y > activeMax.y)
                return;
            active = true;
            //bottomImage.enabled = true;
            //headImage.enabled = true;
            active = true;
            joystickBottom.position = mPos;
            VRockerManager.SetRockers(rockId, true, Vector2.zero);
            if(rockerEvent != null)
                rockerEvent.Invoke(new RockerEventParam { rockerAction = RockerAction.Down, nowPosition = Vector3.zero });
        }
    }
    void TouchedPC()
    {
        mPos = Input.mousePosition;
        Vector3 targetDir = new Vector3(mPos.x, mPos.y, 0) - joystickBottom.position;
        if (targetDir.magnitude < deadErea)
            targetDir = Vector3.zero;
        else if (targetDir.magnitude > maxDistance)
            targetDir = targetDir.normalized * maxDistance;
        //Debug.LogWarning(joystickBottom.position);
        //targetDir /= maxDistance;
        joystickHead.position = joystickBottom.position + targetDir;
        VRockerManager.SetRockers(rockId, true, targetDir / maxDistance);

        if (rockerEvent != null)
        {
            Vector3 pos = joystickHead.localPosition / maxDistance;
            if(lastPos != joystickHead.localPosition)
                rockerEvent.Invoke(new RockerEventParam { rockerAction = RockerAction.Dragging, nowPosition = pos });
            else
                rockerEvent.Invoke(new RockerEventParam { rockerAction = RockerAction.Hold, nowPosition = pos });
            lastPos = joystickHead.localPosition;
        }

        //LGameObject myPlayer = PlayerManager.FindPlayer(PlayerManager.mySeatId);
        //if (myPlayer != null && myPlayer.GetComponent<Player>() != null && myPlayer.GetComponent<Player>().vPlayer != null)
        //{
        //    myPlayer.GetComponent<Player>().vPlayer.preShow(rockId, new Vector3(targetDir.x, 0, targetDir.y) / maxDistance);
        //}
        if (Input.GetMouseButtonUp(0))
        {
            if (rockerEvent != null)
            {
                Vector3 pos = joystickHead.localPosition / maxDistance;
                rockerEvent.Invoke(new RockerEventParam { rockerAction = RockerAction.Up, nowPosition = pos });
            }
               
            //bottomImage.enabled = false;
            //headImage.enabled = false;
            active = false;
            if (autoZeroOnReleased)
                joystickHead.localPosition = Vector3.zero;
            joystickBottom.position = obp;
            VRockerManager.SetRockers(rockId, false, targetDir / maxDistance);
            
        }
    }
}
