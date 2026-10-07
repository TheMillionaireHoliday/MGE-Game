public interface IResettableObserver
{
    /// <summary>
    /// Method <c>ResetState</c> is called by the SERVER and used for restoring objects during player respawns.
    /// </summary>
    public void ResetStateObserver();
}