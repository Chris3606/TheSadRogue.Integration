using SadConsole;
using SadRogue.Primitives;
using TheSadRogue.Integration.New;

namespace TheSadRogue.Integration.ExampleNew.Terrain
{
    /// <summary>
    /// Represents a dungeon door.
    /// </summary>
    public class Door : RoguelikeObject
    {
        private bool _closed;

        public bool Closed
        {
            get => _closed;
            set
            {
                if (_closed == value)
                    return;

                OnClosedChanged(value);
            }
        }

        public Door(Point position, bool closed = true)
            : base(new ColoredGlyph(Color.White, Color.Transparent), position, 0, false, false)
        {
            _closed = closed;
            OnClosedChanged(_closed);
        }

        protected virtual void OnClosed()
        {
            Appearance.Glyph = '+';
            IsWalkable = false;
            IsTransparent = false;
        }

        protected virtual void OnOpened()
        {
            Appearance.Glyph = '-';
            IsWalkable = true;
            IsTransparent = true;
        }

        private void OnClosedChanged(bool newValue)
        {
            _closed = newValue;
            if (_closed)
                OnClosed();
            else
                OnOpened();
        }
    }
}