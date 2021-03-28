using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ParameterCopy))]//拡張するクラスを指定
public class ParameterCopyEditor : Editor
{
    private SkirtSupporter skirtSupporter;

    /// <summary>
    /// InspectorのGUIを更新
    /// </summary>
    public override void OnInspectorGUI()
    {
        //元のInspector部分を表示
        base.OnInspectorGUI();

        //targetを変換して対象を取得
        ParameterCopy parameterCopy = target as ParameterCopy;
        skirtSupporter = parameterCopy.transform.parent.GetComponent<SkirtSupporter>();

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
        foreach (SkirtSupporter.BoneSet skirt in skirtSupporter.skirtBones)
        {
            DynamicBone db = skirt.boneObject.GetComponent<DynamicBone>();
            UnityEditorInternal.ComponentUtility.CopyComponent(skirtSupporter.dynamicBoneModel);
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
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(skirt.boneObject);
            }

            db.m_Root = db.transform;
            foreach (DynamicBoneCollider collider in back_collider)
            {
                db.m_Colliders.Add(collider);
            }
        }
    }

    private void StaticPaste()
    {
        UnityEditorInternal.ComponentUtility.PasteComponentValues(skirtSupporter.dynamicBoneModel);
        Copy();
    }
}