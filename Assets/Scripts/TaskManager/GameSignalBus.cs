using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameSignalBus : MonoBehaviour
{
    public static GameSignalBus Instance { get; private set; }

    public event Action<string> OnSignal;

    private readonly HashSet<string> _latched = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Signal(string id, bool latch = false)
    {
        if (string.IsNullOrWhiteSpace(id)) return;

        if (latch)
            _latched.Add(id);

        OnSignal?.Invoke(id);
    }

    public void Signal(string id)
    {
        Signal(id, false);
    }

    public bool ConsumeLatched(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        return _latched.Remove(id);
    }
}