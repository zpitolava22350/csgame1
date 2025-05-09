using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BlockProperties;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace csgame;

public class Game1: Game {
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont font;
    private Texture2D pixel;

    private float selectedBlockDistance = 0f;
    private int prevScroll = 0;

    World world = new World();

    bool bruh = false;
    bool click = false;

    private Dictionary<Keys, int> keyStates;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

    public Game1() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 360.0);
        IsFixedTimeStep = true;

        keyStates = new Dictionary<Keys, int>();
        foreach (Keys key in Enum.GetValues(typeof(Keys))) {
            keyStates[key] = 0;
        }

        _graphics.IsFullScreen = false;

        //_graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        //_graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;
    }

    protected override void Initialize() {
        base.Initialize();
        ResourceManager.GraphicsDevice = _graphics.GraphicsDevice;

        RawMouseInputReader.SetMoveCallback(MouseMove);
        RawMouseInputReader.SetWheelCallback(MouseScroll);
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        font = Content.Load<SpriteFont>("BruhFont");
        world.lighting = Content.Load<Effect>("Lighting");

        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        world.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(1.57f, GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000f);
        world.texture = Content.Load<Texture2D>("texsheet");
        world.texture.GraphicsDevice.SamplerStates[0] = new SamplerState { Filter = TextureFilter.Point, AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap };
        world.lighting.Parameters[$"Texture"].SetValue(world.texture);
    }

    protected override void Update(GameTime gameTime) {

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Point screenCenter = new Point(ResourceManager.GraphicsDevice.Viewport.Width / 2, ResourceManager.GraphicsDevice.Viewport.Height / 2);

        float moveSpeed = 50f;

        // Get the current mouse state
        var mouseState = Mouse.GetState();

        // Get keyboard state
        var kstate = Keyboard.GetState();

        foreach (Keys key in Enum.GetValues(typeof(Keys))) {
            if (kstate.IsKeyDown(key)) {
                keyStates[key] += 1;
            } else {
                keyStates[key] = 0;
            }
        }

        if (keyStates[Keys.C] == 1) {
            world.AddBlock(world.cameraPosition, new Vector3(1, 1, 1), "grass");
        }

        if (keyStates[Keys.V] == 1) {
            if (world.selectedBlock >= 0) {
                IsMouseVisible = true;
                Block b = world.blocks[world.selectedBlock];
                var pw = new PropertiesWindow();
                pw.dx = b.size.X;
                pw.dy = b.size.Y;
                pw.dz = b.size.Z;
                pw.tex = b.tex;
                if (pw.ShowDialog() == System.Windows.Forms.DialogResult.OK) {

                }
            } // maybe else and allow modifying default placed block
        }

        if (mouseState.LeftButton == ButtonState.Pressed && !click) {
            world.GenerateMap();
            bool rayResult = world.Raycast();
            if(world.selectedBlock < 0) {
                if (rayResult) {
                    world.selectedBlock = world.lastRayIndex;
                    selectedBlockDistance = Vector3.Distance(world.cameraPosition, world.blocks[world.selectedBlock].position);
                }
            } else {
                world.selectedBlock = -1;
            }
            click = true;
        } else if (mouseState.LeftButton == ButtonState.Released && click) {
            click = false;
        }

        Mouse.SetPosition(screenCenter.X, screenCenter.Y);

        // Prevent flipping, maybe remove later when making game cuz it could be cool mechanic
        world.cameraT = MathHelper.Clamp(world.cameraT, -MathHelper.PiOver2, MathHelper.PiOver2);

        // Move player
        if (kstate.IsKeyDown(Keys.W)) {
            world.cameraPosition += new Vector3((float)Math.Sin(world.cameraR) * -moveSpeed * deltaTime, 0f, 0f);
            world.cameraPosition += new Vector3(0f, 0f, (float)Math.Cos(world.cameraR) * -moveSpeed * deltaTime);
        }
        if (kstate.IsKeyDown(Keys.A)) {
            world.cameraPosition += new Vector3((float)Math.Sin(world.cameraR + MathHelper.PiOver2) * -moveSpeed * deltaTime, 0f, 0f);
            world.cameraPosition += new Vector3(0f, 0f, (float)Math.Cos(world.cameraR + MathHelper.PiOver2) * -moveSpeed * deltaTime);
        }
        if (kstate.IsKeyDown(Keys.S)) {
            world.cameraPosition += new Vector3((float)Math.Sin(world.cameraR + MathHelper.Pi) * -moveSpeed * deltaTime, 0f, 0f);
            world.cameraPosition += new Vector3(0f, 0f, (float)Math.Cos(world.cameraR + MathHelper.Pi) * -moveSpeed * deltaTime);
        }
        if (kstate.IsKeyDown(Keys.D)) {
            world.cameraPosition += new Vector3((float)Math.Sin(world.cameraR - MathHelper.PiOver2) * -moveSpeed * deltaTime, 0f, 0f);
            world.cameraPosition += new Vector3(0f, 0f, (float)Math.Cos(world.cameraR - MathHelper.PiOver2) * -moveSpeed * deltaTime);
        }
        if (kstate.IsKeyDown(Keys.Space)) {
            world.cameraPosition += new Vector3(0f, moveSpeed * deltaTime, 0f);
        }
        if (kstate.IsKeyDown(Keys.LeftShift)) {
            world.cameraPosition -= new Vector3(0f, moveSpeed * deltaTime, 0f);
        }

        if(world.selectedBlock >= 0) {
            Vector3 prev = world.blocks[world.selectedBlock].position;
            Vector3 D = new Vector3(
                (float)(Math.Cos(world.cameraT) * -Math.Sin(world.cameraR)),
                (float)(Math.Sin(world.cameraT)),
                (float)(Math.Cos(world.cameraT) * -Math.Cos(world.cameraR))
            );
            D.Normalize();
            world.blocks[world.selectedBlock].position = Vector3.Round(world.cameraPosition + (D * selectedBlockDistance));

            if(prev != world.blocks[world.selectedBlock].position) {
                world.regenMap = true;
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        if (!bruh) {
            bruh = true;
            RawMouseInputReader.Initialize(RawMouseInputReader.GetActiveWindow());
        }

        world.render();

        // HUD
        _spriteBatch.Begin();

        // FPS
        _spriteBatch.DrawString(font, "FPS: " + (1 / (float)gameTime.ElapsedGameTime.TotalSeconds).ToString("0.0"), new Vector2(10, 10), Color.White);

        // Crosshair
        Point screenCenter = new Point(ResourceManager.GraphicsDevice.Viewport.Width / 2, ResourceManager.GraphicsDevice.Viewport.Height / 2);
        _spriteBatch.Draw(pixel, new Rectangle(screenCenter.X - 10, screenCenter.Y - 1, 20, 2), Color.White);
        _spriteBatch.Draw(pixel, new Rectangle(screenCenter.X - 1, screenCenter.Y - 10, 2, 20), Color.White);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void MouseMove(int deltaX, int deltaY) {
        // Update yaw and pitch based on mouse movement
        world.cameraR -= deltaX * 0.0005f;
        world.cameraT -= deltaY * 0.0005f;
    }

    private void MouseScroll(int delta) {
        if(world.selectedBlock >= 0) {
            selectedBlockDistance -= delta * 0.005f;
        }
    }

}