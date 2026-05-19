using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceShield : MonoBehaviour
{
	[SerializeField] private Life life;

	private void OnTriggerEnter(Collider other)
	{
		if(other.TryGetComponent<Bullet>(out Bullet bullet))
		{
			life.TakeDamage(bullet.Damage);
			bullet.Explode();
		}
	}

}
