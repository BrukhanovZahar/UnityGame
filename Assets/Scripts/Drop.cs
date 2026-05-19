using UnityEngine;

public class Drop : MonoBehaviour
{
	[SerializeField] private Vector3 dropOffset = Vector3.up;
	[SerializeField] private PooledObject[] items;
	[SerializeField] private int countMin;
	[SerializeField] private int countMax;

	public void DropItem()
	{
		int count = Random.Range(countMin, countMax);
		for(int i = 0; i < count; i++)
			if(ObjectPool.TryGet(items[Random.Range(0, items.Length)].PoolKey, out PooledObject obj))
			{
				obj.transform.position = transform.position +
					new Vector3(
						Random.Range(-dropOffset.x, dropOffset.x),
						Random.Range(-dropOffset.y, dropOffset.y),
						Random.Range(-dropOffset.z, dropOffset.z)
						);

				obj.Action();
			}
	}
}
