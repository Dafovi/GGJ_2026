using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class StoryFlowManager : MonoBehaviour
{
    [SerializeField]
    private List<FlowStep> _steps;

    private int _index;
    private Coroutine _runCoroutine;

    private bool _waitingSignal;

    private void OnDestroy()
    {
        if (GameSignalBus.Instance != null)
            GameSignalBus.Instance.OnSignal -= OnSignal;
    }

    private void OnEnable()
    {
        GameSignalBus.Instance.OnSignal += OnSignal;

        if (_runCoroutine == null)
            _runCoroutine = StartCoroutine(Run());
    }

    private void OnDisable()
    {
        if (_runCoroutine != null)
        {
            StopCoroutine(_runCoroutine);
            _runCoroutine = null;
        }
    }

    private IEnumerator Run()
    {
        _index = Mathf.Clamp(_index, 0, _steps.Count);

        while (_index < _steps.Count)
        {
            FlowStep step = _steps[_index];
            if (step == null)
            {
                _index++;
                continue;
            }

            if (step.DelayBeforePlay > 0f)
                yield return new WaitForSeconds(step.DelayBeforePlay);

            Play(step);

            if (step.WaitMode == FlowWaitMode.Seconds)
            {
                if (step.Seconds > 0f)
                    yield return new WaitForSeconds(step.Seconds);
            }
            else if (step.WaitMode == FlowWaitMode.Signal)
            {
                if (!string.IsNullOrWhiteSpace(step.SignalId) && GameSignalBus.Instance != null)
                {
                    if (!GameSignalBus.Instance.ConsumeLatched(step.SignalId))
                    {
                        _waitingSignal = true;
                        while (_waitingSignal)
                            if(step.Source != null && !step.Source.isPlaying)
                            DialogueManager.Instance.HideDialogue();

                        yield return null;
                    }
                }
            }
            else
            {
                while (step.Source.isPlaying)
                    yield return null;
            }

            _index++;
        }

        DialogueManager.Instance.HideDialogue();
        _runCoroutine = null;
    }

    private void Play(FlowStep step)
    {
        if (!string.IsNullOrWhiteSpace(step.Subtitle)) DialogueManager.Instance.ShowDialogue(step.Subtitle);
        else DialogueManager.Instance.HideDialogue();

        if (step.Clip == null) return;
        if(step.Source != null) step.Source.PlayOneShot(step.Clip);
    }

    private void OnSignal(string id)
    {
        if (!_waitingSignal) return;

        FlowStep step = (_index >= 0 && _index < _steps.Count) ? _steps[_index] : null;
        if (step == null) return;

        if (!string.IsNullOrWhiteSpace(step.SignalId) && step.SignalId == id)
            _waitingSignal = false;
    }
}