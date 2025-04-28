using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RingLayout : MonoBehaviour
{
	public float radius;

#if UNITY_EDITOR
	void OnValidate()
	{
		var children = transform.childCount;
		var angleDelta = Mathf.PI * 2f / children;

		var i = 0;
		foreach(Transform child in transform)
		{
			i++;
			var angle = angleDelta * i;
			child.localPosition = new Vector3(Mathf.Sin(angle) * radius, child.localPosition.y, Mathf.Cos(angle) * radius);
		}	
	}
#endif
}
