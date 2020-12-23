using SadConsole;
using SadRogue.Primitives;

namespace TheSadRogue.Integration.Example.Terrain
{
    /// <summary>
    /// Represents a basic dungeon floor.
    /// </summary>
    public class Floor : RoguelikeTile
    {
        public Floor(Point position)
            : base(new ColoredGlyph(Color.White, Color.Transparent, '.'), position)
        { }
    }
}