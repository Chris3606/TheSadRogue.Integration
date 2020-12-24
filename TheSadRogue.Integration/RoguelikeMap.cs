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
using TheSadRogue.Integration.CellSurfaces;

namespace TheSadRogue.Integration
{
    /// <summary>
    /// Arguments to ControlledGameObjectChanged event.
    /// </summary>
    [PublicAPI]
    public class ControlledGameObjectChangedArgs<T> : EventArgs where T : RoguelikeEntity
    {
        /// <summary>
        /// The old object that was previously assigned to the field.
        /// </summary>
        public T? OldObject { get; }

        /// <summary>
        /// The new object that was previously assigned to the field.
        /// </summary>
        public T? NewObject { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="oldObject"/>
        /// <param name="newObject"/>
        public ControlledGameObjectChangedArgs(T? oldObject, T? newObject)
        {
            OldObject = oldObject;
            NewObject = newObject;
        }
    }

    /// <summary>
    /// Class that inherits from GoRogue's GameFramework.Map, and provides facilities to create SadConsole renderers
    /// that render the objects on the map.
    /// </summary>
    [PublicAPI]
    public class RoguelikeMap : RoguelikeMap<RoguelikeEntity>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="numberOfEntityLayers"></param>
        /// <param name="distanceMeasurement"></param>
        /// <param name="layersBlockingWalkability"></param>
        /// <param name="layersBlockingTransparency"></param>
        /// <param name="entityLayersSupportingMultipleItems"></param>
        /// <param name="customPlayerFOV"></param>
        /// <param name="customPather"></param>
        /// <param name="customComponentContainer"></param>
        public RoguelikeMap(int width, int height, int numberOfEntityLayers, Distance distanceMeasurement,
                            uint layersBlockingWalkability = 4294967295, uint layersBlockingTransparency = 4294967295,
                            uint entityLayersSupportingMultipleItems = 4294967295, FOV? customPlayerFOV = null,
                            AStar? customPather = null, ITaggableComponentCollection? customComponentContainer = null)
            : base(width, height, numberOfEntityLayers, distanceMeasurement, layersBlockingWalkability,
                layersBlockingTransparency, entityLayersSupportingMultipleItems, customPlayerFOV, customPather,
                customComponentContainer)
        { }
    }

    /// <summary>
    /// Like <see cref="RoguelikeMap"/>, except its ControlledGameObject is of a custom type.
    /// </summary>
    [PublicAPI]
    public class RoguelikeMap<TControlled> : Map
        where TControlled : RoguelikeEntity
    {
        private readonly List<ScreenSurface> _renderers;
        /// <summary>
        /// List of renderers that currently render the map.
        /// </summary>
        public IReadOnlyList<ScreenSurface> Renderers => _renderers.AsReadOnly();

        private TControlled? _controlledGameObject;

        /// <summary>
        /// Object being controlled by a player.  When set, adds/removes objects from the map as appropriate.
        /// </summary>
        public TControlled? ControlledGameObject
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
                
                ControlledGameObjectChanged?.Invoke(this, new ControlledGameObjectChangedArgs<TControlled>(old, value));
            }
        }

        /// <summary>
        /// Fired when <see cref="ControlledGameObject"/> is changed.
        /// </summary>
        public event EventHandler<ControlledGameObjectChangedArgs<TControlled>>? ControlledGameObjectChanged;


        /// <inheritdoc />
        public RoguelikeMap(int width, int height, int numberOfEntityLayers, Distance distanceMeasurement,
                            uint layersBlockingWalkability = 4294967295, uint layersBlockingTransparency = 4294967295,
                            uint entityLayersSupportingMultipleItems = 4294967295, FOV? customPlayerFOV = null,
                            AStar? customPather = null, ITaggableComponentCollection? customComponentContainer = null)
            : base(width, height, numberOfEntityLayers, distanceMeasurement, layersBlockingWalkability,
                layersBlockingTransparency, entityLayersSupportingMultipleItems, customPlayerFOV, customPather,
                customComponentContainer)
        {
            _renderers = new List<ScreenSurface>();
            
            ObjectAdded += OnObjectAdded;
            ObjectRemoved += OnObjectRemoved;
            PlayerFOV.Recalculated += PlayerFOVOnRecalculated;
        }

        #region Renderer Generation/Syncing
        // TODO: Temp, should really just flag this based on appearance.
        private void PlayerFOVOnRecalculated(object? sender, FOVRecalculatedEventArgs e)
        {
            foreach (var surface in _renderers)
                surface.IsDirty = true;
        }
        
        private void OnTerrainAppearanceChanged(object? sender, EventArgs e)
        {
            foreach (var surface in _renderers)
                surface.IsDirty = true;
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

            // Create surface representing the terrain layer of the map
            var cellSurface = new MapCellSurface<TControlled>(this, viewWidth, viewHeight);
            
            // Create screen surface that renders that cell surface and keep track of it
            var renderer = new ScreenSurface(cellSurface, font, fontSize);
            _renderers.Add(renderer);
            
            // Create an EntityRenderer and configure it with all the appropriate entities,
            // then add it to the main surface
            var entityRenderer = new Renderer();
            // TODO: Reverse this order when it won't cause NullReferenceException
            renderer.SadComponents.Add(entityRenderer);
            entityRenderer.AddRange(Entities.Items.Cast<Entity>());

            // Return renderer
            return renderer;
        }

        /// <summary>
        /// Removes a renderer from the list of renders displaying the map.  This must be called when a renderer is no
        /// longer used, in order to ensure that the renderer resources are freed
        /// </summary>
        /// <param name="renderer">The renderer to unlink.</param>
        public void DisposeOfRenderer(ScreenSurface renderer) => _renderers.Remove(renderer);

        private void OnObjectAdded(object? sender, ItemEventArgs<IGameObject> e)
        {
            switch (e.Item)
            {
                case RoguelikeTile terrain:
                    // Ensure we flag the surfaces of renderers as dirty on the add and on subsequent appearance changed events
                    terrain.AppearanceChanged += OnTerrainAppearanceChanged;
                    OnTerrainAppearanceChanged(terrain, EventArgs.Empty);
                    break;
                
                case RoguelikeEntity entity:
                    // Add to any entity renderers we have
                    foreach (var renderer in _renderers)
                    {
                        var entityRenderer = renderer.GetSadComponent<Renderer>();
                        entityRenderer?.Add(entity);
                    }
                    break;
                
                default:
                    throw new InvalidOperationException(
                        $"Objects added to a {nameof(RoguelikeMap<TControlled>)} must be of type {nameof(RoguelikeTile)} for terrain, or {nameof(RoguelikeEntity)} for non-terrain");
            }
        }

        private void OnObjectRemoved(object? sender, ItemEventArgs<IGameObject> e)
        {
            switch (e.Item)
            {
                case RoguelikeTile terrain:
                    // Ensure we flag the surfaces of renderers as dirty on the remove and unlike our changed handler
                    terrain.AppearanceChanged -= OnTerrainAppearanceChanged;
                    OnTerrainAppearanceChanged(e.Item, EventArgs.Empty);
                    break;
                
                case RoguelikeEntity entity:
                    // Remove from any entity renderers we have
                    foreach (var renderer in _renderers)
                    {
                        var entityRenderer = renderer.GetSadComponent<Renderer>();
                        entityRenderer?.Remove(entity);
                    }
                    break;
            }
        }
        #endregion
        
        #region ComponentForwarders
        // TODO: Write others, or maybe even move to GoRogue.
        /// <summary>
        /// Gets all components on all objects of the specified type at the given location, in layer order
        /// </summary>
        /// <param name="position"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetAllObjectComponents<T>(Point position)
            where T : class
        {
            foreach (var entity in GetObjectsAt(position))
                foreach (var component in entity.GoRogueComponents.GetAll<T>())
                    yield return component;
        }
        #endregion
    }
}