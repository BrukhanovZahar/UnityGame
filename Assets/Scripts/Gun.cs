using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
	[SerializeField] private Transform model;
	[SerializeField] private Transform bulletSpawnPoint;
	[SerializeField] private float shootCooldown;
	[SerializeField] private string bulletKey;
	[SerializeField] private string soundKey;
	[SerializeField] private LayerMask aimMask;

	private Camera cam;
	private float lastShootTime = 0;

	private void Start()
	{
		cam = Camera.main;
	}

	private void Update()
	{
		Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

		if (Physics.Raycast(ray, out RaycastHit hit, 500f, aimMask))
			model.LookAt(hit.point);
		else
			model.LookAt(ray.GetPoint(100f));

		if (Input.GetMouseButton(0) && Time.time - lastShootTime >= shootCooldown)
		{
			if (GlobalContext.Pause)
				return;

			lastShootTime = Time.time;
			Shoot();
		}
	}

	public void MultiplyCooldown(float multiplier)
	{
		shootCooldown *= multiplier;
	}

	private void Shoot()
	{
		if(ObjectPool.TryGet(bulletKey, out PooledObject bullet))
		{
			bullet.transform.position = bulletSpawnPoint.position;
			bullet.transform.rotation = bulletSpawnPoint.rotation;
			bullet.Action();
		}
			

		DJ.Play(soundKey);
	}
}