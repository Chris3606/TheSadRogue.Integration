using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue;
using GoRogue.Components;
using GoRogue.GameFramework;
using GoRogue.Pathing;
using GoRogue.SpatialMaps;
using JetBrains.Annotations;
using SadConsole;
using SadConsole.Entities;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace TheSadRogue.Integration.New
{
    /// <summary>
    /// Represents a renderer and its entity rendering components
    /// </summary>
    public struct RendererComponents
    {
        /// <summary>
        /// Screen surface acting as parent object for map renderer
        /// </summary>
        public ScreenSurface ScreenSurface;
        
        /// <summary>
        /// Component that renders terrain.
        /// </summary>
        public Renderer TerrainRenderer;
        
        /// <summary>
        /// Component that renders non-terrain.
        /// </summary>
        public Renderer EntityRenderer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="screenSurface"/>
        /// <param name="terrainRenderer"/>
        /// <param name="entityRenderer"/>
        public RendererComponents(ScreenSurface screenSurface, Renderer terrainRenderer, Renderer entityRenderer)
        {
            ScreenSurface = screenSurface;
            TerrainRenderer = terrainRenderer;
            EntityRenderer = entityRenderer;
        }
    }
    
    /// <summary>
    /// Arguments to ControlledGameObjectChanged event.
    /// </summary>
    [PublicAPI]
    public class ControlledGameObjectChangedArgs : EventArgs
    {
        /// <summary>
        /// The old object that was previously assigned to the field.
        /// </summary>
        public RoguelikeObject? OldObject { get; }

        /// <summary>
        /// The new object that was previously assigned to the field.
        /// </summary>
        public RoguelikeObject? NewObject { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="oldObject"/>
        /// <param name="newObject"/>
        public ControlledGameObjectChangedArgs(RoguelikeObject? oldObject, RoguelikeObject? newObject)
        {
            OldObject = oldObject;
            NewObject = newObject;
        }
    }
    
    /// <summary>
    /// A map of <see cref="RoguelikeObject"/> instances.
    /// </summary>
    public class RoguelikeMap : Map
    {
        private readonly List<RendererComponents> _renderers;
        /// <summary>
        /// List of renderers and their constituent components that currently render the map.
        /// </summary>
        public IReadOnlyList<RendererComponents> Renderers => _renderers.AsReadOnly();
        
        private RoguelikeObject? _controlledGameObject;
        /// <summary>
        /// Object being controlled by a player.  When set, adds/removes objects from the map as appropriate.
        /// </summary>
        public RoguelikeObject? ControlledGameObject
        {
            get => _controlledGameObject;
            set
            {
                if (_controlledGameObject == value)
                    return;

                var old = _controlledGameObject;
                if (old != null)
                    RemoveEntity(old);
                
                _controlledGameObject = value;
                if (value != null && !Entities.Contains(value))
                    AddEntity(value);
                
                ControlledGameObjectChanged?.Invoke(this, new ControlledGameObjectChangedArgs(old, value));
            }
        }

        /// <summary>
        /// Fired when <see cref="ControlledGameObject"/> is changed.
        /// </summary>
        public event EventHandler<ControlledGameObjectChangedArgs>? ControlledGameObjectChanged;
        
        /// <inheritdoc />
        public RoguelikeMap(int width, int height, int numberOfEntityLayers, Distance distanceMeasurement,
                            uint layersBlockingWalkability = 4294967295, uint layersBlockingTransparency = 4294967295,
                            uint entityLayersSupportingMultipleItems = 4294967295, FOV? customPlayerFOV = null,
                            AStar? customPather = null, ITaggableComponentCollection? customComponentContainer = null)
            : base(width, height, numberOfEntityLayers, distanceMeasurement, layersBlockingWalkability,
                layersBlockingTransparency, entityLayersSupportingMultipleItems, customPlayerFOV, customPather,
                customComponentContainer)
        {
            _renderers = new List<RendererComponents>();
            
            ObjectAdded += OnObjectAdded;
            ObjectRemoved += OnObjectRemoved;
        }
        
        /// <summary>
        /// Creates a renderer that renders this Map.  When no longer used, <see cref="DisposeOfRenderer"/> must
        /// be called.
        /// </summary>
        /// <param name="viewSize">Viewport size for the renderer.</param>
        /// <param name="font">Font to use for the renderer.</param>
        /// <param name="fontSize">Size of font to use for the renderer.</param>
        /// <returns>A renderer configured with the given parameters.</returns>
        public ScreenSurface CreateRenderer(Point? viewSize = null, Font? font = null, Point? fontSize = null)
        {
            // Default view size is entire Map
            var (viewWidth, viewHeight) = viewSize ?? (Width, Height);
            
            // Create base screen surface
            var screenSurface = new ScreenSurface(viewWidth, viewHeight, Width, Height);
            
            // TODO: Reverse this order of add to surface vs add objects when it won't cause NullReferenceException
            // Create and configure terrain renderer
            var terrainRenderer = new Renderer { DoEntityUpdate = false };
            screenSurface.SadComponents.Add(terrainRenderer);
            foreach (var terrain in Terrain.Positions().Select(GetTerrainAt<RoguelikeObject>))
            {
                if (terrain == null)
                    continue;
            
                terrainRenderer.Add(terrain);
            }

            // Create and configure entity renderer
            var entityRenderer = new Renderer();
            screenSurface.SadComponents.Add(entityRenderer);
            foreach (var entity in Entities.Items.Cast<Entity>())
                entityRenderer.Add(entity);
            
            // Create and add wrapper
            var renderer = new RendererComponents(screenSurface, terrainRenderer, entityRenderer);
            _renderers.Add(renderer);

            return screenSurface;
        }

        /// <summary>
        /// Removes a renderer from the list of renders displaying the map.  This must be called when a renderer is no
        /// longer used, in order to ensure that the renderer resources are freed
        /// </summary>
        /// <param name="renderer">The renderer to unlink.</param>
        public void DisposeOfRenderer(ScreenSurface renderer)
            => _renderers.RemoveAll(components => components.ScreenSurface == renderer);

        private void OnObjectAdded(object? sender, ItemEventArgs<IGameObject> e)
        {
            if (!(e.Item is RoguelikeObject obj))
                throw new InvalidOperationException(
                    $"Only objects of type {nameof(RoguelikeObject)} can be added to a {nameof(RoguelikeMap)}");
            
            if (e.Item.Layer == 0) // Terrain
            {
                foreach (var renderer in _renderers)
                    renderer.TerrainRenderer.Add(obj);
            }
            else // Entities
            {
                foreach (var renderer in _renderers)
                    renderer.EntityRenderer.Add(obj);
            }
        }
        
        private void OnObjectRemoved(object? sender, ItemEventArgs<IGameObject> e)
        {
            // Cast is fine because nothing else can even be added
            var obj = (RoguelikeObject) e.Item;
            
            if (e.Item.Layer == 0) // Terrain
            {
                foreach (var renderer in _renderers)
                    renderer.TerrainRenderer.Remove(obj);
            }
            else // Entities
            {
                foreach (var renderer in _renderers)
                    renderer.EntityRenderer.Remove(obj);
            }
        }
    }
}