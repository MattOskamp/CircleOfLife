﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Nuclex.UserInterface;
using Nuclex.Input;
using Nuclex.Game;


namespace CircleOfLife
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class CircleOfLifeGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        //graphics fields
        Texture2D spriteSheet;
        Texture2D bushTexture;

        Ecosystem ecosystem;
        User user;


        //Map coordinates: these variables should be moved to a more appropriate class..eventually
        //Vector2 mapSize;    //not used currently

        //temporary variable for implementing map scrolling
        public Vector2 userView;
        public bool scrollLeft = false;
        public bool scrollRight = false;
        public bool scrollDown = false;
        public bool scrollUp = false;

        public CircleOfLifeGame()
        {
            graphics = new GraphicsDeviceManager(this);
          
            //set resolution
            //graphics.PreferredBackBufferWidth = 480;
            //graphics.PreferredBackBufferHeight = 800;
            graphics.IsFullScreen = true;

            //initialize
            userView = new Vector2(0, 0);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {


            //Initialize ecosystem
            ecosystem = new Ecosystem(this);
            //Initialize user interface system
            user = new User(this, ecosystem);



            base.Initialize();
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            spriteSheet = Content.Load<Texture2D>("test_sheet2");
            //predatorTexture = Content.Load<Texture2D>("dragon");
            bushTexture = Content.Load<Texture2D>("bush");

            // set up species in here for now
            Ecosystem.speciesStats preyStats = new Ecosystem.speciesStats();
            preyStats.diet = 0;
            preyStats.size = 5;
            preyStats.detection = 100;
            preyStats.speed = 7;
            preyStats.energyCap = 100;
            preyStats.foodCap = 100;
            preyStats.waterCap = 100;
            preyStats.energyValue = 20;
            preyStats.agility = 0.15f;
           // ecosystem.addSpecies("mouse", preyStats, spriteSheet);

            Ecosystem.speciesStats predStats = new Ecosystem.speciesStats();
            predStats.diet = 1;
            predStats.size = 10;
            predStats.detection = 150;
            predStats.speed = 5;
            predStats.energyCap = 100;
            predStats.foodCap = 100;
            predStats.waterCap = 100;
            predStats.energyValue = 50;
            predStats.agility = 0.15f;
           // ecosystem.addSpecies("cat", predStats, spriteSheet);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            ecosystem.Update(gameTime);


            //Navigation scrolling section
            //the values need to be tuned to make scrolling smooth
            if (scrollLeft)
                userView.X += 3.0f * (gameTime.ElapsedGameTime.Ticks / 100000.0f);
            if (scrollRight)
                userView.X -= 3.0f * (gameTime.ElapsedGameTime.Ticks / 100000.0f);
            if (scrollUp)
                userView.Y += 3.0f * (gameTime.ElapsedGameTime.Ticks / 100000.0f);
            if (scrollDown)
                userView.Y -= 3.0f * (gameTime.ElapsedGameTime.Ticks / 100000.0f);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            ecosystem.draw(gameTime, spriteBatch, spriteSheet,userView);
            spriteBatch.Draw(spriteSheet, new Rectangle(0, 0, 640, 640), new Rectangle(0, 100, 1000, 1000), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

    }



}

