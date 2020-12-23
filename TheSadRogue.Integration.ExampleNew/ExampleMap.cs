using System;
using SadRogue.Primitives;
using TheSadRogue.Integration.ExampleNew.Entities;

namespace TheSadRogue.Integration.ExampleNew
{
    public enum MapLayer
    {
        Terrain,
        Items,
        Monsters,
        Player
    }

    public class ExampleMap : New.RoguelikeMap
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