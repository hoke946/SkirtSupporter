using UnityEngine;

public class SittingAnimationSimulator : MonoBehaviour {

    [SerializeField]
    private Animator avatarAnimator;
    [SerializeField]
    private AnimationClip sittingAnimation;
    [SerializeField]
    private RuntimeAnimatorController simulator;
    [SerializeField]
    private GameObject chair;

    void Start()
    {
        if (avatarAnimator == null) { return; }

        AnimatorOverrideController anim_over = new AnimatorOverrideController();
        anim_over.runtimeAnimatorController = simulator;
        avatarAnimator.runtimeAnimatorController = anim_over;
        anim_over["blank"] = sittingAnimation;
        Instantiate(chair, avatarAnimator.transform.position, avatarAnimator.transform.rotation);
    }

}
