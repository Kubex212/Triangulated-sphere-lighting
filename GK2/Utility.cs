using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK2
{
    public static class Utility
    {
        public static double Clamp0(double val)
        {
            if (val > 1) return 1;
            return val < 0 ? 0 : val;
        }
        public static int Clamp0_255(int val)
        {
            if (val < 0) return 0;
            return val > 255 ? 255 : val;
        }
        public static Vector3 ChangeBase(Vector3 v, Vector3 n)
        {

            Vector3 roty = (n.Z == 1) ? new Vector3(0, 1, 0) : Vector3.Normalize(new Vector3(0, 0, 1) * n);
            Vector3 rotx = Vector3.Normalize(roty * n);
            Vector3 rotz = n;
            return Vector3.Normalize(rotx * v.X + roty * v.Y + rotz * v.Z);
        }
        public static Vector3 RotatePoint(Vector3 pointToRotate, Vector3 centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Vector3
            {
                X =
                    (float)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (float)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y),
                Z = pointToRotate.Z
            };
        }
        public static Vector3 Reflection(Vector3 n, Vector3 l)
        {
            return 2 * Vector3.Dot(n, l) * (n - l);
        }
        public static Vector3 NewPosRight(Vector3 p)
        {
            var offset = GK2.holeOffset;
            var radius = GK2.holeRadius;
            float Z = 421.65f;
            float dist = (p.X - offset) * (p.X - offset) + p.Y * p.Y;
            dist = (float)Math.Sqrt(dist);
            dist = dist * (float)(Math.PI / 2f) / 50f;
            return new Vector3(p.X, p.Y, Z - (float)Math.Cos(dist) * radius);
        }
        public static Vector3 NewPosLeft(Vector3 p)
        {
            var offset = GK2.holeOffset;
            var radius = GK2.holeRadius;
            float Z = 421.65f;
            float dist = (p.X + offset) * (p.X + offset) + p.Y * p.Y;
            dist = (float)Math.Sqrt(dist);
            dist = dist * (float)(Math.PI / 2f) / 50f;
            return new Vector3(p.X, p.Y, Z - (float)Math.Cos(dist) * radius);
        }
        public static Vector3 NewPosUp(Vector3 p)
        {
            var offset = GK2.holeOffset;
            var radius = GK2.holeRadius;
            float Z = 421.65f;
            float dist = p.X * p.X + (p.Y + offset) * (p.Y + offset);
            dist = (float)Math.Sqrt(dist);
            dist = dist * (float)(Math.PI / 2f) / 50f;
            return new Vector3(p.X, p.Y, Z - (float)Math.Cos(dist) * radius);
        }
        public static Vector3 NewPosDown(Vector3 p)
        {
            var offset = GK2.holeOffset;
            var radius = GK2.holeRadius;
            float Z = 421.65f;
            float dist = p.X * p.X + (p.Y - offset) * (p.Y - offset);
            dist = (float)Math.Sqrt(dist);
            dist = dist * (float)(Math.PI / 2f) / 50f;
            return new Vector3(p.X, p.Y, Z - (float)Math.Cos(dist) * radius);
        }
    }
}
