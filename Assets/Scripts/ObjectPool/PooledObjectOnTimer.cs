using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledObjectOnTimer : PooledObject
{
    [SerializeField] private float timeToReturnToPool;

	public override void Refresh()
	{
		base.Refresh();

		Invoke(nameof(ReturnToPool), timeToReturnToPool);
	}

	private void OnDestroy()
	{
		CancelInvoke();
	}
}
