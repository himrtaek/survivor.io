using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using SK;
using UnityEngine;

public class SKComponentSpriteAnimation : SKComponentBase
{
    public bool loop = true;
    public float timeScale = 1.0f;
    public SpriteRenderer spriteRenderer;
    [SerializeField] private SerializedDictionary<SKViewAnimNameType, List<Sprite>> spriteDic;
    [SerializeField] float frameTime = 0.1f;
    
    List<Sprite> _curSpriteList = new List<Sprite>();
    int _curSpriteIndex = 0;
    float _elapsedTime = 0;
    SKViewAnimNameType _curAnimName = SKViewAnimNameType.None;
    SKViewAnimNameType _nextAnimName = SKViewAnimNameType.None;
	    
    protected override void Reset()
    {
        base.Reset();
		    
        if(false == spriteRenderer)
        {
            TryGetComponent(out spriteRenderer);
        }
    }

    // Update is called once per frame
    void Update()
    {
        _elapsedTime += Time.deltaTime * timeScale;
        
        if (_elapsedTime < frameTime)
        {
            return;
        }
        
        _elapsedTime -= frameTime;
        _curSpriteIndex++;
        if (null == _curSpriteList || 0 == _curSpriteList.Count)
        {
            if (SKViewAnimNameType.None != _nextAnimName)
            {
                PlayAnim(_nextAnimName);
                _nextAnimName = SKViewAnimNameType.None;
            }
            
            return;
        }

        if (_curSpriteList.Count <= _curSpriteIndex)
        {
            if (SKViewAnimNameType.None != _nextAnimName)
            {
                PlayAnim(_nextAnimName);
                _nextAnimName = SKViewAnimNameType.None;
                return;
            }

            if (loop)
            {
                _curSpriteIndex = 0;   
            }
            else
            {
                _curSpriteIndex = _curSpriteList.Count - 1;
            }
        }
        
        spriteRenderer.sprite = _curSpriteList[_curSpriteIndex];
    }

    public float PlayAnim(string animName)
    {
        if (Enum.TryParse(animName, out SKViewAnimNameType animType))
        {
            PlayAnim(animType);
            return GetCurrentAnimationClipLength();
        }
        else
        {
            Debug.LogError($"Failed to parse {animName} to SKViewAnimNameType");
            return 0;
        }
    }
    
    public float PlayAnim(SKViewAnimNameType animName)
    {
        _curAnimName = animName;
        _curSpriteList = null;
        _curSpriteIndex = 0;
        _elapsedTime = 0;
        
        if (false == spriteDic.ContainsKey(animName))
        {
            return 0;
        }
        
        _curSpriteList = spriteDic[animName];
        return GetCurrentAnimationClipLength();
    }

    public float PlayAnim(string animName, string nextAnim)
    {
        if (Enum.TryParse(animName, out SKViewAnimNameType animType) && Enum.TryParse(nextAnim, out SKViewAnimNameType nextAnimType))
        {
            return PlayAnim(animType, nextAnimType);
        }
        else
        {
            Debug.LogError($"Failed to parse {animName} or {nextAnim} to SKViewAnimNameType");
            return 0;
        }
    }
    
    public float PlayAnim(SKViewAnimNameType animName, SKViewAnimNameType nextAnim)
    {
        PlayAnim(animName);
        _nextAnimName = nextAnim;
        return GetCurrentAnimationClipLength();
    }

    public float GetCurrentAnimationClipLength()
    {
        if (null == _curSpriteList || 0 == _curSpriteList.Count)
        {
            return 0;
        }
        
        return _curSpriteList.Count * frameTime / timeScale;
    }
}
