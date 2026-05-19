using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PooledObjectOnTimer
{
	[SerializeField] protected Rigidbody rb;

	[SerializeField] private float damage;
	[SerializeField] private string effectKey;
	[SerializeField] private float startImpulse;
	[SerializeField] private LayerMask mask;

	public float Damage => damage;

	public override void Action()
	{
		base.Action();

		rb.velocity = transform.forward * startImpulse;
		rb.angularVelocity = Vector3.zero;
	}

	public void Explode()
	{
		if (ObjectPool.TryGet(effectKey, out PooledObject effect))
		{
			effect.transform.position = transform.position;
		}

		ReturnToPool();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if ((mask.value & (1 << collision.gameObject.layer)) == 0)
			return;

		if (collision.gameObject.TryGetComponent<Life>(out Life target))
			target.TakeDamage(damage);

		Explode();
	}
}