﻿using isometricgame.GameEngine;
using isometricgame.GameEngine.WorldSpace;
using isometricgame.GameEngine.Rendering;
using isometricgame.GameEngine.Systems;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using isometricgame.GameEngine.Exceptions.Services;
using isometricgame.GameEngine.Scenes;
using isometricgame.GameEngine.Events.Arguments;
using isometricgame.GameEngine.Systems.Rendering;
using isometricgame.GameEngine.Systems.Serialization;
using OpenTK.Graphics;
using isometricgame.GameEngine.Systems.Input;
using isometricgame.GameEngine.Tools;

namespace isometricgame.GameEngine
{
    public class Game : GameWindow
    {
        //PATHS
        public readonly string GAME_DIRECTORY_BASE;
        public readonly string GAME_DIRECTORY_WORLDS;
        public readonly string GAME_DIRECTORY_ASSETS;
        public readonly string GAME_DIRECTORY_SHADERS;
        
        #region Systems
        //SERVICES
        private AssetProvider contentPipe;
        private SpriteLibrary spriteLibrary;
        private RenderService renderService;
        private TextDisplayer textDisplayer;
        private InputSystem inputSystem;
        private AnimationSchematicLibrary animationSchematicLibrary;
        private SceneManagementService sceneManagementService;

        private List<GameSystem> systems = new List<GameSystem>();
        /// <summary>
        /// Responsible for loading and unloading textures.
        /// </summary>
        public AssetProvider AssetProvider { get => contentPipe; private set => contentPipe = value; }
        /// <summary>
        /// Responsible for recording and recieving loaded sprites.
        /// </summary>
        public SpriteLibrary SpriteLibrary { get => spriteLibrary; private set => spriteLibrary = value; }
        internal RenderService RenderService { get => renderService; private set => renderService = value; }
        public TextDisplayer TextDisplayer { get => textDisplayer; private set => textDisplayer = value; }
        public InputSystem InputSystem { get => inputSystem; private set => inputSystem = value; }
        public AnimationSchematicLibrary AnimationSchematicLibrary { get => animationSchematicLibrary; private set => animationSchematicLibrary = value; }
        public SceneManagementService SceneManagementService { get => sceneManagementService; private set => sceneManagementService = value; }
        
        public EventScheduler EventScheduler { get; private set; }
        #endregion

        #region Time
        private double renderTime, updateTime;

        public double RenderTime => renderTime;
        public double UpdateTime => updateTime;
        #endregion

        private Scene scene;

        public Game(int width, int height, string title, string GAME_DIR = "", string GAME_DIR_ASSETS = "", string GAME_DIR_WORLDS = "")
            : base(width, height, GraphicsMode.Default, title)
        {
            GAME_DIRECTORY_BASE = (GAME_DIR == String.Empty) ? AppDomain.CurrentDomain.BaseDirectory : GAME_DIR;
            GAME_DIRECTORY_ASSETS = (GAME_DIR_ASSETS == String.Empty) ? Path.Combine(GAME_DIRECTORY_BASE, "Assets" + Path.DirectorySeparatorChar) : GAME_DIR_ASSETS;
            GAME_DIRECTORY_SHADERS = Path.Combine(GAME_DIRECTORY_ASSETS, "Shaders" + Path.DirectorySeparatorChar);
            GAME_DIRECTORY_WORLDS = (GAME_DIR_WORLDS == String.Empty) ? Path.Combine(GAME_DIRECTORY_BASE, "Worlds" + Path.DirectorySeparatorChar) : GAME_DIR_WORLDS;
            
            RegisterSystems();
            EventScheduler = new EventScheduler();
            RenderService.LoadShaders(GetShaders());

            //END SERVICES

            LoadContent();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle);
            RenderService.AdjustProjection(Width, Height);
            scene.RescaleScene();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            updateTime += e.Time;
            EventScheduler.Progress_Events(e.Time);
            scene.UpdateScene(new FrameArgument(UpdateTime, e.Time));
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            renderTime += e.Time;

            scene.BeginRender(RenderService);
            RenderService.BeginRender();

            RenderService.RenderScene(scene, new FrameArgument(RenderTime, e.Time));

            RenderService.EndRender();
            
            SwapBuffers();
        }

        protected override void OnLoad(EventArgs e)
        {

        }

        protected override void OnUnload(EventArgs e)
        {
            foreach (GameSystem gamesys in systems)
                gamesys.Unload();
        }

        public T GetSystem<T>() where T : GameSystem
        {
            foreach (GameSystem system in systems)
                if (system is T && system.Accessable)
                    return system as T;
            throw new ServiceNotFoundException();
        }

        protected void RegisterSystem<T>(T gameService) where T : GameSystem
        {
            if (systems.Exists((s) => s is T))
                throw new ExistingServiceException();
            systems.Add(gameService);
        }

        protected virtual string[] GetShaders()
        {
            return new string[] { "shader" };
        }

        internal virtual void RegisterSystems()
        {
            AssetProvider = new AssetProvider(this);
            SpriteLibrary = new SpriteLibrary(this);
            RenderService = new RenderService(this, Width, Height);
            TextDisplayer = new TextDisplayer(this);
            InputSystem = new InputSystem(this);
            AnimationSchematicLibrary = new AnimationSchematicLibrary(this);
            SceneManagementService = new SceneManagementService(this);

            RegisterSystem(AssetProvider);
            RegisterSystem(SpriteLibrary);
            RegisterSystem(RenderService);
            RegisterSystem(TextDisplayer);
            RegisterSystem(InputSystem);
            RegisterSystem(AnimationSchematicLibrary);
            RegisterSystem(SceneManagementService);

            RegisterCustomSystems();

            foreach (GameSystem system in systems)
                system.Load();
        }

        protected virtual void RegisterCustomSystems() { }
        protected virtual void LoadContent() { }

        public void SetScene(Scene scene)
        {
            this.scene = scene;
        }
    }
}
