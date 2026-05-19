using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystall : PooledObject
{
	[SerializeField] private int value;
	[SerializeField] private Collider collider;

	public int Value => value;

	public void Grab()
	{
		collider.enabled = false;
		StopAllCoroutines();
	}

	private void Start()
	{
		//transform.SetParent(GlobalContext.AsteriodCollider.transform);
	}

	public override void Action()
	{
		base.Action();

		MeshCollider asteroid = GlobalContext.AsteriodCollider;

		Vector3 targetPoint = asteroid.ClosestPoint(transform.position);
		targetPoint += (transform.position - targetPoint).normalized * 3;
		StartCoroutine(Landing(targetPoint));
	}

	private IEnumerator Landing(Vector3 target)
	{
		Vector3 start = transform.localPosition;
		for (float t = 0; t < 1; t += Time.deltaTime / 2)
		{
			transform.localPosition = Vector3.Lerp(start, target, t);
			yield return null;
		}

		transform.SetParent(GlobalContext.AsteriodCollider.transform);
	}

	public override void Refresh()
	{
		base.Refresh();
		collider.enabled = true;
	}
}
