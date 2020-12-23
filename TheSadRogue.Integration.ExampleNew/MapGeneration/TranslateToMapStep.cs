using System.Collections.Generic;
using System.Diagnostics;
using GoRogue.GameFramework;
using GoRogue.MapGeneration;
using GoRogue.MapGeneration.ContextComponents;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using TheSadRogue.Integration.ExampleNew.Entities;
using TheSadRogue.Integration.ExampleNew.Terrain;

namespace TheSadRogue.Integration.ExampleNew.MapGeneration
{
    /// <summary>
    /// Takes the data in the map generation context and translates it to an <see cref="ExampleMap"/>.
    /// </summary>
    public class TranslateToMapStep : GenerationStep
    {
        public string? WallFloorComponentTag;
        public string? DoorListComponentTag;
        public string? MapTag;
        
        public TranslateToMapStep(string? name = null, string? wallFloorComponentTag = "WallFloor",
                                  string? doorListComponentTag = "Doors", string? mapTag = "Map")
            : base(name, 
                (typeof(IGridView<bool>), wallFloorComponentTag), 
                (typeof(DoorList), doorListComponentTag))
        {
            WallFloorComponentTag = wallFloorComponentTag;
            DoorListComponentTag = doorListComponentTag;
            MapTag = mapTag;
        }
        
        protected override IEnumerator<object?> OnPerform(GenerationContext context)
        {
            // Retrieve required components
            var wallFloor = context.GetFirst<IGridView<bool>>(WallFloorComponentTag);
            var doors = context.GetFirst<DoorList>(DoorListComponentTag);
            
            // Create the map
            var map = new ExampleMap(context.Width, context.Height);
            context.Add(map, MapTag);
            
            // Place basic walls/floors based on wall-floor context
            map.ApplyTerrainOverlay(wallFloor, GetTerrain);
            yield return null;
            
            // Place doors at appropriate locations
            foreach (var (_, doorList) in doors.DoorsPerRoom)
                foreach (var door in doorList.Doors)
                    map.SetTerrain(new Door(door));

            yield return null;
            
            // Spawn player
            var player = new Player(map.WalkabilityView.RandomPosition(true), 10);
            map.ControlledGameObject = player;
        }
        
        private static IGameObject GetTerrain(Point position, bool wallFloorVal)
            => wallFloorVal ? (IGameObject)new Floor(position) : new Wall(position);
    }
}