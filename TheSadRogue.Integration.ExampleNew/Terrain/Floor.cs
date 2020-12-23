using SadConsole;
using SadRogue.Primitives;
using TheSadRogue.Integration.New;

namespace TheSadRogue.Integration.ExampleNew.Terrain
{
    /// <summary>
    /// Represents a basic dungeon wall.
    /// </summary>
    public class Floor : RoguelikeObject
    {
        public Floor(Point position)
            : base(new ColoredGlyph(Color.White, Color.Transparent, '.'), position, (int)MapLayer.TERRAIN, true, true)
        { }
    }
}