namespace TheSadRogue.Integration.Example.Components
{
    /// <summary>
    /// Interface for components that will handle bumps.
    /// </summary>
    public interface IBumpHandler
    {
        public void OnBump(RoguelikeEntity bumper);
    }
}