using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Dynamics.PhysBone.Components;

[CustomEditor(typeof(ParameterCopyPB))]
public class ParameterCopyPBEditor : Editor
{
    private SkirtSupporterPB skirtSupporter;

    public override void OnInspectorGUI()
    {
        ParameterCopyPB parameterCopy = target as ParameterCopyPB;
        skirtSupporter = parameterCopy.transform.parent.GetComponent<SkirtSupporterPB>();

        if (GUILayout.Button("PhysBone設定コピー＆ペースト"))
        {
            Copy();
        }
        if (GUILayout.Button("停止後ペースト"))
        {
            StaticPaste();
        }
    }

    private void Copy()
    {
        foreach (SkirtSupporterPB.BoneSet skirt in skirtSupporter.skirtBones)
        {
            VRCPhysBone pb = skirt.boneObject.GetComponent<VRCPhysBone>();
            UnityEditorInternal.ComponentUtility.CopyComponent(skirtSupporter.physBoneModel);
            var back_collider = new List<VRCPhysBoneCollider>();
            if (pb != null)
            {
                foreach (var collider in pb.colliders)
                {
                    back_collider.Add((VRCPhysBoneCollider)collider);
                }
                UnityEditorInternal.ComponentUtility.PasteComponentValues(pb);
            }
            else
            {
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(skirt.boneObject);
            }

            foreach (VRCPhysBoneCollider collider in back_collider)
            {
                pb.colliders.Add(collider);
            }
            pb.enabled = false;
            EditorApplication.delayCall += () => Restart(pb);
        }
    }

    private void Restart(VRCPhysBone pb)
    {
        pb.enabled = true;
    }

    private void StaticPaste()
    {
        UnityEditorInternal.ComponentUtility.PasteComponentValues(skirtSupporter.physBoneModel);
        Copy();
    }
}