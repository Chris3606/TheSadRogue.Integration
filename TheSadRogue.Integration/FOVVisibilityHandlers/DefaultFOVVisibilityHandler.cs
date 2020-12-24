using System;
using JetBrains.Annotations;
using SadConsole;
using SadRogue.Primitives;

namespace TheSadRogue.Integration.FOVVisibilityHandlers
{
    /// <summary>
    /// Handler that will make all terrain/entities inside FOV visible as normal, all entities outside of FOV invisible, all
    /// terrain outside of FOV invisible if unexplored, and set its foreground to <see cref="ExploredColor"/> if explored but out of FOV.
    /// </summary>
    [PublicAPI]
    public class DefaultFOVVisibilityHandler<TControlled> : FOVVisibilityHandlerBase<TControlled>
        where TControlled : RoguelikeEntity
    {
        /// <summary>
        /// Foreground color to set to all terrain that is outside of FOV but has been explored.
        /// </summary>
        public Color ExploredColor { get; }

        /// <summary>
        /// Creates a DefaultFOVVisibilityHandler that will manage visibility of objects for the given map as noted in the class description.
        /// </summary>
        /// <param name="map">The map this handler will manage visibility for.</param>
        /// <param name="unexploredColor">Foreground color to set to all terrain tiles that are outside of FOV but have been explored.</param>
        /// <param name="startingState">The starting state to put the handler in.</param>
        public DefaultFOVVisibilityHandler(RoguelikeMap<TControlled> map, Color unexploredColor, State startingState = State.Enabled)
            : base(map, startingState) => ExploredColor = unexploredColor;

        /// <summary>
        /// Makes entity visible.
        /// </summary>
        /// <param name="entity">Entity to modify.</param>
        protected override void UpdateEntitySeen(RoguelikeEntity entity) => entity.IsVisible = true;

        /// <summary>
        /// Makes entity invisible.
        /// </summary>
        /// <param name="entity">Entity to modify.</param>
        protected override void UpdateEntityUnseen(RoguelikeEntity entity) => entity.IsVisible = false;

        /// <summary>
        /// Makes terrain visible and sets its foreground color to its regular value.
        /// </summary>
        /// <param name="terrain">Terrain to modify.</param>
        protected override void UpdateTerrainSeen(RoguelikeTile terrain)
        {
            terrain.Appearance.IsVisible = true;
            //terrain.SetForeground(Color.White);
            
            
            
            //terrain.Appearance.Decorators = Array.Empty<CellDecorator>();
            // TODO: Can't handle IsDirty properly...

            //terrain.Appearance.RestoreState();
        }

        /// <summary>
        /// Makes terrain invisible if it is not explored.  Makes terrain visible but sets its foreground to
        /// <see cref="ExploredColor"/> if it is explored.
        /// </summary>
        /// <param name="terrain">Terrain to modify.</param>
        protected override void UpdateTerrainUnseen(RoguelikeTile terrain)
        {
            if (Map.PlayerExplored[terrain.Position])
            {
                //terrain.SetForeground(ExploredColor);
                
                
                
                // TODO: Why no background?
                // TODO: Can't handle IsDirty properly...
                // TODO: Busted if current cell has decorators :(
                //terrain.Appearance.Decorators = new[]
                //    { new CellDecorator(ExploredColor, terrain.Appearance.Glyph, terrain.Appearance.Mirror) };
                
                //terrain.Appearance.Decorators[0] = 
                //terrain.Appearance.SaveState();
                //terrain.Appearance.Foreground = ExploredColor;
            }
            else
                terrain.Appearance.IsVisible = false;
        }
    }
}