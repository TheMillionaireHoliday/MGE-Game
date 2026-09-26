using FishNet.Object;
using System.Collections.Generic;

public class StatusEffectTarget : NetworkBehaviour, IResettableServer
{
    private List<StatusEffect> activeEffects = new List<StatusEffect>();

    [Server]
    public void AddEffect(StatusEffect effect)
    {
        RemoveEffect(effect.GetEffectName());

        effect.Apply(this);
        activeEffects.Add(effect);
    }
    void Update()
    {
        if (activeEffects.Count == 0)
            return;

        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect effect = activeEffects[i];

            if (effect == null)
            {
                activeEffects.RemoveAt(i);
                continue;
            }

            effect.Update();

            if (effect.markedForRemoval)
            {
                activeEffects.RemoveAt(i);
            }
        }

        /*for (int i = 0; i < activeEffects.Count; i++) 
        {
            print($"[{i}] {activeEffects[i].GetEffectName()}");
        }*/
    }

    [Server]
    public void RemoveEffect(string effectName)
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i].GetEffectName() == effectName)
            {
                activeEffects[i].Remove();
                activeEffects.RemoveAt(i);
            }
        }
    }

    [Server]
    public bool ContainsEffect(string effectName)
    {
        foreach (var effect in activeEffects)
        {
            print(effect.GetEffectName());

            if (effect.GetEffectName() == effectName)
            {
                return true;
            }
        }

        return false;
    }

    [Server]
    private void RemoveAllEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].Remove();
            activeEffects.RemoveAt(i);
        }
    }

    [Server]
    public void ResetStateServer()
    {
        RemoveAllEffects();
    }
}
