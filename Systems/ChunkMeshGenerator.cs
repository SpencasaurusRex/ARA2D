﻿using Nez;
using ARA2D.Components;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ARA2D.Systems
{
    public class ChunkMeshGenerator : EntityProcessingSystem
    {
        // Generate up to 4 chunk meshes per frame for now 
        // TODO: Have variable handling depending on current performance     
        public int HandlePerFrame = 4;

        Texture2D chunkTextures;

        public ChunkMeshGenerator(Texture2D chunkTextures) : base(new Matcher().all(typeof(ChunkGeneratedEvent)))
        {
            this.chunkTextures = chunkTextures;
        }

        public override void process(Entity entity)
        {
            var chunkGenEvent = entity.getComponent<ChunkGeneratedEvent>();
            var chunk = chunkGenEvent.Chunk;

            Vector2[] positions = CreatePositionsArray();
            short[] triangles = CreateTrianglesArray();
            RenderableComponent renderable = CreateMesh(positions, triangles);

            Entity e = new Entity($"ChunkMesh{chunkGenEvent.Coords.Cx},{chunkGenEvent.Coords.Cy}");
            e.addComponent(renderable);
            Core.scene.addEntity(e);

            entity.destroy();
        }

        static Vector2[] CreatePositionsArray()
        {
            var positions = new Vector2[(Chunk.Size + 1) * (Chunk.Size + 1)];

            for (int i = 0, y = 0; y <= Chunk.Size; y++)
            {
                for (int x = 0; x <= Chunk.Size; x++, i++)
                {
                    positions[i] = new Vector2(x, y);
                }
            }

            return positions;
        }

        static short[] CreateTrianglesArray()
        {
            short[] triangles = new short[Chunk.Size * Chunk.Size * 6];
            for (int ti = 0, vi = 0, y = 0; y < Chunk.Size; y++, vi++)
            {
                for (int x = 0; x < Chunk.Size; x++, ti += 6, vi++)
                {
                    triangles[ti] = (short)vi;
                    triangles[ti + 3] = triangles[ti + 1] = (short)(vi + 1);
                    triangles[ti + 5] = triangles[ti + 2] = (short)(vi + Chunk.Size + 1);
                    triangles[ti + 4] = (short)(vi + Chunk.Size + 2);
                }
            }
            return triangles;
        }

        RenderableComponent CreateMesh(Vector2[] positions, short[] indices)
        {
            UvMesh m = new UvMesh();
            m.SetVertexPositions(positions);
            m.SetIndices(indices);
            m.RecalculateBounds();
            m.SetColorForAllVertices(Color.White);
            m.SetTexture(chunkTextures);

            return m;
        }

        protected override void process(List<Entity> entities)
        {
            for (int i = 0; i < HandlePerFrame && i < entities.Count; i++)
            {
                process(entities[i]);
            }
        }
    }
}