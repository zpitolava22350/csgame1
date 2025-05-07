using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace csgame;



public class Game1: Game {
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont font;
    private Texture2D pixel;

    World world = new World();

    bool bruh = false;
    bool click = false;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

    public Game1() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 360.0);
        IsFixedTimeStep = true;

        _graphics.IsFullScreen = false;

        //_graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        //_graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;
    }

    protected override void Initialize() {
        base.Initialize();
        ResourceManager.GraphicsDevice = GraphicsDevice;

        RawMouseInputReader.SetCallback(MouseMove);
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

        if (mouseState.LeftButton == ButtonState.Pressed && !click) {
            bool buh = world.Raycast();
            if(buh)
                world.cameraPosition = Vector3.Lerp(world.cameraPosition, world.lastRay, 0.05f);
            click = true;
        } else if(mouseState.LeftButton == ButtonState.Released && click) {
            click = false;
        }

        // Calculate the difference between the current mouse position and the center of the screen
        int deltaX = mouseState.X - screenCenter.X;
        int deltaY = mouseState.Y - screenCenter.Y;

        Mouse.SetPosition(screenCenter.X, screenCenter.Y);

        // Clamp the pitch to prevent flipping
        world.cameraT = MathHelper.Clamp(world.cameraT, -MathHelper.PiOver2, MathHelper.PiOver2);

        // Get keyboard state
        var kstate = Keyboard.GetState();

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

}