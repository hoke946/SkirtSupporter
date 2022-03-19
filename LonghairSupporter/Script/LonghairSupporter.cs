using System;
using System.Collections.Generic;
using UnityEngine;

public class LonghairSupporter : MonoBehaviour {

    public enum HairType
    {
        BackHair = 0,
        SideHair = 1,
        Bangs = 2,
        Others = 3
    }

    [Header("Avatar Major Objects")]
    [Tooltip("アバターのAnimator")]
    public Animator avatarAnimator;
    [Tooltip("アバターのHead（コライダー設定用）")]
    public Transform head;
    [Tooltip("アバターのChest（コライダー設定用）")]
    public Transform chest;

    [Serializable]
    public struct HairsGroup
    {
        [Tooltip("髪ボーンの親")]
        public GameObject hairsParent;

        [Serializable]
        public struct BoneSet
        {
            [Tooltip("髪ボーンの根本")]
            public GameObject boneObject;

            [Tooltip("LongBackHair:長い後ろ髪\nLongSideHair:長い横髪\nOthers：その他（前髪や短髪全般など）")]
            public HairType hairType;
        }
        public List<BoneSet> hairBones;
    }
    [Header("Avatar Hair Objects")]
    public List<HairsGroup> hairsGroup = new List<HairsGroup>();

    [Serializable]
    public struct EffectSetting
    {
        public string name;

        public enum ColliderTarget
        {
            Head,
            Chest,
            None
        }
        [Tooltip("影響を受けるコライダー")]
        public ColliderTarget colliderTarget;

        [Tooltip("首を傾けたとき根本が垂れ下がる")]
        public bool hairHang;
    }
    [Header("Setting")]
    public EffectSetting[] effectSetting = new EffectSetting[Enum.GetValues(typeof(HairType)).Length];

    //public EffectSetting[] effectSetting = new EffectSetting[Enum.GetValues(typeof(HairType)).Length];

    [Header("Option")]
    [Tooltip("髪のDynamicBoneをクリアして再設定する")]
    public bool dynamicBoneReset;

    [Header("Inner Objects")]
    public GameObject headColliderPrefab;
    public GameObject chestColliderPrefab;

    [HairTypeArray()]
    public DynamicBone[] dynamicBoneModels = new DynamicBone[Enum.GetValues(typeof(HairType)).Length];

    private Animator before_animator;
    private GameObject[] before_parent;

    private void OnValidate()
    {
        MajorBonesUpdate();
        HairBonesUpdate();
        ArrayFix();
    }

    private void MajorBonesUpdate()
    {
        if (avatarAnimator == before_animator) { return; }
        else
        {
            before_animator = avatarAnimator;
        }
        if (avatarAnimator == null) { return; }

        head = avatarAnimator.GetBoneTransform(HumanBodyBones.Head);
        chest = avatarAnimator.GetBoneTransform(HumanBodyBones.Chest);
    }

    private void HairBonesUpdate()
    {
        OptimizeHairBones();
        int bp_idx = 0;
        Array.Resize(ref before_parent, hairsGroup.Count);
        foreach (HairsGroup group in hairsGroup)
        {
            if (group.hairBones.Count > 0)
            {
                continue;
            }
            if (group.hairsParent == before_parent[bp_idx]) { continue; }
            else
            {
                before_parent[bp_idx] = group.hairsParent;
            }
            if (group.hairsParent == null) { continue; }

            AutoSetHairBones(group);
            bp_idx++;
        }
    }

    private void AutoSetHairBones(HairsGroup group)
    {
        Vector3 before_position = transform.position;
        Quaternion before_rotation = transform.rotation;
        foreach (Transform hairbone in group.hairsParent.transform)
        {
            HairsGroup.BoneSet boneset = new HairsGroup.BoneSet();
            boneset.boneObject = hairbone.gameObject;
            if (hairbone.name.Contains("_root"))
            {
                if (hairbone.childCount > 0 && hairbone.GetChild(0).name.Contains("_branch1"))
                {
                    if (hairbone.GetChild(0).childCount > 0 && hairbone.GetChild(0).GetChild(0).name.Contains("_branch2"))
                    {
                        boneset.boneObject = hairbone.GetChild(0).GetChild(0).GetChild(0).gameObject;
                    }
                }
            }
            group.hairBones.Add(boneset);
        }
        transform.position = before_position;
        transform.rotation = before_rotation;
        Debug.Log(group.hairsParent.name + "配下のGameObjectからHairBonesを自動設定しました。");
    }

    private void OptimizeHairBones()
    {
        List<HairsGroup> new_group = new List<HairsGroup>();
        foreach (HairsGroup group in hairsGroup)
        {
            List<HairsGroup.BoneSet> new_boneset = new List<HairsGroup.BoneSet>();
            foreach (HairsGroup.BoneSet skirt in group.hairBones)
            {
                if (skirt.boneObject != null)
                {
                    new_boneset.Add(skirt);
                }
            }
            HairsGroup new_group_unit = new HairsGroup();
            new_group_unit.hairsParent = group.hairsParent;
            new_group_unit.hairBones = new_boneset;
            new_group.Add(new_group_unit);
        }
        hairsGroup = new_group;
    }

    private void ArrayFix()
    {
        // サイズを固定する
        int size = Enum.GetNames(typeof(HairType)).Length;

        if (effectSetting.Length != size)
        {
            Array.Resize(ref effectSetting, size);
        }

        if (dynamicBoneModels.Length != size)
        {
            Array.Resize(ref dynamicBoneModels, size);
        }

        // グループ名を固定する
        int i = 0;
        foreach (string name in Enum.GetNames(typeof(HairType)))
        {
            effectSetting[i].name = name;
            i++;
        }
    }
}