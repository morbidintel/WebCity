using System;
using System.Threading;
using UnityEngine;

/// <summary>
/// A class that will run a threaded Action and optionally an Action on the main thread
/// </summary>
[Serializable]
public class ThreadJob
{
	public enum State
	{
		Running,
		RunningNoUpdate,
		Paused,
		Abort
	}

	Thread thread = null;
	Action _threadFunc = null, _mainFunc = null;
	public volatile State state = State.Paused;

	public string jobName = "ThreadJob thread"; // default name
	public Action mainFunc
	{
		get { return _mainFunc; }
		set { _mainFunc = value; }
	}
	public Action threadFunc
	{
		get { return _threadFunc; }
		set { if (thread == null || !thread.IsAlive) _threadFunc = value; }
	}

	public ThreadJob() { }

	public ThreadJob(string jobName, Action threadFunc, Action mainFunc = null)
	{
		this.jobName = jobName;
		_threadFunc = threadFunc;
		_mainFunc = mainFunc;
	}

	// Call me to initialize
	public void StartThread()
	{
		if (_threadFunc == null || state == State.Running)
			return;

		state = State.Running;

		thread = new Thread(() =>
		{
			try
			{
				while (_threadFunc != null)
				{
					_threadFunc();

					switch (state)
					{
						case State.Paused:
							try
							{
								Debug.Log("Thread " + thread.Name + " paused");
								Thread.Sleep(Timeout.Infinite);
							}
							catch (ThreadInterruptedException)
							{
								Debug.Log("Thread " + thread.Name + " resumed");
							}
							break;
						case State.Abort:
							return;
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message + '\n' + e.StackTrace);
			}
		});

		thread.Name = jobName;
		thread.IsBackground = true;
		thread.Start();
	}

	public void Update()
	{
		if (_mainFunc == null || state == State.RunningNoUpdate) return;

		_mainFunc();
	}
}