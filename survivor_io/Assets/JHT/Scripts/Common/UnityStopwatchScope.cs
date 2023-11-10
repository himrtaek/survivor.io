using System;
using System.Diagnostics;
using Cysharp.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace JHT.Scripts.Common
{
    /// <summary>
    /// 유니티 스탑워치 : Begin-End를 잘 지킬 수 있도록 using() {} 사용
    /// </summary>
    public readonly struct UnityStopwatchScope : IDisposable
    {
#if !REAL
	    private readonly bool _init;
	    private readonly string _message;
	    private readonly double _startTime;
	    private readonly double _logTIme;
	    private readonly double _warningTime;
	    private readonly double _errorTIme;
#endif
	    
        public UnityStopwatchScope(string message = "", double logTime = 0.0d, double warningTime = double.MinValue, double errorTIme = double.MinValue)
        {
#if !REAL
	        _init = true;
	        _message = message;
	        _startTime = Time.realtimeSinceStartupAsDouble;
	        _logTIme = logTime;
	        _warningTime = warningTime;
	        _errorTIme = errorTIme;
#endif
        }

        public void Dispose()
        {
#if !REAL
	        if (_init == false)
	        {
		        Debug.LogError($"[UnityStopwatchScope] 설정 값을 찾을 수 없습니다. 파라미터를 사용한 생성자를 이용해주세요");
		        return;
	        }

	        var endTime = Time.realtimeSinceStartupAsDouble;
	        var elapsedTime = (endTime - _startTime) * 1000;

	        var stackLog = GetStackLog();
	        
	        if (0 <= _errorTIme && _errorTIme < elapsedTime)
	        {
		        Debug.LogError($"{_message} [ElapsedTime:{elapsedTime:##,##0.00} ms] {stackLog}");
	        }
	        else if (0 <= _warningTime && _warningTime < elapsedTime)
	        {
		        Debug.LogWarning($"{_message} [ElapsedTime:{elapsedTime:##,##0.00} ms] {stackLog}");   
	        }
	        else if (0 <= _logTIme && _logTIme < elapsedTime)
	        {
		        Debug.Log($"{_message} [ElapsedTime:{elapsedTime:##,##0.00} ms] {stackLog}");
	        }
#endif
        }

        public static string GetStackLog(int index = 2)
        {
#if !REAL && DEBUG && UNITY_EDITOR
	        var stackTrace = new StackTrace(true);
	        var frame = stackTrace.GetFrame(index);
	        var fileName = System.IO.Path.GetFileName(frame?.GetFileName() ?? "");
	        var fileLineNo = frame?.GetFileLineNumber() ?? 0;
	        var methodName = frame?.GetMethod()?.Name ?? "";
	        
	        if (index + 3 <= stackTrace.FrameCount 
	            && methodName == "MoveNext()")
	        {
		        frame = stackTrace.GetFrame(index + 2);
		        methodName = frame?.GetMethod()?.Name ?? "";
	        }

	        return ZString.Format("in \"{2}\" (at {0}:{1})", fileName, fileLineNo, methodName);
#else
			return "";
#endif
	        
        }
    }
}
