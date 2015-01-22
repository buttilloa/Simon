using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Simon
{
    public enum SimonColors {GREEN, RED, YELLOW, BLUE, NONE};
    public enum Turn { PLAYER, COMPUTER, PLAYBACK, GAMEOVER,YOUSUCK};
    

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D board;
        Texture2D simon;
        Texture2D cursor;
        Texture2D yousuck;
        SpriteFont font;
        Random rand;
        Turn turn = Turn.COMPUTER;
        bool isPlaybacking = true;
        bool hasclicked = false;
        int timer = 0;
        List<SimonColors> moves;   // Hint
        int PlayBackIndex = 0;  // Index into moves list
        int PlayerTurnIndex = 0; // When it's the player's turn, you can use this to store what move the player is on
        //MouseState oldstate;
        SimonColors Lit = SimonColors.NONE;  // Which button is currently lit up?

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            graphics.ApplyChanges();

            Content.RootDirectory = "Content";

            rand = new Random(System.Environment.TickCount);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

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

            board = Content.Load<Texture2D>("board");
            simon = Content.Load<Texture2D>("simon");
            cursor = Content.Load<Texture2D>("cursor");
            font = Content.Load<SpriteFont>("pericles14");
            yousuck = Content.Load<Texture2D>("yousuck");

            SoundManager.Initialize(Content);

            moves = new List<SimonColors>();

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

            // TODO: Add your update logic here

            if (turn == Turn.YOUSUCK)
            {
                timer++;
                if (timer >= 120)
                {
                    timer = 0;
                    turn = Turn.COMPUTER;
                }
            }
            else if (turn == Turn.COMPUTER)
            {
                // TODO: After 1 second add a random move
                //for(int i =0; i < LitCount;i++)
                moves.Add((SimonColors)rand.Next(0, 4));
               
                turn = Turn.PLAYBACK;
                PlayBackIndex = 0;
                PlayerTurnIndex = 0;
                isPlaybacking = true;
            }
            else if (turn == Turn.PLAYBACK)
            {
                timer++;
                if (isPlaybacking)
                {
                    
                    if (timer >= 60)
                    {
                        timer = 0;

                        Lit = moves[PlayBackIndex];
                        //if(PlayBackIndex != moves.Count-1)
                        PlayBackIndex++;
                        SoundManager.PlaySimonSound(Lit);
                    }
                }
                //Lit = SimonColors.NONE;
                if (PlayBackIndex == moves.Count)
                {
                    isPlaybacking = false;
                    if (timer >= 60)
                    {
                        timer = 0;
                        Lit = SimonColors.NONE;
                        turn = Turn.PLAYER;

                        PlayerTurnIndex = 0;

                    }
                }
            }
            else if (turn == Turn.PLAYER)
            {
                MouseState ms = Mouse.GetState();

                if (ms.LeftButton == ButtonState.Released) hasclicked = false;
                if (ms.LeftButton == ButtonState.Pressed && !hasclicked)
                {
                    // Check to see if green button is hit.. add code to make sure the mouse button is depressed so you
                    // don't respond to this buttonpress twice in a roy
                    Lit = getPressed();

                    if (Lit != SimonColors.NONE)
                    {
                        hasclicked = true;
                        if (Lit == moves[PlayerTurnIndex] && PlayerTurnIndex != moves.Count - 1)
                        {
                            PlayerTurnIndex++;

                        }
                        else if (Lit != moves[PlayerTurnIndex])
                        {
                            SoundManager.PlayGameOver();
                            turn = Turn.YOUSUCK;

                            Lit = SimonColors.NONE;
                            moves.Clear();
                        }
                        else if (PlayerTurnIndex == moves.Count - 1)
                        {
                            turn = Turn.COMPUTER;
                            //Lit = SimonColors.NONE;
                        }
                        SoundManager.PlaySimonSound(Lit);
                    }
                }

            }
            else if (turn == Turn.GAMEOVER)
            {
                SoundManager.PlayGameOver();

                moves.Clear();
                turn = Turn.COMPUTER;
                Lit = SimonColors.NONE;
            }

            base.Update(gameTime);
        }

        // point is the on-screen mouse coordinate where a click occurred
        // destination is a rectangle where this sprite will be drawn
        // source is a rectangle from the simon spritesheet
        // 
        // Returns TRUE if the mouse click was on a non-transparent pixel
        public bool isPressed(Texture2D texture, Rectangle destination, Rectangle source)
        {
            MouseState ms = Mouse.GetState();
            Vector2 point = new Vector2(ms.X, ms.Y);
            uint[] PixelData = new uint[texture.Width * texture.Height];

            texture.GetData<uint>(0, new Rectangle(0,0,texture.Width,texture.Height), PixelData, 0, texture.Width*texture.Height);

            Vector2 point_translated = point - new Vector2(destination.X, destination.Y);

            if (point_translated.X >= 0 && point_translated.X < source.Width && point_translated.Y >= 0 && point_translated.Y < source.Height)
            {
                int offset = ((int)point_translated.Y + source.Y) * texture.Width + (int)point_translated.X + source.X;

                if (PixelData[offset] != 0)
                    return true;
            }

            return false;
        }

        public SimonColors getPressed()
        {
            if (isPressed(simon, new Rectangle(46, 40, 238, 243), new Rectangle(0, 0, 238, 243)))
            {
                return SimonColors.GREEN;
            }

            // RED
            if (isPressed(simon, new Rectangle(46 + 277, 40, 238, 243), new Rectangle(277, 0, 238, 243)))
            {
                return SimonColors.RED;
            }


            // YELLOW
            if (isPressed(simon, new Rectangle(46, 40 + 276, 238, 243), new Rectangle(0, 276, 238, 243)))
            {
                return SimonColors.YELLOW;
            }


            // BLUE
            if (isPressed(simon, new Rectangle(46 + 277, 40 + 276, 238, 243), new Rectangle(277, 276, 238, 243)))
            {
                return SimonColors.BLUE;
            }

            return SimonColors.NONE;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            MouseState ms = Mouse.GetState();

            spriteBatch.Begin();

                spriteBatch.Draw(board, new Rectangle(0, 0, 800, 600), Color.White);

                // Maybe we shouldn't draw all the highlights?   Just the "Lit" one perhaps?   But here's the code if you want to..

                if(Lit == SimonColors.GREEN)
                spriteBatch.Draw(simon, new Rectangle(46, 40, 238, 243), new Rectangle(0, 0, 238, 243), Color.White);

                if(Lit == SimonColors.RED)
                 spriteBatch.Draw(simon, new Rectangle(46 + 277, 40, 238, 243), new Rectangle(277, 0, 238, 243), Color.White);

                if(Lit == SimonColors.YELLOW)
                 spriteBatch.Draw(simon, new Rectangle(46, 40 + 276, 238, 243), new Rectangle(0, 276, 238, 243), Color.White);

                if(Lit == SimonColors.BLUE)
                 spriteBatch.Draw(simon, new Rectangle(46 + 277, 40 + 276, 238, 243), new Rectangle(277, 276, 238, 243), Color.White);

                // Draw cursor
                spriteBatch.Draw(cursor, new Vector2(ms.X, ms.Y), Color.White);
                spriteBatch.DrawString(font, "Count: " + moves.Count , new Vector2(600, 20), Color.White);
                if (turn == Turn.YOUSUCK)
                {
                    spriteBatch.Draw(yousuck, new Rectangle(0, 0, 800, 600), Color.White);
                }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
