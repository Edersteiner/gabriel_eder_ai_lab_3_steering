using Godot;
using System;

public partial class Target : MeshInstance3D
{
    public override void _Process(double delta)
    {
        Camera3D cam = GetViewport().GetCamera3D();
        if (cam == null)
            return;

        Vector2 mousePos = GetViewport().GetMousePosition();
        Vector3 rayOrigin = cam.ProjectRayOrigin(mousePos);
        Vector3 rayDir = cam.ProjectRayNormal(mousePos);

        float planeY = GlobalPosition.Y;

        if (Math.Abs(rayDir.Y) < 0.00001f)
            return;

        float t = (planeY - rayOrigin.Y) / rayDir.Y;
        Vector3 hit = rayOrigin + rayDir * t;

        Vector3 desired = new Vector3(hit.X, GlobalPosition.Y, hit.Z);

        GlobalPosition = desired;
    }
}
