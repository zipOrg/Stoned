using UnityEngine;
using System.Collections;

public class PlayerAnimationEventHandler : MonoBehaviour {
    public Player playerScript;
   
    private void AnimationEvent(string functionName)
    {
        playerScript.Invoke(functionName,0.0f);
    }


}
