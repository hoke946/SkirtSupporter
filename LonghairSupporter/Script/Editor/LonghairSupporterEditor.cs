using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LonghairSupporter))]//拡張するクラスを指定
public class LonghairSupporterEditor : Editor
{
    private LonghairSupporter longhairSupporter;

    /// <summary>
    /// InspectorのGUIを更新
    /// </summary>
    public override void OnInspectorGUI()
    {
        //元のInspector部分を表示
        base.OnInspectorGUI();

        //targetを変換して対象を取得
        longhairSupporter = target as LonghairSupporter;

        if (GUILayout.Button("DynamicBone＆Collider生成"))
        {
            DynamicBoneCreate();
        }
    }

    public void DynamicBoneCreate()
    {
        ParameterCheck();

        ClearObjects();

        DynamicBonePrepare();

        SetDynamicBoneCollider();

        SetHairHang();

        AfterCheck();

        Debug.Log("DynamicBone＆Collider生成完了！");
    }

    private void ParameterCheck()
    {
        foreach (LonghairSupporter.HairsGroup group in longhairSupporter.hairsGroup)
        {
            if (group.hairsParent == null) { Alert("HairsParentをセットしてください"); }
            if (group.hairBones.Count == 0) { Alert("HairBonesを1つ以上セットしてください"); }
            int i = 0;
            foreach (LonghairSupporter.HairsGroup.BoneSet hair in group.hairBones)
            {
                if (longhairSupporter.effectSetting[(int)hair.hairType].colliderTarget == LonghairSupporter.EffectSetting.ColliderTarget.Head)
                {
                    if (longhairSupporter.head == null) { Alert("Headをセットしてください"); }
                }
                if (longhairSupporter.effectSetting[(int)hair.hairType].colliderTarget == LonghairSupporter.EffectSetting.ColliderTarget.Chest)
                {
                    if (longhairSupporter.chest == null) { Alert("Chestをセットしてください"); }
                }

                if (hair.boneObject == null) { Alert(group.hairsParent.name + " : Element " + i + " のBoneObjectをセットしてください"); }

                if (!longhairSupporter.dynamicBoneReset)
                {
                    DynamicBone db = hair.boneObject.GetComponent<DynamicBone>();
                    if (db == null) { Alert("DynamicBoneResetがオフの場合、すべてのHairBonesにDynamicBoneをアタッチしてください"); }
                }
                i++;
            }
        }
        /*
        if (longhairSupporter.head.Find(longhairSupporter.headColliderPrefab.name) != null ||
            longhairSupporter.chest.Find(longhairSupporter.chestColliderPrefab.name) != null)
        {
            Alert("既に生成されたDynamicBoneColliderがあります。");
        }
        */
        foreach (LonghairSupporter.EffectSetting setting in longhairSupporter.effectSetting)
        {
            if (setting.hairHang && PrefabUtility.GetCorrespondingObjectFromSource(longhairSupporter.avatarAnimator.gameObject))
            {
                if (EditorUtility.DisplayDialog("Confirm", "Prefabを解除しても問題ないですか？", "OK", "Cancel"))
                {
                    PrefabUtility.UnpackPrefabInstance(longhairSupporter.avatarAnimator.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
                }
                else
                {
                    throw new Exception("中断しました");
                }
            }
        }
    }

    private void ClearObjects()
    {
        List<Transform> targets = new List<Transform>();
        targets.Add(longhairSupporter.head.Find(longhairSupporter.headColliderPrefab.name));
        targets.Add(longhairSupporter.chest.Find(longhairSupporter.chestColliderPrefab.name));
        foreach (Transform target in targets)
        {
            if (target != null)
            {
                DestroyImmediate(target.gameObject);
            }
        }
    }

    private void DynamicBonePrepare()
    {
        foreach (LonghairSupporter.HairsGroup group in longhairSupporter.hairsGroup)
        {
            foreach (LonghairSupporter.HairsGroup.BoneSet hair in group.hairBones)
            {
                DynamicBone db = hair.boneObject.GetComponent<DynamicBone>();
                if (longhairSupporter.dynamicBoneReset)
                {
                    UnityEditorInternal.ComponentUtility.CopyComponent(longhairSupporter.dynamicBoneModels[(int)hair.hairType]);

                    if (db != null)
                    {
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(db);
                    }
                    else
                    {
                        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(hair.boneObject);
                        db = hair.boneObject.GetComponent<DynamicBone>();
                    }

                    db.m_Root = db.transform;
                }
                else
                {
                    db.m_Colliders.Clear();
                }
            }
        }
    }

    private void SetDynamicBoneCollider()
    {
        bool head_set = false;
        bool chest_set = false;
        DynamicBoneCollider head_collider = new DynamicBoneCollider();
        DynamicBoneCollider chest_collider = new DynamicBoneCollider();

        foreach (LonghairSupporter.HairsGroup group in longhairSupporter.hairsGroup)
        {
            foreach (LonghairSupporter.HairsGroup.BoneSet hair in group.hairBones)
            {
                if (longhairSupporter.effectSetting[(int)hair.hairType].colliderTarget == LonghairSupporter.EffectSetting.ColliderTarget.Head)
                {
                    if (!head_set)
                    {
                        head_set = true;
                        // 頭部コライダー生成
                        GameObject DBC_head = (GameObject)PrefabUtility.InstantiatePrefab(longhairSupporter.headColliderPrefab);
                        DBC_head.transform.parent = longhairSupporter.head;
                        DBC_head.transform.localPosition = Vector3.zero;
                        DBC_head.transform.rotation = Quaternion.identity;
                        float scale = 1 / longhairSupporter.head.lossyScale.y;
                        DBC_head.transform.localScale = new Vector3(scale, scale, scale);
                        head_collider = DBC_head.transform.GetChild(0).GetComponent<DynamicBoneCollider>();
                    }
                    hair.boneObject.GetComponent<DynamicBone>().m_Colliders.Add(head_collider);
                }

                if (longhairSupporter.effectSetting[(int)hair.hairType].colliderTarget == LonghairSupporter.EffectSetting.ColliderTarget.Chest)
                {
                    if (!chest_set)
                    {
                        chest_set = true;
                        // 胸部コライダー生成
                        GameObject DBC_chest = (GameObject)PrefabUtility.InstantiatePrefab(longhairSupporter.chestColliderPrefab);
                        DBC_chest.transform.parent = longhairSupporter.chest;
                        DBC_chest.transform.localPosition = Vector3.zero;
                        DBC_chest.transform.rotation = Quaternion.identity;
                        float scale = 1 / longhairSupporter.chest.lossyScale.y;
                        DBC_chest.transform.localScale = new Vector3(scale, scale, scale);
                        chest_collider = DBC_chest.transform.GetChild(0).GetComponent<DynamicBoneCollider>();
                    }
                    hair.boneObject.GetComponent<DynamicBone>().m_Colliders.Add(chest_collider);
                }
            }
        }
    }

    private void SetHairHang()
    {
        foreach (LonghairSupporter.HairsGroup group in longhairSupporter.hairsGroup)
        {
            foreach (LonghairSupporter.HairsGroup.BoneSet hair in group.hairBones)
            {
                if (longhairSupporter.effectSetting[(int)hair.hairType].hairHang)
                {
                    if (hair.hairType != LonghairSupporter.HairType.Others)
                    {
                        Transform exist = group.hairsParent.transform.Find(hair.boneObject.name + "_root");

                        GameObject hair_root = new GameObject(hair.boneObject.name + "_root");
                        hair_root.transform.parent = group.hairsParent.transform;
                        hair_root.transform.position = hair.boneObject.transform.position + new Vector3(0, 0.001f, 0);
                        hair_root.transform.rotation = Quaternion.identity;
                        hair_root.transform.localScale = new Vector3(1, 1, 1);

                        DynamicBone hang_db = hair_root.AddComponent<DynamicBone>();
                        hang_db.m_Root = hair_root.transform;
                        hang_db.m_Damping = 0;
                        hang_db.m_Elasticity = 0.5f;
                        hang_db.m_Stiffness = 0.01f;
                        hang_db.m_Gravity = new Vector3(0, -1, 0);
                        hang_db.m_Force = new Vector3(0, -1, 0);
                        hang_db.m_Exclusions = new List<Transform>();

                        GameObject hair_branch1 = new GameObject(hair.boneObject.name + "_branch1");
                        hair_branch1.transform.parent = hair_root.transform;
                        hair_branch1.transform.position = hair.boneObject.transform.position;
                        hair_branch1.transform.rotation = Quaternion.identity;
                        hair_branch1.transform.localScale = new Vector3(1, 1, 1);

                        GameObject hair_branch2 = new GameObject(hair.boneObject.name + "_branch2");
                        hair_branch2.transform.parent = hair_branch1.transform;
                        hair_branch2.transform.position = group.hairsParent.transform.position;
                        hair_branch2.transform.rotation = group.hairsParent.transform.rotation;
                        hair_branch2.transform.localScale = new Vector3(1, 1, 1);

                        hair.boneObject.transform.parent = hair_branch2.transform;
                        hang_db.m_Exclusions.Add(hair_branch2.transform);

                        if (exist != null)
                        {
                            DestroyImmediate(exist.gameObject);
                        }
                    }
                }
                else
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
    }

    private void AfterCheck()
    {
        foreach (DynamicBone db in longhairSupporter.head.root.GetComponentsInChildren<DynamicBone>())
        {
            if (!db.gameObject.activeInHierarchy) { continue; }
            if (!db.enabled) { continue; }
            if (!db.m_Root) { continue; }

            List<Transform> hairObjects = new List<Transform>();
            foreach (LonghairSupporter.HairsGroup group in longhairSupporter.hairsGroup)
            {
                foreach (LonghairSupporter.HairsGroup.BoneSet hair in group.hairBones)
                {
                    hairObjects.Add(hair.boneObject.transform);
                }
            }
            if (hairObjects.Contains(db.transform)) { continue; }

            foreach (Transform child in GetAffectedObjects(db.m_Root, db.m_Exclusions))
            {
                if (hairObjects.Contains(child))
                {
                    Alert("設定箇所に他のDynamicBoneが影響しています\n場所 : " + db.gameObject.name, false);
                    break;
                }
            }
        }
    }

    private List<Transform> GetAffectedObjects(Transform root, List<Transform> exclusions)
    {
        List<Transform> affectedObjects = new List<Transform>();
        affectedObjects.Add(root);
        foreach (Transform child in root)
        {
            if (exclusions.Contains(child)) { continue; }
            List<Transform> child_root = GetAffectedObjects(child, exclusions);
            foreach (Transform grandchild in child_root)
            {
                affectedObjects.Add(grandchild);
            }
        }
        return affectedObjects;
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