using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteoritSpawner : MonoBehaviour
{
	[SerializeField] private float cooldownMin;
	[SerializeField] private float cooldownMax;
	[SerializeField] private float angleRange;

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(cooldownMin);
		Spawn();
	}

	private void Spawn()
	{
		if (ObjectPool.TryGet("Meteorit", out PooledObject meteor))
		{
			meteor.transform.position = transform.position;
			meteor.transform.rotation = transform.rotation;
			meteor.transform.localRotation *= Quaternion.Euler(
											Random.Range(-angleRange, angleRange),
											Random.Range(-angleRange, angleRange),
											0);
			meteor.Action();
		}

		Invoke(nameof(Spawn), Random.Range(cooldownMin, cooldownMax));
	}

	private void OnDisable()
	{
		CancelInvoke();
	}
}
