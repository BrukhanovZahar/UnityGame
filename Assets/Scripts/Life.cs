using UnityEngine;
using UnityEngine.Events;

public class Life : MonoBehaviour
{
	[SerializeField] private float _maxHP;
	[SerializeField] private UnityEvent<float> _onHit;
	[SerializeField] private UnityEvent<float> _onChangeValue;
	[SerializeField] private UnityEvent _onDie;
	private float _currentHP;

	public UnityEvent<float> OnHit => _onHit;
	public UnityEvent<float> OnChangeValue => _onChangeValue;
	public UnityEvent OnDie => _onDie;
	public float MaxHP => _maxHP;

	private void Start()
	{
		_currentHP = _maxHP;
	}

	public void TakeDamage(float damage)
	{
		_currentHP -= damage;
		_onHit.Invoke(_currentHP);
		_onChangeValue.Invoke(_currentHP);
		if (_currentHP <= 0)
		{
			_onDie.Invoke();
		}
	}

	public void SetMaxHP(float value)
	{
		_maxHP = value;
		_currentHP = value;
		_onChangeValue.Invoke(_currentHP);
	}

	public void FullHeal()
	{
		_currentHP = _maxHP;
		_onChangeValue.Invoke(_currentHP);
	}

	public void SetFullHP()
	{
		_currentHP = _maxHP;

		_onChangeValue.Invoke(_currentHP);
	}
}
