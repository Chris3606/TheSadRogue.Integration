using System;
using JetBrains.Annotations;
using SadConsole;

namespace TheSadRogue.Integration.Components
{
    /// <summary>
    /// Debug component that outputs game FPS.
    /// </summary>
    [PublicAPI]
    public class FPSCounterComponent : SadConsole.Components.UpdateComponent
    {
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private int _frameCounter;
        
        /// <summary>
        /// Currently recorded frame rate.
        /// </summary>
        public int FrameRate { get; private set; }
        
        /// <inheritdoc />
        public override void Update(IScreenObject host, TimeSpan delta)
        {
            _frameCounter += 1;
            _elapsedTime += delta;

            if (_elapsedTime > TimeSpan.FromSeconds(1))
            {
                _elapsedTime -= TimeSpan.FromSeconds(1);
                FrameRate = _frameCounter;
                System.Console.WriteLine($"Current FPS: {FrameRate}");
                _frameCounter = 0;
            }
        }
        
        
    }
}