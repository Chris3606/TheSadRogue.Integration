using SadConsole;
using SadRogue.Primitives;

namespace TheSadRogue.Integration.Example.Terrain
{
    /// <summary>
    /// Represents a dungeon door.
    /// </summary>
    public class Door : RoguelikeTile
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
            : base(new ColoredGlyph(Color.White, Color.Transparent), position, false, false)
        {
            _closed = closed;
            OnClosedChanged(_closed);
        }

        protected virtual void OnClosed()
        {
            SetGlyph('+');
            IsWalkable = false;
            IsTransparent = false;
        }

        protected virtual void OnOpened()
        {
            SetGlyph('-');
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