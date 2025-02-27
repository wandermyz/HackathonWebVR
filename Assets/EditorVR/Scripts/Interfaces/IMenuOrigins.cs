﻿using UnityEngine;

/// <summary>
/// The transforms for various menu origins provided by device proxies
/// </summary>
public interface IMenuOrigins
{
	/// <summary>
	/// The transform under which the menu should be parented
	/// </summary>
	Transform menuOrigin { set; }

	/// <summary>
	/// The transform under which the alternate menu should be parented
	/// </summary>
	Transform alternateMenuOrigin { set; }
}
