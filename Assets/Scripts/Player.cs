using UnityEngine;
using System;

public class Player : MonoBehaviour
{
	static Player _instance; 
	public static Player instance => _instance ?? (_instance = FindAnyObjectByType<Player>());

	public new Camera camera;
	public Transform cameraTransform;
	public CharacterController controller;

	public Transform CBALocation;

	// game config
	[SerializeField] float acceleration;
	[SerializeField] float deceleration;
	[SerializeField] float walkSpeed;
	[SerializeField] float sprintMultiplier;
	[SerializeField] float sensitivity;
	[SerializeField] float jumpHeight;
	[SerializeField] float gravity;
	[SerializeField] float mouseClamp;

	[NonSerialized] public Vector3 velocity;
	[NonSerialized] public Vector2 lookRotation;

	[NonSerialized] public bool lockInput;

	[NonSerialized] Vector3 entryPos;

	bool bunnyHopPrevention = false;

	void Update()
	{
		if (PauseMenu.paused)
			return;

		if (lockInput)
			return;

		var sprinting = Input.GetKey(KeyCode.LeftShift) ^ Persistence.settings.autoRun;

		var direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
		direction = transform.TransformDirection(direction);

		var playerSpeed = Mathf.Lerp(0.25f, 1.75f, Persistence.settings.playerSpeed);

		velocity += direction * acceleration * playerSpeed * Time.deltaTime;

		var yVelocity = velocity.y;
		velocity.y = 0;

		velocity = Vector3.MoveTowards(velocity, Vector3.zero, deceleration * playerSpeed * Time.deltaTime);
		velocity = Vector3.ClampMagnitude(velocity, (sprinting ? walkSpeed * sprintMultiplier : walkSpeed) * playerSpeed);

		if (!controller.isGrounded)
			velocity.y = yVelocity - gravity * Time.deltaTime;
		else if (velocity.y < 0)
			velocity.y = 0;

		var mouseDirection = new Vector2((Persistence.settings.invertY ? -1f : 1f) * -Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
		var mouseDelta = mouseDirection * sensitivity * Mathf.Lerp(0.25f, 1.75f, Persistence.settings.sensitivity);

		lookRotation += mouseDelta;
		lookRotation = new Vector2(Mathf.Clamp(lookRotation.x, -mouseClamp, +mouseClamp), lookRotation.y);

		// fix mouse look
		if (Time.timeSinceLevelLoad < 0.5f)
			lookRotation = Vector3.zero;

		cameraTransform.localEulerAngles = lookRotation.x * Vector3.right;
		transform.localEulerAngles = lookRotation.y * Vector3.up;

		var jumping = Input.GetAxisRaw("Jump") == 1;
		if (jumping && !bunnyHopPrevention && controller.isGrounded && velocity.y < 0.1)
		{
			velocity += Vector3.up * jumpHeight;
			bunnyHopPrevention = true;
		}

		if (!jumping)
			bunnyHopPrevention = false;

		controller.Move(velocity * Time.deltaTime * 60f);
	}

	void OnControllerColliderHit(ControllerColliderHit hit)
	{	
		if (hit.collider.isTrigger)
			return;

		if (controller.collisionFlags.HasFlag(CollisionFlags.Sides) || controller.collisionFlags.HasFlag(CollisionFlags.Above))
		{	
			if (Vector3.Dot( hit.normal, velocity ) < 0)
				velocity -= hit.normal * Vector3.Dot( hit.normal, velocity );
		}
	}

	void Awake()
	{
		entryPos = transform.position;
	}

	public void GoToEntry()
	{
		controller.enabled = false;

		transform.position = entryPos;
		velocity = Vector3.zero;
		lookRotation = Vector3.zero;

		controller.enabled = true;
	}

	public void CBA()
	{
		controller.enabled = false;

		transform.position = CBALocation.position;
		velocity = Vector3.zero;
		lookRotation = Vector3.up * 90f;

		controller.enabled = true;
	}

	public void TeleportTo(Transform from, Transform to)
	{
		controller.enabled = false;

		transform.position = to.TransformPoint(from.InverseTransformPoint(transform.position));

		controller.enabled = true;
	}
}
