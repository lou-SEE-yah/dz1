﻿using ClassLibrary1;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using System.Linq;

namespace Pong
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public List<Wall> Walls { get; set; }
        public List<Wall> Goals { get; set; }

        /// <summary >
        /// Bottom paddle object
        /// </ summary >
        public Paddle PaddleBottom { get; private set; }

        /// <summary >
        /// Top paddle object
        /// </ summary >
        public Paddle PaddleTop { get; private set; }

        /// <summary >
        /// Ball object
        /// </ summary >
        public Ball Ball { get; private set; }

        /// <summary >
        /// Background image
        /// </ summary >
        public Background Background { get; private set; }

        /// <summary >
        /// Sound when ball hits an obstacle .
        /// SoundEffect is a type defined in Monogame framework
        /// </ summary >
        public SoundEffect HitSound { get; private set; }

        /// <summary >
        /// Background music . Song is a type defined in Monogame framework
        /// </ summary >
        public Song Music { get; private set; }

        /// <summary >
        /// Generic list that holds Sprites that should be drawn on screen
        /// </ summary >
        private IGenericList<Sprite> SpritesForDrawList = new GenericList<Sprite>();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = 700,
                PreferredBackBufferWidth = 500
            };
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
            // Screen bounds details . Use this information to set up game objects positions.
            var screenBounds = GraphicsDevice.Viewport.Bounds;
            PaddleBottom = new Paddle(GameConstants.PaddleDefaultWidth, GameConstants.PaddleDefaulHeight, GameConstants.PaddleDefaulSpeed);
            PaddleBottom.X =250 - GameConstants.PaddleDefaultWidth/2;//<PaddleBottom X Position>;
            PaddleBottom.Y = 700 - GameConstants.PaddleDefaulHeight;// <PaddleBottom Y Position>;
            PaddleTop = new Paddle(GameConstants.PaddleDefaultWidth, GameConstants.PaddleDefaulHeight, GameConstants.PaddleDefaulSpeed);
            PaddleTop.X = 250 - GameConstants.PaddleDefaultWidth / 2;//< PaddleTop X Position >;
            PaddleTop.Y = 0;//< PaddleTop Y Position >;
            Ball = new Ball(GameConstants.DefaultBallSize, GameConstants.DefaultInitialBallSpeed, GameConstants.DefaultBallBumpSpeedIncreaseFactor)
            {
                X = 240, //< Ball center X Position >
                Y = 340 //< Ball center Y Position >
            };
            Background = new Background(500, 700/*< width of the screen bounds >, < height of the screen bounds >*/);

            // Add our game objects to the sprites that should be drawn collection .. you ’ll see why in a second
            SpritesForDrawList.Add(Background);
            SpritesForDrawList.Add(PaddleBottom);
            SpritesForDrawList.Add(PaddleTop);
            SpritesForDrawList.Add(Ball);
            Walls = new List<Wall>()
            {
                new Wall (-GameConstants.WallDefaultSize, 0, GameConstants.WallDefaultSize, screenBounds.Height),
                new Wall (screenBounds.Right, 0, GameConstants.WallDefaultSize, screenBounds.Height),
            };
            Goals = new List<Wall>()
            {
                new Wall (0, screenBounds.Height, screenBounds.Width , GameConstants.WallDefaultSize ),
                new Wall ( screenBounds.Top, -GameConstants.WallDefaultSize, screenBounds.Width, GameConstants.WallDefaultSize),
            };
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Initialize new SpriteBatch object which will be used to draw textures .
            spriteBatch = new SpriteBatch(GraphicsDevice);
            // Set textures
            Texture2D paddleTexture = Content.Load<Texture2D>("paddle");
            PaddleBottom.Texture = paddleTexture;
            PaddleTop.Texture = paddleTexture;
            Ball.Texture = Content.Load<Texture2D>("ball");
            Background.Texture = Content.Load<Texture2D>("background");
            // Load sounds
            // Start background music
            HitSound = Content.Load<SoundEffect>("hit");
            //Music = Content.Load<Song>("music");
            MediaPlayer.IsRepeating = true;
            //MediaPlayer.Play(Music);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var touchState = Keyboard.GetState();
            if (touchState.IsKeyDown(Keys.Left))
                PaddleBottom.X = PaddleBottom.X - (float)(PaddleBottom.Speed * gameTime.ElapsedGameTime.TotalMilliseconds);
            if (touchState.IsKeyDown(Keys.Right))
                PaddleBottom.X = PaddleBottom.X + (float)(PaddleBottom.Speed * gameTime.ElapsedGameTime.TotalMilliseconds);
            
            PaddleBottom.X = MathHelper.Clamp(PaddleBottom.X, 0, 500 - PaddleBottom.Width);

            var touchState2 = Keyboard.GetState();
            if (touchState2.IsKeyDown(Keys.A))
                PaddleTop.X = PaddleTop.X - (float)(PaddleTop.Speed * gameTime.ElapsedGameTime.TotalMilliseconds);
            if (touchState2.IsKeyDown(Keys.D))
                PaddleTop.X = PaddleTop.X + (float)(PaddleTop.Speed * gameTime.ElapsedGameTime.TotalMilliseconds);
            
            PaddleTop.X = MathHelper.Clamp(PaddleTop.X, 0, 500 - PaddleTop.Width);

            var ballPositionChange = Ball.Direction * (float)(gameTime.ElapsedGameTime.TotalMilliseconds * Ball.Speed);

            Ball.X += ballPositionChange.X;
            Ball.Y += ballPositionChange.Y;

            // Ball - side walls
            if (Walls.Any(w => CollisionDetector.Overlaps(Ball, w)))
            {
                Ball.Direction = new Vector2(-Ball.Direction.X, Ball.Direction.Y);
                Ball.Speed = Ball.Speed * Ball.BumpSpeedIncreaseFactor;
            }
            // Ball - winning walls
            if (Goals.Any(w => CollisionDetector.Overlaps(Ball, w)))
            {
                Ball.X = 240;//bounds.Center.ToVector2().X;
                Ball.Y = 400;//bounds.Center.ToVector2().Y;
                Ball.Speed = GameConstants.DefaultInitialBallSpeed;
                HitSound.Play();
            }
            // Paddle - ball collision
            if (CollisionDetector.Overlaps(Ball, PaddleTop) && Ball.Direction.Y < 0
            || (CollisionDetector.Overlaps(Ball, PaddleBottom) && Ball.Direction.Y > 0))
            {
                Ball.Direction = new Vector2(Ball.Direction.X, -Ball.Direction.Y);
                Ball.Speed *= Ball.BumpSpeedIncreaseFactor;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Start drawing .
            spriteBatch.Begin();
            for (int i = 0; i < SpritesForDrawList.Count; i++)
            {
                SpritesForDrawList.GetElement(i).DrawSpriteOnScreen(spriteBatch);
            }
            // End drawing .
            // Send all gathered details to the graphic card in one batch .

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
