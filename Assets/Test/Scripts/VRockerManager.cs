
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VRockerManager  {

    private static bool rightIsDown = false;
    private static bool leftIsDown = false;
    private static Vector2 rightDir = new Vector2();
    private static Vector2 leftDir = new Vector2();

    private static bool lastrightIsDown = false;
    private static bool lastleftIsDown = false;
    private static Vector2 lastrightDir = new Vector2();
    private static Vector2 lastleftDir = new Vector2();

    public static bool leftState { get { return leftIsDown; } }
    public static bool rightState { get { return rightIsDown; } }
    public static Vector2 leftMove { get { return leftDir; } }
    public static Vector2 rightMove { get { return rightDir; } }

    private static int passBallButton = 0;

    public static void Init()
    {
        Reset();
        //SyncOtherBehaviour.syncFunc += SendRockerMsg;
        //ScenesManager.cleanMethods += Reset;
    }

    private static void Reset()
    {
        rightIsDown = false;
        leftIsDown = false;
        rightDir = new Vector2();
        leftDir = new Vector2();

        lastrightIsDown = false;
        lastleftIsDown = false;
        lastrightDir = new Vector2();
        lastleftDir = new Vector2();

        passBallButton = 0;
    }

    

    private static bool isRightEqual { get { return rightIsDown == lastrightIsDown && rightDir == lastrightDir; } }
    private static bool isLeftEqual { get { return leftIsDown == lastleftIsDown && leftDir == lastleftDir; } }

    public static void SetRockers(byte id, bool isDown, Vector2 dir)
    {
        switch (id)
        {
            case 1:
                rightIsDown = isDown;
                rightDir = dir;
                break;
            case 0:
                leftIsDown = isDown;
                leftDir = dir;
                break;
            default:
                break;
        }
    }

    public static void SetPassBall()
    {
        ++passBallButton;
    }

    public static string info {
        get
        {
            return "Right State : isDown " + rightIsDown + " dir " + rightDir + "\nLeft State : isDown " + leftIsDown + " dir " + leftDir;
        }
    }
    public static string lastIndo
    {
        get
        {
            return "Last Right State : isDown " + lastrightIsDown + " dir " + lastrightDir + "\nLast Left State : isDown " + lastleftIsDown + " dir " + lastleftDir;
        }
    }

    public static void SendRockerMsg()
    {

        //if (passBallButton > 0)
        //{
        //    MessageManager.SendSyncTools(0);
        //    --passBallButton;
        //}

        //if (!isLeftEqual && !isRightEqual)
        //{
        //    lastrightDir = rightDir;
        //    lastleftDir = leftDir;
        //    lastrightIsDown = rightIsDown;
        //    lastleftIsDown = leftIsDown;
        //    MessageManager.SendSyncDoubleRocker(rightIsDown, leftIsDown, rightDir, leftDir);
        //}
        //else if (!isLeftEqual)
        //{
        //    lastleftIsDown = leftIsDown;
        //    lastleftDir = leftDir;
        //    MessageManager.SendSyncRocker(leftIsDown, 0, leftDir);
        //}
        //else if (!isRightEqual)
        //{
        //    lastrightIsDown = rightIsDown;
        //    lastrightDir = rightDir;
        //    MessageManager.SendSyncRocker(rightIsDown, 1, rightDir);
        //}

    }
}
