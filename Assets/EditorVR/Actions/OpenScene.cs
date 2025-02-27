﻿namespace UnityEngine.Experimental.EditorVR.Actions
{
	[ActionMenuItem("OpenScene", "Scene")]
	public class OpenScene : BaseAction
	{
		public override void ExecuteAction()
		{
			Debug.LogError("ExecuteAction Action should open a sub-panel showing available scenes to open, if any are found");
		}
	}
}