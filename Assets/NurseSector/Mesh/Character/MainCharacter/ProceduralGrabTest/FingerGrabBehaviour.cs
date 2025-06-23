using UnityEngine;

public class FingerGrabBehaviour : StateMachineBehaviour
{
    [Range(1,5)]
    public int fingerNum;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool($"Finger{fingerNum}Touch", false);
        animator.SetFloat($"Finger{fingerNum}TouchOffset", 0);
    }

    // Override OnStateExit to calculate the remaining animation time and set the cycle offset
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        var NormalizedTimeEnd = stateInfo.normalizedTime;

        //Debug.Log("Finger : " + fingerNum + "  |  StateInfo NormalizedTimeEnd: " + NormalizedTimeEnd + " | layer: " + layerIndex);

        var test = animator.GetBool($"Finger{fingerNum}Touch");

        // if the animation did not finish
        if(NormalizedTimeEnd < 1)
        {
            animator.SetFloat($"Finger{fingerNum}TouchOffset", 1-NormalizedTimeEnd);
            //Debug.Log("ANIM DIDNT FINISH: Finger : " + fingerNum + "  |  Offset: " + (1-NormalizedTimeEnd) + "FingerTouchBool: " + test);
        }
        else
        {
            animator.SetFloat($"Finger{fingerNum}TouchOffset", 0);
            //Debug.Log("Finger : " + fingerNum + "  |  Offset: " + 0 + " | FingerTouchBool: " + test);
        }
        animator.SetBool($"Finger{fingerNum}Touch", false);
        
    }
    
    
    
}