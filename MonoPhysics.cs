using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using BEPUphysics;
using BEPUphysics.UpdateableSystems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace minigolf;

internal interface IPhysicsBehavior {
    public void Update(GameTime gameTime);
    public Vector3 Position {get; internal set;}
    public Vector3 Rotation {get; internal set;}
    public PhysicsDrawableComponent.CollisionCallback callback {get; set;}
}

internal class Standard2DBehavior : IPhysicsBehavior {
    public Vector3 Position {get; set;}
    public Vector3 Rotation {get; set;}
    public Vector3 Velocity {get; internal set;}
    public PhysicsDrawableComponent.CollisionCallback callback {get; set;}
    private int CollisionCooldown = 0;
    public Standard2DBehavior() {
        callback = Collision;
    }

    public void Update(GameTime gameTime) {
        Position += Velocity * (float) gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void Collision(PhysicsDrawableComponent other) {
        if (CollisionCooldown > 0) {
            CollisionCooldown--;
            return;
        }
        CollisionCooldown = 60;
        Velocity = -Velocity;
    }
}

internal class PhysicsDrawableComponent : DrawableGameComponent
{
    public delegate void CollisionCallback(PhysicsDrawableComponent other);
    public IPhysicsBehavior PhysicsBehavior { get; set; }
    private Model _model;
    public Model Model
    {
        get => _model;
        set
        {
            _model = value;
        }
    }
    public Rectangle BoundingBox;
    private Vector3 _position;
    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
        }
    }
    private float _rotation;
    public float Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
        }
    }
    private float aspectRatio;
    private Vector3 cameraPosition;
    public PhysicsDrawableComponent(Game1 game, IPhysicsBehavior behavior, Model model, Vector3 position, float rotation) : base(game)
    {
        //init physicsbehavior
        Model = model;
        Position = position;
        Rotation = rotation;
        aspectRatio = game.AspectRatio;
        cameraPosition = ConversionHelper.MathConverter.Convert(game.CameraPosition);
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        BoundingBox.X = (int)PhysicsBehavior.Position.X;
        BoundingBox.Y = (int)PhysicsBehavior.Position.Y;

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        Matrix[] transforms = new Matrix[Model.Bones.Count];
        Model.CopyAbsoluteBoneTransformsTo(transforms);
        foreach (ModelMesh mesh in Model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.EnableDefaultLighting();
                effect.World = Matrix.CreateRotationY(Rotation) *
                                Matrix.CreateTranslation(Position);
                effect.View = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
                effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
            }
            mesh.Draw();
        }
        base.Draw(gameTime);
    }
}

internal class MonoPhysics
{
    private List<PhysicsDrawableComponent> _physicsComponents;
    private Game _game;

    public MonoPhysics(Game game)
    {
        _game = game;
        _physicsComponents = new List<PhysicsDrawableComponent>();
    }

    public IReadOnlyList<PhysicsDrawableComponent> PhysicsComponents
    {
        get
        {
            return _physicsComponents.AsReadOnly<PhysicsDrawableComponent>();
        }
    }

    public void Add(PhysicsDrawableComponent physicsComponent)
    {
        _game.Components.Add(physicsComponent);
        _physicsComponents.Add(physicsComponent);
    }

    public void Remove(PhysicsDrawableComponent physicsComponent)
    {
        _game.Components.Remove(physicsComponent);
        _physicsComponents.Remove(physicsComponent);
    }

    public void Update(GameTime gameTime)
    {
        foreach (var component in _physicsComponents)
        {
            component.PhysicsBehavior.Update(gameTime);
        }
        DetectCollisions();
    }

    private void DetectCollisions()
    {
        for (int i = 0; i < _physicsComponents.Count; i++)
        {
            for (int j = i + 1; j < _physicsComponents.Count - i; j++)
            {
                //check if bounding box i intersects bb j
                //if yes, call the callback delegate for both objects
                if (_physicsComponents[i].BoundingBox.Intersects(_physicsComponents[j].BoundingBox)) {
                    _physicsComponents[i].PhysicsBehavior.callback(_physicsComponents[j]);
                    _physicsComponents[j].PhysicsBehavior.callback(_physicsComponents[i]);
                }
            }
        }
    }
}