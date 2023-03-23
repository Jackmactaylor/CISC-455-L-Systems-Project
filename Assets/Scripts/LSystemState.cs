using System.Collections.Generic;
using UnityEngine;

public class LSystemState
{
    public Vector3 Position;
    public Quaternion Orientation;
    public Stack<Tuple<Vector3, Quaternion>> Stack;

    public LSystemState(Vector3 position, Quaternion orientation)
    {
        Position = position;
        Orientation = orientation;
        Stack = new Stack<Tuple<Vector3, Quaternion>>();
    }
    
    public void RotateUp(float angle)
    {
        Orientation *= Quaternion.Euler(0, 0, -angle);
    }

    public void PitchDown(float angle)
    {
        Orientation *= Quaternion.Euler(0, 0, angle);
    }

    public void MoveForward(float stepSize)
    {
        Position += Orientation * Vector3.forward * stepSize;
    }

    public void RotateLeft(float angle)
    {
        Orientation *= Quaternion.Euler(0, -angle, 0);
    }

    public void RotateRight(float angle)
    {
        Orientation *= Quaternion.Euler(0, angle, 0);
    }

    public void PushState()
    {
        Stack.Push(new Tuple<Vector3, Quaternion>(Position, Orientation));
    }

    public void PopState()
    {
        if (Stack.Count > 0)
        {
            Tuple<Vector3, Quaternion> state = Stack.Pop();
            Position = state.Item1;
            Orientation = state.Item2;
        }
    }
}