using System;
using Unity.VisualScripting;
using UnityEngine;

public enum FlowWaitMode
{
    AudioEnded,
    Seconds,
    Signal
}

[Serializable]
public class FlowStep
{
    [SerializeField]
    private AudioSource _source;

    [SerializeField]
    private AudioClip _clip;

    [SerializeField]
    private string _subtitle;

    [SerializeField]
    private float _delayBeforePlay;

    [SerializeField]
    private FlowWaitMode _waitMode = FlowWaitMode.AudioEnded;

    [SerializeField]
    private float _seconds;

    [SerializeField]
    private string _signalId;

    public AudioSource Source => _source;
    public AudioClip Clip => _clip;
    public string Subtitle => _subtitle;
    public float DelayBeforePlay => _delayBeforePlay;
    public FlowWaitMode WaitMode => _waitMode;
    public float Seconds => _seconds;
    public string SignalId => _signalId;
}