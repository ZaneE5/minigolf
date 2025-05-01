using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BEPUphysics.Entities;
using BEPUphysics;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Vector3 = BEPUutilities.Vector3;
using Matrix = Microsoft.Xna.Framework.Matrix;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.Xna.Framework.Audio;

//Base code built off BEPU physics Getting-Started Demo, including ModelDataExtractor.cs, Camera.cs, EntityModel.cs, and StaticModel.cs
//All wav files from freesound, and all under Creative Commons 0 (available for public use without any licensing or crediting)
//Golf assets from kenney golf assets posted on Piazza

namespace minigolf
{
    public class Minigolf : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Space space;
        public Model ballModel;
        public Model ball2Model;
        public Model track1;
        public Vector3 track1SpawnPos;
        public Model track2;
        public Vector3 track2SpawnPos;

        public KeyboardState KeyboardState;
        public KeyboardState oldKeyboardState;

        public MouseState MouseState;
        private float hitPower;
        private float MAX_HIT_POWER;
        private int hitPowerChange;
        private float hitPowerPerFrame;
        private Texture2D hitPowerDisplayBall;

        public SpriteBatch _spriteBatch;
        public SpriteFont font;

        private int ball1State;
        private Sphere player1Ball;
        private int player1Strokes1;
        private int player1Strokes2;

        private int ball2State;
        private Sphere player2Ball;
        private int player2Strokes1;
        private int player2Strokes2;

        private int currentPlayer;
        private int numPlayers;

        private bool hasWon;
        private Dictionary<string, int> bestScores;
        private string currentInitials;
        private Keys one;
        private Keys two;
        private Keys three;

        private SoundEffect hitSound;
        private SoundEffectInstance hitSoundInstance;
        private SoundEffect collisionSound;
        private SoundEffect holeSound;
        private Vector3 camSpawnPos;

        public Minigolf()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            //Setup the camera.
            camSpawnPos = new Vector3(0, 50, 50);
            Services.AddService(new Camera(this, camSpawnPos, 10));
            bestScores = new Dictionary<string, int>();
            hitPower = 0;
            MAX_HIT_POWER = 100;
            hitPowerChange = 1;
            hitPowerPerFrame = 5f;
            player1Strokes1 = 0;
            player1Strokes2 = 0;
            ball1State = 0;
            hasWon = false;
            currentInitials = "";
            currentPlayer = 1;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("File");
            hitPowerDisplayBall = Content.Load<Texture2D>("Sprites\\ball-red");
            ballModel = Content.Load<Model>("Models\\ball-blue");
            ball2Model = Content.Load<Model>("Models\\ball-red");

            track1 = Content.Load<Model>("Models\\track1");
            track2 = Content.Load<Model>("Models\\track2");

            hitSound = Content.Load<SoundEffect>("Audio\\putt");
            hitSoundInstance = hitSound.CreateInstance();
            hitSoundInstance.Volume = 0.5f;
            collisionSound = Content.Load<SoundEffect>("Audio\\collision");
            holeSound = Content.Load<SoundEffect>("Audio\\hole");

            space = new Space();
            space.ForceUpdater.Gravity = new Vector3(0, -20f, 0);

            Vector3[] vertices;
            int[] indices;

            ModelDataExtractor.GetVerticesAndIndicesFromModel(track1, out vertices, out indices);
            var mesh = new StaticMesh(vertices, indices, new AffineTransform(new Vector3(0, 0, 0)));
            space.Add(mesh);
            Components.Add(new StaticModel(track1, mesh.WorldTransform.Matrix, this));

            ModelDataExtractor.GetVerticesAndIndicesFromModel(track2, out vertices, out indices);
            track2SpawnPos = new Vector3(150, 0, 0);
            mesh = new StaticMesh(vertices, indices, new AffineTransform(track2SpawnPos));
            space.Add(mesh);
            Components.Add(new StaticModel(track2, mesh.WorldTransform.Matrix, this));

            track1SpawnPos = new Vector3(0, 10, 0);
        }

        void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            var otherEntityInformation = other as EntityCollidable;
            if (otherEntityInformation != null)
            {
                collisionSound.Play();
            }
        }

        protected void HandleInput()
        {
            if (numPlayers != 0)
            {
                if (KeyboardState.IsKeyUp(Keys.Space))
                {
                    if (oldKeyboardState.IsKeyDown(Keys.Space))
                    {
                        hitSound.Play();
                        Vector3 hitDirection;
                        switch (currentPlayer)
                        {
                            case 1:
                                hitDirection = player1Ball.Position - Services.GetService<Camera>().Position;
                                hitDirection.Y = 0;
                                player1Ball.AngularVelocity = new Vector3(0, 0, 0);
                                player1Ball.LinearVelocity = Vector3.Normalize(hitDirection) * hitPower;
                                if (ball1State == 1)
                                {
                                    player1Strokes1 += 1;
                                }
                                if (ball1State == 2)
                                {
                                    player1Strokes2 += 1;
                                }
                                if (numPlayers == 2)
                                {
                                    currentPlayer = 2;
                                }
                                break;
                            case 2:
                                hitDirection = player2Ball.Position - Services.GetService<Camera>().Position;
                                hitDirection.Y = 0;
                                player2Ball.AngularVelocity = new Vector3(0, 0, 0);
                                player2Ball.LinearVelocity = Vector3.Normalize(hitDirection) * hitPower;
                                if (ball2State == 1)
                                {
                                    player2Strokes1 += 1;
                                }
                                if (ball2State == 2)
                                {
                                    player2Strokes2 += 1;
                                }
                                currentPlayer = 1;
                                break;
                            default:
                                break;
                        }
                        hitPower = 0;
                    }
                }
                else
                {
                    if (hitPower >= MAX_HIT_POWER) {
                        hitPowerChange = -1;
                    }
                    if (hitPower <= 0) {
                        hitPowerChange = 1;
                    }
                    hitPower += hitPowerPerFrame * hitPowerChange;
                    hitPower = Math.Clamp(hitPower, 0, MAX_HIT_POWER);
                }
                oldKeyboardState = KeyboardState;
            }
        }

        protected bool CheckIfWonFirstCourse(Entity ball)
        {
            if (ball.Position.Y >= 56 && ball.Position.Y <= 57
                && ball.Position.X <= -195 && ball.Position.X >= -205
                && ball.Position.Z <= -195 && ball.Position.Z >= -205)
            {
                track2SpawnPos = new Vector3(100, 0, 0);
                holeSound.Play();
                ball.Position = track2SpawnPos + new Vector3(50, 50, 0);
                ball.LinearVelocity = new Vector3(0, 0, 0);
                ball.AngularVelocity = new Vector3(0, 0, 0);
                return true;
            }
            return false;
        }

        protected bool CheckIfWonSecondCourse(Entity ball)
        {
            if (ball.Position.Y <= -43 && ball.Position.Y >= -44 && ball.Position.X <= 155 && ball.Position.X >= 145 && ball.Position.Z <= -395 && ball.Position.Z >= -405)
            {
                holeSound.Play();
                return true;
            }
            return false;
        }

        protected void ResetPlayer1()
        {
            Services.GetService<Camera>().Position = camSpawnPos;
            player1Ball.Position = track1SpawnPos;
            player1Ball.LinearVelocity = new Vector3(0, 0, 0);
            player1Ball.AngularVelocity = new Vector3(0, 0, 0);
            player1Strokes1 = 0;
            player1Strokes2 = 0;
            ball1State = 1;
            hasWon = false;
            currentInitials = "";
        }
        protected void ResetPlayer2()
        {
            Services.GetService<Camera>().Position = camSpawnPos;
            player2Ball.Position = track1SpawnPos;
            player2Ball.LinearVelocity = new Vector3(0, 0, 0);
            player2Ball.AngularVelocity = new Vector3(0, 0, 0);
            player2Strokes1 = 0;
            player2Strokes2 = 0;
            ball2State = 1;
            hasWon = false;
            currentInitials = "";
        }

        protected void LoadBalls()
        {
            Sphere ball = new Sphere(track1SpawnPos, 3, 5);
            space.Add(ball);
            player1Ball = ball;
            Components.Add(new EntityModel(ball, ballModel, ConversionHelper.MathConverter.Convert(Matrix.Identity), this));
            ball.CollisionInformation.Events.InitialCollisionDetected += HandleCollision;

            if (numPlayers == 2)
            {
                Sphere ball2 = new Sphere(track1SpawnPos, 3, 5);
                space.Add(ball2);
                player2Ball = ball2;
                Components.Add(new EntityModel(ball2, ball2Model, ConversionHelper.MathConverter.Convert(Matrix.Identity), this));
                ball2.CollisionInformation.Events.InitialCollisionDetected += HandleCollision;
            }
        }

        protected void Player1StateMachine()
        {
            switch (ball1State)
            {
                case 1:
                    if (CheckIfWonFirstCourse(player1Ball))
                    {
                        ball1State = 2;
                    }
                    break;
                case 2:
                    if (CheckIfWonSecondCourse(player1Ball))
                    {
                        ball1State = 3;
                        hasWon = true;
                    }
                    break;
                case 3: //get first character
                    //check for character input
                    if (KeyboardState.GetPressedKeys().Count() != 0)
                    {
                        ball1State = 4;
                        one = KeyboardState.GetPressedKeys()[0];
                        currentInitials += one.ToString();
                    }
                    break;
                case 4: //transition state to wait for lack of key input
                    if (KeyboardState.GetPressedKeys().Count() == 0)
                    {
                        ball1State = 5;
                    }
                    break;
                case 5: //get second character
                    //make sure it goes back to zero state
                    if (KeyboardState.GetPressedKeys().Count() != 0)
                    {
                        ball1State = 6;
                        two = KeyboardState.GetPressedKeys()[0];
                        currentInitials += two.ToString();
                    }
                    break;
                case 6: //transition state to wait for lack of key input
                    if (KeyboardState.GetPressedKeys().Count() == 0)
                    {
                        ball1State = 7;
                    }
                    break;
                case 7: //get third character
                    if (KeyboardState.GetPressedKeys().Count() != 0)
                    {
                        ball1State = 8;
                        three = KeyboardState.GetPressedKeys()[0];
                        currentInitials += three.ToString();
                    }
                    break;
                case 8:
                    //store initials
                    bestScores.Add(currentInitials, player1Strokes1 + player1Strokes2);
                    //this sorting from stack overflow
                    var sortedDict = from entry in bestScores orderby entry.Value ascending select entry;
                    bestScores = sortedDict.Take(3).ToDictionary();
                    ResetPlayer1();
                    break;
                default:
                    break;
            }
        }

        protected void Player2StateMachine()
        {
            switch (ball2State)
            {
                case 1:
                    if (CheckIfWonFirstCourse(player2Ball))
                    {
                        ball2State = 2;
                    }
                    break;
                case 2:
                    if (CheckIfWonSecondCourse(player2Ball))
                    {
                        ball2State = 3;
                        hasWon = true;
                    }
                    break;
                case 3: //get first character
                    //check for character input
                    if (KeyboardState.GetPressedKeys().Count() != 0)
                    {
                        ball2State = 4;
                        one = KeyboardState.GetPressedKeys()[0];
                        currentInitials += one.ToString();
                    }
                    break;
                case 4: //transition state to wait for lack of key input
                    if (KeyboardState.GetPressedKeys().Count() == 0)
                    {
                        ball2State = 5;
                    }
                    break;
                case 5: //get second character
                    //make sure it goes back to zero state
                    if (KeyboardState.GetPressedKeys().Count() != 0)
                    {
                        ball2State = 6;
                        two = KeyboardState.GetPressedKeys()[0];
                        currentInitials += two.ToString();
                    }
                    break;
                case 6: //transition state to wait for lack of key input
                    if (KeyboardState.GetPressedKeys().Count() == 0)
                    {
                        ball2State = 7;
                    }
                    break;
                case 7: //get third character
                    if (KeyboardState.GetPressedKeys().Count() != 0)
                    {
                        ball2State = 8;
                        three = KeyboardState.GetPressedKeys()[0];
                        currentInitials += three.ToString();
                    }
                    break;
                case 8:
                    //store initials
                    bestScores.Add(currentInitials, player2Strokes1 + player2Strokes2);
                    //this sorting from StackOverflow
                    var sortedDict = from entry in bestScores orderby entry.Value ascending select entry;
                    bestScores = sortedDict.Take(3).ToDictionary();
                    ResetPlayer2();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
            // Allows the game to exit
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
                return;
            }
            //Update the camera.
            Services.GetService<Camera>().Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            HandleInput();

            switch (ball1State)
            {
                case 0:
                    if (KeyboardState.IsKeyDown(Keys.NumPad1))
                    {
                        numPlayers = 1;
                        ball1State = 1;
                        LoadBalls();
                    }
                    else if (KeyboardState.IsKeyDown(Keys.NumPad2))
                    {
                        numPlayers = 2;
                        ball1State = 1;
                        ball2State = 1;
                        LoadBalls();
                    }
                    break;
                default:
                    break;
            }

            Player1StateMachine();
            if (numPlayers == 2)
            {
                Player2StateMachine();
            }

            //Steps the simulation forward one time step.
            space.Update();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                null,
                DepthStencilState.Default, // Preserve depth testing
                RasterizerState.CullNone
            );

            if (hitPower > 0)
            {
                _spriteBatch.Draw(hitPowerDisplayBall, new Microsoft.Xna.Framework.Vector2(700, 400 - hitPower), Color.Red);
            }

            string strokeCount;
            if (currentPlayer == 1)
            {
                strokeCount = "Player 1 Total Strokes: " + (player1Strokes1 + player1Strokes2);
            }
            else
            {
                strokeCount = "Player 2 Total Strokes: " + (player2Strokes1 + player2Strokes2);
            }
            switch (currentPlayer)
            {
                case 1:
                    if (ball1State == 1)
                    {
                        strokeCount += "\nPlayer 1 Strokes on this hole: " + player1Strokes1;
                    }
                    else if (ball1State == 2)
                    {
                        strokeCount += "\nPlayer 1 Strokes on this hole: " + player1Strokes2;
                    }
                    break;
                case 2:
                    if (ball1State == 1)
                    {
                        strokeCount += "\nPlayer 2 Strokes on this hole: " + player2Strokes1;
                    }
                    else if (ball1State == 2)
                    {
                        strokeCount += "\nPlayer 2 Strokes on this hole: " + player2Strokes2;
                    }
                    break;
                default:
                    break;
            }

            Microsoft.Xna.Framework.Vector2 position = new Microsoft.Xna.Framework.Vector2(50, 50);
            Color color = Color.Black;
            _spriteBatch.DrawString(font, strokeCount, position, color);
            _spriteBatch.DrawString(font, "Initials  Score", new Microsoft.Xna.Framework.Vector2(500, 50), color);

            int rowHeight = 20;
            for (int i = 0; i < Math.Min(bestScores.Count, 3); i++)
            {
                _spriteBatch.DrawString(font, bestScores.ElementAt(i).Key + "            " + bestScores.ElementAt(i).Value, new Microsoft.Xna.Framework.Vector2(500, 75 + rowHeight * i), color);
            }

            if (hasWon)
            {
                _spriteBatch.DrawString(font, "Enter Initials: " + currentInitials, new Microsoft.Xna.Framework.Vector2(200, 200), color);
            }

            if (numPlayers == 0)
            {
                _spriteBatch.DrawString(font, "Enter number of players: " + numPlayers, new Microsoft.Xna.Framework.Vector2(200, 200), color);
                _spriteBatch.DrawString(font, "(Hold) Space to hit", new Microsoft.Xna.Framework.Vector2(200, 100), color);
                _spriteBatch.DrawString(font, "WASD to move camera", new Microsoft.Xna.Framework.Vector2(200, 120), color);
                _spriteBatch.DrawString(font, "Up/Down to adjust camera height", new Microsoft.Xna.Framework.Vector2(200, 140), color);
                _spriteBatch.DrawString(font, "Hit Direction is away from camera position", new Microsoft.Xna.Framework.Vector2(200, 160), color);
            }

            _spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}