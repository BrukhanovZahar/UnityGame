using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PooledObject : MonoBehaviour
{
	[SerializeField] private string poolKey;
	[SerializeField] private UnityEvent onRefresh;
	[SerializeField] private UnityEvent action;
	public string PoolKey => poolKey;

	public virtual void Refresh()
	{
		gameObject.SetActive(true);
		onRefresh.Invoke();
	}

	public virtual void Action()
	{
		action.Invoke();
	}

	public void ReturnToPool()
	{
		StopAllCoroutines();
		CancelInvoke();
		gameObject.SetActive(false);
		ObjectPool.ReturnToPool(poolKey, this);
	}
}
