using UnityEngine;

// horriblest coed
public class SimpleTeleport : MonoBehaviour
{
	public Transform target;

	void OnTriggerEnter(Collider other)
	{
		if (other.transform != Player.instance.transform)
			return;
			
		Player.instance.TeleportTo(transform, target);
	}

#if UNITY_EDITOR
	void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.position, target.transform.position);
    }
#endif
}
