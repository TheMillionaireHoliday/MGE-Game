using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputRebindingService : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    public static InputRebindingService Instance { get; private set; }
    public InputActionAsset Asset => inputActions;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ApplyOverrides(List<BindingOverride> overrides)
    {
        inputActions.RemoveAllBindingOverrides();
        if (overrides == null) return;

        foreach (var ov in overrides)
        {
            var action = inputActions.FindAction(ov.actionName, false);
            if (action == null) continue;

            for (int i = 0; i < action.bindings.Count; i++)
            {
                var b = action.bindings[i];
                if (b.id.ToString() != ov.bindingId) continue;

                action.ApplyBindingOverride(i, new InputBinding
                {
                    overridePath = ov.path,
                    overrideProcessors = ov.processors
                });
                break;
            }
        }
    }

    public void SnapshotOverridesInto(GameSettings settings)
    {
        settings.bindingOverrides.Clear();
        foreach (var map in inputActions.actionMaps)
        {
            foreach (var action in map.actions)
            {
                foreach (var b in action.bindings)
                {
                    if (string.IsNullOrEmpty(b.overridePath) &&
                        string.IsNullOrEmpty(b.overrideProcessors)) continue;

                    settings.bindingOverrides.Add(new BindingOverride(
                        action.name,
                        b.id.ToString(),
                        b.overridePath ?? b.path,
                        b.overrideProcessors ?? b.processors));
                }
            }
        }
    }
}