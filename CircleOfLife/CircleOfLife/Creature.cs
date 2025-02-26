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
    public class Creature
    {

        public Species parent;

        //species characteristics
        public byte diet;
        private int size;
        public int detection;
        private int speed;
        private int energyCap;
        public int foodCap;
        private int waterCap;
        private int energyValue;
        public float agility;
        public Color color;
        public int stamina;

        //sprite location base
        Rectangle spriteRectangle;
        public RotatedRectangle body;

        int mapHeight;
        int mapWidth;

        //creature state
        public byte state;
        public int food;
        private int water;
        public int energy;
        private float currSpeed;

        //Dead?
      //  public bool dead;      //when killed by natural causes(predator or starvation) and there is a corpse
       // public bool erased;    //when killed by a mormo or power and disappear nearly instantly
        public int foodValue;

        public int feedSpeed;   //amount fed per second

        //creatures
        private Creature predator;
        private Creature prey;

        // flora
        public Environment flora;

        //position
        public Vector2 position;
        private float orientation;
        public Vector2 goalPosition;
        public Vector2 origin;
        public float width;
        public float height;

        // timer
        TimeSpan deathtimer;
        public TimeSpan feedTimer;
        public TimeSpan sprintTime;
        public TimeSpan restTime;

        public TimeSpan corpseTimer;
        public int rotTime;    //temporary?
        public int rotStage;        //three frames/stages to death

        //Animations
        int frameOffset;

        //Random :}
        Random random;

        //accesors

        public Vector2 Position { get { return position; } }
        public int EnergyValue { get { return energyValue; } }
        public Creature Predator { get { return predator; } set { predator = value; } }
        public Creature Prey { get { return prey; } set { prey = value; } }

        //constructor
        public Creature(int xPos, int yPos, Ecosystem.speciesStats stats, Random seed, Species parent)
        {
            this.parent = parent;
            diet = stats.diet;
            switch (diet)
            {
                case 0:
                    spriteRectangle = Sprites.herbivore;
                    break;
                case 1:
                    spriteRectangle = Sprites.carnivore;
                    break;
                default:
                    spriteRectangle = Sprites.mormo;
                    break;
            }
            size = stats.size;
            detection = stats.detection;
            speed = stats.speed;
            energyCap = stats.energyCap;
            foodCap = stats.foodCap;
            waterCap = stats.waterCap;
            energyValue = stats.energyValue;
            agility = stats.agility;

            color = stats.color;

            random = seed;

            position = new Vector2(xPos, yPos);
            
            orientation = new float();
            origin = new Vector2(spriteRectangle.Width / 2, spriteRectangle.Height / 2);

            body = new RotatedRectangle(new Rectangle(xPos - size / 2, yPos - size / 2, size, size), orientation);

            state = 0; //wander
            food = 0;
            water = waterCap;
            energy = energyCap;
            currSpeed = 1f;
            deathtimer = new TimeSpan(0, 0, 0);
            feedTimer = new TimeSpan(0, 0, 0);
            sprintTime = new TimeSpan(0, 0, 0);
            restTime = new TimeSpan(0, 0, 0);
            //Death
            corpseTimer = new TimeSpan(0, 0, 0);
            rotTime = 15;
            rotStage = 0;

            this.stamina = 6;

            this.mapWidth = 1920;   //hmm
            this.mapHeight = 1920;

            foodValue = 30;//Should be tweeked(size)

            randomGoal(mapWidth, mapHeight);
            //animation offset
            frameOffset = random.Next(4);


            //REMOVE THIS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            if(diet == 1)
                this.energyCap *= 5;
        }

        public void update(GameTime gameTime)
        {
            if (state == 4)
            {
                corpseTimer += gameTime.ElapsedGameTime;
                if (corpseTimer > TimeSpan.FromSeconds(rotTime) || foodValue < 1)
                    state = 6;
                else if (corpseTimer > TimeSpan.FromSeconds(rotTime*2/3) || foodValue < 10)
                    rotStage = 2;
                else if (corpseTimer > TimeSpan.FromSeconds(rotTime * 1 / 3) || foodValue < 20)
                    rotStage = 1;

            }
            else if (state == 1) // chase
            {
                this.sprintTime += gameTime.ElapsedGameTime;
                Chase(position, ref prey, ref orientation, agility);
                Vector2 heading = new Vector2((float)Math.Cos(orientation), (float)Math.Sin(orientation));

                if (currSpeed < speed)
                {
                    currSpeed += 0.03f * currSpeed;
                }

                position += heading * currSpeed;
            }
            else if (state == 0) // wander
            {
                this.restTime += gameTime.ElapsedGameTime;
                if (flora != null && this.diet == 0)
                {
                    goalPosition = flora.position;
                }
                else if (prey != null && this.diet == 1)
                {
                    goalPosition = prey.position;
                }

                Wander(position, ref goalPosition, ref orientation, agility);
                Vector2 heading = new Vector2(
                   (float)Math.Cos(orientation), (float)Math.Sin(orientation));

                if (currSpeed > 0.25f * speed)
                {
                    currSpeed -= 0.05f * currSpeed;
                }

                position += heading * currSpeed;
            }
            else if (state == 2) // evade
            {
                this.sprintTime += gameTime.ElapsedGameTime;
                Evade(position, ref predator, ref orientation, agility);
                Vector2 heading = new Vector2(
                   (float)Math.Cos(orientation), (float)Math.Sin(orientation));

                if (currSpeed < speed)
                {
                    currSpeed += 0.07f * currSpeed;
                }
                position += heading * currSpeed;
            }
            else if (state == 3) // feed
            {
                this.currSpeed = 0.25f * speed;
            }

            deathtimer += gameTime.ElapsedGameTime;

            // remove energy
            if (deathtimer > TimeSpan.FromSeconds(1))
            {
                this.energy -= 2;
                if (energy < 0)
                {
                    // kill the creature
                    this.state = 4;
                }
                deathtimer = TimeSpan.Zero;
            }

            float w = this.size * 0.01f * spriteRectangle.Width;
            float h = this.size * 0.01f * spriteRectangle.Height;
            
            body = new RotatedRectangle(new Rectangle((int)position.X, (int)position.Y, (int)w, (int)h), orientation);

            this.width = w;
            this.height = h;
        }

  

        private void Evade(Vector2 position, ref Creature pred, ref float orientation, float turnSpeed)
        {
            if (pred == null || pred.state == 4)
            {
                // it died
                this.state = 0;
                return;
            }
            Vector2 predPosition = pred.position; // bug here, because they can die now
            Vector2 seekPosition = 2 * position - predPosition; // optimal direction to run away (not very exciting)
            float distanceToGoal = Vector2.Distance(position, goalPosition);
            float distanceToPred = Vector2.Distance(position, pred.position);
            
            if (distanceToGoal < 200)
            {
                // assign a new random goal position
                randomGoal(mapWidth, mapHeight);
            }

            if (distanceToPred < 75)
            {
                // high priority choose optimal, can't have full turn speed
                orientation = TurnToFace(position, seekPosition, orientation, 3f * turnSpeed);
            }
            else
            {
                // choose random point to run to
                orientation = TurnToFace(position, goalPosition, orientation, 1.2f * turnSpeed);
            }
        }

        private void Chase(Vector2 position, ref Creature prey, ref float orientation, float turnSpeed)
        {
            if (prey == null || prey.state == 4)
            {
                this.state = 0;
                return;
            }
            Vector2 preyPosition = prey.position;
            
            // we may want to include a flocking algorithm so multiple predators dont get stuck behind the same prey
            /*
            if (prey.Predator != null)
            {
                Vector2 otherPredPosition = position - prey.Predator.position;
                orientation = TurnToFace(position, otherPredPosition, orientation, .5f * turnSpeed);
            }
            */

            orientation = TurnToFace(position, preyPosition, orientation, 0.75f * turnSpeed);
        }

        private void Wander(Vector2 position, ref Vector2 wanderDirection,
            ref float orientation, float turnSpeed)
        {
            float distanceFromGoal = Vector2.Distance(wanderDirection, position);
            if (distanceFromGoal < 50 && this.flora == null)
            {
                // new random goal position
                randomGoal(mapWidth, mapHeight);
            }

            orientation = TurnToFace(position, wanderDirection, orientation,
                .5f * turnSpeed);
        }

        public void randomGoal(int w, int h)
        {
            this.mapWidth = w;
            this.mapHeight = h;
            int i = 0;
            while (i == 0)
            {

                // new random goal position
                goalPosition.X = (float)random.Next(100,mapWidth-100);
                goalPosition.Y = (float)random.Next(100,mapHeight-100);

                Vector2 center = new Vector2(mapWidth / 2, mapHeight / 2);

                float minD = Math.Min(mapHeight / 2, mapWidth / 2);
                if (Vector2.Distance(goalPosition, center) < minD)
                {
                    i++;
                }
            }
            
        }

        public void avoid(Vector2 otherPosition)
        {
            // this is used to control crowding of creatures of the same type
            // works pretty well but a little choppy
            Vector2 seekPosition = position - otherPosition;

            seekPosition.Normalize();

            position += seekPosition * currSpeed;
        }

        public void turnToCenter(float distanceFromScreenCenter, Vector2 center, float maxDistance)
        {
            float normalizedDistance =
                distanceFromScreenCenter / maxDistance;

            float turnToCenterSpeed = 6f * normalizedDistance * normalizedDistance *
                this.agility;

            // once we've calculated how much we want to turn towards the center, we can
            // use the TurnToFace function to actually do the work.
            orientation = TurnToFace(position, center, orientation,
                turnToCenterSpeed);
        }

        /// <summary>
        /// Calculates the angle that an object should face, given its position, its
        /// target's position, its current angle, and its maximum turning speed.
        /// </summary>
        private static float TurnToFace(Vector2 position, Vector2 faceThis,
            float currentAngle, float turnSpeed)
        {

            float x = faceThis.X - position.X;
            float y = faceThis.Y - position.Y;

            float desiredAngle = (float)Math.Atan2(y, x);

            float difference = WrapAngle(desiredAngle - currentAngle);

            // clamp that between -turnSpeed and turnSpeed.
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            return WrapAngle(currentAngle + difference);
        }

        /// <summary>
        /// Returns the angle expressed in radians between -Pi and Pi.
        /// <param name="radians">the angle to wrap, in radians.</param>
        /// <returns>the input value expressed in radians from -Pi to Pi.</returns>
        /// </summary>
        private static float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }

        // mutator
        public void Feed(int value)
        {
            this.food += value;
        }

        public void draw(GameTime gameTime, SpriteBatch spriteBatch, Texture2D spriteSheet, Vector3 offset, int frame)
        {
            float sizeMod = 0.01f;
            if (this.parent.perks[7])
                sizeMod = 0.02f;
            if (state == 4)
            {
                spriteRectangle.X = 1600 + 150 * rotStage;
                spriteBatch.Draw(spriteSheet, new Vector2((int)(offset.Z * (position.X + offset.X)), (int)(offset.Z * (position.Y + offset.Y))), spriteRectangle, color, orientation, origin, 0.01f * size * offset.Z, SpriteEffects.None, 0.9f);
            }
            else
            {
                if (diet == 1)
                {
                    if (this.parent.perks[2] && this.parent.perks[3] && this.parent.perks[4])
                        spriteRectangle = Sprites.carnivoreTailEyesPincer;
                    else if (this.parent.perks[2] && this.parent.perks[3])
                        spriteRectangle = Sprites.carnivoreTailPincer;
                    else if (this.parent.perks[2] && this.parent.perks[4])
                        spriteRectangle = Sprites.carnivoreEyesPincer;
                    else if (this.parent.perks[3] && this.parent.perks[4])
                        spriteRectangle = Sprites.carnivoreTailEyes;
                    else if (this.parent.perks[4])
                        spriteRectangle = Sprites.carnivoreEyes;
                    else if (this.parent.perks[2])
                        spriteRectangle = Sprites.carnivorePincer;
                    else if (this.parent.perks[3])
                        spriteRectangle = Sprites.carnivoreTail;
                    else
                        spriteRectangle = Sprites.carnivore;


                }
                else
                {

                    if (this.parent.perks[2] && this.parent.perks[3] && this.parent.perks[4])
                        spriteRectangle = Sprites.herbivoreTailEyesPincer;
                    else if (this.parent.perks[2] && this.parent.perks[3])
                        spriteRectangle = Sprites.herbivoreTailPincer;
                    else if (this.parent.perks[2] && this.parent.perks[4])
                        spriteRectangle = Sprites.herbivoreEyesPincer;
                    else if (this.parent.perks[3] && this.parent.perks[4])
                        spriteRectangle = Sprites.herbivoreTailEyes;
                    else if (this.parent.perks[4])
                        spriteRectangle = Sprites.herbivoreEyes;
                    else if (this.parent.perks[2])
                        spriteRectangle = Sprites.herbivorePincer;
                    else if (this.parent.perks[3])
                        spriteRectangle = Sprites.herbivoreTail;
                    else
                        spriteRectangle = Sprites.herbivore;
                }


                spriteRectangle.X = 1001 + 150 * ((frame + frameOffset) % 4);
                spriteBatch.Draw(spriteSheet, new Vector2((int)(offset.Z * (position.X + offset.X)), (int)(offset.Z * (position.Y + offset.Y))), spriteRectangle, color, orientation, origin, sizeMod * size * offset.Z, SpriteEffects.None, 0.9f);
            }
        }     


    }
}
