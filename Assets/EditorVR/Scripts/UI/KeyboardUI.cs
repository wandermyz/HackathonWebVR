using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.EditorVR.Extensions;
using UnityEngine.Experimental.EditorVR.Handles;
using UnityEngine.Experimental.EditorVR.Helpers;
using UnityEngine.Experimental.EditorVR.Manipulators;
using UnityEngine.Experimental.EditorVR.Utilities;

public class KeyboardUI : MonoBehaviour
{
	const float kDragWaitTime = 0.2f;
	const float kKeyLayoutTransitionTime = 0.5f;
	const float kKeyExpandCollapseTime = 0.25f;
	const float kHandleChangeColorTime = 0.1f;
	const float kHorizontalThreshold = 0.7f;
	static Color sHandleDragColor = UnityBrandColorScheme.green;

	[SerializeField]
	List<KeyboardButton> m_Buttons = new List<KeyboardButton>();

	[SerializeField]
	List<Transform> m_VerticalLayoutTransforms = new List<Transform>();

	[SerializeField]
	List<Transform> m_HorizontalLayoutTransforms = new List<Transform>();

	[SerializeField]
	DirectManipulator m_DirectManipulator;

	[SerializeField]
	SmoothMotion m_SmoothMotion;

	bool m_EligibleForDrag;
	bool m_CurrentlyHorizontal;
	Material m_HandleMaterial;

	Coroutine m_ChangeDragColorsCoroutine;
	Coroutine m_MoveKeysCoroutine;
	Coroutine m_DragAfterDelayCoroutine;

	public KeyboardButton handleButton { get; set; }

	public bool collapsed { get; set; }
	public bool collapsing { get; set; }

	/// <summary>
	/// Initialize the keyboard and its buttons
	/// </summary>
	/// <param name="keyPress"></param>
	public void Setup(Action<char> keyPress)
	{
		m_DirectManipulator.target = transform;
		m_DirectManipulator.translate = Translate;
		m_DirectManipulator.rotate = Rotate;

		foreach (var handle in m_DirectManipulator.GetComponentsInChildren<BaseHandle>(true))
		{
			handle.dragStarted += OnDragStarted;
			handle.dragging += OnDrag;
			handle.dragEnded += OnDragEnded;
		}

		foreach (var button in m_Buttons)
		{
			button.Setup(keyPress, IsHorizontal, InTransition);
		}

		m_HandleMaterial = handleButton.targetMeshMaterial;

		this.StopCoroutine(ref m_MoveKeysCoroutine);

		if (collapsed)
		{
			foreach (var button in m_Buttons)
			{
				if (button != handleButton)
					button.transform.position = handleButton.transform.position;
				button.SetTextAlpha(0f, 0f);
			}

			m_MoveKeysCoroutine = StartCoroutine(ExpandOverTime());
		}
	}

	/// <summary>
	/// Determines if a ray origin is at the range and orientation to convert to a mallet
	/// </summary>
	/// <param name="rayOrigin">The ray origin to check against</param>
	/// <returns>True if the mallet should be shown, false otherwise</returns>
	public bool ShouldShowMallet(Transform rayOrigin)
	{
		if (!isActiveAndEnabled || !IsHorizontal() || m_EligibleForDrag)
			return false;

		var rayOriginPos = rayOrigin.position;

		var grabbingHandle = false;
		var outOfRange = false;
		var invalidOrientation = false;

		const float nearDist = 0.04f;
		const float maxAngle = 0.5f;
		if ((transform.position - rayOriginPos).magnitude < nearDist
			&& Vector3.Dot(handleButton.transform.forward, rayOrigin.forward) > 0.4f)
			grabbingHandle = true;

		const float farDist = 0.18f;
		if ((transform.position - rayOriginPos).magnitude > farDist)
			outOfRange = true;

		if (transform.InverseTransformPoint(rayOrigin.position).z > 0.02f)
			outOfRange = true;

		if (Vector3.Dot(handleButton.transform.up, rayOrigin.forward) < maxAngle)
			invalidOrientation = true;

		return !(grabbingHandle || outOfRange || invalidOrientation);
	}

	IEnumerator ExpandOverTime()
	{
		const float kButtonMoveTimeOffset = 0.01f;
		var horizontal = IsHorizontal();
		var t = 0f;
		int i = 0;
		while (i < m_Buttons.Count)
		{
			if (t < i * kButtonMoveTimeOffset)
			{
				t += Time.unscaledDeltaTime;
				continue;
			}

			var targetPos = horizontal
				? m_HorizontalLayoutTransforms[i].position
				: m_VerticalLayoutTransforms[i].position;
			m_Buttons[i].MoveToPosition(targetPos, kKeyExpandCollapseTime);
			i++;
			yield return null;
		}

		SetAllButtonsTextAlpha(1f);

		collapsed = false;

		m_MoveKeysCoroutine = null;
	}

	void SetAllButtonsTextAlpha(float alpha)
	{
		foreach (var button in m_Buttons)
		{
			button.SetTextAlpha(alpha, kHandleChangeColorTime);
		}
	}

	/// <summary>
	/// Collapse the keyboard button positions into the handle position
	/// </summary>
	/// <param name="doneCollapse">The callback to be invoked when collapse is done</param>
	public void Collapse(Action doneCollapse)
	{
		SetAllButtonsTextAlpha(0f);

		this.StopCoroutine(ref m_MoveKeysCoroutine);

		if (isActiveAndEnabled)
		{
			collapsing = true;
			m_MoveKeysCoroutine = StartCoroutine(CollapseOverTime(doneCollapse));
		}
		else
		{
			collapsing = false;
			doneCollapse();
		}
	}

	IEnumerator CollapseOverTime(Action doneCollapse)
	{
		const float kButtonMoveTimeOffset = 0.01f;
		var t = 0f;
		int i = 0;
		while (i < m_Buttons.Count)
		{
			if (t < i * kButtonMoveTimeOffset)
			{
				t += Time.unscaledDeltaTime;
				continue;
			}

			var targetPos = handleButton.transform.position;
			m_Buttons[m_Buttons.Count - 1 - i].MoveToPosition(targetPos, kKeyExpandCollapseTime);
			i++;
			yield return null;
		}

		collapsing = false;
		collapsed = true;
		doneCollapse();
		m_MoveKeysCoroutine = null;
	}

	/// <summary>
	/// Activate shift mode on a button
	/// </summary>
	public void ActivateShiftModeOnKey(KeyboardButton key)
	{
		foreach (var button in m_Buttons)
		{
			if (button == key)
				button.SetShiftModeActive(true);
		}
	}

	/// <summary>
	/// Activate shift mode on all buttons
	/// </summary>
	public void ActivateShiftModeOnKeys()
	{
		foreach (var button in m_Buttons)
			button.SetShiftModeActive(true);
	}

	/// <summary>
	/// Deactivate shift mode on all buttons
	/// </summary>
	public void DeactivateShiftModeOnKeys()
	{
		foreach (var button in m_Buttons)
			button.SetShiftModeActive(false);
	}

	/// <summary>
	/// Deactivate shift mode on a button
	/// </summary>
	public void DeactivateShiftModeOnKey(KeyboardButton key)
	{
		foreach (var button in m_Buttons)
		{
			if (button == key)
				button.SetShiftModeActive(false);
		}
	}

	IEnumerator MoveKeysToLayoutPositions(float duration = kKeyLayoutTransitionTime)
	{
		var horizontal = IsHorizontal();
		var t = 0f;
		while (t < duration)
		{
			int i = 0;
			foreach (var button in m_Buttons)
			{
				var targetPos = horizontal
					? m_HorizontalLayoutTransforms[i].position
					: m_VerticalLayoutTransforms[i].position;
				button.transform.position = Vector3.Lerp(button.transform.position, targetPos, t / duration);
				i++;
			}
			t += Time.unscaledDeltaTime;
			yield return null;
		}

		int k = 0;
		foreach (var button in m_Buttons)
		{
			var targetPos = horizontal
				? m_HorizontalLayoutTransforms[k].position
				: m_VerticalLayoutTransforms[k].position;
			button.transform.position = targetPos;
			k++;
		}

		m_MoveKeysCoroutine = null;
	}

	/// <summary>
	/// Instantly move all keys to vertical layout positions
	/// </summary>
	public void ForceMoveButtonsToVerticalLayout()
	{
		int i = 0;
		foreach (var button in m_Buttons)
		{
			var t = m_VerticalLayoutTransforms[i];
			if (t)
			{
				button.transform.position = m_VerticalLayoutTransforms[i].position;
			}
			i++;
		}
	}

	/// <summary>
	/// Instantly move all keys to horizontal layout positions
	/// </summary>
	public void ForceMoveButtonsToHorizontalLayout()
	{
		int i = 0;
		foreach (var button in m_Buttons)
		{
			var t = m_HorizontalLayoutTransforms[i];
			if (t)
			{
				button.transform.position = t.position;
			}
			i++;
		}
	}

	void Awake()
	{
		handleButton = m_DirectManipulator.GetComponent<KeyboardButton>();
		collapsed = true;
	}

	void OnEnable()
	{
		m_EligibleForDrag = false;
		m_SmoothMotion.enabled = false;
	}

	bool IsHorizontal()
	{
		return Vector3.Dot(transform.up, Vector3.up) < kHorizontalThreshold
			&& Vector3.Dot(transform.forward, Vector3.up) < 0f;
	}

	bool InTransition()
	{
		return collapsing || m_EligibleForDrag;
	}

	private void Translate(Vector3 deltaPosition)
	{
		if (m_EligibleForDrag)
			transform.position += deltaPosition;
	}

	private void Rotate(Quaternion deltaRotation)
	{
		if (m_EligibleForDrag)
			transform.rotation *= deltaRotation;
	}

	void OnDragStarted(BaseHandle baseHandle, HandleEventData handleEventData)
	{
		this.StopCoroutine(ref m_DragAfterDelayCoroutine);
		m_DragAfterDelayCoroutine = StartCoroutine(DragAfterDelay());
	}

	IEnumerator DragAfterDelay()
	{
		var t = 0f;
		while (t < kDragWaitTime)
		{
			t += Time.unscaledDeltaTime;
			yield return null;
		}

		m_EligibleForDrag = true;
		m_DragAfterDelayCoroutine = null;

		StartDrag();
	}

	void StartDrag()
	{
		this.StopCoroutine(ref m_ChangeDragColorsCoroutine);
		m_ChangeDragColorsCoroutine = StartCoroutine(SetDragColors());

		m_SmoothMotion.enabled = true;

		SetAllButtonsTextAlpha(0f);
	}

	IEnumerator SetDragColors()
	{
		if (!gameObject.activeInHierarchy) yield break;
		var t = 0f;
		var startColor = m_HandleMaterial.color;
		while (t < kHandleChangeColorTime)
		{
			var alpha = t / kHandleChangeColorTime;
			m_HandleMaterial.color = Color.Lerp(startColor, sHandleDragColor, alpha);
			t += Time.unscaledDeltaTime;
			yield return null;
		}

		m_HandleMaterial.color = sHandleDragColor;

		m_ChangeDragColorsCoroutine = null;
	}

	IEnumerator UnsetDragColors()
	{
		if (!gameObject.activeInHierarchy) yield break;

		var t = 0f;
		var startColor = m_HandleMaterial.color;
		while (t < kHandleChangeColorTime)
		{
			var alpha = t / kHandleChangeColorTime;
			m_HandleMaterial.color = Color.Lerp(startColor, handleButton.targetMeshBaseColor, alpha);
			t += Time.unscaledDeltaTime;
			yield return null;
		}

		m_HandleMaterial.color = handleButton.targetMeshBaseColor;

		m_ChangeDragColorsCoroutine = null;
	}

	void OnDrag(BaseHandle baseHandle, HandleEventData handleEventData)
	{
		if (m_EligibleForDrag)
		{
			var horizontal = IsHorizontal();
			if (m_CurrentlyHorizontal != horizontal)
			{
				this.StopCoroutine(ref m_MoveKeysCoroutine);
				m_MoveKeysCoroutine = StartCoroutine(MoveKeysToLayoutPositions(kKeyExpandCollapseTime));
				
				m_CurrentlyHorizontal = horizontal;
			}
		}
	}

	void OnDragEnded(BaseHandle baseHandle, HandleEventData handleEventData)
	{
		this.StopCoroutine(ref m_DragAfterDelayCoroutine);
		m_DragAfterDelayCoroutine = null;

		if (m_EligibleForDrag)
		{
			m_EligibleForDrag = false;

			m_SmoothMotion.enabled = false;
			SetAllButtonsTextAlpha(1f);

			this.StopCoroutine(ref m_ChangeDragColorsCoroutine);
			if (isActiveAndEnabled)
				m_ChangeDragColorsCoroutine = StartCoroutine(UnsetDragColors());
		}
	}

	void OnDisable()
	{
		this.StopCoroutine(ref m_ChangeDragColorsCoroutine);
		m_ChangeDragColorsCoroutine = null;
	}
}