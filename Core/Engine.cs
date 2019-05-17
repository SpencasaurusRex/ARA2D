﻿using Core.Archetypes;
using Core.Plugins;
using Core.PluginSystems;
using DefaultEcs;
using DefaultEcs.System;

namespace Core
{
    public class Engine
    {
		public static readonly World World = new World();
		public const float TickLength = .3f;

        readonly ISystem<RenderContext> render;
        readonly ISystem<LogicContext> logic;

        readonly ITimeService timeService;

        readonly RenderContext renderContext = new RenderContext();
        readonly LogicContext logicContext = new LogicContext();
        readonly Factory factory;

		float accumulatedTime;

        public Engine(EnginePlugins plugins)
        {
			render = plugins.Render;
			// SequentialSystem handles null systems
			logic = new SequentialSystem<LogicContext>(plugins.PreLogic, new GameLogic(World), plugins.PostLogic);
			
			timeService = plugins.Time;
            factory = new Factory(plugins.Factory);

            Initialize();
        }

        public void Render()
        {
            render.Update(renderContext);
        }

        public void Update()
        {
            UpdateLogicContext();	
            logic.Update(logicContext);
        }

		void Initialize()
		{
            factory.CreateChunk(0, 0);
		}

        void UpdateLogicContext()
        {
            if (timeService.TickMode == TickMode.Automatic)
            {
                accumulatedTime += timeService.DeltaTime;
                logicContext.TicksPassed = (int)(accumulatedTime / TickLength);
                accumulatedTime -= TickLength * logicContext.TicksPassed;
            }
            else if (timeService.TickMode == TickMode.Manual)
            {
                logicContext.TicksPassed = timeService.ForceTick ? 1 : 0;
            }
        }
    }
}
