using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ParameterCopyH))]//拡張するクラスを指定
public class ParameterCopyHEditor : Editor
{
    private ParameterCopyH parameterCopyH;
    private LonghairSupporter longhairSupporter;

    /// <summary>
    /// InspectorのGUIを更新
    /// </summary>
    public override void OnInspectorGUI()
    {
        //元のInspector部分を表示
        base.OnInspectorGUI();

        //targetを変換して対象を取得
        parameterCopyH = target as ParameterCopyH;
        longhairSupporter = parameterCopyH.transform.parent.GetComponent<LonghairSupporter>();

        if (GUILayout.Button("DynamicBone設定コピー＆ペースト"))
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
        foreach (LonghairSupporter.HairsGroup group in longhairSupporter.hairsGroup)
        {
            foreach (LonghairSupporter.HairsGroup.BoneSet hair in group.hairBones)
            {
                if (hair.hairType != parameterCopyH.hairType) { continue; }

                DynamicBone db = hair.boneObject.GetComponent<DynamicBone>();
                UnityEditorInternal.ComponentUtility.CopyComponent(longhairSupporter.dynamicBoneModels[(int)parameterCopyH.hairType]);
                var back_collider = new List<DynamicBoneCollider>();
                if (db != null)
                {
                    foreach (DynamicBoneCollider collider in db.m_Colliders)
                    {
                        back_collider.Add(collider);
                    }
                    UnityEditorInternal.ComponentUtility.PasteComponentValues(db);
                }
                else
                {
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(hair.boneObject);
                }

                db.m_Root = db.transform;
                foreach (DynamicBoneCollider collider in back_collider)
                {
                    db.m_Colliders.Add(collider);
                }
            }
        }
    }

    private void StaticPaste()
    {
        UnityEditorInternal.ComponentUtility.PasteComponentValues(longhairSupporter.dynamicBoneModels[(int)parameterCopyH.hairType]);
        Copy();
    }
}