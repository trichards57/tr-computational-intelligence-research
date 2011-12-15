using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MultiAgentLibrary;

namespace MultiAgentUI
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        private GameTime lastTime;
        private Field field;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Texture2D square;
        private Texture2D agent;

        private const int memoryLength = 16;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            field = new Field(100, 100, @"C:\Users\Tony\Documents\tr-computational-intelligence-research\sensorData.csv");

            for (var i = 0; i < 10; i++)
                field.AgentsList.Add(new Agent(field.StartPoint, memoryLength));

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

            square = Content.Load<Texture2D>("square");
            agent = Content.Load<Texture2D>("agent");

            // TODO: use this.Content to load your game content here
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (field.AgentsList.Count < 250)
                field.AgentsList.Add(new Agent(field.StartPoint, memoryLength));

            field.CycleAgents();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            Debug.WriteLine("Frame time {0}", 1 / gameTime.ElapsedGameTime.TotalSeconds);

            spriteBatch.Begin();

            var xScale = Window.ClientBounds.Width / field.Width;
            var yScale = Window.ClientBounds.Height / field.Height;

            xScale = yScale = Math.Min(xScale, yScale);

            foreach (var a in field.AgentsList)
            {
                spriteBatch.Draw(agent, new Rectangle(a.Position.X * xScale, a.Position.Y * yScale, xScale, yScale), Color.Yellow);
            }

            foreach (var s in field.Squares)
            {
                Color c;
                switch (s.SquareType)
                {
                    case SquareType.Wall:
                        c = Color.Red;
                        break;
                    case SquareType.Destination:
                        c = Color.White;
                        break;
                    case SquareType.Passable:
                        var col = s.SquareColour;
                        c = new Color(col.R, col.G, col.B, 50);
                        break;
                    default:
                        continue;
                }
                spriteBatch.Draw(square, new Rectangle(s.Position.X * xScale, s.Position.Y * yScale, xScale, yScale), c);
            }

            

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
