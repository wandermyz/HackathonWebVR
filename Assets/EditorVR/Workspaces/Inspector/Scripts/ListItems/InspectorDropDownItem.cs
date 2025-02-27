﻿using System;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif
using UnityEngine;
using UnityEngine.Experimental.EditorVR.UI;

public class InspectorDropDownItem : InspectorPropertyItem
{
	const string kNothing = "Nothing";
	const string kEverything = "Everything";

	[SerializeField]
	DropDown m_DropDown;

#if UNITY_EDITOR
	public override void Setup(InspectorData data)
	{
		base.Setup(data);

		if (m_SerializedProperty.propertyType == SerializedPropertyType.LayerMask)
		{
			m_DropDown.multiSelect = true;

			var options = new List<string> { kNothing, kEverything };
			options.AddRange(InternalEditorUtility.layers);
			m_DropDown.options = options.ToArray();

			switch (m_SerializedProperty.intValue)
			{
				case 0:
					m_DropDown.values = new[] { 0 };
					break;
				case ~0:
					m_DropDown.values = EverythingValues();
					m_DropDown.LabelOverride("Everything");
					break;
				default:
					m_DropDown.values = LayerMaskToIndices(m_SerializedProperty.intValue);
					break;
			}

		}
		else
		{
			m_DropDown.multiSelect = false;
			m_DropDown.options = m_SerializedProperty.enumDisplayNames;
			m_DropDown.value = m_SerializedProperty.enumValueIndex;
		}
	}

	protected override void FirstTimeSetup()
	{
		base.FirstTimeSetup();
		m_DropDown.valueChanged += ValueChanged;
	}

	void ValueChanged(int clicked, int[] values)
	{
		if (m_SerializedProperty.propertyType == SerializedPropertyType.LayerMask)
		{
			if (clicked == 0) // Clicked "Nothing"
			{
				values = new[] { 0 };
				m_DropDown.values = values;

				if (m_SerializedProperty.intValue == 0)
					return;
				m_SerializedProperty.intValue = 0;
				data.serializedObject.ApplyModifiedProperties();
			}
			else if (clicked == 1)  // Clicked "Everything"
			{
				m_DropDown.values = EverythingValues();
				m_DropDown.LabelOverride("Everything");

				if (m_SerializedProperty.intValue == ~0)
					return;
				m_SerializedProperty.intValue = ~0;
				data.serializedObject.ApplyModifiedProperties();
			}
			else
			{
				// Remove "Everything" and "Nothing"
				var list = new List<int>(values);
				if (list.Remove(0) || list.Remove(1))
				{
					values = list.ToArray();
					m_DropDown.values = values;
				}

				var layerMask = IndicesToLayerMask(values);
				if (m_SerializedProperty.intValue != layerMask)
				{
					m_SerializedProperty.intValue = layerMask;
					data.serializedObject.ApplyModifiedProperties();
				}
			}
		}
		else
		{
			if (m_SerializedProperty.enumValueIndex != values[0])
			{
				m_SerializedProperty.enumValueIndex = values[0];
				data.serializedObject.ApplyModifiedProperties();
			}
		}
	}

	int[] EverythingValues()
	{
		var values = new int[InternalEditorUtility.layers.Length + 1];
		for (var i = 0; i < values.Length; i++)
			values[i] = i + 1;
		return values;
	}

	protected override object GetDropObjectForFieldBlock(Transform fieldBlock)
	{
		return m_DropDown.multiSelect ? (object) m_DropDown.values : m_DropDown.value;
	}

	protected override bool CanDropForFieldBlock(Transform fieldBlock, object dropObject)
	{
		return m_DropDown.multiSelect && dropObject is int[]
			|| !m_DropDown.multiSelect && dropObject is int;
	}

	protected override void ReceiveDropForFieldBlock(Transform fieldBlock, object dropObject)
	{
		if (m_DropDown.multiSelect && dropObject is int[])
		{
			m_DropDown.values = (int[]) dropObject;
		}

		if (!m_DropDown.multiSelect && dropObject is int)
		{
			m_DropDown.value = (int) dropObject;
		}
	}

	static int[] LayerMaskToIndices(int layerMask)
	{
		var mask = 1;
		var layers = new List<int>();
		for (var i = 0; i < 32; i++)
		{
			if ((layerMask & mask) != 0)
				layers.Add(Array.IndexOf(InternalEditorUtility.layers, LayerMask.LayerToName(i)) + 2);
			mask <<= 1;
		}
		return layers.ToArray();
	}

	static int IndicesToLayerMask(int[] indices)
	{
		var layerMask = 0;
		foreach (var index in indices)
		{
			if (index == 0) // Nothing
				return 0;
			if (index == 1) // Everything
				return ~0;
			var realIndex = index - 2; // Account for "Nothing" and "Everything"
			if (realIndex >= 0)
				layerMask |= 1 << LayerMask.NameToLayer(InternalEditorUtility.layers[realIndex]);
		}

		return layerMask;
	}
#endif
}