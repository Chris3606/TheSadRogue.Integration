using System;
using GoRogue;
using GoRogue.Components;
using GoRogue.GameFramework;
using GoRogue.GameFramework.Components;
using GoRogue.Random;
using JetBrains.Annotations;
using SadConsole;
using SadConsole.Entities;
using SadRogue.Primitives;

namespace TheSadRogue.Integration
{

    /// <summary>
    /// Class that implements IGameObject and uses a SadConsole entity to represent non-terrain objects on a map.
    /// </summary>
    [PublicAPI]
    public class RoguelikeEntity : Entity, IGameObject
    {
        public RoguelikeEntity(ColoredGlyph appearance, Point position, int layer, bool isWalkable = true, bool isTransparent = true,
                               Func<uint>? idGenerator = null, ITaggableComponentCollection? customComponentContainer = null)
            : base(appearance, layer != 0 ? layer : throw new ArgumentException($"{nameof(RoguelikeEntity)} objects may not reside on the terrain layer", nameof(layer)))
        {
            idGenerator ??= GlobalRandom.DefaultRNG.NextUInt;

            Position = position;
            IsWalkable = isWalkable;
            IsTransparent = isTransparent;
            
            // Fire Moved event based on SadConsole's PositionChanged event
            PositionChanged += OnSadConsolePositionChanged;

            CurrentMap = null;

            ID = idGenerator();
            GoRogueComponents = customComponentContainer ?? new ComponentCollection();
            GoRogueComponents.ComponentAdded += On_ComponentAdded;
            GoRogueComponents.ComponentRemoved += On_ComponentRemoved;
        }

        private void OnSadConsolePositionChanged(object? sender, ValueChangedEventArgs<Point> e)
        {
            // Make sure we fire GoRogue's event as well.  This won't reset the property value on failure, so we simply
            // must not catch InvalidOperationException; that's what CanMove is for.
            Moved?.Invoke(this, new GameObjectPropertyChanged<Point>(this, e.OldValue, e.NewValue));
        }

        #region IGameObject Property/Method Implementation
        /// <inheritdoc />
        Point IGameObject.Position
        {
            get => Position;
            // Events are fired by the handler for SadConsole's PositionChanged event
            set => Position = value;
        }

        /// <inheritdoc />
        public event EventHandler<GameObjectPropertyChanged<Point>>? Moved;

        private bool _isWalkable;
        /// <inheritdoc />
        public bool IsWalkable
        {
            get => _isWalkable;
            set => this.SafelySetProperty(ref _isWalkable, value, WalkabilityChanged);
        }

        /// <inheritdoc />
        public event EventHandler<GameObjectPropertyChanged<bool>>? WalkabilityChanged;

        private bool _isTransparent;

        /// <inheritdoc />
        public bool IsTransparent
        {
            get => _isTransparent;
            set => this.SafelySetProperty(ref _isTransparent, value, TransparencyChanged);
        }

        /// <inheritdoc />
        public event EventHandler<GameObjectPropertyChanged<bool>>? TransparencyChanged;

        /// <inheritdoc />
        public uint ID { get; }

        /// <inheritdoc />
        int IHasLayer.Layer => ZIndex;

        /// <inheritdoc />
        public Map? CurrentMap { get; private set; }

        /// <inheritdoc />
        public ITaggableComponentCollection GoRogueComponents { get; }

        /// <inheritdoc />
        public void OnMapChanged(Map? newMap)
        {
            CurrentMap = newMap;
        }
        #endregion

        #region Component Handlers
        private void On_ComponentAdded(object? s, ComponentChangedEventArgs e)
        {
            if (!(e.Component is IGameObjectComponent c))
                return;

            if (c.Parent != null)
                throw new ArgumentException(
                    $"Components implementing {nameof(IGameObjectComponent)} cannot be added to multiple objects at once.");

            c.Parent = this;
        }

        private void On_ComponentRemoved(object? s, ComponentChangedEventArgs e)
        {
            if (e.Component is IGameObjectComponent c)
                c.Parent = null;
        }
        #endregion
    }
}