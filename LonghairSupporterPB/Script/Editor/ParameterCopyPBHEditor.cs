using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Dynamics.PhysBone.Components;

[CustomEditor(typeof(ParameterCopyPBH))]//拡張するクラスを指定
public class ParameterCopyPBHEditor : Editor
{
    private ParameterCopyPBH parameterCopyH;
    private LonghairSupporterPB longhairSupporter;

    public override void OnInspectorGUI()
    {
        parameterCopyH = target as ParameterCopyPBH;
        longhairSupporter = parameterCopyH.transform.parent.GetComponent<LonghairSupporterPB>();

        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("hairType"));

        if (GUILayout.Button("PhysBone設定コピー＆ペースト"))
        {
            Copy();
        }
        if (GUILayout.Button("停止後ペースト"))
        {
            StaticPaste();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void Copy()
    {
        foreach (LonghairSupporterPB.HairsGroup group in longhairSupporter.hairsGroup)
        {
            foreach (LonghairSupporterPB.HairsGroup.BoneSet hair in group.hairBones)
            {
                if (hair.hairType != parameterCopyH.hairType) { continue; }

                VRCPhysBone pb = hair.boneObject.GetComponent<VRCPhysBone>();
                UnityEditorInternal.ComponentUtility.CopyComponent(longhairSupporter.physBoneModels[(int)parameterCopyH.hairType]);
                var back_collider = new List<VRCPhysBoneCollider>();
                if (pb != null)
                {
                    foreach (VRCPhysBoneCollider collider in pb.colliders)
                    {
                        back_collider.Add(collider);
                    }
                    UnityEditorInternal.ComponentUtility.PasteComponentValues(pb);
                }
                else
                {
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(hair.boneObject);
                }

                foreach (VRCPhysBoneCollider collider in back_collider)
                {
                    pb.colliders.Add(collider);
                }
                pb.enabled = false;
                EditorApplication.delayCall += () => Restart(pb);
            }
        }
    }

    private void Restart(VRCPhysBone pb)
    {
        pb.enabled = true;
    }

    private void StaticPaste()
    {
        UnityEditorInternal.ComponentUtility.PasteComponentValues(longhairSupporter.physBoneModels[(int)parameterCopyH.hairType]);
        Copy();
    }
}