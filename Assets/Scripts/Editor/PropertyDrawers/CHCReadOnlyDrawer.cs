using CHCEditorTools;
using UnityEditor;
using UnityEngine;

namespace Editor.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(CHCReadOnlyAttribute))]
	public class CHCReadOnlyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label);
			GUI.enabled = true;
		}
	}
}