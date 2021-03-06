﻿using System.Numerics;
using Core.Archetypes;
using Core.Buildings;
using Core.Input;
using Core.Plugins;
using Core.PluginSystems;
using Core.Position;
using Core.WorldGeneration;
using DefaultEcs;
using DefaultEcs.System;

namespace Core
{
    public class Engine
    {
		public static readonly World World = new World();
		public const float TickLength = .3f;

        readonly ISystem<FrameContext> frameSystems;
        readonly ISystem<TickContext> tickSystems;

        readonly ITimeService timeService;
        readonly IInputService inputService;
        readonly Factory factory;

        FrameContext frameContext;
        TickContext tickContext;
        
		float accumulatedTime;

        public Engine(EnginePlugins plugins)
        {
            inputService = plugins.Input;
			timeService = plugins.Time;
            factory = new Factory(plugins.Factory);
			
            // SequentialSystem handles null systems
			frameSystems = new SequentialSystem<FrameContext>(new FrameLogic(factory), plugins.Render);
			tickSystems = new SequentialSystem<TickContext>(new TickLogic(factory));

            Initialize();
        }

        public void Render()
        {
            UpdateFrameContext();
            frameSystems.Update(frameContext);
        }

        public void Update()
        {
            System.Diagnostics.Trace.WriteLine(factory.GetChunkBodies(TileCoords.Create(0,0,0,0))[0]);
            UpdateTickContext();	
            tickSystems.Update(tickContext);
        }

		void Initialize()
        {
            var globalEntity = factory.CreateGlobal();

            // Right wall
            factory.TryPlaceBuilding(TileCoords.Create(1, 0, 0, 0), 4, 4, BuildingType.Test);
            factory.TryPlaceBuilding(TileCoords.Create(1, 0, 0, 4), 4, 4, BuildingType.Test);
            factory.TryPlaceBuilding(TileCoords.Create(1, 0, 0, 8), 4, 4, BuildingType.Test);
            factory.TryPlaceBuilding(TileCoords.Create(1, 0, 0, 12), 4, 4, BuildingType.Test);

            // Building Ghost
            factory.BuildingPlacementGhost(globalEntity.Get<BuildingMenu>());

            // Chunk
            var coords = TileCoords.Create(0, 0, 0, 0);
            var worldGenerator = new WorldGenerator();
            factory.CreateChunk(coords, worldGenerator.GenerateChunk(coords));

            // Camera
            factory.CreateCamera(Vector2.Zero);

            frameContext = new FrameContext(factory, globalEntity, inputService);
            tickContext = new TickContext(factory, globalEntity, inputService);
        }

        void UpdateFrameContext()
        {
            frameContext.Dt = timeService.DeltaTime;
            if (timeService.TickMode == TickMode.Automatic)
            {
                frameContext.TickProgress = accumulatedTime / TickLength;
            }
        }

        void UpdateTickContext()
        {
            if (timeService.TickMode == TickMode.Automatic)
            {
                accumulatedTime += timeService.DeltaTime;
                tickContext.TicksPassed = (int)(accumulatedTime / TickLength);
                accumulatedTime -= TickLength * tickContext.TicksPassed;
            }
            else if (timeService.TickMode == TickMode.Manual)
            {
                tickContext.TicksPassed = timeService.ForceTick ? 1 : 0;
            }
        }
    }
}
