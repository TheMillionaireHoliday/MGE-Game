using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorStateObserver : NetworkBehaviour
{
    private Animator animator;

    private int currentStateHash = 0;
    private float previousNormalizedTime = 0;

    // Events that other scripts can subscribe to
    public System.Action<string> OnStateEntered;
    public System.Action<string> OnStateExited;
    public System.Action<string> OnStateRestarted;

    // Track which states we care about
    [SerializeField] private string[] watchedStates = new string[] { "Attack", "Shot", "Reload Loop", "Reload End" };
    private HashSet<int> watchedHashes = new HashSet<int>();

    void Start()
    {
        animator = GetComponent<Animator>();

        // Convert state names to hashes for faster comparison
        foreach (string stateName in watchedStates)
        {
            watchedHashes.Add(Animator.StringToHash(stateName));
        }
    }

    void Update()
    {
        if (!IsOwner)
            return;

        bool hasDetectedSelfTransition = false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        int newStateHash = stateInfo.shortNameHash;

        float currentNormalizedTime = stateInfo.normalizedTime % 1f;

        // --- STATE CHANGE DETECTION ---
        if (newStateHash != currentStateHash)
        {
            hasDetectedSelfTransition = false;
            previousNormalizedTime = 0;

            if (watchedHashes.Contains(currentStateHash))
            {
                string exitedName = GetStateName(currentStateHash);
                OnStateExited?.Invoke(exitedName);
                //Debug.Log($"Exited: {exitedName}");
            }

            // If we're entering a watched state
            if (watchedHashes.Contains(newStateHash))
            {
                string enteredName = GetStateName(newStateHash);
                OnStateEntered?.Invoke(enteredName);
                //Debug.Log($"Entered: {enteredName}");
            }

            currentStateHash = newStateHash;
        }

        // --- SELF-TRANSITION DETECTION ---
        else if (currentNormalizedTime < previousNormalizedTime && previousNormalizedTime > 0.5f) // Need a >0.5 because some animation processes can reduce the time without a loop.
        {
            if (watchedHashes.Contains(newStateHash) && !hasDetectedSelfTransition)
            {
                hasDetectedSelfTransition = true;
                string stateName = GetStateName(newStateHash);
                OnStateRestarted?.Invoke(stateName);
                //Debug.Log($"Self-transition detected in: {stateName}");
            }

            previousNormalizedTime = currentNormalizedTime;
        }
        else
        {
            hasDetectedSelfTransition = false;
            previousNormalizedTime = currentNormalizedTime;
        }
    }

    private string GetStateName(int hash)
    {
        // Simple mapping - you can expand this
        foreach (string name in watchedStates)
        {
            if (Animator.StringToHash(name) == hash)
                return name;
        }
        return hash.ToString();
    }
}