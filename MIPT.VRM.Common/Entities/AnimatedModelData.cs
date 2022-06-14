using OpenTK.Mathematics;

namespace MIPT.VRM.Common.Entities;

public class AnimatedModelData
{
    public readonly SkeletonData Joints;
    public readonly MeshData Mesh;
	
    public AnimatedModelData(MeshData mesh, SkeletonData joints)
    {
        this.Joints = joints;
        this.Mesh = mesh;
    }
}

public class AnimationData 
{
    public readonly float LengthSeconds;
    public readonly KeyFrameData[] KeyFrames;

    public AnimationData(float lengthSeconds, KeyFrameData[] keyFrames)
    {
        this.LengthSeconds = lengthSeconds;
        this.KeyFrames = keyFrames;
    }
}

public class JointData
{
    public readonly int Index;
    public readonly string NameId;
    public readonly Matrix4 BindLocalTransform;
    public readonly IList<JointData> Children = new List<JointData>();

    public JointData(int index, String nameId, Matrix4 bindLocalTransform)
    {
        this.Index = index;
        this.NameId = nameId;
        this.BindLocalTransform = bindLocalTransform;
    }

    public void addChild(JointData child)
    {
        this.Children.Add(child);
    }
}

public class JointTransformData 
{
    public readonly string JointNameId;
    public readonly Matrix4 JointLocalTransform;

    public JointTransformData(string jointNameId, Matrix4 jointLocalTransform) 
    {
        this.JointNameId = jointNameId;
        this.JointLocalTransform = jointLocalTransform;
    }
}

public class KeyFrameData 
{
    public readonly float Time;
    public readonly List<JointTransformData> JointTransforms = new List<JointTransformData>();
	
    public KeyFrameData(float time)
    {
        this.Time = time;
    }
	
    public void AddJointTransform(JointTransformData transform)
    {
        this.JointTransforms.Add(transform);
    }
}

public class MeshData
{
    private static readonly int DIMENSIONS = 3;

    public readonly float[] Vertices;
    public readonly float[] TextureCoords;
    public readonly float[] Normals;
    public readonly int[] Indices;
    public readonly int[] JointIds;
    public readonly float[] VertexWeights;

    public MeshData(float[] vertices, float[] textureCoords, float[] normals, int[] indices, int[] jointIds, float[] vertexWeights)
    {
        this.Vertices = vertices;
        this.TextureCoords = textureCoords;
        this.Normals = normals;
        this.Indices = indices;
        this.JointIds = jointIds;
        this.VertexWeights = vertexWeights;
    }

    public int GetVertexCount() {
        return this.Vertices.Length / DIMENSIONS;
    }
}

public class SkeletonData
{
    public readonly int JointCount;
    public readonly JointData HeadJoint;
	
    public SkeletonData(int jointCount, JointData headJoint){
        this.JointCount = jointCount;
        this.HeadJoint = headJoint;
    }
}

public class SkinningData 
{
    public readonly List<string> JointOrder;
    public readonly List<VertexSkinData> VerticesSkinData;
	
    public SkinningData(List<string> jointOrder, List<VertexSkinData> verticesSkinData){
        this.JointOrder = jointOrder;
        this.VerticesSkinData = verticesSkinData;
    }
}

public class VertexSkinData {
	
    public readonly List<int> JointIds = new List<int>();
    public readonly List<float> Weights = new List<float>();
	
    public void addJointEffect(int jointId, float weight)
    {
        for(int i=0;i<this.Weights.Count;i++)
        {
            if(weight > this.Weights[i])
            {
                this.JointIds[i] = jointId;
                this.Weights[i] = weight;
                return;
            }
        }
        
        this.JointIds.Add(jointId);
        this.Weights.Add(weight);
    }
	
    public void LimitJointNumber(int max)
    {
        if(this.JointIds.Count > max)
        {
            var topWeights = new float[max];
            var total = this.SaveTopWeights(topWeights);
            this.RefillWeightList(topWeights, total);
            this.RemoveExcessJointIds(max);
        }
        else if (this.JointIds.Count < max)
            this.FillEmptyWeights(max);
    }

    private void FillEmptyWeights(int max)
    {
        while (this.JointIds.Count < max)
        {
            this.JointIds.Add(0);
            this.Weights.Add(0f);
        }
    }
	
    private float SaveTopWeights(float[] topWeightsArray)
    {
        float total = 0;
        for (var i=0; i<topWeightsArray.Length; i++)
        {
            topWeightsArray[i] = this.Weights[i];
            total += topWeightsArray[i];
        }
        return total;
    }
	
    private void RefillWeightList(float[] topWeights, float total)
    {
        this.Weights.Clear();

        foreach (var t in topWeights)
            this.Weights.Add(Math.Min(t/total, 1));
    }
	
    private void RemoveExcessJointIds(int max){
        while(this.JointIds.Count > max){
            this.JointIds.Remove(this.JointIds.Count-1);
        }
    }
}

public class Vertex
{
    private static readonly int NO_INDEX = -1;
    public readonly Vector3 position;
    public readonly int index;
    public readonly float length;
    public readonly List<Vector3> tangents = new List<Vector3>();
    public readonly VertexSkinData WeightsData;
    public Vertex duplicateVertex = null;
    public Vector3 averagedTangent = new Vector3(0, 0, 0);
    public int textureIndex = NO_INDEX;
    public int normalIndex = NO_INDEX;
	
    public Vertex(int index, Vector3 position, VertexSkinData weightsData)
    {
        this.index = index;
        this.WeightsData = weightsData;
        this.position = position;
        this.length = position.Length;
    }
	
    public void AddTangent(Vector3 tangent)
    {
        tangents.Add(tangent);
    }
	
    public void AverageTangents()
    {
        if(!tangents.Any())
            return;
        
        foreach(var tangent in tangents)
            Vector3.Add(averagedTangent, tangent, out averagedTangent);

        averagedTangent.Normalize();
    }
	
    public bool IsSet()
    {
        return textureIndex!=NO_INDEX && normalIndex!=NO_INDEX;
    }
	
    public bool HasSameTextureAndNormal(int textureIndexOther,int normalIndexOther)
    {
        return textureIndexOther == textureIndex && normalIndexOther == normalIndex;
    }
	
    public void SetTextureIndex(int textureIndex)
    {
        this.textureIndex = textureIndex;
    }
	
    public void SetNormalIndex(int normalIndex)
    {
        this.normalIndex = normalIndex;
    }

    public void SetDuplicateVertex(Vertex duplicateVertex)
    {
        this.duplicateVertex = duplicateVertex;
    }
}