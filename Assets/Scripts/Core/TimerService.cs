using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single Update() loop that manages ALL delayed callbacks in one place.
/// Eliminates per-job coroutines and reduces GC pressure.
/// </summary>
public class TimerService : MonoBehaviour
{
    public static TimerService Instance { get; private set; }

    struct Timer { public float remaining; public Action callback; }

    private readonly List<Timer> timers = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        for (int i = timers.Count - 1; i >= 0; i--)
        {
            var t = timers[i];
            t.remaining -= dt;
            if (t.remaining <= 0f)
            {
                t.callback?.Invoke();
                timers.RemoveAt(i);
            }
            else
            {
                timers[i] = t;
            }
        }
    }

    /// <summary>Schedule a one-shot callback after delaySeconds.</summary>
    public void Schedule(float delaySeconds, Action onComplete)
    {
        timers.Add(new Timer { remaining = delaySeconds, callback = onComplete });
    }
}
