﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using ARA2D.Components;
using ARA2D.WorldGenerators;
using Nez;

namespace ARA2D.Systems
{
    public class World : EntityProcessingSystem
    {
        readonly WorldGenerator generator;
        readonly Dictionary<ChunkCoords, Chunk> loadedChunks;
        readonly Dictionary<int, TileEntity> tileEntities;

        // TODO: Find a better system to track IDs for tile entities
        // This way isn't flexible and likely to break
        int currentTileEntityID;
        Queue<int> ReleasedTileEntityIDs = new Queue<int>(128);

        public World(WorldGenerator generator) : base(new Matcher().all(typeof(PassiveChunkGenerate)))
        {
            this.generator = generator;
            loadedChunks = new Dictionary<ChunkCoords, Chunk>();
            tileEntities = new Dictionary<int, TileEntity>();
        }

        #region Chunks
        public Chunk GenerateChunk(ChunkCoords coords)
        {
            if (loadedChunks.ContainsKey(coords)) return loadedChunks[coords];
            var chunk = generator.GenerateChunk(coords);
            Events.ChunkGenerated(coords, chunk);
            return loadedChunks[coords] = chunk;
        }

        public void UnloadChunk(ChunkCoords coords)
        {
            if (!loadedChunks.ContainsKey(coords)) return;
            // If there is a tile entity in the chunk that can't be put to sleep, don't unload the chunk
            foreach (var tileEntityIDs in loadedChunks[coords].TileEntityIDs)
            {
                if (!tileEntities[tileEntityIDs].CanSleep())
                {
                    return;
                }
            }

            loadedChunks.Remove(coords);
            Events.ChunkRemoved(coords);
        }

        public bool IsChunkLoaded(ChunkCoords coords)
        {
            return loadedChunks.ContainsKey(coords);
        }
        #endregion Chunks

        #region TileEntities
        public bool IsTileEntityLoaded(int id)
        {
            return tileEntities.ContainsKey(id);
        }

        // Place a tile entity at the block coordinates specified
        public void SetTileEntity(TileEntity entity, long bx, long by)
        {
            entity.ID = NextTileEntityID();
            tileEntities[entity.ID] = entity;

            // Find all the containing chunks
            entity.ContainingChunkCoords = new List<ChunkCoords>();
            var anchorCoords = ChunkCoords.FromBlockCoords(bx, by);
            entity.ContainingChunkCoords.Add(anchorCoords);
            var (width, height) = entity.GetBounds();
            bool overX = bx + width >= Chunk.Size;
            bool overY = by + height >= Chunk.Size;
            if (overX) entity.ContainingChunkCoords.Add(new ChunkCoords(anchorCoords.Cx + 1, anchorCoords.Cy));
            if (overY) entity.ContainingChunkCoords.Add(new ChunkCoords(anchorCoords.Cx, anchorCoords.Cy + 1));
            if (overX && overY) entity.ContainingChunkCoords.Add(new ChunkCoords(anchorCoords.Cx + 1, anchorCoords.Cy + 1));

            foreach (var coords in entity.ContainingChunkCoords)
            {
                RequiredChunk(coords).ContainedTileEntityIDs.Add(entity.ID);
            }
        }

        public Chunk RequiredChunk(ChunkCoords coords)
        {
            if (!loadedChunks.ContainsKey(coords)) return GenerateChunk(coords);
            return loadedChunks[coords];
        }

        public int NextTileEntityID()
        {
            if (ReleasedTileEntityIDs.Count == 0)
            {
                return currentTileEntityID++;
            }
            return ReleasedTileEntityIDs.Dequeue();
        }

        public void DeleteTileEntity(int id)
        {
            Insist.isTrue(IsTileEntityLoaded(id));
            tileEntities.Remove(id);
            ReleasedTileEntityIDs.Enqueue(id);
        }

        public TileEntity this[int id] => tileEntities[id];
        #endregion TileEntities

        public Chunk this[ChunkCoords coords] => loadedChunks[coords];

        public override void process(Entity entity)
        {
            var request = entity.getComponent<PassiveChunkGenerate>();
            GenerateChunk(request.Coords);
            entity.destroy();
        }
    }
}
