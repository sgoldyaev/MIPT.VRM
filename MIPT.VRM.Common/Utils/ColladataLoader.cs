using System.Text.RegularExpressions;
using MIPT.VRM.Common.Entities;
using OpenTK.Mathematics;

namespace MIPT.VRM.Common.Utils;

public sealed class Settings
{
	public static readonly Matrix4 CORRECTION = Matrix4.CreateFromQuaternion(new Quaternion(new Vector3(1, 0, 0),  (float)Math.PI/2));
}

public class XmlNode {

	public readonly string Name;
	private Dictionary<string, string> attributes;
	private string data;
	private Dictionary<string, List<XmlNode>> childNodes;

	public XmlNode(string name) {
		this.Name = name;
	}

	public String getData() {
		return data;
	}

	public string GetAttribute(string attr)
	{
		return attributes?.TryGetValue(attr, out var result) == true ? result : null;
	}

	public XmlNode GetChild(string childName) {
		if (childNodes != null 
		    && childNodes.TryGetValue(childName, out var nodes) 
		    && nodes.Any()) 
			return nodes[0];

		return null;
	}

	public XmlNode GetChildWithAttribute(string childName, string attr, string value) {
		List<XmlNode> children = this.GetChildren(childName);
		if (children == null || !children.Any()) 
			return null;

		foreach (var child in children) 
		{
			var val = child.GetAttribute(attr);
			if (value.Equals(val))
				return child;
		}
		return null;
	}

	public List<XmlNode> GetChildren(string name) 
	{
		if (childNodes != null)
		{
			if (childNodes.TryGetValue(name, out var children))
				return children;
		}
		return new List<XmlNode>();
	}

	public void AddAttribute(string attr, string value)
	{
		if (attributes == null)
			attributes = new Dictionary<string, string>();

		attributes[attr] = value;
	}

	public void AddChild(XmlNode child) {
		if (childNodes == null)
			childNodes = new Dictionary<string, List<XmlNode>>();
		
		
		if (!childNodes.TryGetValue(child.Name, out var list))
		{
			list = new List<XmlNode>();
			childNodes.Add(child.Name, list);
		}
		list.Add(child);
	}

	public void SetData(String data) {
		this.data = data;
	}
}

public class XmlParser
{
	private static readonly Regex DATA = new Regex(">(.+?)<");
	private static readonly Regex START_TAG = new Regex("<(.+?)>");
	private static readonly Regex ATTR_NAME = new Regex("(.+?)=");
	private static readonly Regex ATTR_VAL = new Regex("\"(.+?)\"");
	private static readonly Regex CLOSED = new Regex("(</|/>)");

	public static XmlNode LoadXmlFile(FileInfo fileName) 
	{
		try
		{
			using (var reader = fileName.OpenText())
			{
				reader.ReadLine();
				var node = LoadNode(reader);
				reader.Close();
				return node;
			}
		} 
		catch (Exception e) 
		{
			Console.WriteLine($"{fileName} {e}");
			return null;
		}
	}

	private static XmlNode LoadNode(TextReader reader)
	{
		var line = reader.ReadLine()?.Trim();
		if (line.StartsWith("</"))
		{
			return null;
		}
		String[] startTagParts = getStartTag(line).Split(" ");
		XmlNode node = new XmlNode(startTagParts[0].Replace("/", ""));
		AddAttributes(startTagParts, node);
		AddData(line, node);
		if (CLOSED.Match(line).Success)
		{
			return node;
		}
		XmlNode child = null;
		while ((child = LoadNode(reader)) != null)
		{
			node.AddChild(child);
		}
		return node;
	}

	private static void AddData(string line, XmlNode node) {
		Match matcher = DATA.Match(line);
		if (matcher.Success) {
			node.SetData(matcher.Groups[1].Value);
		}
	}

	private static void AddAttributes(String[] titleParts, XmlNode node) {
		for (var i = 1; i < titleParts.Length; i++)
		{
			if (titleParts[i].Contains("=")) {
				AddAttribute(titleParts[i], node);
			}
		}
	}

	private static void AddAttribute(String attributeLine, XmlNode node)
	{
		var nameMatch = ATTR_NAME.Match(attributeLine);
		var valMatch = ATTR_VAL.Match(attributeLine);
		node.AddAttribute(nameMatch.Groups[1].Value, valMatch.Groups[1].Value);
	}

	private static String getStartTag(String line)
	{
		var match = START_TAG.Match(line);
		return match.Groups[1].Value;
	}
}

public class ColladaLoader 
{
    public static AnimatedModelData LoadColladaModel(FileInfo colladaFile, int maxWeights) 
    {
        XmlNode node = XmlParser.LoadXmlFile(colladaFile);

        SkinLoader skinLoader = new SkinLoader(node.GetChild("library_controllers"), maxWeights);
        SkinningData skinningData = skinLoader.extractSkinData();

        SkeletonLoader jointsLoader = new SkeletonLoader(node.GetChild("library_visual_scenes"), skinningData.JointOrder);
        SkeletonData jointsData = jointsLoader.extractBoneData();

        GeometryLoader g = new GeometryLoader(node.GetChild("library_geometries"), skinningData.VerticesSkinData);
        MeshData meshData = g.ExtractModelData();

        return new AnimatedModelData(meshData, jointsData);
    }

    public static AnimationData LoadColladaAnimation(FileInfo colladaFile) 
    {
        XmlNode node = XmlParser.LoadXmlFile(colladaFile);
        XmlNode animNode = node.GetChild("library_animations");
        XmlNode jointsNode = node.GetChild("library_visual_scenes");
        AnimationLoader loader = new AnimationLoader(animNode, jointsNode);
        AnimationData animData = loader.extractAnimation();
        return animData;
    }
}

public class AnimationLoader 
{
	private static readonly Matrix4 CORRECTION = Settings.CORRECTION;
	
	private XmlNode animationData;
	private XmlNode jointHierarchy;
	
	public AnimationLoader(XmlNode animationData, XmlNode jointHierarchy){
		this.animationData = animationData;
		this.jointHierarchy = jointHierarchy;
	}
	
	public AnimationData extractAnimation(){
		String rootNode = findRootJointName();
		float[] times = getKeyTimes();
		float duration = times[times.Length-1];
		KeyFrameData[] keyFrames = initKeyFrames(times);
		List<XmlNode> animationNodes = animationData.GetChildren("animation");
		foreach(var jointNode in animationNodes)
			loadJointTransforms(keyFrames, jointNode, rootNode);
		
		return new AnimationData(duration, keyFrames);
	}
	
	private float[] getKeyTimes(){
		XmlNode timeData = animationData.GetChild("animation").GetChild("source").GetChild("float_array");
		String[] rawTimes = timeData.getData().Split(" ");
		float[] times = new float[rawTimes.Length];
		for(int i=0;i<times.Length;i++){
			times[i] = float.Parse(rawTimes[i]);
		}
		return times;
	}
	
	private KeyFrameData[] initKeyFrames(float[] times){
		KeyFrameData[] frames = new KeyFrameData[times.Length];
		for(int i=0;i<frames.Length;i++){
			frames[i] = new KeyFrameData(times[i]);
		}
		return frames;
	}
	
	private void loadJointTransforms(KeyFrameData[] frames, XmlNode jointData, String rootNodeId){
		String jointNameId = getJointName(jointData);
		String dataId = getDataId(jointData);
		XmlNode transformData = jointData.GetChildWithAttribute("source", "id", dataId);
		String[] rawData = transformData.GetChild("float_array").getData().Split(" ");
		processTransforms(jointNameId, rawData, frames, jointNameId.Equals(rootNodeId));
	}
	
	private String getDataId(XmlNode jointData){
		XmlNode node = jointData.GetChild("sampler").GetChildWithAttribute("input", "semantic", "OUTPUT");
		return node.GetAttribute("source").Substring(1);
	}
	
	private String getJointName(XmlNode jointData){
		XmlNode channelNode = jointData.GetChild("channel");
		String data = channelNode.GetAttribute("target");
		return data.Split("/")[0];
	}
	
	private void processTransforms(String jointName, String[] rawData, KeyFrameData[] keyFrames, bool root){
		//FloatBuffer buffer = BufferUtils.createFloatBuffer(16);
		//float[] matrixData = new float[16];
		for(int i=0;i<keyFrames.Length;i++){
			var transform = new Matrix4();
			for(int j=0;j<16;j++)
			{
				Console.WriteLine();
				//matrixData[j] = float.Parse(rawData[i*16 + j]);
				transform[j / 4, j % 4] = float.Parse(rawData[i * 16 + j]);
			}
			// buffer.clear();
			// buffer.put(matrixData);
			// buffer.flip();
			
			//transform.Load(buffer);
			transform.Transpose();
			if(root){
				//because up axis in Blender is different to up axis in game
				Matrix4.Mult(CORRECTION, transform, out transform);
			}
			keyFrames[i].AddJointTransform(new JointTransformData(jointName, transform));
		}
	}
	
	private String findRootJointName(){
		XmlNode skeleton = jointHierarchy.GetChild("visual_scene").GetChildWithAttribute("node", "id", "Armature");
		return skeleton.GetChild("node").GetAttribute("id");
	}
}

public class GeometryLoader {

	private static readonly Matrix4 CORRECTION = Settings.CORRECTION;
	
	private readonly XmlNode meshData;

	private readonly List<VertexSkinData> vertexWeights;
	
	private float[] verticesArray;
	private float[] normalsArray;
	private float[] texturesArray;
	private int[] indicesArray;
	private int[] jointIdsArray;
	private float[] weightsArray;

	List<Vertex> vertices = new List<Vertex>();
	List<Vector2> textures = new List<Vector2>();
	List<Vector3> normals = new List<Vector3>();
	List<int> indices = new List<int>();
	
	public GeometryLoader(XmlNode geometryNode, List<VertexSkinData> vertexWeights) {
		this.vertexWeights = vertexWeights;
		this.meshData = geometryNode.GetChild("geometry").GetChild("mesh");
	}
	
	public MeshData ExtractModelData(){
		readRawData();
		assembleVertices();
		removeUnusedVertices();
		initArrays();
		convertDataToArrays();
		convertIndicesListToArray();
		return new MeshData(verticesArray, texturesArray, normalsArray, indicesArray, jointIdsArray, weightsArray);
	}

	private void readRawData() {
		readPositions();
		readNormals();
		readTextureCoords();
	}

	private void readPositions() {
		String positionsId = meshData.GetChild("vertices").GetChild("input").GetAttribute("source").Substring(1);
		XmlNode positionsData = meshData.GetChildWithAttribute("source", "id", positionsId).GetChild("float_array");
		int count = int.Parse(positionsData.GetAttribute("count"));
		String[] posData = positionsData.getData().Split(" ");
		for (int i = 0; i < count/3; i++) {
			float x = float.Parse(posData[i * 3]);
			float y = float.Parse(posData[i * 3 + 1]);
			float z = float.Parse(posData[i * 3 + 2]);
			Vector4 position = new Vector4(x, y, z, 1);
			//Matrix4.transform(CORRECTION, position, position);
			position = CORRECTION * position;
			vertices.Add(new Vertex(vertices.Count, new Vector3(position.X, position.Y, position.Z), vertexWeights[vertices.Count]));
		}
	}

	private void readNormals() {
		String normalsId = meshData.GetChild("polylist").GetChildWithAttribute("input", "semantic", "NORMAL")
				.GetAttribute("source").Substring(1);
		XmlNode normalsData = meshData.GetChildWithAttribute("source", "id", normalsId).GetChild("float_array");
		int count = int.Parse(normalsData.GetAttribute("count"));
		String[] normData = normalsData.getData().Split(" ");
		for (int i = 0; i < count/3; i++) {
			float x = float.Parse(normData[i * 3]);
			float y = float.Parse(normData[i * 3 + 1]);
			float z = float.Parse(normData[i * 3 + 2]);
			Vector4 norm = new Vector4(x, y, z, 0f);
			//Matrix4f.transform(CORRECTION, norm, norm);
			norm = CORRECTION * norm;
			normals.Add(new Vector3(norm.X, norm.Y, norm.Z));
		}
	}

	private void readTextureCoords() {
		String texCoordsId = meshData.GetChild("polylist").GetChildWithAttribute("input", "semantic", "TEXCOORD")
				.GetAttribute("source").Substring(1);
		XmlNode texCoordsData = meshData.GetChildWithAttribute("source", "id", texCoordsId).GetChild("float_array");
		int count = int.Parse(texCoordsData.GetAttribute("count"));
		String[] texData = texCoordsData.getData().Split(" ");
		for (int i = 0; i < count/2; i++) {
			float s = float.Parse(texData[i * 2]);
			float t = float.Parse(texData[i * 2 + 1]);
			textures.Add(new Vector2(s, t));
		}
	}
	
	private void assembleVertices(){
		XmlNode poly = meshData.GetChild("polylist");
		int typeCount = poly.GetChildren("input").Count;
		String[] indexData = poly.GetChild("p").getData().Split(" ");
		for(int i=0;i<indexData.Length/typeCount;i++){
			int positionIndex = int.Parse(indexData[i * typeCount]);
			int normalIndex = int.Parse(indexData[i * typeCount + 1]);
			int texCoordIndex = int.Parse(indexData[i * typeCount + 2]);
			processVertex(positionIndex, normalIndex, texCoordIndex);
		}
	}
	

	private Vertex processVertex(int posIndex, int normIndex, int texIndex) {
		Vertex currentVertex = vertices[posIndex];
		if (!currentVertex.IsSet()) 
		{
			currentVertex.SetTextureIndex(texIndex);
			currentVertex.SetNormalIndex(normIndex);
			indices.Add(posIndex);
			return currentVertex;
		} 
		return dealWithAlreadyProcessedVertex(currentVertex, texIndex, normIndex);
	}

	private int[] convertIndicesListToArray() {
		this.indicesArray = new int[indices.Count];
		for (int i = 0; i < indicesArray.Length; i++)
			indicesArray[i] = indices[i];

		return indicesArray;
	}

	private float convertDataToArrays() {
		float furthestPoint = 0;
		for (int i = 0; i < vertices.Count; i++) {
			Vertex currentVertex = vertices[i];
			if (currentVertex.length > furthestPoint) {
				furthestPoint = currentVertex.length;
			}
			Vector3 position = currentVertex.position;
			Vector2 textureCoord = textures[currentVertex.textureIndex];
			Vector3 normalVector = normals[currentVertex.normalIndex];
			verticesArray[i * 3] = position.X;
			verticesArray[i * 3 + 1] = position.Y;
			verticesArray[i * 3 + 2] = position.Z;
			texturesArray[i * 2] = textureCoord.X;
			texturesArray[i * 2 + 1] = 1 - textureCoord.Y;
			normalsArray[i * 3] = normalVector.X;
			normalsArray[i * 3 + 1] = normalVector.Y;
			normalsArray[i * 3 + 2] = normalVector.Z;
			VertexSkinData weights = currentVertex.WeightsData;
			jointIdsArray[i * 3] = weights.JointIds[0];
			jointIdsArray[i * 3 + 1] = weights.JointIds[1];
			jointIdsArray[i * 3 + 2] = weights.JointIds[2];
			weightsArray[i * 3] = weights.Weights[0];
			weightsArray[i * 3 + 1] = weights.Weights[1];
			weightsArray[i * 3 + 2] = weights.Weights[2];

		}
		return furthestPoint;
	}

	private Vertex dealWithAlreadyProcessedVertex(Vertex previousVertex, int newTextureIndex, int newNormalIndex) {
		if (previousVertex.HasSameTextureAndNormal(newTextureIndex, newNormalIndex)) {
			indices.Add(previousVertex.index);
			return previousVertex;
		} else {
			Vertex anotherVertex = previousVertex.duplicateVertex;
			if (anotherVertex != null) {
				return dealWithAlreadyProcessedVertex(anotherVertex, newTextureIndex, newNormalIndex);
			} else {
				Vertex duplicateVertex = new Vertex(vertices.Count, previousVertex.position, previousVertex.WeightsData);
				duplicateVertex.SetTextureIndex(newTextureIndex);
				duplicateVertex.SetNormalIndex(newNormalIndex);
				previousVertex.SetDuplicateVertex(duplicateVertex);
				vertices.Add(duplicateVertex);
				indices.Add(duplicateVertex.index);
				return duplicateVertex;
			}

		}
	}
	
	private void initArrays(){
		this.verticesArray = new float[vertices.Count * 3];
		this.texturesArray = new float[vertices.Count * 2];
		this.normalsArray = new float[vertices.Count * 3];
		this.jointIdsArray = new int[vertices.Count * 3];
		this.weightsArray = new float[vertices.Count * 3];
	}

	private void removeUnusedVertices() {
		foreach (var vertex in vertices) {
			vertex.AverageTangents();
			if (!vertex.IsSet()) {
				vertex.SetTextureIndex(0);
				vertex.SetNormalIndex(0);
			}
		}
	}
}

public class SkeletonLoader 
{
	private static readonly Matrix4 CORRECTION = Settings.CORRECTION;

	private XmlNode armatureData;
	
	private List<String> boneOrder;
	
	private int jointCount = 0;
	
	public SkeletonLoader(XmlNode visualSceneNode, List<String> boneOrder) {
		this.armatureData = visualSceneNode.GetChild("visual_scene").GetChildWithAttribute("node", "id", "Armature");
		this.boneOrder = boneOrder;
	}
	
	public SkeletonData extractBoneData(){
		XmlNode headNode = armatureData.GetChild("node");
		JointData headJoint = loadJointData(headNode, true);
		return new SkeletonData(jointCount, headJoint);
	}
	
	private JointData loadJointData(XmlNode jointNode, bool isRoot){
		JointData joint = extractMainJointData(jointNode, isRoot);
		foreach(var childNode in jointNode.GetChildren("node"))
		{
			joint.addChild(loadJointData(childNode, false));
		}
		return joint;
	}
	
	private JointData extractMainJointData(XmlNode jointNode, bool isRoot){
		String nameId = jointNode.GetAttribute("id");
		int index = boneOrder.IndexOf(nameId);
		String[] matrixData = jointNode.GetChild("matrix").getData().Split(" ");
		Matrix4 matrix = new Matrix4();
		// matrix.load(convertData(matrixData));
		var data = this.convertData(matrixData);
		for (var i = 0; i < data.Length; i++)
			matrix[i / 4, i % 4] = data[15 - i];

		matrix.Transpose();
		if(isRoot)
		{
			//because in Blender z is up, but in our game y is up.
			Matrix4.Mult(CORRECTION, matrix, out matrix);
		}
		jointCount++;
		return new JointData(index, nameId, matrix);
	}
	
	private float[] convertData(String[] rawData)
	{
		float[] matrixData = new float[16];
		for(int i=0;i<matrixData.Length;i++)
			matrixData[i] = float.Parse(rawData[i]);

		return matrixData;
		// FloatBuffer buffer = BufferUtils.createFloatBuffer(16);
		// buffer.put(matrixData);
		// buffer.flip();
		// return buffer;
	}
}

public class SkinLoader {

	private readonly XmlNode skinningData;
	private readonly int maxWeights;

	public SkinLoader(XmlNode controllersNode, int maxWeights) {
		this.skinningData = controllersNode.GetChild("controller").GetChild("skin");
		this.maxWeights = maxWeights;
	}

	public SkinningData extractSkinData() {
		List<String> jointsList = loadJointsList();
		float[] weights = loadWeights();
		XmlNode weightsDataNode = skinningData.GetChild("vertex_weights");
		int[] effectorJointCounts = getEffectiveJointsCounts(weightsDataNode);
		List<VertexSkinData> vertexWeights = getSkinData(weightsDataNode, effectorJointCounts, weights);
		return new SkinningData(jointsList, vertexWeights);
	}

	private List<String> loadJointsList() {
		XmlNode inputNode = skinningData.GetChild("vertex_weights");
		String jointDataId = inputNode.GetChildWithAttribute("input", "semantic", "JOINT").GetAttribute("source")
				.Substring(1);
		XmlNode jointsNode = skinningData.GetChildWithAttribute("source", "id", jointDataId).GetChild("Name_array");
		var names = jointsNode.getData().Split(" ").ToList();
		return names;
	}

	private float[] loadWeights() {
		XmlNode inputNode = skinningData.GetChild("vertex_weights");
		String weightsDataId = inputNode.GetChildWithAttribute("input", "semantic", "WEIGHT").GetAttribute("source")
				.Substring(1);
		XmlNode weightsNode = skinningData.GetChildWithAttribute("source", "id", weightsDataId).GetChild("float_array");
		String[] rawData = weightsNode.getData().Split(" ");
		float[] weights = new float[rawData.Length];
		for (int i = 0; i < weights.Length; i++) {
			weights[i] = float.Parse(rawData[i]);
		}
		return weights;
	}

	private int[] getEffectiveJointsCounts(XmlNode weightsDataNode) {
		String[] rawData = weightsDataNode.GetChild("vcount").getData().Split(" ");
		int[] counts = new int[rawData.Length];
		for (int i = 0; i < rawData.Length; i++)
		{
			if (int.TryParse(rawData[i], out var value))
				counts[i] = value;
			else
				counts[i] = 0;
		}
		return counts;
	}

	private List<VertexSkinData> getSkinData(XmlNode weightsDataNode, int[] counts, float[] weights) {
		String[] rawData = weightsDataNode.GetChild("v").getData().Split(" ");
		List<VertexSkinData> skinningData = new List<VertexSkinData>();
		int pointer = 0;
		foreach (int count in counts) {
			VertexSkinData skinData = new VertexSkinData();
			for (int i = 0; i < count; i++) {
				int jointId = int.Parse(rawData[pointer++]);
				int weightId = int.Parse(rawData[pointer++]);
				skinData.addJointEffect(jointId, weights[weightId]);
			}
			skinData.LimitJointNumber(maxWeights);
			skinningData.Add(skinData);
		}
		return skinningData;
	}
}
