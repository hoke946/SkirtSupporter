using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Dynamics.PhysBone.Components;

[CustomEditor(typeof(LonghairSupporterPB))]
public class LonghairSupporterPBEditor : Editor
{
    private LonghairSupporterPB longhairSupporter;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        longhairSupporter = target as LonghairSupporterPB;

        if (GUILayout.Button("PhysBone＆Collider生成"))
        {
            PhysBoneCreate();
        }
    }

    public void PhysBoneCreate()
    {
        ParameterCheck();

        ClearObjects();

        PhysBonePrepare();

        SetPhysBoneCollider();

        //SetHairHang();
        ClearHairHangObject();

        Debug.Log("PhysBone＆Collider生成完了！");
    }

    private void ParameterCheck()
    {
        foreach (LonghairSupporterPB.HairsGroup group in longhairSupporter.hairsGroup)
        {
            if (group.hairsParent == null) { throw new Exception("HairsParentをセットしてください"); }
            if (group.hairBones.Count == 0) { throw new Exception("HairBonesを1つ以上セットしてください"); }
            int i = 0;
            foreach (LonghairSupporterPB.HairsGroup.BoneSet hair in group.hairBones)
            {
                if (longhairSupporter.effectSetting[(int)hair.hairType].colliderTarget == LonghairSupporterPB.EffectSetting.ColliderTarget.Head)
                {
                    if (longhairSupporter.head == null) { Alert("Headをセットしてください"); }
                }
                if (longhairSupporter.effectSetting[(int)hair.hairType].colliderTarget == LonghairSupporterPB.EffectSetting.ColliderTarget.Chest)
                {
                    if (longhairSupporter.chest == null) { Alert("Chestをセットしてください"); }
                }

                if (hair.boneObject == null) { Alert(group.hairsParent.name + " : Element " + i + " のBoneObjectをセットしてください"); }

                if (!longhairSupporter.physBoneReset)
                {
                    VRCPhysBone pb = hair.boneObject.GetComponent<VRCPhysBone>();
                    if (pb == null) { Alert("PhysBoneResetがオフの場合、すべてのHairBonesにPhysBoneをアタッチしてください"); }
                }
                i++;
            }
        }
    }

    private void ClearObjects()
    {
        List<Transform> targets = new List<Transform>();
        targets.Add(longhairSupporter.head.Find(longhairSupporter.headColliderPrefab.name));
        targets.Add(longhairSupporter.chest.Find(longhairSupporter.chestColliderPrefab.name));

        // DynamicBoneを削除
        List<MonoBehaviour> components = new List<MonoBehaviour>();
        foreach (LonghairSupporterPB.HairsGroup group in longhairSupporter.hairsGroup)
        {
            foreach (LonghairSupporterPB.HairsGroup.BoneSet hair in group.hairBones)
            {
                components.AddRange(hair.boneObject.GetComponents<MonoBehaviour>());
            }
        }
        foreach (var component in components)
        {
            if (component != null && component.GetType().Name == "DynamicBone")
            {
                DestroyImmediate(component);
            }
        }

        foreach (Transform target in targets)
        {
            if (target != null)
            {
                DestroyImmediate(target.gameObject);
            }
        }
    }

    private void PhysBonePrepare()
    {
        foreach (LonghairSupporterPB.HairsGroup group in longhairSupporter.hairsGroup)
        {
            foreach (LonghairSupporterPB.HairsGroup.BoneSet hair in group.hairBones)
            {
                VRCPhysBone pb = hair.boneObject.GetComponent<VRCPhysBone>();
                if (longhairSupporter.physBoneReset)
                {
                    UnityEditorInternal.ComponentUtility.CopyComponent(longhairSupporter.physBoneModels[(int)hair.hairType]);

                    if (pb != null)
                    {
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(pb);
                    }
                    else
                    {
                        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(hair.boneObject);
                        pb = hair.boneObject.GetComponent<VRCPhysBone>();
                    }
                }
                else
                {
                    pb.colliders.Clear();
                }
            }
        }
    }

    private void SetPhysBoneCollider()
    {
        bool head_set = false;
        bool chest_set = false;
        VRCPhysBoneCollider head_collider = new VRCPhysBoneCollider();
        VRCPhysBoneCollider chest_collider = new VRCPhysBoneCollider();

        foreach (LonghairSupporterPB.HairsGroup group in longhairSupporter.hairsGroup)
        {
            foreach (LonghairSupporterPB.HairsGroup.BoneSet hair in group.hairBones)
            {
                if (longhairSupporter.effectSetting[(int)hair.hairType].colliderTarget == LonghairSupporterPB.EffectSetting.ColliderTarget.Head)
                {
                    if (!head_set)
                    {
                        head_set = true;
                        // 頭部コライダー生成
                        GameObject PBC_head = (GameObject)PrefabUtility.InstantiatePrefab(longhairSupporter.headColliderPrefab);
                        PBC_head.transform.SetParent(longhairSupporter.head, false);
                        PBC_head.transform.localPosition = Vector3.zero;
                        PBC_head.transform.rotation = Quaternion.identity;
                        float scale = 1 / longhairSupporter.head.lossyScale.y;
                        PBC_head.transform.localScale = new Vector3(scale, scale, scale);
                        head_collider = PBC_head.transform.GetChild(0).GetComponent<VRCPhysBoneCollider>();
                    }
                    hair.boneObject.GetComponent<VRCPhysBone>().colliders.Add(head_collider);
                }

                if (longhairSupporter.effectSetting[(int)hair.hairType].colliderTarget == LonghairSupporterPB.EffectSetting.ColliderTarget.Chest)
                {
                    if (!chest_set)
                    {
                        chest_set = true;
                        // 胸部コライダー生成
                        GameObject PBC_chest = (GameObject)PrefabUtility.InstantiatePrefab(longhairSupporter.chestColliderPrefab);
                        PBC_chest.transform.SetParent(longhairSupporter.chest, false);
                        PBC_chest.transform.localPosition = Vector3.zero;
                        PBC_chest.transform.rotation = Quaternion.identity;
                        float scale = 1 / longhairSupporter.chest.lossyScale.y;
                        PBC_chest.transform.localScale = new Vector3(scale, scale, scale);
                        chest_collider = PBC_chest.transform.GetChild(0).GetComponent<VRCPhysBoneCollider>();
                    }
                    hair.boneObject.GetComponent<VRCPhysBone>().colliders.Add(chest_collider);
                }
            }
        }
    }

    private void ClearHairHangObject()
    {
        foreach (LonghairSupporterPB.HairsGroup group in longhairSupporter.hairsGroup)
        {
            foreach (LonghairSupporterPB.HairsGroup.BoneSet hair in group.hairBones)
            {
                if (hair.boneObject.transform.parent != group.hairsParent.transform)
                {
                    var parent = hair.boneObject.transform.parent;
                    hair.boneObject.transform.SetParent(group.hairsParent.transform, true);
                    if (parent != null && parent.name.Contains("_branch2"))
                    {
                        if (parent.parent != null && parent.parent.name.Contains("_branch1"))
                        {
                            if (parent.parent.parent != null && parent.parent.parent.name.Contains("_root"))
                            {
                                DestroyImmediate(parent.parent.parent.gameObject);
                            }
                        }
                    }
                }
            }
        }
    }

    private void Alert(string message, bool error = true)
    {
        if (error)
        {
            EditorUtility.DisplayDialog("Error", message, "OK");
            throw new Exception(message);
        }
        else
        {
            EditorUtility.DisplayDialog("Warning", message, "OK");
            Debug.LogWarning(message);
        }
    }
}