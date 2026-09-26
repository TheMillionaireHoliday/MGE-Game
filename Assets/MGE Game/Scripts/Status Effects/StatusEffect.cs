using System;

public class StatusEffect
{
    public bool markedForRemoval = false;

    public virtual void Apply(StatusEffectTarget target)
    {

    }
    public virtual void Remove()
    {
        markedForRemoval = true;
    }
    public virtual void Update()
    {

    }

    public virtual string GetEffectName()
    {
        throw new NotImplementedException();
    }
}
