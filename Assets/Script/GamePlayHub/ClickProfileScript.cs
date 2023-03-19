using GamePlayHub;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickProfileScript : MonoBehaviour
{
    public GamePlayManager GamePlayManager;
    public int ProfileIndex;

    private void OnMouseUp()
    {
        GamePlayManager.OnMouseUpClickProfileScript(ProfileIndex);
    }
}
