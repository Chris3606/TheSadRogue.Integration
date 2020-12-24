using System;
using System.Linq;
using GoRogue.GameFramework;
using GoRogue.SpatialMaps;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace TheSadRogue.Integration.FOVVisibilityHandlers
{
    /// <summary>
    /// A class that controls visibility of the map objects based on the map's FOV.  Create a subclass and implement abstract methods to determine what properties to set for what case.
    /// </summary>
    [PublicAPI]
    public abstract class FOVVisibilityHandlerBase<TControlled>
        where TControlled : RoguelikeEntity
    {
        /// <summary>
        /// Possible states for the FOVVisibilityHandler to be in.
        /// </summary>
        public enum State
        {
            /// <summary>
            /// Enabled state -- FOVVisibilityHandler will actively set things as seen/unseen when appropriate.
            /// </summary>
            Enabled,
            /// <summary>
            /// Disabled state.  All items in the map will be set as seen, and the FOVVisibilityHandler
            /// will not set visibility of any items as FOV changes or as items are added/removed.
            /// </summary>
            DisabledResetVisibility,
            /// <summary>
            /// Disabled state.  No changes to the current visibility of terrain/entities will be made, and the FOVVisibilityHandler
            /// will not set visibility of any items as FOV changes or as items are added/removed.
            /// </summary>
            DisabledNoResetVisibility
        }

        /// <summary>
        /// Whether or not the FOVVisibilityHandler is actively setting things to seen/unseen as appropriate.
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// The map that this handler manages visibility of objects for.
        /// </summary>
        public RoguelikeMap<TControlled> Map { get; }

        /// <summary>
        /// Creates a FOVVisibilityHandler that will manage visibility of objects for the given map.
        /// </summary>
        /// <param name="map">The map this handler will manage visibility for.</param>
        /// <param name="startingState">The starting state to put the handler in.</param>
        public FOVVisibilityHandlerBase(RoguelikeMap<TControlled> map, State startingState = State.Enabled)
        {
            Map = map;

            map.ObjectAdded += Map_ObjectAdded;
            map.ObjectMoved += Map_ObjectMoved;
            map.PlayerFOV.Recalculated += Map_PlayerFOVRecalculated;

            SetState(startingState);
        }

        /// <summary>
        /// Sets the state of the FOVVisibilityHandler, affecting its behavior appropriately.
        /// </summary>
        /// <param name="state">The new state for the FOVVisibilityHandler.  See <see cref="State"/> documentation for details.</param>
        public void SetState(State state)
        {
            switch (state)
            {
                case State.Enabled:
                    Enabled = true;

                    foreach (Point pos in Map.Positions())
                    {
                        var terrain = Map.GetTerrainAt<RoguelikeTile>(pos);
                        if (terrain != null && Map.PlayerFOV.BooleanFOV[pos])
                            UpdateTerrainSeen(terrain);
                        else if (terrain != null)
                            UpdateTerrainUnseen(terrain);
                    }

                    foreach (var renderer in Map.Renderers)
                        renderer.IsDirty = true;

                    foreach (var entity in Map.Entities.Items.Cast<RoguelikeEntity>())
                    {
                        if (Map.PlayerFOV.BooleanFOV[entity.Position])
                            UpdateEntitySeen(entity);
                        else
                            UpdateEntityUnseen(entity);
                    }

                    break;

                case State.DisabledNoResetVisibility:
                    Enabled = false;
                    break;

                case State.DisabledResetVisibility:
                    foreach (Point pos in Map.Positions())
                    {
                        var terrain = Map.GetTerrainAt<RoguelikeTile>(pos);
                        if (terrain != null)
                            UpdateTerrainSeen(terrain);
                    }

                    foreach (var renderer in Map.Renderers)
                        renderer.IsDirty = true;

                    foreach (var entity in Map.Entities.Items.Cast<RoguelikeEntity>())
                        UpdateEntitySeen(entity);

                    Enabled = false;
                    break;
            }
        }

        /// <summary>
        /// Sets the state to enabled.
        /// </summary>
        public void Enable() => SetState(State.Enabled);

        /// <summary>
        /// Sets the state to disabled.  If <paramref name="resetVisibilityToSeen"/> is true, all items will be set to seen before
        /// the FOVVisibilityHandler is disabled.
        /// </summary>
        /// <param name="resetVisibilityToSeen">Whether or not to set all items in the map to seen before disabling the FOVVisibilityHandler.</param>
        public void Disable(bool resetVisibilityToSeen = true)
            => SetState(resetVisibilityToSeen ? State.DisabledResetVisibility : State.DisabledNoResetVisibility);

        /// <summary>
        /// Implement to make appropriate changes to a terrain tile that is now inside FOV.
        /// </summary>
        /// <param name="terrain">Terrain tile to modify.</param>
        protected abstract void UpdateTerrainSeen(RoguelikeTile terrain);

        /// <summary>
        /// Implement to make appropriate changes to a terrain tile that is now outside FOV.
        /// </summary>
        /// <param name="terrain">Terrain tile to modify.</param>
        protected abstract void UpdateTerrainUnseen(RoguelikeTile terrain);

        /// <summary>
        /// Implement to make appropriate changes to an entity that is now inside FOV.
        /// </summary>
        /// <param name="entity">Entity to modify.</param>
        protected abstract void UpdateEntitySeen(RoguelikeEntity entity);

        /// <summary>
        /// Implement to make appropriate changes to an entity that is now outside FOV.
        /// </summary>
        /// <param name="entity">Entity to modify.</param>
        protected abstract void UpdateEntityUnseen(RoguelikeEntity entity);

        private void Map_ObjectAdded(object sender, ItemEventArgs<IGameObject> e)
        {
            if (!Enabled) return;

            switch (e.Item)
            {
                case RoguelikeTile terrain:
                    if (Map.PlayerFOV.BooleanFOV[terrain.Position])
                        UpdateTerrainSeen(terrain);
                    else
                        UpdateTerrainUnseen(terrain);
                    break;
                case RoguelikeEntity entity:
                    if (Map.PlayerFOV.BooleanFOV[entity.Position])
                        UpdateEntitySeen(entity);
                    else
                        UpdateEntityUnseen(entity);
                    break;
            }
        }

        // Only entities (not terrain) can move so this is ok to just assume entities.
        private void Map_ObjectMoved(object sender, ItemMovedEventArgs<IGameObject> e)
        {
            if (!Enabled) return;
            var entity = (RoguelikeEntity)e.Item; 
            if (Map.PlayerFOV.BooleanFOV[e.NewPosition])
                UpdateEntitySeen(entity);
            else
                UpdateEntityUnseen(entity);
        }

        private void Map_PlayerFOVRecalculated(object sender, EventArgs e)
        {
            if (!Enabled) return;

            foreach (Point position in Map.PlayerFOV.NewlySeen)
            {
                var terrain = Map.GetTerrainAt<RoguelikeTile>(position);
                if (terrain != null)
                    UpdateTerrainSeen(terrain);

                foreach (var entity in Map.GetEntitiesAt<RoguelikeEntity>(position))
                    UpdateEntitySeen(entity);
            }

            foreach (var renderer in Map.Renderers)
                renderer.IsDirty = true;

            foreach (Point position in Map.PlayerFOV.NewlyUnseen)
            {
                var terrain = Map.GetTerrainAt<RoguelikeTile>(position);
                if (terrain != null)
                    UpdateTerrainUnseen(terrain);

                foreach (var entity in Map.GetEntitiesAt<RoguelikeEntity>(position))
                    UpdateEntityUnseen(entity);
            }
        }
    }
}