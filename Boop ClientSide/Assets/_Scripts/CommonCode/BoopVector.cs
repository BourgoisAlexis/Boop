using System;
using System.Collections.Generic;

public class BoopVector {
    public int x;
    public int y;

    public BoopVector(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public static BoopVector operator +(BoopVector a, BoopVector b) => new BoopVector(a.x + b.x, a.y + b.y);
    public static BoopVector operator -(BoopVector a, BoopVector b) => new BoopVector(a.x - b.x, a.y - b.y);

    public override string ToString() => $"{x},{y}";
    public static BoopVector FromString(string s) => new BoopVector(int.Parse(s.Split(',')[0]), int.Parse(s.Split(',')[1]));

    public override bool Equals(object obj) {
        BoopVector objAsVector = obj as BoopVector;
        if (obj == null)
            return false;
        else
            return x == objAsVector.x && y == objAsVector.y;
    }
}

class VectorComparer : IEqualityComparer<BoopVector> {
    public bool Equals(BoopVector a, BoopVector b) {
        if (Object.ReferenceEquals(a, b)) return true;

        if (Object.ReferenceEquals(a, null) || Object.ReferenceEquals(b, null))
            return false;

        return a.x == b.x && a.y == b.y;
    }

    // If Equals() returns true for a pair of objects
    // then GetHashCode() must return the same value for these objects.

    public int GetHashCode(BoopVector vector) {
        if (Object.ReferenceEquals(vector, null)) return 0;

        int hashX = vector.x.GetHashCode();
        int hashY = vector.y.GetHashCode();

        return hashX ^ hashY;
    }
}