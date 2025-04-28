using UnityEngine;

public class SpinningObject : MonoBehaviour
{
	public float speed = 30f;
	public float bobSpeed = 0f;
	public float bobAmount = 0f;
	public bool faceCamera = false;

	Vector3 basePosition;
	Vector3 baseRotation;

	void Awake()
	{
		basePosition = transform.localPosition;
		baseRotation = transform.localEulerAngles;
	}

	void LateUpdate()
	{
		if (faceCamera)
		{
			transform.forward = Camera.main.transform.forward;
		}
		else
			transform.eulerAngles += Vector3.down * Time.deltaTime * speed;

		if (bobSpeed * bobAmount == 0)
			return;

		var randomness = GetInstanceID() % 214.23f * 127.6674f;
		transform.localPosition = basePosition + Vector3.up * (bobAmount * Mathf.Sin(bobSpeed * Time.time * Mathf.PI + randomness));
	}
}
