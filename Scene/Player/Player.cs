using Godot;
using System;
using System.ComponentModel.DataAnnotations;

public partial class Player : CharacterBody3D
{
	[Export]
	public Camera3D ThirdPersonCamera;
	[Export] 
	public Node3D character_rotation_root;

	[Export]
	public CollisionShape3D character_crouch;


    // public override void _Ready()
    // {
    //     character_rotation_root = GetNode<Node3D>("CharacterRotation");

	// 	character_crouch = GetNode<CollisionShape3D>("collide");
    // }

	public float Speed = 10.0f;
	public const float JumpVelocity = 9.0f;

	private float _lastRotationY = 0f;

	private float start_time = 0f;

	private float coyote_time = 0f;

	private float air_time =0f;

	private float AccelerationRate = 1.5f;
	private float DecelerationRate = 6f;


	bool isjump = false;
	bool end_crouch = false;

	bool m1 = true;
	bool m2 = false;
	bool m3 = false;

	[Export]
	Curve curve;
	public override void _PhysicsProcess(double delta)
	{

		Vector3 velocity = Velocity;


		void crouch()
		{	
			{
				((CapsuleShape3D) character_crouch.Shape).Height =1.1f;
				Speed=5.0f;
			}
		}
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
		//jump!!!!!!!!!!!(crouch)

		float G = GetGravity().Y;

		Vector3 current_G = GetGravity();

		float Gtimes = 1.0f;

		void jump()
		{
			velocity.Y = JumpVelocity;
		}

		if(end_crouch)
		{
			start_time += (float) delta;
			
			if(start_time<=0.3f)
			{
				crouch();
			}
			if(start_time>0.3f)
			{
				jump();
				end_crouch = false;
				isjump= true;
				start_time=0;
			}
		}

		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			end_crouch = true;
		}
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
 		
		// gravity, jump, coyote_time
		if(IsOnFloor())
		{
			coyote_time = 0;
		}

		if (!IsOnFloor())
		{
			coyote_time += (float)delta;

			//coyote time jump (double jump also lol)
			if(coyote_time<0.3f && Input.IsActionJustPressed("ui_accept"))
			{
				velocity.Y = JumpVelocity;
			}

			if(isjump)
			{
				if(Math.Abs(velocity.Y)<3.0f)
				{
					Gtimes = 0.5f;
					if(velocity.Y<-0.3f)
					{
						isjump = false;
					}
				}
			}
			else
			{
				Gtimes = 1.0f;
			}
			
			
			velocity += current_G * Gtimes *(float)delta;


			//add grativity
		}
//////////////////////////////////////////////////////////////////////////////////////////
		// Mathf.SmoothStep()
		// crouch
		if(Input.IsActionPressed("crouch"))
		{
			crouch();
		}
		else if(end_crouch == false)
		{
			((CapsuleShape3D) character_crouch.Shape).Height =2.2f;
			Speed=10.0f;
		}
////////////////////////////////////////////////////////////////////////////////////////
		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (ThirdPersonCamera != null)
		{
			 Vector3 cameraEuler = ThirdPersonCamera.Transform.Basis.GetEuler();
    		Basis horizontalCameraBasis = Basis.FromEuler(new Vector3(0, cameraEuler.Y, 0));
			direction = (horizontalCameraBasis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		}
		else
		{
			direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		}
		

		Vector3 currentHorizontalVelocity = new Vector3(velocity.X,0,velocity.Z); // horizon
		float currentHorizontalSpeed = currentHorizontalVelocity.Length(); // velocity
		Vector3 horizontalDir = currentHorizontalVelocity.Normalized(); // direction
		Vector2 targetHorizontalVelocity = new Vector2 (direction.X,direction.Z) * Speed;

		if(m1)
		{
			if (direction != Vector3.Zero)
			{
				velocity.X = direction.X * Speed;
				velocity.Z = direction.Z * Speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
			}
		}

		if(m2)
		{
			if(direction != Vector3.Zero || currentHorizontalSpeed < 0.01f)
			{
				horizontalDir = direction;
			}
			float targetHorizontalSpeed = direction != Vector3.Zero ? Speed : 0f;
			float acceleration = direction != Vector3.Zero ? AccelerationRate : DecelerationRate;

			Vector2 newHorizontalVelocity = new Vector2 (currentHorizontalVelocity.X,currentHorizontalVelocity.Z) + (targetHorizontalVelocity - new Vector2 (currentHorizontalVelocity.X,currentHorizontalVelocity.Z)) * acceleration * (float)delta;//A+t*(B-A)
			
			velocity.X = newHorizontalVelocity.X;
			velocity.Z = newHorizontalVelocity.Y;
		}

		if(m3)
		{
			if(direction != Vector3.Zero || currentHorizontalSpeed < 0.01f)
			{
				horizontalDir = direction;
			}
			float targetHorizontalSpeed = direction != Vector3.Zero ? Speed : 0f;
		    float easeweight = Mathf.Ease(AccelerationRate * (float) delta, 1.3f);
			float acceleration = direction != Vector3.Zero ?  easeweight : DecelerationRate;
			if(acceleration == easeweight)
			{
				Vector2 newHorizontalVelocity =  new Vector2 (currentHorizontalVelocity.X,currentHorizontalVelocity.Z) + (targetHorizontalVelocity - new Vector2 (currentHorizontalVelocity.X,currentHorizontalVelocity.Z)) * acceleration ;
				velocity.X = newHorizontalVelocity.X;
				velocity.Z = newHorizontalVelocity.Y;
			}
			else if(acceleration == DecelerationRate)
			{
				Vector2 newHorizontalVelocity =  new Vector2 (currentHorizontalVelocity.X,currentHorizontalVelocity.Z) + (targetHorizontalVelocity - new Vector2 (currentHorizontalVelocity.X,currentHorizontalVelocity.Z)) * acceleration * (float)delta ;
				velocity.X = newHorizontalVelocity.X;
				velocity.Z = newHorizontalVelocity.Y;
			}
		}
		



/////////////////////////////////////////////////////////////////////////////////
        //character rotation
		if(new Vector3(velocity.X,0,velocity.Z).Length()>0.1f)
		{
			//METHOD 1 !!! 
			Vector2 characterDir;
			 if (direction != Vector3.Zero)
			{
				characterDir = new Vector2(direction.Z, direction.X);
			}
			else
			{
				characterDir = new Vector2(velocity.Z, velocity.X);
			}


			var currentRot = character_rotation_root.Rotation;
			double target_angle = characterDir.Angle();
			double new_angle = Mathf.LerpAngle(currentRot.Y,target_angle,delta *7);
			currentRot.Y = (float)new_angle;
		



			character_rotation_root.Rotation = currentRot;




			//METHOD 2 !!! (with quaternion)
			// Quaternion currentRot = Quaternion.FromEuler( new Vector3(0,characterDir.Angle(),0));
			// character_rotation_root.Quaternion = character_rotation_root.Quaternion.Slerp(currentRot, (float)delta * 6 );
		}

		// DebugDraw2D.SetText(m1.ToString(),0,0,Colors.Red,0.001f);
		// DebugDraw2D.SetText(m2.ToString(),0,0,Colors.Red,0.001f);
		// DebugDraw2D.SetText(m3.ToString(),0,0,Colors.Red,0.001f);
////////////////////////////////////////////////////////////////////////////////////////////
     //3 different mode of acceleration
		if(Input.IsActionJustPressed("mode_1"))
		{
			m1 = true;
			m2 = false;
			m3 = false;
			
		}

		if(Input.IsActionJustPressed("mode_2"))
		{
			m1 = false;
			m2 = true;
			m3 = false;
		}

		if(Input.IsActionJustPressed("mode_3"))
		{
			m1 = false;
			m2 = false;
			m3 = true;
		}


		Velocity = velocity;
		MoveAndSlide();
	}
}
