using System;
using GoRogue.Components;
using GoRogue.GameFramework;
using SadConsole;
using SadRogue.Primitives;

namespace TheSadRogue.Integration
{
    /// <summary>
    /// Represents terrain.
    /// </summary>
    public class RoguelikeTile : GameObject
    {
        private ColoredGlyph _appearance;

        // TODO: Bugged because the Map has no way of knowing if you set individual fields of ColoredGlyph (eg. Appearance.Glyph = 'a')
        public ColoredGlyph Appearance
        {
            get => _appearance;

            set
            {
                if (!_appearance.Matches(value))
                {
                    _appearance = value;
                    AppearanceChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler? AppearanceChanged;

        public RoguelikeTile(ColoredGlyph appearance, Point position, bool isWalkable = true, bool isTransparent = true,
                             Func<uint>? idGenerator = null,
                             ITaggableComponentCollection? customComponentContainer = null)
            : base(position, 0, isWalkable, isTransparent, idGenerator, customComponentContainer)
        {
            Appearance = appearance;
        }
    }
}