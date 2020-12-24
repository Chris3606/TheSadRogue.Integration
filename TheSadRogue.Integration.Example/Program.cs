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

        public const int Width = 80;
        public const int Height = 25;

        //public static RoguelikeMap? Map;
        public static MapScreen? MapScreen { get; private set; }

        static void Main()
        {
            // Setup the engine and create the main window.
            Game.Create(Width, Height);

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
            var generator = new Generator(250, 250);
            generator
                .AddSteps(DefaultAlgorithms.RectangleMapSteps())
                .AddStep(new TranslateToMapStep())
                .Generate();
            var map = generator.Context.GetFirst<ExampleMap>();

            // Create map screen from map and set it as active
            MapScreen = new MapScreen(map, (Width, Height));
            GameHost.Instance.Screen = MapScreen;
        }
    }
}