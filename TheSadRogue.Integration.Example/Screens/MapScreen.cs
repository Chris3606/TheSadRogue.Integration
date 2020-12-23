using System.Diagnostics;
using GoRogue.GameFramework;
using SadConsole;
using SadRogue.Primitives;

namespace TheSadRogue.Integration.Example.Screens
{
    public class MapScreen : ScreenObject
    {
        public ExampleMap Map { get; }
        public ScreenSurface MapRenderer { get; }

        public MapScreen(ExampleMap map, Point mapViewSize)
        {
            // Take map from input and create renderer for it with the specified size.
            Map = map;
            MapRenderer = map.CreateRenderer(mapViewSize);
            Children.Add(MapRenderer);
            
            // Set up appropriate handler to recalculate FOV and handle camera centering
            Map.ControlledGameObjectChanged += MapOnPlayerChanged;

            // Initialize already added player, if there is one
            if (Map.ControlledGameObject != null)
            {
                Debug.WriteLine("Focused.");
                Map.ControlledGameObject.Moved += PlayerOnMoved;
                Map.ControlledGameObject.IsFocused = true;
                HandlePlayerMoved();
            }
        }

        #region Player Event Handling
        private void MapOnPlayerChanged(object? sender, ControlledGameObjectChangedArgs e)
        {
            if (e.OldObject != null)
            {
                e.OldObject.Moved -= PlayerOnMoved;
            }

            if (e.NewObject != null)
            {
                e.NewObject.Moved += PlayerOnMoved;
                e.NewObject.IsFocused = true;
                HandlePlayerMoved();
            }
        }
        
        private void PlayerOnMoved(object? sender, GameObjectPropertyChanged<Point> e)
            => HandlePlayerMoved();

        // Center camera on player and recalculate FOV on player added/moved
        private void HandlePlayerMoved()
        {
            MapRenderer.Surface.View = MapRenderer.Surface.View.WithCenter(Map.ControlledGameObject!.Position);
            MapRenderer.IsDirty = true;
            //Map.PlayerFOV.Calculate(Map.ControlledGameObject!.Position, Map.ControlledGameObject.FOVRadius, Distance.Chebyshev);
        }
        #endregion
    }
}