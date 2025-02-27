﻿using System.Collections.Generic;
using UnityEngine.InputNew;
using UnityEngine.Experimental.EditorVR.Tools;

namespace UnityEngine.Experimental.EditorVR.Proxies
{
	/// <summary>
	/// Declares a class as being a proxy for an input device
	/// </summary>
	public interface IProxy
	{
		/// <summary>
		/// Whether the proxy is present and active
		/// </summary>
		bool active
		{
			get;
		}

		/// <summary>
		/// Provided to a proxy for device input (e.g. position / rotation)
		/// </summary>
		TrackedObject trackedObjectInput
		{
			set;
		}

		/// <summary>
		/// The ray origin for each proxy node
		/// </summary>
		Dictionary<Node, Transform> rayOrigins
		{
			get;
		}

		/// <summary>
		/// Whether the proxy is not visible
		/// </summary>
		bool hidden
		{
			set;
		}

		/// <summary>
		/// Origins for where menus show (e.g. main menu)
		/// Key = ray origin
		/// Value = preview transform
		/// </summary>
		Dictionary<Transform, Transform> menuOrigins
		{
			get; set;
		}

		/// <summary>
		/// Origins for alternate menus
		/// Key = ray origin
		/// Value = alternate menu transform
		/// </summary>
		Dictionary<Transform, Transform> alternateMenuOrigins
		{
			get; set;
		}

		/// <summary>
		/// Origins for previews that show
		/// Key = ray origin
		/// Value = preview transform
		/// </summary>
		Dictionary<Transform, Transform> previewOrigins
		{
			get; set;
		}
	}
}
