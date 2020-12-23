using GoRogue.GameFramework;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;

namespace TheSadRogue.Integration.Example.Entities
{
    public class Player : RoguelikeEntity
    {
        public int FOVRadius { get; }
        
        public Player(Point position, int fovRadius)
            : base(new ColoredGlyph(Color.Yellow, Color.Transparent, '@'), position,
                (int) MapLayer.Player, false)
        {
            FOVRadius = fovRadius;
            UseKeyboard = true;
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            Direction dir = Direction.None;
            if (keyboard.IsKeyPressed(Keys.Right))
                dir = Direction.Right;
            else if (keyboard.IsKeyPressed(Keys.Left))
                dir = Direction.Left;
            else if (keyboard.IsKeyPressed(Keys.Up))
                dir = Direction.Up;
            else if (keyboard.IsKeyPressed(Keys.Down))
                dir = Direction.Down;

            if (dir != Direction.None)
            {
                if (this.CanMoveIn(dir))
                    Position += dir;
                return true;
            }
            
            return base.ProcessKeyboard(keyboard);
        }
    }
}