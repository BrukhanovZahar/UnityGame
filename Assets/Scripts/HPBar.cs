using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
	[SerializeField] private Slider _slider;
	[SerializeField] private Life _life;

	private void Start()
	{
		_slider.maxValue = _life.MaxHP;
		_slider.value = _life.MaxHP;
		_life.OnChangeValue.AddListener(SetValue);
	}

	private void SetValue(float value)
	{
		_slider.maxValue = _life.MaxHP;
		_slider.value = value;
	}
}
