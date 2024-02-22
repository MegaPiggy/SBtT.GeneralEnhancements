namespace GeneralEnhancements
{
    public abstract class Feature
    {
        /// <summary> Called on the first update instead of Start if need things to be initialized in base game. </summary>
        public virtual void LateInitialize() { }
        public virtual void OnSettingsUpdate() { }
        public abstract void Update();
    }
}