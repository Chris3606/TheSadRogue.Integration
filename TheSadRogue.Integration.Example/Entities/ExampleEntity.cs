using System;
using GoRogue.Components;
using JetBrains.Annotations;
using SadConsole;
using SadRogue.Primitives;

namespace TheSadRogue.Integration.Example.Entities
{
    public class ExampleEntity : RoguelikeEntity
    {
        public new ExampleMap? CurrentMap => (ExampleMap?)base.CurrentMap;
        
        public ExampleEntity([NotNull] ColoredGlyph appearance, Point position, int layer, bool isWalkable = true, bool isTransparent = true, [CanBeNull] Func<uint>? idGenerator = null, [CanBeNull] ITaggableComponentCollection? customComponentContainer = null)
            : base(appearance, position, layer, isWalkable, isTransparent, idGenerator, customComponentContainer)
        { }
    }
}