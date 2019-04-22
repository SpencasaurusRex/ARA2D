﻿using System.Collections.Generic;
using ARA2D.Chunks;
using ARA2D.Core;
using Nez;

namespace ARA2D.Systems
{
    public class TileEntitySystem : ProcessingSystem
    {
        // TODO: True ECS refactor
        readonly Dictionary<int, ITileEntity> tileEntities;
        readonly Dictionary<ChunkCoords, TileEntityChunk> loadedChunks;

        readonly IDTracker idTracker;

        public TileEntitySystem()
        {
            tileEntities = new Dictionary<int, ITileEntity>();
            loadedChunks = new Dictionary<ChunkCoords, TileEntityChunk>();
            idTracker = new IDTracker();
            Events.OnTileChunkGenerated += TileChunkGenerated;
            Events.OnTileChunkRemoved += TileChunkRemoved;
        }

        public void TileChunkGenerated(ChunkCoords coords, TileChunk chunk)
        {
            // TODO: Load tile entity renderables
        }

        public void TileChunkRemoved(ChunkCoords coords)
        {
            // TODO: Unload the renderable for the chunk
        }

        public override void process()
        {
            // TODO: Run TileEntity logic
        }

        public bool IsTileEntityLoaded(int id)
        {
            return tileEntities.ContainsKey(id);
        }

        public bool CanPlaceTileEntity(ITileEntity entity, long bx, long by)
        {
            return CheckOrMarkBounds(entity.Width, entity.Height, bx, by);
        }

        public bool PlaceTileEntity(ITileEntity tileEntity, long bx, long by)
        {
            if (!CheckOrMarkBounds(tileEntity.Width, tileEntity.Height, bx, by)) return false;
            
            tileEntity.ID = idTracker.GetNextID();
            CheckOrMarkBounds(tileEntity.Width, tileEntity.Height, bx, by, tileEntity.ID);
            tileEntities[tileEntity.ID] = tileEntity;

            // Add tileEntity renderable
            tileEntity.CreateEntity(scene, bx, by);
            return true;
        }

        // TODO: Find a better way to prevent repetition
        /// <summary>
        /// This method has two different uses:
        /// If passed id is 0, then check to see if the bounds can fit at the x,y provided
        /// If passed it is not 0, then set ID of tile entity for the bounds at the x,y provided
        /// </summary>
        /// <param name="bounds">The bounds of the tile entity</param>
        /// <param name="bx">The x location of the anchor</param>
        /// <param name="by">The y location of the anchor</param>
        /// <param name="id">The id of the tileEntity, use 0 to perform a check to see if the bounds fit.</param>
        /// <returns>If id is 0, returns whether or not the bounds fit at the location provided. Otherwise returns true</returns>
        bool CheckOrMarkBounds(int width, int height, long bx, long by, int id = 0)
        {
            //long prevX, prevY;
            for (long y = by; y < by + height; y++)
            {
                for (long x = bx; x < bx + width; x++)
                {
                    // TODO: Optimize this by avoiding unnecessary re-lookups of chunkcoords
                    ChunkCoords coords = ChunkCoords.FromBlockCoords(x, y);

                    if (id == 0)
                    {
                        if (RequiredChunk(coords).TileEntityIDs[x & TileChunk.LocalBitMask, y & TileChunk.LocalBitMask] > 0) return false;
                    }
                    else
                    {
                        RequiredChunk(coords).TileEntityIDs[x & TileChunk.LocalBitMask, y & TileChunk.LocalBitMask] = id;
                    }
                    //prevX = x;
                } 
                //prevY = y;
            }
            return true;
        }

        TileEntityChunk RequiredChunk(ChunkCoords coords)
        {
            if (!loadedChunks.ContainsKey(coords)) return GenerateChunk(coords);
            return loadedChunks[coords];
        }

        public TileEntityChunk GenerateChunk(ChunkCoords coords)
        {
            var chunk = new TileEntityChunk(coords);
            loadedChunks[coords] = chunk;
            return chunk;
        }

        public void DeleteTileEntity(int id)
        {
            Insist.isTrue(IsTileEntityLoaded(id));
            tileEntities.Remove(id);
            idTracker.ReleaseID(id);
        }
    }
}
