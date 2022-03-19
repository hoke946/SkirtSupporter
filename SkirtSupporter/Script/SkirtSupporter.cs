using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkirtSupporter : MonoBehaviour {

    [Header("Avatar Major Objects")]
    [Tooltip("アバターのAnimator（Humanoidの場合）")]
    public Animator avatarAnimator;
    [Tooltip("アバターのHips（相当の部位）")]
    public Transform hips;
    [Tooltip("アバターのLeftUpperLeg（相当の部位）")]
    public Transform leftUpperLeg;
    [Tooltip("アバターのRightUpperLeg（相当の部位）")]
    public Transform rightUpperLeg;
    [Tooltip("[TwistCancel設定時]アバターのLeftLowerLeg（相当の部位）")]
    public Transform leftLowerLeg;
    [Tooltip("[TwistCancel設定時]アバターのRightLowerLeg（相当の部位）")]
    public Transform rightLowerLeg;

    [Header("Avatar Skirt Objects")]
    [Tooltip("スカートボーンの親")]
    public GameObject skirtsParent;

    [Serializable]
    public struct BoneSet
    {
        [Tooltip("スカートボーンの根本")]
        public GameObject boneObject;
        [Tooltip("正面を0°とした場合の右回りの角度(0～359)")]
        [Range(0, 359)]
        public int angle;
    }

    public List<BoneSet> skirtBones = new List<BoneSet>();

    [Header("Option")]
    [Tooltip("座り姿勢を安定させるスカート吊りを実装")]
    public bool skirtHang;
    [Tooltip("ねじり打ち消し機構を実装")]
    public bool twistCancel;
    [Tooltip("SkirtHangやTwistCancelをConstraint方式にする")]
    public bool useConstraint;
    [Tooltip("スカートのDynamicBoneをクリアして再設定する")]
    public bool dynamicBoneReset;

    [Header("Inner Objects")]
    public GameObject colliderPrefab;
    public DynamicBone dynamicBoneModel;

    private Animator before_animator;
    private GameObject before_parent;

    private void OnValidate()
    {
        MajorBonesUpdate();
        SkirtBonesUpdate();
    }

    private void MajorBonesUpdate()
    {
        if (avatarAnimator == before_animator) { return; }
        else {
            before_animator = avatarAnimator;
        }
        if (avatarAnimator == null) { return; }

        hips = avatarAnimator.GetBoneTransform(HumanBodyBones.Hips);
        rightUpperLeg = avatarAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        leftUpperLeg = avatarAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        rightLowerLeg = avatarAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        leftLowerLeg = avatarAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
    }

    private void SkirtBonesUpdate()
    {
        if (skirtBones.Count > 0)
        {
            OptimizeSkirtBones();
            return;
        }
        if (skirtsParent == before_parent) { return; }
        else
        {
            before_parent = skirtsParent;
        }
        if (skirtsParent == null) { return; }
        if (hips == null || leftUpperLeg == null || rightUpperLeg == null)
        {
            skirtsParent = before_parent;
            throw new Exception("Hips, LeftUpperLeg, RightUpperLeg のすべてがセットされた状態でSkirtsParentをセットしてください");
        }

        AutoSetSkirtBones();
    }

    private void AutoSetSkirtBones()
    {
        Vector3 before_position = transform.position;
        Quaternion before_rotation = transform.rotation;
        foreach (Transform skirtbone in skirtsParent.transform)
        {
            if (avatarAnimator != null)
            {
                //HumanBoneのGameObjectは除外
                bool exclud = false;
                for (int i = 0; i < (int)HumanBodyBones.LastBone; ++i)
                {
                    if (skirtbone == avatarAnimator.GetBoneTransform((HumanBodyBones)i))
                    {
                        exclud = true;
                        break;
                    }
                }
                if (exclud) { continue; }
            }
            if (skirtbone.childCount == 0) { continue; }

            BoneSet boneset = new BoneSet();
            boneset.boneObject = skirtbone.gameObject;
            if (skirtbone.name == "SkirtRoot")
            {
                if (skirtbone.childCount > 0 && skirtbone.GetChild(0).name == "SkirtBranch")
                {
                    foreach (Transform bone in skirtbone.GetChild(0).transform)
                    {
                        skirtBones.Add(SetBoneSet(bone));
                    }
                    boneset.boneObject = skirtbone.GetChild(0).GetChild(0).gameObject;
                }
                else
                {
                    foreach (Transform bone in skirtbone.transform)
                    {
                        skirtBones.Add(SetBoneSet(bone));
                    }
                }
            }
            else
            {
                skirtBones.Add(SetBoneSet(skirtbone));
            }
        }
        transform.position = before_position;
        transform.rotation = before_rotation;
        Debug.Log(skirtsParent.name + "配下のGameObjectからSkirtBonesを自動設定しました。");
    }

    private BoneSet SetBoneSet(Transform bone)
    {
        BoneSet boneset = new BoneSet();
        boneset.boneObject = bone.gameObject;
        Vector3 targetposition = bone.GetChild(0).position;
        //右足から推定
        transform.position = new Vector3(rightUpperLeg.position.x, targetposition.y, rightUpperLeg.position.z);
        transform.LookAt(targetposition);
        float guess = transform.eulerAngles.y;
        if (guess > 180)
        {
            //左足から推定
            transform.position = new Vector3(leftUpperLeg.position.x, targetposition.y, leftUpperLeg.position.z);
            transform.LookAt(targetposition);
            guess = transform.eulerAngles.y;
            if (guess < 180 || guess > 360)
            {
                //お尻から推定
                transform.position = new Vector3(hips.position.x, targetposition.y, hips.position.z);
                transform.LookAt(targetposition);
                guess = transform.eulerAngles.y;
            }
        }
        boneset.angle = (int)guess == 360 ? 0 : (int)guess;
        return boneset;
    }

    private void OptimizeSkirtBones()
    {
        List<BoneSet> newlist = new List<BoneSet>();
        foreach (BoneSet skirt in skirtBones)
        {
            if (skirt.boneObject != null)
            {
                newlist.Add(skirt);
            }
        }
        skirtBones = newlist;
    }

    private void Start()
    {
        if (twistCancel && skirtsParent && !useConstraint)
        {
            //StartCoroutine(ResetSequence());
        }
    }

    /* 逆効果？
    private IEnumerator ResetSequence()
    {
        DynamicBone[] dbs = skirtsParent.GetComponentsInChildren<DynamicBone>();
        foreach (DynamicBone db in dbs)
        {
            db.enabled = false;
        }

        yield return null;

        foreach (DynamicBone db in dbs)
        {
            db.enabled = true;
        }
    }
    */
}