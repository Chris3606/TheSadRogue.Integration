using System;
using SadRogue.Primitives;
using TheSadRogue.Integration.Example.Components;
using TheSadRogue.Integration.Example.Entities;
using TheSadRogue.Integration.FOVVisibilityHandlers;

namespace TheSadRogue.Integration.Example
{
    public enum MapLayer
    {
        Terrain,
        Items,
        Monsters,
        Player
    }

    /// <summary>
    /// Map class that uses the layers enum to define the number of entity layers given to the map and defines some
    /// helpful position-related logic.
    /// </summary>
    public class ExampleMap : RoguelikeMap<Player>
    {
        private readonly FOVVisibilityHandlerBase<Player> _fovVisibilityHandler;

        public ExampleMap(int width, int height)
            : base(width, height, Enum.GetNames(typeof(MapLayer)).Length - 1, Distance.Chebyshev)
        {
            _fovVisibilityHandler = new DefaultFOVVisibilityHandler<Player>(this, Color.DarkGray);
        }

        /// <summary>
        /// Generates a bumped event and triggers component handlers for any entity at the position.
        /// </summary>
        /// <param name="bumper">Entity doing the bumping.</param>
        /// <param name="position">Position bumped</param>
        public void Bump(RoguelikeEntity bumper, Point position)
        {
            foreach (var bumpHandler in GetAllObjectComponents<IBumpHandler>(position))
                bumpHandler.OnBump(bumper);
        }
    }
}