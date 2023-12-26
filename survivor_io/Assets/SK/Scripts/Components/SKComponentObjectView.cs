using JHT.Scripts.Common.PerformanceExtension;
using Spine.Unity;
using UnityEngine;

namespace SK
{
	public enum SKViewAnimNameType
	{
		None,
		Appear,
		Move,
		Death,
		Attack_A,
	}
	
    public class SKComponentObjectView : SKComponentBase
    {
	    #region Cache

	    [SerializeField] private Animator animator;
	    public Animator Animator
	    {
		    get
		    {
			    if (false == animator)
			    {
				    TryGetComponent(out animator);
			    }
			    
			    return animator;
		    }
	    }
	    
	    [SerializeField] private SkeletonAnimation skeletonAnimation;
	    public SkeletonAnimation SkeletonAnimation
	    {
		    get
		    {
			    if (false == skeletonAnimation)
			    {
				    TryGetComponent(out skeletonAnimation);
			    }
			    
			    return skeletonAnimation;
		    }
	    }
	    
	    [SerializeField] private SpriteRenderer spriteRenderer;
	    public SpriteRenderer SpriteRenderer
	    {
		    get
		    {
			    if (false == spriteRenderer)
			    {
				    TryGetComponent(out spriteRenderer);
			    }
			    
			    return spriteRenderer;
		    }
	    }

	    protected override void Awake()
	    {
		    base.Awake();

		    if (Animator)
		    {
			    foreach (var clip in Animator.runtimeAnimatorController.animationClips)
			    {
				    var animationStartEvent = new AnimationEvent();
				    animationStartEvent.time = 0;
				    animationStartEvent.functionName = nameof(AnimationStartHandler);
				    animationStartEvent.stringParameter = clip.name;
            
				    var animationEndEvent = new AnimationEvent();
				    animationEndEvent.time = clip.length;
				    animationEndEvent.functionName = nameof(AnimationCompleteHandler);
				    animationEndEvent.stringParameter = clip.name;
            
				    clip.AddEvent(animationStartEvent);
				    clip.AddEvent(animationEndEvent);
			    }
		    }
	    }
	    
	    public void AnimationStartHandler(string name)
	    {
		    
	    }
	    public void AnimationCompleteHandler(string name)
	    {
		    
	    }
	    
	    protected override void Reset()
	    {
		    base.Reset();
		    
		    if(false == animator)
		    {
			    TryGetComponent(out animator);
		    }
		    
		    if(false == skeletonAnimation)
		    {
			    TryGetComponent(out skeletonAnimation);
		    }
		    
		    if(false == spriteRenderer)
		    {
			    TryGetComponent(out spriteRenderer);
		    }
	    }

	    #endregion

	    [SerializeField] private bool defaultIsLeft;
	    
	    public bool IsLeftVIew
	    {
		    get;
		    private set;
	    }

	    private bool _isFirstSet; 

	    private float _elapsedValue;

	    public void LookAtLeftByDirectionValue(float directionValue)
	    {
		    if (directionValue == 0)
		    {
			    return;
		    }

		    if (_elapsedValue * directionValue < 0)
		    {
			    _elapsedValue = 0;
		    }
			    
		    _elapsedValue += directionValue;
		    if (Mathf.Abs(_elapsedValue) < 0.1f)
		    {
			    return;
		    }

		    var isLeftView = _elapsedValue < 0;
		    if (false == _isFirstSet && IsLeftVIew == isLeftView)
		    {
			    return;
		    }
		    
		    LookAtLeft(isLeftView);
	    }

	    public void LookAtLeft(bool isLeftView)
	    {
		    _isFirstSet = false;
		    IsLeftVIew = isLeftView;
		    
		    var newFlipX = false == IsLeftVIew;
		    if (false == defaultIsLeft)
		    {
			    newFlipX = !newFlipX;
		    }

		    if (SpriteRenderer)
		    {
			    if (SpriteRenderer.flipX != newFlipX)
			    {
				    SpriteRenderer.flipX = newFlipX;
			    }
		    }
		    else if (SkeletonAnimation)
		    {
			    var flipX = SkeletonAnimation.skeleton.ScaleX < 0.0f;
			    if (flipX != newFlipX)
			    {
				    SkeletonAnimation.skeleton.ScaleX = newFlipX ? -1.0f : 1.0f;   
			    }
		    }
	    }

	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();

		    _elapsedValue = 0;
		    _isFirstSet = true;
		    LookAtLeft(defaultIsLeft);
		    
		    SKGameManager.Instance.OnPause += (OnPause);
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    SKGameManager.Instance.OnPause -= (OnPause);
		    
		    base.OnSKObjectDestroy();
	    }

	    private void OnPause(bool pause)
	    {
		    if (Animator)
		    {
			    Animator.speed = pause ? 0f : 1f;
		    }

		    if (SkeletonAnimation)
		    {
			    SkeletonAnimation.timeScale = pause ? 0f : 1f;
		    }
	    }

	    public void SetLoop(bool loop)
	    {
		    if (SkeletonAnimation)
		    {
			    SkeletonAnimation.loop = loop;
		    }
	    }

	    public float PlayAnim(SKViewAnimNameType animName)
	    {
		    return PlayAnim(animName.ToStringCached());
	    }
	    
	    public float PlayAnim(SKViewAnimNameType animName, SKViewAnimNameType nextAnimName)
	    {
		    if (nextAnimName == SKViewAnimNameType.None)
		    {
			    return PlayAnim(animName.ToStringCached());
		    }
		    
		    return PlayAnim(animName.ToStringCached(), nextAnimName.ToStringCached());
	    }
	    
	    public float PlayAnim(string animName)
	    {
		    if (SkeletonAnimation)
		    {
			    var trackEntry = SkeletonAnimation.AnimationState.SetAnimation(0, animName, true);
			    return trackEntry.AnimationEnd;
		    }

		    if (Animator)
		    {
			    Animator.Play(animName);
			    Animator.Update(0);
			    return GetCurrentAnimationClipLength(animator);
		    }

		    return 0;
	    }
	    
	    public float PlayAnim(string animName, string nextAnimName)
	    {
		    if (SkeletonAnimation)
		    {
			    var trackEntry = SkeletonAnimation.AnimationState.SetAnimation(0, animName, false);
			    SkeletonAnimation.AnimationState.AddAnimation(0, nextAnimName, true, 0);
			    return trackEntry.AnimationEnd;
		    }

		    if (Animator)
		    {
			    Animator.Play(animName);
			    Animator.Update(0);
			    return GetCurrentAnimationClipLength(animator);
		    }

		    return 0;
	    }
	    
	    public static float GetCurrentAnimationClipLength(Animator animator, int iLayerIndex = 0)
	    {
		    var list = animator.GetCurrentAnimatorClipInfo(iLayerIndex);
		    if (null == list)
		    {
			    return 0;
		    }

		    if (0 == list.Length)
		    {
			    return 0;
		    }

		    if (null == list[0].clip)
		    {
			    return 0;
		    }

		    return list[0].clip.length;
	    }
    }
}
