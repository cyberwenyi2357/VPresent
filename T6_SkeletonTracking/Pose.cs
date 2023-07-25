using Microsoft.Kinect;
namespace NUI3D
{
    public class PoseAngle
    {
        public JointType StartJoint;
        public JointType EndJoint;
        public float Angle;
        public float Threshold;
        public bool Matched = false; 

        public PoseAngle(JointType sj, JointType ej, float a, float t)
        {
            StartJoint = sj;
            EndJoint = ej;
            Angle = a;
            Threshold = t;
        }
    }

    public class Pose
    {
        public string Title;
        public PoseAngle[] Angles;
    }
}
