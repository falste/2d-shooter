using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SelectableHighlighter : MonoBehaviour {
	public Selectable button;

	public void Highlight()
	{
		//Debug.Log(button.name+ ", "+button.isActiveAndEnabled);
		button.Select();
	}
}
