using UnityEngine;

public class Collectible : MonoBehaviour
{
	public Trigger[] triggers;

	void OnTriggerEnter(Collider other)
	{
		Collect();
	}

	void Start()
	{
		if (Persistence.panelData.collectibles.Contains(transform.name))
			Collect();	
	}

	public void Collect()
	{
		AudioManager.Play(SoundType.Pickup);

		foreach(var trigger in triggers)
			trigger?.Interact(true);

		gameObject.SetActive(false);

		Persistence.panelData.collectibles.Add(transform.name);
		Persistence.panelData.Save();
	}

#if UNITY_EDITOR
    void OnDrawGizmosSelected() 
	{
		Gizmos.color = Color.magenta;
		foreach(var trigger in triggers)
		{
			if (trigger == null)
				continue;

			Gizmos.DrawLine(transform.position, trigger.transform.position);
		}
	}
#endif
}
