using System;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobManager : Gamelogic.Extensions.Singleton<ThreadJobManager>
{
	[SerializeField]
	List<ThreadJob> jobs = new List<ThreadJob>();

	public static ThreadJob AddJob(string jobName, Action threadFunc, Action mainFunc, bool bStartThread)
	{
		return AddJob(new ThreadJob(jobName, threadFunc, mainFunc), bStartThread);
	}

	public static ThreadJob AddJob(ThreadJob job, bool bStartThread)
	{
		ThreadJobManager tjm = ThreadJobManager.Instance;
		if (tjm == null) return null;

		tjm.jobs.Add(job);

		if (bStartThread) job.StartThread();

		return job;
	}

	// Use this for initialization
	void Start()
	{
		jobs.ForEach((job) => job.StartThread());
	}

	// Update is called once per frame
	void Update()
	{
		jobs.ForEach((job) => job.Update());
	}

	void OnApplicationPause()
	{
		jobs.ForEach((job) => job.state = ThreadJob.State.Paused);
	}

	void OnDestroy()
	{
		jobs.ForEach((job) => job.state = ThreadJob.State.Abort);
	}
}
