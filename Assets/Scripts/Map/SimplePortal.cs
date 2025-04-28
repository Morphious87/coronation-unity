using UnityEngine;

// horriblest coed
public class SimplePortal : MonoBehaviour
{
    public SimplePortal targetPortal;

    public new BoxCollider collider;
	public BoxCollider readyingCollider;
	public bool skipReadyCheck;

	public bool requireAngle;
	public float requiredAngle;

	bool wasReadyToTeleport;

    void Awake()
    {
        GetComponentInChildren<MeshRenderer>().enabled = false;
    }

    void LateUpdate()
    {
        var player = Player.instance;

		var isReadyToTeleport = wasReadyToTeleport || skipReadyCheck;
		wasReadyToTeleport = IsInsideBounds(player.transform.position, readyingCollider);

		if (requireAngle)
		{
			var angleDist = Mathf.Abs(Mathf.DeltaAngle(requiredAngle, player.lookRotation.y));
			if (angleDist > 45)
				wasReadyToTeleport = false;
		}

		if (isReadyToTeleport && IsInsideBounds(player.transform.position, collider))
        {
            player.controller.enabled = false;

            player.transform.position = RemapPoint(player.transform.position, transform, targetPortal.transform);

            var angleDiff = targetPortal.transform.eulerAngles.y - transform.eulerAngles.y;
            player.lookRotation.y += angleDiff;
		    player.transform.localEulerAngles = player.lookRotation.y * Vector3.up;

            player.velocity = Quaternion.AngleAxis(angleDiff, Vector3.up) * player.velocity;
            player.controller.enabled = true;

#if UNITY_EDITOR
			Debug.Log("teleport!! " + Time.frameCount + $" ({Time.timeSinceLevelLoad})");
#endif
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.position, targetPortal.transform.position);
    }
#endif

    static Vector3 RemapDirection(Vector3 position, Transform from, Transform to) => to.TransformDirection(from.InverseTransformDirection(position));
    static Vector3 RemapPoint(Vector3 position, Transform from, Transform to) => to.TransformPoint(from.InverseTransformPoint(position));

	static bool IsInsideBounds(Vector3 worldPos, BoxCollider box)
	{
	    Vector3 localPos = box.transform.InverseTransformPoint(worldPos);
	    Vector3 delta = localPos - box.center + box.size * 0.5f;
	    return Vector3.Max(Vector3.zero, delta)==Vector3.Min(delta, box.size);
	}
}
