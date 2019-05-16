﻿using Core.Tiles;

namespace Core.Position
{
    public class TileCoords
    {
        // The coords of the chunk
        long chunkX;
        long chunkY;

        // The coords within the chunk
        int localX;
        int localY;

        public bool Dirty;

        public long ChunkX
        {
            get => chunkX;
            set
            {
                chunkX = value;
                Dirty = true;
            }
        }

        public long ChunkY
        {
            get => chunkY;
            set
            {
                chunkY = value;
                Dirty = true;
            }
        }

        public int LocalX
        {
            get => localX;
            set
            {
                localX = value;
                NormalizeLocalX();
                Dirty = true;
            }
        }

        public int LocalY
        {
            get => localY;
            set
            {
                localY = value;
                NormalizeLocalY();
                Dirty = true;
            }
        }

        public TileCoords(long chunkX, int localX, long ChunkY, int localY)
        {
            ChunkX = chunkX;
            LocalX = localX;

            ChunkY = chunkY;
            LocalY = localY;
        }

        void NormalizeLocalX()
        {
            while (LocalX < 0)
            {
                LocalX += Chunk.Size;
                ChunkX--;
            }

            while (LocalX > Chunk.Size)
            {
                LocalX -= Chunk.Size;
                ChunkX++;
            }
        }

        void NormalizeLocalY()
        {
            while (LocalY < 0)
            {
                LocalY += Chunk.Size;
                ChunkY--;
            }

            while (LocalY > Chunk.Size)
            {
                LocalY -= Chunk.Size;
                ChunkY++;
            }
        }
    }
}