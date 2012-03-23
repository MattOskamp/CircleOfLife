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
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        
        //graphics fields
        public Texture2D spriteSheet;
        Texture2D bushTexture;

        Ecosystem ecosystem;
        User user;


        //Map coordinates: these variables should be moved to a more appropriate class..eventually..perhaps
        public int mapSizeX;    
        public int mapSizeY;

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
            graphics.PreferredBackBufferWidth = 1440;
            graphics.PreferredBackBufferHeight = 960;
            //map size is initially twice screen size
            mapSizeX = 2880;
            mapSizeY = 1920;
            graphics.IsFullScreen = false;
            //initialize
            userView = new Vector2(-mapSizeX / 4, -mapSizeY /4);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        /// <summary>
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

            spriteSheet = Content.Load<Texture2D>("spriteSheet");
            bushTexture = Content.Load<Texture2D>("bush");

            user.initializeGameScreen();
        }

        void initializeGameComponents()
        {
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
            if (scrollLeft && userView.X < 0)
                userView.X += 3.0f * (gameTime.ElapsedGameTime.Ticks / 100000.0f);
            if (scrollRight && userView.X > -mapSizeX/2)
                userView.X -= 3.0f * (gameTime.ElapsedGameTime.Ticks / 100000.0f);
            if (scrollUp && userView.Y < 0)
                userView.Y += 3.0f * (gameTime.ElapsedGameTime.Ticks / 100000.0f);
            if (scrollDown && userView.Y > -mapSizeY/2)
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
            spriteBatch.Draw(spriteSheet, new Rectangle((int)userView.X, (int)userView.Y, mapSizeX, mapSizeY), new Rectangle(0, 0, 1000, 1000), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            user.drawHUD(gameTime, spriteBatch, spriteSheet);
            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}

