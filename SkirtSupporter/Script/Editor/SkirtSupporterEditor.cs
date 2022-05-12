using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;

[CustomEditor(typeof(SkirtSupporter))]//拡張するクラスを指定
public class SkirtSupporterEditor : Editor
{
    private SkirtSupporter skirtSupporter;
    private DynamicBone skirtDB;

    /// <summary>
    /// InspectorのGUIを更新
    /// </summary>
    public override void OnInspectorGUI()
    {
        //元のInspector部分を表示
        base.OnInspectorGUI();

        //targetを変換して対象を取得
        skirtSupporter = target as SkirtSupporter;

        if (GUILayout.Button("DynamicBone＆Collider生成"))
        {
            DynamicBoneCreate();
        }
    }

    public void DynamicBoneCreate()
    {
        ParameterCheck();

        Vector3 back_position = skirtSupporter.hips.root.position;
        Quaternion back_rotation = skirtSupporter.hips.root.rotation;
        skirtSupporter.hips.root.position = Vector3.zero;
        skirtSupporter.hips.root.rotation = Quaternion.identity;

        ClearObjects();

        DynamicBonePrepare();

        SetDynamicBoneCollider(true);
        SetDynamicBoneCollider(false);

        if (skirtSupporter.skirtHang)
        {
            SetSkirtHung();
        }
        else
        {
            ClearSkirtHangObject();
        }

        skirtSupporter.hips.root.position = back_position;
        skirtSupporter.hips.root.rotation = back_rotation;

        AfterCheck();

        Debug.Log("DynamicBone＆Collider生成完了！");
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
        foreach (SkirtSupporter.BoneSet skirt in skirtSupporter.skirtBones)
        {
            if (skirt.boneObject == null) { Alert("Element " + i + " のBoneObjectをセットしてください"); }
            if (angle_unit.Contains(skirt.angle)) { Alert("Angleに重複があります"); }
            angle_unit.Add(skirt.angle);

            if (!skirtSupporter.dynamicBoneReset)
            {
                DynamicBone db = skirt.boneObject.GetComponent<DynamicBone>();
                if (db == null) { Alert("DynamicBoneResetがオフの場合、すべてのSkirtBonesにDynamicBoneをアタッチしてください"); }
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
        targets.Add(skirtSupporter.hips.Find("SubLegR"));
        targets.Add(skirtSupporter.hips.Find("SubLegL"));
        targets.Add(skirtSupporter.hips.Find("_SubLegR"));
        targets.Add(skirtSupporter.hips.Find("_SubLegL"));
        targets.Add(skirtSupporter.hips.Find("_HangFrontParent"));
        targets.Add(skirtSupporter.hips.Find("_HangFrontTarget"));
        targets.Add(skirtSupporter.hips.parent.Find("_HangAimParent"));
        foreach (DynamicBone db in skirtSupporter.hips.GetComponents<DynamicBone>())
        {
            foreach (Transform target in targets)
            {
                if (target != null && db.m_Root == target)
                {
                    DestroyImmediate(db);
                }
            }
        }
        targets.Add(skirtSupporter.rightUpperLeg.Find("DBC_R"));
        targets.Add(skirtSupporter.leftUpperLeg.Find("DBC_L"));
        targets.Add(skirtSupporter.rightLowerLeg.Find("SubGuideR"));
        targets.Add(skirtSupporter.leftLowerLeg.Find("SubGuideL"));
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
        foreach (SkirtSupporter.BoneSet skirt in skirtSupporter.skirtBones)
        {
            DynamicBone db = skirt.boneObject.GetComponent<DynamicBone>();
            if (skirtSupporter.dynamicBoneReset)
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(skirtSupporter.dynamicBoneModel);
                if (db != null)
                {
                    UnityEditorInternal.ComponentUtility.PasteComponentValues(db);
                }
                else
                {
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(skirt.boneObject);
                    db = skirt.boneObject.GetComponent<DynamicBone>();
                }

                db.m_Root = db.transform;
            }
            else
            {
                db.m_Colliders.Clear();
            }
        }
    }

    private void SetDynamicBoneCollider(bool right)
    {
        string name = right ? "R" : "L";
        Transform upperLeg = right ? skirtSupporter.rightUpperLeg : skirtSupporter.leftUpperLeg;

        GameObject DBC_root = new GameObject("DBC_" + name);

        if (skirtSupporter.twistCancel)
        {
            Transform lowerLeg = right ? skirtSupporter.rightLowerLeg : skirtSupporter.leftLowerLeg;

            if (skirtSupporter.useConstraint)
            {
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

                DBC_root.transform.SetParent(subLeg1.transform, false);
            }
            else
            {
                GameObject subLeg = new GameObject("SubLeg" + name);
                subLeg.transform.parent = skirtSupporter.hips;
                subLeg.transform.position = upperLeg.transform.position;
                subLeg.transform.rotation = upperLeg.transform.rotation;
                subLeg.transform.localScale = new Vector3(1, 1, 1);

                GameObject subLeg1 = new GameObject("SubLeg" + name + ".1");
                subLeg1.transform.parent = subLeg.transform;
                subLeg1.transform.position = Vector3.Lerp(upperLeg.transform.position, lowerLeg.transform.position, 0.25f);
                subLeg1.transform.localRotation = Quaternion.identity;
                subLeg1.transform.localScale = new Vector3(1, 1, 1);

                DBC_root.transform.parent = subLeg1.transform;
                DBC_root.transform.position = Vector3.Lerp(upperLeg.transform.position, lowerLeg.transform.position, 0.5f);

                GameObject subGuide = new GameObject("SubGuide" + name);
                subGuide.transform.parent = lowerLeg;
                subGuide.transform.localPosition = Vector3.zero;
                subGuide.transform.localRotation = Quaternion.identity;
                subGuide.transform.localScale = new Vector3(1 / lowerLeg.lossyScale.x,
                                                            1 / lowerLeg.lossyScale.y,
                                                            1 / lowerLeg.lossyScale.z);

                DynamicBoneCollider guide_dbc = subGuide.AddComponent<DynamicBoneCollider>();
                guide_dbc.m_Bound = DynamicBoneCollider.Bound.Inside;
                guide_dbc.m_Radius = 0.001f;

                DynamicBone db = skirtSupporter.hips.gameObject.AddComponent<DynamicBone>();
                db.m_Root = subLeg.transform;
                db.m_Damping = 0.01f;
                db.m_Elasticity = 0.01f;
                db.m_Stiffness = 0.01f;
                db.m_Colliders = new List<DynamicBoneColliderBase>();
                db.m_Colliders.Add(subGuide.GetComponent<DynamicBoneCollider>());
                db.m_Exclusions = new List<Transform>();
                db.m_Exclusions.Add(DBC_root.transform);
            }
        }
        else
        {
            DBC_root.transform.parent = upperLeg;
            DBC_root.transform.localPosition = Vector3.zero;
        }

        DBC_root.transform.rotation = Quaternion.identity;
        DBC_root.transform.localScale = new Vector3(1 ,1 ,1);
        DBC_root.transform.localScale = new Vector3(1 / skirtSupporter.hips.lossyScale.x,
                                                    1 / skirtSupporter.hips.lossyScale.y,
                                                    1 / skirtSupporter.hips.lossyScale.z);

        foreach (SkirtSupporter.BoneSet skirt in skirtSupporter.skirtBones)
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
                GameObject DBC_branch = new GameObject("DBC_" + name + "_" + skirt.boneObject.name);
                DBC_branch.transform.parent = DBC_root.transform;
                DBC_branch.transform.position = skirt.boneObject.transform.GetChild(0).position;
                DBC_branch.transform.localRotation = Quaternion.identity;
                DBC_branch.transform.localScale = new Vector3(1, 1, 1);
                DBC_branch.transform.Rotate(0, skirt.angle, 0);

                GameObject DBC_prefab = (GameObject)PrefabUtility.InstantiatePrefab(skirtSupporter.colliderPrefab);
                DBC_prefab.transform.parent = DBC_branch.transform;
                DBC_prefab.transform.localPosition = Vector3.zero;
                DBC_prefab.transform.localRotation = Quaternion.identity;
                DBC_prefab.transform.localScale = new Vector3(1, 1, 1);
                skirt.boneObject.GetComponent<DynamicBone>().m_Colliders.Add(DBC_prefab.transform.GetChild(0).GetComponent<DynamicBoneCollider>());
            }
        }
    }

    private void SetSkirtHung()
    {
        Transform exist = skirtSupporter.hips.Find("SkirtRoot");

        if (skirtSupporter.useConstraint)
        {
            GameObject front_parent = new GameObject("_HangFrontParent");
            front_parent.transform.SetParent(skirtSupporter.hips.transform, false);
            front_parent.transform.rotation = skirtSupporter.hips.root.rotation;

            GameObject front = new GameObject("_HangFrontTarget");
            front.transform.SetParent(front_parent.transform, false);
            front.transform.localPosition = Vector3.forward;

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
            skirt_root.transform.rotation = front.transform.rotation;

            var aimconstraint = skirt_root.AddComponent<AimConstraint>();
            ConstraintSource aimconstraint_source = new ConstraintSource();
            aimconstraint_source.sourceTransform = aim.transform;
            aimconstraint_source.weight = 1;
            aimconstraint.AddSource(aimconstraint_source);
            aimconstraint.locked = true;
            aimconstraint.constraintActive = true;

            foreach (SkirtSupporter.BoneSet skirt in skirtSupporter.skirtBones)
            {
                skirt.boneObject.transform.SetParent(skirt_root.transform, true);
            }
        }
        else
        {
            GameObject skirt_root = new GameObject("SkirtRoot");
            skirt_root.transform.parent = skirtSupporter.hips;
            skirt_root.transform.position = skirtSupporter.skirtsParent.transform.position + new Vector3(0, 0.001f, 0);
            skirt_root.transform.rotation = Quaternion.identity;
            skirt_root.transform.localScale = new Vector3(1, 1, 1);

            DynamicBone db = skirt_root.AddComponent<DynamicBone>();
            db.m_Root = skirt_root.transform;
            db.m_Damping = 0;
            db.m_Elasticity = 0;
            db.m_Stiffness = 0;
            db.m_Gravity = new Vector3(0, -1, 0);
            db.m_Force = new Vector3(0, -1, 0);
            db.m_Exclusions = new List<Transform>();

            GameObject skirt_branch = new GameObject("SkirtBranch");
            skirt_branch.transform.parent = skirt_root.transform;
            skirt_branch.transform.position = skirtSupporter.skirtsParent.transform.position;
            skirt_branch.transform.rotation = skirtSupporter.skirtsParent.transform.rotation;
            skirt_branch.transform.localScale = new Vector3(skirtSupporter.skirtsParent.transform.lossyScale.x / skirtSupporter.hips.lossyScale.x,
                                                            skirtSupporter.skirtsParent.transform.lossyScale.y / skirtSupporter.hips.lossyScale.y,
                                                            skirtSupporter.skirtsParent.transform.lossyScale.z / skirtSupporter.hips.lossyScale.z);

            foreach (SkirtSupporter.BoneSet skirt in skirtSupporter.skirtBones)
            {
                skirt.boneObject.transform.parent = skirt_branch.transform;
                db.m_Exclusions.Add(skirt.boneObject.transform);
            }
        }

        if (exist != null)
        {
            DestroyImmediate(exist.gameObject);
        }
    }

    private void AfterCheck()
    {
        foreach (DynamicBone db in skirtSupporter.hips.root.GetComponentsInChildren<DynamicBone>())
        {
            if (!db.gameObject.activeInHierarchy) { continue; }
            if (!db.enabled) { continue; }
            if (!db.m_Root) { continue; }

            List<Transform> skirtObjects = new List<Transform>();
            foreach (SkirtSupporter.BoneSet skirt in skirtSupporter.skirtBones)
            {
                skirtObjects.Add(skirt.boneObject.transform);
            }
            if (skirtObjects.Contains(db.transform)) { continue; }

            foreach (Transform child in GetAffectedObjects(db.m_Root, db.m_Exclusions))
            {
                if (skirtObjects.Contains(child))
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

    private void ClearSkirtHangObject()
    {
        Transform skirtparent = null;

        foreach (SkirtSupporter.BoneSet skirt in skirtSupporter.skirtBones)
        {
            if (skirt.boneObject.transform.parent != skirtSupporter.skirtsParent)
            {
                var parent = skirt.boneObject.transform.parent;
                if (parent != null)
                {
                    skirtparent = parent;
                }
                skirt.boneObject.transform.SetParent(skirtSupporter.skirtsParent.transform, true);
            }
        }

        if (skirtparent != null && skirtparent.name == "SkirtRoot")
        {
            DestroyImmediate(skirtparent.gameObject);
        }
        else if (skirtparent != null && skirtparent.name == "SkirtBranch")
        {
            if (skirtparent.parent != null && skirtparent.parent.name == "SkirtRoot")
            {
                DestroyImmediate(skirtparent.parent.gameObject);
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