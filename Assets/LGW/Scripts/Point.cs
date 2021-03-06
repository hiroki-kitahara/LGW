﻿using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace LGW
{
    /// <summary>
    /// 
    /// </summary>
    public struct Point : IEquatable<Point>
    {
        public int x;

        public int y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Point other)
        {
            return this.x == other.x && this.y == other.y;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode();
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }

        public static bool operator ==(Point a, Point b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Point a, Point b)
        {
            return !a.Equals(b);
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point { x = a.x + b.x, y = a.y + b.y };
        }
    }
}
