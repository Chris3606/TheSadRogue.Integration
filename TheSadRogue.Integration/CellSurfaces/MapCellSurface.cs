using System;
using System.Collections;
using System.Collections.Generic;
using SadConsole;
using SadConsole.Effects;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace TheSadRogue.Integration.CellSurfaces
{
    /// <summary>
    /// Surface that can be created used to create a ScreenSurface to render the terrain layer of a map.
    /// </summary>
    internal class MapCellSurface : GridViewBase<ColoredGlyph?>, ICellSurface
    {
        private readonly BoundedRectangle _viewArea;
        private Color _defaultBackground;
        private Color _defaultForeground;
        
        private bool _isDirty = true;
        
        /// <summary>
        /// The total width of the cellSurface.
        /// </summary>
        public override int Width => _viewArea.BoundingBox.Width;

        /// <summary>
        /// The total height of the cellSurface.
        /// </summary>
        public override int Height => _viewArea.BoundingBox.Height;

        /// <inheritdoc />
        public override ColoredGlyph? this[Point pos] => _map.GetTerrainAt<RoguelikeTile>(pos)?.Appearance;

        // Disabled nullability check because the issue is due to SadConsole not annotating nullability
        /// <inheritdoc />
#pragma warning disable 8613 
        public IEnumerator<ColoredGlyph?>  GetEnumerator()
#pragma warning restore 8613
        {
            foreach (var pos in this.Positions())
                yield return this[pos];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public void Resize(int width, int height, int bufferWidth, int bufferHeight, bool clear)
            => throw new NotSupportedException($"Surfaces representing a {nameof(RoguelikeMap)} may not be resized.");

        /// <inheritdoc />
        public ICellSurface GetSubSurface(Rectangle view)
            => throw new NotSupportedException($"Surfaces representing a {nameof(RoguelikeMap)} cannot have subsurfaces created from them currently.");

        /// <inheritdoc />
        public void SetSurface(in ICellSurface surface, Rectangle view = new Rectangle())
            => throw new NotSupportedException($"Surfaces representing a {nameof(RoguelikeMap)} do not support SetSurface operations.");

        /// <inheritdoc />
        public void SetSurface(in ColoredGlyph[] cells, int width, int height, int bufferWidth, int bufferHeight)
            => throw new NotSupportedException($"Surfaces representing a {nameof(RoguelikeMap)} do not support SetSurface operations.");

        /// <inheritdoc />
        public int TimesShiftedDown { get; set; }
        
        /// <inheritdoc />
        public int TimesShiftedRight { get; set; }
        
        /// <inheritdoc />
        public int TimesShiftedLeft { get; set; }
        
        /// <inheritdoc />
        public int TimesShiftedUp { get; set; }
        
        /// <inheritdoc />
        public bool UsePrintProcessor { get; set; }
        
        /// <inheritdoc />
        public EffectsManager Effects { get; }
        
        /// <inheritdoc />
        public Rectangle Area => _viewArea.BoundingBox;
        /// <inheritdoc />
        public Color DefaultForeground
        {
            get => _defaultForeground;
            set { _defaultForeground = value; IsDirty = true; }
        }

        /// <inheritdoc />
        public Color DefaultBackground
        {
            get => _defaultBackground;
            set
            {
                _defaultBackground = value;
                IsDirty = true;
            }
        }

        /// <inheritdoc />
        public int DefaultGlyph { get; set; }
        
        /// <inheritdoc />
        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty == value) return;

                _isDirty = value;
                IsDirtyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        /// <inheritdoc />
        public bool IsScrollable => Height != _viewArea.Area.Height || Width != _viewArea.Area.Width;
        
        /// <inheritdoc />
        public Rectangle View
        {
            get => _viewArea.Area;
            set
            {
                _viewArea.SetArea(value);
                IsDirty = true;
            }
        }

        /// <inheritdoc />
        public int ViewWidth
        {
            get => _viewArea.Area.Width;
            set => _viewArea.SetArea(_viewArea.Area.WithWidth(value));
        }

        /// <inheritdoc />
        public int ViewHeight
        {
            get => _viewArea.Area.Height;
            set => _viewArea.SetArea(_viewArea.Area.WithHeight(value));
        }
        
        /// <inheritdoc />
        public Point ViewPosition
        {
            get => _viewArea.Area.Position;
            set
            {
                _viewArea.SetArea(_viewArea.Area.WithPosition(value));
                IsDirty = true;
            }
        }

        /// <inheritdoc />
        public event EventHandler? IsDirtyChanged;

        private RoguelikeMap _map;
        
        public MapCellSurface(RoguelikeMap map, int viewWidth, int viewHeight)
        {
            _map = map;
            Effects = new EffectsManager(this);
            
            _viewArea = new BoundedRectangle((0, 0, map.Width, map.Height),
                (0, 0, viewWidth, viewHeight));
        }
    }
}