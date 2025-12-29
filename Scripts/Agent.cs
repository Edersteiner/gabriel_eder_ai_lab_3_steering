using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Agent : CharacterBody3D
{
    // --- Agent Properties ---

    [ExportGroup("Movement")]
    [Export] public float MaxSpeed = 5.0f;
    [Export] public float MaxForce = 10.0f;
    [Export] public Node3D Target = null;

    [ExportGroup("Arrive")]
    [Export] public float SlowingRadius = 3.0f;

    [ExportGroup("Separation")]
    [Export] public float SeparationRadius = 1.5f;
    [Export] public float SeparationStrength = 10.0f;

    [ExportGroup("Weights")]
    [Export] public float ArriveWeight = 1.0f;
    [Export] public float SeparationWeight = 1.0f;

    // --- Agent State ---

    private Vector3 _velocity = Vector3.Zero;

    // Static List of All Agents
    public static List<Agent> AllAgents = new List<Agent>();

    private int AgentID = 0;
    private float timeSinceLastPathUpdate = 0.0f;
    private float pathUpdateInterval = 0.25f;

    // --- Node References ---

    private NavigationAgent3D _navAgent;

    public override void _Ready()
    {
        _navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

        // Register self in the static list on ready
        AllAgents.Add(this);
        AgentID = AllAgents.Count - 1;

        // Calculate initial path to target if assigned
        if (Target != null)
        {
            _navAgent.TargetPosition = Target.GlobalPosition;
        }

        _navAgent.DebugUseCustom = true;

        // Set individual debug colors based on AgentID
        Color debugColor = Color.FromHsv(AgentID * 0.15f % 1.0f, 1.0f, 1.0f);
        _navAgent.DebugPathCustomColor = debugColor;
    }

    public override void _ExitTree()
    {
        // Unregister self from the static list on exit
        AllAgents.Remove(this);
    }

    public override void _Process(double delta)
    {
        timeSinceLastPathUpdate += (float)delta;

        // Update path to target at intervals, with offset based on AgentID to stagger updates
        if (Target != null && timeSinceLastPathUpdate >= pathUpdateInterval)
        {
            float offset = AgentID % 10 * (pathUpdateInterval / 10.0f);
            if (timeSinceLastPathUpdate >= pathUpdateInterval + offset)
            {
                _navAgent.TargetPosition = Target.GlobalPosition;
                timeSinceLastPathUpdate = 0.0f;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _velocity = Velocity;
        Vector3 steering = Vector3.Zero;

        Vector3 _targetPosition = _navAgent.GetNextPathPosition();

        if (Target != null)
        {
            // Use Seek on all but the last path point, where we use Arrive.
            // This is to avoid agents slowing down at every path point.
            int pointCount = _navAgent.GetCurrentNavigationPath().Count();
            if (_navAgent.GetCurrentNavigationPathIndex() == pointCount - 1)
            {
                steering += Arrive(_targetPosition, SlowingRadius) * ArriveWeight;
            }
            else
            {
                steering += Seek(_targetPosition) * ArriveWeight;
            }
        }

        if (AllAgents.Count > 1)
        {
            steering += Separate(SeparationRadius, SeparationStrength) * SeparationWeight;
        }

        // Limit steering (force) magnitude to MaxForce
        if (steering.Length() > MaxForce)
            steering = steering.Normalized() * MaxForce;

        // Integrate acceleration
        _velocity += steering * (float)delta;

        // Limit speed to MaxSpeed
        if (_velocity.Length() > MaxSpeed)
            _velocity = _velocity.Normalized() * MaxSpeed;


        if (_velocity.LengthSquared() > 0.0001f)
        {
            // Smoothly lerp rotation to face movement direction.
            float targetYaw = Mathf.Atan2(-_velocity.X, -_velocity.Z);
            Vector3 currentEuler = Rotation;
            currentEuler.Y = Mathf.LerpAngle(currentEuler.Y, targetYaw, 5.0f * (float)delta);
            Rotation = currentEuler;
        }

        // Apply Gravity
        if (!IsOnFloor())
        {
            _velocity += GetGravity() * (float)delta;
        }
        else
        {
            _velocity.Y = 0;
        }

        // Move the Agent
        Velocity = _velocity;
        MoveAndSlide();
    }

    public Vector3 Seek(Vector3 targetPosition)
    {
        Vector3 toTarget = targetPosition - GlobalPosition;

        if (toTarget.LengthSquared() < 0.0001f)
            return Vector3.Zero;

        Vector3 desiredVelocity = toTarget.Normalized() * MaxSpeed;

        return desiredVelocity - _velocity;
    }

    public Vector3 Arrive(Vector3 targetPosition, float slowingRadius)
    {
        Vector3 toTarget = targetPosition - GlobalPosition;
        float distance = toTarget.Length();

        if (distance < 0.0001f)
            return Vector3.Zero;

        // Compute stopping distance based on current speed and maximum deceleration (MaxForce)
        float speed = _velocity.Length();
        float decel = (float)Math.Max(MaxForce, 0.0001);
        float stoppingDistance = (speed * speed) / (2.0f * decel);

        // If we're within stopping distance, request full braking (desired velocity = 0)
        if (distance <= stoppingDistance)
        {
            return -_velocity; // steer to cancel current velocity
        }

        float targetSpeed = MaxSpeed;
        if (distance < slowingRadius)
        {
            targetSpeed = MaxSpeed * (distance / slowingRadius);
        }

        Vector3 desiredVelocity = toTarget.Normalized() * targetSpeed;
        return desiredVelocity - _velocity;
    }

    public Vector3 Separate(float separationRadius, float strength)
    {
        Vector3 force = Vector3.Zero;
        int neighborCount = 0;

        foreach (Agent other in AllAgents)
        {
            if (other == this)
                continue;

            Vector3 toOther = other.GlobalPosition - GlobalPosition;
            float distance = toOther.Length();

            if (distance < separationRadius && distance > 0.0001f)
            {
                Vector3 away = -toOther.Normalized() / distance;
                force += away * strength;
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            force /= neighborCount;
            force = force.Normalized() * MaxSpeed - _velocity;

            // Limit force magnitude to strength
            if (force.Length() > strength)
                force = force.Normalized() * strength;
        }

        return force;
    }
}
