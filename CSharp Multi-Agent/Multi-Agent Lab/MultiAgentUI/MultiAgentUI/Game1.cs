using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MultiAgentLibrary;

namespace MultiAgentUI
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        private Field field;

        SpriteBatch spriteBatch;

        private Texture2D square;
        private Texture2D agent;

        private const int MemoryLength = 16;

        public Game1()
        {
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
            const string cacheFile = "cache.xml";
            const string dataFile = @"C:\Users\Tony\Documents\tr-computational-intelligence-research\sensorData.csv";
            const int mapWidth = 100;
            const int mapHeight = 100;

            if (File.Exists(cacheFile) && File.GetLastWriteTimeUtc(cacheFile) > File.GetLastWriteTimeUtc(dataFile))
            {
                Console.WriteLine("Loading data from cache...");
                var deserializer = new XmlSerializer(typeof(Field));
                var stream = XmlReader.Create(cacheFile);
                field = (Field)deserializer.Deserialize(stream);
                stream.Close();
            }
            else
            {
                field = new Field(mapWidth, mapHeight, dataFile);
                Console.WriteLine("Caching field data...");
                var serializer = new XmlSerializer(typeof(Field));
                var stream = XmlWriter.Create(cacheFile);
                serializer.Serialize(stream, field);
                stream.Close();
            }

            for (var i = 0; i < 10; i++)
                field.AgentsList.Add(new Agent(field.StartPoint, MemoryLength));

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
                Exit();

            if (field.AgentsList.Count < 250)
                field.AgentsList.Add(new Agent(field.StartPoint, MemoryLength));

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
