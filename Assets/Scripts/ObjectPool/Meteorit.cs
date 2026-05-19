using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteorit : Bullet
{
	[SerializeField] private Mesh[] meshes;
	[SerializeField] private MeshFilter filter;
	[SerializeField] private MeshCollider collider;
	[SerializeField] private float sizeMin;
	[SerializeField] private float sizeMax;
	[SerializeField] private float angularVelocity;

	public override void Action()
	{
		base.Action();

		Mesh mesh = meshes[Random.Range(0, meshes.Length)];
		filter.mesh = mesh;
		collider.sharedMesh = mesh;
		transform.localScale = Vector3.one * Random.Range(sizeMin, sizeMax);
		rb.angularVelocity = new Vector3(	Random.Range(-angularVelocity, angularVelocity),
											Random.Range(-angularVelocity, angularVelocity),
											Random.Range(-angularVelocity, angularVelocity));
	}
}
