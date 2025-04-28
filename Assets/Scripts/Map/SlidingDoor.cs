using UnityEngine;
using DG.Tweening;

public class SlidingDoor : Trigger
{
	public Ease ease = Ease.Linear;
	public Vector3 offset = new Vector3(0, -3, 0);
	public float duration = .5f;
	public bool disappear;	
	Tween tween;

	Vector3 fromPos, toPos;

	void Awake()
	{
		const float extraOffset = 0.01f;
		var newOffset = offset + new Vector3(offset.x == 0 ? 0 : Mathf.Sign(offset.x) * extraOffset, offset.y == 0 ? 0 : Mathf.Sign(offset.y) * extraOffset, offset.z == 0 ? 0 : Mathf.Sign(offset.z) * extraOffset);

		fromPos = transform.localPosition;
		toPos = transform.localPosition + newOffset;
	}

	public override void Interact(bool activate)
	{
		tween?.Kill();

		var dur = Time.timeSinceLevelLoad < 0.1f ? 0f : duration;
		tween = transform.DOLocalMove(activate ? toPos : fromPos, dur).SetEase(ease);

		if (disappear)
		{
			if (activate)
				tween.OnComplete(() => gameObject.SetActive(false));
			else
				gameObject.SetActive(true);
		}

		tween.Done();
	}
}
