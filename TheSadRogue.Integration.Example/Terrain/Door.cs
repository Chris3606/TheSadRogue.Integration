using GoRogue.GameFramework.Components;
using SadConsole;
using SadRogue.Primitives;
using TheSadRogue.Integration.Example.Components;

namespace TheSadRogue.Integration.Example.Terrain
{
    public class DoorBumpHandler : ComponentBase<Door>, IBumpHandler
    {
        public void OnBump(RoguelikeEntity bumper)
        {
            if (Parent == null || !Parent.Closed) return;
            Parent.Closed = false;
        }
    }
    
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
            GoRogueComponents.Add(new DoorBumpHandler());
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