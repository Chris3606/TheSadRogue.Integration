using GoRogue.MapGeneration;
using SadConsole;
using SadConsole.Host;
using TheSadRogue.Integration.Example.MapGeneration;
using TheSadRogue.Integration.Example.Screens;
using Game = SadConsole.Game;

namespace TheSadRogue.Integration.Example
{
    public static class Program
    {

        public const int ScreenWidth = 80;
        public const int ScreenHeight = 25;
        
        public static MapScreen? MapScreen { get; private set; }

        static void Main()
        {
            // Setup the engine and create the main window.
            Game.Create(ScreenWidth, ScreenHeight);

            // Hook the start event so we can add consoles to the system.
            Game.Instance.OnStart = Init;
            
            // Unlock FPS
            Global.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
            Game.Instance.MonoGameInstance.IsFixedTimeStep = false;
                        
            // Start the game.
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static void Init()
        {
            // Generate a map
            var generator = new Generator(100, 100);
            generator
                .AddSteps(DefaultAlgorithms.RectangleMapSteps())
                //.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps(
                //    minRooms: 15, maxRooms: 25, roomMinSize:5, roomMaxSize: 11, saveDeadEndChance: 0))
                .AddStep(new TranslateToMapStep())
                .Generate();
            var map = generator.Context.GetFirst<ExampleMap>();

            // Create map screen from map and set it as active
            MapScreen = new MapScreen(map, (ScreenWidth, ScreenHeight));
            GameHost.Instance.Screen = MapScreen;
        }
    }
}