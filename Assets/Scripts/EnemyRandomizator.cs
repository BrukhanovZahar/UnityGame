using UnityEngine;
using UnityEngine.AI;

public class EnemyRandomizator : MonoBehaviour
{
    [SerializeField] private Transform _model;
    //[SerializeField] private NavMeshAgent _agent;
    [SerializeField, Range(0f,1f)] private float _sizeRangePercent;
    [SerializeField, Range(0f, 0.95f)] private float _speedRangePercent;
    [SerializeField, Range(0f, 100f)] private float _optionalObjectsShowChance;
    [SerializeField] private GameObject[] _optionalObjects;

    [SerializeField, HideInInspector] private Vector3 _baseScale;
    [SerializeField, HideInInspector] private float _baseSpeed;

	private void OnValidate()
    {
        _baseScale = _model.localScale;
        //_baseSpeed = _agent.speed;
    }

	public void Randomize()
	{
        Vector3 scale = _baseScale;
        scale.x *= Random.Range(1 - _sizeRangePercent, 1 + _sizeRangePercent);
        scale.y *= Random.Range(1 - _sizeRangePercent, 1 + _sizeRangePercent);
        scale.z *= Random.Range(1 - _sizeRangePercent, 1 + _sizeRangePercent);
        _model.localScale = scale;

        //_agent.speed = _baseSpeed * Random.Range(1 - _speedRangePercent, 1 + _speedRangePercent);

        foreach (GameObject obj in _optionalObjects)
            obj.SetActive(Random.Range(0f, 100f) < _optionalObjectsShowChance);
    }
}
