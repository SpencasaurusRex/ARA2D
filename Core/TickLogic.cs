﻿using Core.Archetypes;
using Core.Movement;
using Core.WorldGeneration;
using DefaultEcs.System;

namespace Core
{
    public class TickLogic : AEntitySystem<TickContext>
    {
        readonly ISystem<TickContext> wrappedSystems;

        public TickLogic(Factory factory) : base(Engine.World)
        {
            wrappedSystems = new SequentialSystem<TickContext>
            (
                new CameraDistanceLoader(factory),
                new ChunkLoadProcessor(factory),
                new ComputerMovement(factory),
                new MovementEvaluator(),
                new SuccessfulMoveRemover(factory),
                new SuccessfulMoverPlacer(factory)
                // ,new MoveSystem()
                // ,new TileRandomizer()
            );
        }

        protected override void PreUpdate(TickContext state)
        {
            // TODO: Be a little bit smarter about this. 
            // We could spread tick logic throughout several frames instead.
            for (int i = 0; i < state.TicksPassed; i++)
            {
                wrappedSystems?.Update(state);
            }
        }

        protected override void PostUpdate(TickContext state)
        {
            state.TicksPassed = 0;
        }
    }
}