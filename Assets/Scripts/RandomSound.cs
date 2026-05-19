using UnityEngine;

public class RandomSound : MonoBehaviour
{
	[SerializeField] protected AudioSource _source;
	[SerializeField] protected AudioClip[] _clips;

	public virtual void PlayRandom()
	{
		_source.pitch = Random.Range(0.95f, 1.05f);
		_source.clip = _clips[Random.Range(0, _clips.Length)];
		_source.Play();
	}
}