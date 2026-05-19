using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
	private void Start()
	{
		Time.timeScale = 0;
		GlobalContext.Pause = true;
		GlobalContext.End = true;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void ReStart()
	{
		Time.timeScale = 1;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}
