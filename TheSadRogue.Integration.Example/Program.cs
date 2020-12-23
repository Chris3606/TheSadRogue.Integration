using GoRogue.GameFramework;
using GoRogue.MapGeneration;
using SadConsole;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace TheSadRogue.Integration.Example
{
    static class Program
    {

        public const int Width = 80;
        public const int Height = 25;

        public static RoguelikeMap? Map;

        static void Main(string[] args)
        {
            // Setup the engine and create the main window.
            Game.Create(Width, Height);

            // Hook the start event so we can add consoles to the system.
            Game.Instance.OnStart = Init;
                        
            // Start the game.
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static IGameObject GetTerrain(Point position, bool wallFloorVal)
            => wallFloorVal ?
                new RoguelikeTile(new ColoredGlyph(Color.White, Color.Transparent, '.'), position) :
                new RoguelikeTile(new ColoredGlyph(Color.White, Color.Transparent, '#'), position, false, false);
        

        private static void Init()
        {
            // Generate map data
            var generator = new Generator(100, 70);
            generator.AddSteps(DefaultAlgorithms.RectangleMapSteps()).Generate();
            
            // Create map from data
            var wallFloor = generator.Context.GetFirst<IGridView<bool>>("WallFloor");
            Map = new RoguelikeMap(generator.Context.Width, generator.Context.Height, 1, Distance.Chebyshev);
            Map.ApplyTerrainOverlay(wallFloor, GetTerrain);
            
            // Make an entity
            var entity = new RoguelikeEntity(new ColoredGlyph(Color.White, Color.Transparent, '@'), (1, 2), 1, false);
            Map.AddEntity(entity);
            
            // Create renderer and set as active screen.  Should render the entity and the terrain
            GameHost.Instance.Screen = Map.CreateRenderer((Width, Height));
        }
    }
}