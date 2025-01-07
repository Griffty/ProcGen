using System.Collections.Generic;
using UnityEngine;

namespace Griffty.Utility.Static.GMath
{
    public static class Vector2Utils
    {
        public static float AngleBetweenPoints(Vector2 center, Vector2 first, Vector2 second, bool isDegrees = false)
        {
            Vector2 firstDir = DirectionFromPositions(center, first);
            Vector2 secondDir = DirectionFromPositions(center, second);
            float angle = Vector2.SignedAngle(firstDir, secondDir);
            if (isDegrees)
            {
                return angle;
            }
            return angle * Mathf.Deg2Rad;
        }
        public static float AngleToTarget(Vector2 center, Vector2 target, bool isDegrees = false)
        {
            return AngleFromDirection(DirectionFromPositions(center, target), isDegrees);
        }
        public static Vector2 DirectionFromPositions(Vector2 pos, Vector2 targetPos)
        {
            Vector2 direction = targetPos - pos;
            return direction.normalized;
        }
        public static float AngleFromDirection(Vector2 dir, bool isDegrees = false)
        {
            float rot = Mathf.Atan2(dir.y, dir.x);
            if (isDegrees)
            {
                return rot * Mathf.Rad2Deg;
            }
            return rot;
        }
    
        public static Vector3 GetRandomPosAroundTarget(int innerRadius, int outerRadius, Vector3 targetPos, System.Random random = null, HashSet<Vector2Int> possiblePos = null)
        {
            random ??= new System.Random();
            Vector3Int newPos =
                new Vector3Int(
                    random.Next(innerRadius, outerRadius) * (int)(MathUtils.RandomSign(random) + targetPos.x),
                    random.Next(innerRadius, outerRadius) * (int)(MathUtils.RandomSign(random) + targetPos.y), 0);
            if (possiblePos == null) return newPos;
            
            while (!possiblePos.Contains(new Vector2Int(newPos.x, newPos.y)))
            {
                newPos = new Vector3Int(
                    (int)(Random.Range(innerRadius, outerRadius) * (Random.Range(0, 2) * 2 - 1) + targetPos.x),
                    (int)(Random.Range(innerRadius, outerRadius) * (Random.Range(0, 2) * 2 - 1) + targetPos.y),
                    0);
            }
            return newPos;
        }
        public static Vector2 NormalFromAngle(float m)
        {
            float theta = Mathf.Atan(-1 / m); 

            float xComponent = Mathf.Cos(theta);
            float yComponent = Mathf.Sin(theta);

            return new Vector2(xComponent, yComponent);
        }
    }
}