using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace csgame;



public class Game1: Game {
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont font;

    World world = new World();

    public Game1() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
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
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        font = Content.Load<SpriteFont>("BruhFont");
        world.lighting = Content.Load<Effect>("Lighting");

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
        float mouseSensitivity = 0.003f;

        // Get the current mouse state
        var mouseState = Mouse.GetState();

        // Calculate the difference between the current mouse position and the center of the screen
        int deltaX = mouseState.X - screenCenter.X;
        int deltaY = mouseState.Y - screenCenter.Y;

        Mouse.SetPosition(screenCenter.X, screenCenter.Y);

        // Update yaw and pitch based on mouse movement
        world.cameraR -= deltaX * mouseSensitivity;
        world.cameraT -= deltaY * mouseSensitivity;

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

        world.render();

        base.Draw(gameTime);
    }
}
