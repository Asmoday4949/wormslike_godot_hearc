using System;
using Godot;

public class Rocket : Ammo
{
    private static readonly String EXPLOSION_FILE_PATH = "res://Scenes/Effects/Explosion.tscn";
    private static readonly PackedScene EXPLOSION_SCENE = (PackedScene)ResourceLoader.Load(EXPLOSION_FILE_PATH);

    private static readonly int DAMAGE = 20;
	private static readonly int EXPLOSION_SIZE = 4;

    private CollisionShape2D collisionObject;
    private Area2D areaExplosion;

    private bool launched = false;

    public Rocket() : base(DAMAGE)
    {
    }

    public override void _Ready()
    {
        collisionObject = (CollisionShape2D)this.GetNode("CollisionObject");
        areaExplosion = (Area2D)this.GetNode("AreaExplosion");

        Connect("body_entered", this, "OnBodyEnter");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if (launched)
        {
            Vector2 currentDirection = GetLinearVelocity();
            this.LookAt(this.Position + currentDirection);
        }
    }

    public override void Launch(Vector2 direction, int strength)
    {
        direction = direction.Normalized();
        this.Mode = ModeEnum.Rigid;

        this.ApplyImpulse(Vector2.Zero, direction * strength);
        this.ApplyTorqueImpulse((direction * strength + Vector2.Down * GravityScale * Mass).Angle());

        launched = true;
    }

    private void OnBodyEnter(object body)
    {
        if (!collisionObject.Disabled)
        {
            foreach (Node node in areaExplosion.GetOverlappingBodies())
            {
                if (node is Character)
                {
                    Character character = node as Character;
                    character.Health -= DAMAGE;
                }
            }

            ExplosionEffect explosionEffect = EXPLOSION_SCENE.Instance() as ExplosionEffect;
            explosionEffect.SetGlobalPosition(this.GlobalPosition);
            explosionEffect.SetScale(Vector2.One * EXPLOSION_SIZE);
            this.GetTree().GetRoot().GetNode("Main").AddChild(explosionEffect);

            this.QueueFree();
        }
    }
}
