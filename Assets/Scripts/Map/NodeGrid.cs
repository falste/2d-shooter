public class NodeGrid : IPoolable {
	public static Pool<NodeGrid> pool = new Pool<NodeGrid>();

	public Node[,] nodes;

	public NodeGrid() {
		nodes = new Node[16, 9];

		for (int x = 0; x < nodes.GetLength(0); x++) {
			for (int y = 0; y < nodes.GetLength(1); y++) {
				nodes[x, y] = new Node();
			}
		}
	}
	public void OnReset() {}
	public void OnRelease() {}
}
