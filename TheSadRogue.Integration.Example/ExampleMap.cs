using System;
using SadRogue.Primitives;
using TheSadRogue.Integration.Example.Entities;

namespace TheSadRogue.Integration.Example
{
    public enum MapLayer
    {
        TERRAIN,
        ITEMS,
        MONSTERS,
        PLAYER
    }

    public class ExampleMap : RoguelikeMap
    {
        public new Player? ControlledGameObject
        {
            get => (Player?)base.ControlledGameObject;
            set => base.ControlledGameObject = value;
        }

        public ExampleMap(int width, int height)
            : base(width, height, Enum.GetNames(typeof(MapLayer)).Length - 1, Distance.Chebyshev)
        { }
    }
}