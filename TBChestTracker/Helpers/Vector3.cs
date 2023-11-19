using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker.Helpers
{
    public class Vector3 
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public static Vector3 Zero
        {
            get => new Vector3(0,0,0);
        }
        public static Vector3 One
        {
            get => new Vector3(1, 1, 1);
        }
        public Vector3(Vector3 v1)
        {
            this.x = v1.x;
            this.y = v1.y;
            this.z = v1.z;
        }
        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);  
        }
        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }
        public static Vector3 operator *(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);  
        }

    }
}
