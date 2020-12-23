using SadConsole;
using SadRogue.Primitives;

namespace TheSadRogue.Integration.Example.Terrain
{
    /// <summary>
    /// Represents a basic dungeon wall.
    /// </summary>
    public class Wall : RoguelikeTile
    {
        public Wall(Point position)
            : base(new ColoredGlyph(Color.White, Color.Transparent, '#'), position, false, false)
        { }
    }
}