using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

// TODO: Taking cover when under a lot of fire
// TODO: Dist to enemy should be dependend on weapon, probably.
// TODO: Dist to enemy should be strictly monotonous, so that enemies really far away get closer, too.
// To do this, probably just don't clamp the utility bonus/penalty
namespace AI {
    [RequireComponent(typeof(Vision))]
    [RequireComponent(typeof(MovementHandler))]
    [RequireComponent(typeof(WeaponHandler))]
    public class Positioning : MonoBehaviour {
        public static class DebugOptions {
            public static bool useMovementCost = true;
            public static bool useSameTileBonus = true;
            public static bool useDistToAlly = true;
            public static bool useDistToEnemy = true;
            public static bool useGradient = true;
            public static bool useLOS = true;
            public static bool useNoise = true;
            public static bool useOccupiedCost = true;
        }

        const float sameTileDistance = 0.5f;
        const int notWalkable = -1;
        const float noiseOffsetRange = 1000f;

        public PositioningSettings settings;

        PositioningTile[,] grid;
        float maxUtility;
        float minUtility;
        int lastSizeDiff;

        Coroutine followPath;
        bool currentlyMoving;
        Vector3[] path;

        Vision vis;
        MovementHandler mov;
        WeaponHandler weap;
        Shooting shoot;

        public float noiseOffset;
        float lastCollisionRecalc;

        void Awake() {
            if (settings == null)
                throw new System.ArgumentNullException("Unit is missing PositioningSettings!");

            int size = settings.gridSize+2; // +2 for border
            if (size % 2 == 0)
                throw new System.ArgumentException("Grid size must be odd!");

            grid = new PositioningTile[size, size];

            vis = GetComponent<Vision>();
            mov = GetComponent<MovementHandler>();
            weap = GetComponent<WeaponHandler>();
            shoot = GetComponent<Shooting>();

            noiseOffset = Random.Range(0f, noiseOffsetRange);
        }

        void Start() {
            StartCoroutine(Reposition());
        }

        IEnumerator Reposition() {
            while (true) {
                if (!currentlyMoving) {
                    Vector3 targetPosition;

                    PositionData positionData = new PositionData() {
                        situationRating = vis.GetSituationRating(),
                        cooldown = weap.Cooldown,
                        enemies = vis.EnemiesInfo,
                        allies = vis.AlliesInfo
                    };

                    // Maybe don't check whole grid everytime?
                    targetPosition = CalculateOptimalPosition(positionData, settings.gridSize);

                    PathRequest pathRequest = new PathRequest(transform.position, targetPosition, OnPathCreated);
                    PathRequestManager.RequestPath(pathRequest);
                }

                yield return new WaitForSeconds(settings.positioningUpdateTime);
            }
        }

        void OnPathCreated(Vector3[] path, bool success) {
            //Debug.Log("Path created. Success: " + success);
            if (success) {
                if (this == null)
                    return;

                if (followPath != null) {
                    StopCoroutine(followPath);
                }
                currentlyMoving = true;
                this.path = path;
                followPath = StartCoroutine(FollowPath(null));
            }
        }

        public IEnumerator FollowPath(System.Action OnPathFinished) {
            int pathIndex = 0;

            while (true) {
                if (pathIndex >= path.Length) {
                    yield break;
                }

                /*
                float distance = (path[pathIndex] - transform.position).magnitude;

                if (distance < 0.5f) {
                    pathIndex++;
                }
                */
                if (MapManager.Instance.WorldToTilePos(path[pathIndex]) == MapManager.Instance.WorldToTilePos(transform.position)) {
                    pathIndex++;
                }


                if (pathIndex == path.Length) {
                    mov.SetTargetPosition(path[pathIndex - 1]);
                    currentlyMoving = false;

                    if (OnPathFinished != null)
                        OnPathFinished();
                    yield break;
                }

                Vector2 direction = (path[pathIndex] - transform.position).normalized;
                mov.SetTargetDirection(direction);

                if ((shoot != null && !shoot.TargetFound) || shoot == null)
                    mov.SetTargetRotation();

                yield return new WaitForSeconds(settings.followPathUpdateTime);
            }
        }

        public Vector3 CalculateOptimalPosition(PositionData positionData, int sidelength) {
            if (sidelength > grid.GetLength(0))
                throw new System.ArgumentException("Trying to calculate position grid, but size parameter is too big!");
            if (sidelength % 2 == 0)
                throw new System.ArgumentException("Sidelength must be odd!");

            int sizeDiff = (grid.GetLength(0) - 2 - sidelength)/2;
            lastSizeDiff = sizeDiff;
            Vector2Int unitPosition = MapManager.Instance.WorldToTilePos(transform.position);

            // Calculate grid
            Vector2Int gridOrigin = unitPosition - Vector2Int.one * (grid.GetLength(0) / 2);
            Vector2Int tilePosition;
            for (int x = sizeDiff; x < grid.GetLength(0) - sizeDiff; x++) {
                for (int y = sizeDiff; y < grid.GetLength(1) - sizeDiff; y++) {
                    tilePosition = gridOrigin + new Vector2Int(x, y);
                    grid[x, y] = new PositioningTile(tilePosition, positionData.enemies);
                }
            }

            // Calculate utility in slightly smaller grid and get the best position
            float utility;
            maxUtility = float.MinValue;
            minUtility = float.MaxValue;
            Vector2Int maxUtilityPosition = Vector2Int.zero;
            for (int x = 1 + sizeDiff; x < grid.GetLength(0) - 1 - sizeDiff; x++) {
                for (int y = 1 + sizeDiff; y < grid.GetLength(1) - 1 - sizeDiff; y++) {

                    // Tile can't be the best position, if it's not walkable
                    if (!grid[x, y].walkable)
                        continue;


                    // Calculate utility of this tile
                    utility = 0;

                    if (Positioning.DebugOptions.useMovementCost) { // Remove utility when the tile is far away
                        utility -= (unitPosition - grid[x, y].position).magnitude * settings.movementCost;
                    }

                    if (Positioning.DebugOptions.useSameTileBonus) { // Add utility when it's the tile the unit is currently on
                        if (unitPosition == grid[x, y].position)
                            utility += settings.sameTileBonus;
                    }

                    if (positionData.allies != null) { // Go through all allies
                        foreach (UnitInfo ally in positionData.allies) {
                            if (Positioning.DebugOptions.useDistToAlly) { // Remove utility, when not at optimal distance to allies
                                float dist = (ally.Position - grid[x, y].position).magnitude;
                                dist -= settings.idealDistanceToAlly;
                                dist = Mathf.Abs(dist);
                                dist /= settings.distToAllyBand;
                                dist = Mathf.Pow(dist, settings.distToAllyPower);
                                dist = Mathf.Clamp01(dist) * 2 - 1; // Range -1 to 1.
                                
                                utility -= dist * settings.distToAllyMultiplier;
                            }

                            if (Positioning.DebugOptions.useOccupiedCost) { // Remove utility, when space already occupied
                                if (MapManager.Instance.WorldToTilePos(ally.Position) == grid[x, y].position)
                                    utility -= settings.occupiedCost;
                            }
                        }
                    }

                    if (positionData.enemies != null) { // Go through all enemies
                        foreach (UnitInfo enemy in positionData.enemies) {
                            if (Positioning.DebugOptions.useDistToEnemy) { // Remove utility, when not at optimal distance to enemies
                                float dist = (enemy.Position - grid[x, y].position).magnitude;
                                dist -= settings.idealDistanceToEnemy;
                                dist = Mathf.Abs(dist);
                                dist /= settings.distToEnemyBand;
                                dist = Mathf.Pow(dist, settings.distToEnemyPower);
                                dist = Mathf.Clamp01(dist) * 2 - 1; // Range -1 to 1.

                                utility -= dist * settings.idealDistanceToEnemy * (positionData.situationRating * 0.5f + 0.5f);
                            }

                            if (Positioning.DebugOptions.useOccupiedCost) { // Remove utility, when space already occupied
                                if (MapManager.Instance.WorldToTilePos(enemy.Position) == grid[x, y].position)
                                    utility -= settings.occupiedCost;
                            }
                        }
                    }

                    if (Positioning.DebugOptions.useGradient) {
                        // Calculate magnitude of LOS gradient. It's good to be close to the gradient, if kinda aggressive. 
                        // Watch out for not walkable tiles
                        // https://en.wikipedia.org/wiki/Image_gradient
                        Vector2 gradient = new Vector2 {
                            x = (grid[x - 1, y].walkable ? -grid[x - 1, y].LOSNumber : -grid[x, y].LOSNumber)
                              + (grid[x + 1, y].walkable ? grid[x + 1, y].LOSNumber : grid[x, y].LOSNumber),
                            y = (grid[x, y - 1].walkable ? -grid[x, y - 1].LOSNumber : -grid[x, y].LOSNumber)
                              + (grid[x, y + 1].walkable ? grid[x, y + 1].LOSNumber : grid[x, y].LOSNumber)
                        };
                        utility += gradient.magnitude * Mathf.Clamp(
                            (positionData.situationRating - settings.gradientSitRatThreshold) / (1 + settings.gradientSitRatThreshold),
                            -1f, 1f) * settings.gradientMultiplier; // https://www.wolframalpha.com/input/?i=Clip((x%2B0.3)%2F(1-0.3))+with+x+%3D+-1+to+1 (Watch out for minuses!)
                    }

                    if (Positioning.DebugOptions.useLOS) {
                        // Also, choose a tile with high or low LOSNumber
                        if (grid[x, y].LOSNumber < 0.1f)
                            utility -= Mathf.Clamp(
                            (positionData.situationRating - settings.LOSSitRatThreshold) / (1 + settings.LOSSitRatThreshold),
                            -1f, 1f) * settings.LOSMultiplier; // https://www.wolframalpha.com/input/?i=Clip((x%2B0.3)%2F(1-0.3))+with+x+%3D+-1+to+1 (Watch out for minuses!)
                    }

                    // TODO: Dodging Projectiles: Substract utility based on incoming projectiles??
                    // Probably easier to do the collider-raycast-method. That way, implementing AI being under cover fire is easy, too.

                    if (Positioning.DebugOptions.useNoise) {
                        utility += Utility.Noise.PerlinNoise3D(
                            (grid[x, y].position.x + noiseOffset) * settings.noiseScaling,
                            (grid[x, y].position.y + noiseOffset) * settings.noiseScaling,
                            Time.time * settings.noiseSpeed
                            ) * settings.noiseMultiplier;
                    }

                    grid[x, y].utility = utility;

                    if (utility > maxUtility) { // Is it the best so far?
                        maxUtilityPosition = grid[x, y].position;
                        maxUtility = utility;
                    }
                    minUtility = Mathf.Min(utility, minUtility);
                }
            }

            return MapManager.Instance.TileToWorldPos(maxUtilityPosition);
        }

        void OnCollisionStay2D(Collision2D collision) {
            if (Time.time - lastCollisionRecalc > settings.collisionRecalcTime) {
                lastCollisionRecalc = Time.time;
                
                // Recalculate Path by setting currentlyMoving to false. Coroutines deal with the rest.
                currentlyMoving = false;
            }
        }

        void OnDrawGizmosSelected() {

            if (Application.isPlaying) {
                // Draw path
                if (path != null) {
                    for (int i = 0; i < path.Length; i++) {
                        float brightness = 1 - Mathf.Lerp(0f, 1f, i / (float)(path.Length - 1));
                        Gizmos.color = new Color(brightness * 2, brightness * 2, brightness * 2); // *2 because of Gizmo brightness bug
                        Gizmos.DrawCube(path[i], Vector3.one * 0.25f);
                        if (i != path.Length - 1) {
                            Gizmos.DrawLine(path[i], path[i + 1]);
                        }
                    }
                }


                // Draw positioningGrid
                if (grid != null) {
                    /*
                    GUIStyle style = new GUIStyle {
                        fontSize = 16
                    };
                    */

                    for (int x = 1 + lastSizeDiff; x < grid.GetLength(0) - 1 - lastSizeDiff; x++) {
                        for (int y = 1 + lastSizeDiff; y < grid.GetLength(1) - 1 - lastSizeDiff; y++) {

                            float val = (grid[x, y].utility - minUtility) / (maxUtility - minUtility);
                            val *= 0.4f;
                            val += 0.2f; // Between 0.2 and 0.6
                            Color col;
                            if (grid[x, y].walkable) {
                                col = new Color(0f, 0f, 1f, val);
                                Rect rectangle = new Rect(grid[x, y].position, Vector2.one);
                                Handles.DrawSolidRectangleWithOutline(rectangle, col, Color.black);
                            }




                            /*
                            string text = "";
                            if (positioningGrid[x, y].walkable) {
                                //text += thoroughGrid[x, y].LOSNumber.ToString("0.00") + "\n";
                                text += positioningGrid[x, y].utility.ToString("0");
                            } else {
                                //text += "W";
                            }
                            Handles.Label(mm.TileToWorldPos(thoroughGrid[x, y].position) + new Vector3(-0.5f, 0.5f, 1f), text, style);
                            */
                        }
                    }
                }
            }
        }

        struct PositioningTile {
            public Vector2Int position;
            public bool walkable;
            public float LOSNumber;
            public float utility;

            public PositioningTile(Vector2Int position, List<UnitInfo> enemies) {
                this.position = position;
                Node node = MapManager.Instance.NodeFromTilePos(position);
                walkable = node == null ? false : node.walkable; // Maybe this is bad. Nodes are for pathfinding and contain other information, too.

                LOSNumber = 0f;
                int terrainLayer = 1 << LayerMask.NameToLayer("Terrain"); // Cache this?
                foreach (UnitInfo enemy in enemies) {
                    Vector3 relPosition = MapManager.Instance.TileToWorldPos(position) - (Vector3)enemy.Position;
                    RaycastHit2D hit = Physics2D.Raycast(enemy.Position, relPosition, relPosition.magnitude, terrainLayer);
                    if (hit.collider == null) {
                        LOSNumber += 1f;
                    }
                }
                //LOSNumber = Mathf.Log10(LOSNumber + 1); // TODO: Could be expensive and not worth it... explore other options
                utility = 0;
            }
        }
    }
}