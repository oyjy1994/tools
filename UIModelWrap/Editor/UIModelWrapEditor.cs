using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIModelWrapEidtor : RawImageEditor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var uiModelWrap = target as UIModelWrap;
		GUI.changed = true;
		var newModel = EditorGUILayout.ObjectField("关联模型：", uiModelWrap);
		if(newModel != null)
		{
			Undo.RecordObjects(uiModelWrap, "UIModelWrap Change");
			if(newModel!=uiModelWrap)
				uiModelWrap.ModelObject = newModel as GameObject;
			EditorUtility.SetDirty(uiModelWrap);
		}
		uiModelWrap.isLoop = EditorGUILayout.Toggle("保持更新", uiModelWrap.isLoop);
		uiModelWrap.isDebug = EditorGUILayout.Toggle("保持更新", uiModelWrap.isDebug);
		if(GUILayout.Button("刷新"))
		{
			uiModelWrap.Refresh();
		}
		EditorGUI.BeginDisabledGroup(!uiModelWrap.isDebug);
		if(GUILayout.Button("保存截图"))
		{
			uiModelWrap.Save();			
		}
		EditorGUI.EndDisabledGroup();
		EditorUtility.SetDirty(uiModelWrap);
	}
}