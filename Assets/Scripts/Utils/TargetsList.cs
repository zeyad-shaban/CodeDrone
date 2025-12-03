using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

class TargetsList
{

    private float mergingThresh;
    private float minConfirmationScore;
    private float minLastTimeDetected; // seconds

    private float minClearingInterval;
    private float lastClearingTime;

    private List<Target> targets = new();
    private List<Target> archivedTargets = new();
    public float Count { get => targets.Count; }


    public TargetsList(float mergingThresh = 5, float minLastTimeDetected = 2, float minConfirmationScore = 10, float minClearingInterval = 0.5f)
    {
        this.mergingThresh = mergingThresh;
        this.minLastTimeDetected = minLastTimeDetected;
        this.minClearingInterval = minClearingInterval;
        this.minConfirmationScore = minConfirmationScore;
    }

    public void Add(Target newTarget, float conf)

    {
        // Check if exiting already
        foreach (Target target in targets)
        {
            if (Vector3.Distance(target.worldPos, newTarget.worldPos) <= mergingThresh)
            {
                target.IncrementConfirmationScore(conf, newTarget.worldPos);
                return;
            }
        }

        // Check if archived
        foreach (Target target in archivedTargets)
        {
            if (Vector3.Distance(target.worldPos, newTarget.worldPos) <= mergingThresh)
            {
                target.IncrementConfirmationScore(conf, newTarget.worldPos);
                return;
            }
        }

        // Not existing, Add
        targets.Add(newTarget);
    }

    public void ClearOld()
    {
        if (Time.time - lastClearingTime < minClearingInterval)
            return;
        lastClearingTime = Time.time;

        // Debug.Log(targets);

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            var target = targets[i];
            if (Time.time - target.lastDetectedTime >= minLastTimeDetected)
            {
                targets.RemoveAt(i);
            }
        }
    }

    public List<Target> GetConfirmedTargets()
    {
        List<Target> confirmedTargets = new();
        foreach (Target target in targets)
            if (target.GetConfirmationScore() >= minConfirmationScore)
                confirmedTargets.Add(target);

        return confirmedTargets;
    }

    public Target ArchivePopConfirmed()
    {
        List<Target> confirmedTargets = GetConfirmedTargets();
        if (confirmedTargets.Count <= 0)
            Debug.Log($"ConfirmedTargets Count is {confirmedTargets.Count}");

        Target target = confirmedTargets[0];
        confirmedTargets.RemoveAt(0);

        archivedTargets.Add(target);
        return target;
    }

    public List<Target> GetArchivedTargets()
    {
        return archivedTargets;
    }

    public Target this[int idx]
    {
        get => targets[idx];
        set => targets[idx] = value;
    }
}