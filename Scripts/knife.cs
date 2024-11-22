using Godot;
using System;

public partial class knife : Node3D 
{

	private Node3D testNode;
	private CharacterBody3D player;
	private Camera3D camera;
	private bool isActive = true;
	private const float meleeRange = 10.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		testNode = GetChild<Node3D>(0);
		camera = GetParent<Camera3D>();
		player = GetParent().GetParent().GetParent().GetParent<CharacterBody3D>(); //💀im so sorry
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public override void _PhysicsProcess(double delta)
	{
		var spaceState = GetWorld3D().DirectSpaceState;
		Vector2 mousePos = GetViewport().GetMousePosition();
		Vector3 origin = camera.ProjectRayOrigin(mousePos);
		Vector3 end = origin + camera.ProjectRayNormal(mousePos) * meleeRange;
		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from: origin,to:end,exclude:new Godot.Collections.Array<Rid>{player.GetRid()});
		query.HitFromInside = false;
		var Intersections = spaceState.IntersectRay(query);
		if(Intersections.Count > 0)
		{
			GD.Print(Intersections);
			// GD.Print(player);
		}
	}

    public override void _Input(InputEvent @event)
    {
        
    }
}
