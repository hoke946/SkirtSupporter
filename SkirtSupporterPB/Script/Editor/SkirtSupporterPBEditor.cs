﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Dynamics.PhysBone.Components;
using UnityEngine.Animations;

[CustomEditor(typeof(SkirtSupporterPB))]//拡張するクラスを指定
public class SkirtSupporterPBEditor : Editor
{
    private SkirtSupporterPB skirtSupporter;

    /// <summary>
    /// InspectorのGUIを更新
    /// </summary>
    public override void OnInspectorGUI()
    {
        //元のInspector部分を表示
        base.OnInspectorGUI();

        //targetを変換して対象を取得
        skirtSupporter = target as SkirtSupporterPB;

        if (GUILayout.Button("PhysBone＆Collider生成"))
        {
            PhysBoneCreate();
        }
    }

    public void PhysBoneCreate()
    {
        ParameterCheck();

        Vector3 back_position = skirtSupporter.hips.root.position;
        Quaternion back_rotation = skirtSupporter.hips.root.rotation;
        skirtSupporter.hips.root.position = Vector3.zero;
        skirtSupporter.hips.root.rotation = Quaternion.identity;

        ClearObjects();

        PhysBonePrepare();

        SetPhysBoneCollider(true);
        SetPhysBoneCollider(false);

        if (skirtSupporter.skirtHang)
        {
            SetSkirtHang();
        }
        else
        {
            ClearSkirtHangObject();
        }

        skirtSupporter.hips.root.position = back_position;
        skirtSupporter.hips.root.rotation = back_rotation;

        AfterCheck();

        Debug.Log("PhysBone＆Collider生成完了！");
    }

    private void ParameterCheck()
    {
        if (skirtSupporter.hips == null) { Alert("Hipsをセットしてください"); }
        if (skirtSupporter.leftUpperLeg == null) { Alert("LeftUpperLegをセットしてください"); }
        if (skirtSupporter.rightUpperLeg == null) { Alert("RightUpperLegをセットしてください"); }
        if (skirtSupporter.twistCancel && skirtSupporter.leftLowerLeg == null) { Alert("TwistCancelがオンの場合、LeftLowerLegをセットしてください"); }
        if (skirtSupporter.twistCancel && skirtSupporter.rightLowerLeg == null) { Alert("TwistCancelがオンの場合、RightLowerLegをセットしてください"); }
        if (skirtSupporter.skirtsParent == null) { Alert("SkirtsParentをセットしてください"); }
        if (skirtSupporter.skirtBones.Count == 0) { Alert("SkirtBonesを1つ以上セットしてください"); }
        int i = 0;
        List<int> angle_unit = new List<int>();
        foreach (SkirtSupporterPB.BoneSet skirt in skirtSupporter.skirtBones)
        {
            if (skirt.boneObject == null) { Alert("Element " + i + " のBoneObjectをセットしてください"); }
            if (angle_unit.Contains(skirt.angle)) { Alert("Angleに重複があります"); }
            angle_unit.Add(skirt.angle);

            if (!skirtSupporter.physBoneReset)
            {
                VRCPhysBone pb = skirt.boneObject.GetComponent<VRCPhysBone>();
                if (pb == null) { Alert("PhysBoneResetがオフの場合、すべてのSkirtBonesにPhysBoneをアタッチしてください"); }
            }
            i++;
        }
        if (skirtSupporter.skirtHang && PrefabUtility.GetCorrespondingObjectFromSource(skirtSupporter.avatarAnimator.gameObject))
        {
            if (EditorUtility.DisplayDialog("Confirm", "Prefabを解除しても問題ないですか？", "OK", "Cancel"))
            {
                PrefabUtility.UnpackPrefabInstance(skirtSupporter.avatarAnimator.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
            }
            else
            {
                throw new Exception("中断しました");
            }
        }
    }

    private void ClearObjects()
    {
        List<Transform> targets = new List<Transform>();
        targets.Add(skirtSupporter.hips.Find("_SubLegR"));
        targets.Add(skirtSupporter.hips.Find("_SubLegL"));
        targets.Add(skirtSupporter.rightUpperLeg.Find("PBC_R"));
        targets.Add(skirtSupporter.leftUpperLeg.Find("PBC_L"));
        targets.Add(skirtSupporter.hips.parent.Find("_HangFrontTarget"));
        targets.Add(skirtSupporter.hips.parent.Find("_HangAimParent"));

        // 旧SkirtSupporterの要素やDynamicBoneを削除
        targets.Add(skirtSupporter.hips.Find("SubLegR"));
        targets.Add(skirtSupporter.hips.Find("SubLegL"));
        targets.Add(skirtSupporter.rightUpperLeg.Find("DBC_R"));
        targets.Add(skirtSupporter.leftUpperLeg.Find("DBC_L"));
        targets.Add(skirtSupporter.rightLowerLeg.Find("SubGuideR"));
        targets.Add(skirtSupporter.leftLowerLeg.Find("SubGuideL"));
        List<MonoBehaviour> components = new List<MonoBehaviour>();
        components.AddRange(skirtSupporter.hips.GetComponents<MonoBehaviour>());
        foreach (var bones in skirtSupporter.skirtBones)
        {
            components.AddRange(bones.boneObject.GetComponents<MonoBehaviour>());
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
        foreach (SkirtSupporterPB.BoneSet skirt in skirtSupporter.skirtBones)
        {
            var pb = skirt.boneObject.GetComponent<VRCPhysBone>();
            if (skirtSupporter.physBoneReset)
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(skirtSupporter.physBoneModel);
                if (pb != null)
                {
                    UnityEditorInternal.ComponentUtility.PasteComponentValues(pb);
                }
                else
                {
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(skirt.boneObject);
                    pb = skirt.boneObject.GetComponent<VRCPhysBone>();
                }
            }
            else
            {
                pb.colliders.Clear();
            }
        }
    }

    private void SetPhysBoneCollider(bool right)
    {
        string name = right ? "R" : "L";
        Transform upperLeg = right ? skirtSupporter.rightUpperLeg : skirtSupporter.leftUpperLeg;

        GameObject PBC_root = new GameObject("PBC_" + name);

        if (skirtSupporter.twistCancel)
        {
            Transform lowerLeg = right ? skirtSupporter.rightLowerLeg : skirtSupporter.leftLowerLeg;

            GameObject subLeg = new GameObject("_SubLeg" + name);
            subLeg.transform.SetParent(skirtSupporter.hips, false);
            subLeg.transform.SetPositionAndRotation(upperLeg.transform.position, upperLeg.transform.rotation);

            GameObject subLeg1 = new GameObject("_SubLeg" + name + ".1");
            subLeg1.transform.SetParent(subLeg.transform, false);

            AimConstraint constraint = subLeg1.gameObject.AddComponent<AimConstraint>();
            ConstraintSource source = new ConstraintSource();
            source.sourceTransform = lowerLeg.transform;
            source.weight = 1;
            constraint.AddSource(source);
            constraint.locked = true;
            constraint.worldUpType = AimConstraint.WorldUpType.ObjectRotationUp;
            constraint.worldUpVector = new Vector3(1, 0, 0);
            constraint.worldUpObject = subLeg.transform;
            constraint.aimVector = new Vector3(0, 1, 0);
            constraint.upVector = new Vector3(1, 0, 0);
            constraint.constraintActive = true;

            PBC_root.transform.SetParent(subLeg1.transform, false);
        }
        else
        {
            PBC_root.transform.SetParent(upperLeg);
        }

        PBC_root.transform.rotation = Quaternion.identity;
        PBC_root.transform.localScale = new Vector3(1 ,1 ,1);
        PBC_root.transform.localScale = new Vector3(1 / skirtSupporter.hips.lossyScale.x,
                                                    1 / skirtSupporter.hips.lossyScale.y,
                                                    1 / skirtSupporter.hips.lossyScale.z);

        foreach (SkirtSupporterPB.BoneSet skirt in skirtSupporter.skirtBones)
        {
            bool sw = false;
            if (right)
            {
                if (skirt.angle >= 0 && skirt.angle <= 180)
                {
                    sw = true;
                }
            }
            else
            {
                if ((skirt.angle >= 180 && skirt.angle < 360) || skirt.angle == 0)
                {
                    sw = true;
                }
            }

            if (sw)
            {
                GameObject PBC_branch = new GameObject("PBC_" + name + "_" + skirt.boneObject.name);
                PBC_branch.transform.SetParent(PBC_root.transform, false);
                //PBC_branch.transform.position = skirt.boneObject.transform.GetChild(0).position;
                PBC_branch.transform.position = new Vector3(0, skirt.boneObject.transform.GetChild(0).position.y, 0);
                PBC_branch.transform.Rotate(0, skirt.angle, 0);

                GameObject PBC_prefab = (GameObject)PrefabUtility.InstantiatePrefab(skirtSupporter.colliderPrefab);
                PBC_prefab.transform.SetParent(PBC_branch.transform, false);
                skirt.boneObject.GetComponent<VRCPhysBone>().colliders.Add(PBC_prefab.transform.GetComponentInChildren<VRCPhysBoneCollider>());
            }
        }
    }

    private void SetSkirtHang()
    {
        GameObject front = new GameObject("_HangFrontTarget");
        front.transform.SetParent(skirtSupporter.hips.transform, false);
        front.transform.localPosition = Vector3.forward;
        Transform exist = skirtSupporter.hips.Find("SkirtRoot");

        GameObject aim_parent = new GameObject("_HangAimParent");
        aim_parent.transform.SetParent(skirtSupporter.hips.parent, false);
        aim_parent.transform.SetPositionAndRotation(front.transform.position, front.transform.rotation);

        GameObject aim = new GameObject("_HangAimTarget");
        aim.transform.SetParent(aim_parent.transform, false);

        var posconstraint = aim.AddComponent<PositionConstraint>();
        ConstraintSource posconstraint_source = new ConstraintSource();
        posconstraint_source.sourceTransform = front.transform;
        posconstraint_source.weight = 1;
        posconstraint.AddSource(posconstraint_source);
        posconstraint.locked = true;
        posconstraint.translationAxis = Axis.X | Axis.Z;
        posconstraint.constraintActive = true;

        GameObject skirt_root = new GameObject("SkirtRoot");
        skirt_root.transform.SetParent(skirtSupporter.hips, false);
        skirt_root.transform.position = skirtSupporter.skirtsParent.transform.position;

        var aimconstraint = skirt_root.AddComponent<AimConstraint>();
        ConstraintSource aimconstraint_source = new ConstraintSource();
        aimconstraint_source.sourceTransform = aim.transform;
        aimconstraint_source.weight = 1;
        aimconstraint.AddSource(aimconstraint_source);
        aimconstraint.locked = true;
        aimconstraint.constraintActive = true;

        foreach (SkirtSupporterPB.BoneSet skirt in skirtSupporter.skirtBones)
        {
            skirt.boneObject.transform.SetParent(skirt_root.transform, true);
        }

        if (exist != null)
        {
            DestroyImmediate(exist.gameObject);
        }
    }

    private void AfterCheck()
    {
        foreach (var pb in skirtSupporter.hips.root.GetComponentsInChildren<VRCPhysBone>())
        {
            if (!pb.gameObject.activeInHierarchy) { continue; }
            if (!pb.enabled) { continue; }

            List<Transform> skirtObjects = new List<Transform>();
            foreach (SkirtSupporterPB.BoneSet skirt in skirtSupporter.skirtBones)
            {
                skirtObjects.Add(skirt.boneObject.transform);
            }
            if (skirtObjects.Contains(pb.transform)) { continue; }

            foreach (Transform child in GetAffectedObjects(pb.rootTransform == null ? pb.transform : pb.rootTransform, pb.ignoreTransforms))
            {
                if (skirtObjects.Contains(child))
                {
                    Alert("設定箇所に他のPhysBoneが影響しています\n場所 : " + pb.gameObject.name, false);
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

    private void ClearSkirtHangObject()
    {
        foreach (SkirtSupporterPB.BoneSet skirt in skirtSupporter.skirtBones)
        {
            if (skirt.boneObject.transform.parent != skirtSupporter.skirtsParent)
            {
                var parent = skirt.boneObject.transform.parent;
                skirt.boneObject.transform.SetParent(skirtSupporter.skirtsParent.transform, true);
                if (parent != null && parent.name == "SkirtRoot")
                {
                    DestroyImmediate(parent.gameObject);
                }
                else if (parent != null && parent.name == "SkirtBranch")
                {
                    if (parent.parent != null && parent.parent.name == "SkirtRoot")
                    {
                        DestroyImmediate(parent.parent.gameObject);
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