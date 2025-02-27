﻿using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.EditorVR.UI;

public class InspectorVectorItem : InspectorPropertyItem
{
	[SerializeField]
	GameObject ZGroup;

	[SerializeField]
	GameObject WGroup;

#if UNITY_EDITOR
	public override void Setup(InspectorData data)
	{
		base.Setup(data);

		var vector = Vector4.zero;
		var count = 4;
		switch (m_SerializedProperty.propertyType)
		{
			case SerializedPropertyType.Vector2:
				ZGroup.SetActive(false);
				WGroup.SetActive(false);
				vector = m_SerializedProperty.vector2Value;
				count = 2;
				break;
			case SerializedPropertyType.Quaternion:
				vector = m_SerializedProperty.quaternionValue.eulerAngles;
				ZGroup.SetActive(true);
				WGroup.SetActive(false);
				count = 3;
				break;
			case SerializedPropertyType.Vector3:
				vector = m_SerializedProperty.vector3Value;
				ZGroup.SetActive(true);
				WGroup.SetActive(false);
				count = 3;
				break;
			case SerializedPropertyType.Vector4:
				vector = m_SerializedProperty.vector4Value;
				ZGroup.SetActive(true);
				WGroup.SetActive(true);
				break;
		}

		m_CuboidLayout.UpdateObjects();

		UpdateInputFields(count, vector);
	}

	void UpdateInputFields(int count, Vector4 vector)
	{
		for (var i = 0; i < count; i++)
		{
			m_InputFields[i].text = vector[i].ToString();
			m_InputFields[i].ForceUpdateLabel();
		}
	}

	protected override void FirstTimeSetup()
	{
		base.FirstTimeSetup(); 

		for (var i = 0; i < m_InputFields.Length; i++)
		{
			var index = i;
			m_InputFields[i].onValueChanged.AddListener(value => SetValue(value, index));
		}
	}

	public bool SetValue(string input, int index)
	{
		float value;
		if (!float.TryParse(input, out value)) return false;
		switch (m_SerializedProperty.propertyType)
		{
			case SerializedPropertyType.Vector2:
				var vector2 = m_SerializedProperty.vector2Value;
				if (!Mathf.Approximately(vector2[index], value))
				{
					vector2[index] = value;
					m_SerializedProperty.vector2Value = vector2;
					UpdateInputFields(2, vector2);
				}
				break;
			case SerializedPropertyType.Vector3:
				var vector3 = m_SerializedProperty.vector3Value;
				if (!Mathf.Approximately(vector3[index], value))
				{
					vector3[index] = value;
					m_SerializedProperty.vector3Value = vector3;
					UpdateInputFields(3, vector3);
				}
				break;
			case SerializedPropertyType.Vector4:
				var vector4 = m_SerializedProperty.vector4Value;
				if (!Mathf.Approximately(vector4[index], value))
				{
					vector4[index] = value;
					m_SerializedProperty.vector4Value = vector4;
					UpdateInputFields(4, vector4);
				}
				break;
			case SerializedPropertyType.Quaternion:
				var euler = m_SerializedProperty.quaternionValue.eulerAngles;
				if (!Mathf.Approximately(euler[index], value))
				{
					euler[index] = value;
					m_SerializedProperty.quaternionValue = Quaternion.Euler(euler);
					UpdateInputFields(3, euler);
				}
				break;
		}

		data.serializedObject.ApplyModifiedProperties();

		return true;
	}

	protected override object GetDropObjectForFieldBlock(Transform fieldBlock)
	{
		object dropObject = null;
		var inputfields = fieldBlock.GetComponentsInChildren<NumericInputField>();
		if (inputfields.Length > 1)
		{
			switch (m_SerializedProperty.propertyType)
			{
				case SerializedPropertyType.Vector2:
					dropObject = m_SerializedProperty.vector2Value;
					break;
				case SerializedPropertyType.Quaternion:
					dropObject = m_SerializedProperty.quaternionValue;
					break;
				case SerializedPropertyType.Vector3:
					dropObject = m_SerializedProperty.vector3Value;
					break;
				case SerializedPropertyType.Vector4:
					dropObject = m_SerializedProperty.vector4Value;
					break;
			}
		}
		else if (inputfields.Length > 0)
			dropObject = inputfields[0].text;

		return dropObject;
	}

	protected override bool CanDropForFieldBlock(Transform fieldBlock, object dropObject)
	{
		return dropObject is string || dropObject is Vector2 || dropObject is Vector3
			|| dropObject is Vector4 || dropObject is Quaternion || dropObject is Color;
	}

	protected override void ReceiveDropForFieldBlock(Transform fieldBlock, object dropObject)
	{
		var str = dropObject as string;
		if (str != null)
		{
			var inputField = fieldBlock.GetComponentInChildren<NumericInputField>();
			var index = Array.IndexOf(m_InputFields, inputField);

			if (SetValue(str, index))
			{
				inputField.text = str;
				inputField.ForceUpdateLabel();
			}
		}

		if (dropObject is Vector2)
		{
			var vector2 = (Vector2) dropObject;
			switch (m_SerializedProperty.propertyType)
			{
				case SerializedPropertyType.Vector2:
					m_SerializedProperty.vector2Value = vector2;
					break;
				case SerializedPropertyType.Vector3:
					var vector3 = (Vector4) vector2;
					vector3.z = m_SerializedProperty.vector3Value.z;
					m_SerializedProperty.vector3Value = vector3;
					break;
				case SerializedPropertyType.Vector4:
					var vector4 = m_SerializedProperty.vector4Value;
					vector3.x = vector2.x;
					vector3.y = vector2.y;
					m_SerializedProperty.vector4Value = vector4;
					break;
				case SerializedPropertyType.Quaternion:
					var euler = m_SerializedProperty.quaternionValue.eulerAngles;
					euler.x = vector2.x;
					euler.y = vector2.y;
					m_SerializedProperty.quaternionValue = Quaternion.Euler(euler);
					break;
			}

			UpdateInputFields(2, vector2);

			data.serializedObject.ApplyModifiedProperties();
		}

		if (dropObject is Vector3)
		{
			var vector3 = (Vector3)dropObject;
			switch (m_SerializedProperty.propertyType)
			{
				case SerializedPropertyType.Vector2:
					m_SerializedProperty.vector2Value = vector3;
					break;
				case SerializedPropertyType.Vector3:
					m_SerializedProperty.vector3Value = vector3;
					break;
				case SerializedPropertyType.Vector4:
					var vector4 = (Vector4) vector3;
					vector4.w = m_SerializedProperty.vector4Value.w;
					m_SerializedProperty.vector4Value = vector4;
					break;
				case SerializedPropertyType.Quaternion:
					m_SerializedProperty.quaternionValue = Quaternion.Euler(vector3);
					break;
			}

			UpdateInputFields(3, vector3);

			data.serializedObject.ApplyModifiedProperties();
		}

		if (dropObject is Vector4)
		{
			var vector4 = (Vector4)dropObject;
			switch (m_SerializedProperty.propertyType)
			{
				case SerializedPropertyType.Vector2:
					m_SerializedProperty.vector2Value = vector4;
					break;
				case SerializedPropertyType.Vector3:
					m_SerializedProperty.vector3Value = vector4;
					break;
				case SerializedPropertyType.Vector4:
					m_SerializedProperty.vector4Value = vector4;
					break;
				case SerializedPropertyType.Quaternion:
					m_SerializedProperty.quaternionValue = new Quaternion(vector4.x, vector4.y, vector4.z, vector4.w);
					break;
			}

			UpdateInputFields(4, vector4);

			data.serializedObject.ApplyModifiedProperties();
		}

		if (dropObject is Color)
		{
			var color = (Color)dropObject;
			switch (m_SerializedProperty.propertyType)
			{
				case SerializedPropertyType.Vector2:
					Vector2 vector2;
					vector2.x = color.r;
					vector2.y = color.g;
					m_SerializedProperty.vector2Value = vector2;
					break;
				case SerializedPropertyType.Vector3:
					Vector3 vector3;
					vector3.x = color.r;
					vector3.y = color.g;
					vector3.z = color.b;
					m_SerializedProperty.vector3Value = vector3;
					break;
				case SerializedPropertyType.Vector4:
					m_SerializedProperty.vector4Value = color;
					break;
				case SerializedPropertyType.Quaternion:
					m_SerializedProperty.quaternionValue = new Quaternion(color.r, color.g, color.b, color.a);
					break;
			}

			UpdateInputFields(4, color);

			data.serializedObject.ApplyModifiedProperties();
		}

		if (dropObject is Quaternion)
		{
			var quaternion = (Quaternion)dropObject;
			switch (m_SerializedProperty.propertyType)
			{
				case SerializedPropertyType.Vector2:
					m_SerializedProperty.vector2Value = quaternion.eulerAngles;
					break;
				case SerializedPropertyType.Vector3:
					m_SerializedProperty.vector3Value = quaternion.eulerAngles;
					break;
				case SerializedPropertyType.Vector4:
					m_SerializedProperty.vector4Value = new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
					break;
				case SerializedPropertyType.Quaternion:
					m_SerializedProperty.quaternionValue = quaternion;
					break;
			}

			UpdateInputFields(3, quaternion.eulerAngles);

			data.serializedObject.ApplyModifiedProperties();
		}
	}
#endif
}