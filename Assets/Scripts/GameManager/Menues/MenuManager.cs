using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class MenuManager : MonoBehaviour {

	[SerializeField][ReadOnly]
	GameObject activeMenu;

	[SerializeField] GameObject mainMenu;
	[SerializeField] GameObject gameMenu;
	[SerializeField] GameObject optionsMenu;
	[SerializeField] GameObject playMenu;

	Stack<GameObject> menuStack;

	void Awake() {
		menuStack = new Stack<GameObject>();
		activeMenu = null;

		mainMenu.SetActive(false);
		gameMenu.SetActive(false);
		optionsMenu.SetActive(false);
		playMenu.SetActive(false);

		MainMenu();
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Escape)){
			if (activeMenu == null) {
				OpenMenu(gameMenu);
				Time.timeScale = 0f;
			} else {
				Back();
			}
		}
	}

	void OpenMenu(GameObject menu)
	{
		if (activeMenu != null) {
			activeMenu.SetActive(false);
			menuStack.Push(activeMenu);
		}

		SetActive(menu);
	}

	void SetActive(GameObject menu)
	{
		menu.SetActive(true);
		activeMenu = menu;
		activeMenu.GetComponent<SelectableHighlighter>().Highlight();
	}

	public void CloseAllMenues()
	{
		menuStack.Clear();
		activeMenu.SetActive(false);
		activeMenu = null;
	}

	public void Back()
	{
		if (menuStack.Count > 0) {
			// Go back a menu
			GameObject menu;
			menu = menuStack.Pop();
			activeMenu.SetActive(false);
			SetActive(menu);
		} else {
			// Don't do anything, if we currently are in the title menu / no game is currently running
			if (activeMenu == mainMenu) {
				return;
			}

			// Resume game
			activeMenu.SetActive(false);
			activeMenu = null;
			Time.timeScale = 1f;
		}
	}

	public void OptionsMenu()
	{
		OpenMenu(optionsMenu);
	}

	public void MainMenu()
	{
		OpenMenu(mainMenu);
	}

	public void PlayMenu()
	{
		OpenMenu(playMenu);
	}

	public void PlaySave(int index)
	{
		GameManager.Instance.PlaySave(index);
	}

	public void StopGame()
	{
		GameManager.Instance.StopGame();
	}

	public void CloseGame()
	{
		Application.Quit();
	}
}
