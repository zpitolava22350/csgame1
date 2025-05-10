using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Timers;

namespace csgame;

public class Game1: Game {
    public GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont font;
    private Texture2D pixel;

    private static Random rnd = new Random();

    private float selectedBlockDistance = 0f;
    private int prevScroll = 0;

    World world = new World();

    Timer timer = new Timer();
    int avgFPS = 0;
    int tempFPS = 0;

    bool bruh = false;
    bool click = false;

    bool paused = false;

    List<Button> PauseButtons = new List<Button>();

    private enum Click {
        X = 0,
        Y = 1,
        Left = 2,
        Right = 3,
        Middle = 4,
        XButton1 = 5,
        XButton2 = 6
    }

    private Dictionary<Keys, int> keyStates;

    private Dictionary<Click, int> mouseStates;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

    public Game1() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 360.0);
        IsFixedTimeStep = true;
        _graphics.SynchronizeWithVerticalRetrace = true;

        keyStates = new Dictionary<Keys, int>();
        foreach (Keys key in Enum.GetValues(typeof(Keys))) {
            keyStates[key] = 0;
        }

        mouseStates = new Dictionary<Click, int>();
        foreach (Click btn in Enum.GetValues(typeof(Click))) {
            mouseStates[btn] = 0;
        }

        timer.Interval = 1000;
        timer.Start();
        timer.Elapsed += (s, e) => {
            avgFPS = tempFPS;
            tempFPS = 0;
        };

        _graphics.IsFullScreen = false;

        //_graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        //_graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;

        _graphics.ApplyChanges();
    }

    protected override void Initialize() {
        base.Initialize();
        ResourceManager.GraphicsDevice = _graphics.GraphicsDevice;

        // Create Pause menu buttons
        PauseButtons.Add(new Button(new Vector2(-350, -250), new Vector2(200, 100), new Color(200, 200, 200), () => {
            Exit();
        }));

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

        Point screenCenter = new Point(ResourceManager.GraphicsDevice.Viewport.Width / 2, ResourceManager.GraphicsDevice.Viewport.Height / 2);

        // Get the current mouse state
        var mouseState = Mouse.GetState();

        if (mouseState.LeftButton == ButtonState.Pressed) {
            mouseStates[Click.Left] += 1;
        } else {
            mouseStates[Click.Left] = 0;
        }
        if (mouseState.RightButton == ButtonState.Pressed) {
            mouseStates[Click.Right] += 1;
        } else {
            mouseStates[Click.Right] = 0;
        }
        if (mouseState.MiddleButton == ButtonState.Pressed) {
            mouseStates[Click.Middle] += 1;
        } else {
            mouseStates[Click.Middle] = 0;
        }
        if (mouseState.XButton1 == ButtonState.Pressed) {
            mouseStates[Click.XButton1] += 1;
        } else {
            mouseStates[Click.XButton1] = 0;
        }
        if (mouseState.XButton2 == ButtonState.Pressed) {
            mouseStates[Click.XButton2] += 1;
        } else {
            mouseStates[Click.XButton2] = 0;
        }

        mouseStates[Click.X] = mouseState.X;
        mouseStates[Click.Y] = mouseState.Y;

        //Debug.WriteLine($"{mouseStates[Click.Left]}, {mouseStates[Click.Right]}, {mouseStates[Click.Middle]}, {mouseStates[Click.XButton1]}, {mouseStates[Click.XButton2]}");

        // Get keyboard state
        var kstate = Keyboard.GetState();

        foreach (Keys key in Enum.GetValues(typeof(Keys))) {
            if (kstate.IsKeyDown(key)) {
                keyStates[key] += 1;
            } else {
                keyStates[key] = 0;
            }
        }

        if (keyStates[Keys.Escape] == 1) {
            paused = !paused;
        }

        if (!IsActive) {
            paused = true;
        }

        if (!paused) {
            IsMouseVisible = false;
            if (keyStates[Keys.C] == 1) {
                world.AddBlock(world.cameraPosition, new Vector3(1, 1, 1), "grass");
            }

            if (keyStates[Keys.V] == 1) {
                if (world.selectedBlock >= 0) {

                } // maybe else and allow modifying default placed block
            }

            if (keyStates[Keys.F11] == 1) {
                if (_graphics.IsFullScreen) {
                    _graphics.IsFullScreen = false;
                    _graphics.ApplyChanges();
                    _graphics.PreferredBackBufferWidth = 1280;
                    _graphics.PreferredBackBufferHeight = 720;
                    _graphics.ApplyChanges();
                } else {
                    _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    _graphics.ApplyChanges();
                    _graphics.IsFullScreen = true;
                    _graphics.ApplyChanges();
                }
            }

            if (mouseState.LeftButton == ButtonState.Pressed && !click) {
                world.GenerateMap();
                bool rayResult = world.Raycast();
                if (world.selectedBlock < 0) {
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
            float moveSpeed = 10f;
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

            if (world.selectedBlock >= 0) {
                Vector3 prev = world.blocks[world.selectedBlock].position;
                Vector3 D = new Vector3(
                    (float)(Math.Cos(world.cameraT) * -Math.Sin(world.cameraR)),
                    (float)(Math.Sin(world.cameraT)),
                    (float)(Math.Cos(world.cameraT) * -Math.Cos(world.cameraR))
                );
                D.Normalize();
                world.blocks[world.selectedBlock].position = Vector3.Round(world.cameraPosition + (D * selectedBlockDistance));

                if (prev != world.blocks[world.selectedBlock].position) {
                    world.regenMap = true;
                }
            }
        } else { // PAUSED
            IsMouseVisible = true;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        tempFPS++;

        int width = ResourceManager.GraphicsDevice.Viewport.Width;
        int height = ResourceManager.GraphicsDevice.Viewport.Height;

        // TODO: Add your drawing code here
        if (!bruh) {
            bruh = true;
            RawMouseInputReader.Initialize(RawMouseInputReader.GetActiveWindow());
        }

        world.render();

        // HUD
        _spriteBatch.Begin();

        // Crosshair
        Point screenCenter = new Point(ResourceManager.GraphicsDevice.Viewport.Width / 2, ResourceManager.GraphicsDevice.Viewport.Height / 2);

        if (!paused) {
            _spriteBatch.Draw(pixel, new Rectangle(screenCenter.X - 10, screenCenter.Y - 1, 20, 2), Color.White);
            _spriteBatch.Draw(pixel, new Rectangle(screenCenter.X - 1, screenCenter.Y - 10, 2, 20), Color.White);
            _spriteBatch.Draw(pixel, new Rectangle(50, height - 450, 600, 400), Color.FromNonPremultiplied(80, 80, 80, 220));
        } else {
            _spriteBatch.Draw(pixel, new Rectangle(0, 0, width, height), Color.FromNonPremultiplied(10, 10, 10, 200));
            _spriteBatch.Draw(pixel, new Rectangle(screenCenter.X - 400, screenCenter.Y - 300, 800, 600), Color.FromNonPremultiplied(100, 100, 100, 255));
            foreach (Button b in PauseButtons) {
                _spriteBatch.Draw(pixel, new Rectangle((int)b.Position.X + screenCenter.X, (int)b.Position.Y + screenCenter.Y, (int)b.Size.X, (int)b.Size.Y), b.Color);
                if (mouseStates[Click.Left] == 1) {
                    b.CheckClick(mouseStates[Click.X] - screenCenter.X, mouseStates[Click.Y] - screenCenter.Y);
                }
            }
        }

        // FPS
        _spriteBatch.DrawString(font, "FPS: " + (1 / (float)gameTime.ElapsedGameTime.TotalSeconds).ToString("0.0"), new Vector2(10, 10), Color.White);
        _spriteBatch.DrawString(font, "Avg FPS: " + avgFPS.ToString("0.0"), new Vector2(10, 30), Color.White);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void MouseMove(int deltaX, int deltaY) {
        if (!paused) {
            // Update yaw and pitch based on mouse movement
            world.cameraR -= deltaX * 0.0005f;
            world.cameraT -= deltaY * 0.0005f;
        }
    }

    private void MouseScroll(int delta) {
        if (!paused) {
            if (world.selectedBlock >= 0) {
                selectedBlockDistance += delta * 0.005f;
            }
        }
    }

}