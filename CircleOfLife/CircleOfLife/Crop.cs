﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace CircleOfLife
{
    public class Crop
    {
        int growTime;
        Random random;
        TimeSpan growTimer;
        int maxPlants;

        public List<Environment> plants = new List<Environment>(50);

        public Crop(int growTime, int randSeed, int maxPlants)
        {
            this.growTime = growTime;
            random = new Random(randSeed);
            this.maxPlants = maxPlants;
            growTimer = new TimeSpan(0, 0, 0);
        }

        public void addPlant(Environment plant)
        {
            plants.Add(plant);
        }

        public void grow(GameTime gameTime, int width, int height)
        {
            // just grow one of the plants in the crop
            removeDead(gameTime);
            // don't add anymore if it has reached the max plants for the crop
            if (plants.Count >= maxPlants)
                return;

            growTimer += gameTime.ElapsedGameTime;
            if (growTimer > TimeSpan.FromSeconds(growTime) && plants.Count > 0)
            {
                plants.Add(plants[random.Next(plants.Count)].grow(width, height));
                growTimer = TimeSpan.Zero;
            }
        }

        public void removeDead(GameTime gameTime)
        {
            for (int i = 0; i < plants.Count; i++)
            {
                plants[i].update(gameTime);
                if (plants[i].state == 1)
                {
                    plants.RemoveAt(i);
                }
            }
        }

        public void draw(ref SpriteBatch spriteBatch, ref Texture2D spriteSheet, Vector3 offset, int frame)
        {
            for (int i = 0; i < plants.Count; i ++)
            {
                plants[i].draw(ref spriteBatch, ref spriteSheet, offset);
            }
        }
    }
}
