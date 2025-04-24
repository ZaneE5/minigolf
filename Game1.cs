using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BEPUphysics;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using Vector3 = BEPUutilities.Vector3;
using System.Numerics;

namespace minigolf;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private float aspectRatio;
    public float AspectRatio
    {
        get => aspectRatio;
        set
        {
            aspectRatio = value;
        }
    }
    private Vector3 cameraPosition;
    public Vector3 CameraPosition
    {
        get => cameraPosition;
        set {
            cameraPosition = value;
        }
    }
    private Vector3 cameraDirection;
    public Vector3 CameraDirection {
        get => cameraDirection;
        set {
            cameraDirection = value;
        }
    }

    private MonoPhysics _physics;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        Services.AddService<Space>(new Space());
        // new Golfball(this, Vector3.Zero, Vector3.Zero);
        // _physics.Add(Golfball);
        CameraPosition = new Vector3(0, 0, -5);
        CameraDirection = new Vector3(0, 0, 10);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        Services.GetService<Space>().Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}

// internal class Golfball : PhysicsDrawableComponent {
//     private Model model;
//     // private Texture2D ballTexture;
//     private BEPUphysics.Entities.Prefabs.Sphere physicsObject;
//     private Vector3 currentPosition {
//         get {
//             // return ConversionHelper.MathConverter.Convert(physicsObject.Position);
//             return physicsObject.Position;
//         }
//     }

//     public Golfball(Game1 game, Vector3 position, Vector3 rotation) : base(game, new Standard2DBehavior(), model, position, rotattion){
//         physicsObject = new Sphere(position, 1);
//         physicsObject.AngularDamping = 0f;
//         physicsObject.LinearDamping = 0f;
//         // physicsObject.CollisionInformation.Events.InitialCollisionDetected += Events.InitialCollisionDetected;
//     }
// }
