using System;
using GoRogue.Components;
using GoRogue.GameFramework;
using JetBrains.Annotations;
using SadConsole;
using SadRogue.Primitives;

namespace TheSadRogue.Integration
{
    /// <summary>
    /// Class that implements IGameObject and uses a SadConsole ColoredGlyph to represent terrain objects on a map.
    /// </summary>
    [PublicAPI]
    public class RoguelikeTile : GameObject
    {
        private ColoredGlyph _appearance;
        
        /// <summary>
        /// The appearance of the terrain.
        /// </summary>
        public ColoredGlyph Appearance
        {
            get => _appearance;

            set
            {
                if (_appearance.Matches(value))
                    return;
                
                _appearance = value;
                AppearanceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        // TODO: Bugged because the Map has no way of knowing if you set individual fields of ColoredGlyph (eg. Appearance.Glyph = 'a')
        /// <summary>
        /// Fired when the Appearance is changed.
        /// </summary>
        public event EventHandler? AppearanceChanged;

        public RoguelikeTile(ColoredGlyph appearance, Point position, bool isWalkable = true, bool isTransparent = true,
                             Func<uint>? idGenerator = null,
                             ITaggableComponentCollection? customComponentContainer = null)
            : base(position, 0, isWalkable, isTransparent, idGenerator, customComponentContainer)
        {
            _appearance = appearance;
        }
    }
}