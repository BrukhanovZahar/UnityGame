using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CrystallCollector : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI ui;
	private int amount;

	public bool TryGetCrystals(int value)
	{
		if (value > amount)
			return false;

		amount -= value;
		ui.text = amount.ToString();
		return true;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent<Crystall>(out Crystall crystall))
		{
			crystall.Grab();
			StartCoroutine(TakeExp(crystall));
		}
	}

	private IEnumerator TakeExp(Crystall crystall)
	{
		Vector3 a = crystall.transform.position;
		for (float t = 0; t < 1f; t += Time.deltaTime * 2)
		{
			crystall.transform.position = Vector3.Lerp(a, transform.position, t);
			yield return null;
		}

		amount += crystall.Value;
		ui.text = amount.ToString();
		crystall.ReturnToPool();
	}
}
